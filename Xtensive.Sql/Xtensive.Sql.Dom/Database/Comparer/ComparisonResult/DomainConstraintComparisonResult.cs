// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class DomainConstraintComparisonResult : ConstraintComparisonResult,
    IComparisonResult<DomainConstraint>
  {
    /// <inheritdoc/>
    public new DomainConstraint NewValue
    {
      get { return (DomainConstraint) base.NewValue; }
    }

    /// <inheritdoc/>
    public new DomainConstraint OriginalValue
    {
      get { return (DomainConstraint) base.OriginalValue; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DomainConstraintComparisonResult(DomainConstraint originalValue, DomainConstraint newValue)
      : base(originalValue, newValue)
    {
    }
  }
}