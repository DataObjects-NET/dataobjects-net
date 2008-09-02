// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.26

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Comparison result.
  /// </summary>
  public interface IComparisonResult : ILockable
  {
    /// <summary>
    /// Gets <see langword="true"/> if result contains changes, otherwise gets <see langword="false"/>.
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// Gets comparison type.
    /// </summary>
    ComparisonResultType ResultType { get; }

    /// <summary>
    /// Gets comparison results of nested objects if any.
    /// </summary>
    IEnumerable<IComparisonResult> NestedComparisons { get; }
  }
}