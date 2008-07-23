// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.13

using System;
using System.Linq;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class NestedLoopJoinProvider : BinaryExecutableProvider
  {
    private readonly bool leftJoin;
    private readonly Pair<int>[] joiningPairs;
    private MergeTransform transform;
    private MapTransform leftKeyTransform;
    private MapTransform rightKeyTransform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default;
      int leftCount = Left.Count();
      int rightCount = Right.Count();
      Log.Debug("LeftCount: {0}", leftCount);
      Log.Debug("RightCount: {0}", rightCount);
      IEnumerable<Pair<Tuple, Tuple>> loopJoin = leftJoin ? Left.NestedLoopJoinLeft(Right, KeyExtractorLeft, KeyExtractorRight, comparer) : Left.NestedLoopJoin(Right, KeyExtractorLeft, KeyExtractorRight, comparer);
      foreach (Pair<Tuple, Tuple> pair in loopJoin)
        yield return transform.Apply(TupleTransformType.Auto, pair.First, pair.Second);
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

    protected override void Initialize()
    {
      transform = new MergeTransform(true, Left.Header.TupleDescriptor, Right.Header.TupleDescriptor);
      int[] leftColumns = joiningPairs.Select(pair => pair.First).ToArray();
      int[] rightColumns = joiningPairs.Select(pair => pair.Second).ToArray();
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(leftColumns.Select(i => Left.Header.TupleDescriptor[i]));
      TupleDescriptor rightKeyDescriptor = TupleDescriptor.Create(leftColumns.Select(i => Right.Header.TupleDescriptor[i]));

      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, leftColumns);
      rightKeyTransform = new MapTransform(true, rightKeyDescriptor, rightColumns);
    }


    // Constructors

    public NestedLoopJoinProvider(Provider origin, ExecutableProvider left, ExecutableProvider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base(origin, left, right)
    {
      this.leftJoin = leftJoin;
      this.joiningPairs = joiningPairs;
      Initialize();
    }
  }
}