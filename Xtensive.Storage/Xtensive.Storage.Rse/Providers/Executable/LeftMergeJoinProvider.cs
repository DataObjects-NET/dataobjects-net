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
  internal sealed class LeftMergeJoinProvider: ExecutableProvider
  {
    private readonly Provider left;
    private readonly Provider right;
    private readonly MergeTransform transform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var leftOrdered = left.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      var rightOrdered = right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      foreach (Pair<Tuple, Tuple> pair in Joiner.MergeJoinLeft(leftOrdered, rightOrdered)) {
        Tuple rightTuple = pair.Second ?? Tuple.Create(right.Header.TupleDescriptor);
        yield return transform.Apply(TupleTransformType.Auto, pair.First, rightTuple);
      }
    }


    // Constructor
    
    public LeftMergeJoinProvider(CompilableProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base (origin, left, right)
    {
      this.left = left;
      this.right = right;
      transform = new MergeTransform(true, left.Header.TupleDescriptor, right.Header.TupleDescriptor);
    }
  }
}