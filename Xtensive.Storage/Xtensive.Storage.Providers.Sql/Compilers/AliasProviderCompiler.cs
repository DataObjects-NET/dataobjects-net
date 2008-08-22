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
  internal class AliasProviderCompiler : TypeCompiler<AliasProvider>
  {
    /// <inheritdoc/>
    protected override ExecutableProvider Compile(AliasProvider provider)
    {
      var source = (SqlProvider)Compiler.Compile(provider.Source, true);
      var queryRef  = SqlFactory.QueryRef(source.Query, provider.Alias);
      var sqlSelect = SqlFactory.Select(queryRef);
      sqlSelect.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      return new SqlProvider(provider, sqlSelect, Handlers, source.Parameters);
    }


    // Constructor

    public AliasProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}