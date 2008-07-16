// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.23

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  public sealed class JoinProvider : ProviderProxy
  {
    private readonly Provider left;
    private readonly Provider right;
    private readonly bool leftJoin;
    private readonly Pair<int>[] joiningPairs;

    public override Provider GetRealProvider()
    {
      if (!left.Options.IsIndexed) {
        if (CheckAbilityToRange())
          return new LoopJoinProvider(Header, left, right, leftJoin, joiningPairs);
        return new NestedLoopJoinProvider(Header, left, right, leftJoin, joiningPairs);
      }
      if (!right.Options.IsIndexed)
        return new NestedLoopJoinProvider(Header, left, right, leftJoin, joiningPairs);
      if (CheckAbilityToMerge()) {
        if (leftJoin)
          return new LeftMergeJoinProvider(Header, left, right);
        return new MergeJoinProvider(Header, left, right);
      }
      if (CheckAbilityToRange())
        return new LoopJoinProvider(Header, left, right, leftJoin, joiningPairs);
      return new NestedLoopJoinProvider(Header, left, right, leftJoin, joiningPairs);
    }

    private bool CheckAbilityToMerge()
    {
      if (left.Options.IsIndexed && right.Options.IsIndexed) {
        if (left.Header.OrderInfo.OrderedBy.Count == right.Header.OrderInfo.OrderedBy.Count) {
          for (int i = 0; i < left.Header.OrderInfo.OrderedBy.Count; i++) {
            var leftOrderItem = left.Header.OrderInfo.OrderedBy[i];
            var rightOrderItem = right.Header.OrderInfo.OrderedBy[i];
            if (leftOrderItem.Value != rightOrderItem.Value)
              return false;
            var leftColumn = left.Header.RecordColumnCollection[leftOrderItem.Key];
            var rightColumn = right.Header.RecordColumnCollection[rightOrderItem.Key];
            if (leftColumn != rightColumn)
              return false;
          }
          return true;
        }
      }
      return false;
    }

    private bool CheckAbilityToRange()
    {
      DirectionCollection<int> orderedBy = left.Header.OrderInfo.OrderedBy;
      var orderedByCount = orderedBy.Count();
      IEnumerable<int> select = orderedBy.Select(pair => pair.Key);
      var selectCount = select.Count();
      IEnumerable<int> take = select.Take(joiningPairs.Length);
      var takeCount = take.Count();
      IEnumerable<int> joiningPairsFirst = joiningPairs.Select(joiningPair => joiningPair.First);
      var joiningPairsCount = joiningPairsFirst.Count();
      bool sequenceEqual = take.SequenceEqual(joiningPairsFirst);
      if (left.Options.IsIndexed)
        return sequenceEqual;
      return false;
    }


    // Constructor

    public JoinProvider(RecordHeader header, Provider left, Provider right, bool leftJoin, params Pair<int>[] joiningPairs)
      : base (header, left, right)
    {
      this.left = left;
      this.right = right;
      this.leftJoin = leftJoin;
      this.joiningPairs = joiningPairs;
    }
  }
}