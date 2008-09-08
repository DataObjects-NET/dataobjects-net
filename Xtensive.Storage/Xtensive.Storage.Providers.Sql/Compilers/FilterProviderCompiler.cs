// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Expressions;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal class FilterProviderCompiler : TypeCompiler<FilterProvider>
  {
    protected override ExecutableProvider Compile(FilterProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      var request = new SqlQueryRequest(query, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      var visitor = new FilterVisitor(request);
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