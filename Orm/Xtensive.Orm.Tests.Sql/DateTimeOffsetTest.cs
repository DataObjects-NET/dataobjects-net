// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.01.16

using System;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class DateTimeOffsetTest : SqlTest
  {
    private static readonly TimeSpan DefaultTimeSpan = new TimeSpan(4, 10, 0);
    private static readonly DateTimeOffset DefaultDateTimeOffset = new DateTimeOffset(2001, 2, 3, 14, 15, 16, DefaultTimeSpan);

    [Test]
    public virtual void ExtractTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Day, DefaultDateTimeOffset), DefaultDateTimeOffset.Day);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfWeek, DefaultDateTimeOffset), (int) DefaultDateTimeOffset.DayOfWeek);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfYear, DefaultDateTimeOffset), DefaultDateTimeOffset.DayOfYear);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Hour, DefaultDateTimeOffset), DefaultDateTimeOffset.Hour);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, DefaultDateTimeOffset), DefaultDateTimeOffset.Millisecond);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Minute, DefaultDateTimeOffset), DefaultDateTimeOffset.Minute);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Month, DefaultDateTimeOffset), DefaultDateTimeOffset.Month);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Nanosecond, DefaultDateTimeOffset), DefaultDateTimeOffset.Millisecond * 1000000);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Second, DefaultDateTimeOffset), DefaultDateTimeOffset.Second);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneHour, DefaultDateTimeOffset), DefaultDateTimeOffset.Offset.Hours);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneMinute, DefaultDateTimeOffset), DefaultDateTimeOffset.Offset.Minutes);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Year, DefaultDateTimeOffset), DefaultDateTimeOffset.Year);

      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Date, DefaultDateTimeOffset), DefaultDateTimeOffset.Date);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, DefaultDateTimeOffset), DefaultDateTimeOffset.DateTime);
//      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.LocalDateTime, DefaultDateTimeOffset), DefaultDateTimeOffset.LocalDateTime);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Offset, DefaultDateTimeOffset), DefaultDateTimeOffset.Offset);
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.UtcDateTime, DefaultDateTimeOffset), DefaultDateTimeOffset.UtcDateTime);
    }

    [Test]
    public virtual void DateTimeOffsetAddMonthsTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetAddMonths(DefaultDateTimeOffset, 15), DefaultDateTimeOffset.AddMonths(15));
    }

    [Test]
    public virtual void DateTimeOffsetAddYearsTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetAddYears(DefaultDateTimeOffset, 5), DefaultDateTimeOffset.AddYears(5));
    }

    [Test]
    public virtual void DateTimeOffsetConstructTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetConstruct(DefaultDateTimeOffset.DateTime, DefaultTimeSpan.TotalMinutes), DefaultDateTimeOffset);
    }

    [Test]
    public virtual void DateTimeOffsetMinusDateTimeOffsetTest()
    {
      var now = DateTimeOffset.Now;
      now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond, now.Offset); // nanosecond error
      CheckEquality(SqlDml.DateTimeOffsetMinusDateTimeOffset(DefaultDateTimeOffset, now), DefaultDateTimeOffset - now);
    }

    [Test]
    public virtual void DateTimeOffsetMinusIntervalTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, new TimeSpan(4, 4, 4, 4, 4)), DefaultDateTimeOffset - new TimeSpan(4, 4, 4, 4, 4));
    }

    [Test]
    public virtual void DateTimeOffsetPlusIntervalTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, new TimeSpan(10, 10, 10, 10, 10)), DefaultDateTimeOffset + new TimeSpan(10, 10, 10, 10, 10));
    }

    [Test]
    public virtual void DateTimeOffsetTimeOfDayTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetTimeOfDay(DefaultDateTimeOffset), DefaultDateTimeOffset.TimeOfDay);
    }

    [Test]
    public virtual void DateTimeOffsetTruncateTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Date, DefaultDateTimeOffset), DefaultDateTimeOffset.Date);
    }

    [Test]
    public virtual void DateTimeOffsetToDateTimeTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, DefaultDateTimeOffset), DefaultDateTimeOffset.DateTime);
    }

    [Test]
    public virtual void DateTimeOffsetPartOffsetTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Offset, DefaultDateTimeOffset), DefaultDateTimeOffset.Offset);
    }

    [Test]
    public virtual void DateTimeOffsetToUtcDateTimeTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.UtcDateTime, DefaultDateTimeOffset), DefaultDateTimeOffset.UtcDateTime);
    }

    [Test]
    public virtual void DateTimeOffsetToUtcTimeTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetToUtcTime(DefaultDateTimeOffset), DefaultDateTimeOffset.ToUniversalTime());
    }

    private void CheckEquality(SqlExpression left, SqlExpression right)
    {
      var select = SqlDml.Select("ok");
      select.Where = left==right;
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = command.ExecuteReader()) {
          Assert.IsTrue(reader.Read());
        }
      }
    }
  }
}