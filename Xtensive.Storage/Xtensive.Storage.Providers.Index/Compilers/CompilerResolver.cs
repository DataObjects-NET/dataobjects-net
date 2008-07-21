// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  public sealed class CompilerResolver : Rse.Compilation.Compiler
  {
    public HandlerAccessor HandlerAccessor { get; private set; }

    public override bool IsCompatible(Provider provider)
    {
      return provider as ExecutableProvider!=null;
    }

    public override ExecutableProvider ToCompatible(Provider provider)
    {
      throw new System.NotImplementedException();
    }


    // Constructor

    public CompilerResolver(HandlerAccessor handlerAccessor)
    {
      HandlerAccessor = handlerAccessor;
    }    
  }
}