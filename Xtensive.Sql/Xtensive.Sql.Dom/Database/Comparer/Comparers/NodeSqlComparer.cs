// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.25

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class NodeSqlComparer<TNode, TResult> : SqlComparerBase<TNode> 
    where TNode:Node 
    where TResult : ComparisonResult<TNode>, new()
  {
    public override ComparisonResult<TNode> Compare(TNode originalNode, TNode newNode, IEnumerable<ComparisonHintBase> hints)
    {
      TResult result = InitializeResult<TNode, TResult>(originalNode, newNode);
      result.Lock(true);
      return result;
    }

    public NodeSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}