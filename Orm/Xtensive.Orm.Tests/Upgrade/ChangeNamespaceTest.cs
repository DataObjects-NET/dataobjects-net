// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.ChangeNamespaceTestModel;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.ChangeNamespaceTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.ChangeNamespaceTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace ChangeNamespaceTestModel
  {
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public MyEntity()
      {
        Name = "MyEntity";
      }
    }

    namespace Version1
    {
      public class MyChildEntity : MyEntity
      {
        public MyChildEntity()
        {
          Name = "MyChildEntity";
        }
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
      public class MyChildEntity : MyEntity
      {
        public MyChildEntity()
        {
          Name = "MyChildEntity";
        }
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
    public void DefaultTest()
    {
      RunTest(NamingRules.Default);
    }

    [Test]
    public void UnderscoreDotsTest()
    {
      RunTest(NamingRules.UnderscoreDots);
    }

    private void RunTest(NamingRules namingRules)
    {
      int myEntityTypeId;
      int myChildEntityTypeId;

      using (var domain = BuildInitialDomain(namingRules))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var types = domain.Model.Types;
        myEntityTypeId = types[typeof (MyEntity)].TypeId;
        myChildEntityTypeId = types[typeof (V1.MyChildEntity)].TypeId;

        new MyEntity();
        new V1.MyChildEntity();

        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain(namingRules))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var types = domain.Model.Types;
        Assert.That(types[typeof (MyEntity)].TypeId, Is.EqualTo(myEntityTypeId));
        Assert.That(types[typeof (V2.MyChildEntity)].TypeId, Is.EqualTo(myChildEntityTypeId));

        var allItems = session.Query.All<MyEntity>().ToList();
        Assert.That(allItems.Count, Is.EqualTo(2));
        Assert.That(allItems.Count(item => item is V2.MyChildEntity), Is.EqualTo(1));
        Assert.That(allItems.Count(item => !(item is V2.MyChildEntity)), Is.EqualTo(1));

        var myChildEntity = session.Query.All<V2.MyChildEntity>().Single();
        Assert.That(myChildEntity.Name, Is.EqualTo("MyChildEntity"));

        var myEntity = session.Query.All<MyEntity>().Single(e => e!=myChildEntity);
        Assert.That(myEntity.Name, Is.EqualTo("MyEntity"));

        tx.Complete();
      }
    }

    private Domain BuildInitialDomain(NamingRules namingRules)
    {
      return BuildDomain(DomainUpgradeMode.Recreate, namingRules, typeof (V1.MyChildEntity));
    }

    private Domain BuildUpgradedDomain(NamingRules namingRules)
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, namingRules, typeof (V2.MyChildEntity));
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, NamingRules namingRules, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.NamingConvention.NamingRules = namingRules;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }
  }
}