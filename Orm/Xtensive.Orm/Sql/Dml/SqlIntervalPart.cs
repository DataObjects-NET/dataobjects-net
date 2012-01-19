// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.25

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public enum SqlIntervalPart
  {
    Day = 0,
    Hour = 1,
    Minute = 2,
    Second = 3,
    Millisecond = 4,
    Nanosecond = 5,
    Nothing = 10,
  }
}