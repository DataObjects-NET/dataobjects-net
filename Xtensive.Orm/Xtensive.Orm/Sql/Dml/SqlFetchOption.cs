// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public enum SqlFetchOption
  {
    Next = 0,
    Prior = 1,
    First = 2,
    Last = 3,
    Absolute = 4,
    Relative = 5
  }
}
