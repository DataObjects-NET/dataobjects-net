// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom
{
  [Flags]
  [Serializable]
  public enum Action
  {
    All = 0x01,
    Delete = 0x02,
    Insert = 0x04,
    Update = 0x08,
    Refrences = 0x10,
    Usage = 0x20,
  }
}
