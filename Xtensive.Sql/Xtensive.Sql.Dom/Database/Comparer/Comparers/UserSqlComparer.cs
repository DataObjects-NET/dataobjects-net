// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.25

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class UserSqlComparer : NodeSqlComparer<User, NodeComparisonResult<User>>
  {
    public UserSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}