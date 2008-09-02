// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using System.Linq;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class UniqueConstraintComparisonResult : ConstraintComparisonResult, 
    IComparisonResult<UniqueConstraint>
  {
    private readonly ComparisonResultCollection<TableColumnComparisonResult> columns = new ComparisonResultCollection<TableColumnComparisonResult>();

    /// <inheritdoc/>
    public new UniqueConstraint NewValue
    {
      get { return (UniqueConstraint) base.NewValue; }
    }

    /// <inheritdoc/>
    public new UniqueConstraint OriginalValue
    {
      get { return (UniqueConstraint) base.OriginalValue; }
    }

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
          .Union<IComparisonResult>(columns);
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        columns.Lock(recursive);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public UniqueConstraintComparisonResult(UniqueConstraint originalValue, UniqueConstraint newValue)
      : base(originalValue, newValue)
    {
    }
  }
}