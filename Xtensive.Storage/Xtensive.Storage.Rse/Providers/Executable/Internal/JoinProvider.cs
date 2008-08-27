// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.23

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class JoinProvider : SubstitutingProvider<Compilable.JoinProvider>
  {
    private readonly ExecutableProvider left;
    private readonly ExecutableProvider right;
    private readonly bool leftJoin;
    private readonly Pair<int>[] joiningPairs;

    public override ExecutableProvider<Compilable.JoinProvider> BuildSubstitution()
    {
      var leftEnumerable = left.GetService<IOrderedEnumerable<Tuple,Tuple>>();
      var rightEnumerable = right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      if (leftEnumerable == null) {
        if (CheckAbilityToRange())
          return new LoopJoinProvider(Origin, left, right);
        return new NestedLoopJoinProvider(Origin, left, right);
      }
      if (rightEnumerable == null)
        return new NestedLoopJoinProvider(Origin, left, right);
      if (CheckAbilityToMerge()) {
        if (leftJoin)
          return new LeftMergeJoinProvider(Origin, left, right);
        return new MergeJoinProvider(Origin, left, right);
      }
      if (CheckAbilityToRange())
        return new LoopJoinProvider(Origin, left, right);
      return new NestedLoopJoinProvider(Origin, left, right);
    }

    private bool CheckAbilityToMerge()
    {
      if (left.Header.Order.Count == right.Header.Order.Count) {
        for (int i = 0; i < left.Header.Order.Count; i++) {
          var leftOrderItem = left.Header.Order[i];
          var rightOrderItem = right.Header.Order[i];
          if (leftOrderItem.Value != rightOrderItem.Value)
            return false;
          var leftColumn = left.Header.Columns[leftOrderItem.Key];
          var rightColumn = right.Header.Columns[rightOrderItem.Key];
          if (leftColumn != rightColumn)
            return false;
        }
        return true;
      }
      return false;
    }

    private bool CheckAbilityToRange()
    {
      DirectionCollection<int> orderedBy = left.Header.Order;
      bool sequenceEqual = orderedBy
        .Select(pair => pair.Key)
        .Take(joiningPairs.Length)
        .SequenceEqual(joiningPairs.Select(joiningPair => joiningPair.First));
      var orderedEnumerable = left.GetService<IOrderedEnumerable<Tuple,Tuple>>();
      if (orderedEnumerable != null)
        return sequenceEqual;
      return false;
    }


    // Constructor

    public JoinProvider(Compilable.JoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base (origin, left, right)
    {
      this.left = left;
      this.right = right;
      leftJoin = origin.LeftJoin;
      joiningPairs = origin.EqualIndexes;
    }
  }
}