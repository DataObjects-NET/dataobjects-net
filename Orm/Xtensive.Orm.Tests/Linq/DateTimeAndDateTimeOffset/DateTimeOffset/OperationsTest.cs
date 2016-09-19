// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class OperationsTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void AddYearsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddYears(1)==FirstDateTimeOffset.AddYears(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddYears(-2)==FirstMillisecondDateTimeOffset.AddYears(-2));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddYears(33)==NullableDateTimeOffset.AddYears(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddYears(1)==FirstDateTimeOffset.AddYears(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddYears(-1)==FirstMillisecondDateTimeOffset.AddYears(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddYears(33)==NullableDateTimeOffset.AddYears(44));
      });
    }

    [Test]
    public void AddMonthsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddMonths(1)==FirstDateTimeOffset.AddMonths(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddMonths(-2)==FirstMillisecondDateTimeOffset.AddMonths(-2));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddMonths(33)==NullableDateTimeOffset.AddMonths(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddMonths(1)==FirstDateTimeOffset.AddMonths(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddMonths(-1)==FirstMillisecondDateTimeOffset.AddMonths(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddMonths(33)==NullableDateTimeOffset.AddMonths(44));
      });
    }

    [Test]
    public void AddDaysTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddDays(1)==FirstDateTimeOffset.AddDays(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddDays(-2)==FirstMillisecondDateTimeOffset.AddDays(-2));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddDays(33)==NullableDateTimeOffset.AddDays(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddDays(1)==FirstDateTimeOffset.AddDays(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddDays(-1)==FirstMillisecondDateTimeOffset.AddDays(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddDays(33)==NullableDateTimeOffset.AddDays(44));
      });
    }

    [Test]
    public void AddHoursTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddHours(1)==FirstDateTimeOffset.AddHours(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddHours(-2)==FirstMillisecondDateTimeOffset.AddHours(-2));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddHours(33)==NullableDateTimeOffset.AddHours(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddHours(1)==FirstDateTimeOffset.AddHours(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddHours(-1)==FirstMillisecondDateTimeOffset.AddHours(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddHours(33)==NullableDateTimeOffset.AddHours(44));
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddMinutes(1)==FirstDateTimeOffset.AddMinutes(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddMinutes(-2)==FirstMillisecondDateTimeOffset.AddMinutes(-2));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddMinutes(33)==NullableDateTimeOffset.AddMinutes(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddMinutes(1)==FirstDateTimeOffset.AddMinutes(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddMinutes(-1)==FirstMillisecondDateTimeOffset.AddMinutes(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddMinutes(33)==NullableDateTimeOffset.AddMinutes(44));
      });
    }

    [Test]
    public void AddSecondsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddSeconds(1)==FirstDateTimeOffset.AddSeconds(1));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddSeconds(-2)==FirstMillisecondDateTimeOffset.AddSeconds(-2));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddSeconds(33)==NullableDateTimeOffset.AddSeconds(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.AddSeconds(1)==FirstDateTimeOffset.AddSeconds(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddSeconds(-1)==FirstMillisecondDateTimeOffset.AddSeconds(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.AddSeconds(33)==NullableDateTimeOffset.AddSeconds(44));
      });
    }

    [Test]
    public void AddMillisecondsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddMilliseconds(-2)==FirstMillisecondDateTimeOffset.AddMilliseconds(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.AddMilliseconds(-1)==FirstMillisecondDateTimeOffset.AddMilliseconds(-2));
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Add(FirstOffset)==FirstDateTimeOffset.Add(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Add(SecondOffset)==FirstMillisecondDateTimeOffset.Add(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Add(FirstOffset)==NullableDateTimeOffset.Add(FirstOffset));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Add(FirstOffset)==FirstDateTimeOffset.Add(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Add(SecondOffset)==FirstMillisecondDateTimeOffset.Add(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Add(FirstOffset)==NullableDateTimeOffset.Add(WrongOffset));
      });
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Subtract(FirstOffset)==FirstDateTimeOffset.Subtract(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Subtract(SecondOffset)==FirstMillisecondDateTimeOffset.Subtract(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Subtract(FirstOffset)==NullableDateTimeOffset.Subtract(FirstOffset));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Subtract(FirstOffset)==FirstDateTimeOffset.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Subtract(SecondOffset)==FirstMillisecondDateTimeOffset.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Subtract(FirstOffset)==NullableDateTimeOffset.Subtract(WrongOffset));
      });
    }

    [Test]
    public void SubtractDateTimeTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Subtract(SecondDateTime)==FirstDateTimeOffset.Subtract(SecondDateTime));
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Subtract(SecondDateTime)==FirstMillisecondDateTimeOffset.Subtract(SecondDateTime));
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Subtract(SecondDateTime)==NullableDateTimeOffset.Subtract(SecondDateTime));

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.Subtract(SecondDateTime)==FirstDateTimeOffset.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.Subtract(SecondDateTime)==FirstMillisecondDateTimeOffset.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.Subtract(SecondDateTime)==NullableDateTimeOffset.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void PlusTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset + FirstOffset==FirstDateTimeOffset + FirstOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset + SecondOffset==FirstMillisecondDateTimeOffset + SecondOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset + FirstOffset==NullableDateTimeOffset + FirstOffset);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset + FirstOffset==FirstDateTimeOffset + WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset + SecondOffset==FirstMillisecondDateTimeOffset + WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset + FirstOffset==NullableDateTimeOffset + WrongOffset);
      });
    }

    [Test]
    public void MinusTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset - FirstOffset==FirstDateTimeOffset - FirstOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset - SecondOffset==FirstMillisecondDateTimeOffset - SecondOffset);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset - FirstOffset==NullableDateTimeOffset - FirstOffset);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset - FirstOffset==FirstDateTimeOffset - WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset - SecondOffset==FirstMillisecondDateTimeOffset - WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset - FirstOffset==NullableDateTimeOffset - WrongOffset);
      });
    }

    [Test]
    public void MinusDateTimeTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset - SecondDateTime==FirstDateTimeOffset - SecondDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset - SecondDateTime==FirstMillisecondDateTimeOffset - SecondDateTime);
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset - SecondDateTime==NullableDateTimeOffset - SecondDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset - SecondDateTime==FirstDateTimeOffset - WrongDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset - SecondDateTime==FirstMillisecondDateTimeOffset - WrongDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset - SecondDateTime==NullableDateTimeOffset - WrongDateTime);
      });
    }

    [Test]
    public void ToUniversalTime()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeOffsetEntity>(c => c.DateTimeOffset.ToUniversalTime()==FirstDateTimeOffset.ToUniversalTime());
        RunTest<SingleDateTimeOffsetEntity>(c => c.MillisecondDateTimeOffset.ToUniversalTime()==FirstMillisecondDateTimeOffset.ToUniversalTime());
        RunTest<SingleDateTimeOffsetEntity>(c => c.NullableDateTimeOffset.Value.ToUniversalTime()==NullableDateTimeOffset.ToUniversalTime());
      });
    }
  }
}
