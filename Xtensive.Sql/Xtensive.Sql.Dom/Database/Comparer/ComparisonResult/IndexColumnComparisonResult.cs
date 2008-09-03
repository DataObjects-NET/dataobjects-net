// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="IndexColumn"/> comparison result.
  /// </summary>
  [Serializable]
  public class IndexColumnComparisonResult : NodeComparisonResult, 
    IComparisonResult<IndexColumn>
  {
    private DataTableColumnComparisonResult column;
    private ComparisonResult<bool> ascending;

    /// <summary>
    /// Gets underlying <see cref="DataTableColumn"/> comparison result.
    /// </summary>
    public DataTableColumnComparisonResult Column
    {
      get { return column; }
      internal set
      {
        this.EnsureNotLocked();
        column = value;
      }
    }

    /// <summary>
    /// Gets comparison result of <see cref="IndexColumn.Ascending"/> property.
    /// </summary>
    public ComparisonResult<bool> Ascending
    {
      get { return ascending; }
      internal set
      {
        this.EnsureNotLocked();
        ascending = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        column.LockSafely(recursive);
        ascending.LockSafely(recursive);
      }
    }

    public new IndexColumn NewValue
    {
      get { return (IndexColumn) base.NewValue; }
    }

    public new IndexColumn OriginalValue
    {
      get { return (IndexColumn) base.OriginalValue; }
    }

    /// <inheritdoc/>
    public override System.Collections.Generic.IEnumerable<IComparisonResult> NestedComparisons
    {
      get
      {
        return base.NestedComparisons
          .AddOne(column)
          .AddOne(ascending);
      }
    }
  }
}