// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class TranslationSqlComparer : SqlComparerBase<Translation>
  {
    public override IComparisonResult<Translation> Compare(Translation originalNode, Translation newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public TranslationSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}