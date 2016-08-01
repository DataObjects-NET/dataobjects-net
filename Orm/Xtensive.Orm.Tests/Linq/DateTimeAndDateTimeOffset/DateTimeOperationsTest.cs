// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DateTimeOperationsTest : BaseDateTimeAndDateTimeOffsetTest
  {
    [Test]
    public void AddYearsTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddYears(1)==FirstDateTime.AddYears(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddYears(-2)==FirstMillisecondDateTime.AddYears(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddYears(33)==NullableDateTime.AddYears(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddYears(1)==FirstDateTime.AddYears(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddYears(-1)==FirstMillisecondDateTime.AddYears(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddYears(33)==NullableDateTime.AddYears(44));
      });
    }

    [Test]
    public void AddMonthsTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddMonths(1)==FirstDateTime.AddMonths(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMonths(-2)==FirstMillisecondDateTime.AddMonths(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMonths(33)==NullableDateTime.AddMonths(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddMonths(1)==FirstDateTime.AddMonths(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMonths(-1)==FirstMillisecondDateTime.AddMonths(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMonths(33)==NullableDateTime.AddMonths(44));
      });
    }

    [Test]
    public void AddDaysTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddDays(1)==FirstDateTime.AddDays(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddDays(-2)==FirstMillisecondDateTime.AddDays(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddDays(33)==NullableDateTime.AddDays(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddDays(1)==FirstDateTime.AddDays(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddDays(-1)==FirstMillisecondDateTime.AddDays(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddDays(33)==NullableDateTime.AddDays(44));
      });
    }

    [Test]
    public void AddHoursTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddHours(1)==FirstDateTime.AddHours(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddHours(-2)==FirstMillisecondDateTime.AddHours(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddHours(33)==NullableDateTime.AddHours(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddHours(1)==FirstDateTime.AddHours(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddHours(-1)==FirstMillisecondDateTime.AddHours(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddHours(33)==NullableDateTime.AddHours(44));
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddMinutes(1)==FirstDateTime.AddMinutes(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMinutes(-2)==FirstMillisecondDateTime.AddMinutes(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMinutes(33)==NullableDateTime.AddMinutes(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddMinutes(1)==FirstDateTime.AddMinutes(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMinutes(-1)==FirstMillisecondDateTime.AddMinutes(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMinutes(33)==NullableDateTime.AddMinutes(44));
      });
    }

    [Test]
    public void AddSecondsTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddSeconds(1)==FirstDateTime.AddSeconds(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddSeconds(-2)==FirstMillisecondDateTime.AddSeconds(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddSeconds(33)==NullableDateTime.AddSeconds(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddSeconds(1)==FirstDateTime.AddSeconds(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddSeconds(-1)==FirstMillisecondDateTime.AddSeconds(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddSeconds(33)==NullableDateTime.AddSeconds(44));
      });
    }

    [Test]
    public void AddMillisecondsTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMilliseconds(-2)==FirstMillisecondDateTime.AddMilliseconds(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMilliseconds(-1)==FirstMillisecondDateTime.AddMilliseconds(-2));
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Add(FirstOffset)==FirstDateTime.Add(FirstOffset));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Add(SecondOffset)==FirstMillisecondDateTime.Add(SecondOffset));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Add(FirstOffset)==NullableDateTime.Add(FirstOffset));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Add(FirstOffset)==FirstDateTime.Add(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Add(SecondOffset)==FirstMillisecondDateTime.Add(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Add(FirstOffset)==NullableDateTime.Add(WrongOffset));
      });
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(FirstOffset)==FirstDateTime.Subtract(FirstOffset));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondOffset)==FirstMillisecondDateTime.Subtract(SecondOffset));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(FirstOffset)==NullableDateTime.Subtract(FirstOffset));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(FirstOffset)==FirstDateTime.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondOffset)==FirstMillisecondDateTime.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(FirstOffset)==NullableDateTime.Subtract(WrongOffset));
      });
    }

    [Test]
    public void SubtractDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(SecondDateTime)==FirstDateTime.Subtract(SecondDateTime));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondDateTime)==FirstMillisecondDateTime.Subtract(SecondDateTime));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(SecondDateTime)==NullableDateTime.Subtract(SecondDateTime));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(SecondDateTime)==FirstDateTime.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondDateTime)==FirstMillisecondDateTime.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(SecondDateTime)==NullableDateTime.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void PlusTimeSpanTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime + FirstOffset==FirstDateTime + FirstOffset);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime + SecondOffset==FirstMillisecondDateTime + SecondOffset);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime + FirstOffset==NullableDateTime + FirstOffset);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime + FirstOffset==FirstDateTime + WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime + SecondOffset==FirstMillisecondDateTime + WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime + FirstOffset==NullableDateTime + WrongOffset);
      });
    }

    [Test]
    public void MinusTimeSpanTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime - FirstOffset==FirstDateTime - FirstOffset);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondOffset==FirstMillisecondDateTime - SecondOffset);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime - FirstOffset==NullableDateTime - FirstOffset);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime - FirstOffset==FirstDateTime - WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondOffset==FirstMillisecondDateTime - WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime - FirstOffset==NullableDateTime - WrongOffset);
      });
    }

    [Test]
    public void MinusDateTimeTest()
    {
      OpenSessionAndAction(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime - SecondDateTime==FirstDateTime - SecondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondDateTime==FirstMillisecondDateTime - SecondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime - SecondDateTime==NullableDateTime - SecondDateTime);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime - SecondDateTime==FirstDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondDateTime==FirstMillisecondDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime - SecondDateTime==NullableDateTime - WrongDateTime);
      });
    }
  }
}
