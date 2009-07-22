// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
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
    #region Nested type: GenericKeyTypeInfo

    private class GenericKeyTypeInfo
    {
      public Type Type;
      public Func<HierarchyInfo, TypeInfo, Tuple, Key> DefaultConstructor;
      public Func<HierarchyInfo, TypeInfo, Tuple, int[], Key> KeyIndexBasedConstructor;
    }

    #endregion

    /// <summary>
    /// Maximal supported length (count of values) of purely generic keys.
    /// </summary>
    public static int MaxGenericKeyLength = 2;
    private const string GenericTypeNameFormat = "{0}`{1}";

    private static ThreadSafeDictionary<HierarchyInfo, GenericKeyTypeInfo> genericKeyTypes = 
      ThreadSafeDictionary<HierarchyInfo, GenericKeyTypeInfo>.Create(new object());
    
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
        if (cachedKey==null) {
          if (Hierarchy.Types.Count == 1) {
            type = Hierarchy.Types[0];
            return type;
          }
          if (session.IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is unknown. Fetch is required.", session, this);

          var field = Hierarchy.Root.Fields[WellKnown.TypeIdFieldName];
          cachedKey = session.Handler.Fetch(this, field);
          if (cachedKey==null)
            throw new InvalidOperationException(
              string.Format(Strings.ExUnableToResolveTypeForKeyX, this));
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

    protected int HashCode {
      get {
        if (isHashCodeCalculated)
          return hashCode;
        hashCode = CalculateHashCode();
        isHashCodeCalculated = true;
        return hashCode;
      }
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
      var session = Session.Demand();
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
        session.Handler.Fetch(this);
        state = cache[this, true];
        hasBeenFetched = true;
      }

      if (!hasBeenFetched && session.IsDebugEventLoggingEnabled)
        Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved.", session, this);

      if (state == null || state.IsRemoved)
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
      
      return Create(domain, type, keyTupleDescriptor.Parse(valueString), null, false, false);
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
      var domain = Domain.Demand();
      var typeInfo = domain.Model.Types[type];
      var keyGenerator = domain.KeyGenerators[typeInfo.Hierarchy.GeneratorInfo];
      return Create(domain, typeInfo, keyGenerator.Next(), null, true, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="HierarchyInfo"/>
    /// with newly generated value.
    /// </summary>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    /// <param name="hierarchy">The hierarchy.</param>
    public static Key Create(HierarchyInfo hierarchy)
    {
      var domain = Domain.Demand();
      Tuple keyValue = GenerateKeyValue(domain, hierarchy);
      return Create(domain, hierarchy.Root, keyValue, null, false, false);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// with newly generated value.
    /// </summary>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type)
    {
      var domain = Domain.Demand();
      Tuple keyValue = GenerateKeyValue(domain, type.Hierarchy);
      return Create(domain, type, keyValue, null, true, false);
    }

    private static Tuple GenerateKeyValue(Domain domain, HierarchyInfo hierarchy)
    {
      var keyGenerator = domain.KeyGenerators[hierarchy.GeneratorInfo];
      if (keyGenerator==null)
        throw new InvalidOperationException(
          string.Format(Strings.ExUnableToCreateKeyForXHierarchy, hierarchy));
      return keyGenerator.Next();
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
      return Create(typeof (T), value, false);
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
      var domain = Domain.Demand();
      return Create(domain, domain.Model.Types[type], value, null, false, false);
    }

    internal static Key Create(Type type, Tuple value, bool exactType)
    {
      var domain = Domain.Demand();
      return Create(domain, domain.Model.Types[type], value, null, exactType, false);
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
      return Create(Domain.Demand(), type, value, null, false, false);
    }

    internal static Key Create(TypeInfo type, Tuple value, bool exactType)
    {
      return Create(Domain.Demand(), type, value, null, exactType, false);
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
      return Create(typeof (T), false, values);
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
      var domain = Domain.Demand();
      return Create(domain.Model.Types[type], false, values);
    }

    internal static Key Create(Type type, bool exactType, params object[] values)
    {
      var domain = Domain.Demand();
      return Create(domain.Model.Types[type], exactType, values);
    }

    /// <summary>
    /// Creates <see cref="Key"/> instance 
    /// for the specified <see cref="Entity"/> <paramref name="type"/>
    /// and with specified <paramref name="values"/>.
    /// </summary>
    /// <param name="values">Key values.</param>
    /// <returns>A newly created or existing <see cref="Key"/> instance .</returns>
    public static Key Create(TypeInfo type, params object[] values)
    {
      return Create(type, false, values);
    }

    internal static Key Create(TypeInfo type, bool exactType, params object[] values)
    {
      ArgumentValidator.EnsureArgumentNotNull(values, "values");

      var keyInfo = type.Hierarchy.KeyInfo;
      ArgumentValidator.EnsureArgumentIsInRange(values.Length, 1, keyInfo.TupleDescriptor.Count, "values");

      var tuple = Tuple.Create(keyInfo.TupleDescriptor);
      int tupleIndex = 0;
      for (int valueIndex = 0; valueIndex < values.Length; valueIndex++) {
        var value = values[valueIndex];
        ArgumentValidator.EnsureArgumentNotNull(value, string.Format("values[{0}]", valueIndex));
        var entity = value as Entity;
        if (entity!=null)
          value = entity.Key;
        var key = value as Key;
        if (key!=null) {
          for (int keyIndex = 0; keyIndex < key.Value.Count; keyIndex++)
            tuple[tupleIndex++] = key.Value[keyIndex];
        }
        else
          tuple[tupleIndex++] = value;
      }
      if (tupleIndex < tuple.Count - 1)
        throw new ArgumentException(string.Format(Strings.ExSpecifiedValuesArentEnoughToCreateKeyForTypeX, type.Name));

      return Create(Domain.Demand(), type, tuple, null, exactType, false);
    }

    #endregion

    #region Low-level key creation methods

    /// <exception cref="ArgumentException">Wrong key structure for the specified <paramref name="type"/>.</exception>
    internal static Key Create(Domain domain, TypeInfo type, Tuple value, int[] keyIndexes, bool exactType, bool canCache)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      var hierarchy = type.Hierarchy;
      var keyInfo = hierarchy.KeyInfo;
      if (keyIndexes != null) {
        if (keyIndexes.Length!=keyInfo.TupleDescriptor.Count)
          throw new ArgumentException(Strings.ExWrongKeyStructure);
        for (int i = keyIndexes.Length - 1; i >= 0; i--)
          if (value.Descriptor[keyIndexes[i]]!=keyInfo.TupleDescriptor[i])
            throw new ArgumentException(Strings.ExWrongKeyStructure);
      }
      else if (value.Descriptor!=keyInfo.TupleDescriptor)
        throw new ArgumentException(Strings.ExWrongKeyStructure);

      if (hierarchy.Root.IsLeaf)
        exactType = true;

      // Ensures TypeId is filled in
      if (exactType && keyInfo.TypeIdColumnIndex >= 0)
        value[keyInfo.TypeIdColumnIndex] = type.TypeId;

      Key key;
      if (keyInfo.Length <= MaxGenericKeyLength)
        key = CreateGenericKey(type.Hierarchy, exactType ? type : null, value, keyIndexes);
      else {
        if (keyIndexes!=null)
          throw Exceptions.InternalError(Strings.ExKeyIndexesAreSpecifiedForNonGenericKey, Log.Instance);
        key = new Key(type.Hierarchy, exactType ? type : null, value);
      }
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
          //key.value = value.ToFastReadOnly();
          if (exactType)
            keyCache.Add(key);
        }
      }
      return key;
    }

    private static Key CreateGenericKey(HierarchyInfo hierarchy, TypeInfo type, Tuple tuple, int[] keyIndexes)
    {
      var keyTypeInfo = genericKeyTypes.GetValue(hierarchy, BuildGenericKeyTypeInfo);
      if (keyIndexes==null)
        return keyTypeInfo.DefaultConstructor(hierarchy, type, tuple);
      else
        return keyTypeInfo.KeyIndexBasedConstructor(hierarchy, type, tuple, keyIndexes);
    }

    private static GenericKeyTypeInfo BuildGenericKeyTypeInfo(HierarchyInfo hierarchy)
    {
      var descriptor = hierarchy.KeyInfo.TupleDescriptor;
      int length = descriptor.Count;
      var type = typeof (Key).Assembly.GetType(
        string.Format(GenericTypeNameFormat, typeof (Key).FullName, length));
      type = type.MakeGenericType(descriptor.ToArray());
      return new GenericKeyTypeInfo() {
        Type = type,
        DefaultConstructor =
          DelegateHelper.CreateDelegate<Func<HierarchyInfo, TypeInfo, Tuple, Key>>(null, type, "Create", ArrayUtils<Type>.EmptyArray),
        KeyIndexBasedConstructor =
          DelegateHelper.CreateDelegate<Func<HierarchyInfo, TypeInfo, Tuple, int[], Key>>(null, type, "Create", ArrayUtils<Type>.EmptyArray)
      };
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
