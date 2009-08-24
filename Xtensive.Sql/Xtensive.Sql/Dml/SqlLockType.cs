// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.24

using System;
using System.Diagnostics;

namespace Xtensive.Sql.Dml
{
  [Flags]
  public enum SqlLockType
  {
    Empty = 0,
    Shared = 1,
    Update = 2,
    Exclusive = 3,
    ThrowIfLocked = 4,
    SkipLocked = 8,
  }
}