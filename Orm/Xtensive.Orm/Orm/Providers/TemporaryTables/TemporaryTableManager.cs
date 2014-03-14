// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

using Xtensive.Sql;
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

    public bool Supported { get { return backEnd!=null; } }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="modelMapping">Model mapping.</param>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <returns>Built descriptor.</returns>
    public TemporaryTableDescriptor BuildDescriptor(ModelMapping modelMapping, string name, TupleDescriptor source)
    {
      return BuildDescriptor(modelMapping, name, source, null);
    }

    /// <summary>
    /// Builds the descriptor of a temporary table.
    /// </summary>
    /// <param name="modelMapping">Model mapping.</param>
    /// <param name="name">The name of the temporary table.</param>
    /// <param name="source">The source.</param>
    /// <param name="fieldNames">The names of field in temporary table.</param>
    /// <returns>Built descriptor.</returns>
    public TemporaryTableDescriptor BuildDescriptor(ModelMapping modelMapping, string name, TupleDescriptor source, string[] fieldNames)
    {
      if (!Supported)
        throw new NotSupportedException(Strings.ExTemporaryTablesAreNotSupportedByCurrentStorage);

      var hasColumns = source.Count > 0;

      // TODO: split this method to a set of various simple virtual methods
      var driver = Handlers.StorageDriver;

      var catalog = new Catalog(modelMapping.TemporaryTableDatabase);
      var schema = catalog.CreateSchema(modelMapping.TemporaryTableSchema);
      var collation = modelMapping.TemporaryTableCollation!=null
        ? new Collation(schema, modelMapping.TemporaryTableCollation)
        : null;

      if (fieldNames==null) {
        fieldNames = Enumerable.Range(0, source.Count)
          .Select(i => string.Format(ColumnNamePattern, i))
          .ToArray();
      }

      // table
      var tableName = Handlers.NameBuilder.ApplyNamingRules(string.Format(TableNamePattern, name));
      var table = backEnd.CreateTemporaryTable(schema, tableName);
      var typeMappings = source
        .Select(driver.GetTypeMapping)
        .ToArray();

      if (hasColumns) {
        var fieldIndex = 0;
        foreach (var mapping in typeMappings) {
          var column = table.CreateColumn(fieldNames[fieldIndex], mapping.MapType());
          column.IsNullable = true;
          // TODO: Dmitry Maximov, remove this workaround than collation problem will be fixed
          if (mapping.Type==typeof (string))
            column.Collation = collation;
          fieldIndex++;
        }
      }
      else
        table.CreateColumn("dummy", new SqlValueType(SqlType.Int32));

      // select statement
      var tableRef = SqlDml.TableRef(table);
      var queryStatement = SqlDml.Select(tableRef);
      if (hasColumns) {
        foreach (var column in tableRef.Columns)
          queryStatement.Columns.Add(column);
      }

      // insert statement
      var insertStatement = SqlDml.Insert(tableRef);
      var storeRequestBindings = new List<PersistParameterBinding>();
      if (hasColumns) {
        var fieldIndex = 0;
        foreach (var column in tableRef.Columns) {
          TypeMapping typeMapping = typeMappings[fieldIndex];
          var binding = new PersistParameterBinding(typeMapping, fieldIndex);
          insertStatement.Values[column] = binding.ParameterReference;
          storeRequestBindings.Add(binding);
          fieldIndex++;
        }
      }
      else
        insertStatement.Values[tableRef.Columns[0]] = SqlDml.Literal(0);

      var result = new TemporaryTableDescriptor(name) {
        TupleDescriptor = source,
        CreateStatement = driver.Compile(SqlDdl.Create(table)).GetCommandText(),
        DropStatement = driver.Compile(SqlDdl.Drop(table)).GetCommandText(),
        StoreRequest = new PersistRequest(Handlers.StorageDriver, insertStatement, storeRequestBindings),
        ClearRequest = new PersistRequest(Handlers.StorageDriver, SqlDml.Delete(tableRef), null),
        QueryStatement = queryStatement
      };

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