// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Orm.Services;
using Xtensive.Orm.Operations.Interfaces;


namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Operation context for <see cref="IExecutableOperation.Execute"/> and
  /// <see cref="IExecutableOperation.Prepare"/> methods executed for a set of operations.
  /// </summary>
  public sealed class OperationExecutionContext
  {
    private readonly HashSet<Key> prefetchKeys = new();
    private readonly HashSet<Key> excludedKeys = new();
    private readonly Dictionary<Key, Key> keyMapping = new();

    /// <summary>
    /// The session this instance is bound to.
    /// </summary>
    public readonly Session Session;

    /// <summary>
    /// The mapping for new keys.
    /// </summary>
    public readonly ReadOnlyDictionary<Key, Key> KeyMapping;

    public readonly DirectPersistentAccessor PersistentAccessor;
    public readonly DirectEntityAccessor EntityAccessor;
    public readonly DirectEntitySetAccessor EntitySetAccessor;

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
      if (key is null)
        return key;
      return KeyMapping.TryGetValue(key, out var remappedKey) ? remappedKey : key;
    }

    /// <summary>
    /// Registers the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="isNew">if set to <see langword="true"/>, the key is new.</param>
    public void RegisterKey(Key key, bool isNew)
    {
      if (key is null)
        return;
      if (isNew)
        _ = excludedKeys.Add(key);
      else if (!excludedKeys.Contains(key))
        _ = prefetchKeys.Add(key);
    }

    internal void AddKeyMapping(Key localKey, Key realKey)
    {
      if (localKey != realKey)
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

      EntityAccessor = session.Services.Get<DirectEntityAccessor>();
      PersistentAccessor = session.Services.Get<DirectPersistentAccessor>();
      EntitySetAccessor = session.Services.Get<DirectEntitySetAccessor>();
    }
  }
}