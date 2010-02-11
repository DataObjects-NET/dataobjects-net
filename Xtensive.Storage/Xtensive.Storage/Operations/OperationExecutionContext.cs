// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation context for <see cref="IOperation.Execute"/> and <see cref="IOperation.Prepare"/> methods executed for a set of operations.
  /// </summary>
  public sealed class OperationExecutionContext
  {
    private readonly HashSet<Key> prefetchKeys;
    private readonly HashSet<Key> excludedKeys;
    public readonly Session Session;
    public readonly Dictionary<Key, Key> KeyMapping;
    public readonly ReadOnlyHashSet<Key> KeysToRemap;

    /// <summary>
    /// Remaps the key.
    /// </summary>
    /// <param name="key">The key to remap.</param>
    /// <returns>Remapped key</returns>
    public Key TryRemapKey(Key key)
    {
      if (key==null || !KeysToRemap.Contains(key))
        return key;
      Key remappedKey;
      if (KeyMapping.TryGetValue(key, out remappedKey))
        return remappedKey;
      else {
        var domain = Session.Domain;
        var typeInfo = key.TypeRef.Type;
        var generator = domain.KeyGenerators[typeInfo.KeyProviderInfo];
        if (generator.IsTemporaryKey(key.Value))
          // Only temporary keys are remapped
          remappedKey = KeyFactory.Generate(domain, key.Type);
        else
          remappedKey = key;
        KeyMapping.Add(key, remappedKey);
        return remappedKey;
      }
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

    /// <summary>
    /// Gets the sequence of keys to prefetch.
    /// </summary>
    public IEnumerable<Key> KeysToPrefetch {
      get { return prefetchKeys; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public OperationExecutionContext(Session session, IOperationSet operationSet)
    {
      Session = session;
      KeysToRemap = operationSet.NewKeys;
      KeyMapping = new Dictionary<Key, Key>();
      prefetchKeys = new HashSet<Key>();
      excludedKeys = new HashSet<Key>();
    }
  }
}