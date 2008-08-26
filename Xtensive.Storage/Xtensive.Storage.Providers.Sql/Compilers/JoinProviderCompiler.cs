// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System.Linq;
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
    protected override ExecutableProvider Compile(JoinProvider provider)
    {
      var left = provider.Left.Compile(true) as SqlProvider;
      var right = provider.Right.Compile(true) as SqlProvider;

      if (left == null || right == null)
        return null;

      var leftQuery = SqlFactory.QueryRef(left.Query);
      var rightQuery = SqlFactory.QueryRef(right.Query);
      var joinedTable = SqlFactory.Join(
        provider.LeftJoin ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin,
        leftQuery,
        rightQuery);

      SqlSelect query = SqlFactory.Select(joinedTable);
      query.Columns.AddRange(joinedTable.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query, Handlers, left.Parameters.Union(right.Parameters));
    }


    // Constructors

    public JoinProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}