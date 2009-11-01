// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.08.17

using System;

namespace Xtensive.Core
{
  public enum LockType
  {
    None = 0,
    Read = 1,
    Shared = 1,
    Write = 2,
    Exclusive = 2,
    Suspend = 4,
  }
}