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
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// Metadata accessor used to read persistent metadata
  /// on early stage of <see cref="Domain"/> build.
  /// </summary>
  public sealed class MetadataAccessor
  {
    private readonly NameBuilder nameBuilder;
    private readonly ISqlExecutor executor;

    private readonly string metadataAssembly;
    private readonly string metadataAssemblyName;
    private readonly string metadataAssemblyVersion;

    private readonly string metadataType;
    private readonly string metadataTypeId;
    private readonly string metadataTypeName;

    private readonly string metadataExtension;
    private readonly string metadataExtensionName;
    private readonly string metadataExtensionText;

    /// <summary>
    /// Gets all <see cref="AssemblyMetadata"/> found in the storage.
    /// </summary>
    /// <returns>Stored <see cref="AssemblyMetadata"/> instances.</returns>
    public IEnumerable<AssemblyMetadata> GetAssemblies(string databaseName, string schemaName)
    {
      var query = CreateQuery(
        databaseName, schemaName, metadataAssembly,
        metadataAssemblyName, metadataAssemblyVersion);

      return ExecuteQuery(query, ParseAssembly);
    }

    /// <summary>
    /// Gets all <see cref="TypeMetadata"/> found in the storage.
    /// </summary>
    /// <returns>Stored <see cref="TypeMetadata"/> instances.</returns>
    public IEnumerable<TypeMetadata> GetTypes(string databaseName, string schemaName)
    {
      var query = CreateQuery(
        databaseName, schemaName, metadataType,
        metadataTypeId, metadataTypeName);

      return ExecuteQuery(query, ParseType);
    }

    /// <summary>
    /// Gets all <see cref="ExtensionMetadata"/> found int storage.
    /// </summary>
    /// <returns>Stored <see cref="ExtensionMetadata"/> instances.</returns>
    public IEnumerable<ExtensionMetadata> GetExtensions(string databaseName, string schemaName)
    {
      var query = CreateQuery(
        databaseName, schemaName, metadataExtension,
        metadataExtensionName, metadataExtensionText);

      return ExecuteQuery(query, ParseExtension);
    }

    #region Private / internal methods

    private static ExtensionMetadata ParseExtension(DbDataReader reader)
    {
      var name = reader.GetString(0);
      var text = reader.IsDBNull(1) ? null : reader.GetString(1);
      return new ExtensionMetadata(name, text);
    }

    private static AssemblyMetadata ParseAssembly(DbDataReader reader)
    {
      var name = reader.GetString(0);
      var version = reader.IsDBNull(1) ? null : reader.GetString(1);
      return new AssemblyMetadata(name, version);
    }

    private static TypeMetadata ParseType(DbDataReader reader)
    {
      var id = reader.GetInt32(0);
      var name = reader.GetString(1);
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
 
    private SqlSelect CreateQuery(string databaseName, string schemaName, string tableName, params string[] columnNames)
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
      var memberExpression = (MemberExpression) expression.StripCasts();
      var name = memberExpression.Member.Name;
      return nameBuilder.ApplyNamingRules(name);
    }

    #endregion

    // Constructors

    public MetadataAccessor(NameBuilder nameBuilder, ISqlExecutor executor)
    {
      ArgumentValidator.EnsureArgumentNotNull(nameBuilder, "nameBuilder");
      ArgumentValidator.EnsureArgumentNotNull(executor, "executor");

      this.nameBuilder = nameBuilder;
      this.executor = executor;

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