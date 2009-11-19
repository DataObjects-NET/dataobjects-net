// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Xtensive.Storage
{
  /// <summary>
  /// Serializable wrapper for <see cref="Key"/>.
  /// </summary>
  [Serializable]
  public sealed class SerializableKey : 
    ISerializable, 
    IEquatable<Key>,
    IEquatable<SerializableKey>
  {
    /// <summary>
    /// Gets the key.
    /// </summary>
    /// <value>The key.</value>
    public Key Key { get; private set; }

    /// <inheritdoc/>
    public bool Equals(Key other)
    {
      return Key.Equals(other);
    }

    /// <inheritdoc/>
    public bool Equals(SerializableKey other)
    {
      return Key.Equals((Key)other);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      var sk = obj as SerializableKey;
      if (sk != null)
        return Equals(sk);
      var key = obj as Key;
      return key != null && Equals(key);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Key.GetHashCode();
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Xtensive.Storage.Key"/> to <see cref="Xtensive.Storage.SerializableKey"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator SerializableKey(Key key)
    {
      return new SerializableKey(key);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Xtensive.Storage.SerializableKey"/> to <see cref="Xtensive.Storage.Key"/>.
    /// </summary>
    /// <param name="serializableKey">The serializable key.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Key(SerializableKey serializableKey)
    {
      return serializableKey.Key;
    }

    
    // Constructors

    private SerializableKey(Key key)
    {
      Key = key;
    }

    // Serialization

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("key", Key.Format());
    }

    protected SerializableKey(SerializationInfo info, StreamingContext context)
    {
      Key = Key.Parse(info.GetString("key"));
    }
  }
}