// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeOffsetExtractTest : BaseDateTimeAndDateTimeOffsetTest
  {
    protected override void CheckRequirements()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
    }

    [Test]
    public void ExtractYearTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Year==FirstDateTimeOffset.Year);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Year==FirstMillisecondDateTimeOffset.Year);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Year==NullableDateTimeOffset.Year);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Year==WrongDateTimeOffset.Year);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Year==WrongMillisecondDateTimeOffset.Year);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Year==WrongDateTimeOffset.Year);
      });
    }

    [Test]
    public void ExtractMonthTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Month==FirstDateTimeOffset.Month);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Month==FirstMillisecondDateTimeOffset.Month);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Month==NullableDateTimeOffset.Month);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Month==WrongDateTimeOffset.Month);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Month==WrongMillisecondDateTimeOffset.Month);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Month==WrongDateTimeOffset.Month);
      });
    }

    [Test]
    public void ExtractDayTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Day==FirstDateTimeOffset.Day);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Day==FirstMillisecondDateTimeOffset.Day);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Day==NullableDateTimeOffset.Day);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Day==WrongDateTimeOffset.Day);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Day==WrongMillisecondDateTimeOffset.Day);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Day==WrongDateTimeOffset.Day);
      });
    }

    [Test]
    public void ExtractHourTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Hour==FirstDateTimeOffset.Hour);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Hour==FirstMillisecondDateTimeOffset.Hour);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Hour==NullableDateTimeOffset.Hour);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Hour==WrongDateTimeOffset.Hour);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Hour==WrongMillisecondDateTimeOffset.Hour);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Hour==WrongDateTimeOffset.Hour);
      });
    }

    [Test]
    public void ExtractMinuteTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Minute==FirstDateTimeOffset.Minute);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Minute==FirstMillisecondDateTimeOffset.Minute);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Minute==NullableDateTimeOffset.Minute);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Minute==WrongDateTimeOffset.Minute);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Minute==WrongMillisecondDateTimeOffset.Minute);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Minute==WrongDateTimeOffset.Minute);
      });
    }

    [Test]
    public void ExtractSecondTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Second==FirstDateTimeOffset.Second);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Second==FirstMillisecondDateTimeOffset.Second);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Second==NullableDateTimeOffset.Second);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Second==WrongDateTimeOffset.Second);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Second==WrongMillisecondDateTimeOffset.Second);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Second==WrongDateTimeOffset.Second);
      });
    }

    [Test]
    public void ExtractMillisecondTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Millisecond==FirstMillisecondDateTimeOffset.Millisecond);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Second==WrongMillisecondDateTimeOffset.Millisecond);
      });
    }

    [Test]
    public void ExtractDateTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Date==FirstDateTimeOffset.Date);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Date==FirstMillisecondDateTimeOffset.Date);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Date==NullableDateTimeOffset.Date);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Date==WrongDateTimeOffset.Date);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Date==WrongMillisecondDateTimeOffset.Date);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Date==WrongDateTimeOffset.Date);
      });
    }

    [Test]
    public void ExtractTimeOfDayTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.TimeOfDay==FirstDateTimeOffset.TimeOfDay);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.TimeOfDay==FirstMillisecondDateTimeOffset.TimeOfDay);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.TimeOfDay==NullableDateTimeOffset.TimeOfDay);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.TimeOfDay==WrongDateTimeOffset.TimeOfDay);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.TimeOfDay==WrongMillisecondDateTimeOffset.TimeOfDay);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.TimeOfDay==WrongDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfYear==FirstDateTimeOffset.DayOfYear);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfYear==FirstMillisecondDateTimeOffset.DayOfYear);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfYear==NullableDateTimeOffset.DayOfYear);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfYear==WrongDateTimeOffset.DayOfYear);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfYear==WrongMillisecondDateTimeOffset.DayOfYear);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfYear==WrongDateTimeOffset.DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfWeek==FirstDateTimeOffset.DayOfWeek);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfWeek==FirstMillisecondDateTimeOffset.DayOfWeek);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfWeek==NullableDateTimeOffset.DayOfWeek);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfWeek==WrongDateTimeOffset.DayOfWeek);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfWeek==WrongMillisecondDateTimeOffset.DayOfWeek);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfWeek==WrongDateTimeOffset.DayOfWeek);
      });
    }

    [Test]
    public void ExtractDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DateTime==FirstDateTimeOffset.DateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DateTime==FirstMillisecondDateTimeOffset.DateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DateTime==NullableDateTimeOffset.DateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DateTime==WrongDateTimeOffset.DateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DateTime==WrongMillisecondDateTimeOffset.DateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DateTime==WrongDateTimeOffset.DateTime);
      });
    }

    [Test]
    public void ExtractLocalDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.LocalDateTime==FirstDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.LocalDateTime==FirstMillisecondDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.LocalDateTime==NullableDateTimeOffset.LocalDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.LocalDateTime==WrongDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.LocalDateTime==WrongMillisecondDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.LocalDateTime==WrongDateTimeOffset.LocalDateTime);
      });
    }

    [Test]
    public void ExtractUtcDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.UtcDateTime==FirstDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.UtcDateTime==FirstMillisecondDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.UtcDateTime==NullableDateTimeOffset.UtcDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.UtcDateTime==WrongDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.UtcDateTime==WrongMillisecondDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.UtcDateTime==WrongDateTimeOffset.UtcDateTime);
      });
    }
  }
}
