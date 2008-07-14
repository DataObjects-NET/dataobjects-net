// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public class AliasProviderCompiler : ProviderCompiler<AliasProvider>
  {
    /// <inheritdoc/>
    protected override Provider Compile(AliasProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null; // Fallback to base compiler.

      var queryRef = Xtensive.Sql.Dom.Sql.QueryRef(source.Query, provider.Alias);
      SqlSelect query = Xtensive.Sql.Dom.Sql.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      return new SqlProvider(provider.Header, query);
    }

    // Constructor

    public AliasProviderCompiler(Rse.Compilation.CompilerResolver resolver)
      : base(resolver)
    {
    }
  }
}