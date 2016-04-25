// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.27

using NUnit.Framework;
using System;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql
{
  public abstract class DateTimeIntervalTest : SqlTest
  {
    [Test]
    public virtual void DateTimeAddIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimePlusInterval(new DateTime(2001, 1, 1, 1, 1, 1, 1), new TimeSpan(10, 10, 10, 10, 10)),
        new DateTime(2001, 1, 11, 11, 11, 11, 11));
    }

    [Test]
    public virtual void DateTimeAddMonthsTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddMonths(new DateTime(2001, 1, 1), 15) ,
        new DateTime(2002, 4, 1));
    }

    [Test]
    public virtual void DateTimeAddYearsTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddYears(new DateTime(2001, 1, 1), 5) ,
        new DateTime(2006, 1, 1));
    }

    [Test]
    public virtual void DateTimeConstructTest()
    {
      CheckEquality(
        SqlDml.DateTimeConstruct(2005, 5, 5),
        new DateTime(2005, 5, 5));
    }

    [Test]
    public virtual void DateTimeSubtractDateTimeTest()
    {
      CheckEquality(
        SqlDml.DateTimeMinusDateTime(new DateTime(2005, 5, 5, 5, 5, 5, 5), new DateTime(2005, 5, 6, 6, 6, 6, 6)),
        new TimeSpan(1, 1, 1, 1, 1).Negate());
    }

    [Test]
    public virtual void DateTimeSubtractIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeMinusInterval(new DateTime(2005, 5, 5, 5, 5, 5, 5), new TimeSpan(4, 4, 4, 4, 4)),
        new DateTime(2005, 5, 1, 1, 1, 1, 1));
    }

    [Test]
    public virtual void DateTimeTruncateTest()
    {
      CheckEquality(
        SqlDml.DateTimeTruncate(new DateTime(2005, 1, 1, 1, 1, 1, 1)),
        new DateTime(2005, 1, 1));
    }

    [Test]
    public virtual void DateTimeExtractYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Year, new DateTime(2006, 5, 4)),
        2006);
    }

    [Test]
    public virtual void DateTimeExtractMonthTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Month, new DateTime(2006, 5, 4)),
        5);
    }

    [Test]
    public virtual void DateTimeExtractDayTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Day, new DateTime(2006, 5, 4)),
        4);
    }

    [Test]
    public virtual void DateTimeExtractHourTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Hour, new DateTime(2006, 5, 4, 3, 2, 1, 333)),
        3);
    }

    [Test]
    public virtual void DateTimeExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Minute, new DateTime(2006, 5, 4, 3, 2, 1, 333)),
        2);
    }

    [Test]
    public virtual void DateTimeExtractSecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Second, new DateTime(2006, 5, 4, 3, 2, 1, 333)),
        1);
    }

    [Test]
    public virtual void DateTimeExtractMillisecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Millisecond, new DateTime(2006, 5, 4, 3, 2, 1, 333)),
        333);
    }
    
    [Test]
    public virtual void DateTimeExtractDayOfWeekTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfWeek, new DateTime(2009, 3, 2)),
        (int) DayOfWeek.Monday);
    }

    [Test]
    public virtual void DateTimeExtractDayOfYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfYear, new DateTime(2005, 2, 2)),
        33);
    }

    [Test]
    public virtual void IntervalConstructTest()
    {
      CheckEquality(
        SqlDml.IntervalConstruct(500000000),
        new TimeSpan(0, 0, 0, 0, 500));
    }

    [Test]
    public virtual void IntervalExtractDayTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Day, new TimeSpan(6, 5, 4, 3, 2)),
        6);
    }

    [Test]
    public virtual void IntervalExtractHourTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Hour, new TimeSpan(6, 5, 4, 3, 2)),
        5);
    }

    [Test]
    public virtual void IntervalExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Minute, new TimeSpan(6, 5, 4, 3, 2)),
        4);
    }

    [Test]
    public virtual void IntervalExtractSecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Second, new TimeSpan(6, 5, 4, 3, 2)),
        3);
    }

    [Test]
    public virtual void IntervalExtractMillisecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Millisecond, new TimeSpan(6, 5, 4, 3, 2)),
        2);
    }

    [Test]
    public virtual void IntervalToMillisecondsTest()
    {
      CheckEquality(
        SqlDml.IntervalToMilliseconds(new TimeSpan(0, 0, 8, 5, 5)),
        (int)new TimeSpan(0, 0, 8, 5, 5).TotalMilliseconds);
    }

    [Test]
    public virtual void IntervalAbsTest()
    {
      CheckEquality(
        SqlDml.IntervalAbs(new TimeSpan(10, 0, 0, 0).Negate()),
        new TimeSpan(10, 0, 0, 0));
    }

    protected void CheckEquality(SqlExpression left, SqlExpression right)
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
