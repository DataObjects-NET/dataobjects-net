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
  public sealed class NestedLoopJoinProvider : ExecutableProvider
  {
    private readonly Provider left;
    private readonly Provider right;
    private readonly MergeTransform transform;
    private readonly MapTransform leftKeyTransform;
    private readonly MapTransform rightKeyTransform;

    public override IEnumerator<Tuple> GetEnumerator()
    {
      AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default;
      int leftCount = left.Count();
      int rightCount = right.Count();
      Log.Debug("LeftCount: {0}", leftCount);
      Log.Debug("RightCount: {0}", rightCount);
      IEnumerable<Pair<Tuple, Tuple>> loopJoin = Joiner.NestedLoopJoin(left, right, KeyExtractorLeft, KeyExtractorRight, comparer);
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


    // Constructors

    public NestedLoopJoinProvider(RecordHeader header, Provider left, Provider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base(header, left, right)
    {
      this.left = left;
      this.right = right;
      transform = new MergeTransform(true, left.Header.TupleDescriptor, right.Header.TupleDescriptor);
      int[] leftColumns = joiningPairs.Select(pair => pair.First).ToArray();
      int[] rightColumns = joiningPairs.Select(pair => pair.Second).ToArray();
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(leftColumns.Select(i => left.Header.TupleDescriptor[i]));
      TupleDescriptor rightKeyDescriptor = TupleDescriptor.Create(leftColumns.Select(i => right.Header.TupleDescriptor[i]));

      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, leftColumns);
      rightKeyTransform = new MapTransform(true, rightKeyDescriptor, rightColumns);
    }
  }
}