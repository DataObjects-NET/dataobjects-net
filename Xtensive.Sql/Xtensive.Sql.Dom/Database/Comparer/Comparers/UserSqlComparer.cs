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
  internal class UserSqlComparer : SqlComparerBase<User>
  {
    /// <inheritdoc/>
    public override ComparisonResult<User> Compare(User originalNode, User newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = new NodeComparisonResult<User>();
      ValidateArguments(originalNode, newNode);
      ProcessDbName(originalNode, newNode, result);
      return result;
    }

    public UserSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}