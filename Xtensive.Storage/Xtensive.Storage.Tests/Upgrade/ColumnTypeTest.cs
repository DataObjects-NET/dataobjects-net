// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Upgrade;
using Mode = Xtensive.Storage.DomainUpgradeMode;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class ColumnTypeTest
  {
    private Domain domain;
    
    [SetUp]
    public void SetUp()
    {
      BuildDomain(Mode.Recreate);
      using (Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          var x = new X {
            FInt = 1, 
            FInt2 = 12345, 
            FLong = (long) int.MaxValue + 12345, 
            FLong2 = 12345, 
            FBool = true, 
            FString1 = "a", 
            FString5 = "12345", 
            FGuid = new Guid("E484EE28-3801-445B-9DF0-FBCBE5AA4883"), 
            FDecimal = new decimal(1.2),
          };
          t.Complete();
        }
      }
    }

    [Test]
    public void ValidateModeTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        UpgradeDomain(typeof (string), null, null, null, "FInt", "1", Mode.Validate));
    }
    
    [Test]
    public void Int32ToStringTest()
    {
      UpgradeDomain(typeof (string), null, null, null, "FInt", "1", Mode.Perform);
    }

    [Test]
    public void Int32ToStringSafelyTest()
    {
      UpgradeDomain(typeof (string), null, null, null, "FInt", "1", Mode.PerformSafely);      
    }

    [Test]
    public void Int32ToShortStringTest()
    {
      UpgradeDomain(typeof (string), 3, null, null, "FInt2", null, Mode.Perform);
    }

    [Test]
    public void Int32ToShortStringSafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        UpgradeDomain(typeof (string), 3, null, null, "FInt2", null, Mode.PerformSafely));
    }

    [Test]
    public void StringToInt32Test()
    {
      UpgradeDomain(typeof (int), null, null, null, "FString1", 0, Mode.Perform);
    }

    [Test]
    public void StringToInt32SafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        UpgradeDomain(typeof (int), null, null, null, "FString1", 0, Mode.PerformSafely));
    }

    [Test]
    public void StringToInt32WithHintTest()
    {
      using (TestUpgrader.Enable(new ChangeFieldTypeHint(typeof (X), "FString5"))) {
        UpgradeDomain(typeof (int), null, null, null, "FString5", 12345, Mode.PerformSafely);
      }
    }

    [Test]
    public void StringToShortStringTest()
    {
      UpgradeDomain(typeof (string), 3, null, null, "FString5", "123", Mode.Perform);
    }

    [Test]
    public void StringToShortStringSafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        UpgradeDomain(typeof (string), 3, null, null, "FString5", string.Empty, Mode.PerformSafely));
    }

    [Test]
    public void StringToLongStringTest()
    {
      UpgradeDomain(typeof (string), 3, null, null, "FString1", "a", Mode.Perform);
    }

    [Test]
    public void StringToLongStringSafelyTest()
    {
      UpgradeDomain(typeof (string), 3, null, null, "FString1", "a", Mode.PerformSafely);
    }

    [Test]
    public void BoolToStringTest()
    {
      UpgradeDomain(typeof (string), 100, null, null, "FBool", null, Mode.Perform);
    }

    [Test]
    public void BoolToStringSafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
        UpgradeDomain(typeof (string), 100, null, null, "FBool", string.Empty, Mode.PerformSafely));
    }

    [Test]
    public void Int32ToInt64Test()
    {
      UpgradeDomain(typeof (long), null, null, null, "FInt2", 12345L, Mode.Perform);
    }

    [Test]
    public void Int32ToInt64SafelyTest()
    {
      UpgradeDomain(typeof (long), null, null, null, "FInt2", 12345L, Mode.PerformSafely);
    }

    [Test]
    public void Int64ToInt32Test()
    {
      UpgradeDomain(typeof (int), null, null, null, "FLong", 0, Mode.Perform);
    }

    [Test]
    public void Int64ToInt32SafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => 
       UpgradeDomain(typeof (int), null, null, null, "FLong", 12345, Mode.PerformSafely));
    }

    [Test]
    public void DecimalToLongDecimalTest()
    {
      UpgradeDomain(typeof (decimal), null, 3, 2, "FDecimal", new decimal(1.2), Mode.Perform);
    }

    [Test]
    public void DecimalToLongDecimalSafelyTest()
    {
      UpgradeDomain(typeof (decimal), null, 3, 2, "FDecimal", new decimal(1.2), Mode.PerformSafely);
    }

    [Test]
    public void DecimalToShortDecimalTest()
    {
      UpgradeDomain(typeof (decimal), null, 2, 0, "FDecimal", new decimal(1.0), Mode.Perform);
    }

    [Test]
    public void DecimalToShortDecimalSafelyTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        UpgradeDomain(typeof (decimal), null, 2, 0, "FDecimal", new decimal(1.2), Mode.PerformSafely));
    }

    [Test]
    public void AddNonNullableColumnTest()
    {
      UpgradeDomain(typeof (Guid));
      UpgradeDomain(typeof (bool));
      UpgradeDomain(typeof (int));
      UpgradeDomain(typeof (long));
      UpgradeDomain(typeof (float));
      UpgradeDomain(typeof (TimeSpan));
      UpgradeDomain(typeof (DateTime));
    }

    #region Helper methods

    private void BuildDomain(DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof (X));
      configuration.Types.Register(typeof (TestUpgrader));
      configuration.Types.Register(typeof (FieldTypeChanger));
      domain = Domain.Build(configuration);
    }

    private void UpgradeDomain(Type newColumnType, int? newLength, int? newPresicion, int? newScale,
      string changedFieldName, object expectedValue, DomainUpgradeMode mode)
    {
      using (FieldTypeChanger.Enable(newColumnType, changedFieldName, newLength, newPresicion, newScale)) {
        BuildDomain(mode);
      }

      using (Session.Open(domain)) {
        using (var t = Transaction.Open()) {
          var x = Query.All<X>().First();
          Assert.AreEqual(expectedValue, x[changedFieldName]);
        }
      }
    }

    private void UpgradeDomain(Type newColumnType)
    {
      SetUp();
      if (domain != null)
        domain.DisposeSafely();
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.Types.Register(typeof (X));
      configuration.Types.Register(typeof (TestUpgrader));
      configuration.Types.Register(typeof (FieldTypeChanger));
      configuration.Types.Register(typeof (AddColumnBuilder));
      AddColumnBuilder.NewColumnType = newColumnType;
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
    public decimal FDecimal{ get; set;}
  }

  public class FieldTypeChanger : IModule
  {
    private static Type ColumnType { get; set; }
    private static string ColumnName { get; set; }
    private static int? ColumnLength { get; set; }
    private static int? ColumnScale { get; set; }
    private static int? ColumnPrecision { get; set; }
    private static bool isEnabled;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(Type newType, string fieldName, int? length, int? precision, int? scale)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      ColumnType = newType;
      ColumnName = fieldName;
      ColumnLength = length;
      ColumnScale = scale;
      ColumnPrecision = precision;
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
      if (!isEnabled || !model.Types.Contains("X"))
        return;

      var xType = model.Types["X"];
      var oldFieled = xType.Fields[ColumnName];
      var newField = new FieldDef(ColumnType);
      newField.Length = ColumnLength;
      newField.Scale = ColumnScale;
      newField.Precision = ColumnPrecision;
      newField.Name = oldFieled.Name;
      xType.Fields.Remove(oldFieled);
      xType.Fields.Add(newField);
    }
    
  }

  public class AddColumnBuilder : IModule
  {
    public static bool IsEnabled;

    public static Type NewColumnType { get; set; }

    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      if (!model.Types.Contains("X"))
        return;
      
      var xType = model.Types["X"];
      var newField = new FieldDef(NewColumnType);
      newField.Name = "NewColumn";
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
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      columnHint = hint;
      return new Disposable(_ => {
        isEnabled = false;
        columnHint = null;
      });
    }

    public override bool IsEnabled { get { return isEnabled; } }
    
    protected override string DetectAssemblyVersion()
    {
      return "1";
    }

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    protected override void AddUpgradeHints()
    {
      base.AddUpgradeHints();
      if (columnHint!=null)
        UpgradeContext.Current.Hints.Add(columnHint);
    }

    public override bool IsTypeAvailable(Type type, UpgradeStage upgradeStage)
    {
      return true;
    }
  }
}

