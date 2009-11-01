// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.05

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// An implementation of <see cref="IEntire{T}"/>, which is bound to index segment.
  /// </summary>
  /// <typeparam name="T">The type of underlying entire value.</typeparam>
  public struct SegmentBoundEntire<T>: IEntire<SegmentBound<T>>
  {
    private readonly IEntire<T> entire;
    private readonly int segmentNumber;

    /// <summary>
    /// Gets the wrapped entire.
    /// </summary>
    /// <value>The wrapped entire.</value>
    public IEntire<T> Entire
    {
      get { return entire; }
    }

    /// <summary>
    /// Gets the number of segment this entire belongs to.
    /// </summary>
    public int SegmentNumber
    {
      get { return segmentNumber; }
    }

    #region IEntire<SegmentBound<T>> Members

    /// <inheritdoc/>
    public TupleDescriptor Descriptor
    {
      get { return entire.Descriptor; }
    }

    /// <inheritdoc/>
    public int Count
    {
      get { return entire.Count; }
    }

    /// <inheritdoc/>
    public SegmentBound<T> Value
    {
      get { return new SegmentBound<T>(entire.Value, segmentNumber); }
    }

    /// <inheritdoc/>
    public EntireValueType[] ValueTypes
    {
      get { return entire.ValueTypes; }
    }

    /// <inheritdoc/>
    public object this[int fieldIndex]
    {
      get { return Entire[fieldIndex]; }
      set { Entire[fieldIndex] = value; }
    }

    /// <inheritdoc/>
    public TupleFieldState GetFieldState(int fieldIndex)
    {
      return entire.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public EntireValueType GetValueType(int fieldIndex)
    {
      return entire.GetValueType(fieldIndex);
    }

    /// <inheritdoc/>
    public bool IsAvailable(int fieldIndex)
    {
      return entire.IsAvailable(fieldIndex);
    }

    /// <inheritdoc/>
    public bool IsNull(int fieldIndex)
    {
      return entire.IsNull(fieldIndex);
    }

    /// <inheritdoc/>
    public bool HasValue(int fieldIndex)
    {
      return entire.HasValue(fieldIndex);
    }

    /// <inheritdoc/>
    public TFieldValue GetValue<TFieldValue>(int fieldIndex)
    {
      return entire.GetValue<TFieldValue>(fieldIndex);
    }

    /// <inheritdoc/>
    public object GetValue(int fieldIndex)
    {
      return entire.GetValue(fieldIndex);
    }

    /// <inheritdoc/>
    public TFieldValue GetValueOrDefault<TFieldValue>(int fieldIndex)
    {
      return entire.GetValueOrDefault<TFieldValue>(fieldIndex);
    }

    /// <inheritdoc/>
    public object GetValueOrDefault(int fieldIndex)
    {
      return entire.GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc/>
    public void SetValue<TFieldValue>(int fieldIndex, TFieldValue fieldValue)
    {
      entire.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    public void SetValue(int fieldIndex, object fieldValue)
    {
      entire.SetValue(fieldIndex, fieldValue);
    }

    /// <inheritdoc/>
    ITuple ITuple.Clone()
    {
      return entire.Clone();
    }

    /// <inheritdoc/>
    public object Clone()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public ITuple CreateNew()
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public int CompareTo(SegmentBound<T> other)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Equals(SegmentBound<T> other)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public int CompareTo(IEntire<SegmentBound<T>> other)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Equals(IEntire<SegmentBound<T>> other)
    {
      throw new NotImplementedException();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="entire"><see cref="Entire"/> property value.</param>
    /// <param name="segmentNumber"><see cref="SegmentNumber"/> property value.</param>
    public SegmentBoundEntire(IEntire<T> entire, int segmentNumber)
    {
      this.entire = entire;
      this.segmentNumber = segmentNumber;
    }
  }
}