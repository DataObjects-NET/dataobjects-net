// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2014.03.01

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
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
        protected override string DetectAssemblyVersion() => "1";
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
        public override bool CanUpgradeFrom(string oldVersion) => true;

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

    [Test]
    public async Task MainAsyncTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyEntity();
        new V1.MyEntityBase();
        tx.Complete();
      }

      using (var domain = await BuildUpgradedDomainAsync())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = session.Query.All<V2.MyOtherEntityBase>().ToList();
        Assert.That(items.Count, Is.EqualTo(2));
        Assert.That(items.Count(i => i is V2.MyOtherEntity), Is.EqualTo(1));
        Assert.That(items.Count(i => !(i is V2.MyOtherEntity)), Is.EqualTo(1));
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain() =>
      BuildDomain(DomainUpgradeMode.Recreate, typeof(V1.MyEntity));

    private Domain BuildUpgradedDomain() =>
      BuildDomain(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private Task<Domain> BuildUpgradedDomainAsync() =>
      BuildDomainAsync(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

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

    private DomainConfiguration BuildDomainConfiguration(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.RegisterCaching(sampleType.Assembly, sampleType.Namespace);
      return configuration;
    }
  }
}