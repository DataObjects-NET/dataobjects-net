// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class UniqueConstraintComparisonResult : NodeComparisonResult<UniqueConstraint>,
    IComparisonResult<Constraint>
  {
    private readonly ComparisonResultCollection<TableColumnComparisonResult> columns = new ComparisonResultCollection<TableColumnComparisonResult>();

    /// <inheritdoc/>
    Constraint IComparisonResult<Constraint>.NewValue
    {
      get { return NewValue; }
    }

    /// <inheritdoc/>
    Constraint IComparisonResult<Constraint>.OriginalValue
    {
      get { return OriginalValue; }
    }

    public ComparisonResultCollection<TableColumnComparisonResult> Columns
    {
      get { return columns; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(recursive);
      }
    }
  }
}