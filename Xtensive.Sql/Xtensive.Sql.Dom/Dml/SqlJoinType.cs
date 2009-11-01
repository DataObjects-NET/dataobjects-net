// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public enum SqlJoinType
  {
    CrossJoin,

    InnerJoin,

    RightOuterJoin,

    LeftOuterJoin,

    FullOuterJoin,

    UnionJoin,

    UsingJoin,
  }
}
