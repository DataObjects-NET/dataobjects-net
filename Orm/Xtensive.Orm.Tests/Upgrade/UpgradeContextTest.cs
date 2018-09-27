using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Hosting;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.UpgradeContextTestModel;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.UpgradeContextTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  public class AccessGatherer
  {
    Dictionary<MethodBase, ProviderInfo> stagesAccess = new Dictionary<MethodBase, ProviderInfo>();

    public ProviderInfo this[MethodInfo method]
    {
      get
      {
        ProviderInfo result;
        if (stagesAccess.TryGetValue(method, out result))
          return result;
        return null;
      }
    }

    public void Add(MethodBase method, ProviderInfo givenProvider)
    {
      stagesAccess.Add(method, givenProvider);
    }
  }

  public class CustomUpgrader : UpgradeHandler
  {
    private readonly AccessGatherer gatherer = new AccessGatherer();

    public override bool CanUpgradeFrom(string oldVersion)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.CanUpgradeFrom(oldVersion);
    }

    public override void OnPrepare()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnPrepare();
    }

    public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnBeforeExecuteActions(actions);
    }

    public override void OnBeforeStage()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnBeforeStage();
    }

    public override void OnSchemaReady()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnSchemaReady();
    }

    public override void OnStage()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnStage();
    }

    public override void OnUpgrade()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnUpgrade();
    }

    public override void OnComplete(Domain domain)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      domain.Extensions.Set(gatherer);
    }

    protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.AddUpgradeHints(hints);
    }

    protected override void AddAutoHints(Collections.ISet<UpgradeHint> hints)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.AddAutoHints(hints);
    }

    protected override void AddRecycledDefinitions(ICollection<RecycledDefinition> recycledDefinitions)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.AddRecycledDefinitions(recycledDefinitions);
    }

    protected override string DetectAssemblyName()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.DetectAssemblyName();
    }

    protected override string DetectAssemblyVersion()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.DetectAssemblyVersion();
    }

    protected override string TryStripRecycledSuffix(string nameSpace)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.TryStripRecycledSuffix(nameSpace);
    }

    protected override string GetOriginalName(Type recycledType)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.GetOriginalName(recycledType);
    }

    private void TryGather(MethodBase method)
    {
      if (!SkipGathering())
        gatherer.Add(method, UpgradeContext.ProviderInfo);
    }

    private bool SkipGathering()
    {
      // we don't need final stage in case of multistage upgrade. If we have access on first stage we will have it on the second as well
      return UpgradeContext.UpgradeMode.IsMultistage() && UpgradeContext.Stage==UpgradeStage.Final;
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public sealed class UpgradeContextTest
  {
    private static string additionalNodeName = "additional";
    private MethodInfo[] upgraderMethods;

    [TestFixtureSetUp]
    public void TestFixture()
    {
      upgraderMethods = UpgraderMethods();
    }

    [Test]
    public void AccessToProviderInfoOnDomainBuildingTest()
    {
      Domain domain;

      using (domain = BuildDomain(DomainUpgradeMode.Recreate))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = BuildDomain(DomainUpgradeMode.Perform))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = BuildDomain(DomainUpgradeMode.PerformSafely))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = BuildDomain(DomainUpgradeMode.Validate))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = BuildDomain(DomainUpgradeMode.Skip))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = BuildDomain(DomainUpgradeMode.LegacyValidate))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = BuildDomain(DomainUpgradeMode.LegacySkip))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
    }

    [Test]
    public void AccessToProviderInfoOnStorageNodeBuildingTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var domain = BuildMultischemaDomain(DomainUpgradeMode.Recreate)) {
        AddStorageNode(domain, DomainUpgradeMode.Recreate);
        var gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.Perform);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.PerformSafely);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.Validate);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.Skip);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.LegacySkip);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.LegacyValidate);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        domain.StorageNodeManager.RemoveNode(additionalNodeName);
      }
    }

    private void ValidateAccess(AccessGatherer accessGatherer)
    {
      var expected = StorageProviderInfo.Instance.Info;

      foreach (var upgraderMethod in upgraderMethods) {
        var tested = accessGatherer[upgraderMethod];
        if (tested==null)
          continue;

        Assert.That(tested.ConstantPrimaryIndexName, Is.EqualTo(expected.ConstantPrimaryIndexName));
        Assert.That(tested.DefaultDatabase, Is.EqualTo(expected.DefaultDatabase));
        Assert.That(tested.DefaultSchema, Is.EqualTo(expected.DefaultSchema));
        Assert.That(tested.MaxIdentifierLength, Is.EqualTo(expected.MaxIdentifierLength));
        Assert.That(tested.ProviderFeatures, Is.EqualTo(expected.ProviderFeatures));
        Assert.That(tested.ProviderName, Is.EqualTo(expected.ProviderName));
        Assert.That(tested.StorageVersion, Is.EqualTo(expected.StorageVersion));

        foreach (var supportedType in tested.SupportedTypes)
          Assert.That(expected.SupportedTypes.Contains(supportedType), Is.True);
      }
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof (TestEntity));
      configuration.Types.Register(typeof (CustomUpgrader));

      return Domain.Build(configuration);
    }

    private Domain BuildMultischemaDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgrader));
      configuration.DefaultSchema = "dbo";

      return Domain.Build(configuration);
    }

    private void AddStorageNode(Domain domain, DomainUpgradeMode upgradeMode)
    {
      var nodeConfiguration = new NodeConfiguration(additionalNodeName);
      nodeConfiguration.SchemaMapping.Add("dbo", "Model1");
      nodeConfiguration.UpgradeMode = upgradeMode;

      domain.StorageNodeManager.AddNode(nodeConfiguration);
    }

    private MethodInfo[] UpgraderMethods()
    {
      var upgrader = typeof (CustomUpgrader);

      var publicMethods = 
        upgrader.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
      var protectedMethods =
        upgrader.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

      return publicMethods.Union(protectedMethods).ToArray();
    }

  }
}
