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
  internal sealed class SelectProviderCompiler : TypeCompiler<SelectProvider>
  {
    protected override ExecutableProvider Compile(SelectProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlFactory.QueryRef(source.Query);
      SqlSelect query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(provider.ColumnIndexes.Select(i => (SqlColumn)queryRef.Columns[i]));

      return new SqlProvider(provider, query, Handlers, source.Parameters);
    }


    // Constructors

    public SelectProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}