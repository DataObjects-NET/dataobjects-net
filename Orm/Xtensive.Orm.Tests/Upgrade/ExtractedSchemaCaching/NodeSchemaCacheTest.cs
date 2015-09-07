// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.03

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Internals;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Upgrade.Multinode
{
  public class NodeSchemaCacheTest : AutoBuildTest
  {
    [Test]
    public void GetCatalogByNonDefaultNode()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");
      VerifyCatalog(initialCatalog);

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model1");
      nodeConfiguration1.Lock();

      var nodeConfiguration2 = new NodeConfiguration("TestNode2");
      nodeConfiguration2.SchemaMapping.Add("dbo", "Model5");
      nodeConfiguration2.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, nodeConfiguration1);

      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration2);
      Assert.That(catalogs, Is.Not.Null);
      Assert.That(catalogs.Length, Is.EqualTo(1));
      Assert.That(object.ReferenceEquals(initialCatalog, catalogs[0]), Is.False);
      var catalog = catalogs[0];
      Assert.That(catalog.Schemas.Count, Is.EqualTo(nodeConfiguration2.SchemaMapping.Count));
      Assert.That(catalog.Schemas["dbo"], Is.Null);
      Assert.That(catalog.Schemas["Model1"], Is.Null);
      Assert.That(catalog.Schemas["Model2"], Is.Null);
      Assert.That(catalog.Schemas["Model3"], Is.Null);
      Assert.That(catalog.Schemas["Model4"], Is.Null);
      Assert.That(catalog.Schemas["Model5"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model6"], Is.Null);

      Assert.That(catalog.Schemas["Model5"].Tables.Count, Is.EqualTo(3));

      Assert.That(catalog.Schemas["Model5"].Sequences.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetCatalogForNodeWithoutConnectionInfoTest()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");
      VerifyCatalog(initialCatalog);

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model5");
      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, defaultNodeConfiguration);
      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration1);
      Assert.That(catalogs, Is.Not.Null);
      Assert.That(catalogs.Length, Is.EqualTo(1));
      Assert.That(object.ReferenceEquals(initialCatalog, catalogs[0]), Is.False);
      var catalog = catalogs[0];
      Assert.That(catalog.Schemas.Count, Is.EqualTo(1));
      Assert.That(catalog.Schemas["dbo"], Is.Null);
      Assert.That(catalog.Schemas["Model1"], Is.Null);
      Assert.That(catalog.Schemas["Model2"], Is.Null);
      Assert.That(catalog.Schemas["Model3"], Is.Null);
      Assert.That(catalog.Schemas["Model4"], Is.Null);
      Assert.That(catalog.Schemas["Model5"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model6"], Is.Null);

      Assert.That(catalog.Schemas["Model5"].Tables.Count, Is.EqualTo(3));

      Assert.That(catalog.Schemas["Model5"].Sequences.Count, Is.EqualTo(1));

      var bookTable = catalog.Schemas["Model5"].Tables["Book"];
      Assert.That(bookTable, Is.Not.Null);
      Assert.That(bookTable.Columns.Count, Is.EqualTo(3));
      Assert.That(bookTable.TableColumns.Count, Is.EqualTo(3));
      Assert.That(bookTable.Indexes.Count, Is.EqualTo(2));
      Assert.That(bookTable.TableConstraints.Count, Is.EqualTo(2));

      var authorTable = catalog.Schemas["Model5"].Tables["Author"];
      Assert.That(authorTable, Is.Not.Null);
      Assert.That(authorTable.Columns.Count, Is.EqualTo(4));
      Assert.That(authorTable.TableColumns.Count, Is.EqualTo(4));
      Assert.That(authorTable.Indexes.Count, Is.EqualTo(1));
      Assert.That(authorTable.TableConstraints.Count, Is.EqualTo(1));

      var authorsOfBookTable = catalog.Schemas["Model5"].Tables["AuthorsOfBook"];
      Assert.That(authorsOfBookTable, Is.Not.Null);
      Assert.That(authorsOfBookTable.Columns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.TableColumns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.Indexes.Count, Is.EqualTo(0));
      Assert.That(authorsOfBookTable.TableConstraints.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetCatalogForNodeWithSameConnectionInfoTest()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");
      VerifyCatalog(initialCatalog);

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.ConnectionInfo = domainConfiguration.ConnectionInfo;
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model5");
      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, defaultNodeConfiguration);

      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration1);
      Assert.That(catalogs, Is.Not.Null);
      Assert.That(catalogs.Length, Is.EqualTo(1));
      Assert.That(object.ReferenceEquals(initialCatalog, catalogs[0]), Is.False);
      var catalog = catalogs[0];
      Assert.That(catalog.Schemas.Count, Is.EqualTo(1));
      Assert.That(catalog.Schemas["dbo"], Is.Null);
      Assert.That(catalog.Schemas["Model1"], Is.Null);
      Assert.That(catalog.Schemas["Model2"], Is.Null);
      Assert.That(catalog.Schemas["Model3"], Is.Null);
      Assert.That(catalog.Schemas["Model4"], Is.Null);
      Assert.That(catalog.Schemas["Model5"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model6"], Is.Null);

      Assert.That(catalog.Schemas["Model5"].Tables.Count, Is.EqualTo(3));

      Assert.That(catalog.Schemas["Model5"].Sequences.Count, Is.EqualTo(1));

      var bookTable = catalog.Schemas["Model5"].Tables["Book"];
      Assert.That(bookTable, Is.Not.Null);
      Assert.That(bookTable.Columns.Count, Is.EqualTo(3));
      Assert.That(bookTable.TableColumns.Count, Is.EqualTo(3));
      Assert.That(bookTable.Indexes.Count, Is.EqualTo(2));
      Assert.That(bookTable.TableConstraints.Count, Is.EqualTo(2));

      var authorTable = catalog.Schemas["Model5"].Tables["Author"];
      Assert.That(authorTable, Is.Not.Null);
      Assert.That(authorTable.Columns.Count, Is.EqualTo(4));
      Assert.That(authorTable.TableColumns.Count, Is.EqualTo(4));
      Assert.That(authorTable.Indexes.Count, Is.EqualTo(1));
      Assert.That(authorTable.TableConstraints.Count, Is.EqualTo(1));

      var authorsOfBookTable = catalog.Schemas["Model5"].Tables["AuthorsOfBook"];
      Assert.That(authorsOfBookTable, Is.Not.Null);
      Assert.That(authorsOfBookTable.Columns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.TableColumns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.Indexes.Count, Is.EqualTo(0));
      Assert.That(authorsOfBookTable.TableConstraints.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetCatalogWithClonedConnectionInfoTest()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");
      VerifyCatalog(initialCatalog);

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.ConnectionInfo = Cloner.Clone(domainConfiguration.ConnectionInfo);
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model5");
      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, defaultNodeConfiguration);

      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration1);
      Assert.That(catalogs, Is.Not.Null);
      Assert.That(catalogs.Length, Is.EqualTo(1));
      Assert.That(object.ReferenceEquals(initialCatalog, catalogs[0]), Is.False);
      var catalog = catalogs[0];
      Assert.That(catalog.Schemas.Count, Is.EqualTo(1));
      Assert.That(catalog.Schemas["dbo"], Is.Null);
      Assert.That(catalog.Schemas["Model1"], Is.Null);
      Assert.That(catalog.Schemas["Model2"], Is.Null);
      Assert.That(catalog.Schemas["Model3"], Is.Null);
      Assert.That(catalog.Schemas["Model4"], Is.Null);
      Assert.That(catalog.Schemas["Model5"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model6"], Is.Null);

      Assert.That(catalog.Schemas["Model5"].Tables.Count, Is.EqualTo(3));

      Assert.That(catalog.Schemas["Model5"].Sequences.Count, Is.EqualTo(1));

      var bookTable = catalog.Schemas["Model5"].Tables["Book"];
      Assert.That(bookTable, Is.Not.Null);
      Assert.That(bookTable.Columns.Count, Is.EqualTo(3));
      Assert.That(bookTable.TableColumns.Count, Is.EqualTo(3));
      Assert.That(bookTable.Indexes.Count, Is.EqualTo(2));
      Assert.That(bookTable.TableConstraints.Count, Is.EqualTo(2));

      var authorTable = catalog.Schemas["Model5"].Tables["Author"];
      Assert.That(authorTable, Is.Not.Null);
      Assert.That(authorTable.Columns.Count, Is.EqualTo(4));
      Assert.That(authorTable.TableColumns.Count, Is.EqualTo(4));
      Assert.That(authorTable.Indexes.Count, Is.EqualTo(1));
      Assert.That(authorTable.TableConstraints.Count, Is.EqualTo(1));

      var authorsOfBookTable = catalog.Schemas["Model5"].Tables["AuthorsOfBook"];
      Assert.That(authorsOfBookTable, Is.Not.Null);
      Assert.That(authorsOfBookTable.Columns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.TableColumns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.Indexes.Count, Is.EqualTo(0));
      Assert.That(authorsOfBookTable.TableConstraints.Count, Is.EqualTo(3));
    }

    [Test]
    public void GetCatalogForNodeWithDifferentConnectionInfoTest()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");
      VerifyCatalog(initialCatalog);

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.ConnectionInfo =
        new ConnectionInfo(@"sqlserver://dotest:dotest@ALEXEYKULAKOVPC\LOCSERV/DO40-Tests");
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model5");
      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");

      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, defaultNodeConfiguration);

      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration1);
      Assert.That(catalogs, Is.Null);
    }

    [Test]
    public void MultishemaTest()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultSchema = "dbo";
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace).ToSchema("dbo");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace).ToSchema("Model2");

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model4");
      nodeConfiguration1.SchemaMapping.Add("Model2", "Model5");
      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");
      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, defaultNodeConfiguration);

      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration1);
      Assert.That(catalogs, Is.Not.Null);
      Assert.That(catalogs.Length, Is.EqualTo(1));
      Assert.That(object.ReferenceEquals(initialCatalog, catalogs[0]), Is.False);

      var catalog = catalogs[0];
      Assert.That(catalog.Schemas.Count, Is.EqualTo(2));
      Assert.That(catalog.Schemas["dbo"], Is.Null);
      Assert.That(catalog.Schemas["Model1"], Is.Null);
      Assert.That(catalog.Schemas["Model2"], Is.Null);
      Assert.That(catalog.Schemas["Model3"], Is.Null);
      Assert.That(catalog.Schemas["Model4"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model5"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model6"], Is.Null);

      Assert.That(catalog.Schemas["Model4"].Tables.Count, Is.EqualTo(3));
      Assert.That(catalog.Schemas["Model5"].Tables.Count, Is.EqualTo(4));

      Assert.That(catalog.Schemas["Model4"].Sequences.Count, Is.EqualTo(1));
      Assert.That(catalog.Schemas["Model5"].Sequences.Count, Is.EqualTo(0));

      var bookTable = catalog.Schemas["Model4"].Tables["Book"];
      Assert.That(bookTable, Is.Not.Null);
      Assert.That(bookTable.Columns.Count, Is.EqualTo(3));
      Assert.That(bookTable.TableColumns.Count, Is.EqualTo(3));
      Assert.That(bookTable.Indexes.Count, Is.EqualTo(2));
      Assert.That(bookTable.TableConstraints.Count, Is.EqualTo(2));

      var authorTable = catalog.Schemas["Model4"].Tables["Author"];
      Assert.That(authorTable, Is.Not.Null);
      Assert.That(authorTable.Columns.Count, Is.EqualTo(4));
      Assert.That(authorTable.TableColumns.Count, Is.EqualTo(4));
      Assert.That(authorTable.Indexes.Count, Is.EqualTo(1));
      Assert.That(authorTable.TableConstraints.Count, Is.EqualTo(1));

      var authorsOfBookTable = catalog.Schemas["Model4"].Tables["AuthorsOfBook"];
      Assert.That(authorsOfBookTable, Is.Not.Null);
      Assert.That(authorsOfBookTable.Columns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.TableColumns.Count, Is.EqualTo(2));
      Assert.That(authorsOfBookTable.Indexes.Count, Is.EqualTo(0));
      Assert.That(authorsOfBookTable.TableConstraints.Count, Is.EqualTo(3));

      var teacherTable = catalog.Schemas["Model5"].Tables["Teacher"];
      Assert.That(teacherTable, Is.Not.Null);
      Assert.That(teacherTable.Columns.Count, Is.EqualTo(4));
      Assert.That(teacherTable.TableColumns.Count, Is.EqualTo(4));
      Assert.That(teacherTable.Indexes.Count, Is.EqualTo(2));
      Assert.That(teacherTable.TableConstraints.Count, Is.EqualTo(2));

      var subjectTable = catalog.Schemas["Model5"].Tables["Subject"];
      Assert.That(subjectTable, Is.Not.Null);
      Assert.That(subjectTable.Columns.Count, Is.EqualTo(3));
      Assert.That(subjectTable.TableColumns.Count, Is.EqualTo(3));
      Assert.That(subjectTable.Indexes.Count, Is.EqualTo(1));
      Assert.That(subjectTable.TableConstraints.Count, Is.EqualTo(2));

      var degreeTable = catalog.Schemas["Model5"].Tables["Degree"];
      Assert.That(degreeTable, Is.Not.Null);
      Assert.That(degreeTable.Columns.Count, Is.EqualTo(2));
      Assert.That(degreeTable.TableColumns.Count, Is.EqualTo(2));
      Assert.That(degreeTable.Indexes.Count, Is.EqualTo(0));
      Assert.That(degreeTable.TableConstraints.Count, Is.EqualTo(1));

      var subjectsPerTeacherTable = catalog.Schemas["Model5"].Tables["SubjectsPerTeacher"];
      Assert.That(subjectsPerTeacherTable, Is.Not.Null);
      Assert.That(subjectsPerTeacherTable.Columns.Count, Is.EqualTo(2));
      Assert.That(subjectsPerTeacherTable.TableColumns.Count, Is.EqualTo(2));
      Assert.That(subjectsPerTeacherTable.Indexes.Count, Is.EqualTo(0));
      Assert.That(subjectsPerTeacherTable.TableConstraints.Count, Is.EqualTo(3));
    }

    [Test]
    public void MultidatabaseTest()
    {
      var initialCatalog = BuildCatalog("DO40-Tests");
      var anotherInitialCatalog = BuildCatalog("DO-Tests-1");

      var domainConfiguration = base.BuildConfiguration();
      domainConfiguration.DefaultDatabase = "DO40-Tests";
      domainConfiguration.DefaultSchema = "dbo";
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace).To("DO40-Tests", "dbo");
      domainConfiguration.MappingRules.Map(GetType().Assembly, GetType().Namespace).To("DO-Tests-1", "Model2");

      Assert.That(domainConfiguration.ConnectionInfo, Is.Not.Null);
      Assert.That(domainConfiguration.ConnectionInfo.ConnectionUrl, Is.Not.Null);
      var catalogName = domainConfiguration.ConnectionInfo.ConnectionUrl.Resource;
      Assert.That(string.IsNullOrEmpty(catalogName), Is.False);

      var defaultNodeConfiguration = new NodeConfiguration(WellKnown.DefaultNodeId);
      defaultNodeConfiguration.Lock();

      var nodeConfiguration1 = new NodeConfiguration("TestNode1");
      nodeConfiguration1.DatabaseMapping.Add("DO40-Tests", "DO40-Tests");
      nodeConfiguration1.DatabaseMapping.Add("DO-Tests-1", "DO-Tests-1");
      nodeConfiguration1.SchemaMapping.Add("dbo", "Model4");
      nodeConfiguration1.SchemaMapping.Add("Model2", "Model5");
      nodeConfiguration1.Lock();

      var defaultSchemaInfo = new DefaultSchemaInfo(catalogName, "dbo");
      var catalogCache = new NodeSchemaCache(domainConfiguration, defaultSchemaInfo);
      catalogCache.Add(initialCatalog, defaultNodeConfiguration);
      catalogCache.Add(anotherInitialCatalog, defaultNodeConfiguration);

      var catalogs = catalogCache.GetNodeSchema(nodeConfiguration1);
    }

    private Catalog BuildCatalog(string catalogName, int countOfFullSchemes = 1)
    {
      var catalog = new Catalog(catalogName);

      #region dbo

      var schema = catalog.CreateSchema("dbo");
      var table = schema.CreateTable("Book");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("ISBN", new SqlValueType(SqlType.VarChar, 100));
      table.CreateColumn("Title", new SqlValueType(SqlType.VarChar, 250));
      table.CreatePrimaryKey("PK_Book", table.TableColumns["Id"]);
      var index = table.CreateIndex("IX_ISBN");
      index.IsUnique = true;
      index.CreateIndexColumn(table.TableColumns["ISBN"]);

      index = table.CreateIndex("IX_Title");
      index.IsUnique = false;
      index.CreateIndexColumn(table.TableColumns["Title"]);

      table.CreateUniqueConstraint("ISBN_UniqueConstraint", table.TableColumns["ISBN"]);

      table = schema.CreateTable("Author");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("FirstName", new SqlValueType(SqlType.VarChar, 255));
      table.CreateColumn("LastName", new SqlValueType(SqlType.VarChar, 255));
      table.CreateColumn("Patronymic", new SqlValueType(SqlType.VarChar, 255));
      table.CreatePrimaryKey("PK_Author", table.TableColumns["Id"]);

      index = table.CreateIndex("IX_FirstName");
      index.CreateIndexColumn(table.TableColumns["FirstName"]);

      table = schema.CreateTable("AuthorsOfBook");
      table.CreateColumn("Book.Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Author.Id", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_AuthorsOfBook", table.TableColumns["Book.Id"], table.TableColumns["Author.Id"]);
      var foreignKey = table.CreateForeignKey("FK_Author_Id");
      foreignKey.Columns.Add(table.TableColumns["Author.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Author"].TableColumns["Id"]);

      foreignKey = table.CreateForeignKey("FK_Book_Id");
      foreignKey.Columns.Add(table.TableColumns["Book.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Book"].TableColumns["Id"]);

      var sequence = schema.CreateSequence("Int32-Generator");
      sequence.SequenceDescriptor.IsCyclic = false;
      sequence.SequenceDescriptor.Increment = 1;
      sequence.SequenceDescriptor.StartValue = 1;
      sequence.SequenceDescriptor.MinValue = 1;
      sequence.SequenceDescriptor.MaxValue = (int)Int32.MaxValue/2;

      #endregion

      #region Model1

      schema = catalog.CreateSchema("Model1");
      table = schema.CreateTable("Book");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("ISBN", new SqlValueType(SqlType.VarChar, 100));
      table.CreateColumn("Title", new SqlValueType(SqlType.VarChar, 250));
      table.CreatePrimaryKey("PK_Book", table.TableColumns["Id"]);
      index = table.CreateIndex("IX_ISBN");
      index.IsUnique = true;
      index.CreateIndexColumn(table.TableColumns["ISBN"]);

      index = table.CreateIndex("IX_Title");
      index.IsUnique = false;
      index.CreateIndexColumn(table.TableColumns["Title"]);

      table.CreateUniqueConstraint("ISBN_UniqueConstraint", table.TableColumns["ISBN"]);

      table = schema.CreateTable("Author");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("FirstName", new SqlValueType(SqlType.VarChar, 255));
      table.CreateColumn("LastName", new SqlValueType(SqlType.VarChar, 255));
      table.CreateColumn("Patronymic", new SqlValueType(SqlType.VarChar, 255));
      table.CreatePrimaryKey("PK_Author", table.TableColumns["Id"]);

      index = table.CreateIndex("IX_FirstName");
      index.CreateIndexColumn(table.TableColumns["FirstName"]);

      table = schema.CreateTable("AuthorsOfBook");
      table.CreateColumn("Book.Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Author.Id", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_AuthorsOfBook", table.TableColumns["Book.Id"], table.TableColumns["Author.Id"]);
      foreignKey = table.CreateForeignKey("FK_Author_Id");
      foreignKey.Columns.Add(table.TableColumns["Author.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Author"].TableColumns["Id"]);

      foreignKey = table.CreateForeignKey("FK_Book_Id");
      foreignKey.Columns.Add(table.TableColumns["Book.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Book"].TableColumns["Id"]);

      sequence = schema.CreateSequence("Int32-Generator");
      sequence.SequenceDescriptor.IsCyclic = false;
      sequence.SequenceDescriptor.Increment = 1;
      sequence.SequenceDescriptor.StartValue = 1;
      sequence.SequenceDescriptor.MinValue = 1;
      sequence.SequenceDescriptor.MaxValue = (int)Int32.MaxValue/2;

      #endregion

      #region Model2

      schema = catalog.CreateSchema("Model2");
      table = schema.CreateTable("Subject");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 255));
      table.CreateColumn("Hours", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_Subject", table.TableColumns["Id"]);
      table.CreateUniqueConstraint("UX_Subject_Name", table.TableColumns["Name"]);
      index = table.CreateIndex("IX_Subject_Name");
      index.CreateIndexColumn(table.TableColumns["Name"]);
      index.IsUnique = true;

      table = schema.CreateTable("Degree");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 150));
      table.CreatePrimaryKey("PK_Degree", table.TableColumns["Id"]);

      table = schema.CreateTable("Teacher");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("FirstName", new SqlValueType(SqlType.VarChar, 150));
      table.CreateColumn("LastName", new SqlValueType(SqlType.VarChar, 150));
      table.CreateColumn("Degree", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_Teacher", table.TableColumns["Id"]);
      index = table.CreateIndex("IX_FirstName");
      index.CreateIndexColumn(table.TableColumns["FirstName"]);
      index = table.CreateIndex("IX_LastName");
      index.CreateIndexColumn(table.TableColumns["LastName"]);
      foreignKey = table.CreateForeignKey("FK_Teacher_Degree_Degree");
      foreignKey.Columns.Add(table.TableColumns["Degree"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Degree"].TableColumns["Id"]);

      table = schema.CreateTable("SubjectsPerTeacher");
      table.CreateColumn("Teacher.Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Subject.Id", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_SubjectsPerTeacher", table.TableColumns["Teacher.Id"], table.TableColumns["Subject.Id"]);
      foreignKey = table.CreateForeignKey("FK_Teacher_Id");
      foreignKey.Columns.Add(table.TableColumns["Teacher.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Teacher"].TableColumns["Id"]);
      foreignKey = table.CreateForeignKey("FK_Subject_Id");
      foreignKey.Columns.Add(table.TableColumns["Subject.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Subject"].TableColumns["Id"]);

      #endregion

      #region Model3

      schema = catalog.CreateSchema("Model3");
      table = schema.CreateTable("Invoice");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Number", new SqlValueType(SqlType.VarChar, 32));
      table.CreateColumn("CreationDate", new SqlValueType(SqlType.DateTime));
      table.CreatePrimaryKey("PK_Invoice", table.TableColumns["Id"]);

      table = schema.CreateTable("Product");
      table.CreateColumn("Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Name", new SqlValueType(SqlType.VarChar, 250));
      table.CreatePrimaryKey("PK_Product", table.TableColumns["Id"]);

      table = schema.CreateTable("InvoiceItems");
      table.CreateColumn("Invoice.Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Product.Id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("Count", new SqlValueType(SqlType.Int32));
      table.CreatePrimaryKey("PK_InvoiceItems", table.TableColumns["Invoice.Id"], table.TableColumns["Product.Id"]);
      foreignKey = table.CreateForeignKey("FK_Invoice_Id");
      foreignKey.Columns.Add(table.TableColumns["Invoice.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Invoice"].TableColumns["Id"]);

      foreignKey = table.CreateForeignKey("FK_Product_Id");
      foreignKey.Columns.Add(table.TableColumns["Product.Id"]);
      foreignKey.ReferencedColumns.Add(schema.Tables["Product"].TableColumns["Id"]);

      #endregion

      catalog.CreateSchema("Model4");
      catalog.CreateSchema("Model5");
      catalog.CreateSchema("Model6");

      return catalog;
    }

    private void VerifyCatalog(Catalog catalog)
    {
      Assert.That(catalog.Schemas.Count, Is.EqualTo(7));
      Assert.That(catalog.Schemas["dbo"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model1"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model2"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model3"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model4"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model5"], Is.Not.Null);
      Assert.That(catalog.Schemas["Model6"], Is.Not.Null);

      Assert.That(catalog.Schemas["dbo"].Tables.Count, Is.EqualTo(3));
      Assert.That(catalog.Schemas["Model1"].Tables.Count, Is.EqualTo(3));
      Assert.That(catalog.Schemas["Model2"].Tables.Count, Is.EqualTo(4));
      Assert.That(catalog.Schemas["Model3"].Tables.Count, Is.EqualTo(3));
      Assert.That(catalog.Schemas["Model4"].Tables.Count, Is.EqualTo(0));
      Assert.That(catalog.Schemas["Model5"].Tables.Count, Is.EqualTo(0));
      Assert.That(catalog.Schemas["Model6"].Tables.Count, Is.EqualTo(0));

      Assert.That(catalog.Schemas["dbo"].Sequences.Count, Is.EqualTo(1));
      Assert.That(catalog.Schemas["Model1"].Sequences.Count, Is.EqualTo(1));
      Assert.That(catalog.Schemas["Model2"].Sequences.Count, Is.EqualTo(0));
      Assert.That(catalog.Schemas["Model3"].Sequences.Count, Is.EqualTo(0));
      Assert.That(catalog.Schemas["Model4"].Sequences.Count, Is.EqualTo(0));
      Assert.That(catalog.Schemas["Model5"].Sequences.Count, Is.EqualTo(0));
      Assert.That(catalog.Schemas["Model6"].Sequences.Count, Is.EqualTo(0));
    }
  }
}