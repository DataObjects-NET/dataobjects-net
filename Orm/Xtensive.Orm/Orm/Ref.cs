// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.15

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using Xtensive.Reflection;


namespace Xtensive.Orm
{
  /// <summary>
  /// Typed reference to <see cref="Entity"/>.
  /// </summary>
  /// <typeparam name="T">The type of referenced object (<see cref="Value"/> property).</typeparam>
  [Serializable]
  [DebuggerDisplay("Key = {Key}")]
  public struct Ref<T> : 
    IEquatable<Ref<T>>,
    IEquatable<Key>,
    ISerializable
    where T : class, IEntity
  {
    private object keyOrString;

    /// <summary>
    /// Gets the key of the referenced entity.
    /// </summary>
    public Key Key {
      get {
        if (keyOrString==null)
          return null;
        var key = keyOrString as Key;
        if (key==null) {
          key = Key.Parse(Domain.Demand(), (string) keyOrString);
          keyOrString = key;
        }
        return key;
      }
    }

    /// <summary>
    /// Gets the formatted key of the referenced entity.
    /// Formatted key is the string produced with <see cref="Orm.Key.Format"/>.
    /// </summary>
    public string FormattedKey {
      get {
        if (keyOrString==null)
          return null;
        var key = keyOrString as Key;
        return key!=null ? key.Format() : (string) keyOrString;
      }
    }

    /// <summary>
    /// Gets the referenced entity (resolves the reference).
    /// </summary>
    public T Value {
      get { return Session.Demand().Query.Single<T>(Key); }
    }

    #region Equality members

    
    public bool Equals(Ref<T> other)
    {
      return other.Key==Key;
    }

    
    public bool Equals(Key other)
    {
      return other==Key;
    }

    
    public override bool Equals(object other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (other.GetType()==typeof (Ref<T>))
        return Equals((Ref<T>) other);
      var otherKey = other as Key;
      if (otherKey!=null)
        return Equals(otherKey);
      return false;
    }

    
    public override int GetHashCode()
    {
      return (Key!=null ? Key.GetHashCode() : 0);
    }

    #endregion

    
    public override string ToString()
    {
      var key = Key;
      return string.Format(Strings.RefFormat,
        typeof (T).GetShortName(),
        key==null ? Strings.Null : Key.ToString());
    }

    #region Cast operators

    /// <summary>
    /// Implicit conversion of <see cref="Key"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="key">Key of the entity to provide typed reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Ref<T>(Key key)
    {
      return new Ref<T>(key);
    }

    /// <summary>
    /// Implicit conversion of <see cref="IEntity"/> to <see cref="Ref{T}"/>.
    /// </summary>
    /// <param name="entity">The entity to provide typed reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Ref<T>(T entity)
    {
      return new Ref<T>(entity);
    }

    /// <summary>
    /// Implicit conversion of <see cref="Ref{T}"/> to <see cref="Key"/>.
    /// </summary>
    /// <param name="reference">The typed reference to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Key(Ref<T> reference)
    {
      return reference.Key;
    }

    /// <summary>
    /// Implicit conversion of <see cref="Ref{T}"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reference">The typed reference to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator T(Ref<T> reference)
    {
      return reference.Value;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="key">The key of entity this reference points to.</param>
    public Ref(Key key)
    {
      keyOrString = key;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="formattedKey">The formatted key of entity this reference points to.</param>
    public Ref(string formattedKey)
    {
      keyOrString = formattedKey;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entity">The entity this reference points to.</param>
    public Ref(T entity)
    {
      if (entity==null)
        keyOrString = null;
      else
        keyOrString = entity.Key;
    }

    #region ISerializable members

    private Ref(SerializationInfo info, StreamingContext context)
    {
      keyOrString = info.GetString("FormattedKey");
    }
    
    /// <summary>
    /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
    /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
    /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("FormattedKey", FormattedKey);
    }

    #endregion
  }
}