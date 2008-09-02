// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.02

using System;
using System.Collections.Generic;
using Xtensive.Core;
using System.Linq;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class ComparisonResultNavigator
  {
    private readonly IComparisonResult root;

    public IComparisonResult<T> Find<T>(T node) where T : Node
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");
      return Find<T>(root, comparisonResult => comparisonResult.OriginalValue==node || comparisonResult.NewValue==node, c => false).FirstOrDefault();
    }

    public IEnumerable<IComparisonResult<T>> Find<T>(IComparisonResult baseNode, Predicate<IComparisonResult<T>> select, Predicate<IComparisonResult> isFinal)
    {
      ArgumentValidator.EnsureArgumentNotNull(select, "select");
      baseNode = baseNode ?? root;
      isFinal = isFinal ?? (c => false);
      if (baseNode is IComparisonResult<T> && select((IComparisonResult<T>) baseNode)) {
        yield return (IComparisonResult<T>) baseNode;
      }
      if (!isFinal(baseNode)) {
        foreach (IComparisonResult nestedComparison in baseNode.NestedComparisons) {
          foreach (IComparisonResult<T> comparisonResult in Find(nestedComparison, select, isFinal)) {
            yield return comparisonResult;
          }
        }
      }
    }

    public ComparisonResultNavigator(IComparisonResult root)
    {
      ArgumentValidator.EnsureArgumentNotNull(root, "root");
      if (!root.IsLocked)
        throw new ArgumentException(Resources.Strings.ExComparisonRootNotLocked, "root");
      this.root = root;
    }
  }
}