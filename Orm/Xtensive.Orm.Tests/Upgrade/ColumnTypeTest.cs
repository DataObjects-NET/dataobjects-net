// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.06.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Orm;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Upgrade;
using Mode = Xtensive.Orm.DomainUpgradeMode;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class ColumnTypeTest
  {
    private Domain domain;
    private bool canConvertBoolToString;
    private bool ignoreColumnPrecision;

    [SetUp]
    public void SetUp()
    {
      BuildDomain(Mode.Recreate);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var x = new X {
          FInt = 1,
          FInt2 = 12345,
          FLong = (long) int.MaxValue + 12345,
          FLong2 = 12345,
          FBool = true,
          FString1 = "a",
          FString5 = "12345",
          FNotNullableString = "str",
          FNullableDecimal = 123,
          FGuid = new Guid("E484EE28-3801-445B-9DF0-FBCBE5AA4883"),
          FDecimal = new decimal(1.2),
        };
        t.Complete();
      }
    }

    [Test]
    public void ValidateModeTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        ChangeFieldTypeTest("FInt", typeof (string), "1", Mode.Validate, null, null, null));
    }

    [Test]
    public void Int32ToStringTest() =>
      ChangeFieldTypeTest("FInt", typeof(string), "1", Mode.Perform, null, null, null);

    [Test]
    public void Int32ToStringSafelyTest() =>
      ChangeFieldTypeTest("FInt", typeof(string), "1", Mode.PerformSafely, null, null, null);

    [Test]
    public void Int32ToShortStringTest()
    {
      var expectedValue = ignoreColumnPrecision ? "12345" : null;
      ChangeFieldTypeTest("FInt2", typeof (string), expectedValue, Mode.Perform, 3, null, null);
    }

    [Test]
    public void Int32ToShortStringSafelyTest()
    {
      if (ignoreColumnPrecision) {
        ChangeFieldTypeTest("FInt2", typeof (string), "12345", Mode.PerformSafely, 3, null, null);
      }
      else {
        AssertEx.Throws<SchemaSynchronizationException>(() => 
          ChangeFieldTypeTest("FInt2", typeof (string), null, Mode.PerformSafely, 3, null, null));
      }
    }

    [Test]
    public void StringToInt32Test() =>
      ChangeFieldTypeTest("FString1", typeof(int), 0, Mode.Perform, null, null, null);

    [Test]
    public void StringToInt32SafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        ChangeFieldTypeTest("FString1", typeof (int), 0, Mode.PerformSafely, null, null, null));
    }

    [Test]
    public void StringToInt32WithHintTest()
    {
      using (TestUpgrader.Enable(new ChangeFieldTypeHint(typeof (X), "FString5"))) {
        ChangeFieldTypeTest("FString5", typeof (int), 12345, Mode.PerformSafely, null, null, null);
      }
    }

    [Test]
    public void StringToShortStringTest()
    {
      var expectedValue = ignoreColumnPrecision ? "12345" : "123";
      ChangeFieldTypeTest("FString5", typeof (string), expectedValue, Mode.Perform, 3, null, null);
    }

    [Test]
    public void StringToShortStringSafelyTest()
    {
      if (ignoreColumnPrecision) {
        ChangeFieldTypeTest("FString5", typeof (string), "12345", Mode.PerformSafely, 3, null, null);
      }
      else {
        AssertEx.Throws<SchemaSynchronizationException>(() =>
          ChangeFieldTypeTest("FString5", typeof (string), string.Empty, Mode.PerformSafely, 3, null, null));
      }
    }

    [Test]
    public void StringToLongStringTest() =>
      ChangeFieldTypeTest("FString1", typeof(string), "a", Mode.Perform, 3, null, null);

    [Test]
    public void StringToLongStringSafelyTest() =>
      ChangeFieldTypeTest("FString1", typeof(string), "a", Mode.PerformSafely, 3, null, null);

    [Test]
    public void BoolToStringTest()
    {
      var expectedValue = canConvertBoolToString ? "1" : null;
      ChangeFieldTypeTest("FBool", typeof (string), expectedValue, Mode.Perform, 100, null, null);
    }

    [Test]
    public void BoolToStringSafelyTest()
    {
      if (canConvertBoolToString) {
        ChangeFieldTypeTest("FBool", typeof (string), "1", Mode.PerformSafely, 100, null, null);
      }
      else {
        AssertEx.Throws<SchemaSynchronizationException>(() =>
          ChangeFieldTypeTest("FBool", typeof (string), string.Empty, Mode.PerformSafely, 100, null, null));
      }
    }

    [Test]
    public void Int32ToInt64Test() =>
      ChangeFieldTypeTest("FInt2", typeof(long), 12345L, Mode.Perform, null, null, null);

    [Test]
    public void Int32ToInt64SafelyTest() =>
      ChangeFieldTypeTest("FInt2", typeof(long), 12345L, Mode.PerformSafely, null, null, null);

    [Test]
    public void Int64ToInt32Test() =>
      ChangeFieldTypeTest("FLong", typeof(int), 0, Mode.Perform, null, null, null);

    [Test]
    public void Int64ToInt32SafelyTest()
    {
      _ = Assert.Throws<SchemaSynchronizationException>(() => 
       ChangeFieldTypeTest("FLong", typeof (int), 12345, Mode.PerformSafely, null, null, null));
    }

    [Test]
    public void DecimalToLongDecimalTest() =>
      ChangeFieldTypeTest("FDecimal", typeof(decimal), new decimal(1.2), Mode.Perform, null, 3, 2);

    [Test]
    public void DecimalToLongDecimalSafelyTest() =>
      ChangeFieldTypeTest("FDecimal", typeof(decimal), new decimal(1.2), Mode.PerformSafely, null, 3, 2);

    [Test]
    public void DecimalToShortDecimalTest()
    {
      var expectedValue = ignoreColumnPrecision ? 1.2m : 1m;
      ChangeFieldTypeTest("FDecimal", typeof (decimal), expectedValue, Mode.Perform, null, 2, 0);
    }

    [Test]
    public void DecimalToShortDecimalSafelyTest()
    {
      if (ignoreColumnPrecision) {
        ChangeFieldTypeTest("FDecimal", typeof (decimal), new decimal(1.2), Mode.PerformSafely, null, 2, 0);
      }
      else {
        AssertEx.Throws<SchemaSynchronizationException>(() =>
          ChangeFieldTypeTest("FDecimal", typeof (decimal), new decimal(1.2), Mode.PerformSafely, null, 2, 0));
      }
    }

    [Test]
    public void DecimalToNullableDecimalSafelyTest() =>
      ChangeFieldTypeTest("FDecimal", typeof(decimal?), new decimal(1.2), Mode.PerformSafely, null, 2, 1);

    [Test]
    public void DecimalToNullableDecimalTest() =>
      ChangeFieldTypeTest("FDecimal", typeof(decimal?), new decimal(1.2), Mode.Perform, null, 2, 1);

    [Test]
    public void NullableDecimalToDecimalTest() =>
      ChangeFieldTypeTest("FNullableDecimal", typeof(decimal), new decimal(123), Mode.Perform, null, null, null);

    [Test]
    public void NullableDecimalToDecimalSafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        ChangeFieldTypeTest("FNullableDecimal", typeof (decimal), new decimal(123), Mode.PerformSafely, null, null, null));
    }

    [Test]
    public void StringToNullableStringSafelyTest() =>
      ChangeFieldTypeTest("FNotNullableString", typeof(string), "str", Mode.PerformSafely, null, null, null);

    [Test]
    public void StringToNullableStringTest() =>
      ChangeFieldTypeTest("FNotNullableString", typeof(string), "str", Mode.Perform, null, null, null);

    [Test]
    public void NullableStringToStringTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.TreatEmptyStringAsNull);
      ChangeFieldTypeTest("FString1", typeof (string), "a", Mode.Perform, 1, null, null, false);
    }

    [Test]
    public void NullableStringToStringSafelyTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.TreatEmptyStringAsNull);
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        ChangeFieldTypeTest("FString1", typeof (string), "a", Mode.PerformSafely, 1, null, null, false));
    }


    [Test]
    public void AddNonNullableColumnTest()
    {
      AddFieldTest(typeof(Guid), Guid.Empty);
      AddFieldTest(typeof(bool));
      AddFieldTest(typeof(int));
      AddFieldTest(typeof(long));
      AddFieldTest(typeof(float));
      AddFieldTest(typeof(TimeSpan));
      AddFieldTest(typeof(DateTime));
    }

    #region Helper methods

    private void BuildDomain(DomainUpgradeMode upgradeMode)
    {
      if (domain != null) {
        domain.DisposeSafely();
      }

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof(X));
      configuration.Types.Register(typeof(TestUpgrader));
      configuration.Types.Register(typeof(FieldTypeChanger));
      ConfigureStorageTraits();
      domain = Domain.Build(configuration);
    }

    private void ConfigureStorageTraits()
    {
      var providerInfo = StorageProviderInfo.Instance;

      canConvertBoolToString = providerInfo
        .CheckProviderIs(StorageProvider.Firebird | StorageProvider.Sqlite | StorageProvider.Oracle);

      ignoreColumnPrecision = providerInfo
        .CheckProviderIs(StorageProvider.Sqlite);
    }

    private void ChangeFieldTypeTest(
      string fieldName, Type newColumnType, object expectedValue,
      DomainUpgradeMode mode, int? newLength, int? newPresicion, int? newScale) =>
      ChangeFieldTypeTest(fieldName, newColumnType, expectedValue, mode, newLength, newPresicion, newScale, null);

    private void ChangeFieldTypeTest(
      string fieldName, Type newColumnType, object expectedValue,
      DomainUpgradeMode mode, int? newLength, int? newPrecision, int? newScale, bool? isNullable)
    {
      using (FieldTypeChanger.Enable(newColumnType, fieldName, newLength, newPrecision, newScale, isNullable)) {
        BuildDomain(mode);
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var x = session.Query.All<X>().First();
        Assert.AreEqual(expectedValue, x[fieldName]);
      }
    }

    private void AddFieldTest(Type newColumnType, object defaultValue = null)
    {
      SetUp();
      if (domain != null) {
        domain.DisposeSafely();
      }

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.Types.Register(typeof(X));
      configuration.Types.Register(typeof(TestUpgrader));
      configuration.Types.Register(typeof(FieldTypeChanger));
      configuration.Types.Register(typeof(AddColumnBuilder));
      AddColumnBuilder.NewColumnType = newColumnType;
      AddColumnBuilder.DefaultValue = defaultValue;
      AddColumnBuilder.IsEnabled = true;
      domain = Domain.Build(configuration);
    }

    #endregion
  }

  [Serializable]
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int FInt { get; set; }

    [Field]
    public int FInt2 { get; set; }

    [Field]
    public long FLong { get; set; }

    [Field]
    public long FLong2 { get; set; }

    [Field(Length = 1)]
    public string FString1 { get; set; }

    [Field(Length = 5)]
    public string FString5 { get; set; }

    [Field]
    public bool FBool { get; set; }

    [Field]
    public Guid FGuid { get; set; }

    [Field(Precision = 2, Scale = 1)]
    public decimal FDecimal{ get; set; }

    [Field(Nullable = false)]
    public string FNotNullableString { get; set; }

    [Field]
    public decimal? FNullableDecimal { get; set; }
  }

  public class FieldTypeChanger : IModule
  {
    private static Type ColumnType { get; set; }
    private static string ColumnName { get; set; }
    private static int? ColumnLength { get; set; }
    private static int? ColumnScale { get; set; }
    private static int? ColumnPrecision { get; set; }
    private static bool? ColumnNullable { get; set; }
    private static bool isEnabled;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(Type newType, string fieldName, int? length, int? precision, int? scale, bool? isNullable)
    {
      if (isEnabled) {
        throw new InvalidOperationException();
      }

      isEnabled = true;
      ColumnType = newType;
      ColumnName = fieldName;
      ColumnLength = length;
      ColumnScale = scale;
      ColumnPrecision = precision;
      ColumnNullable = isNullable;
      return new Disposable(_ => {
        isEnabled = false;
        ColumnType = null;
        ColumnName = null;
        ColumnLength = null;
      });
    }

    public virtual void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!isEnabled || !model.Types.Contains("X")) {
        return;
      }

      var xType = model.Types["X"];
      var oldFieled = xType.Fields[ColumnName];
      var newField = new FieldDef(ColumnType, context.Validator);
      newField.Length = ColumnLength;
      newField.Scale = ColumnScale;
      newField.Precision = ColumnPrecision;
      newField.Name = oldFieled.Name;
      if (ColumnNullable != null) {
        newField.IsNullable = ColumnNullable.Value;
      }
      _ = xType.Fields.Remove(oldFieled);
      xType.Fields.Add(newField);
    }
    
  }

  public class AddColumnBuilder : IModule
  {
    public static bool IsEnabled;

    public static Type NewColumnType { get; set; }

    public static object DefaultValue { get; set; }

    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled) {
        return;
      }
      if (!model.Types.Contains("X")) {
        return;
      }

      var xType = model.Types["X"];
      var newField = new FieldDef(NewColumnType, context.Validator);
      newField.Name = "NewColumn";
      if (DefaultValue != null) {
        newField.DefaultValue = DefaultValue;
      }
      xType.Fields.Add(newField);
    }
  }

  [Serializable]
  public class TestUpgrader : UpgradeHandler
  {
    private static bool isEnabled = false;
    private static UpgradeHint columnHint;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(UpgradeHint hint)
    {
      if (isEnabled) {
        throw new InvalidOperationException();
      }

      isEnabled = true;
      columnHint = hint;
      return new Disposable(_ => {
        isEnabled = false;
        columnHint = null;
      });
    }

    public override bool IsEnabled => isEnabled;

    protected override string DetectAssemblyVersion() => "1";

    public override bool CanUpgradeFrom(string oldVersion) => true;

    protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
    {
      if (columnHint!=null) {
        _ = hints.Add(columnHint);
      }
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage) => true;
  }
}

