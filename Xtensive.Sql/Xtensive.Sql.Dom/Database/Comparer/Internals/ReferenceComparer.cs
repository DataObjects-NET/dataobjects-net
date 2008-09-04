// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.04

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class ReferenceComparer<T> : NodeComparerBase<T>
    where T : Node
  {
    private NodeComparer<T> comparer;

    public override IComparisonResult<T> Compare(T originalNode, T newNode)
    {
      IComparisonResult comparisonResult;
      if (ComparisonContext.Current.Registry.TryGetValue(originalNode, newNode, out comparisonResult)) {
        var result = new ComparisonResult<T>();
        result.Initialize(originalNode, newNode);
        result.ResultType = comparisonResult.ResultType;
        return result;
      }
      if (comparer == null)
        comparer = Provider.GetNodeComparer<T>();
      return comparer.Compare(originalNode, newNode);
    }

    public ReferenceComparer(INodeComparerProvider provider)
      :base(provider)
    {
    }
  }
}