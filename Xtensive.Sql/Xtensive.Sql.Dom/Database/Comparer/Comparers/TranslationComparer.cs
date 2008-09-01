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
  internal class TranslationComparer : NodeComparerBase<Translation>
  {
    public override IComparisonResult<Translation> Compare(Translation originalNode, Translation newNode)
    {
      return new TranslationComparisonResult(originalNode, newNode);
    }

    public TranslationComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}