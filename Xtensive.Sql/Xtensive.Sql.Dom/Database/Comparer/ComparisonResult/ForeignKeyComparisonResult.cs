// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Core.Helpers;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class ForeignKeyComparisonResult : ConstraintComparisonResult,
    IComparisonResult<ForeignKey>
  {
    private readonly ComparisonResultCollection<TableColumnComparisonResult> columns = new ComparisonResultCollection<TableColumnComparisonResult>();
    private readonly ComparisonResultCollection<TableColumnComparisonResult> referencedColumns = new ComparisonResultCollection<TableColumnComparisonResult>();
    private ComparisonResult<SqlMatchType> matchType;
    private ComparisonResult<ReferentialAction> onUpdate;
    private ComparisonResult<ReferentialAction> onDelete;

    public ComparisonResultCollection<TableColumnComparisonResult> Columns
    {
      get { return columns; }
    }

    public ComparisonResultCollection<TableColumnComparisonResult> ReferencedColumns
    {
      get { return referencedColumns; }
    }

    public ComparisonResult<SqlMatchType> MatchType
    {
      get { return matchType; }
      set
      {
        this.EnsureNotLocked();
        matchType = value;
      }
    }

    public ComparisonResult<ReferentialAction> OnUpdate
    {
      get { return onUpdate; }
      set
      {
        this.EnsureNotLocked();
        onUpdate = value;
      }
    }

    public ComparisonResult<ReferentialAction> OnDelete
    {
      get { return onDelete; }
      set
      {
        this.EnsureNotLocked();
        onDelete = value;
      }
    }

    /// <inheritdoc/> 
    public new ForeignKey NewValue
    {
      get { return (ForeignKey) base.NewValue; }
    }

    /// <inheritdoc/>
    public new ForeignKey OriginalValue
    {
      get { return (ForeignKey) base.OriginalValue; }
    }

    /// <inheritdoc/>
    public override System.Collections.Generic.IEnumerable<IComparisonResult> NestedComparisons
    {
      get
      {
        return base.NestedComparisons
          .AddOne(matchType)
          .AddOne(onUpdate)
          .AddOne(onDelete)
          .Union<IComparisonResult>(columns)
          .Union<IComparisonResult>(referencedColumns);
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(recursive);
        referencedColumns.Lock(recursive);
        matchType.LockSafely(recursive);
        onUpdate.LockSafely(recursive);
        onDelete.LockSafely(recursive);
      }
    }
  }
}