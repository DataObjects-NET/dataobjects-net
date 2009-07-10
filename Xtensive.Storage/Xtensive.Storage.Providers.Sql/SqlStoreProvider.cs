// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlStoreProvider : SqlProvider,
    IHasNamedResult
  {
    public new StoreProvider Origin { get; private set; }

    public ExecutableProvider Source { get; private set; }

    public Schema Schema { get; private set; }

    public Table Table { get; private set; }

    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);

      // Table already was created and added to schema
      if (Table.Schema!=null)
        return;
      if (Source==null)
        return;

      SessionHandler sessionHandler = (SessionHandler) handlers.SessionHandler;
      SqlBatch batch = SqlDml.Batch();
      batch.Add(SqlDdl.Create(Table));
      sessionHandler.ExecuteNonQuery(batch);
      Schema.Tables.Add(Table);

      SqlTableRef tableRef = SqlDml.TableRef(Table);
      SqlInsert insert = SqlDml.Insert(tableRef);
      var bindings = new List<SqlUpdateParameterBinding>();
      int i = 0;
      foreach (SqlTableColumn column in tableRef.Columns) {
        int fieldIndex = i;
        TypeMapping typeMapping = ((DomainHandler) handlers.DomainHandler).ValueTypeMapper.GetTypeMapping(Header.Columns[i].Type);
        var binding = new SqlUpdateParameterBinding((target => target.IsNull(fieldIndex) ? DBNull.Value : target.GetValue(fieldIndex)), typeMapping);
        insert.Values[column] = binding.ParameterReference;
        bindings.Add(binding);
        i++;
      }
      SqlUpdateRequest updateRequest = new SqlUpdateRequest(insert, -1, bindings);
      foreach (Tuple tuple in Source)
        sessionHandler.ExecuteUpdateRequest(updateRequest, tuple);
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnAfterEnumerate(context);

      // Table already was deleted and removed from schema
      if (Table.Schema==null)
        return;
      if (Scope==TemporaryDataScope.Global)
        return;

      SqlBatch batch = SqlDml.Batch();
      batch.Add(SqlDdl.Drop(Table));
      ((SessionHandler) handlers.SessionHandler).ExecuteNonQuery(batch);
      Schema.Tables.Remove(Table);
    }

    /// <inheritdoc/>
    public TemporaryDataScope Scope { get { return Origin.Scope; } }

    /// <inheritdoc/>
    public string Name { get { return Origin.Name; } }
    
    // Constructors

    public SqlStoreProvider(StoreProvider origin, SqlSelect request, HandlerAccessor handlers, ExecutableProvider source, Table table)
      : base(origin, request, handlers, source)
    {
      AddService<IHasNamedResult>();
      Origin = origin;
      Source = source;
      Schema = ((DomainHandler) handlers.DomainHandler).Schema;
      Table = table;
    }
  }
}