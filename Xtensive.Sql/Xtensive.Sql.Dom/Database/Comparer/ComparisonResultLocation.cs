// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Flags]
  public enum ComparisonResultLocation
  {
    None = 0x00,
    Nested = 0x01,
    Property = 0x02,
    All = Property + Nested
  }
}