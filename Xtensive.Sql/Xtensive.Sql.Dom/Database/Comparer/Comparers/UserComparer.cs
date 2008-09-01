// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.25

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class UserComparer : NodeComparerBase<User>
  {
    public override IComparisonResult<User> Compare(User originalNode, User newNode, IEnumerable<ComparisonHintBase> hints)
    {
      return new UserComparisonResult(originalNode, newNode);
    }

    public UserComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}