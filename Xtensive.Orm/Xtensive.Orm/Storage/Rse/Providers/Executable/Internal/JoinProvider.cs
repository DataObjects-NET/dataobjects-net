// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.23

using System;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class JoinProvider : SubstitutingProvider<Compilable.JoinProvider>
  {
    private readonly ExecutableProvider left;
    private readonly ExecutableProvider right;
    private readonly JoinType joinType;
    private readonly Pair<int>[] joiningPairs;
    private IOrderedEnumerable<Tuple, Tuple> leftEnumerable;
    private IOrderedEnumerable<Tuple, Tuple> rightEnumerable;

    public override ExecutableProvider<Compilable.JoinProvider> BuildSubstitution()
    {
      leftEnumerable = left.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      rightEnumerable = right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      switch (Origin.JoinAlgorithm) {
      case JoinAlgorithm.NestedLoop:
        return new NestedLoopJoinProvider(Origin, left, right);
      case JoinAlgorithm.Hash:
        return new HashJoinProvider(Origin, left, right);
      case JoinAlgorithm.Loop:
        bool isAbleToRange = CheckAbilityToRange();
        if (isAbleToRange)
          return new LoopJoinProvider(Origin, left, right);
        return DefaultProvider(isAbleToRange, CheckAbilityToMerge());
      case JoinAlgorithm.Merge:
        bool isAbleToMerge = CheckAbilityToMerge();
        if (isAbleToMerge) {
          if (joinType == JoinType.LeftOuter)
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
        if (joinType == JoinType.LeftOuter)
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
      DirectionCollection<int> orderedBy = right.Header.Order;
      bool sequenceEqual = orderedBy
        .Select(pair => pair.Key)
        .Take(joiningPairs.Length)
        .SequenceEqual(joiningPairs.Select(joiningPair => joiningPair.Second));
      var orderedEnumerable = right.GetService<IOrderedEnumerable<Tuple, Tuple>>();
      if (orderedEnumerable!=null)
        return sequenceEqual;
      return false;
    }


    // Constructors

    public JoinProvider(Compilable.JoinProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      this.left = left;
      this.right = right;
      joinType = origin.JoinType;
      joiningPairs = origin.EqualIndexes;
    }
  }
}