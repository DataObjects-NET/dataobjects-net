// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class AssertionSqlComparer : WrappingSqlComparer<Assertion, Constraint>
  {
    public override ComparisonResult<Assertion> Compare(Assertion originalNode, Assertion newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public AssertionSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}