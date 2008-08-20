// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class SqlProvider : ExecutableProvider
  {
    private readonly SqlSelect sqlSelect;
    private readonly HandlerAccessor handlers;

    public SqlSelect Query
    {
      [DebuggerStepThrough]
      get { return sqlSelect; }
    }

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var sessionHandler = (SessionHandler) handlers.SessionHandler;
      using (var e = sessionHandler.Execute(Query, Header.TupleDescriptor)) {
        while (e.MoveNext())
          yield return e.Current;
      }
    }


    // Constructor

    public SqlProvider(CompilableProvider origin, SqlSelect sqlSelect, HandlerAccessor handlers)
      : base(origin)
    {
      this.sqlSelect = sqlSelect;
      this.handlers = handlers;
    }
  }
}