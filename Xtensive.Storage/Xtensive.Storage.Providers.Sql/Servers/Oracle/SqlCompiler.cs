// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Servers.Oracle
{
  internal class SqlCompiler : ManualPagingSqlCompiler
  {
    protected override SqlSelect AddRowNumberColumn(SqlSelect sourceQuery, CompilableProvider provider, string rowNumberColumnName)
    {
      var sourceQueryOrigin = sourceQuery.From;
      foreach (KeyValuePair<int, Direction> order in provider.Header.Order)
        sourceQuery.OrderBy.Add(sourceQueryOrigin[order.Key], order.Value==Direction.Positive);
      var sourceQueryRef = SqlDml.QueryRef(sourceQuery);
      var resultQuery = SqlDml.Select(sourceQueryRef);
      resultQuery.Columns.AddRange(sourceQueryRef.Columns.Cast<SqlColumn>());
      resultQuery.Columns.Add(SqlDml.Native("rownum"), rowNumberColumnName);
      return resultQuery;
    }

    protected override string ProcessAliasedName(string name)
    {
      return Handlers.NameBuilder.ApplyNamingRules(name);
    }

    // Constructors
    
    public SqlCompiler(HandlerAccessor handlers, BindingCollection<object, ExecutableProvider> compiledSources)
      : base(handlers, compiledSources)
    {
    }
  }
}