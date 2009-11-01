// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.15

using System;

namespace Xtensive.Storage.Rse
{
  public struct ProviderOptionsStruct
  {
    private readonly ProviderOptions options;

    public ProviderOptions Internal
    {
      get { return options; }
    }

    public bool IsIndexed
    {
      get { return (options & ProviderOptions.Indexed) != 0; }
    }

    public bool IsOrdered
    {
      get { return (options & ProviderOptions.Ordered) != 0; }
    }

    public bool IsCountAvailable
    {
      get { return (options & ProviderOptions.FastCount) != 0; }
    }

    public bool RandomAccess
    {
      get { return (options & ProviderOptions.RandomAccess) != 0; }
    }

    public bool StreamingEnumeration
    {
      get { return (options & ProviderOptions.FastFirst) != 0; }
    }

    public static implicit operator ProviderOptionsStruct(ProviderOptions options)
    {
      return new ProviderOptionsStruct(options);
    }


    // Constructor

    public ProviderOptionsStruct(ProviderOptions options)
    {
      this.options = options;
    }
  }
}