// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeExtractTest : BaseDateTimeAndDateTimeOffsetTest
  {
    [Test]
    public void ExtractYearTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Year==FirstDateTime.Year);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Year==FirstMillisecondDateTime.Year);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Year==NullableDateTimeOffset.Year);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Year==WrongDateTime.Year);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Year==WrongMillisecondDateTime.Year);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Year==WrongDateTime.Year);
      });
    }

    [Test]
    public void ExtractMonthTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Month==FirstDateTime.Month);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Month==FirstMillisecondDateTime.Month);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Month==NullableDateTimeOffset.Month);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Month==WrongDateTime.Month);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Month==WrongMillisecondDateTime.Month);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Month==WrongDateTime.Month);
      });
    }

    [Test]
    public void ExtractDayTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Day==FirstDateTime.Day);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Day==FirstMillisecondDateTime.Day);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Day==NullableDateTimeOffset.Day);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Day==WrongDateTime.Day);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Day==WrongMillisecondDateTime.Day);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Day==WrongDateTime.Day);
      });
    }

    [Test]
    public void ExtractHourTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Hour==FirstDateTime.Hour);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Hour==FirstMillisecondDateTime.Hour);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Hour==NullableDateTimeOffset.Hour);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Hour==WrongDateTime.Hour);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Hour==WrongMillisecondDateTime.Hour);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Hour==WrongDateTime.Hour);
      });
    }

    [Test]
    public void ExtractMinuteTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Minute==FirstDateTime.Minute);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Minute==FirstMillisecondDateTime.Minute);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Minute==NullableDateTimeOffset.Minute);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Minute==WrongDateTime.Minute);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Minute==WrongMillisecondDateTime.Minute);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Minute==WrongDateTime.Minute);
      });
    }

    [Test]
    public void ExtractSecondTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Second==FirstDateTime.Second);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Second==FirstMillisecondDateTime.Second);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Second==NullableDateTimeOffset.Second);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Second==WrongDateTime.Second);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Second==WrongMillisecondDateTime.Second);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Second==WrongDateTime.Second);
      });
    }

    [Test]
    public void ExtractMillisecondTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Millisecond==FirstMillisecondDateTime.Millisecond);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Second==WrongMillisecondDateTime.Millisecond);
      });
    }

    [Test]
    public void ExtractDateTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Date==FirstDateTime.Date);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Date==FirstMillisecondDateTime.Date);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Date==NullableDateTimeOffset.Date);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Date==WrongDateTime.Date);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Date==WrongMillisecondDateTime.Date);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Date==WrongDateTime.Date);
      });
    }

    [Test]
    public void ExtractTimeOfDayTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.TimeOfDay==FirstDateTime.TimeOfDay);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.TimeOfDay==FirstMillisecondDateTime.TimeOfDay);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.TimeOfDay==NullableDateTimeOffset.TimeOfDay);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.TimeOfDay==WrongDateTime.TimeOfDay);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.TimeOfDay==WrongMillisecondDateTime.TimeOfDay);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.TimeOfDay==WrongDateTime.TimeOfDay);
      });
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.DayOfYear==FirstDateTime.DayOfYear);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.DayOfYear==FirstMillisecondDateTime.DayOfYear);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.DayOfYear==NullableDateTimeOffset.DayOfYear);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.DayOfYear==WrongDateTime.DayOfYear);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.DayOfYear==WrongMillisecondDateTime.DayOfYear);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.DayOfYear==WrongDateTime.DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.DayOfWeek==FirstDateTime.DayOfWeek);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.DayOfWeek==FirstMillisecondDateTime.DayOfWeek);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.DayOfWeek==NullableDateTimeOffset.DayOfWeek);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.DayOfWeek==WrongDateTime.DayOfWeek);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.DayOfWeek==WrongMillisecondDateTime.DayOfWeek);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.DayOfWeek==WrongDateTime.DayOfWeek);
      });
    }
  }
}
