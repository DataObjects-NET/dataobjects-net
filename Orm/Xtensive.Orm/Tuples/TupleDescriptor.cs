// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
  public readonly struct TupleDescriptor : IEquatable<TupleDescriptor>, IReadOnlyList<Type>, ISerializable
  {
    public static readonly TupleDescriptor Empty = new TupleDescriptor(Array.Empty<Type>());

    internal readonly int ValuesLength;
    internal readonly int ObjectsLength;

    [NonSerialized]
    internal readonly PackedFieldDescriptor[] FieldDescriptors;

    [NonSerialized]
    private readonly Type[] fieldTypes;

    #region IReadOnlyList members

    /// <inheritdoc/>
    public Type this[int fieldIndex]
    {
      get => fieldTypes[fieldIndex];
    }

    /// <inheritdoc/>
    public int Count
    {
      [DebuggerStepThrough]
      get => fieldTypes.Length;
    }

    /// <inheritdoc/>
    public IEnumerator<Type> GetEnumerator()
    {
      for (int index = 0, count = Count; index < count; index++) {
        yield return fieldTypes[index];
      }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Creates tuple descriptor containing head of the current one.
    /// </summary>
    /// <param name="fieldCount">Head field count.</param>
    /// <returns>
    /// New tuple descriptor describing the specified set of fields.
    /// </returns>
    public TupleDescriptor Head(int fieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldCount, 1, Count, nameof(fieldCount));
      var fieldTypes = new Type[fieldCount];
      Array.Copy(this.fieldTypes, 0, fieldTypes, 0, fieldCount);
      return new TupleDescriptor(fieldTypes);
    }

    /// <summary>
    /// Creates tuple descriptor containing tail of the current one.
    /// </summary>
    /// <param name="tailFieldCount">Tail field count.</param>
    /// <returns>
    /// New tuple descriptor describing the specified set of fields.
    /// </returns>
    public TupleDescriptor Tail(int tailFieldCount)
    {
      ArgumentValidator.EnsureArgumentIsInRange(tailFieldCount, 1, Count, nameof(tailFieldCount));
      var fieldTypes = new Type[tailFieldCount];
      Array.Copy(this.fieldTypes, Count - tailFieldCount, fieldTypes, 0, tailFieldCount);
      return new TupleDescriptor(fieldTypes);
    }

    /// <summary>
    /// Creates tuple descriptor containing segment of the current one
    /// </summary>
    /// <param name="segment">Offset and length of segment in form of Segment</param>
    /// <returns>
    /// New tuple descriptor describing the specified set of fields.
    /// </returns>
    public TupleDescriptor Segment(in Segment<int> segment)
    {
      var fieldTypes = new Type[segment.Length];
      Array.Copy(this.fieldTypes, segment.Offset, fieldTypes, 0, segment.Length);

      return new TupleDescriptor(fieldTypes);
    }

    /// <summary>
    /// Concats fields of the current and the given descriptors to form new one.
    /// </summary>
    /// <param name="second">Tail fields descriptor.</param>
    /// <returns>New tuple descriptor containing fields of the both given source descriptors.</returns>
    public TupleDescriptor ConcatWith(in TupleDescriptor second)
    {
      var firstLength = this.fieldTypes.Length;
      var secondLength = second.fieldTypes.Length;
      var fieldTypes = new Type[firstLength + secondLength];
      Array.Copy(this.fieldTypes, fieldTypes, firstLength);
      Array.Copy(second.fieldTypes, 0, fieldTypes, firstLength, secondLength);

      return new TupleDescriptor(fieldTypes);
    }

    #region IEquatable members, GetHashCode

    /// <inheritdoc/>
    public bool Equals(TupleDescriptor other)
    {
      if (fieldTypes == null) {
        return other.fieldTypes == null;
      }
      if (other.fieldTypes == null || Count != other.Count) {
        return false;
      }

      for (int i = 0, count = Count; i < count; i++) {
        if (fieldTypes[i] != other.fieldTypes[i]) {
          return false;
        }
      }
      return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is TupleDescriptor other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = Count;
      for (int i = 0, count = Count; i < count; i++)
        result = unchecked (fieldTypes[i].GetHashCode() + 29 * result);
      return result;
    }

    public static bool operator ==(in TupleDescriptor left, in TupleDescriptor right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(in TupleDescriptor left, in TupleDescriptor right)
    {
      return !(left == right);
    }

    #endregion

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(nameof(ValuesLength), ValuesLength);
      info.AddValue(nameof(ObjectsLength), ObjectsLength);

      var typeNames = new string[fieldTypes.Length];
      for (var i = 0; i < typeNames.Length; i++)
        typeNames[i] = fieldTypes[i].ToSerializableForm();

      info.AddValue(nameof(fieldTypes), typeNames);
      info.AddValue(nameof(FieldDescriptors), FieldDescriptors);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new ValueStringBuilder(stackalloc char[4096]);
      for (int i = 0, count = Count; i < count; i++) {
        if (i > 0)
          sb.Append(", ");
        sb.Append(fieldTypes[i].GetShortName());
      }
      return string.Format(Strings.TupleDescriptorFormat, sb.ToString());
    }

    #region Create methods (base)

    public static TupleDescriptor Create(Type t1)
    {
      return new TupleDescriptor(new [] {t1});
    }

    public static TupleDescriptor Create(Type t1, Type t2)
    {
      return new TupleDescriptor(new [] {t1, t2});
    }

    public static TupleDescriptor Create(Type t1, Type t2, Type t3)
    {
      return new TupleDescriptor(new [] {t1, t2, t3});
    }

    public static TupleDescriptor Create(Type t1, Type t2, Type t3, Type t4)
    {
      return  new TupleDescriptor(new [] {t1, t2, t3, t4});
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
      ArgumentNullException.ThrowIfNull(fieldTypes);
      if (fieldTypes.Length == 0) {
        return Empty;
      }
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
      var fieldCount = fieldTypes.Length;
      this.fieldTypes = fieldTypes;
      FieldDescriptors = new PackedFieldDescriptor[fieldCount];

      switch (fieldCount) {
        case 0:
          ValuesLength = 0;
          ObjectsLength = 0;
          return;
        case 1:
          TupleLayout.ConfigureLen1(ref this.fieldTypes[0],
            ref FieldDescriptors[0],
            out ValuesLength, out ObjectsLength);
          break;
        case 2:
          TupleLayout.ConfigureLen2(this.fieldTypes,
            ref FieldDescriptors[0], ref FieldDescriptors[1],
            out ValuesLength, out ObjectsLength);
          break;
        default:
          TupleLayout.Configure(this.fieldTypes, FieldDescriptors, out ValuesLength, out ObjectsLength);
          break;
      }
    }

    public TupleDescriptor(SerializationInfo info, StreamingContext context)
    {
      ValuesLength = info.GetInt32(nameof(ValuesLength));
      ObjectsLength = info.GetInt32(nameof(ObjectsLength));

      var typeNames = (string[]) info.GetValue(nameof(fieldTypes), typeof(string[]));
      FieldDescriptors = (PackedFieldDescriptor[])info.GetValue(
        nameof(FieldDescriptors), typeof(PackedFieldDescriptor[]));

      fieldTypes = new Type[typeNames.Length];
      for (var i = 0; i < typeNames.Length; i++) {
        fieldTypes[i] = typeNames[i].GetTypeFromSerializableForm();
        TupleLayout.ConfigureFieldAccessor(ref FieldDescriptors[i], fieldTypes[i]);
      }
    }
  }
}
