// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlProvider : ExecutableProvider
  {
    protected readonly HandlerAccessor handlers;
    protected SqlQueryRequest request;

    public SqlQueryRequest Request
    {
      [DebuggerStepThrough]
      get { return request; }

      [DebuggerStepThrough]
      set { request = value; }
    }

    protected override IEnumerable<Tuple> OnEnumerate(Rse.Providers.EnumerationContext context)
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

    public SqlProvider(CompilableProvider origin, SqlQueryRequest request, HandlerAccessor handlers, params ExecutableProvider[] sources)
      : base(origin, sources)
    {
      this.request = request;
      this.handlers = handlers;
      foreach (ExecutableProvider source in sources) {
        SqlProvider sqlProvider = source as SqlProvider;
        if (sqlProvider!=null && sqlProvider.Request != null)
          foreach (var pair in sqlProvider.Request.ParameterBindings)
            request.ParameterBindings[pair.Key] = pair.Value;
      }
    }
  }
}