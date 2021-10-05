// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Operation context for <see cref="IOperation.Execute"/> and
  /// <see cref="IOperation.Prepare"/> methods executed for a set of operations.
  /// </summary>
  public sealed class OperationExecutionContext
  {
    private readonly HashSet<Key> prefetchKeys;
    private readonly HashSet<Key> excludedKeys;
    private readonly Dictionary<Key, Key> keyMapping = new Dictionary<Key, Key>();

    /// <summary>
    /// The session this instance is bound to.
    /// </summary>
    public readonly Session Session;

    /// <summary>
    /// The mapping for new keys.
    /// </summary>
    public readonly ReadOnlyDictionary<Key, Key> KeyMapping;

    /// <summary>
    /// Gets the sequence of keys to prefetch.
    /// </summary>
    public IEnumerable<Key> KeysToPrefetch { get { return prefetchKeys; } }

    /// <summary>
    /// Remaps the key.
    /// </summary>
    /// <param name="key">The key to remap.</param>
    /// <returns>Remapped key</returns>
    public Key TryRemapKey(Key key)
    {
      if (key==null)
        return key;
      Key remappedKey;
      return KeyMapping.TryGetValue(key, out remappedKey) ? remappedKey : key;
    }

    /// <summary>
    /// Registers the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="isNew">if set to <see langword="true"/>, the key is new.</param>
    public void RegisterKey(Key key, bool isNew)
    {
      if (key == null)
        return;
      if (isNew)
        excludedKeys.Add(key);
      else
        if (!excludedKeys.Contains(key))
          prefetchKeys.Add(key);
    }

    internal void AddKeyMapping(Key localKey, Key realKey)
    {
      if (localKey!=realKey)
        keyMapping.Add(localKey, realKey);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public OperationExecutionContext(Session session)
    {
      Session = session;
      KeyMapping = new ReadOnlyDictionary<Key, Key>(keyMapping);
      prefetchKeys = new HashSet<Key>();
      excludedKeys = new HashSet<Key>();
    }
  }
}