// Copyright (C) 2017-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2017.03.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  public class MakeNamesUnreadableTest : SqlTest
  {
    private const string TableName = "DenyNamesReadingTest";
    private const string CatalogName = "DO-Tests";
    private const string SchemaName = "dbo";
    private const string DummySchemaName = "DummySchema";
    private const string DummyDatabaseName = "DummyDatabase";

    private readonly Dictionary<string, string> emptyMap = new(0);

    [Test]
    public void ReadingNamesTest()
    {
      var defaultSchema = GetSchema();
      Catalog catalog = null;
      Assert.DoesNotThrow(() => catalog = defaultSchema.Catalog);
      Table table = null;
      Assert.DoesNotThrow(() => table = defaultSchema.Tables[TableName]);

      Assert.DoesNotThrow(() => {var catalogName = catalog.Name;});
      Assert.DoesNotThrow(() => {var catalogDbName = catalog.DbName;});

      Assert.DoesNotThrow(() => {var catalogName = catalog.GetNameInternal();});
      Assert.DoesNotThrow(() => {var catalogDbName = catalog.GetDbNameInternal();});

      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.Name; });
      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.DbName;});

      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.GetNameInternal();});
      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.GetDbNameInternal();});

      Assert.DoesNotThrow(() => {var schemaName = table.Name;});
      Assert.DoesNotThrow(() => {var schemaName = table.DbName;});

      catalog.MakeNamesUnreadable();

      _ = Assert.Throws<InvalidOperationException>(() => { var catalogName = catalog.Name; });
      _ = Assert.Throws<InvalidOperationException>(() => { var catalogDbName = catalog.DbName; });

      Assert.DoesNotThrow(() => {var catalogName = catalog.GetNameInternal();});
      Assert.DoesNotThrow(() => {var catalogDbName = catalog.GetDbNameInternal();});

      _ = Assert.Throws<InvalidOperationException>(() => {var schemaName = defaultSchema.Name;});
      _ = Assert.Throws<InvalidOperationException>(() => {var schemaName = defaultSchema.DbName;});

      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.GetNameInternal();});
      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.GetDbNameInternal();});

      Assert.DoesNotThrow(() => {var schemaName = table.Name; });
      Assert.DoesNotThrow(() => {var schemaName = table.DbName; });
    }

    [Test]
    public void ChangingNamesTest()
    {
      var defaultSchema = GetSchema();
      Catalog catalog = null;

      Assert.DoesNotThrow(() => catalog = defaultSchema.Catalog);
      Table table = null;
      Assert.DoesNotThrow(() => table = defaultSchema.Tables[TableName]);

      Assert.DoesNotThrow(() => {var catalogName = catalog.Name; });
      Assert.DoesNotThrow(() => {var catalogDbName = catalog.DbName; });

      Assert.DoesNotThrow(() => {var catalogName = catalog.GetNameInternal(); });
      Assert.DoesNotThrow(() => {var catalogDbName = catalog.GetDbNameInternal(); });

      var oldCatalogName = catalog.Name;
      var oldCatalogDbName = catalog.DbName;

      var newCatalogName = oldCatalogName + "New";
      var newCatalogDbName = oldCatalogDbName + "New";

      Assert.DoesNotThrow(() => catalog.Name = newCatalogName);
      Assert.DoesNotThrow(() => catalog.DbName = newCatalogDbName);

      catalog.Name = oldCatalogName;
      catalog.DbName = oldCatalogDbName;

      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.Name;});
      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.DbName;});

      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.GetNameInternal();});
      Assert.DoesNotThrow(() => {var schemaName = defaultSchema.GetDbNameInternal();});

      var oldSchemaName = defaultSchema.Name;
      var oldSchemaDbName = defaultSchema.DbName;

      var newSchemaName = oldSchemaName + "New";
      var newSchemaDbName = oldSchemaDbName + "New";

      Assert.DoesNotThrow(() => defaultSchema.Name = newSchemaName);
      Assert.DoesNotThrow(() => defaultSchema.DbName = newSchemaDbName);

      defaultSchema.Name = oldSchemaName;
      defaultSchema.DbName = oldSchemaDbName;

      Assert.DoesNotThrow(() => {var schemaName = table.Name;});
      Assert.DoesNotThrow(() => {var schemaName = table.DbName;});

      catalog.MakeNamesUnreadable();

      _ = Assert.Throws<InvalidOperationException>(() => catalog.Name = newCatalogName);
      _ = Assert.Throws<InvalidOperationException>(() => catalog.DbName = newCatalogDbName);

      _ = Assert.Throws<InvalidOperationException>(() => defaultSchema.Name = newSchemaName);
      _ = Assert.Throws<InvalidOperationException>(() => defaultSchema.DbName = newSchemaDbName);
    }

    [Test]
    public void TableCreationTest()
    {
      var defaultSchema = GetSchema();

      var table = defaultSchema.CreateTable(string.Format("Crt1_{0}", TableName));
      var column = table.CreateColumn("Id", GetServerTypeFor(typeof (int)));
      _ = table.CreatePrimaryKey("PK_Crt_DenyNamesReadingTest", column);
      _ = table.CreateColumn("CreationDate", GetServerTypeFor(typeof (DateTime)));
      var createTableQuery = SqlDdl.Create(table);

      TestQueryNamesReadable(createTableQuery, defaultSchema);
    }

    [Test]
    public void TableCreationUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var table = defaultSchema.CreateTable(string.Format("Crt1_{0}", TableName));
      var column = table.CreateColumn("Id", GetServerTypeFor(typeof (int)));
      _ = table.CreatePrimaryKey("PK_Crt_DenyNamesReadingTest", column);
      _ = table.CreateColumn("CreationDate", GetServerTypeFor(typeof (DateTime)));
      var createTableQuery = SqlDdl.Create(table);

      TestQueryNamesUnreadable(createTableQuery, defaultSchema);
    }

    [Test]
    public void TableAlterTest()
    {
      var defaultSchema = GetSchema();

      var table = defaultSchema.Tables[TableName];
      var column = table.CreateColumn("Text", GetServerTypeFor(typeof (string), 255));
      column.IsNullable = true;
      var alterTableQuery = SqlDdl.Alter(table, SqlDdl.AddColumn(column));

      TestQueryNamesReadable(alterTableQuery, defaultSchema);
    }

    [Test]
    public void TableAlterUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var table = defaultSchema.Tables[TableName];
      var column = table.CreateColumn("Text", GetServerTypeFor(typeof (string), 255));
      column.IsNullable = true;
      var alterTableQuery = SqlDdl.Alter(table, SqlDdl.AddColumn(column));

      TestQueryNamesUnreadable(alterTableQuery, defaultSchema);
    }

    [Test]
    public void TableDeletionTest()
    {
      var defaultSchema = GetSchema();

      var table = defaultSchema.Tables[TableName];
      var dropTableQuery = SqlDdl.Drop(table);

      TestQueryNamesReadable(dropTableQuery, defaultSchema);
    }

    [Test]
    public void TableDeletionUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var table = defaultSchema.Tables[TableName];
      var dropTableQuery = SqlDdl.Drop(table);

      TestQueryNamesUnreadable(dropTableQuery, defaultSchema);
    }

    [Test]
    public void RowInsertionTest()
    {
      var defaultSchema = GetSchema();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var insertQuery = SqlDml.Insert(tableRef);
      insertQuery.Values.Add(tableRef["Id"], 1);
      insertQuery.Values.Add(tableRef["CreationDate"], DateTime.UtcNow);

      TestQueryNamesReadable(insertQuery, defaultSchema);
    }

    [Test]
    public void RowInsertionUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var insertQuery = SqlDml.Insert(tableRef);
      insertQuery.Values.Add(tableRef["Id"], 1);
      insertQuery.Values.Add(tableRef["CreationDate"], DateTime.UtcNow);

      TestQueryNamesUnreadable(insertQuery, defaultSchema);
    }

    [Test]
    public void RowUpdateTest()
    {
      var defaultSchema = GetSchema();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var updateQuery = SqlDml.Update(tableRef);
      updateQuery.Values.Add(tableRef["CreationDate"], DateTime.UtcNow);
      updateQuery.Where = tableRef["Id"]==1;

      TestQueryNamesReadable(updateQuery, defaultSchema);
    }

    [Test]
    public void RowUpdateUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var updateQuery = SqlDml.Update(tableRef);
      updateQuery.Values.Add(tableRef["CreationDate"], DateTime.UtcNow);
      updateQuery.Where = tableRef["Id"]==1;

      TestQueryNamesUnreadable(updateQuery, defaultSchema);
    }

    [Test]
    public void RowDeletionTest()
    {
      var defaultSchema = GetSchema();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var deleteQuery = SqlDml.Delete(tableRef);
      deleteQuery.Where = tableRef["Id"]==1;

      TestQueryNamesReadable(deleteQuery, defaultSchema);
    }

    [Test]
    public void RowDeletionUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var deleteQuery = SqlDml.Delete(tableRef);
      deleteQuery.Where = tableRef["Id"]==1;

      TestQueryNamesUnreadable(deleteQuery, defaultSchema);
    }

    [Test]
    public void RowSelectionTest()
    {
      var defaultSchema = GetSchema();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var selectQuery = SqlDml.Select(tableRef);
      selectQuery.Where = tableRef["CreationDate"] < DateTime.UtcNow;

      TestQueryNamesReadable(selectQuery, defaultSchema);
    }

    [Test]
    public void RowSelectionUnreadableNamesTest()
    {
      var defaultSchema = GetSchema();
      defaultSchema.Catalog.MakeNamesUnreadable();

      var tableRef = SqlDml.TableRef(defaultSchema.Tables[TableName]);
      var selectQuery = SqlDml.Select(tableRef);
      selectQuery.Where = tableRef["CreationDate"] < DateTime.UtcNow;

      TestQueryNamesUnreadable(selectQuery, defaultSchema);
    }

    private void TestQueryNamesReadable(ISqlCompileUnit query, Schema defaultSchema)
    {
      var queryText = string.Empty;

      if (IsMultidatabaseSupported) {
        var compilerConfiguration = new SqlCompilerConfiguration { DatabaseQualifiedObjects = true };
        var postCompilerConfiguration = new SqlPostCompilerConfiguration(emptyMap, emptyMap);

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query, compilerConfiguration).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.True);

        compilerConfiguration = new SqlCompilerConfiguration { DatabaseQualifiedObjects = true };
        postCompilerConfiguration = new SqlPostCompilerConfiguration(GetDatabaseMap(defaultSchema.Catalog), GetSchemaMap(defaultSchema));

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query, compilerConfiguration).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(DummyDatabaseName), Is.False);
        Assert.That(queryText.Contains(DummySchemaName), Is.False);
      }
      if (IsMultischemaSupported) {
        var postCompilerConfiguration = new SqlPostCompilerConfiguration(emptyMap, emptyMap);

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);

        postCompilerConfiguration = new SqlPostCompilerConfiguration(GetDatabaseMap(defaultSchema.Catalog), GetSchemaMap(defaultSchema));

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(DummyDatabaseName), Is.False);
        Assert.That(queryText.Contains(DummySchemaName), Is.False);
      }
      else {
        var postCompilerConfiguration = new SqlPostCompilerConfiguration(emptyMap, emptyMap);

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);

        postCompilerConfiguration = new SqlPostCompilerConfiguration(GetDatabaseMap(defaultSchema.Catalog), GetSchemaMap(defaultSchema));

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(DummyDatabaseName), Is.False);
        Assert.That(queryText.Contains(DummySchemaName), Is.False);
      }
    }

    private void TestQueryNamesUnreadable(ISqlCompileUnit query, Schema defaultSchema)
    {
      var queryText = string.Empty;

      if (IsMultidatabaseSupported) {
        var compilerConfiguration = new SqlCompilerConfiguration() { DatabaseQualifiedObjects = true };

        _ = Assert.Throws<InvalidOperationException>(() => queryText = Driver.Compile(query, compilerConfiguration).GetCommandText());

        var postCompilerConfiguration = new SqlPostCompilerConfiguration(emptyMap, emptyMap);

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query, compilerConfiguration).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.True);

        postCompilerConfiguration = new SqlPostCompilerConfiguration(GetDatabaseMap(defaultSchema.Catalog), GetSchemaMap(defaultSchema));

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query, compilerConfiguration).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(DummyDatabaseName), Is.True);
        Assert.That(queryText.Contains(DummySchemaName), Is.True);
      }
      if (IsMultischemaSupported) {
        _ = Assert.Throws<InvalidOperationException>(() => queryText = Driver.Compile(query).GetCommandText());

        var postCompilerConfiguration = new SqlPostCompilerConfiguration(emptyMap, emptyMap);
        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.True);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);

        postCompilerConfiguration = new SqlPostCompilerConfiguration(GetDatabaseMap(defaultSchema.Catalog), GetSchemaMap(defaultSchema));

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(DummyDatabaseName), Is.False);
        Assert.That(queryText.Contains(DummySchemaName), Is.True);
      }
      else {
        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText());

        var postCompilerConfiguration = new SqlPostCompilerConfiguration(emptyMap, emptyMap);

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText(postCompilerConfiguration));
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);

        postCompilerConfiguration = new SqlPostCompilerConfiguration(GetDatabaseMap(defaultSchema.Catalog), GetSchemaMap(defaultSchema));

        Assert.DoesNotThrow(() => queryText = Driver.Compile(query).GetCommandText());
        Assert.That(queryText.Contains(defaultSchema.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(defaultSchema.Catalog.GetDbNameInternal()), Is.False);
        Assert.That(queryText.Contains(DummyDatabaseName), Is.False);
        Assert.That(queryText.Contains(DummySchemaName), Is.False);
      }
    }

    private static Dictionary<string, string> GetSchemaMap(Schema schema) =>
      new() { { schema.GetDbNameInternal(), DummySchemaName } };

    private static Dictionary<string, string> GetDatabaseMap(Catalog catalog) =>
      new() { { catalog.GetDbNameInternal(), DummyDatabaseName } };


    private Schema GetSchema()
    {
      var catalog = new Catalog(CatalogName);
      var schema = catalog.CreateSchema(SchemaName);

      var defaultSchema = catalog.DefaultSchema = schema;
      var table = defaultSchema.CreateTable(TableName);
      var column = table.CreateColumn("Id", GetServerTypeFor(typeof (int)));
      _ = table.CreatePrimaryKey("PK_DenyNamesReadingTest", column);
      _ = table.CreateColumn("CreationDate", GetServerTypeFor(typeof (DateTime)));
      return defaultSchema;
    }

    private SqlValueType GetServerTypeFor(Type type) =>
      Driver.TypeMappings.Mappings[type].MapType(255, null, null);

    private SqlValueType GetServerTypeFor(Type type, int length) =>
      Driver.TypeMappings.Mappings[type].MapType(length, null, null);
  }
}
