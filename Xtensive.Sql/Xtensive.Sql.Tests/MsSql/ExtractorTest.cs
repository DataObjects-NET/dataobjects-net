// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

// TODO: Refactor stupid MSSqlExtractorTests.cs and put all stuff here

using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.MsSql
{
  [TestFixture]
  public class ExtractorTest
  {
    private SqlConnection connection;
    private SqlDriver driver;

    private void ExecuteCommand(string commandText)
    {
      using (var command = connection.CreateCommand()) {
        command.CommandText = commandText;
        command.ExecuteNonQuery();
      }
    }

    private Schema ExtractModel()
    {
      using (var transaction = connection.BeginTransaction()) {
        var result = driver.ExtractModel(connection, transaction);
        transaction.Commit();
        return result.DefaultSchema;
      }
    }

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      driver = SqlDriver.Create(TestUrl.SqlServer2005);
      connection = driver.CreateConnection(TestUrl.SqlServer2005);
      try {
        connection.Open();
      }
      catch (SystemException e) {
        Console.WriteLine(connection.Url);
        Console.WriteLine(e);
      }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      if (connection!=null && connection.State==ConnectionState.Open)
        connection.Close();
    }

    [Test]
    public void ExtractDomainsTest()
    {
      string createTable =
        "create table table_with_domained_columns (id int primary key, value test_type)";
      string dropTable =
        "if object_id('table_with_domained_columns') is not null drop table table_with_domained_columns";

      ExecuteCommand(dropTable);
      DropDomain();
      CreateDomain();
      ExecuteCommand(createTable);

      var schema = ExtractModel();
      var definedDomain = schema.Domains.Single(domain => domain.Name=="test_type");
      Assert.AreEqual(driver.ServerInfo.DataTypes["bigint"].Type, definedDomain.DataType.Type);

      var columnDomain = schema.Tables["table_with_domained_columns"].TableColumns["value"].Domain;
      Assert.IsNotNull(columnDomain);
      Assert.AreEqual("test_type", columnDomain.Name);
    }

    [Test]
    public void ExtractDefaultConstraintTest()
    {
      string createTable =
        "create table table_with_default_constraint (id int default 0)";
      string dropTable =
        "if object_id('table_with_default_constraint') is not null drop table table_with_default_constraint";

      ExecuteCommand(dropTable);
      ExecuteCommand(createTable);

      var schema = ExtractModel();
      var table = schema.Tables["table_with_default_constraint"];
      Assert.AreEqual(1, table.TableConstraints.Count);
      Assert.AreEqual("id", ((DefaultConstraint) table.TableConstraints[0]).Column.Name);
    }

    private void CreateDomain()
    {
      var schema = ExtractModel();
      var domain = schema.CreateDomain("test_type", new SqlValueType(SqlType.Int64));
      var commandText = driver.Compile(SqlDdl.Create(domain)).GetCommandText();
      ExecuteCommand(commandText);
    }

    private void DropDomain()
    {
      var schema = ExtractModel();
      var domain = schema.Domains["test_type"];
      if (domain==null)
        return;
      var commandText = driver.Compile(SqlDdl.Drop(domain)).GetCommandText();
      ExecuteCommand(commandText);
    }
  }
}
