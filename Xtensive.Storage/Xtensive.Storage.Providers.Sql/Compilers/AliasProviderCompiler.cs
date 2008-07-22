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

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal class AliasProviderCompiler : TypeCompiler<AliasProvider>
  {
    /// <inheritdoc/>
    protected override ExecutableProvider Compile(AliasProvider provider)
    {
      var source = (SqlProvider)Compiler.Compile(provider.Source, true);
      var queryRef = Xtensive.Sql.Dom.Sql.QueryRef(source.Query, provider.Alias);
      SqlSelect query = Xtensive.Sql.Dom.Sql.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider, query);
    }

    // Constructor

    public AliasProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}