// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.16

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.Multimapping;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.IssueJira0449_RenameTableInNonDefaultDatabaseTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.IssueJira0449_RenameTableInNonDefaultDatabaseTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace IssueJira0449_RenameTableInNonDefaultDatabaseTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public class MyEntity1 : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Value { get; set; }
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
      public class MyEntity2 : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Value2 { get; set; }
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

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint(typeof (V1.MyEntity1).FullName, typeof (MyEntity2)));
          hints.Add(new RenameFieldHint(typeof (MyEntity2), "Value", "Value2"));
        }
      }
    }
  }

  [TestFixture]
  public class IssueJira0449_RenameTableInNonDefaultDatabaseTest : MultidatabaseTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyEntity1 {Value = "Hello"};
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var e = session.Query.All<V2.MyEntity2>().Single();
        Assert.That(e.Value2, Is.EqualTo("Hello"));
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain()
    {
      return BuildDomain(DomainUpgradeMode.Recreate, typeof (V1.MyEntity1));
    }

    private Domain BuildUpgradedDomain()
    {
      return BuildDomain(DomainUpgradeMode.PerformSafely, typeof (V2.MyEntity2));
    }

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      configuration.MappingRules.Map(sampleType.Assembly, sampleType.Namespace).ToDatabase(Database2Name);
      return Domain.Build(configuration);
    }
  }
}