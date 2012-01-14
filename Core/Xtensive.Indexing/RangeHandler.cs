// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Comparison;

namespace Xtensive.Indexing
{
  ///<summary>
  /// Represents a wrapper that handles all range-specific operations.
  ///</summary>
  ///<typeparam name="TNode">Node type to which range operations are applied.</typeparam>
  ///<typeparam name="TPoint">Type of the point of <see cref="Range{T}"/>.</typeparam>
  public abstract class RangeHandler<TNode, TPoint>
  {
    private readonly Converter<TNode, Range<TPoint>> rangeExtractor;
    private readonly AdvancedComparer<TPoint> comparer;

    ///<summary>
    /// Gets <see cref="IComparer{T}"/> instance that compares range' points.
    ///</summary>
    public AdvancedComparer<TPoint> Comparer
    {
      [DebuggerStepThrough]
      get { return comparer; }
    }

    /// <summary>
    /// Gets <see cref="Converter{TInput,TOutput}"/> range extractor.
    /// </summary>
    public Converter<TNode, Range<TPoint>> RangeExtractor
    {
      [DebuggerStepThrough]
      get { return rangeExtractor; }
    }

    ///<summary>
    /// Intersects <paramref name="x"/> with <see cref="Range{T}"/> <paramref name="y"/>.
    ///</summary>
    ///<param name="x">Node to intersect with.</param>
    ///<param name="y">Range.</param>
    ///<returns>A result <typeparamref name="TNode"/> of the operation.</returns>
    public abstract TNode Intersect(TNode x, Range<TPoint> y);

    ///<summary>
    /// Unions <paramref name="x"/> with <typeparamref name="TNode"/> <paramref name="y"/>.
    ///</summary>
    ///<param name="x">Node to union with.</param>
    ///<param name="y">Node.</param>
    ///<returns><see cref="FixedList3{T}"/> as result of the operation.</returns>
    public abstract FixedList3<TNode> Union(TNode x, TNode y);

    ///<summary>
    /// Subtracts from <paramref name="x"/> <see cref="Range{T}"/> <paramref name="y"/>.
    ///</summary>
    ///<param name="x">Minuend <typeparamref name="TNode"/>.</param>
    ///<param name="y">Subtrahend <see cref="Range{T}"/>.</param>
    ///<returns><see cref="FixedList3{T}"/> as a result of the operation.</returns>
    public abstract FixedList3<TNode> Subtract(TNode x, Range<TPoint> y);

    protected RangeHandler(Converter<TNode, Range<TPoint>> rangeExtractor, AdvancedComparer<TPoint> comparer)
    {
      this.rangeExtractor = rangeExtractor;
      this.comparer = comparer;
    }
  }
}