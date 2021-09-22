// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alena Mikshina
// Created:    2014.02.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.UpgradeDateTimeOffsetTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.UpgradeDateTimeOffsetTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace UpgradeDateTimeOffsetTestModel
  {
    namespace Version1
    {
      [HierarchyRoot]
      public class EntityWithDateTimeOffset : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public DateTimeOffset DateTimeOffset { get; set; }

        [Field]
        public DateTime DateTime { get; set; }
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
      public class EntityWithDateTimeOffsetUpgrade : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public DateTime DateTimeOffset { get; set; }

        [Field]
        public DateTimeOffset DateTime { get; set; }
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
          _ = hints.Add(new RenameTypeHint(typeof(V1.EntityWithDateTimeOffset).FullName, typeof(EntityWithDateTimeOffsetUpgrade)));
          _ = hints.Add(new ChangeFieldTypeHint(typeof(EntityWithDateTimeOffsetUpgrade), "DateTimeOffset"));
          _ = hints.Add(new ChangeFieldTypeHint(typeof(EntityWithDateTimeOffsetUpgrade), "DateTime"));
        }
      }
    }
  }

  [TestFixture]
  public class UpgradeDateTimeOffsetTest
  {
    private readonly DateTimeOffset dateTimeOffset = new DateTimeOffset(2014, 11, 28, 16, 53, 0, 0, new TimeSpan(4, 10, 0));
    private readonly DateTime dateTime = new DateTime(2014, 11, 28, 16, 53, 0, 0);

    [Test]
    public void UpgradeDateTimeOffsetAndDateTimeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset);
      Require.ProviderIsNot(StorageProvider.PostgreSql, "ToLocalTime is not supported");
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.EntityWithDateTimeOffset {
          DateTimeOffset = dateTimeOffset,
          DateTime = dateTime
        };

        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var valuesAfterUpgrade = session.Query.All<V2.EntityWithDateTimeOffsetUpgrade>().Single();

        var queryServerOffset =
          from t in session.Query.All<V2.EntityWithDateTimeOffsetUpgrade>()
          group t by new {
            ServerOffset = t.DateTime.ToLocalTime().Offset
          };
        var resultQueryServerOffset = queryServerOffset.ToList().FirstOrDefault();

        if (resultQueryServerOffset!=null)
          Assert.That(valuesAfterUpgrade.DateTime, Is.EqualTo(new DateTimeOffset(dateTime, resultQueryServerOffset.Key.ServerOffset)));
        Assert.That(valuesAfterUpgrade.DateTimeOffset, Is.EqualTo(dateTime));

        tx.Complete();
      }
    }

    [Test]
    public async Task UpgradeDateTimeOffsetAndDateTimeAsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.DateTimeOffset);
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.EntityWithDateTimeOffset {
          DateTimeOffset = dateTimeOffset,
          DateTime = dateTime
        };

        tx.Complete();
      }

      using (var domain = await BuildUpgradedDomainAsync())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var valuesAfterUpgrade = session.Query.All<V2.EntityWithDateTimeOffsetUpgrade>().Single();

        var queryServerOffset =
          from t in session.Query.All<V2.EntityWithDateTimeOffsetUpgrade>()
          group t by new {
            ServerOffset = t.DateTime.ToLocalTime().Offset
          };
        var resultQueryServerOffset = queryServerOffset.ToList().FirstOrDefault();

        if (resultQueryServerOffset != null)
          Assert.That(valuesAfterUpgrade.DateTime, Is.EqualTo(new DateTimeOffset(dateTime, resultQueryServerOffset.Key.ServerOffset)));
        Assert.That(valuesAfterUpgrade.DateTimeOffset, Is.EqualTo(dateTime));

        tx.Complete();
      }
    }

    private Domain BuildInitialDomain() =>
      BuildDomain(DomainUpgradeMode.Recreate, typeof(V1.EntityWithDateTimeOffset));

    private Domain BuildUpgradedDomain() =>
      BuildDomain(DomainUpgradeMode.PerformSafely, typeof(V2.EntityWithDateTimeOffsetUpgrade));

    private Task<Domain> BuildUpgradedDomainAsync() =>
      BuildDomainAsync(DomainUpgradeMode.PerformSafely, typeof(V2.EntityWithDateTimeOffsetUpgrade));

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType) =>
      Domain.Build(BuildConfiguration(upgradeMode, sampleType));

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode, Type sampleType) =>
      Domain.BuildAsync(BuildConfiguration(upgradeMode, sampleType));

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return configuration;
    }
  }
}
