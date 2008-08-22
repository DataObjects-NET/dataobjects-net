// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Table"/> comparison result.
  /// </summary>
  [Serializable]
  public class TableComparisonResult : NodeComparisonResult<Table>
  {
    private readonly ComparisonResult<string> filegroup = new ComparisonResult<string>();
    private readonly ComparisonResultCollection<IndexComparisonResult> indexes = new ComparisonResultCollection<IndexComparisonResult>();
    private readonly ComparisonResultCollection<TableColumnComparisonResult> columns = new ComparisonResultCollection<TableColumnComparisonResult>();
    private readonly ComparisonResultCollection<TableConstraintComparisonResult> constraints = new ComparisonResultCollection<TableConstraintComparisonResult>();

    /// <summary>
    /// Gets comparison result of filegroup.
    /// </summary>
    public ComparisonResult<string> Filegroup
    {
      get { return filegroup; }
    }

    /// <summary>
    /// Gets comparison results of nested indexes.
    /// </summary>
    public ComparisonResultCollection<IndexComparisonResult> Indexes
    {
      get { return indexes; }
    }

    /// <summary>
    /// Gets comparison results of nested columns.
    /// </summary>
    public ComparisonResultCollection<TableColumnComparisonResult> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets comparison results of nested constraints.
    /// </summary>
    public ComparisonResultCollection<TableConstraintComparisonResult> Constraints
    {
      get { return constraints; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        filegroup.Lock(recursive);
        indexes.Lock(recursive);
        columns.Lock(recursive);
        constraints.Lock(recursive);
     }
    }
  }
}