// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public sealed class Compiler : Rse.Compilation.Compiler
  {
    public HandlerAccessor Handlers { get; private set; }

    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      return provider is SqlProvider;
    }

    /// <inheritdoc/>
    /// <exception cref="NotImplementedException">[Suppresses Agent Johnson warning]</exception>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      throw new NotImplementedException();
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Compiler(HandlerAccessor handlers)
    {
      Handlers = handlers;
    }
  }
}