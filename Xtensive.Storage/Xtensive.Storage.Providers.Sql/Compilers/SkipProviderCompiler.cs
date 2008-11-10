// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.08

using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SkipProviderCompiler : TypeCompiler<SkipProvider>
  {
    protected override ExecutableProvider Compile(SkipProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement.Clone();
      query.Offset += provider.CompiledCount();
      var request = new SqlFetchRequest(query, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);
    }

    // Constructor

    public SkipProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}