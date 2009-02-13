// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains a set of identifying values of an <see cref="Entity"/>.
  /// Every entity is uniquely identified by its <see cref="Key"/>.
  /// </summary>
  [Serializable]
  public sealed class Key : IEquatable<Key>
  {
    private readonly HierarchyInfo hierarchy;
    private Tuple value;
    private readonly int hashCode;
    private TypeInfo type;
    private string cachedFormatResult;

    /// <summary>
    /// Gets the hierarchy this instance belongs to.
    /// </summary>
    public HierarchyInfo Hierarchy
    {
      get { return hierarchy; }
    }

    /// <summary>
    /// Gets the key value.
    /// </summary>
    public Tuple Value {
      get { return value; }
    }

    /// <summary>
    /// Gets the type of <see cref="Entity"/> this instance identifies.
    /// </summary>
    /// <exception cref="NotSupportedException">Type is already initialized.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve type for Key.</exception>
    public TypeInfo Type {
      get {
        if (type!=null)
          return type;

        var session = Session.Current;
        if (session==null)
          return null;

        var domain = session.Domain;
        var keyCache = domain.KeyCache;
        Key cachedKey;
        lock (keyCache)
          keyCache.TryGetItem(this, true, out cachedKey);
        if (cachedKey==null) {
          if (session.IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required.", session, this);

          var field = Hierarchy.Root.Fields[domain.NameBuilder.TypeIdFieldName];
          cachedKey = Fetcher.Fetch(this, field);
          if (cachedKey==null)
            throw new InvalidOperationException(string.Format("Unable to resolve type for Key '{0}'.", this));
        }
        type = cachedKey.type;
        return type;
      }
    }

    /// <summary>
    /// Determines whether <see cref="Type"/> property has cached type value or not.
    /// </summary>
    public bool IsTypeCached {
      get { return type!=null ? true : false; }
    }

    /// <summary>
    /// Resolves this key.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to resolve this instance to.</typeparam>
    /// <returns>The <see cref="Entity"/> this key belongs to.</returns>
    public T Resolve<T>()
      where T : Entity
    {
      return (T) Resolve();
    }

    /// <summary>
    /// Resolves this key.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to resolve this instance to.</typeparam>
    /// <param name="session">The session to resolve the key in.</param>
    /// <returns>The <see cref="Entity"/> this key belongs to.</returns>
    public T Resolve<T>(Session session)
      where T : Entity
    {
      return (T) Resolve(session);
    }

    /// <summary>
    /// Resolves this instance.
    /// </summary>
    /// <returns>The <see cref="Entity"/> this key belongs to.</returns>
    public Entity Resolve()
    {
      var session = Session.Current;
      if (session==null)
        throw new InvalidOperationException(Strings.ExNoCurrentSession);
      return Resolve(session);
    }

    /// <summary>
    /// Resolves this instance.
    /// </summary>
    /// <param name="session">The session to resolve the key in.</param>
    /// <returns>The <see cref="Entity"/> this key belongs to.</returns>
    public Entity Resolve(Session session)
    {
      var cache = session.EntityStateCache;
      var state = cache[this, true];
      bool hasBeenFetched = false;

      if (state==null) {
        if (session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is {0}.", session, this,
            IsTypeCached ? "known" : "unknown");
        Fetcher.Fetch(this);
        state = cache[this, true];
        hasBeenFetched = true;
      }

      if (!hasBeenFetched && session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved.", session, this);

      if (state.IsRemoved)
        return null;

      return state.Entity;
    }

    #region Equals, GetHashCode, ==, != 

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(Key other)
    {
      if (ReferenceEquals(other, null))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (hashCode!=other.hashCode)
        return false;
      if (Hierarchy!=other.Hierarchy)
        return false;
      return value.Equals(other.value);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (obj.GetType()!=typeof (Key))
        return false;
      return Equals(obj as Key);
    }

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator ==(Key left, Key right)
    {
      return Equals(left, right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    [DebuggerStepThrough]
    public static bool operator !=(Key left, Key right)
    {
      return !Equals(left, right);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      return hashCode;
    }

    #endregion

    #region ToString(bool), Format, Parse methods

    /// <summary>
    /// Converts the <see cref="Key"/> to its string representation.
    /// </summary>
    /// <param name="format">Indicates whether to use <see cref="Format"/>,
    /// or <see cref="ToString()"/> method.</param>
    /// <returns>String representation of the <see cref="Key"/>.</returns>
    public string ToString(bool format)
    {
      return format ? Format() : ToString();
    }

    /// <summary>
    /// Gets the string representation of this instance.
    /// </summary>
    public string Format()
    {
      if (cachedFormatResult==null) {
        var builder = new StringBuilder();
        builder.Append(hierarchy.Root.TypeId);
        builder.Append(":");
        builder.Append(value.Format());
        cachedFormatResult = builder.ToString();
      }
      return cachedFormatResult;
    }

    /// <summary>
    /// Parses the specified <paramref name="source"/> string 
    /// produced by <see cref="Format"/> back to the <see cref="Key"/>
    /// instance.
    /// </summary>
    /// <param name="source">The string to parse.</param>
    /// <returns><see cref="Key"/> instance corresponding to the specified
    /// <paramref name="source"/> string.</returns>
    public static Key Parse(string source)
    {
      if (source==null)
        return null;
      int separatorIndex = source.IndexOf(':');
      if (separatorIndex<0)
        throw new InvalidOperationException(Strings.ExInvalidKeyString);

      string typeIdString = source.Substring(0, separatorIndex);
      string valueString = source.Substring(separatorIndex+1);

      var domain = Domain.Current;
      var type = domain.Model.Types[Int32.Parse(typeIdString)];
      var keyTupleDescriptor = type.Hierarchy.KeyInfo.TupleDescriptor;
      
      return Create(domain, type, keyTupleDescriptor.Parse(valueString), false, false);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}, {1}",
        IsTypeCached ? Type.Name : Hierarchy.Name,
        value.ToRegular());
    }

    #region Create methods

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// with newly generated value.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create<T>()
      where T : Entity
    {
      return Create(typeof (T));
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// with newly generated value.
    /// </summary>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(Type type)
    {
      var domain = Domain.Current;
      var typeInfo = domain.Model.Types[type];
      var keyGenerator = domain.KeyGenerators[typeInfo.Hierarchy.GeneratorInfo];
      return Create(domain, typeInfo, keyGenerator.Next(), true, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// with newly generated value.
    /// </summary>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type)
    {
      var domain = Domain.Current;
      var keyGenerator = domain.KeyGenerators[type.Hierarchy.GeneratorInfo];
      return Create(domain, type, keyGenerator.Next(), true, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="value">Key value.</param>
    /// <returns>
    /// A newly created or existing <see cref="Key"/> instance .
    /// </returns>
    public static Key Create<T>(Tuple value)
      where T : Entity
    {
      return Create(typeof (T), value, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="value">Key value.</param>
    /// <param name="exactType">Specified whether type is known exactly or not.</param>
    /// <returns>
    /// A newly created or existing <see cref="Key"/> instance .
    /// </returns>
    public static Key Create<T>(Tuple value, bool exactType)
      where T : Entity
    {
      return Create(typeof (T), value, exactType);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Key value.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(Type type, Tuple value)
    {
      var domain = Domain.Current;
      return Create(domain, domain.Model.Types[type], value, false, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Key value.</param>
    /// <param name="exactType">Specified whether type is known exactly or not.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(Type type, Tuple value, bool exactType)
    {
      var domain = Domain.Current;
      return Create(domain, domain.Model.Types[type], value, exactType, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Key value.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type, Tuple value)
    {
      return Create(Domain.Current, type, value, false, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Key value.</param>
    /// <param name="exactType">Specified whether type is known exactly or not.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type, Tuple value, bool exactType)
    {
      return Create(Domain.Current, type, value, exactType, false);
    }

    /// <exception cref="ArgumentException">Wrong key structure for the specified <paramref name="type"/>.</exception>
    internal static Key Create(Domain domain, TypeInfo type, Tuple value, bool exactType, bool canCache)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      var hierarchy = type.Hierarchy;
      if (value.Descriptor!=hierarchy.KeyInfo.TupleDescriptor)
        throw new ArgumentException(Strings.ExWrongKeyStructure);
      var key = new Key(type.Hierarchy, exactType ? type : null, value);
      if (!canCache || domain==null) {
        key.value = value.ToFastReadOnly();
        return key;
      }
      var keyCache = domain.KeyCache;
      lock (keyCache) {
        Key foundKey;
        if (keyCache.TryGetItem(key, true, out foundKey))
          key = foundKey;
        else {
          key.value = value.ToFastReadOnly();
          if (exactType)
            keyCache.Add(key);
        }
      }
      return key;
    }

    #endregion


    // Constructors

    private Key(HierarchyInfo hierarchy, TypeInfo type, Tuple value)
    {
      this.hierarchy = hierarchy;
      this.type = type;
      this.value = value;
      hashCode = value.GetHashCode() ^ hierarchy.GetHashCode();
    }
  }
}
