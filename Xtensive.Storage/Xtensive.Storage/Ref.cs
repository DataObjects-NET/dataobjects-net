// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.15

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Typed reference to <see cref="Entity"/>.
  /// </summary>
  /// <typeparam name="T">The type of referenced object (<see cref="Value"/> property).</typeparam>
  [Serializable]
  [DebuggerDisplay("Key = {Key}")]
  public struct Ref<T> : IEquatable<Ref<T>>,
    ISerializable
    where T : class, IEntity
  {
    private readonly Key key;

    /// <summary>
    /// Gets the key of the referenced entity.
    /// </summary>
    public Key Key {
      get { return key; }
    }

    /// <summary>
    /// Gets the referenced entity (resolves the reference).
    /// </summary>
    public T Value {
      get { return Query.Single<T>(key); }
    }

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(Ref<T> other)
    {
      return other.Key==Key;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (obj.GetType()!=typeof (Ref<T>))
        return false;
      return Equals((Ref<T>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return (Key!=null ? Key.GetHashCode() : 0);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.RefFormat,
        typeof (T).GetShortName(),
        key==null ? Strings.Null : key.ToString());
    }

    #region Explicit cast operators

    /// <summary>
    /// Explicit conversion of <see cref="Key"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="key">Key of the entity to provide typed reference for.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Ref<T>(Key key)
    {
      return new Ref<T>(key);
    }

    /// <summary>
    /// Explicit conversion of <see cref="Entity"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="entity">The entity to provide typed reference for.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator Ref<T>(T entity)
    {
      return new Ref<T>(entity);
    }

    #endregion


    // Constructors

    private Ref(Key key)
    {
      this.key = key;
    }

    private Ref(T entity)
    {
      if (entity==null)
        key = null;
      else
        key = entity.Key;
    }

    #region ISerializable members

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    private Ref(SerializationInfo info, StreamingContext context)
    {
      key = Key.Parse(info.GetString("Key"));
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true" />
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Key", key.Format());
    }

    #endregion
  }
}