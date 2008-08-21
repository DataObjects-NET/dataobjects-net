// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using Xtensive.Sql.Dom.Dml;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// View comparison result.
  /// </summary>
  public class ViewComparisonResult : ComparisonResult<View>
  {
    private ComparisonResult<string> dbName;
    private ComparisonResult<CheckOptions> checkOptions;
    private ComparisonResult<SqlNative> definition;
    private readonly ComparisonResultCollection<ComparisonResult<View>> views = new ComparisonResultCollection<ComparisonResult<View>>();
    private readonly ComparisonResultCollection<ComparisonResult<ViewColumn>> columns = new ComparisonResultCollection<ComparisonResult<ViewColumn>>();
    private readonly ComparisonResultCollection<ComparisonResult<Index>> indexes = new ComparisonResultCollection<ComparisonResult<Index>>();


    /// <summary>
    /// Gets comparison result of db name.
    /// </summary>
    public ComparisonResult<string> DbName
    {
      get { return dbName; }
      internal set
      {
        this.EnsureNotLocked();
        dbName = value;
      }
    }

    /// <summary>
    /// Gets comparison result of check options.
    /// </summary>
    public ComparisonResult<CheckOptions> CheckOptions
    {
      get { return checkOptions; }
      internal set
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
      internal set
      {
        this.EnsureNotLocked();
        definition = value;
      }
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
        dbName.Lock(recursive);
        checkOptions.Lock(recursive);
        definition.Lock(recursive);
      }
    }
  }
}