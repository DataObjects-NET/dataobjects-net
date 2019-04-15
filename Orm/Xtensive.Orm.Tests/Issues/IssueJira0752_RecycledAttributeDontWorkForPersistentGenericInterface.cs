// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.12.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Tests.Issues.IssueJira0752_RecycledAttributeDontWorkForPersistentGenericInterfaceModels;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0752_RecycledAttributeDontWorkForPersistentGenericInterface
  {
    [Test]
    public void RecreateTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(DomainUpgradeMode.Recreate))) {
        TestResutlts(domain, typeof (RecycledWithInterfaceEntity), true);
        TestResutlts(domain, typeof (NonRecycledWithInterfaceEntity), false);
        TestResutlts(domain, typeof (RecycledEntity), true);
      }
    }

    [Test]
    public void PerformSafelyTest()
    {
      BuildDomain(DomainConfigurationFactory.Create());

      using (var domain = BuildDomain(BuildConfiguration(DomainUpgradeMode.PerformSafely))) {
        TestResutlts(domain, typeof (RecycledWithInterfaceEntity), true);
        TestResutlts(domain, typeof (NonRecycledWithInterfaceEntity), false);
        TestResutlts(domain, typeof (RecycledEntity), true);
      }
    }

    [Test]
    public void PerformTest()
    {
      BuildDomain(DomainConfigurationFactory.Create()).Dispose();

      using (var domain = BuildDomain(BuildConfiguration(DomainUpgradeMode.Perform))) {
        TestResutlts(domain, typeof (RecycledWithInterfaceEntity), true);
        TestResutlts(domain, typeof (NonRecycledWithInterfaceEntity), false);
        TestResutlts(domain, typeof (RecycledEntity), true);
      }
    }

    [Test]
    public void ValidateTest()
    {
      var initialConfiguration = DomainConfigurationFactory.Create();
      initialConfiguration.Types.Register(typeof (RecycledEntity).Assembly, typeof (RecycledEntity).Namespace);
      BuildDomain(initialConfiguration).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(BuildConfiguration(DomainUpgradeMode.Validate)));
      using (domain) {
        Assert.That(domain.Model.Types.Contains(typeof (RecycledWithInterfaceEntity)), Is.False);
        Assert.That(domain.Model.Types.Contains(typeof (RecycledEntity)), Is.False);
        Assert.That(domain.Model.Types.Contains(typeof (NonRecycledWithInterfaceEntity)), Is.True);
      }
    }

    private void TestResutlts(Domain domain, Type type, bool isRecycled)
    {
      TypeInfo typeInfo;
      string tableName = type.Name;
      if (domain.Model.Types.TryGetValue(type, out typeInfo))
        tableName = typeInfo.MappingName;

      var actionSequences = domain.Extensions.Get<ActionGatherer>();
      bool hasCreations;
      bool hasRemovals;
      if (actionSequences.UpgradingActions!=null) {
        hasCreations = actionSequences.UpgradingActions.Flatten().OfType<CreateNodeAction>().Any(x => x.Path.StartsWith("Tables/" + tableName));
        hasRemovals = actionSequences.UpgradingActions.Flatten().OfType<RemoveNodeAction>().Any(x => x.Path.StartsWith("Tables/" + tableName));
        Assert.That(hasCreations, Is.True);
        Assert.That(hasRemovals, Is.False);
      }

      hasCreations = actionSequences.FinalActions.Flatten().OfType<CreateNodeAction>().Any(x => x.Path.StartsWith("Tables/" + tableName));
      hasRemovals = actionSequences.FinalActions.Flatten().OfType<RemoveNodeAction>().Any(x => x.Path.StartsWith("Tables/" + tableName));

      if (domain.Configuration.UpgradeMode.IsMultistage()) {
        Assert.That(hasCreations, Is.False);
        Assert.That(hasRemovals, Is.EqualTo(isRecycled));
      }
      else {
        Assert.That(hasCreations, Is.EqualTo(!isRecycled));
        Assert.That(hasRemovals, Is.False);
      }
    }

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode)
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (RecycledEntity).Assembly, typeof (RecycledEntity).Namespace);
      config.Types.Register(typeof (UpgradeCatcherHandler));
      config.UpgradeMode = upgradeMode;
      return config;
    }

    private class UpgradeCatcherHandler : UpgradeHandler
    {
      private ActionSequence upgradingActions;
      private ActionSequence finalActions;

      public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
      {
        if (UpgradeContext.Stage==UpgradeStage.Upgrading)
          upgradingActions = UpgradeContext.SchemaUpgradeActions;
        else
          finalActions = UpgradeContext.SchemaUpgradeActions;

      }

      public override void OnComplete(Domain domain)
      {
        domain.Extensions.Set(new ActionGatherer(upgradingActions, finalActions));
        base.OnComplete(domain);
      }
    }

    private class ActionGatherer
    {
      public ActionSequence UpgradingActions { get; private set; }
      public ActionSequence FinalActions { get; private set; }

      public ActionGatherer(ActionSequence upgrading, ActionSequence final)
      {
        UpgradingActions = upgrading;
        FinalActions = final;
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0752_RecycledAttributeDontWorkForPersistentGenericInterfaceModels
{
  [Serializable]
  [HierarchyRoot]
  [Recycled]
  public class RecycledEntity : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  interface ITestInterface<T> : IEntity
  {
  }

  [Serializable]
  [HierarchyRoot]
  [Recycled]
  public class RecycledWithInterfaceEntity :
    Entity,
    ITestInterface<RecycledWithInterfaceEntity>
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class NonRecycledWithInterfaceEntity :
    Entity,
    ITestInterface<RecycledWithInterfaceEntity>
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }
  }
}