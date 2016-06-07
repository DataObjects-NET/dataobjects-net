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
    protected static readonly DateTime DefaultDateTime = new DateTime(2001, 2, 3, 4, 5, 6, 334);
    protected static readonly DateTime SecondDateTime = new DateTime(2000, 12, 11, 10, 9, 8, 765);
    protected static readonly TimeSpan DefaultTimeSpan = new TimeSpan(10, 9, 8, 7, 652);
    protected static readonly int AddYearsConst = 5;
    protected static readonly int AddMonthsConst = 15;

    [Test]
    public virtual void DateTimeAddIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimePlusInterval(DefaultDateTime, DefaultTimeSpan),
        DefaultDateTime.Add(DefaultTimeSpan));
    }

    [Test]
    public virtual void DateTimeAddMonthsTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddMonths(DefaultDateTime, AddMonthsConst),
        DefaultDateTime.AddMonths(AddMonthsConst));
    }

    [Test]
    public virtual void DateTimeAddYearsTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddYears(DefaultDateTime, AddYearsConst),
        DefaultDateTime.AddYears(AddYearsConst));
    }

    [Test]
    public virtual void DateTimeConstructTest()
    {
      CheckEquality(
        SqlDml.DateTimeConstruct(DefaultDateTime.Year, DefaultDateTime.Month, DefaultDateTime.Day),
        DefaultDateTime.Date);
    }

    [Test]
    public virtual void DateTimeSubtractDateTimeTest()
    {
      CheckEquality(
        SqlDml.DateTimeMinusDateTime(DefaultDateTime, SecondDateTime),
        DefaultDateTime.Subtract(SecondDateTime));
    }

    [Test]
    public virtual void DateTimeSubtractIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeMinusInterval(DefaultDateTime, DefaultTimeSpan),
        DefaultDateTime.Subtract(DefaultTimeSpan));
    }

    [Test]
    public virtual void DateTimeTruncateTest()
    {
      CheckEquality(
        SqlDml.DateTimeTruncate(DefaultDateTime),
        DefaultDateTime.Date);
    }

    [Test]
    public virtual void DateTimeExtractYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Year, DefaultDateTime),
        DefaultDateTime.Year);
    }

    [Test]
    public virtual void DateTimeExtractMonthTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Month, DefaultDateTime),
        DefaultDateTime.Month);
    }

    [Test]
    public virtual void DateTimeExtractDayTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Day, DefaultDateTime),
        DefaultDateTime.Day);
    }

    [Test]
    public virtual void DateTimeExtractHourTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Hour, DefaultDateTime),
        DefaultDateTime.Hour);
    }

    [Test]
    public virtual void DateTimeExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Minute, DefaultDateTime),
        DefaultDateTime.Minute);
    }

    [Test]
    public virtual void DateTimeExtractSecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Second, DefaultDateTime),
        DefaultDateTime.Second);
    }

    [Test]
    public virtual void DateTimeExtractMillisecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Millisecond, DefaultDateTime),
        DefaultDateTime.Millisecond);
    }

    [Test]
    public virtual void DateTimeExtractDayOfWeekTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfWeek, DefaultDateTime),
        (int) DefaultDateTime.DayOfWeek);
    }

    [Test]
    public virtual void DateTimeExtractDayOfYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfYear, DefaultDateTime),
        DefaultDateTime.DayOfYear);
    }

    [Test]
    public virtual void IntervalConstructTest()
    {
      CheckEquality(
        SqlDml.IntervalConstruct((long) DefaultTimeSpan.TotalMilliseconds * 1000000),
        DefaultTimeSpan);
    }

    [Test]
    public virtual void IntervalExtractDayTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Day, DefaultTimeSpan),
        DefaultTimeSpan.Days);
    }

    [Test]
    public virtual void IntervalExtractHourTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Hour, DefaultTimeSpan),
        DefaultTimeSpan.Hours);
    }

    [Test]
    public virtual void IntervalExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Minute, DefaultTimeSpan),
        DefaultTimeSpan.Minutes);
    }

    [Test]
    public virtual void IntervalExtractSecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Second, DefaultTimeSpan),
        DefaultTimeSpan.Seconds);
    }

    [Test]
    public virtual void IntervalExtractMillisecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlIntervalPart.Millisecond, DefaultTimeSpan),
        DefaultTimeSpan.Milliseconds);
    }

    [Test]
    public virtual void IntervalToMillisecondsTest()
    {
      CheckEquality(
        SqlDml.IntervalToMilliseconds(DefaultTimeSpan),
        (int) DefaultTimeSpan.TotalMilliseconds);
    }

    [Test]
    public virtual void IntervalAbsTest()
    {
      CheckEquality(
        SqlDml.IntervalAbs(DefaultTimeSpan.Negate()),
        DefaultTimeSpan);
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
