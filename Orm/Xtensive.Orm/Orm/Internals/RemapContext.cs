// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.04.07

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Orm.Internals
{
  internal sealed class RemapContext
  {
    private readonly EntityChangeRegistry registry;
    private readonly Dictionary<Key, Key> keyMap;

    /// <summary>
    /// Gets maps from local key to actual(storage) key.
    /// </summary>
    public KeyMapping KeyMapping {
      get { return new KeyMapping(keyMap); }
    }

    /// <summary>
    /// Gets entities which need to be remap.
    /// </summary>
    public IEnumerable<EntityState> EntitiesToRemap
    {
      get { return registry.GetItems(PersistenceState.New); }
    }

    /// <summary>
    /// Registers map from <paramref name="localKey"/> to <paramref name="realKey"/>.
    /// </summary>
    /// <param name="localKey">Temporary key</param>
    /// <param name="realKey">Actual key</param>
    public void RegisterKeyMap(Key localKey, Key realKey)
    {
      ArgumentValidator.EnsureArgumentNotNull(localKey, "localKey");
      ArgumentValidator.EnsureArgumentNotNull(realKey, "realKey");
      if(localKey!=realKey)
        keyMap.Add(localKey, realKey);
    }

    /// <summary>
    /// Finds actual key for local key.
    /// </summary>
    /// <param name="oldKey">Local key</param>
    /// <returns>Real key</returns>
    public Key TryRemapKey(Key oldKey)
    {
      if (oldKey==null)
        return oldKey;
      Key newKey;
      return keyMap.TryGetValue(oldKey, out newKey) ? newKey : oldKey;
    }

    public RemapContext(EntityChangeRegistry registry)
    {
      ArgumentValidator.EnsureArgumentNotNull(registry, "registry");
      this.registry = registry;
      keyMap = new Dictionary<Key, Key>();
    }
  }
}
