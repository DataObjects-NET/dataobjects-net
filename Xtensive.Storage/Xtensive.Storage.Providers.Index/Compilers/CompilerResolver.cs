// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

namespace Xtensive.Storage.Providers.Index.Compilers
{
  public sealed class CompilerResolver : Rse.Compilation.Compiler
  {
    public HandlerAccessor HandlerAccessor { get; private set; }


    // Constructor

    public CompilerResolver(HandlerAccessor handlerAccessor)
    {
      HandlerAccessor = handlerAccessor;
    }
  }
}