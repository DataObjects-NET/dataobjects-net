// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataExtractor
  {
    private TypeMapping stringMapping;
    private TypeMapping intMapping;

    private readonly NameBuilder nameBuilder;
    private readonly ISqlExecutor executor;

    private readonly string databaseName;
    private readonly string schemaName;

    private readonly string metadataAssembly;
    private readonly string metadataAssemblyName;
    private readonly string metadataAssemblyVersion;

    private readonly string metadataType;
    private readonly string metadataTypeId;
    private readonly string metadataTypeName;

    private readonly string metadataExtension;
    private readonly string metadataExtensionName;
    private readonly string metadataExtensionText;

    public List<AssemblyMetadata> GetAssemblies()
    {
      var query = CreateQuery(metadataAssembly,
        metadataAssemblyName, metadataAssemblyVersion);

      return ExecuteQuery(query, ParseAssembly);
    }

   public List<TypeMetadata> GetTypes()
    {
      var query = CreateQuery(metadataType,
        metadataTypeId, metadataTypeName);

      return ExecuteQuery(query, ParseType);
    }

    public List<ExtensionMetadata> GetExtensions()
    {
      var query = CreateQuery(metadataExtension,
        metadataExtensionName, metadataExtensionText);

      return ExecuteQuery(query, ParseExtension);
    }

    #region Private / internal methods

    private ExtensionMetadata ParseExtension(DbDataReader reader)
    {
      var name = ReadString(reader, 0);
      var text = ReadString(reader, 1);
      return new ExtensionMetadata(name, text);
    }

    private AssemblyMetadata ParseAssembly(DbDataReader reader)
    {
      var name = ReadString(reader, 0);
      var version = ReadString(reader, 1);
      return new AssemblyMetadata(name, version);
    }

    private TypeMetadata ParseType(DbDataReader reader)
    {
      var id = ReadInt(reader, 0);
      var name = ReadString(reader, 1);
      return new TypeMetadata(id, name);
    }

    private List<T> ExecuteQuery<T>(ISqlCompileUnit query, Func<DbDataReader, T> parser)
    {
      var result = new List<T>();

      using (var command = executor.ExecuteReader(query)) {
        var reader = command.Reader;
        while (reader.Read())
          result.Add(parser.Invoke(reader));
      }

      return result;
    }
 
    private SqlSelect CreateQuery(string tableName, params string[] columnNames)
    {
      var catalog = new Catalog(databaseName);
      var schema = catalog.CreateSchema(schemaName);
      var table = schema.CreateTable(tableName);
      foreach (var column in columnNames)
        table.CreateColumn(column);
      var tableRef = SqlDml.TableRef(table);
      var select = SqlDml.Select(tableRef);
      var columnRefs = columnNames
        .Select((c, i) => SqlDml.ColumnRef(tableRef.Columns[i], c));
      foreach (var columnRef in columnRefs)
        select.Columns.Add(columnRef);
      return select;
    }

    private string ReadString(DbDataReader reader, int index)
    {
      return reader.IsDBNull(index) ? null : (string) stringMapping.ReadValue(reader, index);
    }

    private int ReadInt(DbDataReader reader, int index)
    {
      return (int) intMapping.ReadValue(reader, index);
    }

    private string TableOf(System.Type type)
    {
      var name = type
        .GetCustomAttributes(typeof (TableMappingAttribute), false)
        .Cast<TableMappingAttribute>()
        .Single().Name;
      return nameBuilder.ApplyNamingRules(name);
    }

    private string ColumnOf<TItem, TProperty>(Expression<Func<TItem, TProperty>> expression)
    {
      var memberExpression = (MemberExpression) expression.Body.StripCasts();
      var name = memberExpression.Member.Name;
      return nameBuilder.ApplyNamingRules(name);
    }

    #endregion

    // Constructors

    public MetadataExtractor(StorageDriver driver, NameBuilder nameBuilder, ISqlExecutor executor, Schema schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(nameBuilder, "nameBuilder");
      ArgumentValidator.EnsureArgumentNotNull(executor, "executor");
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");

      this.nameBuilder = nameBuilder;
      this.executor = executor;

      stringMapping = driver.GetTypeMapping(typeof (string));
      intMapping = driver.GetTypeMapping(typeof (int));

      databaseName = schema.Catalog.Name;
      schemaName = schema.Name;

      metadataAssembly = TableOf(typeof (Assembly));
      metadataAssemblyName = ColumnOf((Assembly x) => x.Name);
      metadataAssemblyVersion = ColumnOf((Assembly x) => x.Version);

      metadataType = TableOf(typeof (Metadata.Type));
      metadataTypeId = ColumnOf((Metadata.Type x) => x.Id);
      metadataTypeName = ColumnOf((Metadata.Type x) => x.Name);

      metadataExtension = TableOf(typeof (Extension));
      metadataExtensionName = ColumnOf((Extension x) => x.Name);
      metadataExtensionText = ColumnOf((Extension x) => x.Text);
    }
  }
}