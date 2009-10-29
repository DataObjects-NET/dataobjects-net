// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Storage.Disconnected.Log
{
  public sealed class PrefetchContext : IEnumerable<Key>
  {
    private readonly HashSet<Key> prefetchKeys;
    private readonly HashSet<Key> excludedKeys;

    public void Register(Key key)
    {
      if (key == null)
        return;
      if (!excludedKeys.Contains(key))
        prefetchKeys.Add(key);
    }

    public void RegisterNew(Key key)
    {
      if (key == null)
        return;
      excludedKeys.Add(key);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<Key> GetEnumerator()
    {
      return prefetchKeys.GetEnumerator();
    }


    // Constructors

    public PrefetchContext()
    {
      prefetchKeys = new HashSet<Key>();
      excludedKeys = new HashSet<Key>();
    }
  }
}