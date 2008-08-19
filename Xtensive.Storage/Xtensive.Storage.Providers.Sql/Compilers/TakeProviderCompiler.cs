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
    protected override ExecutableProvider Compile(TakeProvider provider)
    {
      var source = (SqlProvider)Compiler.Compile(provider.Source, true);
      var query = source.Query.Clone() as SqlSelect;
      if (source.Query.Top==0 || source.Query.Top > provider.CompiledCount())
        source.Query.Top = provider.CompiledCount();
      return new SqlProvider(provider, query, Handlers);
    }

    // Constructor

    public TakeProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}