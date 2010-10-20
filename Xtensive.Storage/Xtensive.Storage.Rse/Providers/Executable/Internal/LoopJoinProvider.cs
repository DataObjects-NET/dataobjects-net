// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.21

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class LoopJoinProvider : BinaryExecutableProvider<Compilable.JoinProvider>
  {
    private readonly JoinType joinType;
    private readonly Pair<int>[] joiningPairs;
    private CombineTransform transform;
    private MapTransform leftKeyTransform;
    private Tuple rightBlank;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var rightOrdered = Right.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      var left = Left.Enumerate(context);
      foreach (Pair<Tuple, Tuple> pair in joinType == JoinType.LeftOuter 
        ? left.LoopJoinLeft(rightOrdered, KeyExtractorLeft) 
        : left.LoopJoin(rightOrdered, KeyExtractorLeft)) {
        Tuple rightTuple = pair.Second ?? rightBlank;
        yield return transform.Apply(TupleTransformType.Auto, pair.First, rightTuple);
      }
    }

    private Tuple KeyExtractorLeft(Tuple input)
    {
      return leftKeyTransform.Apply(TupleTransformType.Auto, input);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      int[] map = joiningPairs.Select(pair => pair.First).ToArray();
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(map.Select(i => Left.Header.TupleDescriptor[i]));
      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, map);
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
      rightBlank.Initialize(new BitArray(Right.Header.Length, true));
    }


    // Constructors

    public LoopJoinProvider(Compilable.JoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      joinType = origin.JoinType;
      joiningPairs = origin.EqualIndexes;      
    }
  }
}