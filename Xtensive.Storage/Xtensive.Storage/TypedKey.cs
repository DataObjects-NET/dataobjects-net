// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.15

using System;

namespace Xtensive.Storage
{
  [Serializable]
  public sealed class TypedKey<T> : IEquatable<TypedKey<T>>
    where T : Entity
  {
    public Key Key { get; private set; }

    public T Entity
    {
      get { return Query.Single<T>(Key); }
    }

    #region Equiatable members

    /// <inheritdoc/>
    public bool Equals(TypedKey<T> other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Key, Key);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (TypedKey<T>))
        return false;
      return Equals((TypedKey<T>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return (Key!=null ? Key.GetHashCode() : 0);
    }

    #endregion

    /// <summary>
    /// Explicit conversion of <see cref="Key"/> to <see cref="TypedKey{T}"/>.
    /// </summary>
    /// <param name="key">Key to provide the typed key for.</param>
    public static explicit operator TypedKey<T>(Key key)
    {
      return new TypedKey<T>(key);
    }

    /// <summary>
    /// Implicit conversion of <see cref="TypedKey{T}"/> to <see cref="Key"/>.
    /// </summary>
    /// <param name="typedKey">Typed key to provide the key for.</param>
    public static implicit operator Key(TypedKey<T> typedKey)
    {
      return typedKey==null ? null : typedKey.Key;
    }


    // Constructors

    private TypedKey(Key key)
    {
      Key = key;
    }
  }
}