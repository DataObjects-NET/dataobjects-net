// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Collections;
using Xtensive.Comparison;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;
using Xtensive.Resources;
using Xtensive.Tuples.Internals;
using WellKnown = Xtensive.Reflection.WellKnown;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Tuple descriptor.
  /// Provides information about <see cref="Tuple"/> structure.
  /// </summary>
  [Serializable]
  public class TupleDescriptor : 
    IIdentified<int>,
    IEquatable<TupleDescriptor>,
    IComparable<TupleDescriptor>,
    ICountable<Type>,
    IList<Type>
  {
    private static int totalCount;
    private int identifier;
    private int fieldCount;
    internal readonly Type[] fieldTypes;
    private readonly bool[] isValueTypeFlags;
    private ITupleFactory tupleFactory;
    private bool isInitialized;
    private int cachedHashCode;
    internal Delegate[] GetValueDelegates;
    internal Delegate[] GetNullableValueDelegates;
    internal Delegate[] SetValueDelegates;
    internal Delegate[] SetNullableValueDelegates;

    /// <summary>
    /// Gets the empty tuple descriptor.
    /// </summary>
    /// <value>The empty tuple descriptor.</value>
    public static TupleDescriptor Empty
    {
      [DebuggerStepThrough]
      get { return EmptyTupleDescriptor.Instance; }
    }

    /// <summary>
    /// Gets total count of compiled descriptors.
    /// </summary>
    public static int TotalCount
    {
      [DebuggerStepThrough]
      get { return totalCount; }
    }

    /// <inheritdoc/>
    public int Identifier
    {
      [DebuggerStepThrough]
      get { return identifier; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      [DebuggerStepThrough]
      get { return identifier; }
    }

    /// <summary>
    /// Indicates whether class for handling underlying 
    /// <see cref="Tuple"/> is already compiled.
    /// </summary>
    public bool IsInitialized
    {
      [DebuggerStepThrough]
      get { return isInitialized; }
    }

    /// <summary>
    /// Gets the type of underlying <see cref="Tuple"/>
    /// implementation. <see langword="Null"/>, if
    /// <see cref="IsInitialized"/>==<see langword="false"/>.
    /// </summary>
    public Type TupleType {
      get
      {
        return isInitialized 
          ? TupleFactory.GetType() 
          : null;
      }
    }

    /// <summary>
    /// Gets the length of the common part.
    /// </summary>
    /// <param name="other">The other descriptor.</param>
    public int GetCommonPartLength(TupleDescriptor other)
    {
      ArgumentValidator.EnsureArgumentNotNull(other, "other");
      var minCount = fieldCount < other.fieldCount ? fieldCount : other.fieldCount;
      for (int i = 0; i < minCount; i++) {
        if (fieldTypes[i] != other.fieldTypes[i])
          return i;
      }
      return minCount;
    }

    /// <summary>
    /// Determines whether the specified field is a value type field.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to check.</param>
    /// <returns>
    /// <see langword="true"/> if specified field is a value type field; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsValueType(int fieldIndex)
    {
      return isValueTypeFlags[fieldIndex];
    }

    #region Initialization related members

    /// <summary>
    /// Ensures the descriptor is compiled.
    /// </summary>
    protected internal void EnsureIsInitialized()
    {
      if (!isInitialized)
        TupleDescriptorCache.Initialize();
      Debug.Assert(isInitialized, "TupleDescriptor should have IsInitialized flag set to true after the initialization!");
    }

    internal void Initialize(ITupleFactory factory)
    {
      if (factory == null) 
        throw new ArgumentNullException("factory");
      identifier = ++totalCount;
      tupleFactory = factory;
      isInitialized = true;
      var tupleType = factory.GetType();

      GetValueDelegates = new Delegate[Count];
      GetNullableValueDelegates = new Delegate[Count];
      SetValueDelegates = new Delegate[Count];
      SetNullableValueDelegates = new Delegate[Count];
      if (Count > MaxGeneratedTupleLength.Value) {
        var tupleExtender = (JoinedTuple)factory;
        var firstDescriptor = tupleExtender.First.Descriptor;
        var secondDescriptor = tupleExtender.Second.Descriptor;
        if (firstDescriptor == null || secondDescriptor == null)
          throw new InvalidOperationException();
        for (int fieldIndex = 0; fieldIndex < Count; fieldIndex++) {
          var type = fieldTypes[fieldIndex];
          var extenderAccessorType = typeof (JoinedTupleAccessor<>).MakeGenericType(type);
          object extenderAccessorInstance;
          if (fieldIndex < MaxGeneratedTupleLength.Value)
            extenderAccessorInstance = Activator.CreateInstance(
              extenderAccessorType,
              firstDescriptor.GetValueDelegates[fieldIndex],
              firstDescriptor.SetValueDelegates[fieldIndex],
              fieldIndex);
          else
            extenderAccessorInstance = Activator.CreateInstance(
              extenderAccessorType,
              secondDescriptor.GetValueDelegates[fieldIndex - MaxGeneratedTupleLength.Value],
              secondDescriptor.SetValueDelegates[fieldIndex - MaxGeneratedTupleLength.Value],
              fieldIndex);
          var extenderAccessor = (JoinedTupleAccessor)extenderAccessorInstance;
          var getValueDelegate = extenderAccessor.GetValueDelegate;
          var setValueDelegate = extenderAccessor.SetValueDelegate;
          GetValueDelegates[fieldIndex] = getValueDelegate;
          SetValueDelegates[fieldIndex] = setValueDelegate;
          if (isValueTypeFlags[fieldIndex]) {
            var nullableType = typeof (Nullable<>).MakeGenericType(type);
            var accessorType = typeof (NullableAccessor<>).MakeGenericType(type);
            var accessor = (NullableAccessor)Activator.CreateInstance(accessorType, getValueDelegate, setValueDelegate, fieldIndex);
            GetNullableValueDelegates[fieldIndex] = accessor.GetValueDelegate;
            SetNullableValueDelegates[fieldIndex] = accessor.SetValueDelegate;
          }
          else {
            GetNullableValueDelegates[fieldIndex] = getValueDelegate;
            SetNullableValueDelegates[fieldIndex] = setValueDelegate;
          }
        }
      }
      else
        for (int fieldIndex = 0; fieldIndex < Count; fieldIndex++) {
          var type = fieldTypes[fieldIndex];
          var getValueDelegateType = typeof (GetValueDelegate<>).MakeGenericType(type);
          var getValueDelegate = Delegate.CreateDelegate(getValueDelegateType, tupleType.GetMethod(Reflection.WellKnown.Tuple.GetValueX.FormatWith(fieldIndex)), true);
          var setValueDelegateType = typeof(Action<,>).MakeGenericType(typeof(Tuple), type);
          var setValueDelegate = Delegate.CreateDelegate(setValueDelegateType, tupleType.GetMethod(Reflection.WellKnown.Tuple.SetValueX.FormatWith(fieldIndex)), true);
          GetValueDelegates[fieldIndex] = getValueDelegate;
          SetValueDelegates[fieldIndex] = setValueDelegate;
          if (isValueTypeFlags[fieldIndex]) {
            var nullableType = typeof (Nullable<>).MakeGenericType(type);
            var accessorType = typeof (NullableAccessor<>).MakeGenericType(type);
            var accessor = (NullableAccessor)Activator.CreateInstance(accessorType, getValueDelegate, setValueDelegate, fieldIndex);
            GetNullableValueDelegates[fieldIndex] = accessor.GetValueDelegate;
            SetNullableValueDelegates[fieldIndex] = accessor.SetValueDelegate;
          }
          else {
            GetNullableValueDelegates[fieldIndex] = getValueDelegate;
            SetNullableValueDelegates[fieldIndex] = setValueDelegate;
          }
        }
    }

    #endregion

    #region ICountable, IList members

    /// <inheritdoc/>
    public Type this[int fieldIndex] {
      get {
        return fieldTypes[fieldIndex];
      }
      set {
        throw Exceptions.CollectionIsReadOnly(null);
      }
    }

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return fieldCount; }
    }

    /// <inheritdoc/>
    long ICountable.Count
    {
      [DebuggerStepThrough]
      get { return fieldCount; }
    }

    /// <inheritdoc/>
    public int IndexOf(Type item)
    {
      return fieldTypes.IndexOf(item, true);
    }

    /// <inheritdoc/>
    public void Insert(int index, Type item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void Add(Type item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public bool Contains(Type item)
    {
      return fieldTypes.IndexOf(item, true)>=0;
    }

    /// <inheritdoc/>
    public void CopyTo(Type[] array, int arrayIndex)
    {
      fieldTypes.Copy(array, arrayIndex);
    }

    /// <inheritdoc/>
    public bool Remove(Type item)
    {
      throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return true; }
    }

    /// <inheritdoc/>
    public IEnumerator<Type> GetEnumerator()
    {
      for (int i = 0; i<fieldCount; i++)
        yield return fieldTypes[i];
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IEquatable members, GetHashCode 

    /// <inheritdoc/>
    public bool Equals(TupleDescriptor other)
    {
      if (other==null) return false;
      if (other==this) return true;
      if (isInitialized && other.isInitialized)
        return false;
      if (fieldCount != other.fieldCount)
        return false;
      var result = true;
      for (int i = fieldCount - 1; i >= 0 && result; i--)
        result &= fieldTypes[i] == other.fieldTypes[i];
      return result;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return ReferenceEquals(this, obj) || Equals(obj as TupleDescriptor);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (cachedHashCode==0) {
        int hashCode = fieldCount;
        for (int i = 0; i < fieldCount; i++)
          hashCode = unchecked (fieldTypes[i].GetHashCode() + 29*hashCode);
        if (hashCode==0)
          hashCode = -1;
        cachedHashCode = hashCode;
      }
      return cachedHashCode;
    }

    #endregion

    #region IComparable members

    /// <inheritdoc/>
    public int CompareTo(TupleDescriptor other)
    {
      if (other==null) return 1;
      if (other==this) return 0;
      return AdvancedComparerStruct<Type[]>.Default.Compare(fieldTypes, other.fieldTypes);
    }

    #endregion

    #region Private \ internal methods

    internal ITupleFactory TupleFactory
    {
      get { return tupleFactory; }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i<fieldCount; i++) {
        if (i>0)
          sb.Append(", ");
        sb.Append(fieldTypes[i].GetShortName());
      }
      return string.Format(Strings.TupleDescriptorFormat, sb);
    }

    #region Create methods (base)

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">List of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(Type[] fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      var descriptor = new TupleDescriptor(fieldTypes);
      return TupleDescriptorCache.Register(descriptor);
    }

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">List of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(IList<Type> fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      var descriptor = new TupleDescriptor(fieldTypes);
      return TupleDescriptorCache.Register(descriptor);
    }

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">Enumerable of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(IEnumerable<Type> fieldTypes)
    {
      return Create(fieldTypes.ToArray());
    }

    /// <summary>
    /// Trims the field's set of the current tuple to the specified length.
    /// </summary>
    /// <param name="newTupleFieldCount">The length of the field's set
    /// of the new <see cref="TupleDescriptor"/>.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public TupleDescriptor TrimFields(int newTupleFieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(newTupleFieldCount, 1, Count, "newTupleFieldCount");
      return Create(fieldTypes.Take(newTupleFieldCount));
    }

    /// <summary>
    /// Skips first fields of the current tuple.
    /// </summary>
    /// <param name="skipFieldCount">The length of the fields to skip.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public TupleDescriptor SkipFields(int skipFieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(skipFieldCount, 0, Count-1, "skipFieldCount");
      return Create(fieldTypes.Skip(skipFieldCount));
    }

    #endregion

    #region Create<...> methods (generic shortcuts)

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T">Type of the only tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object.</returns>
    public static TupleDescriptor Create<T>()
    {
      return Create(new[] {
        typeof(T)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3,T4>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3,T4,T5>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4),
        typeof(T5)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <typeparam name="T6">Type of the 6th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1,T2,T3,T4,T5,T6>()
    {
      return Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4),
        typeof(T5),
        typeof(T6)
      });
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fieldTypes">The types of <see cref="TupleDescriptor"/> fields.</param>
    protected TupleDescriptor(IList<Type> fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      fieldCount = fieldTypes.Count;
      this.fieldTypes = new Type[fieldCount];
      isValueTypeFlags = new bool[fieldCount];
      for (int i = 0; i<fieldCount; i++) {
        Type t = fieldTypes[i];
        if (t.IsNullable())
          t = t.GetGenericArguments()[0]; // Substituting Nullable<T> to T
        this.fieldTypes[i] = t;
        isValueTypeFlags[i] = t.IsValueType;
      }
      
    }
  }
}
