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
    protected override ExecutableProvider Compile(SortProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null;

      var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
      SqlSelect query = SqlFactory.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      foreach (KeyValuePair<int, Direction> sortOrder in provider.Order)
        query.OrderBy.Add(sortOrder.Key, sortOrder.Value==Direction.Positive);

      SqlQueryRequest request = new SqlQueryRequest(query, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers);
    }


    // Constructors

    public SortProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}