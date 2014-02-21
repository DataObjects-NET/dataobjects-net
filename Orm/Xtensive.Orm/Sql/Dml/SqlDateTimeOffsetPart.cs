// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public enum SqlDateTimeOffsetPart
  {
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second,
    Millisecond,
    Nanosecond,
    TimeZoneHour,
    TimeZoneMinute,
    DayOfYear,
    DayOfWeek,
    Nothing,
  }
}