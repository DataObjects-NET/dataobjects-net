// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Ddl
{
  [Flags]
  [Serializable]
  public enum SqlAlterIdentityInfoOptions
  {
    None = 0x0,
    RestartWithOption = 0x1,
    IncrementByOption = 0x2,
    MaxValueOption = 0x4,
    MinValueOption = 0x8,
    CycleOption = 0x10,
    All = 0x1F
  }
}
