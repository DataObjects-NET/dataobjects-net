// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// This class is responsible for mapping between local <see cref="Key"/> instances into storage <see cref="Key"/> instances.
  /// </summary>
  [Serializable]
  public sealed class KeyMapping : IEnumerable<KeyValuePair<Key,Key>>,
                                   ISerializable
  {
    private readonly Dictionary<Key, Key> mapping;

    /// <summary>
    /// Remaps the specified local key.
    /// </summary>
    /// <param name="localKey">The local key.</param>
    /// <returns>The mapped storage <see cref="Key"/>.</returns>
    public Key Remap(Key localKey)
    {
      var key = (Key) null;
      return mapping.TryGetValue(localKey, out key) 
               ? key 
               : localKey;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Key, Key>> GetEnumerator()
    {
      return mapping.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public KeyMapping(Dictionary<Key,Key> mapping)
    {
      this.mapping = mapping;
    }

    // Serialization

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = new Dictionary<SerializableKey, SerializableKey>();
      foreach (var pair in mapping)
        serializedMapping.Add(pair.Key, pair.Value);

      info.AddValue("mapping", serializedMapping, typeof(Dictionary<SerializableKey,SerializableKey>));
    }

    protected KeyMapping(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = (Dictionary<SerializableKey, SerializableKey>)info.GetValue(
                                                                              "mapping", 
                                                                              typeof(Dictionary<SerializableKey, SerializableKey>));
      mapping = new Dictionary<Key, Key>();
      foreach (var pair in serializedMapping)
        mapping.Add((Key) pair.Key, (Key) pair.Value);
    }
  }
}