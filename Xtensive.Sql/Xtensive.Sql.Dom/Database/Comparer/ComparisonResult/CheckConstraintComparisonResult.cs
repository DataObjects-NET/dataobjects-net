// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class CheckConstraintComparisonResult : ConstraintComparisonResult,
    IComparisonResult<CheckConstraint>
  {
    /// <inheritdoc/>
    public new CheckConstraint NewValue
    {
      get { return (CheckConstraint)base.NewValue; }
    }

    /// <inheritdoc/>
    public new CheckConstraint OriginalValue
    {
      get { return (CheckConstraint)base.OriginalValue; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CheckConstraintComparisonResult(CheckConstraint originalValue, CheckConstraint newValue)
      : base(originalValue, newValue)
    {
    }
  }
}