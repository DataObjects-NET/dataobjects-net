// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Core;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Index.Compilers
{
  internal sealed class Compiler : Rse.Compilation.Compiler
  {
    public HandlerAccessor Handlers { get; private set; }

    public override bool IsCompatible(ExecutableProvider provider)
    {
      return true;
    }

    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      return provider;
    }


    // Constructor

    public Compiler(HandlerAccessor handlers)
      : base(handlers.Domain.Configuration.ConnectionInfo)
    {
      Handlers = handlers;
    }    
  }
}