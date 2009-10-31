// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public enum SqlDateTimePart
  {
    Year,
    Month,
    Day,
    Hour,
    Minute,
    Second,
    Millisecond,
    TimeZoneHour,
    TimeZoneMinute,
    DayOfYear,
    DayOfWeek,
    Nothing,
  }
}
