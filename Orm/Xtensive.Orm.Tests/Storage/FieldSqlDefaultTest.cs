// Copyright (C) 2014-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.FieldSqlDefaultTestModel;

namespace Xtensive.Orm.Tests.Storage.FieldSqlDefaultTestModel
{
  [HierarchyRoot]
  public class TestEntity1 : Entity
  {
    [Field (DefaultSqlExpression = "newid()"), Key]
    public Guid Id { get; private set; }
  }

  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field (DefaultSqlExpression = "1"), Key]
    public int Id { get; private set; }

    [Field(DefaultValue = byte.MaxValue, DefaultSqlExpression = "64")]
    public byte FByte { get; set; }

    [Field(DefaultValue = sbyte.MaxValue, DefaultSqlExpression = "65")]
    public sbyte FSByte { get; set; }

    [Field(DefaultValue = short.MaxValue, DefaultSqlExpression = "66")]
    public short FShort { get; set; }

    [Field(DefaultValue = ushort.MaxValue, DefaultSqlExpression = "67")]
    public ushort FUShort { get; set; }

    [Field(DefaultValue = int.MaxValue, DefaultSqlExpression = "68")]
    public int FInt { get; set; }

    [Field(DefaultValue = uint.MaxValue, DefaultSqlExpression = "69")]
    public uint FUInt { get; set; }

    [Field(DefaultValue = long.MaxValue, DefaultSqlExpression = "70")]
    public long FLong { get; set; }

    [Field(DefaultValue = long.MaxValue, DefaultSqlExpression = "71")] // SQLite provides only 8 byte signed integer
    public ulong FULong { get; set; }

    [Field(DefaultValue = float.MaxValue, DefaultSqlExpression = "72.0")]
    public float FFloat { get; set; }

    [Field(DefaultValue = float.MaxValue, DefaultSqlExpression = "73.0")]
    public double FDouble { get; set; }

    [Field(DefaultValue = 12.12, DefaultSqlExpression = "12.12")]
    public decimal FDecimal { get; set; }

    [Field(DefaultValue = "2012-12-12", DefaultSqlExpression = "'2013-12-13'")]// for oracle it will be set in IModule
    public DateTime FDateTime { get; set; }
    
    [Field(Length = 1000, DefaultValue = "default value", DefaultSqlExpression = "'sql value'")]
    public string FString { get; set; }
  }

  public class OracleDefaultValueModifier : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (StorageProviderInfo.Instance.Provider != StorageProvider.Oracle) {
        return;
      }

      var field = model.Types[nameof(TestEntity)].Fields[nameof(TestEntity.FDateTime)];
      field.DefaultValue = FormatDate(new DateTime(2012, 12, 12));
      field.DefaultSqlExpression = $"'{FormatDate(new DateTime(2013, 12, 13))}'";
    }

    private static string FormatDate(DateTime dateToFormat) => dateToFormat.ToString("dd-MMM-yyyy");
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class FieldSqlDefaultTest : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof(TestEntity));
      configuration.Types.Register(typeof(OracleDefaultValueModifier));
      Domain = Domain.Build(configuration);

      var driver = TestSqlDriver.Create(Domain.Configuration.ConnectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        var command = connection.CreateCommand();
        var translator = driver.Translator;
        command.CommandText = $"INSERT INTO {translator.QuoteIdentifier("TestEntity")}({translator.QuoteIdentifier("Id")}) values(1)";
        _ = command.ExecuteNonQuery();
        connection.Close();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<TestEntity>().First();
        Assert.That(entity.Id, Is.EqualTo(1));
        Assert.That(entity.FByte, Is.EqualTo(64));
        Assert.That(entity.FSByte, Is.EqualTo(65));
        Assert.That(entity.FShort, Is.EqualTo(66));
        Assert.That(entity.FUShort, Is.EqualTo(67));
        Assert.That(entity.FInt, Is.EqualTo(68));
        Assert.That(entity.FUInt, Is.EqualTo(69));
        Assert.That(entity.FLong, Is.EqualTo(70L));
        Assert.That(entity.FULong, Is.EqualTo(71L));
        Assert.That(entity.FFloat, Is.EqualTo(72.0));
        Assert.That(entity.FDouble, Is.EqualTo(73.0));
        Assert.That(entity.FDecimal, Is.EqualTo(12.12M));
        Assert.That(entity.FDateTime, Is.EqualTo(DateTime.Parse("2013.12.13")));
        Assert.That(entity.FString, Is.EqualTo("sql value"));
        transaction.Complete();
      }
    }

    [Test]
    public void DefaultValueTestForKeyFieldTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (TestEntity1));
      Domain = Domain.Build(configuration);

      var driver = TestSqlDriver.Create(Domain.Configuration.ConnectionInfo);
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        var command = connection.CreateCommand();
        var translator = driver.Translator;
        command.CommandText = $"INSERT INTO {translator.QuoteIdentifier("TestEntity1")} DEFAULT VALUES;";
        _ = command.ExecuteNonQuery();
        connection.Close();
      }
      
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<TestEntity1>().FirstOrDefault();
        Assert.That(entity, Is.Not.Null);
        Assert.That(entity.Id, Is.Not.Null);
      }
    }
  }
}
