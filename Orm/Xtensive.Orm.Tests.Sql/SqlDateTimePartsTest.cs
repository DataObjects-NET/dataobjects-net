// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Sql.Tests
{
  [TestFixture]
  public class SqlDateTimePartsTest
  {
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

    [Test]
    public void MainTest()
    {
      var validNames = new HashSet<string>() {
        "Year", "Month", "Day", "Hour",
        "Minute", "Second", "Millisecond", "Nanosecond",
        "TimeZoneHour", "TimeZoneMinute",
        "DayOfYear", "DayOfWeek",
        "Date", "DateTime",
        "LocalDateTime","UtcDateTime", "Offset",
        "Nothing",
      };

      var enums = typeof(SqlDateTimeOffsetPart).Assembly.GetTypes()
        .Where(t => t.Namespace == "Xtensive.Sql.Dml" && t.IsEnum && (t.Name.StartsWith("Sql") && t.Name.EndsWith("Part")))
        .ToList();
      foreach (var @enum in enums) {
        foreach(var name in Enum.GetNames(@enum)) {
          Assert.That(validNames.Contains(name), $"Does the enum {@enum.Name} have new item?");
        }
      }
    }

    [Test]
    public void DateTimeOffsetPartsValueTest()
    {
      Assert.That((int) SqlDateTimeOffsetPart.Year, Is.EqualTo(0));
      Assert.That((int) SqlDateTimeOffsetPart.Month, Is.EqualTo(1));
      Assert.That((int) SqlDateTimeOffsetPart.Day, Is.EqualTo(2));
      Assert.That((int) SqlDateTimeOffsetPart.Hour, Is.EqualTo(3));
      Assert.That((int) SqlDateTimeOffsetPart.Minute, Is.EqualTo(4));
      Assert.That((int) SqlDateTimeOffsetPart.Second, Is.EqualTo(5));
      Assert.That((int) SqlDateTimeOffsetPart.Millisecond, Is.EqualTo(6));
      Assert.That((int) SqlDateTimeOffsetPart.Nanosecond, Is.EqualTo(7));
      Assert.That((int) SqlDateTimeOffsetPart.TimeZoneHour, Is.EqualTo(8));
      Assert.That((int) SqlDateTimeOffsetPart.TimeZoneMinute, Is.EqualTo(9));
      Assert.That((int) SqlDateTimeOffsetPart.DayOfYear, Is.EqualTo(10));
      Assert.That((int) SqlDateTimeOffsetPart.DayOfWeek, Is.EqualTo(11));
      Assert.That((int) SqlDateTimeOffsetPart.Date, Is.EqualTo(12));
      Assert.That((int) SqlDateTimeOffsetPart.DateTime, Is.EqualTo(13));
      Assert.That((int) SqlDateTimeOffsetPart.LocalDateTime, Is.EqualTo(14));
      Assert.That((int) SqlDateTimeOffsetPart.UtcDateTime, Is.EqualTo(15));
      Assert.That((int) SqlDateTimeOffsetPart.Offset, Is.EqualTo(16));
      Assert.That((int) SqlDateTimeOffsetPart.Nothing, Is.EqualTo(25));
    }

    [Test]
    public void DateTimePartsValueTest()
    {
      Assert.That((int) SqlDateTimePart.Year, Is.EqualTo(0));
      Assert.That((int) SqlDateTimePart.Month, Is.EqualTo(1));
      Assert.That((int) SqlDateTimePart.Day, Is.EqualTo(2));
      Assert.That((int) SqlDateTimePart.Hour, Is.EqualTo(3));
      Assert.That((int) SqlDateTimePart.Minute, Is.EqualTo(4));
      Assert.That((int) SqlDateTimePart.Second, Is.EqualTo(5));
      Assert.That((int) SqlDateTimePart.Millisecond, Is.EqualTo(6));
      Assert.That((int) SqlDateTimePart.Nanosecond, Is.EqualTo(7));
      Assert.That((int) SqlDateTimePart.TimeZoneHour, Is.EqualTo(8));
      Assert.That((int) SqlDateTimePart.TimeZoneMinute, Is.EqualTo(9));
      Assert.That((int) SqlDateTimePart.DayOfYear, Is.EqualTo(10));
      Assert.That((int) SqlDateTimePart.DayOfWeek, Is.EqualTo(11));
      Assert.That((int) SqlDateTimePart.Nothing, Is.EqualTo(25));
    }

    [Test]
    public void DatePartsValueTest()
    {
      Assert.That((int) SqlDatePart.Year, Is.EqualTo(0));
      Assert.That((int) SqlDatePart.Month, Is.EqualTo(1));
      Assert.That((int) SqlDatePart.Day, Is.EqualTo(2));
      Assert.That((int) SqlDatePart.DayOfYear, Is.EqualTo(10));
      Assert.That((int) SqlDatePart.DayOfWeek, Is.EqualTo(11));
      Assert.That((int) SqlDatePart.Nothing, Is.EqualTo(25));
    }

    [Test]
    public void TimePartsValueTest()
    {
      Assert.That((int) SqlTimePart.Hour, Is.EqualTo(3));
      Assert.That((int) SqlTimePart.Minute, Is.EqualTo(4));
      Assert.That((int) SqlTimePart.Second, Is.EqualTo(5));
      Assert.That((int) SqlTimePart.Millisecond, Is.EqualTo(6));
      Assert.That((int) SqlTimePart.Nanosecond, Is.EqualTo(7));
      Assert.That((int) SqlTimePart.Nothing, Is.EqualTo(25));
    }

    [Test]
    public void IntervalPartsValueTest()
    {
      Assert.That((int) SqlIntervalPart.Day, Is.EqualTo(2));
      Assert.That((int) SqlIntervalPart.Hour, Is.EqualTo(3));
      Assert.That((int) SqlIntervalPart.Minute, Is.EqualTo(4));
      Assert.That((int) SqlIntervalPart.Second, Is.EqualTo(5));
      Assert.That((int) SqlIntervalPart.Millisecond, Is.EqualTo(6));
      Assert.That((int) SqlIntervalPart.Nanosecond, Is.EqualTo(7));
      
      Assert.That((int) SqlIntervalPart.Nothing, Is.EqualTo(25));
    }

    [Test]
    public void DateTimePartConversionTest()
    {
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Year, Is.EqualTo(SqlDateTimePart.Year));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Month, Is.EqualTo(SqlDateTimePart.Month));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Day, Is.EqualTo(SqlDateTimePart.Day));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Hour, Is.EqualTo(SqlDateTimePart.Hour));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Minute, Is.EqualTo(SqlDateTimePart.Minute));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Second, Is.EqualTo(SqlDateTimePart.Second));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Millisecond, Is.EqualTo(SqlDateTimePart.Millisecond));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Nanosecond, Is.EqualTo(SqlDateTimePart.Nanosecond));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.TimeZoneHour, Is.EqualTo(SqlDateTimePart.TimeZoneHour));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.TimeZoneMinute, Is.EqualTo(SqlDateTimePart.TimeZoneMinute));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.DayOfYear, Is.EqualTo(SqlDateTimePart.DayOfYear));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.DayOfWeek, Is.EqualTo(SqlDateTimePart.DayOfWeek));
      Assert.That((SqlDateTimePart) (int) SqlDateTimeOffsetPart.Nothing, Is.EqualTo(SqlDateTimePart.Nothing));

      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Year, Is.EqualTo(SqlDateTimeOffsetPart.Year));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Month, Is.EqualTo(SqlDateTimeOffsetPart.Month));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Day, Is.EqualTo(SqlDateTimeOffsetPart.Day));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Hour, Is.EqualTo(SqlDateTimeOffsetPart.Hour));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Minute, Is.EqualTo(SqlDateTimeOffsetPart.Minute));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Second, Is.EqualTo(SqlDateTimeOffsetPart.Second));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Millisecond, Is.EqualTo(SqlDateTimeOffsetPart.Millisecond));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Nanosecond, Is.EqualTo(SqlDateTimeOffsetPart.Nanosecond));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.TimeZoneHour, Is.EqualTo(SqlDateTimeOffsetPart.TimeZoneHour));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.TimeZoneMinute, Is.EqualTo(SqlDateTimeOffsetPart.TimeZoneMinute));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.DayOfYear, Is.EqualTo(SqlDateTimeOffsetPart.DayOfYear));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.DayOfWeek, Is.EqualTo(SqlDateTimeOffsetPart.DayOfWeek));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDateTimePart.Nothing, Is.EqualTo(SqlDateTimeOffsetPart.Nothing));
    }

    [Test]
    public void DatePartConversionTest()
    {
      Assert.That((SqlDatePart) (int) SqlDateTimeOffsetPart.Year, Is.EqualTo(SqlDatePart.Year));
      Assert.That((SqlDatePart) (int) SqlDateTimeOffsetPart.Month, Is.EqualTo(SqlDatePart.Month));
      Assert.That((SqlDatePart) (int) SqlDateTimeOffsetPart.Day, Is.EqualTo(SqlDatePart.Day));
      Assert.That((SqlDatePart) (int) SqlDateTimeOffsetPart.DayOfYear, Is.EqualTo(SqlDatePart.DayOfYear));
      Assert.That((SqlDatePart) (int) SqlDateTimeOffsetPart.DayOfWeek, Is.EqualTo(SqlDatePart.DayOfWeek));
      Assert.That((SqlDatePart) (int) SqlDateTimeOffsetPart.Nothing, Is.EqualTo(SqlDatePart.Nothing));

      Assert.That((SqlDateTimeOffsetPart) (int) SqlDatePart.Year, Is.EqualTo(SqlDateTimeOffsetPart.Year));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDatePart.Month, Is.EqualTo(SqlDateTimeOffsetPart.Month));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDatePart.Day, Is.EqualTo(SqlDateTimeOffsetPart.Day));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDatePart.DayOfYear, Is.EqualTo(SqlDateTimeOffsetPart.DayOfYear));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDatePart.DayOfWeek, Is.EqualTo(SqlDateTimeOffsetPart.DayOfWeek));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlDatePart.Nothing, Is.EqualTo(SqlDateTimeOffsetPart.Nothing));
    }

    [Test]
    public void TimePartConversionTest()
    {
      Assert.That((SqlTimePart) (int) SqlDateTimeOffsetPart.Hour, Is.EqualTo(SqlTimePart.Hour));
      Assert.That((SqlTimePart) (int) SqlDateTimeOffsetPart.Minute, Is.EqualTo(SqlTimePart.Minute));
      Assert.That((SqlTimePart) (int) SqlDateTimeOffsetPart.Second, Is.EqualTo(SqlTimePart.Second));
      Assert.That((SqlTimePart) (int) SqlDateTimeOffsetPart.Millisecond, Is.EqualTo(SqlTimePart.Millisecond));
      Assert.That((SqlTimePart) (int) SqlDateTimeOffsetPart.Nanosecond, Is.EqualTo(SqlTimePart.Nanosecond));
      Assert.That((SqlTimePart) (int) SqlDateTimeOffsetPart.Nothing, Is.EqualTo(SqlTimePart.Nothing));

      Assert.That((SqlDateTimeOffsetPart) (int) SqlTimePart.Hour, Is.EqualTo(SqlDateTimeOffsetPart.Hour));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlTimePart.Minute, Is.EqualTo(SqlDateTimeOffsetPart.Minute));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlTimePart.Second, Is.EqualTo(SqlDateTimeOffsetPart.Second));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlTimePart.Millisecond, Is.EqualTo(SqlDateTimeOffsetPart.Millisecond));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlTimePart.Nanosecond, Is.EqualTo(SqlDateTimeOffsetPart.Nanosecond));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlTimePart.Nothing, Is.EqualTo(SqlDateTimeOffsetPart.Nothing));
    }

    [Test]
    public void IntervalPartConversionTest()
    {
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Day, Is.EqualTo(SqlIntervalPart.Day));
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Hour, Is.EqualTo(SqlIntervalPart.Hour));
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Minute, Is.EqualTo(SqlIntervalPart.Minute));
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Second, Is.EqualTo(SqlIntervalPart.Second));
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Millisecond, Is.EqualTo(SqlIntervalPart.Millisecond));
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Nanosecond, Is.EqualTo(SqlIntervalPart.Nanosecond));
      Assert.That((SqlIntervalPart) (int) SqlDateTimeOffsetPart.Nothing, Is.EqualTo(SqlIntervalPart.Nothing));

      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Day, Is.EqualTo(SqlDateTimeOffsetPart.Day));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Hour, Is.EqualTo(SqlDateTimeOffsetPart.Hour));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Minute, Is.EqualTo(SqlDateTimeOffsetPart.Minute));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Second, Is.EqualTo(SqlDateTimeOffsetPart.Second));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Millisecond, Is.EqualTo(SqlDateTimeOffsetPart.Millisecond));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Nanosecond, Is.EqualTo(SqlDateTimeOffsetPart.Nanosecond));
      Assert.That((SqlDateTimeOffsetPart) (int) SqlIntervalPart.Nothing, Is.EqualTo(SqlDateTimeOffsetPart.Nothing));
    }
  }
}
