// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.16

using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  public abstract class ProviderProxy : ExecutableProvider
  {
    private Provider realProvider;
    public abstract Provider GetRealProvider();

    public Provider Real
    {
      get
      {
        EnsureRealProviderIsReady();
        return realProvider;
      }
    }

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
    public override IEnumerator<Tuple> GetEnumerator()
    {
      EnsureRealProviderIsReady();
      return Real.GetEnumerator();
    }


    // Constructors

    protected ProviderProxy(RecordHeader header, params Provider[] sourceProviders)
      : base(header, sourceProviders)
    {
    }

  }
}