// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class JoinProviderCompiler : TypeCompiler<JoinProvider>
  {
    protected override ExecutableProvider Compile(JoinProvider provider)
    {
      return new Providers.Executable.JoinProvider(
        provider, 
        Compiler.Compile(provider.Left, true), 
        Compiler.Compile(provider.Right, true), 
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