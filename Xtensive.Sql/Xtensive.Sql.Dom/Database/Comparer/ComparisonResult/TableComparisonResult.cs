// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="Table"/> comparison result.
  /// </summary>
  [Serializable]
  public class TableComparisonResult : DataTableComparisonResult,
    IComparisonResult<Table>
  {
    private ComparisonResult<string> filegroup;
    private readonly ComparisonResultCollection<IndexComparisonResult> indexes = new ComparisonResultCollection<IndexComparisonResult>();
    private readonly ComparisonResultCollection<TableColumnComparisonResult> columns = new ComparisonResultCollection<TableColumnComparisonResult>();
    private readonly ComparisonResultCollection<ConstraintComparisonResult> constraints = new ComparisonResultCollection<ConstraintComparisonResult>();

    /// <inheritdoc/>
    public new Table NewValue
    {
      get { return (Table) base.NewValue; }
    }

    /// <inheritdoc/>
    public new Table OriginalValue
    {
      get { return (Table) base.OriginalValue; }
    }

    /// <summary>
    /// Gets comparison result of filegroup.
    /// </summary>
    public ComparisonResult<string> Filegroup
    {
      get { return filegroup; }
      set
      {
        this.EnsureNotLocked();
        filegroup = value;
      }
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

    /// <inheritdoc/>
    public override IEnumerable<IComparisonResult> NestedComparisons
    {
      get
      {
        return base.NestedComparisons
          .AddOne(filegroup)
          .Union<IComparisonResult>(indexes)
          .Union<IComparisonResult>(columns)
          .Union<IComparisonResult>(constraints);
      }
    }

    /// <summary>
    /// Gets comparison results of nested constraints.
    /// </summary>
    public ComparisonResultCollection<ConstraintComparisonResult> Constraints
    {
      get { return constraints; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        filegroup.LockSafely(recursive);
        indexes.Lock(recursive);
        columns.Lock(recursive);
        constraints.Lock(recursive);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TableComparisonResult(Table originalValue, Table newValue)
      : base(originalValue, newValue)
    {
    }
  }
}