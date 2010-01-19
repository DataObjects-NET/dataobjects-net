// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A manager of temporary tables.
  /// </summary>
  public class TemporaryTableManager : InitializableHandlerBase
  {
    private const string TableNamePattern = "Tmp_{0}";
    private const string ColumnNamePattern = "C{0}";

    private DomainHandler DomainHandler { get { return (DomainHandler) Handlers.DomainHandler; } }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <returns>Built descriptor.</returns>
    public TemporaryTableDescriptor BuildDescriptor(string name, TupleDescriptor source)
    {
      return BuildDescriptor(name, source, null);
    }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <param name="fieldNames">The names of field in temporary table.</param>
    /// <returns>Built descriptor.</returns>
    public TemporaryTableDescriptor BuildDescriptor(string name, TupleDescriptor source, string[] fieldNames)
    {
      var driver = DomainHandler.Driver;
      var catalog = new Catalog(DomainHandler.Schema.Catalog.Name);
      var schema = catalog.CreateSchema(DomainHandler.Schema.Name);

      if (fieldNames==null) {
        fieldNames = Enumerable.Range(0, source.Count)
          .Select(i => string.Format(ColumnNamePattern, i))
          .ToArray();
      }

      // table
      string tableName = string.Format(TableNamePattern, name);
      var supportTmpTables = DomainHandler.ProviderInfo.Supports(ProviderFeatures.TemporaryTables);
      var table = supportTmpTables ? schema.CreateTemporaryTable(tableName) : schema.CreateTable(tableName);
      var typeMappings = source
        .Select(type => driver.GetTypeMapping(type))
        .ToArray();
      int fieldIndex = 0;
      foreach (var mapping in typeMappings) {
        var column = table.CreateColumn(fieldNames[fieldIndex], mapping.BuildSqlType());
        column.IsNullable = true;
        // TODO: Dmitry Maximov, remove this workaround than collation problem will be fixed
        if (mapping.Type==typeof (string))
           column.Collation = DomainHandler.Schema.Collations.FirstOrDefault();
        fieldIndex++;
      }


      // select statement
      var tableRef = SqlDml.TableRef(table);
      var queryStatement = SqlDml.Select(tableRef);
      foreach (var column in tableRef.Columns)
        queryStatement.Columns.Add(column);

      // insert statement
      var insertStatement = SqlDml.Insert(tableRef);
      var storeRequestBindings = new List<PersistParameterBinding>();
      fieldIndex = 0;
      foreach (var column in tableRef.Columns) {
        TypeMapping typeMapping = typeMappings[fieldIndex];
        var binding = new PersistParameterBinding(fieldIndex, typeMapping);
        insertStatement.Values[column] = binding.ParameterReference;
        storeRequestBindings.Add(binding);
        fieldIndex++;
      }

      var result = new TemporaryTableDescriptor(name) {
        TupleDescriptor = source,
        CreateStatement = driver.Compile(SqlDdl.Create(table)).GetCommandText(),
        DropStatement = driver.Compile(SqlDdl.Drop(table)).GetCommandText(),
        StoreRequest = new PersistRequest(insertStatement, storeRequestBindings),
        ClearRequest = new PersistRequest(SqlDml.Delete(tableRef)),
        QueryStatement = queryStatement
      };
      return result;
    }

    /// <summary>
    /// Acquires the lock on the specified temporary table.
    /// </summary>
    /// <param name="descriptor">The descriptor of temporary table.</param>
    /// <returns>A <see cref="IDisposable"/> implementor that should be used to free acquired lock.</returns>
    public IDisposable Acquire(TemporaryTableDescriptor descriptor)
    {
      var name = descriptor.Name;
      var sessionHandler = Handlers.SessionHandler;
      var session = sessionHandler.Session;
      var registry = session.Extensions.Get<TemporaryTableStateRegistry>();
      if (registry == null) {
        registry = new TemporaryTableStateRegistry();
        session.Extensions.Set(registry);
      }
      bool isLocked;
      if (!registry.States.TryGetValue(name, out isLocked))
        Handlers.SessionHandler.GetService<IQueryExecutor>().ExecuteNonQuery(descriptor.CreateStatement);
      else if (isLocked)
        throw new InvalidOperationException(string.Format(Strings.ExTemporaryTableXIsLocked, name));
      registry.States[name] = true;
      return new Disposable(disposing => {
        registry.States[name] = false;
      });
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}