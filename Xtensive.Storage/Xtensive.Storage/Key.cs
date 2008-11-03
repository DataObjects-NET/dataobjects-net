// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Diagnostics;
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
  public sealed class Key : Tuple,
    IEquatable<Key>
  {
    private TypeInfo type;
    private readonly int hashCode;
    private readonly Tuple tuple;

    /// <summary>
    /// Gets the hierarchy this instance belongs to.
    /// </summary>
    public HierarchyInfo Hierarchy { get; private set; }

    /// <summary>
    /// Gets the type of <see cref="Entity"/> this instance identifies.
    /// </summary>
    /// <exception cref="NotSupportedException">Type is already initialized.</exception>
    /// <exception cref="InvalidOperationException">Unable to resolve type for Key.</exception>
    public TypeInfo Type
    {
      [DebuggerStepThrough]
      get {
        if (type == null) {
          var session = Session.Current;
          if (session!=null) {
            var domain = session.Domain;
            var keyCache = domain.KeyCache;
            Key cachedKey;
            lock (keyCache)
              keyCache.TryGetItem(this, true, out cachedKey);
            if (cachedKey == null) {
              if (session.IsDebugEventLoggingEnabled)
                Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required.", session, this);

              var field = Hierarchy.Root.Fields[domain.NameBuilder.TypeIdFieldName];
              cachedKey = Fetcher.Fetch(this, field);
              if (cachedKey == null)
                throw new InvalidOperationException(string.Format("Unable to resolve type for Key '{0}'.", this));
            }
            type = cachedKey.type;
          }
        }
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
      var entityCache = session.Cache;
      var state = entityCache[this];
      bool hasBeenFetched = false;

      if (state==null) {
        if (session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is {0}.", session, this,
            IsTypeCached ? "known" : "unknown");
        Fetcher.Fetch(this);
        state = entityCache[this];
        hasBeenFetched = true;
      }

      if (!hasBeenFetched && session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved.", session, this);
      
      state.EnsureIsActual();
      if (state.IsRemoved)
        return null;

      state.EnsureHasEntity();
      return state.Entity;
    }

    #region Tuple methods

    /// <inheritdoc/>
    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      return tuple.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public override object GetValueOrDefault(int fieldIndex)
    {
      return tuple.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc/>
    /// <exception cref="Exception">Instance is read-only.</exception>
    public override void SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    /// <inheritdoc/>
    public override TupleDescriptor Descriptor
    {
      get { return tuple.Descriptor; }
    }

    #endregion

    #region Equals, GetHashCode, ==, != 

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(Key other)
    {
      if (other==null)
        return false;
      if (hashCode!=other.hashCode)
        return false;
      if (Hierarchy!=other.Hierarchy)
        return false;
      return tuple.Equals(other.tuple);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (obj.GetType()!=typeof(Key))
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

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}, {1}", (Type != null) ? Type.Name : Hierarchy.Name, tuple.ToRegular());
    }

    #region Create methods

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified <paramref name="tuple"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="tuple"><see cref="Tuple"/> with key values.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public static Key Create<T>(Tuple tuple)
      where T: Entity
    {
      return new Key(typeof (T), tuple);
    }

    /// <summary>
    /// Builds the <see cref="Key"/> according to specified key value.
    /// </summary
    /// <typeparam name="TEntity">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <typeparam name="TKey">Key value type.</typeparam>
    /// <param name="keyValue">Key value.</param>
    /// <returns>Newly created <see cref="Key"/> instance.</returns>
    public static Key Create<TEntity, TKey>(TKey keyValue)
      where TEntity: Entity
    {
      return new Key(typeof(TEntity), Create(keyValue));
    }

    #endregion


    // Constructors

    ///<summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    ///</summary>
    ///<param name="hierarchy">Hierarchy value</param>
    ///<param name="tuple">Tuple value</param>
    public Key(HierarchyInfo hierarchy, Tuple tuple)
    {
      Hierarchy = hierarchy;
      this.tuple = Create(hierarchy.KeyTupleDescriptor);
      tuple.CopyTo(this.tuple, 0, this.tuple.Count);
      hashCode = this.tuple.GetHashCode() ^ hierarchy.GetHashCode();
    }

    ///<summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    ///</summary>
    ///<param name="type">Type value</param>
    ///<param name="tuple">Tuple value</param>
    public Key(Type type, Tuple tuple)
      :this(Domain.Current.Model.Types[type].Hierarchy, tuple)
    {
    }

    internal Key(TypeInfo type, Tuple tuple)
      : this(type.Hierarchy, tuple)
    {
      this.type = type;
    }
  }
}