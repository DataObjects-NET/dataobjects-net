// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class ConstraintComparisonResult : NodeComparisonResult, 
    IComparisonResult<Constraint>
  {
    public new Constraint NewValue
    {
      get { return (Constraint) base.NewValue; }
    }

    public new Constraint OriginalValue
    {
      get { return (Constraint)base.OriginalValue; }
    }
  }
}