// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public class JoinProviderCompiler : TypeCompiler<JoinProvider>
  {
    /// <inheritdoc/>
    protected override ExecutableProvider Compile(JoinProvider provider)
    {
      var left = (SqlProvider)Compiler.Compile(provider.Left, true);
      var right = (SqlProvider)Compiler.Compile(provider.Right, true);

      var leftQuery = Xtensive.Sql.Dom.Sql.QueryRef(left.Query);
      var rightQuery = Xtensive.Sql.Dom.Sql.QueryRef(right.Query);
      var joinedTable = Xtensive.Sql.Dom.Sql.Join(
        provider.LeftJoin ? SqlJoinType.LeftOuterJoin : SqlJoinType.InnerJoin,
        leftQuery,
        rightQuery);

      SqlSelect query = Xtensive.Sql.Dom.Sql.Select(joinedTable);
      query.Columns.AddRange(joinedTable.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query);
    }


    // Constructors

    public JoinProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}