// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.30

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class LeftMergeJoinProvider: BinaryExecutableProvider
  {
    private CombineTransform transform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var leftOrdered = Left.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      var rightOrdered = Right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      foreach (Pair<Tuple, Tuple> pair in leftOrdered.MergeJoinLeft(rightOrdered)) {
        Tuple rightTuple = pair.Second ?? Tuple.Create(Right.Header.TupleDescriptor);
        yield return transform.Apply(TupleTransformType.Auto, pair.First, rightTuple);
      }
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
    }


    // Constructor
    
    public LeftMergeJoinProvider(Provider origin, ExecutableProvider left, ExecutableProvider right)
      : base (origin, left, right)
    {      
    }
  }
}