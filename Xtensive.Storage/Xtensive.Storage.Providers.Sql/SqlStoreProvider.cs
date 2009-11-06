// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System.Collections.Generic;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlStoreProvider : SqlProvider,
    IHasNamedResult
  {
    private const string TableIsCreatedName = "TableIsCreated";

    private readonly Table table;
    private SqlPersistRequest persistRequest;

    public new StoreProvider Origin { get { return (StoreProvider) base.Origin; } }
    public ExecutableProvider Source { get { return (ExecutableProvider) Sources[0]; } }

    /// <inheritdoc/>
    public TemporaryDataScope Scope { get { return Origin.Scope; } }

    /// <inheritdoc/>
    public string Name { get { return Origin.Name; } }
    
    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);

      var executor = handlers.SessionHandler.GetService<IQueryExecutor>();
      executor.ExecuteNonQuery(SqlDdl.Create(table));
      context.SetValue(this, TableIsCreatedName, true);
      executor.Store(persistRequest, Source);
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnAfterEnumerate(context);

      var tableIsCreated = context.GetValue<bool?>(this, TableIsCreatedName) ?? false;
      if (tableIsCreated)
        handlers.SessionHandler.GetService<IQueryExecutor>().ExecuteNonQuery(SqlDdl.Drop(table));
    }

    protected override void Initialize()
    {
      base.Initialize();

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);
      var bindings = new List<SqlPersistParameterBinding>();
      var driver = ((DomainHandler) handlers.DomainHandler).Driver;
      int fieldIndex = 0;
      foreach (SqlTableColumn column in tableRef.Columns) {
        TypeMapping typeMapping = driver.GetTypeMapping(Header.Columns[fieldIndex].Type);
        var binding = new SqlPersistParameterBinding(fieldIndex, typeMapping);
        insert.Values[column] = binding.ParameterReference;
        bindings.Add(binding);
        fieldIndex++;
      }
      persistRequest = new SqlPersistRequest(insert, null, bindings);
    }

    // Constructors

    public SqlStoreProvider(StoreProvider origin, SqlSelect request, HandlerAccessor handlers, ExecutableProvider source, Table table)
     : base(origin, request, handlers, null, false, source)
    {
      AddService<IHasNamedResult>();
      this.table = table;
    }
  }
}