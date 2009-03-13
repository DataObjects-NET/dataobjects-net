// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.23

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using System.Linq;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class JoinProvider : SubstitutingProvider<Compilable.JoinProvider>
  {
    private readonly ExecutableProvider left;
    private readonly ExecutableProvider right;
    private readonly bool outerJoin;
    private readonly Pair<int>[] joiningPairs;
    private IOrderedEnumerable<Tuple, Tuple> leftEnumerable;
    private IOrderedEnumerable<Tuple, Tuple> rightEnumerable;

    public override ExecutableProvider<Compilable.JoinProvider> BuildSubstitution()
    {
      leftEnumerable = left.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      rightEnumerable = right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      switch (Origin.JoinType) {
      case JoinType.NestedLoop:
        return new NestedLoopJoinProvider(Origin, left, right);
      case JoinType.Hash:
        return new HashJoinProvider(Origin, left, right);
      case JoinType.Loop:
        bool isAbleToRange = CheckAbilityToRange();
        if (isAbleToRange)
          return new LoopJoinProvider(Origin, left, right);
        return DefaultProvider(isAbleToRange, CheckAbilityToMerge());
      case JoinType.Merge:
        bool isAbleToMerge = CheckAbilityToMerge();
        if (isAbleToMerge) {
          if (outerJoin)
            return new OuterMergeJoinProvider(Origin, left, right);
          return new MergeJoinProvider(Origin, left, right);
        }
        return DefaultProvider(CheckAbilityToRange(), isAbleToMerge);
      default:
        return DefaultProvider(CheckAbilityToRange(), CheckAbilityToMerge());
      }
    }

    private ExecutableProvider<Compilable.JoinProvider> DefaultProvider(bool isAbleToRange, bool isAbleToMerge)
    {
      if (leftEnumerable==null) {
        if (isAbleToRange)
          return new LoopJoinProvider(Origin, left, right);
        return new NestedLoopJoinProvider(Origin, left, right);
      }
      if (rightEnumerable==null)
        return new NestedLoopJoinProvider(Origin, left, right);
      if (isAbleToMerge) {
        if (outerJoin)
          return new OuterMergeJoinProvider(Origin, left, right);
        return new MergeJoinProvider(Origin, left, right);
      }
      if (isAbleToRange)
        return new LoopJoinProvider(Origin, left, right);
      return new NestedLoopJoinProvider(Origin, left, right);
    }

    private bool CheckAbilityToMerge()
    {
      if (left.Header.Order.Count==right.Header.Order.Count) {
        for (int i = 0; i < left.Header.Order.Count; i++) {
          var leftOrderItem = left.Header.Order[i];
          var rightOrderItem = right.Header.Order[i];
          if (leftOrderItem.Value!=rightOrderItem.Value)
            return false;
          var leftColumn = left.Header.Columns[leftOrderItem.Key];
          var rightColumn = right.Header.Columns[rightOrderItem.Key];
          if (leftColumn!=rightColumn)
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
      var orderedEnumerable = left.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      if (orderedEnumerable!=null)
        return sequenceEqual;
      return false;
    }


    // Constructor

    public JoinProvider(Compilable.JoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      this.left = left;
      this.right = right;
      outerJoin = origin.Outer;
      joiningPairs = origin.EqualIndexes;
    }
  }
}