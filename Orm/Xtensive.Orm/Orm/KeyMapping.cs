// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core;


namespace Xtensive.Orm
{
  /// <summary>
  /// Maps local ("disconnected") <see cref="Key"/> instances 
  /// to actual (storage) <see cref="Key"/> instances.
  /// </summary>
  [Serializable]
  public sealed class KeyMapping : ISerializable
  {
    /// <summary>
    /// Gets the key map.
    /// </summary>
    public IReadOnlyDictionary<Key, Key> Map { get; }

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
      return $"{Strings.KeyMapping}:\r\n" + (
        from pair in Map
        let pairKeyString = pair.Key.ToString()
        orderby pairKeyString
        select $"  {pairKeyString} => {pair.Value}"
        ).ToDelimitedString(Environment.NewLine);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public KeyMapping(IDictionary<Key,Key> map)
    {
      Map = new ReadOnlyDictionary<Key, Key>(map);
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
    private KeyMapping(SerializationInfo info, StreamingContext context)
    {
      var serializedMapping = (Dictionary<Ref<Entity>, Ref<Entity>>)
        info.GetValue("Map", typeof(Dictionary<Ref<Entity>, Ref<Entity>>));
      var map = new Dictionary<Key, Key>();
      foreach (var pair in serializedMapping)
        map.Add(pair.Key, pair.Value);
      Map = new ReadOnlyDictionary<Key, Key>(map);
    }
  }
}