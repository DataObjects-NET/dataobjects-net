// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  internal sealed class Compiler : Rse.Compilation.Compiler
  {
    private readonly DefaultCompiler fallbackCompiler;

    public HandlerAccessor HandlerAccessor { get; private set; }

    public override bool IsCompatible(Provider provider)
    {
      return provider as ExecutableProvider!=null;
    }

    public override ExecutableProvider ToCompatible(Provider provider)
    {
      return fallbackCompiler.Compile(provider);
    }


    // Constructor

    public Compiler(HandlerAccessor handlerAccessor)
    {
      HandlerAccessor = handlerAccessor;
      fallbackCompiler = new DefaultCompiler();
    }    
  }
}