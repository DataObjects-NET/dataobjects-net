// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Sql.Info;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Sql;
using Xtensive.Sql.Model;
using Xtensive.Orm.Providers.Sql.Resources;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A manager of temporary tables.
  /// </summary>
  public class TemporaryTableManager : InitializableHandlerBase
  {
    private const string TableNamePattern = "Tmp_{0}";
    private const string ColumnNamePattern = "C{0}";

//    private SqlDriver Driver { get { return DomainHandler.Driver; } }
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
      // TODO: split this method to a set of various simple virtual methods
      var driver = DomainHandler.Driver;
      var catalog = new Catalog(DomainHandler.Schema.Catalog.Name);
      var schema = catalog.CreateSchema(DomainHandler.Schema.Name);

      if (fieldNames==null) {
        fieldNames = Enumerable.Range(0, source.Count)
          .Select(i => string.Format(ColumnNamePattern, i))
          .ToArray();
      }

      // table
      var tableName = Handlers.NameBuilder.ApplyNamingRules(string.Format(TableNamePattern, name));
      var table = CreateTemporaryTable(schema, tableName);
      var typeMappings = source
        .Select(driver.GetTypeMapping)
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
        ClearRequest = new PersistRequest(SqlDml.Delete(tableRef), null),
        QueryStatement = queryStatement
      };

      result.StoreRequest.Prepare(DomainHandler);
      result.ClearRequest.Prepare(DomainHandler);

      return result;
    }

    /// <summary>
    /// Acquires the lock on the specified temporary table.
    /// </summary>
    /// <param name="context">The <see cref="EnumerationContext"/>.</param>
    /// <param name="descriptor">The descriptor of temporary table.</param>
    /// <returns>
    /// A <see cref="IDisposable"/> implementor that should be used to free acquired lock.
    /// </returns>
    public IDisposable Acquire(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      var name = descriptor.Name;
      var sessionHandler = context.SessionHandler;
      var session = sessionHandler.Session;
      var registry = GetRegistry(session);

      bool isLocked;
      if (!registry.States.TryGetValue(name, out isLocked))
        InitializeTable(context, descriptor);
      else if (isLocked)
        return null;

      registry.States[name] = true;
      AcquireTable(context, descriptor);

      return new Disposable(disposing => {
        ReleaseTable(context, descriptor);
        registry.States[name] = false;
      });
    }

    /// <summary>
    /// Creates the temporary table with the specified name.
    /// </summary>
    /// <param name="schema">The schema to create table in.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <returns>Created table.</returns>
    protected virtual Table CreateTemporaryTable(Schema schema, string tableName)
    {
      var table = schema.CreateTemporaryTable(tableName);
      if (!DomainHandler.ProviderInfo.Supports(TemporaryTableFeatures.Local))
        table.IsGlobal = true;
      return table;
    }

    /// <summary>
    /// Initializes the table. This is called once per session on a first acquire request.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    protected virtual void InitializeTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
      ExecuteNonQuery(context, descriptor.CreateStatement);
    }

    /// <summary>
    /// Gets the lock on a temporary table.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    protected virtual void AcquireTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
    }

    /// <summary>
    /// Releases the lock on a temporary table.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    protected virtual void ReleaseTable(EnumerationContext context, TemporaryTableDescriptor descriptor)
    {
    }

    protected void ExecuteNonQuery(EnumerationContext context, string statement)
    {
      context.SessionHandler.GetService<IQueryExecutor>(true).ExecuteNonQuery(statement);
    }

    private static TemporaryTableStateRegistry GetRegistry(Session session)
    {
      var registry = session.Extensions.Get<TemporaryTableStateRegistry>();
      if (registry==null) {
        registry = new TemporaryTableStateRegistry();
        session.Extensions.Set(registry);
      }
      return registry;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
    }
  }
}