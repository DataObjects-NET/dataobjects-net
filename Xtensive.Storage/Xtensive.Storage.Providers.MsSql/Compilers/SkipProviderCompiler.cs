// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.08

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SkipProviderCompiler : TypeCompiler<SkipProvider>
  {
    protected override ExecutableProvider Compile(SkipProvider provider)
    {
      return null;
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null;

      var sourceQuery = (SqlSelect)source.Request.Statement.Clone();
      var columns = sourceQuery.Columns.ToList();
      var orderClause = provider.Header.Order.Select(pair =>  sourceQuery[pair.Key].Name + (pair.Value==Direction.Positive ? " ASC" : " DESC")).ToCommaDelimitedString();
      sourceQuery.Columns.Add(SqlFactory.Native("ROW_NUMBER() OVER (ORDER BY " + orderClause + ")"), "RowNumber");
      sourceQuery.OrderBy.Clear();
      var query = SqlFactory.Select(SqlFactory.QueryRef(sourceQuery));
      query.Columns.AddRange(columns);
      query.Where = sourceQuery["RowNumber"] > provider.CompiledCount();
      foreach (KeyValuePair<int, Direction> sortOrder in provider.Header.Order)
        query.OrderBy.Add(sortOrder.Key, sortOrder.Value==Direction.Positive);
      var request = new SqlQueryRequest(sourceQuery, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    // Constructor

    public SkipProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}