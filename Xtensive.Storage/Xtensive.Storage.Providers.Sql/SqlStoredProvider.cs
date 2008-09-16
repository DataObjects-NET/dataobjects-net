// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlStoredProvider : SqlProvider,
    IHasNamedResult
  {
    public new StoredProvider Origin { get; private set; }

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
      SqlBatch batch = SqlFactory.Batch();
      batch.Add(SqlFactory.Create(Table));
      sessionHandler.ExecuteNonQuery(batch);
      Schema.Tables.Add(Table);

      SqlTableRef tableRef = SqlFactory.TableRef(Table);
      SqlInsert insert = SqlFactory.Insert(tableRef);
      SqlModificationRequest modificationRequest = new SqlModificationRequest(insert);
      int i = 0;
      foreach (SqlTableColumn column in tableRef.Columns) {
        SqlParameter p = new SqlParameter();
        insert.Values[column] = p;
        int fieldIndex = i;
        modificationRequest.ParameterBindings.Add(p, (target => target.IsNull(fieldIndex) ? DBNull.Value : target.GetValue(fieldIndex)));
        i++;
      }
      sessionHandler.DomainHandler.Compile(modificationRequest);
      foreach (Tuple tuple in Source.ToList()) {
        modificationRequest.BindParametersTo(tuple);
        sessionHandler.ExecuteNonQuery(modificationRequest);
      }
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnAfterEnumerate(context);

      // Table already was deleted and removed from schema
      if (Table.Schema==null)
        return;
      if (Scope==TemporaryDataScope.Global)
        return;

      SqlBatch batch = SqlFactory.Batch();
      batch.Add(SqlFactory.Drop(Table));
      ((SessionHandler) handlers.SessionHandler).ExecuteNonQuery(batch);
      Schema.Tables.Remove(Table);
    }

    /// <inheritdoc/>
    public TemporaryDataScope Scope
    {
      get { return Origin.Scope; }
    }

    /// <inheritdoc/>
    public string Name
    {
      get { return Origin.Name; }
    }


    // Constructor

    public SqlStoredProvider(StoredProvider origin, SqlFetchRequest request, HandlerAccessor handlers, ExecutableProvider source, Table table)
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