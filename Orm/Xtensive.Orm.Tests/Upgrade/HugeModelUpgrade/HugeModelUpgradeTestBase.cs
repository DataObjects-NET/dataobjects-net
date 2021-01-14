// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Upgrade.HugeModelUpgrade
{
  [TestFixture]
  public abstract class HugeModelUpgradeTestBase
  {
    protected virtual bool SupportsParallel => true;

    [OneTimeSetUp]
    public void OneTimeSetUp() => Require.ProviderIs(StorageProvider.SqlServer);

    [SetUp]
    public void SetUp()
    {
      var isParallelTest = TestContext.CurrentContext.Test.MethodName == nameof(ParallelTest);
      if (isParallelTest && !SupportsParallel) {
        throw new IgnoreException("Parallel test not supported.");
      }

      using var domain = BuildDomain(BuildConfiguration(), false);
      PopulateData(domain);
    }

    [Test]
    [Explicit]
    public void SequentialTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();

      using var domain = BuildDomain(configuration, false);
      var counters = domain.Extensions.Get<PerformanceResultContainer>();
      Console.WriteLine(counters.ToString());
      CheckIfQueriesWork(domain);
    }

    [Test]
    [Explicit]
    public void ParallelTest()
    {
      var configuration = BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Skip;
      GC.Collect();

      using var domain = BuildDomain(configuration, true);
      var counters = domain.Extensions.Get<PerformanceResultContainer>();
      Console.WriteLine(counters.ToString());
      CheckIfQueriesWork(domain);
    }

    protected abstract void PopulateData(Domain domain);
    protected abstract void CheckIfQueriesWork(Domain domain);
    protected abstract IEnumerable<NodeConfiguration> GetAdditionalNodeConfigurations(DomainUpgradeMode upgradeMode);

    protected virtual Domain BuildDomain(DomainConfiguration configuration, bool isParallel = false)
    {
      var domain = Domain.Build(configuration);
      var nodes = GetAdditionalNodeConfigurations(configuration.UpgradeMode);
      if (isParallel) {
        void BuildNode(object domainAndNodeToBuild)
        {
          var pair = ((Domain domain, NodeConfiguration nodeConfig)) domainAndNodeToBuild;
          _ = pair.domain.StorageNodeManager.AddNode(pair.nodeConfig);
        }

        var tasks = new List<Task>();
        foreach (var nodeConfiguration in nodes) {
          tasks.Add(Task.Factory.StartNew(BuildNode, (domain, nodeConfiguration)));
        }
        Task.WaitAll(tasks.ToArray());
      }
      else {
        foreach (var nodeConfiguration in nodes) {
          _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
        }
      }

      return domain;
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(UpgradePerformanceCounter));
      return configuration;
    }
  }
}
