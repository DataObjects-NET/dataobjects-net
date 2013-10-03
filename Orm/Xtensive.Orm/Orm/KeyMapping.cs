// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Xtensive.Core;
using Xtensive.Collections;



namespace Xtensive.Orm
{
  /// <summary>
  /// Maps local ("disconnected") <see cref="Key"/> instances 
  /// to actual (storage) <see cref="Key"/> instances.
  /// </summary>
  [Serializable]
  public sealed class KeyMapping : ISerializable
  {
    private readonly IDictionary<Key, Key> map;

    /// <summary>
    /// Gets the key map.
    /// </summary>
    public ReadOnlyDictionary<Key, Key> Map { get; private set; }

    /// <summary>
    /// Tries to remaps the specified key;
    /// returns the original key, if there is no 
    /// remapped key in <see cref="Map"/> for it.
    /// </summary>
    /// <param name="key">The key to remap.</param>
    /// <returns>The mapped storage <see cref="Key"/>.</returns>
    public Key TryRemapKey(Key key)
    {
      Key remappedKey;
      if (key!=null && Map.TryGetValue(key, out remappedKey))
        return remappedKey;
      return key;
    }

    /// <summary>
    /// Remaps the keys of cached entities
    /// in the specified <paramref name="session"/>
    /// accordingly with this key mapping.
    /// </summary>
    /// <param name="session">The session to remap entity keys in.</param>
    public void RemapEntityKeys(Session session)
    {
      session.RemapEntityKeys(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return "{0}:\r\n".FormatWith(Strings.KeyMapping) + (
        from pair in map
        let pairKeyString = pair.Key.ToString()
        orderby pairKeyString
        select string.Format("  {0} => {1}", pairKeyString, pair.Value)
        ).ToDelimitedString(Environment.NewLine);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public KeyMapping(ReadOnlyDictionary<Key,Key> map)
    {
      this.map = map;
      Map = new ReadOnlyDictionary<Key, Key>(map, false);
    }

    // Serialization

    /// <inheritdoc/>
    [SecurityCritical]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = new Dictionary<Ref<Entity>, Ref<Entity>>();
      foreach (var pair in Map)
        serializedMapping.Add(pair.Key, pair.Value);

      info.AddValue("Map", serializedMapping, typeof(Dictionary<Ref<Entity>, Ref<Entity>>));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyMapping"/> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    protected KeyMapping(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = (Dictionary<Ref<Entity>, Ref<Entity>>)
        info.GetValue("Map", typeof(Dictionary<Ref<Entity>, Ref<Entity>>));
      map = new Dictionary<Key, Key>();
      foreach (var pair in serializedMapping)
        map.Add(pair.Key, pair.Value);
      Map = new ReadOnlyDictionary<Key, Key>(map, false);
    }
  }
}