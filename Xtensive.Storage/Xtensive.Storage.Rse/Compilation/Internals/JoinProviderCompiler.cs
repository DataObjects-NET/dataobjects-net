// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class JoinProviderCompiler : TypeCompiler<JoinProvider>
  {
    protected override Provider Compile(JoinProvider provider)
    {
      return new Providers.Executable.JoinProvider(
        provider.Header, 
        Compiler.Compile(provider.Left), 
        Compiler.Compile(provider.Right), 
        provider.LeftJoin, 
        provider.JoiningPairs);
    }


    // Constructor

    public JoinProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}