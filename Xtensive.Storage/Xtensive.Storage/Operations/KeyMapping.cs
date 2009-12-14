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
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Maps local ("disconnected") <see cref="Key"/> instances to actual (storage) <see cref="Key"/> instances.
  /// </summary>
  [Serializable]
  public sealed class KeyMapping : 
    IEnumerable<KeyValuePair<Key,Key>>,
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
      Key key;
      if (mapping.TryGetValue(localKey, out key))
        return key;
      else
        return localKey;
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<Key, Key>> GetEnumerator()
    {
      return mapping.GetEnumerator();
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
    public KeyMapping(Dictionary<Key,Key> mapping)
    {
      this.mapping = mapping;
    }

    // Serialization

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = new Dictionary<SerializableKey, SerializableKey>();
      foreach (var pair in mapping)
        serializedMapping.Add(pair.Key, pair.Value);

      info.AddValue("mapping", serializedMapping, typeof(Dictionary<SerializableKey,SerializableKey>));
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true"/>
    protected KeyMapping(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = (Dictionary<SerializableKey, SerializableKey>)
        info.GetValue("mapping", typeof(Dictionary<SerializableKey, SerializableKey>));
      mapping = new Dictionary<Key, Key>();
      foreach (var pair in serializedMapping)
        mapping.Add((Key) pair.Key, (Key) pair.Value);
    }
  }
}