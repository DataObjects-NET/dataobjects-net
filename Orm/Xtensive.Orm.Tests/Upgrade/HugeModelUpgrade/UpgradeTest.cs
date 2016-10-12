using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Collections.Graphs;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;
using Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  [TestFixture]
  public class UpgradeTest
  {
    private Queue<NodeConfiguration> sequentalQueue;
    private ConcurrentQueue<NodeConfiguration> concurentQueue;

    protected void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.DefaultDatabase = "DO-Tests";
      configuration.DefaultSchema = "dbo";
      configuration.Types.Register(typeof (TestEntity0).Assembly, typeof (TestEntity0).Namespace);
      configuration.Types.Register(typeof (UpgradePerformanceCounter));
      return configuration;
    }

    [Test]
    public void SequentialBuildingTest()
    {
      using (BuildDomain(BuildConfiguration(), false)){}

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      var logFile = Path.Combine(Path.GetTempPath(), string.Format("{0}.log", MethodBase.GetCurrentMethod().Name));
      using (ConsoleToFile(logFile))
      using (var domain = BuildDomain(configuration, false)) {
        var counters = domain.Extensions.Get<PerformanceResultContainer>();
        foreach (var counter in counters.ToList()) {
          Console.WriteLine("----------------------{0}-------------------------", counter.Key.IsNullOrEmpty() ? "Default" : string.Format("-{0}-", counter.Key));

          foreach (var point in counter.Value) {
            Measure measure;
            var value = AdaptToReadableFrom(point.Value, out measure);
            Console.WriteLine("{0}:{1} {2}", point.Key, value.ToString("##.000"), measure);
          }
          Console.WriteLine("------------------------end---------------------------");
        }
      }
    }

    [Test]
    public void ParallelBuildingTest()
    {
      using (BuildDomain(BuildConfiguration(), false)){}

      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();
      var logFile = Path.Combine(Path.GetTempPath(), string.Format("{0}.log", MethodBase.GetCurrentMethod().Name));
      using (ConsoleToFile(logFile))
      using (var domain = BuildDomain(configuration, true)) {
        var counters = domain.Extensions.Get<PerformanceResultContainer>();
        foreach (var counter in counters) {
          Console.WriteLine("----------------------{0}-------------------------", counter.Key.IsNullOrEmpty() ? "Default" : string.Format("-{0}-", counter.Key));

          foreach (var point in counter.Value) {
            Measure measure;
            var value = AdaptToReadableFrom(point.Value, out measure);
            Console.WriteLine("{0}:{1} {2}", point.Key, value.ToString("##.000"), measure);
          }
          Console.WriteLine("------------------------end---------------------------");
        }
      }
    }

    private IDisposable ConsoleToFile(string filePath)
    {
      var fs = new FileStream(filePath, FileMode.OpenOrCreate);
      var previousWriter = Console.Out;
      var sw = new StreamWriter(fs);
      Console.SetOut(sw);

      var disposable = new Disposable((state) =>
      {
        Console.SetOut(previousWriter);
        sw.Close();
      });
      return disposable;
    }

    private double AdaptToReadableFrom(long bytes, out Measure measure)
    {
      measure = Measure.Kilobytes;
      var kilobytes = (double)bytes / 1024;
      if (kilobytes < 1024) {
        return kilobytes;
      }
      measure = Measure.Megabytes;
      var megabytes = kilobytes / 1024;
      if (megabytes < 1024)
        return megabytes;
      measure = Measure.Gigabytes;
      return megabytes / 1024;
    }

    protected void PopulateData()
    {
      sequentalQueue = new Queue<NodeConfiguration>(GetConfigurations(DomainUpgradeMode.Skip));
      concurentQueue = new ConcurrentQueue<NodeConfiguration>(GetConfigurations(DomainUpgradeMode.Skip));
    }

    private IEnumerable<NodeConfiguration> GetConfigurations(DomainUpgradeMode upgradeMode)
    {
      var databases = new[] {
        "DO-Tests-1",
        "DO-Tests-2",
        "DO-Tests-3",
        "DO-Tests-4",
      };
      var schemas = new[] {
        "dbo",
        "Model1",
        "Model2",
        "Model3",
        "Model4"
      };

      var index = 0;
      foreach (var database in databases) {
        index++;
        var node = new NodeConfiguration("Node" + index);
        node.UpgradeMode = upgradeMode;
        node.DatabaseMapping.Add("DO-Tests", database);
        yield return node;
      }
    }

    protected Domain BuildDomain(DomainConfiguration configuration, bool isParallel)
    {
      var domain = Domain.Build(configuration);
      var nodes = GetConfigurations(configuration.UpgradeMode);
      if (isParallel) {
        Action<object> action = nodeConfg => domain.StorageNodeManager.AddNode((NodeConfiguration) nodeConfg);
        var tasks = new List<Task>();
        foreach (var nodeConfiguration in nodes)
          tasks.Add(Task.Factory.StartNew(action, nodeConfiguration));
        Task.WaitAll(tasks.ToArray());
      }
      else {
        foreach (var nodeConfiguration in nodes)
          domain.StorageNodeManager.AddNode(nodeConfiguration);
      }

      return domain;
    }

    private void Bod(NodeConfiguration nodeConfiguration, ParallelLoopState parallelLoopState, long arg3)
    {
      throw new NotImplementedException();
    }
  }

  public class PerformanceResultContainer : Dictionary<string, Dictionary<string, long>>
  {
  }

  public enum Measure
  {
    Kilobytes,
    Megabytes,
    Gigabytes,
  }

  public class UpgradePerformanceCounter : UpgradeHandler
  {
    Dictionary<string, long> memoryUsages = new Dictionary<string, long>();

    public override bool IsEnabled
    {
      get { return true; }
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      memoryUsages.Add("OnPrepare before base", GC.GetTotalMemory(false));
      base.OnPrepare();
      memoryUsages.Add("OnPrepare after base", GC.GetTotalMemory(false));
    }

    public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
      memoryUsages.Add("OnBeforeExecuteActions before base", GC.GetTotalMemory(false));
      base.OnBeforeExecuteActions(actions);
      memoryUsages.Add("OnBeforeExecuteActions after base", GC.GetTotalMemory(false));
    }

    public override void OnBeforeStage()
    {
      memoryUsages.Add("OnBeforeStage before base", GC.GetTotalMemory(false));
      base.OnBeforeStage();
      memoryUsages.Add("OnBeforeStage after base", GC.GetTotalMemory(false));
    }

    public override void OnUpgrade()
    {
      memoryUsages.Add("OnUpgrade before base", GC.GetTotalMemory(false));
      base.OnUpgrade();
      memoryUsages.Add("OnUpgrade after base", GC.GetTotalMemory(false));
    }

    public override void OnStage()
    {
      memoryUsages.Add("On Premare before base", GC.GetTotalMemory(false));
      base.OnStage();
      memoryUsages.Add("On Premare after base", GC.GetTotalMemory(false));
    }

    public override void OnSchemaReady()
    {
      memoryUsages.Add("OnSchemaReady before base", GC.GetTotalMemory(false));
      base.OnSchemaReady();
      memoryUsages.Add("OnSchemaReady after base", GC.GetTotalMemory(false));
    }

    public override void OnComplete(Domain domain)
    {
      memoryUsages.Add("OnComplete", GC.GetTotalMemory(false));
      memoryUsages.Add("OnComplete with garbage collection", GC.GetTotalMemory(true));

      var container = domain.Extensions.Get<PerformanceResultContainer>();
      if (container==null) {
        container = new PerformanceResultContainer();
        domain.Extensions.Set(typeof (PerformanceResultContainer), container);
      }
      container.Add(UpgradeContext.StorageNode.Id, memoryUsages);
    }
  }
}
