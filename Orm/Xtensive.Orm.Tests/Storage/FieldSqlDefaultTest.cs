// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.02.26

using System;
using System.Linq;
using NUnit.Framework;
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

    [Field(DefaultValue = "2012.12.12", DefaultSqlExpression = "'2013.12.13'")]
    public DateTime FDateTime { get; set; }
    
    [Field(Length = 1000, DefaultValue = "default value", DefaultSqlExpression = "'sql value'")]
    public string FString { get; set; }
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
      Domain = Domain.Build(configuration);

      var driver = TestSqlDriver.Create(Domain.Configuration.ConnectionInfo);
      var isPostgreSql = StringComparer.InvariantCultureIgnoreCase.Compare(Domain.Configuration.ConnectionInfo.Provider, "postgresql")==0;
      using (var connection = driver.CreateConnection()) {
        connection.Open();
        var command = connection.CreateCommand();
        if (isPostgreSql)
          command.CommandText = "INSERT INTO \"TestEntity\" Default values;";
        else
          command.CommandText = "INSERT INTO TestEntity(Id) values(1);";
        command.ExecuteNonQuery(); 
        connection.Close();
      }


      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<TestEntity>().First();
          Assert.AreEqual(1, entity.Id);
          Assert.AreEqual(64, entity.FByte);
          Assert.AreEqual(65, entity.FSByte);
          Assert.AreEqual(66, entity.FShort);
          Assert.AreEqual(67, entity.FUShort);
          Assert.AreEqual(68, entity.FInt);
          Assert.AreEqual(69, entity.FUInt);
          Assert.AreEqual(70L, entity.FLong);
          Assert.AreEqual(71L, entity.FULong);
          Assert.AreEqual(72.0, entity.FFloat);
          Assert.AreEqual(73.0, entity.FDouble);
          Assert.AreEqual(12.12M, entity.FDecimal);
          Assert.AreEqual(DateTime.Parse("2013.12.13"), entity.FDateTime);
          Assert.AreEqual("sql value", entity.FString);
          transaction.Complete();
        }
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
        command.CommandText = "INSERT INTO TestEntity1 DEFAULT VALUES;";
        command.ExecuteNonQuery(); 
        connection.Close();
      }
      
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<TestEntity1>().FirstOrDefault();
        Assert.IsNotNull(entity);
        Assert.IsNotNull(entity.Id);
      }
    }
  }
}
