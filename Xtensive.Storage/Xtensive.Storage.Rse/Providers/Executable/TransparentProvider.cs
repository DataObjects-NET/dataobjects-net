// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  public abstract class TransparentProvider : UnaryExecutableProvider
  {
    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return Source.GetService<T>();
    }


    // Constructor

    /// <inheritdoc/>
    protected TransparentProvider(CompilableProvider origin, Provider source)
      : base(origin, source)
    {
    }
  }
}