// Copyright (C) 2016-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class OperationsTest : DateTimeBaseTest
  {
    [Test]
    public void AddYearsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddYears(1) == FirstDateTime.AddYears(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddYears(-2) == FirstMillisecondDateTime.AddYears(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddYears(33) == NullableDateTime.AddYears(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddYears(1) == FirstDateTime.AddYears(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddYears(-1) == FirstMillisecondDateTime.AddYears(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddYears(33) == NullableDateTime.AddYears(44));
      });
    }

    [Test]
    public void AddMonthsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddMonths(1) == FirstDateTime.AddMonths(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMonths(-2) == FirstMillisecondDateTime.AddMonths(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMonths(33) == NullableDateTime.AddMonths(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddMonths(1) == FirstDateTime.AddMonths(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMonths(-1) == FirstMillisecondDateTime.AddMonths(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMonths(33) == NullableDateTime.AddMonths(44));
      });
    }

    [Test]
    public void AddDaysTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddDays(1) == FirstDateTime.AddDays(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddDays(-2) == FirstMillisecondDateTime.AddDays(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddDays(33) == NullableDateTime.AddDays(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddDays(1) == FirstDateTime.AddDays(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddDays(-1) == FirstMillisecondDateTime.AddDays(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddDays(33) == NullableDateTime.AddDays(44));
      });
    }

    [Test]
    public void AddHoursTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddHours(1) == FirstDateTime.AddHours(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddHours(-2) == FirstMillisecondDateTime.AddHours(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddHours(33) == NullableDateTime.AddHours(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddHours(1) == FirstDateTime.AddHours(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddHours(-1) == FirstMillisecondDateTime.AddHours(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddHours(33) == NullableDateTime.AddHours(44));
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddMinutes(1) == FirstDateTime.AddMinutes(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMinutes(-2) == FirstMillisecondDateTime.AddMinutes(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMinutes(33) == NullableDateTime.AddMinutes(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddMinutes(1) == FirstDateTime.AddMinutes(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMinutes(-1) == FirstMillisecondDateTime.AddMinutes(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddMinutes(33) == NullableDateTime.AddMinutes(44));
      });
    }

    [Test]
    public void AddSecondsTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.AddSeconds(1) == FirstDateTime.AddSeconds(1));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddSeconds(-2) == FirstMillisecondDateTime.AddSeconds(-2));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddSeconds(33) == NullableDateTime.AddSeconds(33));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.AddSeconds(1) == FirstDateTime.AddSeconds(2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddSeconds(-1) == FirstMillisecondDateTime.AddSeconds(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.AddSeconds(33) == NullableDateTime.AddSeconds(44));
      });
    }

    [Test]
    public void AddMillisecondsTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMilliseconds(-2) == FirstMillisecondDateTime.AddMilliseconds(-2));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.AddMilliseconds(-1) == FirstMillisecondDateTime.AddMilliseconds(-2));
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Add(FirstOffset) == FirstDateTime.Add(FirstOffset));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Add(SecondOffset) == FirstMillisecondDateTime.Add(SecondOffset));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Add(FirstOffset) == NullableDateTime.Add(FirstOffset));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Add(FirstOffset) == FirstDateTime.Add(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Add(SecondOffset) == FirstMillisecondDateTime.Add(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Add(FirstOffset) == NullableDateTime.Add(WrongOffset));
      });
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(FirstOffset) == FirstDateTime.Subtract(FirstOffset));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondOffset) == FirstMillisecondDateTime.Subtract(SecondOffset));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(FirstOffset) == NullableDateTime.Subtract(FirstOffset));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(FirstOffset) == FirstDateTime.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondOffset) == FirstMillisecondDateTime.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(FirstOffset) == NullableDateTime.Subtract(WrongOffset));
      });
    }

    [Test]
    public void SubtractDateTimeTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(SecondDateTime) == FirstDateTime.Subtract(SecondDateTime));
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondDateTime) == FirstMillisecondDateTime.Subtract(SecondDateTime));
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(SecondDateTime) == NullableDateTime.Subtract(SecondDateTime));

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime.Subtract(SecondDateTime) == FirstDateTime.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime.Subtract(SecondDateTime) == FirstMillisecondDateTime.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime.Value.Subtract(SecondDateTime) == NullableDateTime.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void PlusTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime + FirstOffset == FirstDateTime + FirstOffset);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime + SecondOffset == FirstMillisecondDateTime + SecondOffset);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime + FirstOffset == NullableDateTime + FirstOffset);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime + FirstOffset == FirstDateTime + WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime + SecondOffset == FirstMillisecondDateTime + WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime + FirstOffset == NullableDateTime + WrongOffset);
      });
    }

    [Test]
    public void MinusTimeSpanTest()
    {
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime - FirstOffset == FirstDateTime - FirstOffset);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondOffset == FirstMillisecondDateTime - SecondOffset);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime - FirstOffset == NullableDateTime - FirstOffset);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime - FirstOffset == FirstDateTime - WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondOffset == FirstMillisecondDateTime - WrongOffset);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime - FirstOffset == NullableDateTime - WrongOffset);
      });
    }

    [Test]
    public void MinusDateTimeTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession(() => {
        RunTest<SingleDateTimeEntity>(c => c.DateTime - SecondDateTime == FirstDateTime - SecondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondDateTime == FirstMillisecondDateTime - SecondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime - SecondDateTime == NullableDateTime - SecondDateTime);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime - SecondDateTime == FirstDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - SecondDateTime == FirstMillisecondDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime - SecondDateTime == NullableDateTime - WrongDateTime);
      });
    }

    [Test]
    public void MysqlMinusDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      ExecuteInsideSession(() => {
        var firstDateTime = FirstDateTime.AdjustDateTime(0);
        var firstMillisecondDateTime = FirstMillisecondDateTime.AdjustDateTime(0);
        var secondDateTime = SecondDateTime.AdjustDateTime(0);
        var nullableDateTime = NullableDateTime.AdjustDateTime(0);

        RunTest<SingleDateTimeEntity>(c => c.DateTime - secondDateTime == firstDateTime - secondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - secondDateTime == firstMillisecondDateTime - secondDateTime);
        RunTest<SingleDateTimeEntity>(c => c.NullableDateTime - secondDateTime == nullableDateTime - secondDateTime);

        RunWrongTest<SingleDateTimeEntity>(c => c.DateTime - secondDateTime == firstDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.MillisecondDateTime - secondDateTime == firstMillisecondDateTime - WrongDateTime);
        RunWrongTest<SingleDateTimeEntity>(c => c.NullableDateTime - secondDateTime == nullableDateTime - WrongDateTime);
      });
    }
  }
}
