// Copyright (C) 2014-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Sql.Dml
{
  // IMPORTANT DateTime related enums are similar to each other to a certain degree
  // and contain some subset of parts listed below.
  // To be able to fast-convert between enums
  // the same parts of enums have the same assigned value keep this pattern.
  // Newer parts, if any should be added to this comment and assighed accordingly 
  // Year           = 0,
  // Month          = 1,
  // Day            = 2,
  // Hour           = 3,
  // Minute         = 4,
  // Second         = 5,
  // Millisecond    = 6,
  // Nanosecond     = 7,
  // TimeZoneHour   = 8,
  // TimeZoneMinute = 9,
  // DayOfYear      = 10,
  // DayOfWeek      = 11,
  // Date           = 12,
  // DateTime       = 13,
  // LocalDateTime  = 14,
  // UtcDateTime    = 15,
  // Offset         = 16,
  // Nothing        = 25,

  [Serializable]
  public enum SqlDateTimeOffsetPart
  {
    Year = 0,
    Month = 1,
    Day = 2,
    Hour = 3,
    Minute = 4,
    Second = 5,
    Millisecond = 6,
    Nanosecond = 7,
    TimeZoneHour = 8,
    TimeZoneMinute = 9,
    DayOfYear = 10,
    DayOfWeek = 11,
    Date = 12,
    DateTime = 13,
    LocalDateTime = 14,
    UtcDateTime = 15,
    Offset = 16,
    Nothing = 25,
  }
}