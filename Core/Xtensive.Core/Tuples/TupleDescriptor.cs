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
using System.Text;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Reflection;
using Xtensive.Resources;
using Xtensive.Tuples.Packed;

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
    IList<Type>
  {
    private static readonly TupleDescriptor empty = Create(new Type[0]);

    private int cachedHashCode;

    internal readonly int FieldCount;
    internal readonly Type[] FieldTypes;

    /// <summary>
    /// Gets the empty tuple descriptor.
    /// </summary>
    /// <value>The empty tuple descriptor.</value>
    public static TupleDescriptor Empty
    {
      [DebuggerStepThrough]
      get { return empty; }
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
    [Obsolete("This property always returns 0.")]
    public int Identifier
    {
      [DebuggerStepThrough]
      get { return 0; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
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
      if (other==null)
        return false;
      if (other==this)
        return true;
      if (FieldCount!=other.FieldCount)
        return false;
      var result = true;
      for (int i = FieldCount - 1; i >= 0 && result; i--)
        result &= FieldTypes[i]==other.FieldTypes[i];
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
        int hashCode = FieldCount;
        for (int i = 0; i < FieldCount; i++)
          hashCode = unchecked (FieldTypes[i].GetHashCode() + 29 * hashCode);
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
      if (other==null)
        return 1;
      if (other==this)
        return 0;
      return AdvancedComparerStruct<Type[]>.Default.Compare(FieldTypes, other.FieldTypes);
    }

    #endregion

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
      return new PackedTupleDescriptor(fieldTypes.ToList());
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
      return new PackedTupleDescriptor(fieldTypes.ToList());
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
      return Create(new[] {
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
      return Create(new[] {
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
      return Create(new[] {
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
      return Create(new[] {
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
      return Create(new[] {
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
      return Create(new[] {
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fieldTypes">The types of <see cref="TupleDescriptor"/> fields.</param>
    protected TupleDescriptor(IList<Type> fieldTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldTypes, "fieldTypes");
      FieldCount = fieldTypes.Count;
      FieldTypes = new Type[FieldCount];
      for (int i = 0; i < FieldCount; i++) {
        Type t = fieldTypes[i];
        if (t.IsNullable())
          t = t.GetGenericArguments()[0]; // Substituting Nullable<T> to T
        FieldTypes[i] = t;
      }
    }
  }
}
