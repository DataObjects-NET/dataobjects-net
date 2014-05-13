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
    [Test]
    public virtual void ExtractTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Day, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.DayOfWeek, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.DayOfYear, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Hour, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Minute, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Month, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Nanosecond, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1000000);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Second, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        1);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneHour, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        4);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneMinute, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        10);
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Year, new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        2001);
    }

    [Test]
    public virtual void DateTimeOffsetAddMonthsTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetAddMonths(new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)), 15),
        new DateTimeOffset(2002, 4, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)));
    }

    [Test]
    public virtual void DateTimeOffsetAddYearsTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetAddYears(new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)), 5),
        new DateTimeOffset(2006, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)));
    }

    [Test]
    public virtual void DateTimeOffsetConstructTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetConstruct(new DateTime(2001, 1, 1, 1,1, 1,  1), 250),
        new DateTimeOffset(2001, 1, 1, 0, 0, 0, 0, new TimeSpan(4, 10, 0)));
    }

    [Test]
    public virtual void DateTimeOffsetMinusDateTimeOffsetTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetMinusDateTimeOffset(new DateTimeOffset(2005, 5, 5, 5, 5, 5, 5, new TimeSpan(4, 10, 0)),
          new DateTimeOffset(2005, 5, 6, 6, 6, 6, 6, new TimeSpan(4, 10, 0))),
        new TimeSpan(1, 1, 1, 1, 1).Negate());
    }

    [Test]
    public virtual void DateTimeOffsetMinusIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetMinusInterval(new DateTimeOffset(2005, 5, 5, 5, 5, 5, 5, new TimeSpan(4, 10, 0)), new TimeSpan(4, 4, 4, 4, 4)),
        new DateTimeOffset(2005, 5, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)));
    }

    [Test]
    public virtual void DateTimeOffsetPlusIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetPlusInterval(new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0)), new TimeSpan(10, 10, 10, 10, 10)),
        new DateTimeOffset(2001, 1, 11, 11, 11, 11, 11, new TimeSpan(4, 10, 0)));
    }

    [Test]
    public virtual void DateTimeOffsetTimeOfDayTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetTimeOfDay(new DateTimeOffset(2001, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        new TimeSpan(0, 1, 1, 1, 1));
    }

    [Test]
    public virtual void DateTimeOffsetTruncateTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Date, new DateTimeOffset(2005, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        new DateTime(2005, 1, 1, 0, 0, 0, 0));
    }

    [Test]
    public virtual void DateTimeOffsetToDateTimeTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, new DateTimeOffset(2005, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        new DateTime(2005, 1, 1, 1, 1, 1, 1));
    }

    [Test]
    public virtual void DateTimeOffsetPartOffsetTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.Offset, new DateTimeOffset(2005, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        new TimeSpan(4, 10, 0));
    }

    [Test]
    public virtual void DateTimeOffsetToUtcDateTimeTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimeOffsetPart.UtcDateTime, new DateTimeOffset(2005, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        new DateTime(2004, 12, 31, 20, 51, 1, 1));
    }

    [Test]
    public virtual void DateTimeOffsetToUtcTimeTest()
    {
      CheckEquality(
        SqlDml.DateTimeOffsetToUtcTime(new DateTimeOffset(2005, 1, 1, 1, 1, 1, 1, new TimeSpan(4, 10, 0))),
        new DateTimeOffset(2004, 12, 31, 20, 51, 1, 1, new TimeSpan(0, 0, 0)));
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
