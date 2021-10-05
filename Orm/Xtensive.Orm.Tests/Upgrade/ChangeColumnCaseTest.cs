// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.11.15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
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

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
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
        var newEntity = new V2.MyEntity { Name = "V2" };
        session.SaveChanges(); // saving new object
        var existingObject = session.Query.All<V2.MyEntity>().Single(e => e.Name=="V1");
        existingObject.Name += "Changed";
        session.SaveChanges(); // saving modified object
        existingObject.Remove();
        session.SaveChanges(); // removing object
        tx.Complete();
      }
    }

    [Test]
    public async Task MainAsyncTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyEntity { NAme = "V1" };
        tx.Complete();
      }

      using (var domain = await BuildUpgradedDomainAsync())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var newEntity = new V2.MyEntity { Name = "V2" };
        session.SaveChanges(); // saving new object
        var existingObject = session.Query.All<V2.MyEntity>().Single(e => e.Name == "V1");
        existingObject.Name += "Changed";
        session.SaveChanges(); // saving modified object
        existingObject.Remove();
        session.SaveChanges(); // removing object
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain() => BuildDomain(DomainUpgradeMode.Recreate, typeof(V1.MyEntity));

    private Domain BuildUpgradedDomain() => BuildDomain(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private Task<Domain> BuildUpgradedDomainAsync() => BuildDomainAsync(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = BuildDomainConfiguration(upgradeMode, sampleType);
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = BuildDomainConfiguration(upgradeMode, sampleType);
      return Domain.BuildAsync(configuration);
    }

    private static DomainConfiguration BuildDomainConfiguration(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return configuration;
    }
  }
}