// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Xtensive.Core;
using Xtensive.Linq.SerializableExpressions.Internals;
using Xtensive.Reflection;

using Xtensive.Tuples.Packed;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Tuple descriptor.
  /// Provides information about <see cref="Tuple"/> structure.
  /// </summary>
  [Serializable]
  public sealed class TupleDescriptor : IEquatable<TupleDescriptor>, IList<Type>, ISerializable
  {
    [NonSerialized]
    private static readonly TupleDescriptor EmptyDescriptor = new TupleDescriptor(new Type[0]);
    [NonSerialized]
    private static readonly Dictionary<Type, TupleDescriptor> SingleFieldDescriptors = new Dictionary<Type, TupleDescriptor>();

    //private string[] fieldTypeNames;

    [NonSerialized]
    private Type[] fieldTypes;

    internal readonly int FieldCount;
    internal readonly int ValuesLength;
    internal readonly int ObjectsLength;

    [NonSerialized]
    internal readonly PackedFieldDescriptor[] FieldDescriptors;
    
    internal Type[] FieldTypes
    {
      get { return fieldTypes; }
    }

    /// <summary>
    /// Gets the empty tuple descriptor.
    /// </summary>
    /// <value>The empty tuple descriptor.</value>
    public static TupleDescriptor Empty
    {
      [DebuggerStepThrough]
      get { return EmptyDescriptor; }
    }

    /// <summary>
    /// Gets total count of compiled descriptors.
    /// </summary>
    [Obsolete("Tuple descriptors are no longer cached. This property always returns -1")]
    public static int TotalCount
    {
      [DebuggerStepThrough]
      get { return -1; }
    }

    /// <inheritdoc/>
    [Obsolete("Tuple descriptors no longer has unique indentifier. This property always returns 0.")]
    public int Identifier
    {
      [DebuggerStepThrough]
      get { return 0; }
    }

    /// <summary>
    /// Indicates whether class for handling underlying 
    /// <see cref="Tuple"/> is already compiled.
    /// </summary>
    [Obsolete("Tuple descriptors are always initialized. This property always returns true.")]
    public bool IsInitialized
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    /// <summary>
    /// Gets the type of underlying <see cref="Tuple"/>
    /// implementation. <see langword="Null"/>, if
    /// <see cref="IsInitialized"/>==<see langword="false"/>.
    /// </summary>
    [Obsolete("This property always returns the same type.")]
    public Type TupleType
    {
      get
      {
        return typeof (PackedTuple);
      }
    }

    /// <summary>
    /// Gets the length of the common part.
    /// </summary>
    /// <param name="other">The other descriptor.</param>
    public int GetCommonPartLength(TupleDescriptor other)
    {
      ArgumentValidator.EnsureArgumentNotNull(other, "other");
      var minCount = FieldCount < other.FieldCount ? FieldCount : other.FieldCount;
      for (int i = 0; i < minCount; i++) {
        if (FieldTypes[i]!=other.FieldTypes[i])
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
      return FieldTypes[fieldIndex].IsValueType;
    }

    #region IList members

    /// <inheritdoc/>
    public Type this[int fieldIndex]
    {
      get { return FieldTypes[fieldIndex]; }
      set { throw Exceptions.CollectionIsReadOnly(null); }
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get { return FieldCount; }
    }

    /// <inheritdoc/>
    public int IndexOf(Type item)
    {
      return FieldTypes.IndexOf(item, true);
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
      return FieldTypes.IndexOf(item, true) >= 0;
    }

    /// <inheritdoc/>
    public void CopyTo(Type[] array, int arrayIndex)
    {
      FieldTypes.Copy(array, arrayIndex);
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
      for (int i = 0; i < FieldCount; i++)
        yield return FieldTypes[i];
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
      if (ReferenceEquals(other, null))
        return false;
      if (ReferenceEquals(other, this))
        return true;
      if (FieldCount!=other.FieldCount)
        return false;
      for (int i = 0; i < FieldCount; i++)
        if (FieldTypes[i]!=other.FieldTypes[i])
          return false;
      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return Equals(obj as TupleDescriptor);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = FieldCount;
      for (int i = 0; i < FieldCount; i++)
        result = unchecked (FieldTypes[i].GetHashCode() + 29 * result);
      return result;
    }

    public static bool operator==(TupleDescriptor left, TupleDescriptor right)
    {
      if (ReferenceEquals(left, right))
        return true;
      if (ReferenceEquals(left, null))
        return false;
      return left.Equals(right);
    }

    public static bool operator !=(TupleDescriptor left, TupleDescriptor right)
    {
      return !(left==right);
    }

    #endregion

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("FieldCount", FieldCount);
      info.AddValue("ValuesLength", ValuesLength);
      info.AddValue("ObjectsLength", ObjectsLength);

      var typeNames = new string[FieldTypes.Length];
      for (var i = 0; i < typeNames.Length; i++)
        typeNames[i] = FieldTypes[i].ToSerializableForm();

      info.AddValue("FieldTypes", typeNames);
      info.AddValue("FieldDescriptors", FieldDescriptors);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      for (int i = 0; i < FieldCount; i++) {
        if (i > 0)
          sb.Append(", ");
        sb.Append(FieldTypes[i].GetShortName());
      }
      return string.Format(Strings.TupleDescriptorFormat, sb);
    }

    //[OnSerializing]
    //private void OnSerializing(StreamingContext context)
    //{
    //  fieldTypeNames = new string[FieldTypes.Length];
    //  for (var i = 0; i < fieldTypeNames.Length; i++)
    //    fieldTypeNames[i] = FieldTypes[i].ToSerializableForm();
    //}

    //[OnDeserialized]
    //private void OnDeserialized(StreamingContext context)
    //{
    //  fieldTypes = new Type[fieldTypeNames.Length];
    //  for (int i = 0; i < fieldTypeNames.Length; i++)
    //    FieldTypes[i] = fieldTypeNames[i].GetTypeFromSerializableForm();
    //  for (int i = 0; i < FieldCount; i++)
    //    PackedFieldAccessorFactory.ProvideAccessor(FieldTypes[i], FieldDescriptors[i]);
    //}

    private static TupleDescriptor CreateInternal(IList<Type> fieldTypes)
    {
      var fieldCount = fieldTypes.Count;
      if (fieldCount==0)
        return EmptyDescriptor;
      if (fieldCount==1) {
        TupleDescriptor cachedDescriptor;
        if (SingleFieldDescriptors.TryGetValue(fieldTypes[0], out cachedDescriptor))
          return cachedDescriptor;
      }
      return new TupleDescriptor(fieldTypes);
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
      return CreateInternal(fieldTypes);
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
      return CreateInternal(fieldTypes);
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
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      return CreateInternal(fieldTypes.ToList());
    }

    /// <summary>
    /// Creates tuple descriptor containing head of the current one.
    /// </summary>
    /// <param name="headFieldCount">Head field count.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public TupleDescriptor Head(int headFieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(headFieldCount, 1, Count, "headFieldCount");
      return Create(FieldTypes.Take(headFieldCount));
    }

    /// <summary>
    /// Creates tuple descriptor containing tail of the current one.
    /// </summary>
    /// <param name="tailFieldCount">Tail field count.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public TupleDescriptor Tail(int tailFieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(tailFieldCount, 1, Count, "tailFieldCount");
      return Create(FieldTypes.Skip(Count - tailFieldCount));
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
      return CreateInternal(new[] {
        typeof (T)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1, T2>()
    {
      return CreateInternal(new[] {
        typeof (T1),
        typeof (T2)
      });
    }

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1, T2, T3>()
    {
      return CreateInternal(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3)
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
    public static TupleDescriptor Create<T1, T2, T3, T4>()
    {
      return CreateInternal(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4)
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
    public static TupleDescriptor Create<T1, T2, T3, T4, T5>()
    {
      return CreateInternal(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5)
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
    public static TupleDescriptor Create<T1, T2, T3, T4, T5, T6>()
    {
      return CreateInternal(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6)
      });
    }

    #endregion

    // Constructors

    private TupleDescriptor(IList<Type> fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");

      FieldCount = fieldTypes.Count;
      this.fieldTypes = new Type[FieldCount];
      FieldDescriptors = new PackedFieldDescriptor[FieldCount];

      const int longBits = 64;
      const int stateBits = 2;
      const int statesPerLong = longBits / stateBits;

      var objectIndex = 0;

      var valueIndex = FieldCount / statesPerLong + Math.Min(1, FieldCount % statesPerLong);
      var valueBitOffset = 0;

      var stateIndex = 0;
      var stateBitOffset = 0;

      for (int i = 0; i < FieldCount; i++) {
        var fieldType = fieldTypes[i].StripNullable();
        var descriptor = new PackedFieldDescriptor {FieldIndex = i};

        PackedFieldAccessorFactory.ProvideAccessor(fieldType, descriptor);

        FieldTypes[i] = fieldType;
        FieldDescriptors[i] = descriptor;
      }

      var orderedDescriptors = FieldDescriptors
        .OrderByDescending(d => d.ValueBitCount)
        .ThenBy(d => d.FieldIndex);

      foreach (var descriptor in orderedDescriptors) {
        switch (descriptor.PackingType) {
        case FieldPackingType.Object:
          descriptor.ValueIndex = objectIndex++;
          break;
        case FieldPackingType.Value:
          if (descriptor.ValueBitCount > longBits) {
            if (valueBitOffset > 0) {
              valueIndex++;
              valueBitOffset = 0;
            }
            descriptor.ValueIndex = valueIndex;
            descriptor.ValueBitOffset = 0;
            valueIndex += descriptor.ValueBitCount / longBits + Math.Min(1, descriptor.ValueBitCount % longBits);
          }
          else {
            if (valueBitOffset + descriptor.ValueBitCount > longBits) {
              valueIndex++;
              valueBitOffset = 0;
            }
            descriptor.ValueIndex = valueIndex;
            descriptor.ValueBitOffset = valueBitOffset;
            valueBitOffset += descriptor.ValueBitCount;
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("descriptor.PackType");
        }

        if (stateBitOffset + stateBits > longBits) {
          stateIndex++;
          stateBitOffset = 0;
        }

        descriptor.StateIndex = stateIndex;
        descriptor.StateBitOffset = stateBitOffset;
        stateBitOffset += stateBits;
      }

      ValuesLength = valueIndex + Math.Min(1, valueBitOffset);
      ObjectsLength = objectIndex;
    }

    public TupleDescriptor(SerializationInfo info, StreamingContext context)
    {
      FieldCount = info.GetInt32("FieldCount");
      ValuesLength = info.GetInt32("ValuesLength");
      ObjectsLength = info.GetInt32("ObjectsLength");


      var typeNames = (string[]) info.GetValue("FieldTypes", typeof(string[]));
      FieldDescriptors = (PackedFieldDescriptor[])info.GetValue("FieldDescriptors", typeof(PackedFieldDescriptor[]));

      fieldTypes = new Type[typeNames.Length];
      for (int i = 0; i < typeNames.Length; i++)
        FieldTypes[i] = typeNames[i].GetTypeFromSerializableForm();
      for (int i = 0; i < FieldCount; i++)
        PackedFieldAccessorFactory.ProvideAccessor(FieldTypes[i], FieldDescriptors[i]);

    }


    static TupleDescriptor()
    {
      foreach (var type in PackedFieldAccessorFactory.KnownTypes)
        SingleFieldDescriptors.Add(type, new TupleDescriptor(new[] {type}));
    }
  }
}
