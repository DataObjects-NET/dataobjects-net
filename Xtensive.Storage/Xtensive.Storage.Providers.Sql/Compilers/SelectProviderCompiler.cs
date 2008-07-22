// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SelectProviderCompiler : TypeCompiler<SelectProvider>
  {
    protected override ExecutableProvider Compile(SelectProvider provider)
    {
      var source = (SqlProvider)Compiler.Compile(provider.Source, true);
      var queryRef = Xtensive.Sql.Dom.Sql.QueryRef(source.Query);
      SqlSelect query = Xtensive.Sql.Dom.Sql.Select(queryRef);
      query.Columns.AddRange(provider.ColumnsToSelect.Select(i => (SqlColumn)queryRef.Columns[i]));

      return new SqlProvider(provider, query);
    }

    // Constructors

    public SelectProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}