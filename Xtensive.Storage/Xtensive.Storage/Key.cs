// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains a set of identifying values of an <see cref="Entity"/>.
  /// </summary>
  /// <remarks>
  /// Every entity is uniquely identified by its <see cref="Entity.Key"/>.
  /// </remarks>
  /// <seealso cref="Entity.Key"/>
  [Serializable]
  public class Key : IEquatable<Key>
  {
    private readonly HierarchyInfo hierarchy;
    protected Tuple value;

    [NonSerialized]
    private bool isHashCodeCalculated;

    [NonSerialized]
    private int hashCode;
    
    [NonSerialized]
    private TypeInfo type;

    [NonSerialized]
    private string cachedFormatResult;

    /// <summary>
    /// Gets the hierarchy identified entity belongs to.
    /// </summary>
    public HierarchyInfo Hierarchy
    {
      get { return hierarchy; }
    }

    /// <summary>
    /// Gets the key value.
    /// </summary>
    public virtual Tuple Value {
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
        if (cachedKey!=null) {
          type = cachedKey.type;
          return type;
        }
        if (Hierarchy.Types.Count==1) {
          type = Hierarchy.Types[0];
          return type;
        }
        if (session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required.", session, this);

        var entityState = session.Handler.FetchInstance(this);
        if (entityState==null)
          throw new InvalidOperationException(string.Format(Strings.ExUnableToResolveTypeForKeyX, this));
        type = entityState.Type;
        return type;

      }
    }

    /// <summary>
    /// Determines whether <see cref="Type"/> property has cached type value or not.
    /// </summary>
    public bool IsTypeCached {
      get { return type!=null ? true : false; }
    }

    private int HashCode {
      get {
        if (isHashCodeCalculated)
          return hashCode;
        hashCode = CalculateHashCode();
        isHashCodeCalculated = true;
        return hashCode;
      }
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
      if (HashCode!=other.HashCode)
        return false;
      if (Hierarchy!=other.Hierarchy)
        return false;
      if (other.GetType().IsGenericType)
        return other.ValueEquals(this);
      return ValueEquals(other);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      var other = obj as Key;
      if (other==null)
        return false;
      return Equals(other);
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
      return HashCode;
    }

    /// <summary>
    /// Compares key value for equality.
    /// </summary>
    /// <param name="other">The other key to compare.</param>
    /// <returns>Equality comparison result.</returns>
    protected virtual bool ValueEquals(Key other)
    {
      return value.Equals(other.value);
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
        builder.Append(Value.Format());
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

      var domain = Domain.Demand();
      var type = domain.Model.Types[Int32.Parse(typeIdString)];
      var keyTupleDescriptor = type.Hierarchy.KeyInfo.TupleDescriptor;
      
      return KeyFactory.Create(type, keyTupleDescriptor.Parse(valueString), null, false, false);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      if (IsTypeCached)
        return string.Format(Strings.KeyFormat,
          Type.UnderlyingType.GetShortName(),
          Value.ToRegular());
      else
        return string.Format(Strings.KeyFormatUnknownKeyType,
          Hierarchy.Root.UnderlyingType.GetShortName(),
          Value.ToRegular());
    }

    /// <summary>
    /// Calculates hash code.
    /// </summary>
    /// <returns>Calculated hash code.</returns>
    protected virtual int CalculateHashCode()
    {
      return value.GetHashCode() ^ hierarchy.GetHashCode();
    }

    #region Create next key methods

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
      return Create(Domain.Demand().Model.Types[type]);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// with newly generated value.
    /// </summary>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type)
    {
      return KeyFactory.CreateNext(type);
    }

    #endregion

    #region Create key by tuple methods

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// and with specified <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="value">Key value.</param>
    /// <returns>
    /// A newly created or existing <see cref="Key"/> instance.
    /// </returns>
    public static Key Create<T>(Tuple value)
      where T : Entity  
    {
      return Create<T>(value, false);
    }

    internal static Key Create<T>(Tuple value, bool exactType)
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
      return Create(type, value, false);
    }

    internal static Key Create(Type type, Tuple value, bool exactType)
    {
      return Create(Domain.Demand().Model.Types[type], value, exactType);
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
      return Create(type, value, false);
    }

    internal static Key Create(TypeInfo type, Tuple value, bool exactType)
    {
      return KeyFactory.Create(type, value, null, exactType, false);
    }

    #endregion

    #region Create key by params object[] methods

    /// <summary>
    /// Creates <see cref="Key"/> instance
    /// for the specified <see cref="Entity"/> type <typeparamref name="T"/>
    /// and with specified <paramref name="values"/>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="Entity"/> descendant to get <see cref="Key"/> for.</typeparam>
    /// <param name="values">Key values.</param>
    /// <returns>
    /// A newly created or existing <see cref="Key"/> instance.
    /// </returns>
    public static Key Create<T>(params object[] values)
      where T : Entity  
    {
      return Create<T>(false, values);
    }

    internal static Key Create<T>(bool exactType, params object[] values)
      where T : Entity
    {
      return Create(typeof (T), exactType, values);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="values"/>.
    /// </summary>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(Type type, params object[] values)
    {
      return Create(type, false, values);
    }

    private static Key Create(Type type, bool exactType, params object[] values)
    {
      return Create(Domain.Demand().Model.Types[type], exactType, values);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="values"/>.
    /// </summary>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type, bool exactType, params object[] values)
    {
      return KeyFactory.Create(type, exactType, values);
    }

    #endregion


    // Constructors

    internal Key(HierarchyInfo hierarchy, TypeInfo type, Tuple value)
    {
      this.hierarchy = hierarchy;
      this.type = type;
      this.value = value;
    }
  }
}
