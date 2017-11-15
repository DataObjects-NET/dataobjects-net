// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.09.28

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.NewUpgradeEventsAndPropertiesTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.NewUpgradeEventsAndPropertiesTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace NewUpgradeEventsAndPropertiesTestModel
  {
    public enum ExpectedEvent
    {
      OnPrepare,
      OnBeforeStage,
      OnStage,
      OnBeforeExecuteActions,
      OnSchemaReady,
      OnComplete,
      None,
    }

    public abstract class CheckingUpgradeHandler : UpgradeHandler
    {
      private ExpectedEvent expectedEvent = ExpectedEvent.OnPrepare;

      public override void OnPrepare()
      {
        CheckExpectedEvent(ExpectedEvent.OnPrepare, ExpectedEvent.OnBeforeStage);
        CheckNonStageEvent();
      }

      public override void OnBeforeStage()
      {
        base.OnBeforeStage();

        CheckExpectedEvent(ExpectedEvent.OnBeforeStage, ExpectedEvent.OnSchemaReady);
        CheckNonStageEvent();
      }

      public override void OnSchemaReady()
      {
        CheckExpectedEvent(ExpectedEvent.OnSchemaReady, ExpectedEvent.OnBeforeExecuteActions);
        CheckNonStageEvent();
      }

      public override void OnBeforeExecuteActions(UpgradeActionSequence actions)
      {
        Assert.That(actions, Is.Not.Null);
        CheckExpectedEvent(ExpectedEvent.OnBeforeExecuteActions, ExpectedEvent.OnStage);
        CheckNonStageEvent();
      }

      public override void OnStage()
      {
        base.OnStage();

        var nextEvent = UpgradeContext.Stage==UpgradeStage.Upgrading
          ? ExpectedEvent.OnBeforeStage
          : ExpectedEvent.OnComplete;

        CheckExpectedEvent(ExpectedEvent.OnStage, nextEvent);
        CheckStageEvent();
      }

      public override void OnComplete(Domain domain)
      {
        Assert.That(domain, Is.Not.Null);
        CheckExpectedEvent(ExpectedEvent.OnComplete, ExpectedEvent.None);
        CheckNonStageEvent();
      }

      private void CheckNonStageEvent()
      {
        Assert.That(UpgradeContext.Session, Is.Null);
        CheckConnection();
      }

      private void CheckStageEvent()
      {
        Assert.That(UpgradeContext.Session, Is.Not.Null);
      }

      private void CheckConnection()
      {
        Assert.That(UpgradeContext.Connection, Is.Not.Null);
        Assert.That(UpgradeContext.Connection.State, Is.EqualTo(ConnectionState.Open));
        Assert.That(UpgradeContext.Transaction, Is.Not.Null);
      }

      private void CheckExpectedEvent(ExpectedEvent actual, ExpectedEvent next)
      {
        Assert.That(actual, Is.EqualTo(expectedEvent));
        expectedEvent = next;
      }
    }

    namespace Version1
    {
      [HierarchyRoot]
      public abstract class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : CheckingUpgradeHandler
      {
        protected override string DetectAssemblyVersion()
        {
          return "1";
        }
      }
    }

    namespace Version2
    {
      [HierarchyRoot]
      public abstract class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : CheckingUpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override string DetectAssemblyVersion()
        {
          return "2";
        }

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint(typeof (V1.MyEntity).FullName, typeof (MyEntity)));
        }
      }
    }
  }

  [TestFixture]
  public class NewUpgradeEventsAndPropertiesTest
  {
    [Test]
    public void ParallelTest()
    {
      RunTest(true);
    }

    [Test]
    public void SequentalTest()
    {
      RunTest(false);
    }

    private void RunTest(bool buildInParallel)
    {
      BuildInitialDomain(buildInParallel).Dispose();
      BuildUpgradedDomain(buildInParallel).Dispose();
    }

    private Domain BuildInitialDomain(bool buildInParallel)
    {
      return BuildDomain(DomainUpgradeMode.Recreate, typeof (V1.MyEntity), buildInParallel);
    }

    private Domain BuildUpgradedDomain(bool inParallel)
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, typeof (V2.MyEntity), inParallel);
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType, bool buildInParallel)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      configuration.BuildInParallel = buildInParallel;
      return Domain.Build(configuration);
    }
  }
}