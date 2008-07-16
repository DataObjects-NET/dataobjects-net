// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.30

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class LeftMergeJoinProvider: BinaryExecutableProvider
  {
    private readonly MergeTransform transform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var leftOrdered = Left.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      var rightOrdered = Right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      foreach (Pair<Tuple, Tuple> pair in Joiner.MergeJoinLeft(leftOrdered, rightOrdered)) {
        Tuple rightTuple = pair.Second ?? Tuple.Create(Right.Header.TupleDescriptor);
        yield return transform.Apply(TupleTransformType.Auto, pair.First, rightTuple);
      }
    }


    // Constructor
    
    public LeftMergeJoinProvider(CompilableProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base (origin, left, right)
    {
      transform = new MergeTransform(true, left.Header.TupleDescriptor, right.Header.TupleDescriptor);
    }
  }
}