// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.11.15

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.ChangeColumnCaseTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.ChangeColumnCaseTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace ChangeColumnCaseTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string NAme { get; set; }
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

        [Field]
        public string Name { get; set; }
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

        protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint(typeof (V1.MyEntity).FullName, typeof (MyEntity)));
        }
      }
    }
  }

  [TestFixture]
  public class ChangeColumnCaseTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyEntity {NAme = "V1"};
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var newEntity = new V2.MyEntity {Name = "V2"};
        session.SaveChanges(); // saving new object
        var existingObject = session.Query.All<V2.MyEntity>().Single(e => e.Name=="V1");
        existingObject.Name += "Changed";
        session.SaveChanges(); // saving modified object
        existingObject.Remove();
        session.SaveChanges(); // removing object
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