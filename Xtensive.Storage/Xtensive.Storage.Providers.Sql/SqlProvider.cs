// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlProvider : ExecutableProvider
  {
    private readonly HandlerAccessor handlers;
    private readonly SqlQueryRequest request;

    public SqlQueryRequest Request
    {
      [DebuggerStepThrough]
      get { return request; }
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sessionHandler = (SessionHandler) handlers.SessionHandler;
      sessionHandler.DomainHandler.Compile(request);
      request.Bind();
      using (var e = sessionHandler.Execute(request)) {
        while (e.MoveNext())
          yield return e.Current;
      }
    }


    // Constructor

    public SqlProvider(CompilableProvider origin, SqlQueryRequest request, HandlerAccessor handlers)
      : base(origin)
    {
      this.request = request;
      this.handlers = handlers;
    }

    public SqlProvider(CompilableProvider origin, SqlQueryRequest request, HandlerAccessor handlers, IEnumerable<KeyValuePair<SqlParameter, Func<object>>> parameterBindings)
      : this(origin, request, handlers)
    {
      foreach (var pair in parameterBindings)
        request.ParameterBindings.Add(pair.Key, pair.Value);
    }
  }
}