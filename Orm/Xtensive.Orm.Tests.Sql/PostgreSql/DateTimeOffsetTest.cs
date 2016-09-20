// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.09.05

using System;
using NUnit.Framework;
using Npgsql;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  public class DateTimeOffsetTest : Sql.DateTimeOffsetTest
  {
    // Following values were chosen in accordance with Sql.DateTimeOffsetTest.DefaultDateTimeOffset value.
    // If base value is changed the following values may lose their actuality
    protected readonly TimeSpan DaysTimeSpan                          = new TimeSpan(2, 0, 0, 0);
    protected readonly TimeSpan WithinSameDayHoursTimeSpan            = new TimeSpan(3, 0, 0);
    protected readonly TimeSpan ReachNewDayHoursTimeSpan              = new TimeSpan(19, 0, 0);
    protected readonly TimeSpan WithinSameHourMinutesTimeSpan         = new TimeSpan(0, 20, 0);
    protected readonly TimeSpan ReachNewHourMinutesTimeSpan           = new TimeSpan(0, 38, 0);
    protected readonly TimeSpan WithinSameMinuteSecondsTimeSpan       = new TimeSpan(0, 0, 13);
    protected readonly TimeSpan ReachNewMinuteSecondsTimeSpan         = new TimeSpan(0, 0, 48);
    protected readonly TimeSpan WithinSameSecondMillisecondsTimeSpan  = new TimeSpan(0, 0, 0, 0, 100);
    protected readonly TimeSpan ReachNewSecondMillisecondsTimeSpan    = new TimeSpan(0, 0, 0, 0, 800);

    protected override bool ShouldTransformToLocalZone
    {
      get { return true; }
    }

    protected override bool IsNanosecondSupported
    {
      get { return false; }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
    }

    protected override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      var localZone = DateTimeOffset.Now.ToLocalTime().Offset;
      var localZoneString = ((localZone < TimeSpan.Zero) ? "-" : "+") + localZone.ToString(@"hh\:mm");
      using (var command = Connection.CreateCommand()) {
        command.CommandText = string.Format("SET TIME ZONE INTERVAL '{0}' HOUR TO MINUTE", localZoneString);
        command.ExecuteNonQuery();
      }
    }

    [Test]
    public override void DateTimeOffsetToUtcTimeTest()
    {
      Assert.Throws<NotSupportedException>(() => CheckEquality(SqlDml.DateTimeOffsetToUtcTime(DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).ToUniversalTime()));
    }

    [Test]
    public void ExtractDateTimePartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.DateTime, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).DateTime);
    }

    [Test]
    public void ExtractDatePartLocalTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Date, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Date);
    }
    
    [Test]
    public void ExtractYearPartLocalTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Year, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Year);
    }

    [Test]
    public void ExtractMonthPartLocalTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Month, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Month);
    }

    [Test]
    public void ExtractDayPartLocalTest()
    {
      CheckEquality(SqlDml.Extract(SqlDateTimeOffsetPart.Day, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Day);
    }

    [Test]
    public void ExtractDayOfWeekLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfWeek, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).DayOfWeek);
    }

    [Test]
    public void ExtractDayOfYearLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.DayOfYear, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).DayOfYear);
    }

    [Test]
    public void ExtractHourPartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.Hour, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Hour);
    }

    [Test]
    public void ExtractMinutePartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.Minute, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Minute);
    }

    [Test]
    public void ExtractSecondPartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.Second, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Second);
    }

    [Test]
    public void ExtractMillisecondPartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.Millisecond, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Millisecond);
    }

    [Test]
    public void ExtractOffsetPartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.Offset, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Offset);
    }

    [Test]
    public void ExtractTimezoneHourLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneHour, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Offset.Hours);
    }

    [Test]
    public void ExtractTimezoneMinuteLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.TimeZoneMinute, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Offset.Minutes);
    }

    [Test]
    public void ExtractLocalDateTimePartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.LocalDateTime, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).LocalDateTime);
    }

    [Test]
    public void ExtractUtcDateTimePartLocalTest()
    {
      CheckEqualityLocal(SqlDml.Extract(SqlDateTimeOffsetPart.UtcDateTime, DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).UtcDateTime);
    }

    [Test]
    public void AddMonthsLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetAddMonths(DefaultDateTimeOffset, AddMonthsConst), TryTranformToLocalZone(DefaultDateTimeOffset).AddMonths(AddMonthsConst));
    }

    [Test]
    public void AddYearsLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetAddYears(DefaultDateTimeOffset, AddYearsConst), TryTranformToLocalZone(DefaultDateTimeOffset).AddYears(AddYearsConst));
    }

    [Test]
    public void AddDayLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, DaysTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + DaysTimeSpan);
    }

    [Test]
    public void AddHourLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, WithinSameDayHoursTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + WithinSameDayHoursTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, ReachNewDayHoursTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + ReachNewDayHoursTimeSpan);
    }

    [Test]
    public void AddMinuteLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, WithinSameHourMinutesTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + WithinSameHourMinutesTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, ReachNewHourMinutesTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + ReachNewHourMinutesTimeSpan);
    }

    [Test]
    public void AddSecondLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, WithinSameMinuteSecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + WithinSameMinuteSecondsTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, ReachNewMinuteSecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + ReachNewMinuteSecondsTimeSpan);
    }

    [Test]
    public void AddMillisecondLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, WithinSameSecondMillisecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + WithinSameSecondMillisecondsTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetPlusInterval(DefaultDateTimeOffset, ReachNewSecondMillisecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) + ReachNewSecondMillisecondsTimeSpan);
    }

    [Test]
    public void ConstructLocalTest()
    {
      var dateTimeLocal = TryTranformToLocalZone(DefaultDateTimeOffset).DateTime;
      var dateTimeUtc = DefaultDateTimeOffset.ToOffset(new TimeSpan(0, 0, 0)).DateTime;
      var offset = DefaultTimeSpan.TotalMinutes;
      var dtoBasedOnLocalDateTime = new DateTimeOffset(dateTimeLocal, DefaultTimeSpan);
      var dtoBasedOnUtcDateTime = new DateTimeOffset(dateTimeUtc, DefaultTimeSpan);


      CheckEqualityLocal(SqlDml.DateTimeOffsetConstruct(dateTimeLocal, offset), dtoBasedOnLocalDateTime.ToLocalTime());
      CheckEqualityLocal(SqlDml.DateTimeOffsetConstruct(dateTimeUtc, offset), dtoBasedOnUtcDateTime.ToLocalTime());
    }

    [Test]
    public void DateTimeOffsetMinusDateTimeOffset()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusDateTimeOffset(DefaultDateTimeOffset, SecondDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).Subtract(SecondDateTimeOffset));
    }

    [Test]
    public void SubstractYearsLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetAddYears(DefaultDateTimeOffset, - AddYearsConst), TryTranformToLocalZone(DefaultDateTimeOffset).AddYears(-AddYearsConst));
    }

    [Test]
    public void SubstractMonthsLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetAddMonths(DefaultDateTimeOffset, - AddMonthsConst), TryTranformToLocalZone(DefaultDateTimeOffset).AddMonths(- AddMonthsConst));
    }

    [Test]
    public void SubstractDaysLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, DaysTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - DaysTimeSpan);
    }

    [Test]
    public void SubstractHoursLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, WithinSameDayHoursTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - WithinSameDayHoursTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, ReachNewDayHoursTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - ReachNewDayHoursTimeSpan);
    }

    [Test]
    public void SubstractMinutesLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, WithinSameHourMinutesTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - WithinSameHourMinutesTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, ReachNewHourMinutesTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - ReachNewHourMinutesTimeSpan);
    }

    [Test]
    public void SubstructSecondsLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, WithinSameMinuteSecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - WithinSameMinuteSecondsTimeSpan);
      CheckEqualityLocal(
       SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, ReachNewMinuteSecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - ReachNewMinuteSecondsTimeSpan);
    }

    [Test]
    public void SubstructMillisecondsLocalTest()
    {
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, WithinSameSecondMillisecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - WithinSameSecondMillisecondsTimeSpan);
      CheckEqualityLocal(
        SqlDml.DateTimeOffsetMinusInterval(DefaultDateTimeOffset, ReachNewSecondMillisecondsTimeSpan), TryTranformToLocalZone(DefaultDateTimeOffset) - ReachNewSecondMillisecondsTimeSpan);
    }

    [Test]
    public void TimeOfDayLocalTest()
    {
      CheckEqualityLocal(SqlDml.DateTimeOffsetTimeOfDay(DefaultDateTimeOffset), TryTranformToLocalZone(DefaultDateTimeOffset).TimeOfDay);
    }

    protected void CheckEqualityLocal(SqlExpression expression, int expectedValue)
    {
      var select = SqlDml.Select(expression);
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = command.ExecuteReader()) {
          reader.Read();
          if (expression.NodeType==SqlNodeType.Extract) {
            if (IsSecondOrMillisecondExtraction(expression))
              Assert.That(reader.GetInt64(0), Is.EqualTo((long) expectedValue));
            else
              Assert.That(reader.GetDouble(0), Is.EqualTo((double) expectedValue));
          }
          else
          Assert.That(reader.GetInt32(0), Is.EqualTo(expectedValue));
        }
      }
    }

    protected void CheckEqualityLocal(SqlExpression expression, TimeSpan expectedValue)
    {
      var select = SqlDml.Select(expression);
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = (NpgsqlDataReader)command.ExecuteReader()) {
          reader.Read();
          Assert.That(reader.GetTimeSpan(0), Is.EqualTo(expectedValue));
        }
      }
    }

    protected void CheckEqualityLocal(SqlExpression expression, DateTimeOffset expectedValue)
    {
      var select = SqlDml.Select(expression);
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = (NpgsqlDataReader) command.ExecuteReader()) {
          reader.Read();
          Assert.That((DateTimeOffset) reader.GetTimeStampTZ(0), Is.EqualTo(expectedValue));
        }
      } 
    }

    protected void CheckEqualityLocal(SqlExpression expression, DateTime expectedValue)
    {
      var select = SqlDml.Select(expression);
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = command.ExecuteReader()) {
          reader.Read();
          Assert.That(reader.GetDateTime(0), Is.EqualTo(expectedValue));
        }
      }
    }

    protected void CheckEqualityLocal(SqlExpression expression, DayOfWeek expectedValue)
    {
      var select = SqlDml.Select(expression);
      using (var command = Connection.CreateCommand(select)) {
        Console.WriteLine(command.CommandText);
        using (var reader = command.ExecuteReader()) {
          reader.Read();
          var readValue = reader.GetDouble(0);
          Assert.That(Enum.ToObject(typeof (DayOfWeek), Convert.ToInt32(readValue)), Is.EqualTo(expectedValue));
        }
      }
    }

    protected bool IsSecondOrMillisecondExtraction(SqlExpression expression)
    {
      var extractExpression = expression as SqlExtract;
      if (extractExpression==null)
        return false;
      return extractExpression.DateTimeOffsetPart==SqlDateTimeOffsetPart.Second ||
             extractExpression.DateTimeOffsetPart==SqlDateTimeOffsetPart.Millisecond;
    }
  }
}
