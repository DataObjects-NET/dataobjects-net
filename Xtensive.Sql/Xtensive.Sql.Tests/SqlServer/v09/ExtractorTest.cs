// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

// TODO: Refactor stupid MSSqlExtractorTests.cs and put all stuff here

using NUnit.Framework;
using System.Linq;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Tests.SqlServer.v09
{
  [TestFixture]
  public class ExtractorTest : SqlTest
  {
    protected override string Url { get { return TestUrl.SqlServer2005Aw; } }

    [Test]
    public void Extract()
    {
      var catalog = ExtractCatalog();
    }

    [Test]
    public void ExtractDomainsTest()
    {
      string createTable =
        "create table table_with_domained_columns (id int primary key, value test_type)";
      string dropTable =
        "if object_id('table_with_domained_columns') is not null drop table table_with_domained_columns";

      ExecuteNonQuery(dropTable);
      DropDomain();
      CreateDomain();
      ExecuteNonQuery(createTable);

      var schema = ExtractCatalog().DefaultSchema;
      var definedDomain = schema.Domains.Single(domain => domain.Name=="test_type");
      Assert.AreEqual(Driver.ServerInfo.DataTypes["bigint"].Type, definedDomain.DataType.Type);

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

      ExecuteNonQuery(dropTable);
      ExecuteNonQuery(createTable);

      var schema = ExtractCatalog().DefaultSchema;
      var table = schema.Tables["table_with_default_constraint"];
      Assert.AreEqual(1, table.TableConstraints.Count);
      Assert.AreEqual("id", ((DefaultConstraint) table.TableConstraints[0]).Column.Name);
    }

    [Test]
    public void ExtractComputedColumnTest()
    {
      string createTable = @"
        CREATE TABLE Tmp ( [Value] [int] NOT NULL, [Sum]  AS ([Value]+(1)) PERSISTED ) ON [PRIMARY]
        CREATE NONCLUSTERED INDEX [IX_Sum] ON Tmp ( [Sum] ASC ) ON [PRIMARY]";
      string createView = @"
        CREATE VIEW Tmp_View WITH SCHEMABINDING AS SELECT Value, Sum FROM dbo.Tmp";
      string createIndex = @"
        CREATE UNIQUE CLUSTERED INDEX [x] ON Tmp_View (	[Value] ASC )";
      string drop = @"
        if object_id('Tmp_View') is not null drop view Tmp_View
        if object_id('Tmp') is not null drop table Tmp";

      ExecuteNonQuery(drop);
      ExecuteNonQuery(createTable);
      ExecuteNonQuery(createView);

      ExtractDefaultSchema();
      ExecuteNonQuery(drop);
    }

    private void CreateDomain()
    {
      var schema = ExtractCatalog().DefaultSchema;
      var domain = schema.CreateDomain("test_type", new SqlValueType(SqlType.Int64));
      var commandText = Driver.Compile(SqlDdl.Create(domain)).GetCommandText();
      ExecuteNonQuery(commandText);
    }

    private void DropDomain()
    {
      var schema = ExtractCatalog().DefaultSchema;
      var domain = schema.Domains["test_type"];
      if (domain==null)
        return;
      var commandText = Driver.Compile(SqlDdl.Drop(domain)).GetCommandText();
      ExecuteNonQuery(commandText);
    }
  }
}