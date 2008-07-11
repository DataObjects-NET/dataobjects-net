// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;
using System.Linq;

namespace Xtensive.Storage.Rse.Compilation
{
  public sealed class CompilationContext : Context<CompilationScope>
  {
    public IEnumerable<CompilerResolver> CompilerResolvers { get; private set; }

    public Provider Compile(Provider provider)
    {
      if (provider == null)
        return null;
      var compilableProvider = provider as CompilableProvider;
      if (compilableProvider == null)
        return provider;
      foreach (var resolver in CompilerResolvers) {
        var compiler = resolver.GetCompiler(compilableProvider);
        if (compiler != null) {
          var result = compiler.Compile(compilableProvider);
          if(result != null)
            return result;
        }
      }
      throw new InvalidOperationException();
    }

    protected override CompilationScope CreateActiveScope()
    {
      return new CompilationScope(this);
    }

    public override bool IsActive
    {
      get { return CompilationScope.CurrentContext == this; }
    }


    // Constructor
    public CompilationContext(IEnumerable<CompilerResolver> compilerResolvers)
    {
      CompilerResolvers = compilerResolvers;
    }

    public CompilationContext(CompilationContext baseContext, IEnumerable<CompilerResolver> compilerResolvers)
    {
      CompilerResolvers = compilerResolvers.Union(baseContext.CompilerResolvers).ToList();
    }
  }
}