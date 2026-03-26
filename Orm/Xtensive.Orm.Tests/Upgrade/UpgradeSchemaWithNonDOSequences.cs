// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture]
  public class UpgradeSchemaWithNonDOSequences
  {
    public class DummyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [OneTimeSetUp]
    public void Setup()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.AllFeaturesSupported(Providers.ProviderFeatures.Sequences);
    }

    [Test]
    [TestCase(typeof(byte), DomainUpgradeMode.Recreate, TestName = "TinyIntRecreateTest")]
    [TestCase(typeof(byte), DomainUpgradeMode.Perform, TestName = "TinyIntPerformTest")]
    [TestCase(typeof(byte), DomainUpgradeMode.PerformSafely, TestName = "TinyIntPerformSafelyTest")]
    [TestCase(typeof(byte), DomainUpgradeMode.Validate, TestName = "TinyIntValidateTest")]
    [TestCase(typeof(byte), DomainUpgradeMode.Skip, TestName = "TinyIntSkipTest")]
    [TestCase(typeof(byte), DomainUpgradeMode.LegacyValidate, TestName = "TinyIntLegacyValidateTest")]
    [TestCase(typeof(byte), DomainUpgradeMode.LegacySkip, TestName = "TinyIntLegacySkipTest")]

    [TestCase(typeof(short), DomainUpgradeMode.Recreate, TestName = "SmallIntRecreateTest")]
    [TestCase(typeof(short), DomainUpgradeMode.Perform, TestName = "SmallIntPerformTest")]
    [TestCase(typeof(short), DomainUpgradeMode.PerformSafely, TestName = "SmallIntPerformSafelyTest")]
    [TestCase(typeof(short), DomainUpgradeMode.Validate, TestName = "SmallIntValidateTest")]
    [TestCase(typeof(short), DomainUpgradeMode.Skip, TestName = "SmallIntSkipTest")]
    [TestCase(typeof(short), DomainUpgradeMode.LegacyValidate, TestName = "SmallIntLegacyValidateTest")]
    [TestCase(typeof(short), DomainUpgradeMode.LegacySkip, TestName = "SmallIntLegacySkipTest")]

    [TestCase(typeof(int), DomainUpgradeMode.Recreate, TestName = "IntRecreateTest")]
    [TestCase(typeof(int), DomainUpgradeMode.Perform, TestName = "IntPerformTest")]
    [TestCase(typeof(int), DomainUpgradeMode.PerformSafely, TestName = "IntPerformSafelyTest")]
    [TestCase(typeof(int), DomainUpgradeMode.Validate, TestName = "IntValidateTest")]
    [TestCase(typeof(int), DomainUpgradeMode.Skip, TestName = "IntSkipTest")]
    [TestCase(typeof(int), DomainUpgradeMode.LegacyValidate, TestName = "IntLegacyValidateTest")]
    [TestCase(typeof(int), DomainUpgradeMode.LegacySkip, TestName = "IntLegacySkipTest")]

    [TestCase(typeof(long), DomainUpgradeMode.Recreate, TestName = "BigIntRecreateTest")]
    [TestCase(typeof(long), DomainUpgradeMode.Perform, TestName = "BigIntPerformTest")]
    [TestCase(typeof(long), DomainUpgradeMode.PerformSafely, TestName = "BigIntPerformSafelyTest")]
    [TestCase(typeof(long), DomainUpgradeMode.Validate, TestName = "BigIntValidateTest")]
    [TestCase(typeof(long), DomainUpgradeMode.Skip, TestName = "BigIntSkipTest")]
    [TestCase(typeof(long), DomainUpgradeMode.LegacyValidate, TestName = "BigIntLegacyValidateTest")]
    [TestCase(typeof(long), DomainUpgradeMode.LegacySkip, TestName = "BigIntLegacySkipTest")]
    public void MainTest(Type seqBaseType, DomainUpgradeMode upgradeMode)
    {
      var initConfig = GetDomainConfig(DomainUpgradeMode.Recreate);
      using (var initialDomain = Domain.Build(initConfig))
      using (var session = initialDomain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var service = session.Services.Get<DirectSqlAccessor>();
        var command = service.CreateCommand();
        command.CommandText = GetSequenceCreatorQuery(seqBaseType);
        using (command) {
          _ = command.ExecuteNonQuery();
        }
        tx.Complete();
      }

      var upgradeConfig = GetDomainConfig(upgradeMode);
      using ( var initialDomain = Domain.Build(upgradeConfig))
      using (var session = initialDomain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var service = session.Services.Get<DirectSqlAccessor>();
        var command = service.CreateCommand();
        command.CommandText = GetSequenceValidatorQuery(seqBaseType, upgradeMode);
        using (command) {
          Assert.IsTrue((bool)command.ExecuteScalar());
        }
      }
    }

    private DomainConfiguration GetDomainConfig(DomainUpgradeMode upgradeMode)
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (DummyEntity));
      config.UpgradeMode = upgradeMode;
      return config;
    }

    private string GetSequenceCreatorQuery(Type baseType)
    {
      var sequenceName = GetSequenceName(baseType);
      if (baseType == typeof(byte)) {
        return $"CREATE SEQUENCE dbo.{sequenceName} AS tinyint START WITH 12 INCREMENT BY 3 MINVALUE 10 MAXVALUE 200 CYCLE CACHE 3;";
      }
      else if(baseType == typeof(short)) {
        return $"CREATE SEQUENCE dbo.{sequenceName} AS smallint START WITH 12 INCREMENT BY 3 MINVALUE 10 MAXVALUE 200 CYCLE CACHE 3;";
      }
      else if (baseType == typeof(int)) {
        return $"CREATE SEQUENCE dbo.{sequenceName} AS int START WITH 12 INCREMENT BY 3 MINVALUE 10 MAXVALUE 200 CYCLE CACHE 3;";
      }
      else if(baseType == typeof(long)) {
        return $"CREATE SEQUENCE dbo.{sequenceName} AS bigint START WITH 12 INCREMENT BY 3 MINVALUE 10 MAXVALUE 200 CYCLE CACHE 3;";
      }
      throw new ArgumentOutOfRangeException();
    }

    private string GetSequenceValidatorQuery(Type baseType, DomainUpgradeMode upgradeMode)
    {
      var sequenceName = GetSequenceName(baseType);
      if (upgradeMode == DomainUpgradeMode.Recreate) {
        // no sequence
        return $"SELECT CASE WHEN Count([name]) = 0 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      else if (upgradeMode == DomainUpgradeMode.Perform) {
        // no sequence
        return $"SELECT CASE WHEN Count([name]) = 0 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      else if (upgradeMode == DomainUpgradeMode.PerformSafely) {
        // no sequence
        return $"SELECT CASE WHEN Count([name]) = 0 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      else if (upgradeMode == DomainUpgradeMode.Skip) {
        // sequence
        return $"SELECT CASE WHEN Count([name]) = 1 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      else if (upgradeMode == DomainUpgradeMode.Validate) {
        // sequence
        return $"SELECT CASE WHEN Count([name]) = 1 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      else if (upgradeMode == DomainUpgradeMode.LegacySkip) {
        // sequence
        return $"SELECT CASE WHEN Count([name]) = 1 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      else if (upgradeMode == DomainUpgradeMode.LegacyValidate) {
        // sequence
        return $"SELECT CASE WHEN Count([name]) = 1 THEN Cast(1 as bit) ELSE CAST(0 as bit) END AS r FROM [DO-Tests].[sys].[sequences] WHERE[name] = N'{sequenceName}'";
      }
      throw new ArgumentOutOfRangeException();
    }

    private string GetSequenceName(Type baseType)
    {
      if (baseType == typeof(byte)) {
        return "TinyIntSeq";
      }
      else if (baseType == typeof(short)) {
        return "SmallIntSeq";
      }
      else if (baseType == typeof(int)) {
        return "IntSeq";
      }
      else if (baseType==typeof(long)) {
        return "BigIntSeq";
      }
      throw new ArgumentOutOfRangeException();
    }
  }
}
