// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="View"/> comparison result.
  /// </summary>
  [Serializable]
  public class ViewComparisonResult : NodeComparisonResult<View>
  {
    private IComparisonResult<CheckOptions> checkOptions;
    private IComparisonResult<SqlNative> definition;
    private readonly ComparisonResultCollection<IComparisonResult<View>> views = new ComparisonResultCollection<IComparisonResult<View>>();
    private readonly ComparisonResultCollection<IComparisonResult<ViewColumn>> columns = new ComparisonResultCollection<IComparisonResult<ViewColumn>>();
    private readonly ComparisonResultCollection<IComparisonResult<Index>> indexes = new ComparisonResultCollection<IComparisonResult<Index>>();

    /// <summary>
    /// Gets comparison result of check options.
    /// </summary>
    public IComparisonResult<CheckOptions> CheckOptions
    {
      get { return checkOptions; }
      set
      {
        this.EnsureNotLocked();
        checkOptions = value;
      }
    }

    /// <summary>
    /// Gets comparison result of definition.
    /// </summary>
    public IComparisonResult<SqlNative> Definition
    {
      get { return definition; }
      set
      {
        this.EnsureNotLocked();
        definition = value;
      }
    }

    /// <summary>
    /// Gets comparison results of nested views.
    /// </summary>
    public ComparisonResultCollection<IComparisonResult<View>> Views
    {
      get { return views; }
    }

    /// <summary>
    /// Gets comparison results of nested columns.
    /// </summary>
    public ComparisonResultCollection<IComparisonResult<ViewColumn>> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets comparison results of nested indexes.
    /// </summary>
    public ComparisonResultCollection<IComparisonResult<Index>> Indexes
    {
      get { return indexes; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        views.Lock(recursive);
        columns.Lock(recursive);
        indexes.Lock(recursive);
        checkOptions.LockSafely(recursive);
        definition.LockSafely(recursive);
      }
    }
  }
}