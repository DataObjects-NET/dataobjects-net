// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class TakeProviderCompiler : TypeCompiler<TakeProvider>
  {
    protected override ExecutableProvider Compile(TakeProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      if (query.Top==0 || query.Top > provider.CompiledCount())
        query.Top = provider.CompiledCount();
      var request = new SqlFetchRequest(query, provider.Header.TupleDescriptor, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    // Constructor

    public TakeProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}