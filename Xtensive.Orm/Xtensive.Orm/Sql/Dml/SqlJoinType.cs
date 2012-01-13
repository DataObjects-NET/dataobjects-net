// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
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

    CrossApply,

    LeftOuterApply
  }
}
