// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class CheckConstraintComparisonResult : NodeComparisonResult<CheckConstraint>,
    IComparisonResult<Constraint>
  {
    /// <inheritdoc/>
    public Constraint NewValue
    {
      get { return base.NewValue; }
    }

    /// <inheritdoc/>
    public Constraint OriginalValue
    {
      get { return base.OriginalValue; }
    }
  }
}