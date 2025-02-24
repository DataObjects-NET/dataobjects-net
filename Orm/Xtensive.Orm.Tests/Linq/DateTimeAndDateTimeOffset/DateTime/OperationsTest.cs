// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class OperationsTest : DateTimeBaseTest
  {
    [Test]
    public void AddYearsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.AddYears(1) == FirstDateTime.AddYears(1));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddYears(-2) == FirstMillisecondDateTime.AddYears(-2));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddYears(33) == NullableDateTime.AddYears(33));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.AddYears(1) == FirstDateTime.AddYears(2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddYears(-1) == FirstMillisecondDateTime.AddYears(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddYears(33) == NullableDateTime.AddYears(44));
      });
    }

    [Test]
    public void MinMaxValueAddYearsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddYears(5) == DateTime.MinValue.AddYears(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddYears(-5) == DateTime.MaxValue.AddYears(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddYears(5) == DateTime.MinValue.AddYears(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddYears(-5) == DateTime.MaxValue.AddYears(-2));
      });
    }

    [Test]
    public void AddMonthsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.AddMonths(1) == FirstDateTime.AddMonths(1));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddMonths(-2) == FirstMillisecondDateTime.AddMonths(-2));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddMonths(33) == NullableDateTime.AddMonths(33));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.AddMonths(1) == FirstDateTime.AddMonths(2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddMonths(-1) == FirstMillisecondDateTime.AddMonths(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddMonths(33) == NullableDateTime.AddMonths(44));
      });
    }

    [Test]
    public void MinMaxValueAddMonthsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddMonths(5) == DateTime.MinValue.AddMonths(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddMonths(-5) == DateTime.MaxValue.AddMonths(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddMonths(5) == DateTime.MinValue.AddMonths(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddMonths(-5) == DateTime.MaxValue.AddMonths(-2));
      });
    }

    [Test]
    public void AddDaysTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.AddDays(1) == FirstDateTime.AddDays(1));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddDays(-2) == FirstMillisecondDateTime.AddDays(-2));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddDays(33) == NullableDateTime.AddDays(33));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.AddDays(1) == FirstDateTime.AddDays(2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddDays(-1) == FirstMillisecondDateTime.AddDays(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddDays(33) == NullableDateTime.AddDays(44));
      });
    }

    [Test]
    public void MinMaxValueAddDaysTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddDays(5) == DateTime.MinValue.AddDays(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddDays(-5) == DateTime.MaxValue.AddDays(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddDays(5) == DateTime.MinValue.AddDays(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddDays(-5) == DateTime.MaxValue.AddDays(-2));
      });
    }

    [Test]
    public void AddHoursTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.AddHours(1) == FirstDateTime.AddHours(1));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddHours(-2) == FirstMillisecondDateTime.AddHours(-2));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddHours(33) == NullableDateTime.AddHours(33));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.AddHours(1) == FirstDateTime.AddHours(2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddHours(-1) == FirstMillisecondDateTime.AddHours(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddHours(33) == NullableDateTime.AddHours(44));
      });
    }

    [Test]
    public void MinMaxValueAddHoursTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddHours(5) == DateTime.MinValue.AddHours(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddHours(-5) == DateTime.MaxValue.AddHours(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddHours(5) == DateTime.MinValue.AddHours(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddHours(-5) == DateTime.MaxValue.AddHours(-2));
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.AddMinutes(1) == FirstDateTime.AddMinutes(1));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddMinutes(-2) == FirstMillisecondDateTime.AddMinutes(-2));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddMinutes(33) == NullableDateTime.AddMinutes(33));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.AddMinutes(1) == FirstDateTime.AddMinutes(2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddMinutes(-1) == FirstMillisecondDateTime.AddMinutes(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddMinutes(33) == NullableDateTime.AddMinutes(44));
      });
    }

    [Test]
    public void MinMaxValueAddMinutesTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddMinutes(5) == DateTime.MinValue.AddMinutes(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddMinutes(-5) == DateTime.MaxValue.AddMinutes(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddMinutes(5) == DateTime.MinValue.AddMinutes(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddMinutes(-5) == DateTime.MaxValue.AddMinutes(-2));
      });
    }

    [Test]
    public void AddSecondsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.AddSeconds(1) == FirstDateTime.AddSeconds(1));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddSeconds(-2) == FirstMillisecondDateTime.AddSeconds(-2));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddSeconds(33) == NullableDateTime.AddSeconds(33));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.AddSeconds(1) == FirstDateTime.AddSeconds(2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddSeconds(-1) == FirstMillisecondDateTime.AddSeconds(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.AddSeconds(33) == NullableDateTime.AddSeconds(44));
      });
    }

    [Test]
    public void MinMaxValueAddSecondsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddSeconds(5) == DateTime.MinValue.AddSeconds(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddSeconds(-5) == DateTime.MaxValue.AddSeconds(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddSeconds(5) == DateTime.MinValue.AddSeconds(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddSeconds(-5) == DateTime.MaxValue.AddSeconds(-2));
      });
    }

    [Test]
    public void AddMillisecondsTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddMilliseconds(-2) == FirstMillisecondDateTime.AddMilliseconds(-2));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.AddMilliseconds(-1) == FirstMillisecondDateTime.AddMilliseconds(-2));
      });
    }

    [Test]
    public void MinMaxValueAddMillisecondsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddMilliseconds(5) == DateTime.MinValue.AddMilliseconds(5));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddMilliseconds(-5) == DateTime.MaxValue.AddMilliseconds(-5));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.AddMilliseconds(5) == DateTime.MinValue.AddMilliseconds(2));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.AddMilliseconds(-5) == DateTime.MaxValue.AddMilliseconds(-2));
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Add(FirstOffset) == FirstDateTime.Add(FirstOffset));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Add(SecondOffset) == FirstMillisecondDateTime.Add(SecondOffset));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Add(FirstOffset) == NullableDateTime.Add(FirstOffset));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Add(FirstOffset) == FirstDateTime.Add(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Add(SecondOffset) == FirstMillisecondDateTime.Add(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Add(FirstOffset) == NullableDateTime.Add(WrongOffset));
      });
    }

    [Test]
    public void MinValueAddTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.Add(FirstOffset) == DateTime.MinValue.Add(FirstOffset));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.Add(FirstOffset) == DateTime.MinValue.Add(WrongOffset));
      });
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Subtract(FirstOffset) == FirstDateTime.Subtract(FirstOffset));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Subtract(SecondOffset) == FirstMillisecondDateTime.Subtract(SecondOffset));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Subtract(FirstOffset) == NullableDateTime.Subtract(FirstOffset));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Subtract(FirstOffset) == FirstDateTime.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Subtract(SecondOffset) == FirstMillisecondDateTime.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Subtract(FirstOffset) == NullableDateTime.Subtract(WrongOffset));
      });
    }

    [Test]
    public void MaxValueSubtractTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.Subtract(FirstOffset) == DateTime.MaxValue.Subtract(FirstOffset));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.Subtract(FirstOffset) == DateTime.MaxValue.Subtract(WrongOffset));
      });
    }

    [Test]
    public void SubtractDateTimeTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Subtract(SecondDateTime) == FirstDateTime.Subtract(SecondDateTime));
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Subtract(SecondDateTime) == FirstMillisecondDateTime.Subtract(SecondDateTime));
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Subtract(SecondDateTime) == NullableDateTime.Subtract(SecondDateTime));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Subtract(SecondDateTime) == FirstDateTime.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Subtract(SecondDateTime) == FirstMillisecondDateTime.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Subtract(SecondDateTime) == NullableDateTime.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void MaxValueSubtractDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.Subtract(SecondDateTime) == DateTime.MaxValue.Subtract(SecondDateTime));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.Subtract(SecondDateTime) == DateTime.MaxValue.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void PlusTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime + FirstOffset == FirstDateTime + FirstOffset);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime + SecondOffset == FirstMillisecondDateTime + SecondOffset);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime + FirstOffset == NullableDateTime + FirstOffset);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime + FirstOffset == FirstDateTime + WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime + SecondOffset == FirstMillisecondDateTime + WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime + FirstOffset == NullableDateTime + WrongOffset);
      });
    }

    [Test]
    public void MinValuePlusTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue + FirstOffset == DateTime.MinValue + FirstOffset);
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue + FirstOffset == DateTime.MinValue + WrongOffset);
      });
    }

    [Test]
    public void MinusTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime - FirstOffset == FirstDateTime - FirstOffset);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime - SecondOffset == FirstMillisecondDateTime - SecondOffset);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime - FirstOffset == NullableDateTime - FirstOffset);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime - FirstOffset == FirstDateTime - WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime - SecondOffset == FirstMillisecondDateTime - WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime - FirstOffset == NullableDateTime - WrongOffset);
      });
    }

    [Test]
    public void MaxValueMinusTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue - FirstOffset == DateTime.MaxValue - FirstOffset);
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue - FirstOffset == DateTime.MaxValue - WrongOffset);
      });
    }

    [Test]
    public void MinusDateTimeTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime - SecondDateTime == FirstDateTime - SecondDateTime);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime - SecondDateTime == FirstMillisecondDateTime - SecondDateTime);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime - SecondDateTime == NullableDateTime - SecondDateTime);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime - SecondDateTime == FirstDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime - SecondDateTime == FirstMillisecondDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime - SecondDateTime == NullableDateTime - WrongDateTime);
      });
    }

    [Test]
    public void MaxValueMinusDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue - SecondDateTime == DateTime.MaxValue - SecondDateTime);
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue - SecondDateTime == DateTime.MaxValue - WrongDateTime);
      });
    }

    [Test]
    public void MysqlMinusDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        var firstDateTime = FirstDateTime.AdjustDateTime(0);
        var firstMillisecondDateTime = FirstMillisecondDateTime.AdjustDateTime(0);
        var secondDateTime = SecondDateTime.AdjustDateTime(0);
        var nullableDateTime = NullableDateTime.AdjustDateTime(0);

        RunTest<SingleDateTimeEntity>(s, c => c.DateTime - secondDateTime == firstDateTime - secondDateTime);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime - secondDateTime == firstMillisecondDateTime - secondDateTime);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime - secondDateTime == nullableDateTime - secondDateTime);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime - secondDateTime == firstDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime - secondDateTime == firstMillisecondDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime - secondDateTime == nullableDateTime - WrongDateTime);
      });
    }
  }
}
