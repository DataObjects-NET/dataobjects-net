// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="View"/> comparison result.
  /// </summary>
  [Serializable]
  public class ViewComparisonResult : NodeComparisonResult<View>
  {
    private readonly ComparisonResult<CheckOptions> checkOptions = new ComparisonResult<CheckOptions>();
    private readonly ComparisonResult<SqlNative> definition = new ComparisonResult<SqlNative>();
    private readonly ComparisonResultCollection<ComparisonResult<View>> views = new ComparisonResultCollection<ComparisonResult<View>>();
    private readonly ComparisonResultCollection<ComparisonResult<ViewColumn>> columns = new ComparisonResultCollection<ComparisonResult<ViewColumn>>();
    private readonly ComparisonResultCollection<ComparisonResult<Index>> indexes = new ComparisonResultCollection<ComparisonResult<Index>>();

    /// <summary>
    /// Gets comparison result of check options.
    /// </summary>
    public ComparisonResult<CheckOptions> CheckOptions
    {
      get { return checkOptions; }
    }

    /// <summary>
    /// Gets comparison result of definition.
    /// </summary>
    public ComparisonResult<SqlNative> Definition
    {
      get { return definition; }
    }

    /// <summary>
    /// Gets comparison results of nested views.
    /// </summary>
    public ComparisonResultCollection<ComparisonResult<View>> Views
    {
      get { return views; }
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
        views.Lock(recursive);
        columns.Lock(recursive);
        indexes.Lock(recursive);
        checkOptions.Lock(recursive);
        definition.Lock(recursive);
      }
    }
  }
}