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
    protected static readonly TimeSpan DefaultTimeSpan = -new TimeSpan(4, 10, 0);
    protected static readonly TimeSpan SecondTimeSpan = new TimeSpan(9, 45, 0);

    // some other values depend on this following value, be careful when change it.
    protected static readonly DateTimeOffset DefaultDateTimeOffset = new DateTimeOffset(2001, 2, 3, 14, 15, 16, 334, DefaultTimeSpan);
    protected static readonly DateTimeOffset SecondDateTimeOffset = new DateTimeOffset(2000, 12, 11, 10, 9, 8, 765, SecondTimeSpan);
    protected static readonly TimeSpan OperationTimeSpanConst = new TimeSpan(10, 9, 8, 7, 542);
    protected static readonly int AddYearsConst = 5;
    protected static readonly int AddMonthsConst = 15;

    protected virtual bool IsNanosecondSupported
    {
      get { return true; }
    }

    protected virtual bool ShouldTransformToLocalZone
    {
      get { return false; }
    }

    [Test]
    public virtual void ExtractDateTimePartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).DateTime);
    }

    [Test]
    public virtual void ExtractDatePartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Date, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Date);
    }

    [Test]
    public virtual void ExtractYearPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Year, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Year);
    }

    [Test]
    public virtual void ExtractMonthPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Month, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Month);
    }
    
    [Test]
    public virtual void ExtractDayPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Day, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Day);
    }

    [Test]
    public virtual void ExtractDayOfWeekPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfWeek, DefaultDateTimeOffset), (int) TryTranformToLocalZone(DefaultDateTimeOffset).DayOfWeek);
    }

    [Test]
    public virtual void ExtractDayOfYearPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfYear, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).DayOfYear);
    }

    [Test]
    public virtual void ExtractHourPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Hour, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Hour);
    }

    [Test]
    public virtual void ExtractMinutePartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Minute, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Minute);
    }

    [Test]
    public virtual void ExtractSecondPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Second, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Second);
    }

    [Test]
    public virtual void ExtractMillisecondPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Millisecond);
    }

    [Test]
    public virtual void ExtractNanosecondPartTest()
    {
      if (IsNanosecondSupported)
        CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Nanosecond, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Millisecond * 1000000);
    }

    [Test]
    public virtual void ExtractOffsetPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Offset, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Offset);
    }

    [Test]
    public virtual void ExtractTimeZoneHourPartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneHour, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Offset.Hours);
    }

    [Test]
    public virtual void ExtractTimezoneMinutePartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneMinute, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Offset.Minutes);
    }

    [Test]
    public virtual void ExtractLocalDateTimePartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.LocalDateTime, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).LocalDateTime);
    }

    [Test]
    public virtual void ExtractUtcDateTimePartTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.UtcDateTime, DefaultDateTimeOffset), DefaultDateTimeOffset.UtcDateTime);
    }
    
    [Test]
    public virtual void DateTimeOffsetAddMonthsTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetAddMonths(DefaultDateTimeOffset, AddMonthsConst), TryTranformToLocalZone(DefaultDateTimeOffset).AddMonths(AddMonthsConst));
    }

    [Test]
    public virtual void DateTimeOffsetAddYearsTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetAddYears(DefaultDateTimeOffset, AddYearsConst), TryTranformToLocalZone(DefaultDateTimeOffset).AddYears(AddYearsConst));
    }

    [Test]
    public virtual void DateTimeOffsetConstructTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetConstruct(DefaultDateTimeOffset.DateTime, DefaultTimeSpan.TotalMinutes), TryTranformToLocalZone(DefaultDateTimeOffset));
    }

    [Test]
    public virtual void DateTimeOffsetMinusDateTimeOffsetTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetMinusDateTimeOffset(DefaultDateTimeOffset, SecondDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Subtract(SecondDateTimeOffset));
    }

    [Test]
    public virtual void DateTimeOffsetMinusIntervalTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, OperationTimeSpanConst), TryTranformToLocalZone(DefaultDateTimeOffset) - OperationTimeSpanConst);
    }

    [Test]
    public virtual void DateTimeOffsetPlusIntervalTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, OperationTimeSpanConst), TryTranformToLocalZone(DefaultDateTimeOffset) + OperationTimeSpanConst);
    }

    [Test]
    public virtual void DateTimeOffsetTimeOfDayTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetTimeOfDay(DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).TimeOfDay);
    }

    [Test]
    public virtual void DateTimeOffsetToUtcTimeTest()
    {
      CheckEquality(SqlDml.DateTimeOffsetToUtcTime(DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).ToUniversalTime());
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

    protected DateTimeOffset TryTranformToLocalZone(DateTimeOffset origin)
    {
      if (!ShouldTransformToLocalZone)
        return origin;
      return origin.ToLocalTime();
    }
  }
}
