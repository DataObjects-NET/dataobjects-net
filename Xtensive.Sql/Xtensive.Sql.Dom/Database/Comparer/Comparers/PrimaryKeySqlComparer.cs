// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class PrimaryKeySqlComparer : WrappingSqlComparer<PrimaryKey, UniqueConstraint>
  {
    public override IComparisonResult<PrimaryKey> Compare(PrimaryKey originalNode, PrimaryKey newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public PrimaryKeySqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}