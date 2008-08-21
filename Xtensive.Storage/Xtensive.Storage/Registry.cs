// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.21

using System;
using System.Collections.Generic;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage
{
  public class Registry<TKey, TValue> : LockableBase,
    IDisposable
  {
    private readonly object _lock = new object();
    private bool isDisposed;
    private readonly Dictionary<TKey, TValue> providers = new Dictionary<TKey, TValue>();

    public TValue this[TKey key]
    {
      get
      {
        TValue value;
        if (providers.TryGetValue(key, out value))
          return value;
        return default(TValue);
      }
    }

    public void Register(TKey key, TValue value)
    {
      providers.Add(key, value);
    }

    public void Dispose()
    {
      if (!isDisposed) lock (_lock) if (!isDisposed) {
        try {
          foreach (var pair in providers) {
            var disposable = pair.Value as IDisposable;
            disposable.DisposeSafely();
          }
        }
        finally {
          isDisposed = true;
        }
      }
    }
  }
}