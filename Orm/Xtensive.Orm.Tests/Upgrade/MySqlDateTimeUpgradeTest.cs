// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Upgrade.MySqlDateTimeUpgradeTestModel;

namespace Xtensive.Orm.Tests.Upgrade.MySqlDateTimeUpgradeTestModel
{
  [HierarchyRoot]
  public class TestDateTimeEntity : Entity
  {
    public const string DynamicFieldName = "DateTimeValueNew";

    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime DateTimeValue { get; set; }

    public TestDateTimeEntity(Session session, DateTime value)
      : base(session)
    {
      DateTimeValue = value;
    }
  }

  public class ModelModifier : IModule2
  {
    public void OnAutoGenericsBuilt(BuildingContext context, ICollection<Type> autoGenerics) { }
    public void OnBuilt(Domain domain) { }
    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      _ = model.Types[typeof(TestDateTimeEntity)].DefineField(
        TestDateTimeEntity.DynamicFieldName, typeof(DateTime?));
    }
  }
}


namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  internal class MySqlDateTimeUpgradeTest
  {
    private DateTime[] testValues;

    [OneTimeSetUp]
    public void TestFixutreSetup()
    {
      Require.ProviderIs(StorageProvider.MySql);
      Require.ProviderVersionAtLeast(StorageProviderVersion.MySql56);

      var bValue = DateTime.UtcNow;
      bValue = new DateTime(bValue.Year, bValue.Month, bValue.Day, bValue.Hour, bValue.Minute, bValue.Second, 222, DateTimeKind.Utc);
      testValues = new[] {
        bValue,
        bValue.AddDays(-1),
        bValue.AddDays(1),
        bValue.AddHours(-1),
        bValue.AddHours(1),
      };
    }

    [Test]
    public void RecreateTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.Recreate);
    }

    [Test]
    public void SkipTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.Skip);
    }

    [Test]
    public void ValidateTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.Validate);
    }

    [Test]
    public void LegacySkipTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.LegacySkip);
    }

    [Test]
    public void LegacyValidateTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.LegacyValidate);
    }


    [Test]
    public void PerformTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.Perform);
    }

    [Test]
    public void PerformSafelyTest()
    {
      BuildAndPopulateInitDomain();

      UpgradeAndCheckData(DomainUpgradeMode.PerformSafely);
    }


    private void BuildAndPopulateInitDomain()
    {
      var initDomainConfig = BuildConfiguration(DomainUpgradeMode.Recreate);
      initDomainConfig.ForcedServerVersion = "5.5";// in this version datetime type is used, the type we try to upgrade from

      using (var initDomain = Domain.Build(initDomainConfig))
      using (var session = initDomain.OpenSession())
      using (var tx = session.OpenTransaction()) {

        foreach (var dt in testValues) {
          _ = new TestDateTimeEntity(session, dt);
        }

        tx.Complete();
      }
    }

    private void UpgradeAndCheckData(DomainUpgradeMode upgradeMode)
    {
      var upgradingDomainConfig = BuildConfiguration(upgradeMode);
      if (upgradeMode == DomainUpgradeMode.Perform || upgradeMode == DomainUpgradeMode.PerformSafely) {
        upgradingDomainConfig.Types.Register(typeof(ModelModifier));
      }

      Domain upgradedDomain = null;
      Assert.DoesNotThrow(() => upgradedDomain = Domain.Build(upgradingDomainConfig));

      using (upgradedDomain) {
        using (var session = upgradedDomain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          if (upgradeMode == DomainUpgradeMode.Recreate) {
            Assert.That(session.Query.All<TestDateTimeEntity>().Any(), Is.False);
          }
          else {
            var results = session.Query.All<TestDateTimeEntity>().OrderBy(e => e.Id).ToArray();

            Assert.That(results.Length, Is.EqualTo(testValues.Length));
            for (var i = 0; i < testValues.Length; i++) {
              var result = results[i].DateTimeValue;
              var expected = testValues[i].AdjustDateTime(0, true);
              Assert.That(result, Is.EqualTo(expected));
            }
          }
        }

        if (upgradeMode == DomainUpgradeMode.Perform || upgradeMode == DomainUpgradeMode.PerformSafely) {
          var bValue = DateTime.UtcNow;
          bValue = new DateTime(bValue.Year, bValue.Month, bValue.Day, bValue.Hour, bValue.Minute, bValue.Second, 111, DateTimeKind.Utc);
          int id;
          using (var session = upgradedDomain.OpenSession())
          using (var tx = session.OpenTransaction()) {
            var newRow = new TestDateTimeEntity(session, bValue);
            newRow[TestDateTimeEntity.DynamicFieldName] = new DateTime(bValue.Year, bValue.Month, bValue.Day, bValue.Hour, bValue.Minute, bValue.Second, 333, DateTimeKind.Utc);
            id = newRow.Id;
            tx.Complete();
          }

          using (var session = upgradedDomain.OpenSession())
          using (var tx = session.OpenTransaction()) {
            var newRow = session.Query.All<TestDateTimeEntity>().First(e => e.Id == id);
            Assert.That(newRow.DateTimeValue.Millisecond, Is.EqualTo(0)); // already created columns remain datetime
            Assert.That(((DateTime) newRow[TestDateTimeEntity.DynamicFieldName]).Millisecond, Is.EqualTo(333)); // but for new columns datetime(6) is used
            tx.Complete();
          }
        }
      }
    }

    private DomainConfiguration BuildConfiguration(DomainUpgradeMode upgradeMode)
    {
      var domainConfig = DomainConfigurationFactory.Create();
      domainConfig.Types.Register(typeof(TestDateTimeEntity));
      domainConfig.UpgradeMode = upgradeMode;

      return domainConfig;
    }
  }
}
