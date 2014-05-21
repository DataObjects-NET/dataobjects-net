// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.01

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.TypeIdPreserveTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.TypeIdPreserveTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace TypeIdPreserveTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public class MyEntityBase : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class MyEntity : MyEntityBase
      {
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
      public class MyOtherEntityBase : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class MyOtherEntity : MyOtherEntityBase
      {
      }

      [HierarchyRoot]
      [Recycled("Xtensive.Orm.Tests.Upgrade.TypeIdPreserveTestModel.Version1.MyEntityBase")]
      public class MyEntityBase : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      [Recycled("Xtensive.Orm.Tests.Upgrade.TypeIdPreserveTestModel.Version1.MyEntity")]
      public class MyEntity : MyEntityBase
      {
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        public override void OnUpgrade()
        {
          base.OnUpgrade();
          new MyOtherEntity();
          new MyOtherEntityBase();
        }

        protected override string DetectAssemblyVersion()
        {
          return "2";
        }
      }
    }
  }

  [TestFixture]
  public class TypeIdPreserveTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyEntity();
        new V1.MyEntityBase();
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = session.Query.All<V2.MyOtherEntityBase>().ToList();
        Assert.That(items.Count, Is.EqualTo(2));
        Assert.That(items.Count(i => i is V2.MyOtherEntity), Is.EqualTo(1));
        Assert.That(items.Count(i => !(i is V2.MyOtherEntity)), Is.EqualTo(1));
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