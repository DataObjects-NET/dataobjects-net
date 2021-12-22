// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.08.20

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using Model1 = Xtensive.Orm.Tests.Upgrade.NonNullableReferenceRemovalTestModel.Model1;
using Model2 = Xtensive.Orm.Tests.Upgrade.NonNullableReferenceRemovalTestModel.Model2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace NonNullableReferenceRemovalTestModel
  {
    namespace Model1
    {
      [HierarchyRoot]
      public class Target : Entity
      {
        [Key, Field]
        public int Id { get; private set; }
      }

      [HierarchyRoot, Index("T", Unique = true)]
      public class Owner : Entity
      {
        [Key, Field]
        public int Id { get; private set; }

        [Field(Nullable = false)]
        public Target T { get; set; }

        public Owner(Target target)
        {
          T = target;
        }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion() => "1";
      }
    }

    namespace Model2
    {
      [HierarchyRoot]
      public class Owner : Entity
      {
        [Key, Field]
        public int Id { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion() => "2";

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          _ = hints.Add(new RemoveFieldHint(typeof(Model1.Owner).FullName, "T"));
          _ = hints.Add(new RemoveTypeHint(typeof(Model1.Target).FullName));
        }

        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }
      }
    }
  }

  [TestFixture]
  public class NonNullableReferenceRemovalTest
  {
    private Domain BuildDomain(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.BuildAsync(configuration);
    }

    [Test]
    public void UpgradeTest()
    {
      using (var domain1 = BuildDomain(typeof(Model1.Owner), DomainUpgradeMode.Recreate))
      using (var session = domain1.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Model1.Owner(new Model1.Target());
        new Model1.Owner(new Model1.Target());
        tx.Complete();
      }

      using (var domain2 = BuildDomain(typeof(Model2.Owner), DomainUpgradeMode.PerformSafely))
      using (var session = domain2.OpenSession())
      using (var tx = session.OpenTransaction()) {
      }
    }

    [Test]
    public async Task UpgradeAsyncTest()
    {
      using (var domain1 = BuildDomain(typeof(Model1.Owner), DomainUpgradeMode.Recreate))
      using (var session = domain1.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new Model1.Owner(new Model1.Target());
        new Model1.Owner(new Model1.Target());
        tx.Complete();
      }

      using (var domain2 = await BuildDomainAsync(typeof(Model2.Owner), DomainUpgradeMode.PerformSafely))
      using (var session = domain2.OpenSession())
      using (var tx = session.OpenTransaction()) {
      }
    }
  }
}