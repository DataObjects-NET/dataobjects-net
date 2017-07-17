// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

// TODO: Refactor stupid MSSqlExtractorTests.cs and put all stuff here

using NUnit.Framework;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.SqlServer
{
  [TestFixture]
  public class ExtractorTest : SqlTest
  {
    
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

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
    
    [Test]
    public void ExtractUDTTest()
    {
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2008);
      string create = "CREATE TYPE GuidList AS TABLE ( Id UNIQUEIDENTIFIER NULL )";
      string drop = "if type_id('GuidList') is not null drop type GuidList";

      ExecuteNonQuery(drop);
      ExecuteNonQuery(create);
      ExtractDefaultSchema();
    }

    [Test]
    public void ExtractFulltextChangeTrackingTest()
    {
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2005);
      var createTable = @"CREATE TABLE [FullTextTestTable]
                            (
                              [Id] int not null,
                              [Auto] nvarchar(MAX),
                              [Manual] nvarchar(MAX),
                              [Off] nvarchar(MAX),
                              [OffNoPopulation] nvarchar(MAX),
                              CONSTRAINT PK_FullTextTestTable PRIMARY KEY (Id)
                            );";
      var dropTable = @"IF OBJECT_ID('dbo.FullTextTestTable', 'U') IS NOT NULL 
                          DROP TABLE dbo.FullTextTestTable;";

      var createAutoTrackingIndex = "CREATE FULLTEXT INDEX ON [dbo].[FullTextTestTable] ([Auto] LANGUAGE 1033) KEY INDEX PK_FullTextTestTable WITH CHANGE_TRACKING AUTO;";
      var createManualTrackingIndex = "CREATE FULLTEXT INDEX ON [dbo].[FullTextTestTable] ([Manual] LANGUAGE 1033) KEY INDEX PK_FullTextTestTable WITH CHANGE_TRACKING MANUAL;";
      var createOffTrackingIndex = "CREATE FULLTEXT INDEX ON [dbo].[FullTextTestTable] ([Off] LANGUAGE 1033) KEY INDEX PK_FullTextTestTable WITH CHANGE_TRACKING OFF;";
      var createOffNoPopulationIndex = "CREATE FULLTEXT INDEX ON [dbo].[FullTextTestTable] ([OffNoPopulation] LANGUAGE 1033) KEY INDEX PK_FullTextTestTable WITH CHANGE_TRACKING OFF, NO POPULATION;";

      var cases = new Pair<string, ChangeTrackingMode>[4];
      cases[0] = new Pair<string, ChangeTrackingMode>(createAutoTrackingIndex, ChangeTrackingMode.Auto);
      cases[1] = new Pair<string, ChangeTrackingMode>(createManualTrackingIndex , ChangeTrackingMode.Manual);
      cases[2] = new Pair<string, ChangeTrackingMode>(createOffTrackingIndex, ChangeTrackingMode.Off);
      cases[3] = new Pair<string, ChangeTrackingMode>(createOffNoPopulationIndex, ChangeTrackingMode.OffWithNoPopulation);

      foreach (var @case in cases) {
        ExecuteNonQuery(dropTable);
        ExecuteNonQuery(createTable);
        ExecuteNonQuery(@case.First);
        var schema = ExtractDefaultSchema();
        var table = schema.Tables["FullTextTestTable"];
        var ftIndex = table.Indexes.OfType<FullTextIndex>().FirstOrDefault();
        Assert.That(ftIndex, Is.Not.Null);
        Assert.That(ftIndex.ChangeTrackingMode, Is.EqualTo(@case.Second));
      }
      ExecuteNonQuery(dropTable);
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