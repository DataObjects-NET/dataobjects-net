// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.28

using System;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  [Serializable]
  internal sealed class DistinctProviderCompiler : TypeCompiler<DistinctProvider>
  {
    protected override ExecutableProvider Compile(DistinctProvider provider, params ExecutableProvider[] compiledSources)
    {
      var source = compiledSources[0] as SqlProvider;
      if (source == null)
        return null;

      var query = (SqlSelect)source.Request.Statement;
      if (query.Distinct)
        return source;

      var clone = (SqlSelect)query.Clone();
      clone.Distinct = true;
      var request = new SqlFetchRequest(clone, provider.Header, source.Request.ParameterBindings);
      return new SqlProvider(provider, request, Handlers, source);

    }

    // Constructor
    
    public DistinctProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}