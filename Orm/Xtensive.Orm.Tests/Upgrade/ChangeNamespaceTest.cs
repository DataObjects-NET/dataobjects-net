// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.ChangeNamespaceTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.ChangeNamespaceTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace ChangeNamespaceTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : UpgradeHandler
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
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override string DetectAssemblyVersion()
        {
          return "2";
        }
      }
    }
  }

  [TestFixture]
  public class ChangeNamespaceTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyEntity();
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(session.Query.All<V2.MyEntity>().Count(), Is.EqualTo(1));
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain()
    {
      return BuildDomain(DomainUpgradeMode.Recreate, typeof (V1.MyEntity));
    }

    private Domain BuildUpgradedDomain()
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, typeof (V2.MyEntity));
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }
  }
}