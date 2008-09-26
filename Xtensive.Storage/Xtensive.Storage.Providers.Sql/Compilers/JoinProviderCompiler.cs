// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public class JoinProviderCompiler : TypeCompiler<JoinProvider>
  {
    /// <inheritdoc/>
    protected override ExecutableProvider Compile(JoinProvider provider, params ExecutableProvider[] compiledSources)
    {
      var left = compiledSources[0] as SqlProvider;
      var right = compiledSources[1] as SqlProvider;

      if (left == null || right == null)
        return null;
      var leftSelect = (SqlSelect)left.Request.Statement;
      var leftQuery = SqlFactory.QueryRef(leftSelect);
      var rightSelect = (SqlSelect)right.Request.Statement;
      var rightQuery = SqlFactory.QueryRef(rightSelect);
      var joinedTable = SqlFactory.Join(
        provider.LeftJoin ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin,
        leftQuery,
        rightQuery,
        provider.EqualIndexes
          .Select(pair => leftQuery.Columns[pair.First] == rightQuery.Columns[pair.Second])
          .Aggregate(null as SqlExpression, (expression,binary) => expression & binary)
        );

      SqlSelect query = SqlFactory.Select(joinedTable);
      query.Columns.AddRange(leftQuery.Columns.Union(rightQuery.Columns).Cast<SqlColumn>());
      var request = new SqlFetchRequest(query, provider.Header, left.Request.ParameterBindings.Union(right.Request.ParameterBindings));
      return new SqlProvider(provider, request, Handlers, left, right);
    }


    // Constructors

    public JoinProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}