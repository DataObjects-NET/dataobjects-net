// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SortProviderCompiler : TypeCompiler<SortProvider>
  {
    protected override ExecutableProvider Compile(SortProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      query.OrderBy.Clear();
      foreach (KeyValuePair<int, Direction> sortOrder in provider.Order)
        query.OrderBy.Add(sortOrder.Key + 1, sortOrder.Value == Direction.Positive);

      var request = new SqlFetchRequest(query, provider.Header.TupleDescriptor, source.Request.Parameters);
      return new SqlProvider(provider, request, Handlers, source);
    }


    // Constructors

    public SortProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}