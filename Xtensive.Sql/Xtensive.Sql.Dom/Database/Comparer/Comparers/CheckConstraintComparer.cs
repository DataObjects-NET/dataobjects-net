// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class CheckConstraintComparer : NodeComparerBase<CheckConstraint>
  {
    public override IComparisonResult<CheckConstraint> Compare(CheckConstraint originalNode, CheckConstraint newNode)
    {
      return new CheckConstraintComparisonResult(originalNode, newNode);

    }

    public CheckConstraintComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}