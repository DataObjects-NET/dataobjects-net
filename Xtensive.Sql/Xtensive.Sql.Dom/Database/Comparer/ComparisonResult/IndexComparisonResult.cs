// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Index"/> comparison result.
  /// </summary>
  [Serializable]
  public class IndexComparisonResult : NodeComparisonResult<Index>
  {
    private readonly ComparisonResult<bool> isUnique = new ComparisonResult<bool>();
    private readonly ComparisonResult<bool> isBitmap = new ComparisonResult<bool>();
    private readonly ComparisonResult<bool> isClustered = new ComparisonResult<bool>();
    private readonly ComparisonResult<byte?> fillFactor = new ComparisonResult<byte?>();
    private readonly ComparisonResult<string> filegroup = new ComparisonResult<string>();
    private readonly ComparisonResultCollection<IndexColumnComparisonResult> columns = new ComparisonResultCollection<IndexColumnComparisonResult>();
    private readonly ComparisonResultCollection<DataTableColumnComparisonResult> nonkeyColumns = new ComparisonResultCollection<DataTableColumnComparisonResult>();

    /// <summary>
    /// Gets comparison result of <see cref="Index.IsUnique"/> property.
    /// </summary>
    public ComparisonResult<bool> IsUnique
    {
      get { return isUnique; }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.IsBitmap"/> property.
    /// </summary>
    public ComparisonResult<bool> IsBitmap
    {
      get { return isBitmap; }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.IsClustered"/> property.
    /// </summary>
    public ComparisonResult<bool> IsClustered
    {
      get { return isClustered; }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.FillFactor"/> property.
    /// </summary>
    public ComparisonResult<byte?> FillFactor
    {
      get { return fillFactor; }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.Filegroup"/> property.
    /// </summary>
    public ComparisonResult<string> Filegroup
    {
      get { return filegroup; }
    }

    /// <summary>
    /// Gets comparison results of nested columns.
    /// </summary>
    public ComparisonResultCollection<IndexColumnComparisonResult> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets comparison results of nested non-key columns.
    /// </summary>
    public ComparisonResultCollection<DataTableColumnComparisonResult> NonkeyColumns
    {
      get { return nonkeyColumns; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(recursive);
        nonkeyColumns.Lock(recursive);
        isUnique.Lock(recursive);
        isBitmap.Lock(recursive);
        isClustered.Lock(recursive);
        fillFactor.Lock(recursive);
        filegroup.Lock(recursive);
      }
    }
  }
}