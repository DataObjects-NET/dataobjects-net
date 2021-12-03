// Copyright (C) 2018-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.09.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly Dictionary<MethodBase, ProviderInfo> stagesAccess = new Dictionary<MethodBase, ProviderInfo>();

    public ProviderInfo this[MethodInfo method] =>
      stagesAccess.TryGetValue(method, out var result) ? result : null;

    public void Add(MethodBase method, ProviderInfo givenProvider) => stagesAccess.Add(method, givenProvider);
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

    public override ValueTask OnPrepareAsync(CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return default;
    }

    public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnBeforeExecuteActions(actions);
    }

    public override ValueTask OnBeforeExecuteActionsAsync(UpgradeActionSequence actions, CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return default;
    }

    public override void OnBeforeStage()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnBeforeStage();
    }

    public override ValueTask OnBeforeStageAsync(CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return default;
    }

    public override void OnSchemaReady()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnSchemaReady();
    }

    public override ValueTask OnSchemaReadyAsync(CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return default;
    }

    public override void OnStage()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnStage();
    }

    public override ValueTask OnStageAsync(CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.OnStageAsync(token);
    }

    public override void OnUpgrade()
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.OnUpgrade();
    }

    protected override ValueTask OnUpgradeAsync(CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      return base.OnUpgradeAsync(token);
    }

    public override void OnComplete(Domain domain)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      domain.Extensions.Set(gatherer);
    }

    public override ValueTask OnCompleteAsync(Domain domain, CancellationToken token = default)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      domain.Extensions.Set(gatherer);
      return default;
    }

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      TryGather(MethodInfo.GetCurrentMethod());
      base.AddUpgradeHints(hints);
    }

    protected override void AddAutoHints(ISet<UpgradeHint> hints)
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
      // we don't need final stage in case of multistage upgrade. If we have access on first stage we will have it on the second one as well
      return UpgradeContext.UpgradeMode.IsMultistage() && UpgradeContext.Stage==UpgradeStage.Final;
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade
{
  public sealed class UpgradeContextTest
  {
    private const string AdditionalNodeName = "additional";

    private MethodInfo[] upgraderMethods;

    [OneTimeSetUp]
    public void TestFixture() => upgraderMethods = UpgraderMethods();

    [Test]
    public void AccessToProviderInfoOnDomainBuildingTest()
    {
      Domain domain;

      using (domain = BuildDomain(DomainUpgradeMode.Recreate)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }

      using (domain = BuildDomain(DomainUpgradeMode.Perform)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }

      using (domain = BuildDomain(DomainUpgradeMode.PerformSafely)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }

      using (domain = BuildDomain(DomainUpgradeMode.Validate)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }

      using (domain = BuildDomain(DomainUpgradeMode.Skip)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }

      using (domain = BuildDomain(DomainUpgradeMode.LegacyValidate)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }

      using (domain = BuildDomain(DomainUpgradeMode.LegacySkip)) {
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());
      }
    }

    [Test]
    public async Task AccessToProviderInfoOnDomainBuildingAsyncTest()
    {
      Domain domain;

      using (domain = await BuildDomainAsync(DomainUpgradeMode.Recreate))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = await BuildDomainAsync(DomainUpgradeMode.Perform))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = await BuildDomainAsync(DomainUpgradeMode.PerformSafely))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = await BuildDomainAsync(DomainUpgradeMode.Validate))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = await BuildDomainAsync(DomainUpgradeMode.Skip))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = await BuildDomainAsync(DomainUpgradeMode.LegacyValidate))
        ValidateAccess(domain.Extensions.Get<AccessGatherer>());

      using (domain = await BuildDomainAsync(DomainUpgradeMode.LegacySkip))
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
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.Perform);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.PerformSafely);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.Validate);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.Skip);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.LegacySkip);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        AddStorageNode(domain, DomainUpgradeMode.LegacyValidate);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);
      }
    }

    [Test]
    public async Task AccessToProviderInfoOnStorageNodeBuildingAsyncTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      using (var domain = await BuildMultischemaDomainAsync(DomainUpgradeMode.Recreate)) {
        await AddStorageNodeAsync(domain, DomainUpgradeMode.Recreate);
        var gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        await AddStorageNodeAsync(domain, DomainUpgradeMode.Perform);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        await AddStorageNodeAsync(domain, DomainUpgradeMode.PerformSafely);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        await AddStorageNodeAsync(domain, DomainUpgradeMode.Validate);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        await AddStorageNodeAsync(domain, DomainUpgradeMode.Skip);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        await AddStorageNodeAsync(domain, DomainUpgradeMode.LegacySkip);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);

        await AddStorageNodeAsync(domain, DomainUpgradeMode.LegacyValidate);
        gatherer = domain.Extensions.Get<AccessGatherer>();
        ValidateAccess(gatherer);
        _ = domain.StorageNodeManager.RemoveNode(AdditionalNodeName);
      }
    }

    private void ValidateAccess(AccessGatherer accessGatherer)
    {
      var expected = StorageProviderInfo.Instance.Info;

      foreach (var upgraderMethod in upgraderMethods) {
        var tested = accessGatherer[upgraderMethod];
        if (tested == null) {
          continue;
        }

        Assert.That(tested.ConstantPrimaryIndexName, Is.EqualTo(expected.ConstantPrimaryIndexName));
        Assert.That(tested.DefaultDatabase, Is.EqualTo(expected.DefaultDatabase));
        Assert.That(tested.DefaultSchema, Is.EqualTo(expected.DefaultSchema));
        Assert.That(tested.MaxIdentifierLength, Is.EqualTo(expected.MaxIdentifierLength));
        Assert.That(tested.ProviderFeatures, Is.EqualTo(expected.ProviderFeatures));
        Assert.That(tested.ProviderName, Is.EqualTo(expected.ProviderName));
        Assert.That(tested.StorageVersion, Is.EqualTo(expected.StorageVersion));

        foreach (var supportedType in tested.SupportedTypes) {
          Assert.That(expected.SupportedTypes.Contains(supportedType), Is.True);
        }
      }
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = BuildConfiguration(upgradeMode);
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode)
    {
      var configuration = BuildConfiguration(upgradeMode);
      return Domain.BuildAsync(configuration);
    }

    private Domain BuildMultischemaDomain(DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgrader));
      configuration.DefaultSchema = WellKnownSchemas.SqlServerDefaultSchema;

      return Domain.Build(configuration);
    }

    private Task<Domain> BuildMultischemaDomainAsync(DomainUpgradeMode upgradeMode)
    {
      var configuration = BuildConfiguration(upgradeMode);
      configuration.DefaultSchema = "dbo";

      return Domain.BuildAsync(configuration);
    }

    private void AddStorageNode(Domain domain, DomainUpgradeMode upgradeMode)
    {
      var nodeConfiguration = new NodeConfiguration(AdditionalNodeName);
      nodeConfiguration.SchemaMapping.Add("dbo", "Model1");
      nodeConfiguration.UpgradeMode = upgradeMode;

      _ = domain.StorageNodeManager.AddNode(nodeConfiguration);
    }

    private async Task AddStorageNodeAsync(Domain domain, DomainUpgradeMode upgradeMode)
    {
      var nodeConfiguration = new NodeConfiguration(AdditionalNodeName);
      nodeConfiguration.SchemaMapping.Add("dbo", "Model1");
      nodeConfiguration.UpgradeMode = upgradeMode;

      _ = await domain.StorageNodeManager.AddNodeAsync(nodeConfiguration);
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(CustomUpgrader));
      return configuration;
    }

    private MethodInfo[] UpgraderMethods()
    {
      var upgrader = typeof(CustomUpgrader);

      var publicMethods =
        upgrader.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
      var protectedMethods =
        upgrader.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

      return publicMethods.Union(protectedMethods).ToArray();
    }
  }
}
