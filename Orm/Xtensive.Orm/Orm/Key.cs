// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Identifies a particular <see cref="Entity"/>.
  /// Stores the set of <see cref="Entity"/>'s <see cref="KeyAttribute">[Key]</see> field values, 
  /// as well as <see cref="HierarchyInfo"/> the entity belongs to.
  /// </summary>
  /// <remarks>
  /// Every entity is uniquely identified by its <see cref="Entity.Key"/>.
  /// </remarks>
  /// <seealso cref="Entity.Key"/>
  public abstract class Key : IEquatable<Key>
  {
    private const char KeyFormatEscape = '\\';
    private const char KeyFormatDelimiter = ':';

    /// <summary>
    /// Protected member caching the tuple with key values.
    /// Can be <see langword="null" />, if the value isn't materialized yet.
    /// </summary>
    protected Tuple value;

    /// <summary>
    /// Gets the key value.
    /// </summary>
    public Tuple Value
    {
      get
      {
        if (value==null)
          value = GetValue();
        return value;
      }
    }

    /// <summary>
    /// Gets the <see cref="TypeReference"/> object
    /// describing the type this key belongs to.
    /// </summary>
    public TypeReference TypeReference { get; internal set; }

    /// <summary>
    /// Gets node identifier for this instance.
    /// </summary>
    public string NodeId { get; private set; }

    /// <summary>
    /// Gets the type of <see cref="Entity"/> this instance identifies.
    /// </summary>
    [CanBeNull]
    public TypeInfo TypeInfo
    {
      get
      {
        return TypeReference.Accuracy==TypeReferenceAccuracy.ExactType ? TypeReference.Type : null;
      }
    }

    /// <summary>
    /// Determines whether this key is a temporary key in the specified <paramref name="domain"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>Check result.</returns>
    public bool IsTemporary(Domain domain)
    {
      var keyInfo = TypeReference.Type.Key;
      var keyGenerator = domain.KeyGenerators.GetTemporary(keyInfo);
      return keyGenerator!=null && keyGenerator.IsTemporaryKey(Value);
    }

    /// <summary>
    /// Resolves the type of <see cref="Entity"/> this instance identifies.
    /// </summary>
    /// <exception cref="InvalidOperationException">Unable to resolve type for Key.</exception>
    /// <param name="session">Session to use for resolving</param>
    /// <returns><see cref="TypeInfo"/> for this instance.</returns>
    [NotNull]
    public TypeInfo ResolveTypeInfo([NotNull, InstantHandle] Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");

      if (TypeReference.Accuracy==TypeReferenceAccuracy.ExactType)
        return TypeReference.Type;

      var domain = session.Domain;
      var keyCache = domain.KeyCache;
      Key cachedKey;

      lock (keyCache)
        keyCache.TryGetItem(this, true, out cachedKey);
      if (cachedKey!=null) {
        TypeReference = cachedKey.TypeReference;
        return TypeReference.Type;
      }

      var hierarchy = TypeReference.Type.Hierarchy;
      if (hierarchy!=null && hierarchy.Types.Count==1) {
        TypeReference = new TypeReference(hierarchy.Types[0], TypeReferenceAccuracy.ExactType);
        return TypeReference.Type;
      }

      if (IsTemporary(domain))
        return TypeReference.Type;

      OrmLog.Debug(Strings.LogSessionXResolvingKeyYExactTypeIsUnknownFetchIsRequired, session, this);

      var entityState = session.Handler.FetchEntityState(this);
      if (entityState==null || entityState.IsNotAvailableOrMarkedAsRemoved)
        throw new InvalidOperationException(string.Format(Strings.ExUnableToResolveTypeForKeyX, this));
      TypeReference = new TypeReference(entityState.Type, TypeReferenceAccuracy.ExactType);
      return TypeReference.Type;
    }

    /// <summary>
    /// Gets the value in form of <see cref="Tuple"/>.
    /// </summary>
    protected abstract Tuple GetValue();

    /// <summary>
    /// Determines whether <see cref="TypeInfo"/> property has exact type value or not.
    /// </summary>
    internal bool HasExactType
    {
      get { return TypeReference.Accuracy==TypeReferenceAccuracy.ExactType; }
    }

    internal Tuple CreateTuple()
    {
      var descriptor = TypeReference.Type.Key.TupleDescriptor;
      return Tuple.Create(descriptor);
    }

    #region Equals, GetHashCode, ==, != 

    /// <inheritdoc/>
    public bool Equals(Key other)
    {
      if (ReferenceEquals(other, null))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      var thisType = TypeReference.Type;
      var otherType = other.TypeReference.Type;
      if (HasExactType && other.HasExactType && thisType!=otherType)
        return false;
      if (!thisType.IsInterface && !otherType.IsInterface && thisType.Hierarchy!=otherType.Hierarchy)
        return false;
      if (thisType.Key.EqualityIdentifier!=otherType.Key.EqualityIdentifier)
        return false;
      if (NodeId!=other.NodeId)
        return false;
      if (other.GetType().IsGenericType)
        return other.ValueEquals(this);
      return ValueEquals(other);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return Equals(obj as Key);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    [DebuggerStepThrough]
    public static bool operator ==(Key left, Key right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    [DebuggerStepThrough]
    public static bool operator !=(Key left, Key right)
    {
      return !Equals(left, right);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      var result = CalculateHashCode();
      result = result * Tuple.HashCodeMultiplier ^ NodeId.GetHashCode();
      result = result * Tuple.HashCodeMultiplier ^ TypeReference.Type.Key.EqualityIdentifier.GetHashCode();
      return result;
    }

    /// <summary>
    /// Compares key value for equality.
    /// </summary>
    /// <param name="other">The other key to compare.</param>
    /// <returns>Equality comparison result.</returns>
    protected abstract bool ValueEquals(Key other);

    /// <summary>
    /// Calculates hash code.
    /// </summary>
    /// <returns>Calculated hash code.</returns>
    protected abstract int CalculateHashCode();

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
      var items = NodeId==WellKnown.DefaultNodeId
        ? new[] {Value.Format(), TypeReference.Type.UnderlyingType.FullName}
        : new[] {Value.Format(), TypeReference.Type.UnderlyingType.FullName, NodeId};
      return items.RevertibleJoin(KeyFormatEscape, KeyFormatDelimiter);
    }

    /// <summary>
    /// Parses the specified <paramref name="source"/> string
    /// produced by <see cref="Format"/> back to the <see cref="Key"/>
    /// instance.
    /// </summary>
    /// <param name="source">The string to parse.</param>
    /// <param name="domain">The domain.</param>
    /// <returns>
    /// <see cref="Key"/> instance corresponding to the specified
    /// <paramref name="source"/> string.
    /// </returns>
    /// <exception cref="InvalidOperationException">Invalid key format.</exception>
    public static Key Parse([NotNull] Domain domain, string source)
    {
      if (source==null)
        return null;

      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");

      var parts = source.RevertibleSplit(KeyFormatEscape, KeyFormatDelimiter).ToList();
      if (parts.Count!=2 && parts.Count!=3 || parts.Contains(null))
        throw new InvalidOperationException(Strings.ExInvalidKeyString);

      var valueString = parts[0];
      var typeName = parts[1];
      var nodeId = parts.Count==3 ? parts[2] : WellKnown.DefaultNodeId;

      var typeInfo = domain.Model.Types.Find(typeName);
      if (typeInfo==null)
        throw new InvalidOperationException(string.Format(Strings.ExTypeWithNameXIsNotRegistered, typeName));

      var value = typeInfo.Key.TupleDescriptor.Parse(valueString);
      return Create(domain, nodeId, typeInfo, TypeReferenceAccuracy.Hierarchy, value);
    }

    #endregion

    #region ToString() method

    /// <inheritdoc/>
    public override string ToString()
    {
      if (TypeInfo!=null)
        return string.Format(
          Strings.KeyFormat,
          TypeInfo.UnderlyingType.GetShortName(),
          Value.ToRegular());

      return string.Format(
        Strings.KeyFormatUnknownKeyType,
        TypeReference.Type.UnderlyingType.GetShortName(),
        Value.ToRegular());
    }

    #endregion

    #region Generate key methods

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// with newly generated value.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="session">The session.</param>
    /// <returns>
    /// A newly created <see cref="Key"/> instance .
    /// </returns>
    public static Key Generate<T>([NotNull] Session session)
      where T : Entity
    {
      return Generate(session, typeof (T));
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// with newly generated value.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="type">The type.</param>
    /// <returns>
    /// A newly created <see cref="Key"/> instance .
    /// </returns>
    public static Key Generate([NotNull] Session session, [NotNull] Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      return Generate(session, session.Domain.Model.Types[type]);
    }

    internal static Key Generate(Session session, TypeInfo type)
    {
      return KeyFactory.Generate(session, type);
    }

    #endregion

    #region Create key by tuple methods

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="domain">Domain to use.</param>
    /// <param name="value">Key value.</param>
    /// <returns>
    /// A newly created or existing <see cref="Key"/> instance.
    /// </returns>
    public static Key Create<T>([NotNull] Domain domain, [NotNull] Tuple value)
      where T : IEntity
    {
      return Create(domain, WellKnown.DefaultNodeId, typeof (T), TypeReferenceAccuracy.BaseType, value);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <param name="domain">Domain to use.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="value">Key value.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create([NotNull] Domain domain, [NotNull] Type type, [NotNull] Tuple value)
    {
      return Create(domain, WellKnown.DefaultNodeId, type, TypeReferenceAccuracy.BaseType, value);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> <paramref name="type"/>,
    /// with specified <paramref name="value"/> and <paramref name="accuracy"/>.
    /// </summary>
    /// <param name="domain">Domain to use.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="accuracy">Key accuracy.</param>
    /// <param name="value">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create([NotNull] Domain domain, [NotNull] Type type, TypeReferenceAccuracy accuracy, [NotNull] Tuple value)
    {
      return Create(domain, WellKnown.DefaultNodeId, type, accuracy, value);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> <paramref name="type"/>,
    /// with specified <paramref name="value"/>, <paramref name="accuracy"/> and <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="domain">Domain to use.</param>
    /// <param name="nodeId">Node identifier to use.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="accuracy">Key accuracy.</param>
    /// <param name="value">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create([NotNull] Domain domain, [NotNull] string nodeId, [NotNull] Type type, TypeReferenceAccuracy accuracy, [NotNull] Tuple value)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(value, "value");

      return Create(domain, nodeId, domain.Model.Types[type], accuracy, value);
    }

    internal static Key Create(Domain domain, string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, Tuple value)
    {
      return KeyFactory.Materialize(domain, nodeId, type, value, accuracy, false, null);
    }

    #endregion

    #region Create key by params object[] methods

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// and with specified <paramref name="values"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="domain">Domain to use.</param>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create<T>([NotNull] Domain domain, [NotNull] params object[] values)
      where T : IEntity
    {
      return Create(domain, WellKnown.DefaultNodeId, typeof (T), TypeReferenceAccuracy.BaseType, values);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="values"/>.
    /// </summary>
    /// <param name="domain">Domain to use.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create([NotNull] Domain domain, [NotNull] Type type, [NotNull] params object[] values)
    {
      return Create(domain, WellKnown.DefaultNodeId, type, TypeReferenceAccuracy.BaseType, values);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> <paramref name="type"/>,
    /// with specified <paramref name="values"/> and <paramref name="accuracy"/>.
    /// </summary>
    /// <param name="domain">Domain to use.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="accuracy">Key accuracy.</param>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create([NotNull] Domain domain, [NotNull] Type type, TypeReferenceAccuracy accuracy, [NotNull] params object[] values)
    {
      return Create(domain, WellKnown.DefaultNodeId, type, accuracy, values);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> <paramref name="type"/>,
    /// with specified <paramref name="values"/>, <paramref name="accuracy"/> and <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="domain">Domain to use.</param>
    /// <param name="nodeId">Node identifier to use.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="accuracy">Key accuracy.</param>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance.</returns>
    public static Key Create([NotNull] Domain domain, [NotNull] string nodeId, [NotNull] Type type, TypeReferenceAccuracy accuracy, [NotNull] params object[] values)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(values, "values");

      return Create(domain, nodeId, domain.Model.Types[type], accuracy, values);
    }

    internal static Key Create(Domain domain, string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, object[] values)
    {
      return KeyFactory.Materialize(domain, nodeId, type, accuracy, values);
    }

    #endregion

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="accuracy">The typre reference accuracy.</param>
    /// <param name="value">The value.</param>
    protected Key(string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, Tuple value)
    {
      NodeId = nodeId;
      TypeReference = new TypeReference(type, accuracy);

      this.value = value;
    }
  }
}
