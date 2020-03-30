// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Collections;
using System.Collections.Concurrent;
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
  public sealed class TupleDescriptor : IEquatable<TupleDescriptor>, IList<Type>, IReadOnlyList<Type>, ISerializable
  {
    private static readonly TupleDescriptor EmptyDescriptor = new TupleDescriptor(Array.Empty<Type>());
    [NonSerialized]
    private static readonly Dictionary<Type, TupleDescriptor> CachedDescriptors1 = 
        new Dictionary<Type, TupleDescriptor>();
    [NonSerialized]
    private static readonly Dictionary<(Type, Type), TupleDescriptor> CachedDescriptors2 = 
      new Dictionary<(Type, Type), TupleDescriptor>();
    [NonSerialized]
    private static readonly Dictionary<(Type, Type, Type), TupleDescriptor> CachedDescriptors3 = 
      new Dictionary<(Type, Type, Type), TupleDescriptor>();
    [NonSerialized]
    private static readonly ConcurrentDictionary<(Type, Type, Type, Type), TupleDescriptor> CachedDescriptors4 = 
      new ConcurrentDictionary<(Type, Type, Type, Type), TupleDescriptor>();

    internal readonly int FieldCount;
    internal readonly int ValuesLength;
    internal readonly int ObjectsLength;

    [NonSerialized]
    internal readonly PackedFieldDescriptor[] FieldDescriptors;
    
    [field: NonSerialized]
    internal Type[] FieldTypes { get; }

    /// <summary>
    /// Gets the empty tuple descriptor.
    /// </summary>
    /// <value>The empty tuple descriptor.</value>
    public static TupleDescriptor Empty
    {
      [DebuggerStepThrough]
      get => EmptyDescriptor;
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
        if (FieldTypes[i] != other.FieldTypes[i])
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
      => FieldTypes[fieldIndex].IsValueType;

    #region IList members

    /// <inheritdoc/>
    public Type this[int fieldIndex]
    {
      get => FieldTypes[fieldIndex];
      set => throw Exceptions.CollectionIsReadOnly(null);
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get => FieldCount;
    }

    /// <inheritdoc/>
    public int IndexOf(Type item) 
      => FieldTypes.IndexOf(item, true);

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

    #region Create methods (base)

    public static TupleDescriptor Create(Type t1)
    {
      if (CachedDescriptors1.TryGetValue(t1, out var cachedDescriptor))
        return cachedDescriptor;
      return new TupleDescriptor(new [] {t1});
    }

    public static TupleDescriptor Create(Type t1, Type t2)
    {
      var key = (t1, t2);
      if (CachedDescriptors2.TryGetValue(key, out var cachedDescriptor))
        return cachedDescriptor;
      return new TupleDescriptor(new [] {t1, t2});
    }

    public static TupleDescriptor Create(Type t1, Type t2, Type t3)
    {
      var key = (t1, t2, t3);
      if (CachedDescriptors3.TryGetValue(key, out var cachedDescriptor))
        return cachedDescriptor;
      return new TupleDescriptor(new [] {t1, t2, t3});
    }

    public static TupleDescriptor Create(Type t1, Type t2, Type t3, Type t4)
    {
      var key = (t1, t2, t3, t4);
      return CachedDescriptors4.GetOrAdd(key, k => 
        new TupleDescriptor(new [] {k.Item1, k.Item2, k.Item3, k.Item4}));
    }

    /// <summary>
    /// Creates or returns already created descriptor
    /// for provided set of types.
    /// </summary>
    /// <param name="fieldTypes">List of tuple field types.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public static TupleDescriptor Create(Type[] fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, nameof(fieldTypes));
      switch (fieldTypes.Length) {
      case 0:
        return EmptyDescriptor;
      case 1: 
        if (CachedDescriptors1.TryGetValue(fieldTypes[0], out var cachedDescriptor))
          return cachedDescriptor;
        break;
      case 2:
        var key2 = (fieldTypes[0], fieldTypes[1]);
        if (CachedDescriptors2.TryGetValue(key2, out cachedDescriptor))
          return cachedDescriptor;
        break;
      case 3:
        var key3 = (fieldTypes[0], fieldTypes[1], fieldTypes[2]);
        if (CachedDescriptors3.TryGetValue(key3, out cachedDescriptor))
          return cachedDescriptor;
        break;
      case 4:
        var key4 = (fieldTypes[0], fieldTypes[1], fieldTypes[2], fieldTypes[3]);
        return CachedDescriptors4.GetOrAdd(key4, k => 
          new TupleDescriptor(new [] {k.Item1, k.Item2, k.Item3, k.Item4}));
      }
      return new TupleDescriptor(fieldTypes);
    }

    /// <summary>
    /// Creates tuple descriptor containing head of the current one.
    /// </summary>
    /// <param name="fieldCount">Head field count.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public TupleDescriptor Head(int fieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldCount, 1, Count, nameof(fieldCount));
      var fieldTypes = new Type[fieldCount];
      Array.Copy(FieldTypes, 0, fieldTypes, 0, fieldCount);
      return new TupleDescriptor(fieldTypes);
    }

    /// <summary>
    /// Creates tuple descriptor containing tail of the current one.
    /// </summary>
    /// <param name="tailFieldCount">Tail field count.</param>
    /// <returns>Either new or existing tuple descriptor
    /// describing the specified set of fields.</returns>
    public TupleDescriptor Tail(int tailFieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(tailFieldCount, 1, Count, nameof(tailFieldCount));
      var fieldTypes = new Type[tailFieldCount];
      Array.Copy(FieldTypes, Count - tailFieldCount, fieldTypes, 0, tailFieldCount);
      return new TupleDescriptor(fieldTypes);
    }

    #endregion

    #region Create<...> methods (generic shortcuts)

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T">Type of the only tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object.</returns>
    public static TupleDescriptor Create<T>() 
      => Create(typeof(T));

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1, T2>() 
      => Create(typeof(T1), typeof(T2));

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1, T2, T3>()
      => Create(typeof(T1), typeof(T2), typeof(T3));

    /// <summary>
    /// Creates new <see cref="TupleDescriptor"/> by its field type(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <returns>Newly created <see cref="TupleDescriptor"/> object</returns>
    public static TupleDescriptor Create<T1, T2, T3, T4>()
      => Create(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

    #endregion

    // Constructors

    private TupleDescriptor(Type[] fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, nameof(fieldTypes));

      FieldTypes = fieldTypes;
      FieldCount = fieldTypes.Length;
      FieldDescriptors = new PackedFieldDescriptor[FieldCount];

      TupleLayout.Configure(fieldTypes, FieldDescriptors, out ValuesLength, out ObjectsLength);
    }

    public TupleDescriptor(SerializationInfo info, StreamingContext context)
    {
      FieldCount = info.GetInt32("FieldCount");
      ValuesLength = info.GetInt32("ValuesLength");
      ObjectsLength = info.GetInt32("ObjectsLength");

      var typeNames = (string[]) info.GetValue("FieldTypes", typeof(string[]));
      FieldDescriptors = (PackedFieldDescriptor[])info.GetValue(
        "FieldDescriptors", typeof(PackedFieldDescriptor[]));

      FieldTypes = new Type[typeNames.Length];
      for (var i = 0; i < typeNames.Length; i++) {
        FieldTypes[i] = typeNames[i].GetTypeFromSerializableForm();
        TupleLayout.ConfigureFieldAccessor(ref FieldDescriptors[i], FieldTypes[i]);
      }
    }

    static TupleDescriptor()
    {
      var types = TupleLayout.KnownTypes;
      foreach (var type1 in types) {
        CachedDescriptors1.Add(type1, new TupleDescriptor(new[] {type1}));
        foreach (var type2 in types) {
          CachedDescriptors2.Add((type1, type2), new TupleDescriptor(new[] {type1, type2}));
          foreach (var type3 in types) {
            CachedDescriptors3.Add((type1, type2, type3), new TupleDescriptor(new[] {type1, type2, type3}));
          }
        }
      }
    }
  }
}
