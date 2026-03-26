// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    protected static readonly DateTime DefaultDateTime =
      StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)
        ? new DateTime(2001, 2, 3, 4, 5, 6)
        : new DateTime(2001, 2, 3, 4, 5, 6, 334);
    protected static readonly DateTime SecondDateTime =
      StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)
        ? new DateTime(2000, 12, 11, 10, 9, 8)
        : new DateTime(2000, 12, 11, 10, 9, 8, 765);
#if NET6_0_OR_GREATER

    protected static readonly DateOnly DefaultDateOnly = new DateOnly(2001, 2, 3);
    protected static readonly DateOnly SecondDateOnly = new DateOnly(2000, 12, 11);

    protected static readonly TimeOnly DefaultTimeOnly = new TimeOnly(4, 5, 6, 334);
    protected static readonly TimeOnly SecondTimeOnly = new TimeOnly(10, 9, 8, 765);
#endif

    protected static readonly TimeSpan DefaultTimeSpan = new TimeSpan(10, 9, 8, 7, 652);
    protected static readonly int AddYearsConst = 5;
    protected static readonly int AddMonthsConst = 15;

    [Test]
    public virtual void DateTimeAddIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimePlusInterval(PrepareDateTimeLiteral(DefaultDateTime), DefaultTimeSpan),
        DefaultDateTime.Add(DefaultTimeSpan));
    }

    [Test]
    public virtual void DateTimeAddMonthsTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddMonths(PrepareDateTimeLiteral(DefaultDateTime), AddMonthsConst),
        PrepareDateTimeLiteral(DefaultDateTime.AddMonths(AddMonthsConst)));
    }

    [Test]
    public virtual void DateTimeAddYearsTest()
    {
      CheckEquality(
        SqlDml.DateTimeAddYears(PrepareDateTimeLiteral(DefaultDateTime), AddYearsConst),
        PrepareDateTimeLiteral(DefaultDateTime.AddYears(AddYearsConst)));
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
        SqlDml.DateTimeMinusDateTime(PrepareDateTimeLiteral(DefaultDateTime), PrepareDateTimeLiteral(SecondDateTime)),
        DefaultDateTime.Subtract(SecondDateTime));
    }

    [Test]
    public virtual void DateTimeSubtractIntervalTest()
    {
      CheckEquality(
        SqlDml.DateTimeMinusInterval(PrepareDateTimeLiteral(DefaultDateTime), DefaultTimeSpan),
        DefaultDateTime.Subtract(DefaultTimeSpan));
    }

    [Test]
    public virtual void DateTimeTruncateTest()
    {
      CheckEquality(
        SqlDml.DateTimeTruncate(PrepareDateTimeLiteral(DefaultDateTime)),
        PrepareDateTimeLiteral(DefaultDateTime.Date));
    }

    [Test]
    public virtual void DateTimeExtractYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Year, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.Year);
    }

    [Test]
    public virtual void DateTimeExtractMonthTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Month, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.Month);
    }

    [Test]
    public virtual void DateTimeExtractDayTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Day, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.Day);
    }

    [Test]
    public virtual void DateTimeExtractHourTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Hour, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.Hour);
    }

    [Test]
    public virtual void DateTimeExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Minute, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.Minute);
    }

    [Test]
    public virtual void DateTimeExtractSecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Second, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.Second);
    }

    [Test]
    public virtual void DateTimeExtractMillisecondTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.Millisecond, DefaultDateTime),
        DefaultDateTime.Millisecond);
    }

    [Test]
    public virtual void DateTimeExtractDayOfWeekTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfWeek, PrepareDateTimeLiteral(DefaultDateTime)),
        (int) DefaultDateTime.DayOfWeek);
    }

    [Test]
    public virtual void DateTimeExtractDayOfYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDateTimePart.DayOfYear, PrepareDateTimeLiteral(DefaultDateTime)),
        DefaultDateTime.DayOfYear);
    }
#if NET6_0_OR_GREATER

    [Test]
    public virtual void DateOnlyAddYearsTest()
    {
      CheckEquality(
        SqlDml.DateAddYears(PrepareDateLiteral(DefaultDateOnly), AddYearsConst),
        PrepareDateLiteral(DefaultDateOnly.AddYears(AddYearsConst)));
    }

    [Test]
    public virtual void DateOnlyAddMonthsTest()
    {
      CheckEquality(
        SqlDml.DateAddMonths(PrepareDateLiteral(DefaultDateOnly), AddMonthsConst),
        PrepareDateLiteral(DefaultDateOnly.AddMonths(AddMonthsConst)));
    }

    [Test]
    public virtual void DateOnlyConstructTest()
    {
      CheckEquality(
        SqlDml.DateConstruct(DefaultDateOnly.Year, DefaultDateOnly.Month, DefaultDateOnly.Day),
        DefaultDateOnly);
    }

    [Test]
    public virtual void DateOnlyExtractYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDatePart.Year, PrepareDateLiteral(DefaultDateOnly)),
        DefaultDateOnly.Year);
    }

    [Test]
    public virtual void DateOnlyExtractMonthTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDatePart.Month, PrepareDateLiteral(DefaultDateOnly)),
        DefaultDateOnly.Month);
    }

    [Test]
    public virtual void DateOnlyExtractDayTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDatePart.Day, PrepareDateLiteral(DefaultDateOnly)),
        DefaultDateOnly.Day);
    }

    [Test]
    public virtual void DateOnlyExtractDayOfWeekTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDatePart.DayOfWeek, PrepareDateLiteral(DefaultDateOnly)),
        (int) DefaultDateOnly.DayOfWeek);
    }

    [Test]
    public virtual void DateOnlyExtractDayOfYearTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlDatePart.DayOfYear, PrepareDateLiteral(DefaultDateOnly)),
        DefaultDateOnly.DayOfYear);
    }

    [Test]
    public virtual void TimeOnlyExtractHourTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlTimePart.Hour, PrepareTimeLiteral(DefaultTimeOnly)),
        DefaultTimeOnly.Hour);
    }

    [Test]
    public virtual void TimeOnlyExtractMinuteTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlTimePart.Minute, PrepareTimeLiteral(DefaultTimeOnly)),
        DefaultTimeOnly.Minute);
    }

    [Test]
    public virtual void TimeOnlyExtractSecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlTimePart.Second, PrepareTimeLiteral(DefaultTimeOnly)),
        DefaultTimeOnly.Second);
    }

    [Test]
    public virtual void TimeOnlyExtractMillisecondTest()
    {
      CheckEquality(
        SqlDml.Extract(SqlTimePart.Millisecond, PrepareTimeLiteral(DefaultTimeOnly)),
        DefaultTimeOnly.Millisecond);
    }

    [Test]
    public virtual void TimeOnlyConstructTest1()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite | StorageProvider.MySql);

      CheckEquality(
        SqlDml.TimeConstruct(DefaultTimeOnly.Hour, DefaultTimeOnly.Minute, DefaultTimeOnly.Second, DefaultTimeOnly.Millisecond),
        DefaultTimeOnly);
    }

    [Test]
    public virtual void TimeOnlyConstructTest2()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite | StorageProvider.MySql);

      var ticksPerHour = new TimeOnly(1, 0).Ticks;
      var ticksPerMinute = new TimeOnly(0, 1).Ticks;
      var ticksPerSecond = new TimeOnly(0, 0, 1).Ticks;
      var ticksPerMillisecond = new TimeOnly(0, 0, 0, 1).Ticks;
      var testTicks = ticksPerHour * DefaultTimeOnly.Hour +
        ticksPerMinute * DefaultTimeOnly.Minute +
        ticksPerSecond * DefaultTimeOnly.Second +
        ticksPerMillisecond * DefaultTimeOnly.Millisecond;

      CheckEquality(
        SqlDml.TimeConstruct(testTicks),
        DefaultTimeOnly);
    }

    [Test]
    public virtual void TimeOnlySubtractTimeOnlyTest()
    {
      CheckEquality(
        SqlDml.TimeMinusTime(PrepareTimeLiteral(DefaultTimeOnly), PrepareTimeLiteral(SecondTimeOnly)),
        DefaultTimeOnly - SecondTimeOnly);
    }
#endif

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

    private SqlExpression PrepareDateTimeLiteral(DateTime value)
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)) {
        return SqlDml.Cast(SqlDml.Literal(value), SqlType.DateTime);
      }
      return SqlDml.Literal(value);
    }
#if NET6_0_OR_GREATER

    private SqlExpression PrepareDateLiteral(DateOnly value)
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)) {
        return SqlDml.Cast(SqlDml.Literal(value), SqlType.Date);
      }
      return SqlDml.Literal(value);
    }

    private SqlExpression PrepareTimeLiteral(TimeOnly value)
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)) {
        return SqlDml.Cast(SqlDml.Literal(value), SqlType.Time);
      }
      return SqlDml.Literal(value);
    }
#endif
  }
}
