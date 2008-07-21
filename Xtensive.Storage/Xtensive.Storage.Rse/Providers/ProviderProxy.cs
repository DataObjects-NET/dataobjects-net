// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.16

using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  public abstract class ProviderProxy : ExecutableProvider
  {
    private ExecutableProvider realProvider;

    public ExecutableProvider Real
    {
      get
      {
        EnsureRealProviderIsReady();
        return realProvider;
      }
    }

    public abstract ExecutableProvider GetRealProvider();

    private void EnsureRealProviderIsReady()
    {
      if (realProvider == null) lock (this) if (realProvider == null) {
        realProvider = GetRealProvider();
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

    protected ProviderProxy(Provider origin, params ExecutableProvider[] sourceProviders)
      : base(origin, sourceProviders)
    {
    }

  }
}