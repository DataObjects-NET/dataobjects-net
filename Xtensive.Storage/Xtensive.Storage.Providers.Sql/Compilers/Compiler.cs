// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public sealed class Compiler : Rse.Compilation.Compiler
  {
    public HandlerAccessor HandlerAccessor { get; private set; }

    /// <inheritdoc/>
    public override bool IsCompatible(Provider provider)
    {
      return provider as SqlProvider!=null;
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(Provider provider)
    {
      throw new System.NotImplementedException();
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Compiler(HandlerAccessor handlerAccessor)
    {
      HandlerAccessor = handlerAccessor;
    }
  }
}