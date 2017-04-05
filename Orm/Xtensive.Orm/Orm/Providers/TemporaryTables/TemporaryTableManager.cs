// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A manager of temporary tables.
  /// </summary>
  public class TemporaryTableManager : DomainBoundHandler
  {
    private const string TableNamePattern = "Tmp_{0}";
    private const string ColumnNamePattern = "C{0}";

    private TemporaryTableBackEnd backEnd;

    /// <summary>
    /// Gets value indicating whether temporary tables are supported.
    /// </summary>
    public bool Supported { get { return backEnd!=null; } }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="modelMapping">Model mapping.</param>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <returns>Built descriptor.</returns>
    [Obsolete("Use actual method with storage node parameter")]
    public TemporaryTableDescriptor BuildDescriptor(ModelMapping modelMapping, string name, TupleDescriptor source)
    {
      return BuildDescriptor(modelMapping, null, name, source, null);
    }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="modelMapping">Model mapping.</param>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <param name="fieldNames">The names of field in temporary table.</param>
    /// <returns>Built descriptor.</returns>
    [Obsolete("Use actual method with storage node parameter")]
    public TemporaryTableDescriptor BuildDescriptor(ModelMapping modelMapping, string name, TupleDescriptor source, string[] fieldNames)
    {
      return BuildDescriptor(modelMapping, null, name, source, fieldNames);
    }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="modelMapping">Model mapping.</param>
    /// <param name="nodeConfiguration">Storage node configuration.</param>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <returns>Built descriptor.</returns>
    public TemporaryTableDescriptor BuildDescriptor(ModelMapping modelMapping, NodeConfiguration nodeConfiguration, string name, TupleDescriptor source)
    {
      return BuildDescriptor(modelMapping, nodeConfiguration, name, source, null);
    }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="modelMapping">Model mapping.</param>
    /// <param name="nodeConfiguration">Storage node configuration.</param>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <param name="fieldNames">The names of field in temporary table.</param>
    /// <returns>Built descriptor.</returns>
    public TemporaryTableDescriptor BuildDescriptor(ModelMapping modelMapping, NodeConfiguration nodeConfiguration, string name, TupleDescriptor source, string[] fieldNames)
    {
      EnsureTemporaryTablesSupported();

      var hasColumns = source.Count > 0;

      // TODO: split this method to a set of various simple virtual methods
      var driver = Handlers.StorageDriver;

      var catalog = new Catalog(modelMapping.TemporaryTableDatabase);
      var schema = catalog.CreateSchema(modelMapping.TemporaryTableSchema);
      var collation = modelMapping.TemporaryTableCollation != null
        ? new Collation(schema, modelMapping.TemporaryTableCollation)
        : null;

      if (fieldNames==null)
        fieldNames = BuildFieldNames(source);

      var typeMappings = source
        .Select(driver.GetTypeMapping)
        .ToArray();

      // table
      var table = CreateTemporaryTable(schema, name, source, typeMappings, fieldNames, collation);
      var tableRef = SqlDml.TableRef(table);

      // select statement
      var queryStatement = MakeUpSelectQuery(tableRef, hasColumns);

      // insert statement
      var storeRequestBindings = new List<PersistParameterBinding>();
      var insertStatement = MakeUpInsertQuery(tableRef, typeMappings, storeRequestBindings, hasColumns);
      var result = new TemporaryTableDescriptor(name) {
        TupleDescriptor = source,
        QueryStatement = queryStatement
      };
      if (nodeConfiguration!=null) {
        result.CreateStatement = driver.Compile(SqlDdl.Create(table), nodeConfiguration).GetCommandText();
        result.DropStatement = driver.Compile(SqlDdl.Drop(table), nodeConfiguration).GetCommandText();
        result.StoreRequest = new PersistRequest(Handlers.StorageDriver, insertStatement, storeRequestBindings, nodeConfiguration);
        result.ClearRequest = new PersistRequest(Handlers.StorageDriver, SqlDml.Delete(tableRef), null, nodeConfiguration);
      }
      else {
        result.CreateStatement = driver.Compile(SqlDdl.Create(table)).GetCommandText();
        result.DropStatement = driver.Compile(SqlDdl.Drop(table)).GetCommandText();
        result.StoreRequest = new PersistRequest(Handlers.StorageDriver, insertStatement, storeRequestBindings);
        result.ClearRequest = new PersistRequest(Handlers.StorageDriver, SqlDml.Delete(tableRef), null);
      }

      result.StoreRequest.Prepare();
      result.ClearRequest.Prepare();

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
      var session = context.Session;
      var registry = GetRegistry(session);

      bool isLocked;
      if (!registry.States.TryGetValue(name, out isLocked))
        backEnd.InitializeTable(context, descriptor);
      else if (isLocked)
        return null;

      registry.States[name] = true;
      backEnd.AcquireTable(context, descriptor);

      return new Disposable(disposing => {
        backEnd.ReleaseTable(context, descriptor);
        registry.States[name] = false;
      });
    }

    protected void ExecuteNonQuery(EnumerationContext context, string statement)
    {
      var executor = context.Session.Services.Demand<ISqlExecutor>();
      executor.ExecuteNonQuery(statement);
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

    private void EnsureTemporaryTablesSupported()
    {
      if (!Supported)
        throw new NotSupportedException(Strings.ExTemporaryTablesAreNotSupportedByCurrentStorage);
    }

    private string[] BuildFieldNames(TupleDescriptor source)
    {
      return Enumerable.Range(0, source.Count)
          .Select(i => string.Format(ColumnNamePattern, i))
          .ToArray();
    }

    private Table CreateTemporaryTable(Schema schema, string name, TupleDescriptor source, TypeMapping[] typeMappings, string[]fieldNames, Collation collation)
    {
      var tableName = Handlers.NameBuilder.ApplyNamingRules(string.Format(TableNamePattern, name));
      var table = backEnd.CreateTemporaryTable(schema, tableName);

      if (source.Count > 0) {
        var fieldIndex = 0;
        foreach (var mapping in typeMappings) {
          var column = table.CreateColumn(fieldNames[fieldIndex], mapping.MapType());
          column.IsNullable = true;
          // TODO: Dmitry Maximov, remove this workaround than collation problem will be fixed
          if (mapping.Type==typeof(string))
            column.Collation = collation;
          fieldIndex++;
        }
      }
      else
        table.CreateColumn("dummy", new SqlValueType(SqlType.Int32));

      return table;
    }

    private SqlSelect MakeUpSelectQuery(SqlTableRef temporaryTable, bool hasColumns)
    {
      var queryStatement = SqlDml.Select(temporaryTable);
      if (hasColumns) {
        foreach (var column in temporaryTable.Columns)
          queryStatement.Columns.Add(column);
      }
      return queryStatement;
    }

    private SqlInsert MakeUpInsertQuery(SqlTableRef temporaryTable, TypeMapping[] typeMappings, List<PersistParameterBinding> storeRequestBindings, bool hasColumns)
    {
      var insertStatement = SqlDml.Insert(temporaryTable);
      if (hasColumns) {
        var fieldIndex = 0;
        foreach (var column in temporaryTable.Columns) {
          TypeMapping typeMapping = typeMappings[fieldIndex];
          var binding = new PersistParameterBinding(typeMapping, fieldIndex);
          insertStatement.Values[column] = binding.ParameterReference;
          storeRequestBindings.Add(binding);
          fieldIndex++;
        }
      }
      else
        insertStatement.Values[temporaryTable.Columns[0]] = SqlDml.Literal(0);
      return insertStatement;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      var providerInfo = Handlers.ProviderInfo;

      if (providerInfo.Supports(ProviderFeatures.TemporaryTables))
        backEnd = new RealTemporaryTableBackEnd();
      else if (providerInfo.Supports(ProviderFeatures.TemporaryTableEmulation))
        backEnd = new EmulatedTemporaryTableBackEnd();
    }
  }
}