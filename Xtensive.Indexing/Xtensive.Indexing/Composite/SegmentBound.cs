// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.29

using System;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Index segment item.
  /// </summary>
  public struct SegmentBound<T>:
    IComparable<SegmentBound<T>>,
    IEquatable<SegmentBound<T>>
  {
    private T value;
    private int segmentNumber;

    /// <summary>
    /// Gets the index segment.
    /// </summary>
    /// <value>The index segment.</value>
    public int SegmentNumber
    {
      get { return segmentNumber; }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value
    {
      get { return value; }
    }

    #region IComparable<SegmentBound<T>> Members

    /// <inheritdoc/>
    public int CompareTo(SegmentBound<T> other)
    {
      return AdvancedComparerStruct<SegmentBound<T>>.System.Compare(this, other);
    }

    #endregion

    #region IEquatable<SegmentBound<T>> Members

    /// <inheritdoc/>
    public bool Equals(SegmentBound<T> other)
    {
      return AdvancedComparerStruct<SegmentBound<T>>.System.Equals(this, other);
    }

    #endregion


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="item">The indexed item.</param>
    public SegmentBound(T item)
      : this(item, 0)
    {
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="item">The indexed item.</param>
    /// <param name="segmentNumber">The index segment.</param>
    public SegmentBound(T item, int segmentNumber)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      this.value = item;
      this.segmentNumber = segmentNumber;
    }
  }
}