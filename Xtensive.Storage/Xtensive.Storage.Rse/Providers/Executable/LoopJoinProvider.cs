// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.21

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  internal sealed class LoopJoinProvider : BinaryExecutableProvider
  {
    private readonly bool leftJoin;
    private readonly MergeTransform transform;
    private readonly MapTransform leftKeyTransform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var rightOrdered = Right.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      foreach (Pair<Tuple, Tuple> pair in leftJoin ? Joiner.LoopJoinLeft(left, rightOrdered, KeyExtractorLeft) : Joiner.LoopJoin(left, rightOrdered, KeyExtractorLeft)) {
        Tuple rightTuple = pair.Second;
        if (rightTuple == null)
          rightTuple = Tuple.Create(Right.Header.TupleDescriptor);
        yield return transform.Apply(TupleTransformType.Auto, pair.First, rightTuple);
      }
    }

    private Tuple KeyExtractorLeft(Tuple input)
    {
      return leftKeyTransform.Apply(TupleTransformType.Auto, input);
    }


    // Constructors

    public LoopJoinProvider(CompilableProvider origin, ExecutableProvider left, ExecutableProvider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base(origin, left, right)
    {
      this.leftJoin = leftJoin;
      transform = new MergeTransform(true, left.Header.TupleDescriptor, right.Header.TupleDescriptor);
      int[] map = joiningPairs.Select(pair => pair.First).ToArray();
      TupleDescriptor leftKeyDescriptor = TupleDescriptor.Create(map.Select(i => left.Header.TupleDescriptor[i]));
      leftKeyTransform = new MapTransform(true, leftKeyDescriptor, map);
    }
  }
}