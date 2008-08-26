// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Index"/> comparison result.
  /// </summary>
  [Serializable]
  public class IndexComparisonResult : NodeComparisonResult<Index>
  {
    private IComparisonResult<bool> isUnique;
    private IComparisonResult<bool> isBitmap;
    private IComparisonResult<bool> isClustered;
    private IComparisonResult<byte?> fillFactor;
    private IComparisonResult<string> filegroup;
    private readonly ComparisonResultCollection<IndexColumnComparisonResult> columns = new ComparisonResultCollection<IndexColumnComparisonResult>();
    private readonly ComparisonResultCollection<DataTableColumnComparisonResult> nonkeyColumns = new ComparisonResultCollection<DataTableColumnComparisonResult>();

    /// <summary>
    /// Gets comparison result of <see cref="Index.IsUnique"/> property.
    /// </summary>
    public IComparisonResult<bool> IsUnique
    {
      get { return isUnique; }
      internal set
      {
        this.EnsureNotLocked();
        isUnique = value;
      }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.IsBitmap"/> property.
    /// </summary>
    public IComparisonResult<bool> IsBitmap
    {
      get { return isBitmap; }
      internal set
      {
        this.EnsureNotLocked();
        isBitmap = value;
      }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.IsClustered"/> property.
    /// </summary>
    public IComparisonResult<bool> IsClustered
    {
      get { return isClustered; }
      internal set
      {
        this.EnsureNotLocked();
        isClustered = value;
      }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.FillFactor"/> property.
    /// </summary>
    public IComparisonResult<byte?> FillFactor
    {
      get { return fillFactor; }
      internal set
      {
        this.EnsureNotLocked();
        fillFactor = value;
      }
    }

    /// <summary>
    /// Gets comparison result of <see cref="Index.Filegroup"/> property.
    /// </summary>
    public IComparisonResult<string> Filegroup
    {
      get { return filegroup; }
      internal set
      {
        this.EnsureNotLocked();
        filegroup = value;
      }
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
        isUnique.LockSafely(recursive);
        isBitmap.LockSafely(recursive);
        isClustered.LockSafely(recursive);
        fillFactor.LockSafely(recursive);
        filegroup.LockSafely(recursive);
      }
    }
  }
}