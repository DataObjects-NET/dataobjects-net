// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

namespace Xtensive.Storage.Rse.Providers.Implementation
{
  public abstract class ProviderWrapper : ProviderImplementation
  {
    private readonly Provider source;

    public Provider Source
    {
      get { return source; }
    }

    public override ProviderOptionsStruct Options
    {
      get { return source.Options; }
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return source.GetService<T>();
    }


    // Constructor

    protected ProviderWrapper(RecordHeader header, Provider source)
      : base(header, source)
    {
      this.source = source;
    }
  }
}