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
  public class ViewComparisonResult : DataTableComparisonResult,
    IComparisonResult<View>
  {
    private ComparisonResult<CheckOptions> checkOptions;
    private ComparisonResult<SqlNative> definition;
    private readonly ComparisonResultCollection<ComparisonResult<ViewColumn>> columns = new ComparisonResultCollection<ComparisonResult<ViewColumn>>();
    private readonly ComparisonResultCollection<ComparisonResult<Index>> indexes = new ComparisonResultCollection<ComparisonResult<Index>>();


    /// <inheritdoc/>
    public View NewValue
    {
      get { return (View) base.NewValue; }
    }

    /// <inheritdoc/>
    public View OriginalValue
    {
      get { return (View) base.OriginalValue; }
    }

    /// <summary>
    /// Gets comparison result of check options.
    /// </summary>
    public ComparisonResult<CheckOptions> CheckOptions
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
    public ComparisonResult<SqlNative> Definition
    {
      get { return definition; }
      set
      {
        this.EnsureNotLocked();
        definition = value;
      }
    }

    /// <summary>
    /// Gets comparison results of nested columns.
    /// </summary>
    public ComparisonResultCollection<ComparisonResult<ViewColumn>> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets comparison results of nested indexes.
    /// </summary>
    public ComparisonResultCollection<ComparisonResult<Index>> Indexes
    {
      get { return indexes; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(recursive);
        indexes.Lock(recursive);
        checkOptions.LockSafely(recursive);
        definition.LockSafely(recursive);
      }
    }

    public ViewComparisonResult(View originalValue, View newValue)
      : base(originalValue, newValue)
    {
    }
  }
}