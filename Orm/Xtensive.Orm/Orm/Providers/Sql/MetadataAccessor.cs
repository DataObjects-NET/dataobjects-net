// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System;
using System.Collections.Generic;
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
    private readonly StorageDriver driver;
    private IProviderExecutor providerExecutor;

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

      var compiled = driver.Compile(query).GetCommandText();

      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all <see cref="TypeMetadata"/> found in the storage.
    /// </summary>
    /// <returns>Stored <see cref="TypeMetadata"/> instances.</returns>
    public IEnumerable<TypeMetadata> GetTypes(string databaseName, string schemaName)
    {
//      var table = CreateQuery(databaseName, schemaName, metadataType);
//      var idColumn = table.CreateColumn(metadataTypeId);
//      var nameColumn = table.CreateColumn(metadataTypeName);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets <see cref="ExtensionMetadata"/>
    /// with the specified <paramref name="extensionName"/> from the storage.
    /// </summary>
    /// <returns>Stored <see cref="ExtensionMetadata"/> instance
    /// with the specified <paramref name="extensionName"/>.</returns>
    public IEnumerable<ExtensionMetadata> GetExtension(string databaseName, string schemaName, string extensionName)
    {
//      var table = CreateQuery(databaseName, schemaName, metadataExtension);
//      var nameColumn = table.CreateColumn(metadataExtensionName);
//      var textColumn = table.CreateColumn(metadataExtensionText);
      throw new NotImplementedException();
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

    public MetadataAccessor(NameBuilder nameBuilder, StorageDriver driver, IProviderExecutor providerExecutor)
    {
      ArgumentValidator.EnsureArgumentNotNull(nameBuilder, "nameBuilder");
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");
      ArgumentValidator.EnsureArgumentNotNull(providerExecutor, "queryExecutor");

      this.nameBuilder = nameBuilder;
      this.driver = driver;
      this.providerExecutor = providerExecutor;

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