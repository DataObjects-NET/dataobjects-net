// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.09.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.SplitHierarchyTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.SplitHierarchyTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace SplitHierarchyTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public abstract class MyBaseClass : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string StringValue { get; set; }
      }

      public class MyFirstClass: MyBaseClass
      {
        [Field]
        public int IntValue { get; set; }
      }

      public class MySecondClass: MyBaseClass
      {
        [Field]
        public double DoubleValue { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion() => "1";
      }
    }

    namespace Version2
    {
      public abstract class MyBaseClass : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string StringValue { get; set; }
      }

      [HierarchyRoot]
      public class MyFirstClass : MyBaseClass
      {
        [Field]
        public int IntValue { get; set; }
      }

      [HierarchyRoot]
      public class MySecondClass : MyBaseClass
      {
        [Field]
        public double DoubleValue { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override string DetectAssemblyVersion() => "2";

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          _ = hints.Add(new RemoveTypeHint(typeof(V1.MyBaseClass).FullName));
          _ = hints.Add(new RenameTypeHint(typeof(V1.MyFirstClass).FullName, typeof(MyFirstClass)));
          _ = hints.Add(new RenameTypeHint(typeof(V1.MySecondClass).FullName, typeof(MySecondClass)));
          _ = hints.Add(new CopyFieldHint(typeof(V1.MyBaseClass), "StringValue", typeof(MyFirstClass)));
          _ = hints.Add(new CopyFieldHint(typeof(V1.MyBaseClass), "StringValue", typeof(MySecondClass)));
        }
      }
    }
  }

  [TestFixture]
  public class SplitHierarchyTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyFirstClass {IntValue = 1, StringValue = "1"};
        new V1.MySecondClass {DoubleValue = 2.0, StringValue = "2"};
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = session.Query.All<V2.MyFirstClass>().Single();
        Assert.That(first.StringValue, Is.EqualTo("1"));
        Assert.That(first.IntValue, Is.EqualTo(1));

        var second = session.Query.All<V2.MySecondClass>().Single();
        Assert.That(second.StringValue, Is.EqualTo("2"));
        Assert.That(second.DoubleValue, Is.EqualTo(2.0));
      }
    }

    [Test]
    public async Task MainAsyncTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.MyFirstClass { IntValue = 1, StringValue = "1" };
        new V1.MySecondClass { DoubleValue = 2.0, StringValue = "2" };
        tx.Complete();
      }

      using (var domain = await BuildUpgradedDomainAsync())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var first = session.Query.All<V2.MyFirstClass>().Single();
        Assert.That(first.StringValue, Is.EqualTo("1"));
        Assert.That(first.IntValue, Is.EqualTo(1));

        var second = session.Query.All<V2.MySecondClass>().Single();
        Assert.That(second.StringValue, Is.EqualTo("2"));
        Assert.That(second.DoubleValue, Is.EqualTo(2.0));
      }
    }

    private Domain BuildInitialDomain() => BuildDomain(DomainUpgradeMode.Recreate, typeof(V1.MyBaseClass));

    private Domain BuildUpgradedDomain() => BuildDomain(DomainUpgradeMode.Perform, typeof(V2.MyBaseClass));

    private Task<Domain> BuildUpgradedDomainAsync() => BuildDomainAsync(DomainUpgradeMode.Perform, typeof(V2.MyBaseClass));

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
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return configuration;
    }
  }
}