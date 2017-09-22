// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

// TODO: Refactor stupid MSSqlExtractorTests.cs and put all stuff here

using System;
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
    private string defaultTableName = "EntityWithIndexes";
    private string secondaryTableName = "EntityWithIndexesAux";
    private string clusteredPkName = "PK_Clustered_Index";
    private string nonClusteredPkName = "PK_SomePK";
    private string clusteredIndexName = "IX_Clustered_Index";
    private string nonClusteredIndexName = "IX_Non_Clustered_Index";
    private string nonClusteredColumnStore = "IX_Columnstore";
    private string clusteredColumnStore = "IX_Non_Clustered_columnstore";
    private string spatialIndexName = "IX_Spatial_Index";
    private string xmlIndexName = "IX_XML_Index";

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    public override void SetUp()
    {
      DropTableIfExists(defaultTableName);
      DropTableIfExists(secondaryTableName);
    }

    [Test]
    public void Extract()
    {
      var catalog = ExtractCatalog();
    }

    [Test]
    public void ExtractIndexTypesTest1()
    {
      CreateTable(defaultTableName, new[] {
        "[id][int] NOT NULL",  
        "[IntField1] [int] NOT NULL", 
        "[XmlField] [xml]",
        "[GeometryField] [geometry]"
      });

      CreatePrimaryKey(defaultTableName, clusteredPkName, "id", true);
      CreatePrimaryXmlIndex(xmlIndexName, defaultTableName, "XmlField");
      CreateIndex(nonClusteredIndexName, defaultTableName, "IntField1", false);
      CreateSpatialIndex(spatialIndexName, defaultTableName, "GeometryField");

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table = schema.Tables[defaultTableName];
      var tableIndexes = table.Indexes;
      Assert.IsTrue(table.Indexes.Count==2);

      var nonClusteredIndex = tableIndexes[nonClusteredIndexName];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsFullText);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var spatialIndex = tableIndexes[spatialIndexName];
      Assert.IsNotNull(spatialIndex);
      Assert.IsTrue(spatialIndex.IsSpatial);
      Assert.IsFalse(spatialIndex.IsClustered);
      Assert.IsFalse(spatialIndex.IsFullText);
      Assert.IsFalse(spatialIndex.IsBitmap);
      Assert.IsFalse(spatialIndex.IsFullText);

      var tableConstratins = table.TableConstraints;
      Assert.IsTrue(table.TableConstraints.Count==1);

      var clusteredPk = tableConstratins[clusteredPkName] as PrimaryKey;
      Assert.IsNotNull(clusteredPk);
      Assert.IsTrue(clusteredPk.IsClustered);
    }

    [Test]
    public void ExtractIndexTypesTest2()
    {
      CreateTable(secondaryTableName, new [] {
        "[Id] uniqueidentifier NOT NULL",
        "[A] integer NOT NULL",
        "[B] integer NOT NULL",
        "[C] integer NOT NULL",
      });

      CreatePrimaryKey(secondaryTableName, nonClusteredPkName+"_aux", "Id", false);
      CreateIndex(nonClusteredIndexName + "_aux", secondaryTableName, "A", false);
      CreateColumnStoreIndex(nonClusteredColumnStore + "_aux", secondaryTableName, "C", false);

      CreateTable(defaultTableName, new [] {
        "[id] [int] NOT NULL",
        "[IntField1] [int] NOT NULL", 
        "[IntField2] [int] NOT NULL",
        "[IntField3] [int] NOT NULL",
        "[IntField4] [int] NOT NULL",
        "[IntField5] [int] NOT NULL",
      });

      CreateColumnStoreIndex(nonClusteredColumnStore, defaultTableName, "IntField5", false);
      CreateIndex(nonClusteredIndexName, defaultTableName, "IntField2", false);
      CreatePrimaryKey(defaultTableName, nonClusteredPkName, "id", false);

      CreateIndex(clusteredIndexName+"_aux", secondaryTableName, "B", true);

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table = schema.Tables[defaultTableName];
      var indexes = table.Indexes;
      Assert.IsTrue(indexes.Count==1);

      var nonClusteredIndex = indexes[nonClusteredIndexName];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var tableConstratins = table.TableConstraints;
      Assert.IsTrue(tableConstratins.Count==1);

      var nonClusteredPk = tableConstratins[nonClusteredPkName] as PrimaryKey;
      Assert.IsNotNull(nonClusteredPk);
      Assert.IsFalse(nonClusteredPk.IsClustered);

      var table2 = schema.Tables[secondaryTableName];
      var table2Indexes  = table2.Indexes;
      Assert.IsTrue(table2Indexes.Count==2);

      var table2NonClusteredIndex = table2Indexes[nonClusteredIndexName + "_aux"];
      Assert.IsNotNull(table2NonClusteredIndex);
      Assert.IsFalse(table2NonClusteredIndex.IsClustered);
      Assert.IsFalse(table2NonClusteredIndex.IsSpatial);
      Assert.IsFalse(table2NonClusteredIndex.IsBitmap);
      Assert.IsFalse(table2NonClusteredIndex.IsFullText);

      var table2ClusteredIndex = table2Indexes[clusteredIndexName + "_aux"];
      Assert.IsNotNull(table2ClusteredIndex);
      Assert.IsTrue(table2ClusteredIndex.IsClustered);
      Assert.IsFalse(table2ClusteredIndex.IsSpatial);
      Assert.IsFalse(table2ClusteredIndex.IsBitmap);
      Assert.IsFalse(table2ClusteredIndex.IsFullText);

      var table2Constratins = table2.TableConstraints;
      Assert.IsTrue(table2Constratins.Count==1);
      var table2NonClusteredPk = table2Constratins[nonClusteredPkName + "_aux"] as PrimaryKey;
      Assert.IsNotNull(table2NonClusteredPk);
      Assert.IsFalse(table2NonClusteredPk.IsClustered);
    }

    [Test]
    public void ExtractIndexTypesTest3()
    {
      Require.ProviderVersionAtLeast(new Version(12, 0));

      CreateTable(secondaryTableName, new[] {
        "[Id] uniqueidentifier NOT NULL",
        "[A] integer NOT NULL",
      });
      CreatePrimaryKey(secondaryTableName, nonClusteredPkName+"_aux", "Id", false);
      CreateIndex(clusteredColumnStore + "_aux", secondaryTableName, "A", false);

      CreateTable(defaultTableName, new[] {
        "[id] [int] NOT NULL",
        "[IntField1] [int] NOT NULL", 
        "[IntField2] [int] NOT NULL",
        "[IntField3] [int] NOT NULL",
        "[IntField4] [int] NOT NULL"
      });

      CreatePrimaryKey(defaultTableName, nonClusteredPkName, "id", false);
      CreateIndex(nonClusteredIndexName, defaultTableName, "IntField1", false);
      CreateColumnStoreIndex(clusteredColumnStore, defaultTableName, "IntField2", true);

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table = schema.Tables[defaultTableName];
      var indexes = table.Indexes;
      Assert.IsTrue(indexes.Count==1);

      var nonClusteredIndex = indexes[nonClusteredIndexName];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var tableConstratins = table.TableConstraints;
      Assert.IsTrue(tableConstratins.Count==1);

      var nonClusteredPk = tableConstratins[nonClusteredPkName] as PrimaryKey;
      Assert.IsNotNull(nonClusteredPk);
      Assert.IsFalse(nonClusteredPk.IsClustered);

      var table2 = schema.Tables[secondaryTableName];
      var table2Indexes = table2.Indexes;
      Assert.IsTrue(indexes.Count==1);

      var table2NonClusteredIndex = table2Indexes[clusteredColumnStore + "_aux"];
      Assert.IsNotNull(table2NonClusteredIndex);
      Assert.IsFalse(table2NonClusteredIndex.IsClustered);
      Assert.IsFalse(table2NonClusteredIndex.IsSpatial);
      Assert.IsFalse(table2NonClusteredIndex.IsBitmap);
      Assert.IsFalse(table2NonClusteredIndex.IsFullText);

      var table2Constratins = table2.TableConstraints;
      Assert.IsTrue(table2Constratins.Count==1);

      var table2NonClusteredPk = tableConstratins[nonClusteredPkName] as PrimaryKey;
      Assert.IsNotNull(table2NonClusteredPk);
      Assert.IsFalse(table2NonClusteredPk.IsClustered);
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

#region Helpers

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

    private void CreateTable(string tableName, string[] columns)
    {
      var createTableTemplate = @"CREATE TABLE [{0}] ({1})";
      ExecuteNonQuery(string.Format(createTableTemplate, tableName, string.Join(", ", columns)));
    }

    private void DropTableIfExists(string tableName)
    {
      var dropTableTemplate = "if object_id('[{0}]') is not null drop table [{0}]";
      ExecuteNonQuery(string.Format(dropTableTemplate, tableName));
    }

    private void CreateIndex(string indexName, string tableName, string columnName, bool isClustered)
    {
      var createClusteredIndexTemplate = @"CREATE {0} INDEX [{1}] ON [{2}] ([{3}]);";
      ExecuteNonQuery(string.Format(createClusteredIndexTemplate, isClustered ? "CLUSTERED" : "NONCLUSTERED", indexName, tableName, columnName));
    }

    private void CreatePrimaryXmlIndex(string indexName, string tableName, string columnName)
    {
      var createPrimaryXmlIndexTemplate = @"CREATE PRIMARY XML INDEX [{0}] ON [{1}] ([{2}]);";
      ExecuteNonQuery(string.Format(createPrimaryXmlIndexTemplate, indexName, tableName, columnName));
    }

    private void CreateSpatialIndex(string indexName, string tableName, string columnName)
    {
      var createSpatialIdnexTamplate = @"CREATE SPATIAL INDEX [{0}] ON [{1}] ([{2}]) WITH (BOUNDING_BOX = ( 0, 0, 500, 200 ))";
      ExecuteNonQuery(string.Format(createSpatialIdnexTamplate, indexName, tableName, columnName));
    }

    private void CreatePrimaryKey(string tableName, string pkName, string columnName, bool isClustered)
    {
      var createPrimaryKeyTemplate = @"ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY {2} ([{3}])";
      ExecuteNonQuery(string.Format(createPrimaryKeyTemplate, tableName, pkName, isClustered ? "CLUSTERED" : "NONCLUSTERED", columnName));
    }

    private void CreateColumnStoreIndex(string indexName, string tableName, string columnName, bool isClustered)
    {
      var createColumnStoreIndex = "CREATE {0} COLUMNSTORE INDEX [{1}] ON [{2}]";

      ExecuteNonQuery(isClustered ? string.Format(createColumnStoreIndex, "CLUSTERED", indexName, tableName)
        : string.Format(createColumnStoreIndex + " ([{3}]);", "NONCLUSTERED", indexName, tableName, columnName));
    }
#endregion
  }
}