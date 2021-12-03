// Copyright (C) <YEAR> Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: <AUTHOR>
// Created:    <YYYY.MM.DD>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.UpgradeTemplateTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.UpgradeTemplateTestModel.Version2;

// TODO: Search & Replace string 'UpgradeTemplate' with your test name

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace UpgradeTemplateTestModel
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
        protected override string DetectAssemblyVersion() => "1";
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
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override string DetectAssemblyVersion() => "2";

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          _ = hints.Add(new RenameTypeHint(typeof(V1.MyEntity).FullName, typeof(MyEntity)));
        }
      }
    }
  }

  [TestFixture]
  public class UpgradeTemplateTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        tx.Complete();
      }
    }

    [Test]
    public async Task MainAsyncTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        tx.Complete();
      }

      using (var domain = await BuildUpgradedDomainAsync())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain() =>
      BuildDomain(DomainUpgradeMode.Recreate, typeof(V1.MyEntity));

    private Domain BuildUpgradedDomain() =>
      BuildDomain(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private async Task<Domain> BuildUpgradedDomainAsync() =>
      await BuildDomainAsync(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType) =>
      Domain.Build(BuildConfiguration(upgradeMode, sampleType));

    private async Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode, Type sampleType) =>
      await Domain.BuildAsync(BuildConfiguration(upgradeMode, sampleType));

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return configuration;
    }
  }
}