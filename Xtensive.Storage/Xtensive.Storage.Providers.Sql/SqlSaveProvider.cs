// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.02

using System;
using System.Collections.Generic;
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
  internal class SqlSaveProvider : SqlProvider,
    IHasNamedResult
  {
    private readonly SaveProvider concreteOrigin;
    private readonly ExecutableProvider source;

    #region Cached properties

    private const string CachedStateName = "CachedState";

    private SqlSaveProviderState CachedState
    {
      get
      {
        SqlSaveProviderState result = GetCachedValue<SqlSaveProviderState>(EnumerationContext.Current, CachedStateName);
        if (result == null) {
          result = new SqlSaveProviderState();
          CachedState = result;
        }
        return result;
      }
      set { SetCachedValue(EnumerationContext.Current, CachedStateName, value); }
    }
    #endregion

    /// <inheritdoc/>
    public TemporaryDataScope Scope
    {
      get { return concreteOrigin.Scope; }
    }

    /// <inheritdoc/>
    public string Name
    {
      get { return concreteOrigin.Name; }
    }

    internal SqlSaveProviderData Data { get; private set; }

    protected override void OnBeforeEnumerate(Rse.Providers.EnumerationContext context)
    {
      if (!CachedState.BeforeEnumerationExecuted) {
        base.OnBeforeEnumerate(context);
        var sessionHandler = (SessionHandler) handlers.SessionHandler;
        sessionHandler.ExecuteNonQuery(Data.BeforeEnumerate);
        SqlInsert insert = SqlFactory.Insert(Data.Table);
        SqlModificationRequest request = new SqlModificationRequest(insert);
        int i = 0;
        foreach (SqlTableColumn column in Data.Table.Columns) {
          SqlParameter p = new SqlParameter();
          insert.Values[column] = p;
          int fieldIndex = i;
          request.ParameterBindings.Add(p, (target => target.IsNull(fieldIndex) ? DBNull.Value : target.GetValue(fieldIndex)));
          i++;
        }
        sessionHandler.DomainHandler.Compile(request);
        foreach (Tuple tuple in source.ToList()) {
          request.BindTo(tuple);
          sessionHandler.ExecuteNonQuery(request);
        }
        CachedState.BeforeEnumerationExecuted = true;
      }
    }

    protected override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
    {
      throw new NotSupportedException();
    }

    protected override void OnAfterEnumerate(Rse.Providers.EnumerationContext context)
    {
      base.OnAfterEnumerate(context);
      if (!CachedState.AfterEnumerationExecuted) {
        var sessionHandler = (SessionHandler) handlers.SessionHandler;
        sessionHandler.ExecuteNonQuery(Data.AfterEnumerate);
        sessionHandler.DomainHandler.Schema.Tables.Remove(Data.Table.DataTable as Table);
        CachedState.AfterEnumerationExecuted = true;
      }
    }


    // Constructors

    public SqlSaveProvider(CompilableProvider origin, HandlerAccessor handlers, ExecutableProvider source, SqlSaveProviderData data)
      : base(origin, null, handlers, source)
    {
      concreteOrigin = (SaveProvider) origin;
      this.source = source;
      AddService<IHasNamedResult>();
      Data = data;
    }
  }
}