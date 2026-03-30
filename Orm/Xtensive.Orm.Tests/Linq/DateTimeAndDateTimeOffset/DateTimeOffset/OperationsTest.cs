// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class OperationsTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void AddYearsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddYears(1) == FirstDateTimeOffset.AddYears(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddYears(-2) == FirstMillisecondDateTimeOffset.AddYears(-2));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddYears(33) == NullableDateTimeOffset.AddYears(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddYears(1) == FirstDateTimeOffset.AddYears(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddYears(-1) == FirstMillisecondDateTimeOffset.AddYears(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddYears(33) == NullableDateTimeOffset.AddYears(44));
      });
    }

    [Test]
    public void MinMaxValueAddYearsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddYears(1) == DateTimeOffset.MinValue.AddYears(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddYears(-2) == DateTimeOffset.MaxValue.AddYears(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddYears(1) == FirstDateTimeOffset.AddYears(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddYears(-1) == FirstDateTimeOffset.AddYears(-2));
      });
    }

    [Test]
    public void AddMonthsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddMonths(1) == FirstDateTimeOffset.AddMonths(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddMonths(-2) == FirstMillisecondDateTimeOffset.AddMonths(-2));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddMonths(33) == NullableDateTimeOffset.AddMonths(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddMonths(1) == FirstDateTimeOffset.AddMonths(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddMonths(-1) == FirstMillisecondDateTimeOffset.AddMonths(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddMonths(33) == NullableDateTimeOffset.AddMonths(44));
      });
    }

    [Test]
    public void MinMaxValueAddMonthsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddMonths(1) == DateTimeOffset.MinValue.AddMonths(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddMonths(-2) == DateTimeOffset.MaxValue.AddMonths(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddMonths(1) == FirstDateTimeOffset.AddMonths(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddMonths(-1) == FirstDateTimeOffset.AddMonths(-2));
      });
    }

    [Test]
    public void AddDaysTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddDays(1) == FirstDateTimeOffset.AddDays(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddDays(-2) == FirstMillisecondDateTimeOffset.AddDays(-2));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddDays(33) == NullableDateTimeOffset.AddDays(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddDays(1) == FirstDateTimeOffset.AddDays(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddDays(-1) == FirstMillisecondDateTimeOffset.AddDays(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddDays(33) == NullableDateTimeOffset.AddDays(44));
      });
    }

    [Test]
    public void MinMaxValueAddDaysTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddDays(1) == DateTimeOffset.MinValue.AddDays(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddDays(-2) == DateTimeOffset.MaxValue.AddDays(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddDays(1) == FirstDateTimeOffset.AddDays(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddDays(-1) == FirstDateTimeOffset.AddDays(-2));
      });
    }

    [Test]
    public void AddHoursTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddHours(1) == FirstDateTimeOffset.AddHours(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddHours(-2) == FirstMillisecondDateTimeOffset.AddHours(-2));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddHours(33) == NullableDateTimeOffset.AddHours(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddHours(1) == FirstDateTimeOffset.AddHours(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddHours(-1) == FirstMillisecondDateTimeOffset.AddHours(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddHours(33) == NullableDateTimeOffset.AddHours(44));
      });
    }

    [Test]
    public void MinMaxValueAddHoursTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddHours(1) == DateTimeOffset.MinValue.AddHours(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddHours(-2) == DateTimeOffset.MaxValue.AddHours(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddHours(1) == FirstDateTimeOffset.AddHours(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddHours(-1) == FirstDateTimeOffset.AddHours(-2));
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddMinutes(1) == FirstDateTimeOffset.AddMinutes(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddMinutes(-2) == FirstMillisecondDateTimeOffset.AddMinutes(-2));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddMinutes(33) == NullableDateTimeOffset.AddMinutes(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddMinutes(1) == FirstDateTimeOffset.AddMinutes(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddMinutes(-1) == FirstMillisecondDateTimeOffset.AddMinutes(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddMinutes(33) == NullableDateTimeOffset.AddMinutes(44));
      });
    }

    [Test]
    public void MinMaxValueAddMinutesTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddMinutes(1) == DateTimeOffset.MinValue.AddMinutes(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddMinutes(-2) == DateTimeOffset.MaxValue.AddMinutes(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddMinutes(1) == FirstDateTimeOffset.AddMinutes(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddMinutes(-1) == FirstDateTimeOffset.AddMinutes(-2));
      });
    }

    [Test]
    public void AddSecondsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddSeconds(1) == FirstDateTimeOffset.AddSeconds(1));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddSeconds(-2) == FirstMillisecondDateTimeOffset.AddSeconds(-2));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddSeconds(33) == NullableDateTimeOffset.AddSeconds(33));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.AddSeconds(1) == FirstDateTimeOffset.AddSeconds(2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddSeconds(-1) == FirstMillisecondDateTimeOffset.AddSeconds(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.AddSeconds(33) == NullableDateTimeOffset.AddSeconds(44));
      });
    }

    [Test]
    public void MinMaxValueAddSecondsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddSeconds(1) == DateTimeOffset.MinValue.AddSeconds(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddSeconds(-2) == DateTimeOffset.MaxValue.AddSeconds(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddSeconds(1) == FirstDateTimeOffset.AddSeconds(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddSeconds(-1) == FirstDateTimeOffset.AddSeconds(-2));
      });
    }

    [Test]
    public void AddMillisecondsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddMilliseconds(-2) == FirstMillisecondDateTimeOffset.AddMilliseconds(-2));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.AddMilliseconds(-1) == FirstMillisecondDateTimeOffset.AddMilliseconds(-2));
      });
    }

    [Test]
    public void MinMaxValueAddMillisecondsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddMilliseconds(1) == DateTimeOffset.MinValue.AddMilliseconds(1));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddMilliseconds(-2) == DateTimeOffset.MaxValue.AddMilliseconds(-2));

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.AddMilliseconds(1) == FirstDateTimeOffset.AddMilliseconds(2));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.AddMilliseconds(-1) == FirstDateTimeOffset.AddMilliseconds(-2));
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Add(FirstOffset) == FirstDateTimeOffset.Add(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Add(SecondOffset) == FirstMillisecondDateTimeOffset.Add(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Add(FirstOffset) == NullableDateTimeOffset.Add(FirstOffset));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Add(FirstOffset) == FirstDateTimeOffset.Add(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Add(SecondOffset) == FirstMillisecondDateTimeOffset.Add(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Add(FirstOffset) == NullableDateTimeOffset.Add(WrongOffset));
      });
    }

    [Test]
    public void MinValueAddTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Add(FirstOffset) == DateTimeOffset.MinValue.Add(FirstOffset));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Add(FirstOffset) == FirstDateTimeOffset.Add(WrongOffset));
      });
    }

    [Test]
    public void SubtractTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Subtract(FirstOffset) == FirstDateTimeOffset.Subtract(FirstOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Subtract(SecondOffset) == FirstMillisecondDateTimeOffset.Subtract(SecondOffset));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Subtract(FirstOffset) == NullableDateTimeOffset.Subtract(FirstOffset));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Subtract(FirstOffset) == FirstDateTimeOffset.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Subtract(SecondOffset) == FirstMillisecondDateTimeOffset.Subtract(WrongOffset));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Subtract(FirstOffset) == NullableDateTimeOffset.Subtract(WrongOffset));
      });
    }

    [Test]
    public void MaxValueSubtractTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(FirstOffset) == DateTimeOffset.MaxValue.Subtract(FirstOffset));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(FirstOffset) == FirstDateTimeOffset.Subtract(WrongOffset));
      });
    }

    [Test]
    public void SubtractDateTimeTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Subtract(SecondDateTime) == FirstDateTimeOffset.Subtract(SecondDateTime));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Subtract(SecondDateTime) == FirstMillisecondDateTimeOffset.Subtract(SecondDateTime));
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Subtract(SecondDateTime) == NullableDateTimeOffset.Subtract(SecondDateTime));

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Subtract(SecondDateTime) == FirstDateTimeOffset.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Subtract(SecondDateTime) == FirstMillisecondDateTimeOffset.Subtract(WrongDateTime));
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Subtract(SecondDateTime) == NullableDateTimeOffset.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void MaxValueSubtractDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(SecondDateTime) == DateTimeOffset.MaxValue.Subtract(SecondDateTime));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(SecondDateTime) == FirstDateTimeOffset.Subtract(WrongDateTime));
      });
    }

    [Test]
    public void SubstractDateTimeOffsetAndIntervalUsageTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Subtract(SecondDateTimeOffset).TotalMilliseconds == FirstDateTimeOffset.Subtract(SecondDateTimeOffset).TotalMilliseconds);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Subtract(FirstDateTimeOffset).TotalMilliseconds == FirstMillisecondDateTimeOffset.Subtract(FirstDateTimeOffset).TotalMilliseconds);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Subtract(SecondDateTimeOffset).TotalMilliseconds == NullableDateTimeOffset.Subtract(SecondDateTimeOffset).TotalMilliseconds);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Subtract(SecondDateTimeOffset).TotalMilliseconds == FirstDateTimeOffset.Subtract(WrongDateTimeOffset).TotalMilliseconds);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Subtract(SecondDateTimeOffset).TotalMilliseconds == FirstMillisecondDateTimeOffset.Subtract(WrongDateTimeOffset).TotalMilliseconds);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Subtract(SecondDateTimeOffset).TotalMilliseconds == NullableDateTimeOffset.Subtract(WrongDateTimeOffset).TotalMilliseconds);
      });
    }

    [Test]
    public void MaxValueSubstractDateTimeOffsetAndIntervalUsageTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(SecondDateTimeOffset) == DateTimeOffset.MaxValue.Subtract(SecondDateTimeOffset));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(SecondDateTimeOffset).TotalMilliseconds == DateTimeOffset.MaxValue.Subtract(SecondDateTimeOffset).TotalMilliseconds);

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(SecondDateTimeOffset) == DateTimeOffset.MaxValue.Subtract(WrongDateTimeOffset));
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Subtract(SecondDateTimeOffset).TotalMilliseconds == DateTimeOffset.MaxValue.Subtract(WrongDateTimeOffset).TotalMilliseconds);
      });
    }

    [Test]
    public void PlusTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset + FirstOffset == FirstDateTimeOffset + FirstOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset + SecondOffset == FirstMillisecondDateTimeOffset + SecondOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset + FirstOffset == NullableDateTimeOffset + FirstOffset);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset + FirstOffset == FirstDateTimeOffset + WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset + SecondOffset == FirstMillisecondDateTimeOffset + WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset + FirstOffset == NullableDateTimeOffset + WrongOffset);
      });
    }

    [Test]
    public void MinValuePlusTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue + FirstOffset == DateTimeOffset.MinValue + FirstOffset);

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue + FirstOffset == DateTimeOffset.MinValue + WrongOffset);
      });
    }

    [Test]
    public void MinusTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset - FirstOffset == FirstDateTimeOffset - FirstOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset - SecondOffset == FirstMillisecondDateTimeOffset - SecondOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset - FirstOffset == NullableDateTimeOffset - FirstOffset);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset - FirstOffset == FirstDateTimeOffset - WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset - SecondOffset == FirstMillisecondDateTimeOffset - WrongOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset - FirstOffset == NullableDateTimeOffset - WrongOffset);
      });
    }

    [Test]
    public void MaxValueMinusTimeSpanTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue - FirstOffset == DateTimeOffset.MaxValue - FirstOffset);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue - FirstOffset == DateTimeOffset.MaxValue - WrongOffset);
      });
    }

    [Test]
    public void MinusDateTimeTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset - SecondDateTime == FirstDateTimeOffset - SecondDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset - SecondDateTime == FirstMillisecondDateTimeOffset - SecondDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset - SecondDateTime == NullableDateTimeOffset - SecondDateTime);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset - SecondDateTime == FirstDateTimeOffset - WrongDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset - SecondDateTime == FirstMillisecondDateTimeOffset - WrongDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset - SecondDateTime == NullableDateTimeOffset - WrongDateTime);
      });
    }

    [Test]
    public void MaxValueMinusDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue - SecondDateTime == DateTimeOffset.MaxValue - SecondDateTime);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue - SecondDateTime == DateTimeOffset.MaxValue - WrongDateTime);
      });
    }

    [Test]
    public void MinusDateTimeOffsetAndIntervalUsageTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => (c.DateTimeOffset - SecondDateTimeOffset).TotalMilliseconds == (FirstDateTimeOffset - SecondDateTimeOffset).TotalMilliseconds);
        RunTest<SingleDateTimeOffsetEntity>(s, c => (c.MillisecondDateTimeOffset - FirstDateTimeOffset).TotalMilliseconds == (FirstMillisecondDateTimeOffset - FirstDateTimeOffset).TotalMilliseconds);
        RunTest<SingleDateTimeOffsetEntity>(s, c => (c.NullableDateTimeOffset.Value - SecondDateTimeOffset).TotalMilliseconds == (NullableDateTimeOffset - SecondDateTimeOffset).TotalMilliseconds);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => (c.DateTimeOffset - SecondDateTimeOffset).TotalMilliseconds == (FirstDateTimeOffset - WrongDateTimeOffset).TotalMilliseconds);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => (c.MillisecondDateTimeOffset - SecondDateTimeOffset).TotalMilliseconds == (FirstMillisecondDateTimeOffset - WrongDateTimeOffset).TotalMilliseconds);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => (c.NullableDateTimeOffset.Value - SecondDateTimeOffset).TotalMilliseconds == (NullableDateTimeOffset - WrongDateTimeOffset).TotalMilliseconds);
      });
    }

    [Test]
    public void MaxValueMinusDateTimeOffsetAndIntervalUsageTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue - FirstDateTimeOffset == DateTimeOffset.MaxValue - FirstDateTimeOffset);
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => (c.MaxValue - FirstDateTimeOffset).TotalMilliseconds == (DateTimeOffset.MaxValue - FirstDateTimeOffset).TotalMilliseconds);

        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue - FirstDateTimeOffset == DateTimeOffset.MaxValue - WrongDateTimeOffset);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => (c.MaxValue - FirstDateTimeOffset).TotalMilliseconds == (DateTimeOffset.MaxValue - WrongDateTimeOffset).TotalMilliseconds);
      });
    }

    [Test]
    public void ToUniversalTime()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql, "ToUniversalTime is not supported");
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.ToUniversalTime() == FirstDateTimeOffset.ToUniversalTime());
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.ToUniversalTime() == FirstMillisecondDateTimeOffset.ToUniversalTime());
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.ToUniversalTime() == NullableDateTimeOffset.ToUniversalTime());
      });
    }

    [Test]
    public void ToUniversalTimePostgresql()
    {
      Require.ProviderIs(StorageProvider.PostgreSql, "ToUniversalTime is not supported");
      ExecuteInsideSession((s) => {
        var ex = Assert.Throws<QueryTranslationException>(()=> RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.ToUniversalTime() == FirstDateTimeOffset.ToUniversalTime()));
        Assert.That(ex.InnerException, Is.TypeOf<NotSupportedException>());
      });
    }

    [Test]
    public void ToLocalTimePostgresql()
    {
      Require.ProviderIs(StorageProvider.PostgreSql, "ToLocalTime is not supported");
      ExecuteInsideSession((s) => {
        var ex = Assert.Throws<QueryTranslationException>(() => RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.ToLocalTime() == FirstDateTimeOffset.ToLocalTime()));
        Assert.That(ex.InnerException, Is.TypeOf<NotSupportedException>());
      });
    }
  }
}
