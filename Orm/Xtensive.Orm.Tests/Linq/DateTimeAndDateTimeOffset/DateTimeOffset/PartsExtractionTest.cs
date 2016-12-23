// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class PartsExtractionTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void ExtractYearTest()
    {
      ExecuteInsideSession(() => {
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
      ExecuteInsideSession(() => {
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
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Day==firstDateTimeOffset.Day);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Day==firstMillisecondDateTimeOffset.Day);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Day==nullableDateTimeOffset.Day);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Day==wrongDateTimeOffset.Day);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Day==wrongMillisecondDateTimeOffset.Day);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Day==wrongDateTimeOffset.Day);
      });
    }

    [Test]
    public void ExtractHourTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);

        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Hour==firstDateTimeOffset.Hour);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Hour==firstMillisecondDateTimeOffset.Hour);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Hour==nullableDateTimeOffset.Hour);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Hour==wrongDateTimeOffset.Hour);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Hour==wrongMillisecondDateTimeOffset.Hour);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Hour==wrongDateTimeOffset.Hour);
      });
    }

    [Test]
    public void ExtractMinuteTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Minute==firstDateTimeOffset.Minute);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Minute==firstMillisecondDateTimeOffset.Minute);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Minute==nullableDateTimeOffset.Minute);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Minute==wrongDateTimeOffset.Minute);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Minute==wrongMillisecondDateTimeOffset.Minute);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Minute==wrongDateTimeOffset.Minute);
      });
    }

    [Test]
    public void ExtractSecondTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Second==firstDateTimeOffset.Second);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Second==firstMillisecondDateTimeOffset.Second);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Second==nullableDateTimeOffset.Second);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Second==wrongDateTimeOffset.Second);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Second==wrongMillisecondDateTimeOffset.Second);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Second==wrongDateTimeOffset.Second);
      });
    }

    [Test]
    public void ExtractMillisecondTest()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Millisecond==firstMillisecondDateTimeOffset.Millisecond);

        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Second==wrongMillisecondDateTimeOffset.Millisecond);
      });
    }

    [Test]
    public void ExtractDateTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Date==firstDateTimeOffset.Date);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Date==firstMillisecondDateTimeOffset.Date);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Date==nullableDateTimeOffset.Date);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Date==wrongDateTimeOffset.Date);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Date==wrongMillisecondDateTimeOffset.Date);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Date==wrongDateTimeOffset.Date);
      });
    }

    [Test]
    public void ExtractTimeOfDayTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.TimeOfDay==firstDateTimeOffset.TimeOfDay);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.TimeOfDay==wrongDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractTimeOfDayWithMillisecondsTest()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.TimeOfDay==firstMillisecondDateTimeOffset.TimeOfDay);

        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.TimeOfDay==wrongMillisecondDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractTimeOfDayOfNullableValueTest()
    {
      ExecuteInsideSession(() => {
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.TimeOfDay==nullableDateTimeOffset.TimeOfDay);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.TimeOfDay==wrongDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfYear==firstDateTimeOffset.DayOfYear);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfYear==firstMillisecondDateTimeOffset.DayOfYear);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfYear==nullableDateTimeOffset.DayOfYear);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfYear==wrongDateTimeOffset.DayOfYear);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfYear==wrongMillisecondDateTimeOffset.DayOfYear);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfYear==wrongDateTimeOffset.DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfWeek==firstDateTimeOffset.DayOfWeek);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfWeek==firstMillisecondDateTimeOffset.DayOfWeek);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfWeek==nullableDateTimeOffset.DayOfWeek);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DayOfWeek==wrongDateTimeOffset.DayOfWeek);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DayOfWeek==wrongMillisecondDateTimeOffset.DayOfWeek);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DayOfWeek==wrongDateTimeOffset.DayOfWeek);
      });
    }

    [Test]
    public void ExtractDateTimeTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DateTime==firstDateTimeOffset.DateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DateTime==firstMillisecondDateTimeOffset.DateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DateTime==nullableDateTimeOffset.DateTime);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.DateTime==wrongDateTimeOffset.DateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.DateTime==wrongMillisecondDateTimeOffset.DateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.DateTime==wrongDateTimeOffset.DateTime);
      });
    }

    [Test]
    public void ExtractLocalDateTimeTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.LocalDateTime==firstDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.LocalDateTime==firstMillisecondDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.LocalDateTime==nullableDateTimeOffset.LocalDateTime);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.LocalDateTime==WrongDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.LocalDateTime==wrongMillisecondDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.LocalDateTime==wrongDateTimeOffset.LocalDateTime);
      });
    }

    [Test]
    public void ExtractUtcDateTimeTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.UtcDateTime==firstDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.UtcDateTime==firstMillisecondDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.UtcDateTime==nullableDateTimeOffset.UtcDateTime);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.UtcDateTime==wrongDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.UtcDateTime==wrongMillisecondDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.UtcDateTime==wrongDateTimeOffset.UtcDateTime);
      });
    }

    [Test]
    public void ExtractOffsetTest()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Offset==firstDateTimeOffset.Offset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Offset==firstMillisecondDateTimeOffset.Offset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Offset==nullableDateTimeOffset.Offset);
      });
    }
  }
}
