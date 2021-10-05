// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.16

// TODO: Refactor stupid MSSqlExtractorTests.cs and put all stuff here

using System;
using System.Collections.Generic;
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
    private List<string> dropOperations = new List<string>();

    protected override void TestFixtureTearDown()
    {
      foreach (var dropOperation in dropOperations) {
        try {
          ExecuteNonQuery(dropOperation);
        }
        catch (Exception) {
          Console.Write("Operation '{0}' wasn't performed correctly", dropOperation);
        }
      }
    }

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
    public void ExtractIndexTypesTest1()
    {
      #region Preparation

      var createScript = @"
        CREATE TABLE [Table1] (
          Id int NOT NULL,
          IntField1 int NOT NULL,
          XmlField [xml],
          CONSTRAINT [PK_Table1] PRIMARY KEY CLUSTERED (Id));

        CREATE PRIMARY XML INDEX [IX_XML_Table1_XmlField] ON [Table1] (XmlField);
        CREATE NONCLUSTERED INDEX [IX_Table1_IntField1] ON [Table1] (IntField1);";

      var dropScript = "IF OBJECT_ID('Table1') IS NOT NULL DROP TABLE [Table1]";

      RegisterDropForLater(dropScript);
      ExecuteNonQuery(dropScript);
      ExecuteNonQuery(createScript);

      #endregion

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table = schema.Tables["Table1"];
      var tableIndexes = table.Indexes;
      Assert.IsTrue(table.Indexes.Count==1);

      var nonClusteredIndex = tableIndexes["IX_Table1_IntField1"];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsFullText);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var tableConstratins = table.TableConstraints;
      Assert.IsTrue(table.TableConstraints.Count==1);

      var clusteredPk = tableConstratins["PK_Table1"] as PrimaryKey;
      Assert.IsNotNull(clusteredPk);
      Assert.IsTrue(clusteredPk.IsClustered);
    }

    [Test]
    public void ExtractIndexTypesTest2()
    {
      Require.ProviderVersionAtLeast(new Version(10, 0));

      #region Preparation

      var createScript = @"
        CREATE TABLE [Table1] (
          Id int NOT NULL,
          IntField1 int NOT NULL,
          XmlField xml,
          GeometryField [geometry],
          CONSTRAINT [PK_Table1] PRIMARY KEY CLUSTERED (Id));

        CREATE PRIMARY XML INDEX [IX_XML_Table1_XmlField] ON [Table1] (XmlField);
        CREATE NONCLUSTERED INDEX [IX_Table1_IntField1] ON [Table1] (IntField1);
        CREATE SPATIAL INDEX [IXS_Table1_GeometryField] ON [Table1] (GeometryField) WITH (BOUNDING_BOX = ( 0, 0, 500, 200 ));";

      var dropScript = @"IF OBJECT_ID('Table1') IS NOT NULL DROP TABLE [Table1]";

      RegisterDropForLater(dropScript);
      ExecuteNonQuery(dropScript);
      ExecuteNonQuery(createScript);

      #endregion

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      Assert.That(schema, Is.Not.Null);

      var table = schema.Tables["Table1"];
      var tableIndexes = table.Indexes;
      Assert.IsTrue(table.Indexes.Count==2);

      var nonClusteredIndex = tableIndexes["IX_Table1_IntField1"];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsFullText);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var spatialIndex = tableIndexes["IXS_Table1_GeometryField"];
      Assert.IsNotNull(spatialIndex);
      Assert.IsTrue(spatialIndex.IsSpatial);
      Assert.IsFalse(spatialIndex.IsClustered);
      Assert.IsFalse(spatialIndex.IsFullText);
      Assert.IsFalse(spatialIndex.IsBitmap);
      Assert.IsFalse(spatialIndex.IsFullText);

      var tableConstratins = table.TableConstraints;
      Assert.IsTrue(table.TableConstraints.Count==1);

      var clusteredPk = tableConstratins["PK_Table1"] as PrimaryKey;
      Assert.IsNotNull(clusteredPk);
      Assert.IsTrue(clusteredPk.IsClustered);
    }

    [Test]
    public void ExtractIndexTypesTest3()
    {
      Require.ProviderVersionAtLeast(new Version(11, 0));

      #region Preparation

      var createScript = @"
        CREATE TABLE Table1 (
          [Id] uniqueidentifier NOT NULL,
          [A] integer NOT NULL,
          [B] integer NOT NULL,
          [C] integer NOT NULL,
          CONSTRAINT [PK_Table1] PRIMARY KEY NONCLUSTERED (Id));

        CREATE NONCLUSTERED INDEX [IX_Table1_A] ON [Table1] (A);
        CREATE NONCLUSTERED COLUMNSTORE INDEX [IXCS_Table1_C] ON [Table1] (C);

        CREATE TABLE Table2 (
          [id] integer NOT NULL,
          [IntField1] integer NOT NULL,
          [IntField2] integer NOT NULL,
          [IntField3] integer NOT NULL,
          [IntField4] integer NOT NULL,
          [IntField5] integer NOT NULL);

        CREATE NONCLUSTERED COLUMNSTORE INDEX [IXCS_Table2_IntField5] ON [Table2] (IntField5);
        CREATE NONCLUSTERED INDEX [IX_Table2_IntField2] ON [Table2] (IntField2);
        ALTER TABLE [Table2] ADD CONSTRAINT [PK_Table2] PRIMARY KEY NONCLUSTERED ([Id]);

        CREATE CLUSTERED INDEX [IX_Table1_B] ON [Table1] (B);";

      var dropScript = @"
        IF OBJECT_ID('Table1') IS NOT NULL DROP TABLE [Table1];
        IF OBJECT_ID('Table2') IS NOT NULL DROP TABLE [Table2];";

      RegisterDropForLater(dropScript);
      ExecuteNonQuery(dropScript);
      ExecuteNonQuery(createScript);

      #endregion

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table1 = schema.Tables["Table1"];
      var table1Indexes  = table1.Indexes;
      Assert.IsTrue(table1Indexes.Count==2);

      var table1NonClusteredIndex = table1Indexes["IX_Table1_A"];
      Assert.IsNotNull(table1NonClusteredIndex);
      Assert.IsFalse(table1NonClusteredIndex.IsClustered);
      Assert.IsFalse(table1NonClusteredIndex.IsSpatial);
      Assert.IsFalse(table1NonClusteredIndex.IsBitmap);
      Assert.IsFalse(table1NonClusteredIndex.IsFullText);

      var table1ClusteredIndex = table1Indexes["IX_Table1_B"];
      Assert.IsNotNull(table1ClusteredIndex);
      Assert.IsTrue(table1ClusteredIndex.IsClustered);
      Assert.IsFalse(table1ClusteredIndex.IsSpatial);
      Assert.IsFalse(table1ClusteredIndex.IsBitmap);
      Assert.IsFalse(table1ClusteredIndex.IsFullText);

      var table1Constratins = table1.TableConstraints;
      Assert.IsTrue(table1Constratins.Count==1);
      var table1NonClusteredPk = table1Constratins["PK_Table1"] as PrimaryKey;
      Assert.IsNotNull(table1NonClusteredPk);
      Assert.IsFalse(table1NonClusteredPk.IsClustered);


      var table2 = schema.Tables["Table2"];
      var indexes = table2.Indexes;
      Assert.IsTrue(indexes.Count==1);

      var nonClusteredIndex = indexes["IX_Table2_IntField2"];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var table2Constraints = table2.TableConstraints;
      Assert.IsTrue(table2Constraints.Count==1);

      var nonClusteredPk = table2Constraints["PK_Table2"] as PrimaryKey;
      Assert.IsNotNull(nonClusteredPk);
      Assert.IsFalse(nonClusteredPk.IsClustered);
    }

    [Test]
    public void ExtractIndexTypesTest4()
    {
      Require.ProviderVersionAtLeast(new Version(12, 0));

      #region Preparation

      var createScript = @"
        CREATE TABLE [Table1] (
          [Id] uniqueidentifier NOT NULL,
          [A] integer NOT NULL,
          CONSTRAINT [PK_Table1] PRIMARY KEY NONCLUSTERED (Id));

        CREATE NONCLUSTERED INDEX [IX_Table1_A] ON [Table1] (A);

        CREATE TABLE [Table2] (
          [Id] integer NOT NULL,
          [IntField1] integer NOT NULL,
          [IntField2] integer NOT NULL,
          [IntField3] integer NOT NULL,
          [IntField4] integer NOT NULL);

        CREATE CLUSTERED COLUMNSTORE INDEX [IXSC_Table2_IntField1] ON [Table2];";

      var dropScript = @"
        IF OBJECT_ID('Table1') IS NOT NULL DROP TABLE [Table1];
        IF OBJECT_ID('Table2') IS NOT NULL DROP TABLE [Table2];";

      RegisterDropForLater(dropScript);
      ExecuteNonQuery(dropScript);
      ExecuteNonQuery(createScript);

      #endregion

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table1 = schema.Tables["Table1"];
      var table1Indexes = table1.Indexes;
      Assert.IsTrue(table1Indexes.Count==1);

      var table1NonClusteredIndex = table1Indexes["IX_Table1_A"];
      Assert.IsNotNull(table1NonClusteredIndex);
      Assert.IsFalse(table1NonClusteredIndex.IsClustered);
      Assert.IsFalse(table1NonClusteredIndex.IsSpatial);
      Assert.IsFalse(table1NonClusteredIndex.IsBitmap);
      Assert.IsFalse(table1NonClusteredIndex.IsFullText);

      var table1Constratins = table1.TableConstraints;
      Assert.IsTrue(table1Constratins.Count==1);

      var table1NonClusteredPk = table1Constratins["PK_Table1"] as PrimaryKey;
      Assert.IsNotNull(table1NonClusteredPk);
      Assert.IsFalse(table1NonClusteredPk.IsClustered);

      var table2 = schema.Tables["Table2"];
      var indexes = table2.Indexes;
      Assert.IsTrue(indexes.Count==0);

      var tableConstratins = table2.TableConstraints;
      Assert.IsTrue(tableConstratins.Count==0);
    }

    [Test]
    public void ExtractIndexTypesTest5()
    {
      Require.ProviderVersionAtLeast(new Version(13, 0));

      #region Preparation

      var createScript = @"
        CREATE TABLE [Table1] (
          [Id] uniqueidentifier NOT NULL,
          [A] integer NOT NULL,
          CONSTRAINT [PK_Table1] PRIMARY KEY NONCLUSTERED (Id));

        CREATE NONCLUSTERED INDEX [IX_Table1_A] ON [Table1] (A);

        CREATE TABLE [Table2] (
          [Id] integer NOT NULL,
          [IntField1] integer NOT NULL,
          [IntField2] integer NOT NULL,
          [IntField3] integer NOT NULL,
          [IntField4] integer NOT NULL,
          CONSTRAINT [PK_Table2] PRIMARY KEY NONCLUSTERED (Id));

        CREATE NONCLUSTERED INDEX [IX_Table2_IntField1] ON [Table2] (IntField2);
        CREATE CLUSTERED COLUMNSTORE INDEX [IXSC_Table2_IntField1] ON [Table2];";

      var dropScript = @"
        IF OBJECT_ID('Table1') IS NOT NULL DROP TABLE [Table1];
        IF OBJECT_ID('Table2') IS NOT NULL DROP TABLE [Table2];";

      RegisterDropForLater(dropScript);
      ExecuteNonQuery(dropScript);
      ExecuteNonQuery(createScript);

      #endregion

      Schema schema = null;
      Assert.DoesNotThrow(() => { schema = ExtractCatalog().DefaultSchema; });

      var table1 = schema.Tables["Table1"];
      var table1Indexes = table1.Indexes;
      Assert.IsTrue(table1Indexes.Count==1);

      var table1NonClusteredIndex = table1Indexes["IX_Table1_A"];
      Assert.IsNotNull(table1NonClusteredIndex);
      Assert.IsFalse(table1NonClusteredIndex.IsClustered);
      Assert.IsFalse(table1NonClusteredIndex.IsSpatial);
      Assert.IsFalse(table1NonClusteredIndex.IsBitmap);
      Assert.IsFalse(table1NonClusteredIndex.IsFullText);

      var table1Constratins = table1.TableConstraints;
      Assert.IsTrue(table1Constratins.Count==1);

      var table1NonClusteredPk = table1Constratins["PK_Table1"] as PrimaryKey;
      Assert.IsNotNull(table1NonClusteredPk);
      Assert.IsFalse(table1NonClusteredPk.IsClustered);

      var table2 = schema.Tables["Table2"];
      var indexes = table2.Indexes;
      Assert.IsTrue(indexes.Count==1);

      var nonClusteredIndex = indexes["IX_Table2_IntField1"];
      Assert.IsNotNull(nonClusteredIndex);
      Assert.IsFalse(nonClusteredIndex.IsClustered);
      Assert.IsFalse(nonClusteredIndex.IsSpatial);
      Assert.IsFalse(nonClusteredIndex.IsBitmap);
      Assert.IsFalse(nonClusteredIndex.IsFullText);

      var tableConstratins = table2.TableConstraints;
      Assert.IsTrue(tableConstratins.Count==1);

      var nonClusteredPk = tableConstratins["PK_Table2"] as PrimaryKey;
      Assert.IsNotNull(nonClusteredPk);
      Assert.IsFalse(nonClusteredPk.IsClustered);
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

    private void RegisterDropForLater(string dropScript)
    {
      dropOperations.Add(dropScript);
    }

#endregion
  }
}