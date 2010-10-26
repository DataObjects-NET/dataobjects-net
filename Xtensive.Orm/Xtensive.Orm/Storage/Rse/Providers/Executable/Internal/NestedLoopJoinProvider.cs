// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.13

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class NestedLoopJoinProvider : BinaryExecutableProvider<Compilable.JoinProvider>
  {
    private readonly JoinType joinType;
    private readonly Pair<int>[] joiningPairs;
    private CombineTransform transform;
    private MapTransform leftKeyTransform;
    private MapTransform rightKeyTransform;
    private Tuple rightBlank;

    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default;
      IEnumerable<Pair<Tuple, Tuple>> loopJoin = joinType == JoinType.LeftOuter 
        ? Left.NestedLoopJoinLeft(Right, KeyExtractorLeft, KeyExtractorRight, comparer) 
        : Left.NestedLoopJoin(Right, KeyExtractorLeft, KeyExtractorRight, comparer);
      foreach (Pair<Tuple, Tuple> pair in loopJoin)
        yield return transform.Apply(TupleTransformType.Auto, pair.First, pair.Second ?? rightBlank);
    }

    #region Helper methods

    private Tuple KeyExtractorRight(Tuple input)
    {
      return rightKeyTransform.Apply(TupleTransformType.Auto, input);
    }

    private Tuple KeyExtractorLeft(Tuple input)
    {
      return leftKeyTransform.Apply(TupleTransformType.Auto, input);
    }

    #endregion

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      transform = new CombineTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      int[] leftColumns = joiningPairs.Select(pair => pair.First).ToArray();
      int[] rightColumns = joiningPairs.Select(pair => pair.Second).ToArray();
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(leftColumns
        .Select(i => Left.Header.TupleDescriptor[i]));
      TupleDescriptor rightKeyDescriptor = TupleDescriptor.Create(rightColumns
        .Select(i => Right.Header.TupleDescriptor[i]));

      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, leftColumns);
      rightKeyTransform = new MapTransform(true, rightKeyDescriptor, rightColumns);
      rightBlank = Tuple.Create(Right.Header.TupleDescriptor);
      rightBlank.Initialize(new BitArray(Right.Header.Length, true));
    }


    // Constructors

    public NestedLoopJoinProvider(Compilable.JoinProvider origin, ExecutableProvider left,
      ExecutableProvider right)
      : base(origin, left, right)
    {
      joinType = origin.JoinType;
      joiningPairs = origin.EqualIndexes;      
    }
  }
}