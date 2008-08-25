// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlProvider : ExecutableProvider
  {
    private const string PARAMETERS = "Parameters";
    private readonly SqlSelect query;
    private readonly HandlerAccessor handlers;

    public SqlSelect Query
    {
      [DebuggerStepThrough]
      get { return query; }
    }

    public Dictionary<SqlParameter, Func<object>> Parameters { get; private set; }

    protected override void OnBeforeEnumerate(EnumerationContext context)
    {
      base.OnBeforeEnumerate(context);
      BindParameters(context);
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sessionHandler = (SessionHandler) handlers.SessionHandler;
      SqlRequest request = new SqlRequest(Query, Header.TupleDescriptor);
      request.Parameters.AddRange(GetSqlParameters(context));
      using (var e = sessionHandler.Execute(request)) {
        while (e.MoveNext())
          yield return e.Current;
      }
    }

    private void BindParameters(EnumerationContext context)
    {
      List<SqlParameter> sqlParameters = GetSqlParameters(context);
      int i = 0;
      foreach (var pair in Parameters) {
        SqlParameter p = pair.Key;
        p.ParameterName = "p" + i++;
        p.Value = pair.Value.Invoke();
        sqlParameters.Add(p);
      }
    }

    private List<SqlParameter> GetSqlParameters(EnumerationContext context)
    {
      List<SqlParameter> sqlParameters = context.GetValue<List<SqlParameter>>(PARAMETERS);
      if (sqlParameters == null) {
        sqlParameters = new List<SqlParameter>();
        context.SetValue(PARAMETERS, sqlParameters);
      }
      return sqlParameters;
    }


    // Constructor

    public SqlProvider(CompilableProvider origin, SqlSelect query, HandlerAccessor handlers)
      : base(origin)
    {
      this.query = query;
      this.handlers = handlers;
      Parameters = new Dictionary<SqlParameter, Func<object>>();
    }

    public SqlProvider(CompilableProvider origin, SqlSelect query, HandlerAccessor handlers, IEnumerable<KeyValuePair<SqlParameter, Func<object>>> parameters)
      : this(origin, query, handlers)
    {
      foreach (var pair in parameters)
        Parameters.Add(pair.Key, pair.Value);
    }
  }
}