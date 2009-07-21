// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.27

using System;
using System.Data;
using NUnit.Framework;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Tests
{
  [TestFixture]
  public abstract class DateTimeIntervalTest : SqlTest
  {
    [Test]
    public virtual void ExtractDayOfWeekTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfWeek, new DateTime(2009, 3, 2)),
        (int) DayOfWeek.Monday);
    }

    [Test]
    public virtual void ExtractDayOfYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfYear, new DateTime(2005, 2, 2)),
        33);
    }
  
    [Test]
    public virtual void DateTimeAddIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddInterval(new DateTime(2001, 1, 1, 1, 1, 1, 1), new TimeSpan(10, 10, 10, 10, 10)),
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
        SqlDml.DateTimeSubtractDateTime(new DateTime(2005, 5, 5, 5, 5, 5), new DateTime(2005, 5, 6, 6, 6, 6)),
        new TimeSpan(1, 1, 1, 1).Negate());
    }

    [Test]
    public virtual void DateTimeSubtractIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeSubtractInterval(new DateTime(2005, 5, 5, 5, 5, 5, 5), new TimeSpan(4, 4, 4, 4, 4)),
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
    public virtual void IntervalConstructTest()
    {
      CheckEquality(
        SqlDml.IntervalConstruct(500),
        new TimeSpan(0, 0, 0, 0, 500));
    }

    [Test]
    public virtual void IntervalExtractDayTest()
    {
      CheckEquality(
        SqlDml.IntervalExtract(SqlIntervalPart.Day, new TimeSpan(6, 5, 4, 3, 2)),
        6);
    }

    [Test]
    public virtual void IntervalExtractHourTest()
    {
      CheckEquality(
        SqlDml.IntervalExtract(SqlIntervalPart.Hour, new TimeSpan(6, 5, 4, 3, 2)),
        5);
    }

    [Test]
    public virtual void IntervalExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.IntervalExtract(SqlIntervalPart.Minute, new TimeSpan(6, 5, 4, 3, 2)),
        4);
    }

    [Test]
    public virtual void IntervalExtractSecondTest()
    {
      CheckEquality(
        SqlDml.IntervalExtract(SqlIntervalPart.Second, new TimeSpan(6, 5, 4, 3, 2)),
        3);
    }

    [Test]
    public virtual void IntervalExtractMillisecondTest()
    {
      CheckEquality(
        SqlDml.IntervalExtract(SqlIntervalPart.Millisecond, new TimeSpan(6, 5, 4, 3, 2)),
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
    public virtual void IntervalDurationTest()
    {
      CheckEquality(
        SqlDml.IntervalDuration(new TimeSpan(10, 0, 0, 0).Negate()),
        new TimeSpan(10, 0, 0, 0));
    }

    private void CheckEquality(SqlExpression left, SqlExpression right)
    {
      var select = SqlDml.Select();
      select.Columns.Add("ok");
      select.Where = left == right;

      using (var command = Connection.CreateCommand(select))
        using (var reader = command.ExecuteReader()) {
          if (!reader.Read())
            Assert.Fail(string.Format("expression \"{0}\" evaluated to false", command.CommandText));
      }
    }
  }
}
