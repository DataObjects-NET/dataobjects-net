// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.22

using System;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  ///<summary>
  /// Default implementation of <see cref="RangeHandler{TNode,TPoint}"/>. 
  ///</summary>
  ///<remarks>
  /// Casts <typeparamref name="TNode"/> to <see cref="IHasRange{TObject,TPoint}"/> and calls interface methods. If cast is invalid, throws <see cref="InvalidOperationException"/> exception.
  ///</remarks>
  ///<typeparam name="TNode">Type of node.</typeparam>
  ///<typeparam name="TPoint">Type of <see cref="Range{T}"/>' point.</typeparam>
  public class DefaultRangeHandler<TNode, TPoint>: RangeHandler<TNode, TPoint>
    where TNode : IHasRange<TNode, TPoint>
  {
    public override TNode Intersect(TNode x, Range<TPoint> y)
    {
      return x.Intersect(y, Comparer);
    }

    public override FixedList3<TNode> Union(TNode x, TNode y)
    {
      return x.Merge(y, Comparer);
    }

    public override FixedList3<TNode> Subtract(TNode x, Range<TPoint> y)
    {
      return x.Subtract(y, Comparer);
    }

    
    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    ///<param name="comparer">Point comparer.</param>
    ///<exception cref="InvalidOperationException"><typeparamref name="TNode"/> could not be casted to <see cref="IHasRange{TObject,TPoint}"/> interface.</exception>
    public DefaultRangeHandler(AdvancedComparer<TPoint> comparer)
      : base(delegate(TNode node) { return node.Range; }, comparer)
    {
    }
  }
}