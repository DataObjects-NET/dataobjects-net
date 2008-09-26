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
    protected override ExecutableProvider Compile(SelectProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
      SqlSelect query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(provider.ColumnIndexes.Select(i => (SqlColumn)queryRef.Columns[i]));
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);

      return new SqlProvider(provider, request, Handlers, source);
    }


    // Constructors

    public SelectProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}