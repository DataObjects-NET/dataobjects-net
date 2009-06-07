// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.04

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Core.Disposing;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Upgrade;
using Xtensive.Storage.Upgrade.Hints;
using Mode = Xtensive.Storage.DomainUpgradeMode;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class ColumnTypeTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain(Mode.Recreate);
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var x = new X();
          x.FInt = 1;
          x.FInt2 = 12345;
          x.FLong = (long) int.MaxValue + 12345;
          x.FLong2 = 12345;
          x.FBool = true;
          x.FString1 = "a";
          x.FString5 = "12345";
          x.FGuid = new Guid("E484EE28-3801-445B-9DF0-FBCBE5AA4883");
          t.Complete();
        }
      }
    }

    [Test]
    public void Int32ToStringTest()
    {
      var newValue = "1";
      Build(newValue, "FInt", Mode.Perform);
      SetUp();
      Build(newValue, "FInt", Mode.PerformSafely);
    }

    [Test]
    public void Int32ToShortStringTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(typeof (int), 3, "FInt2", Mode.PerformSafely));
      Build(typeof (int), 3, "FInt2", Mode.Perform);
    }

    [Test]
    public void StringToInt32Test()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(0, "FString1", Mode.PerformSafely));
      Build(typeof (int), null, "FString1", Mode.Perform);
    }

    [Test]
    public void StringToShortStringTest()
    {
      var newValue = "123";
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(newValue, 3, "FString5", Mode.PerformSafely));
      Build(newValue, 3, "FString5", Mode.Perform);
    }

    [Test]
    public void StringToStringTest()
    {
      var newValue = "a";
      Build(newValue, 3, "FString1", Mode.Perform);
      SetUp();
      Build(newValue, 3, "FString1", Mode.PerformSafely);
      SetUp();
    }

    [Test]
    public void BoolToStringTest()
    {
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(typeof (string), 100, "FBool", Mode.PerformSafely));
      Build(typeof (string), 100, "FBool", Mode.Perform);
    }

    [Test]
    public void GuidToStringTest()
    {
      var newValue = "E484EE28-3801-445B-9DF0-FBCBE5AA4883";
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(newValue, 100, "FGuid", Mode.PerformSafely));
      Build(typeof (string), 100, "FGuid", Mode.Perform);
    }

    [Test]
    public void Int32ToInt64Test()
    {
      var newValue = 12345L;
      Build(newValue, "FInt2", Mode.Perform);
      SetUp();
      Build(newValue, "FInt2", Mode.PerformSafely);
      SetUp();
    }

    [Test]
    public void Int64ToInt32Test()
    {
      var newValue = 12345;
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(newValue, "FLong", Mode.PerformSafely));
      AssertEx.Throws<SchemaSynchronizationException>(() => Build(newValue, "FLong2", Mode.PerformSafely));
      Build(0, "FLong", Mode.Perform);
      SetUp();
      Build(0, "FLong2", Mode.Perform);
    }

    [Test]
    public void AddNonNullableColumnTest()
    {
      BuildWithNewColumn(typeof (Guid));
      BuildWithNewColumn(typeof (bool));
      BuildWithNewColumn(typeof (int));
      BuildWithNewColumn(typeof (long));
      BuildWithNewColumn(typeof (float));
      BuildWithNewColumn(typeof (TimeSpan));
      BuildWithNewColumn(typeof (DateTime));
    }

    [Test]
    public void ColumnTypesTest()
    {
      if (domain != null)
        domain.DisposeSafely();
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Storage.DbTypeSupportModel.X));
      domain = Domain.Build(configuration);
      
      if (domain != null)
        domain.DisposeSafely();
      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      configuration.Types.Register(typeof (Storage.DbTypeSupportModel.X));
      domain = Domain.Build(configuration);
    }

    # region Helper methods

    public void Build<T>(T value, int? length, string fieldName, DomainUpgradeMode mode)
    {
      using (FieldTypeChanger.Enable<T>(fieldName, length)) {
        BuildDomain(mode);
      }

      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var x = Query<X>.All.First();
          Assert.AreEqual(value, x.GetFieldValue<T>(x.Type.Fields[fieldName], false));
        }
      }
    }

    public void Build(Type newType, int? length, string fieldName, DomainUpgradeMode mode)
    {
      using (FieldTypeChanger.Enable(newType, fieldName, length)) {
        BuildDomain(mode);
      }
    }

    public void Build<T>(T value, string fieldName, DomainUpgradeMode mode)
    {
      Build<T>(value, null, fieldName, mode);
    }

    private void BuildDomain(DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(typeof (X));
      configuration.Builders.Add(typeof (FieldTypeChanger));
      domain = Domain.Build(configuration);
    }

    private void BuildWithNewColumn(Type columnType)
    {
      SetUp();
      if (domain != null)
        domain.DisposeSafely();
      AddColumnBuilder.NewColumnType = columnType;
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.Types.Register(typeof (X));
      configuration.Builders.Add(typeof (AddColumnBuilder));
      domain = Domain.Build(configuration);
    }

    # endregion
  }

  public class FieldTypeChanger : IDomainBuilder
  {
    private static Type ColumnType { get; set; }
    private static string ColumnName { get; set; }
    private static int? ColumnLength { get; set; }
    private static bool isEnabled;

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable<T>(string fieldName, int? length)
    {
      return Enable(typeof (T), fieldName, length);
    }

    /// <exception cref="InvalidOperationException">Handler is already enabled.</exception>
    public static IDisposable Enable(Type newType, string fieldName, int? length)
    {
      if (isEnabled)
        throw new InvalidOperationException();
      isEnabled = true;
      ColumnType = newType;
      ColumnName = fieldName;
      ColumnLength = length;
      return new Disposable(_ => {
        isEnabled = false;
        ColumnType = null;
        ColumnName = null;
        ColumnLength = null;
      });
    }

    public void Build(BuildingContext context, DomainModelDef model)
    {
      if (!isEnabled || !model.Types.Contains("X"))
        return;

      var xType = model.Types["X"];
      var oldFieled = xType.Fields[ColumnName];
      var newField = new FieldDef(ColumnType);
      if (ColumnLength.HasValue)
        newField.Length = ColumnLength.Value;
      newField.Name = oldFieled.Name;
      xType.Fields.Remove(oldFieled);
      xType.Fields.Add(newField);
    }
    
  }

  public class AddColumnBuilder : IDomainBuilder
  {
    public static Type NewColumnType { get; set; }

    public void Build(BuildingContext context, DomainModelDef model)
    {
      if (!model.Types.Contains("X"))
        return;
      
      var xType = model.Types["X"];
      var newField = new FieldDef(NewColumnType);
      newField.Name = "NewColumn";
      xType.Fields.Add(newField);
    }
    
  }

  [HierarchyRoot]
  public class X : Entity
  {
    [Field, KeyField]
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

    public override bool IsEnabled {
      get {
        return isEnabled;
      }
    }
    
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

