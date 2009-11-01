// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.07

namespace Xtensive.Storage.Rse.Providers
{
  public abstract class ProviderImplementation : Provider
  {
    private readonly RecordHeader header;

    protected sealed override RecordHeader BuildHeader()
    {
      return header;
    }


    // Constructor

    protected ProviderImplementation(RecordHeader header, params Provider[] sourceProviders)
      : base(sourceProviders)
    {
      this.header = header;
    }
  }
}