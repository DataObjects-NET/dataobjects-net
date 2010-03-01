// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.24

using System;

namespace Xtensive.Sql.Dml
{
  [Flags]
  public enum SqlLockType
  {
    Empty = 0,
    Shared = 1,
    Update = 2,
    Exclusive = 4,
    ThrowIfLocked = 8,
    SkipLocked = 16,
  }
}