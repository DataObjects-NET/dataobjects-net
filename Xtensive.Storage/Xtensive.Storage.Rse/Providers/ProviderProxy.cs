// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.16

using System;
using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public abstract class ProviderProxy : ExecutableProvider
  {
    private ExecutableProvider real;

    public ExecutableProvider Real {
      get {
        EnsureRealProviderIsReady();
        return real;
      }
    }

    public abstract ExecutableProvider BuildReal();

    private void EnsureRealProviderIsReady()
    {
      if (real == null) lock (this) if (real == null) {
        real = BuildReal();
      }
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return Real.GetService<T>();
    }

    /// <inheritdoc/>
    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Real.Enumerate(context);
    }

   
    // Constructors

    protected ProviderProxy(CompilableProvider origin, params ExecutableProvider[] sourceProviders)
      : base(origin, sourceProviders)
    {
    }
  }
}