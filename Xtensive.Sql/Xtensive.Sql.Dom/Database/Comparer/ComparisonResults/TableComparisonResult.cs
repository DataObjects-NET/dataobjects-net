// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using System;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Table comparison result.
  /// </summary>
  [Serializable]
  public class TableComparisonResult : ComparisonResult<Table>
  {
    private readonly CollectionBaseSlim<TableColumnComparisonResult> columns = new CollectionBaseSlim<TableColumnComparisonResult>();
    private readonly CollectionBaseSlim<IndexComparisonResult> indexes = new CollectionBaseSlim<IndexComparisonResult>();
    private readonly CollectionBaseSlim<ConstraintComparisonResult> constraints = new CollectionBaseSlim<ConstraintComparisonResult>();

    /// <summary>
    /// Gets column comparison results.
    /// </summary>
    public CollectionBaseSlim<TableColumnComparisonResult> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets index comparison results.
    /// </summary>
    public CollectionBaseSlim<IndexComparisonResult> Indexes
    {
      get { return indexes; }
    }

    /// <summary>
    /// Gets constraint comparison results.
    /// </summary>
    public CollectionBaseSlim<ConstraintComparisonResult> Constraints
    {
      get { return constraints; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(true);
        indexes.Lock(true);
        constraints.Lock(true);
      }
    }
  }
}