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
      var result = new List<Tuple>(0);
      var sessionHandler = (SessionHandler)handlers.SessionHandler;
      using(DbDataReader reader = sessionHandler.ExecuteReader(Query)) {
        while(reader.Read()) {
          var tuple = Tuple.Create(Header.TupleDescriptor);
          for (int i = 0; i < reader.FieldCount; i++)
            tuple.SetValue(i, DBNull.Value == reader[i] ? null : reader[i]);
          result.Add(tuple);
        }
      }
      return result;
    }

    protected override void Initialize()
    {}


    // Constructor

    public SqlProvider(Provider origin, SqlSelect sqlSelect, HandlerAccessor handlers)
      : base(origin)
    {
      this.sqlSelect = sqlSelect;
      this.handlers = handlers;
    }
  }
}