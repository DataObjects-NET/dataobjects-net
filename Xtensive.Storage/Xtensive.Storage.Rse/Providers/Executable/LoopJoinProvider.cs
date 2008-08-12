// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class LoopJoinProvider : BinaryExecutableProvider
  {
    private readonly bool leftJoin;
    private readonly Pair<int>[] joiningPairs;
    private CombineTransform transform;
    private MapTransform leftKeyTransform;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var rightOrdered = Right.GetService<IOrderedEnumerable<Tuple, Tuple>>(true);
      var left = Left.Enumerate(context);
      foreach (Pair<Tuple, Tuple> pair in leftJoin ? left.LoopJoinLeft(rightOrdered, KeyExtractorLeft) : left.LoopJoin(rightOrdered, KeyExtractorLeft)) {
        Tuple rightTuple = pair.Second ?? Tuple.Create(Right.Header.TupleDescriptor);
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
    }


    // Constructors

    public LoopJoinProvider(CompilableProvider origin, ExecutableProvider left, ExecutableProvider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base(origin, left, right)
    {
      this.leftJoin = leftJoin;
      this.joiningPairs = joiningPairs;      
    }
  }
}