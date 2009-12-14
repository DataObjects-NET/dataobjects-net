// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.21

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation context for <see cref="IOperation.Execute"/> and <see cref="IOperation.Prepare"/> methods executed for a set of operations.
  /// </summary>
  public sealed class OperationExecutionContext : IEnumerable<Key>
  {
    private readonly HashSet<Key> prefetchKeys;
    private readonly HashSet<Key> excludedKeys;
    public readonly Session Session;
    public readonly Dictionary<Key, Key> KeyMapping;
    public readonly HashSet<Key> KeysForRemap;

    /// <summary>
    /// Registers the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    public void Register(Key key)
    {
      if (key == null)
        return;
      if (!excludedKeys.Contains(key))
        prefetchKeys.Add(key);
    }

    /// <summary>
    /// Registers new key.
    /// </summary>
    /// <param name="key">The key.</param>
    public void RegisterNew(Key key)
    {
      if (key == null)
        return;
      excludedKeys.Add(key);
    }

    /// <inheritdoc/>
    public IEnumerator<Key> GetEnumerator()
    {
      return prefetchKeys.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public OperationExecutionContext(Session session, IOperationSet set)
    {
      Session = session;
      KeysForRemap = set.GetKeysToRemap();
      KeyMapping = new Dictionary<Key, Key>();
      prefetchKeys = new HashSet<Key>();
      excludedKeys = new HashSet<Key>();
    }
  }
}