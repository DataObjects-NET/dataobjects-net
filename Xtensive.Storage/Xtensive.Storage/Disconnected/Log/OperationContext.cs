// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Storage.Disconnected.Log
{
  public sealed class OperationContext : IEnumerable<Key>
  {
    private readonly HashSet<Key> prefetchKeys;
    private readonly HashSet<Key> excludedKeys;
    public readonly Session Session;
    public readonly Dictionary<Key, Key> KeyMapping;
    public readonly HashSet<Key> KeysForRemap;

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

    public OperationContext(Session session, IOperationSet set)
    {
      Session = session;
      KeysForRemap = set.GetKeysForRemap();
      KeyMapping = new Dictionary<Key, Key>();
      prefetchKeys = new HashSet<Key>();
      excludedKeys = new HashSet<Key>();
    }
  }
}