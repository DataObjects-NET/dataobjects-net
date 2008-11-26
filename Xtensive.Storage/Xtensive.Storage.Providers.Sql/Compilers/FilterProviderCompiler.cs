// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Compilation;
using SqlFactory = Xtensive.Sql.Dom.Sql;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal class FilterProviderCompiler : TypeCompiler<FilterProvider>
  {
    protected override ExecutableProvider Compile(FilterProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      SqlSelect query;
      if (provider.Source is AggregateProvider) {
        var queryRef = SqlFactory.QueryRef(source.Request.Statement as SqlSelect);
        query = SqlFactory.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      else
        query = (SqlSelect) source.Request.Statement.Clone();

      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      var visitor = new ExpressionProcessor(request);
      visitor.AppendFilterToRequest(provider.Predicate);

      return new SqlProvider(provider, request, Handlers, source);
    }


    // Constructor

    public FilterProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}