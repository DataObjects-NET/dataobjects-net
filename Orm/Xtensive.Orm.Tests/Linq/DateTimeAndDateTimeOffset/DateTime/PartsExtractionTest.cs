// Copyright (C) 2016-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class PartsExtractionTest : DateTimeBaseTest
  {
    [Test]
    public void ExtractYearTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Year == FirstDateTime.Year);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Year == FirstMillisecondDateTime.Year);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Year == NullableDateTime.Year);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Year == WrongDateTime.Year);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Year == WrongMillisecondDateTime.Year);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Year == WrongDateTime.Year);
      });
    }

    [Test]
    public void ExtractMonthTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Month == FirstDateTime.Month);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Month == FirstMillisecondDateTime.Month);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Month == NullableDateTime.Month);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Month == WrongDateTime.Month);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Month == WrongMillisecondDateTime.Month);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Month == WrongDateTime.Month);
      });
    }

    [Test]
    public void ExtractDayTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Day == FirstDateTime.Day);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Day == FirstMillisecondDateTime.Day);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Day == NullableDateTime.Day);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Day == WrongDateTime.Day);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Day == WrongMillisecondDateTime.Day);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Day == WrongDateTime.Day);
      });
    }

    [Test]
    public void ExtractHourTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Hour == FirstDateTime.Hour);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Hour == FirstMillisecondDateTime.Hour);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Hour == NullableDateTime.Hour);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Hour == WrongDateTime.Hour);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Hour == WrongMillisecondDateTime.Hour);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Hour == WrongDateTime.Hour);
      });
    }

    [Test]
    public void ExtractMinuteTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Minute == FirstDateTime.Minute);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Minute == FirstMillisecondDateTime.Minute);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Minute == NullableDateTime.Minute);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Minute == WrongDateTime.Minute);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Minute == WrongMillisecondDateTime.Minute);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Minute == WrongDateTime.Minute);
      });
    }

    [Test]
    public void ExtractSecondTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Second == FirstDateTime.Second);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Second == FirstMillisecondDateTime.Second);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Second == NullableDateTime.Second);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Second == WrongDateTime.Second);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Second == WrongMillisecondDateTime.Second);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Second == WrongDateTime.Second);
      });
    }

    [Test]
    public void ExtractMillisecondTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Millisecond == FirstMillisecondDateTime.Millisecond);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Second == WrongMillisecondDateTime.Millisecond);
      });
    }

    [Test]
    public void MysqlExtractMillisecondTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        var firstMillisecondDateTime = FirstMillisecondDateTime.AdjustDateTime(0);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Millisecond == firstMillisecondDateTime.Millisecond);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Second == WrongMillisecondDateTime.Millisecond);
      });
    }

    [Test]
    public void ExtractDateTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.Date == FirstDateTime.Date);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Date == FirstMillisecondDateTime.Date);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Date == NullableDateTime.Date);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.Date == WrongDateTime.Date);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Date == WrongMillisecondDateTime.Date);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.Date == WrongDateTime.Date);
      });
    }

    [Test]
    [TestCase("2018-10-30 12:15:32.123")]
    [TestCase("2018-10-30 12:15:32.1234")]
    [TestCase("2018-10-30 12:15:32.12345")]
    [TestCase("2018-10-30 12:15:32.123456")]
    [TestCase("2018-10-30 12:15:32.1234567")]
    public void ExtractDateFromMicrosecondsTest(string testValueString)
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      ExecuteInsideSession((s) => {
        var testDateTime = DateTime.Parse(testValueString);
        _ = new SingleDateTimeEntity(s) { MillisecondDateTime = testDateTime };
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.Date == testDateTime.Date);
      });
    }

    [Test]
    public void ExtractTimeOfDayTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.TimeOfDay == FirstDateTime.TimeOfDay);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.TimeOfDay == FirstMillisecondDateTime.TimeOfDay);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.TimeOfDay == NullableDateTime.TimeOfDay);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.TimeOfDay == WrongDateTime.TimeOfDay);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.TimeOfDay == WrongMillisecondDateTime.TimeOfDay);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.TimeOfDay == WrongDateTime.TimeOfDay);
      });
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.DayOfYear == FirstDateTime.DayOfYear);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.DayOfYear == FirstMillisecondDateTime.DayOfYear);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.DayOfYear == NullableDateTime.DayOfYear);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.DayOfYear == WrongDateTime.DayOfYear);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.DayOfYear == WrongMillisecondDateTime.DayOfYear);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.DayOfYear == WrongDateTime.DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.DayOfWeek == FirstDateTime.DayOfWeek);
        RunTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.DayOfWeek == FirstMillisecondDateTime.DayOfWeek);
        RunTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.DayOfWeek == NullableDateTime.DayOfWeek);

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.DayOfWeek == WrongDateTime.DayOfWeek);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime.DayOfWeek == WrongMillisecondDateTime.DayOfWeek);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.NullableDateTime.Value.DayOfWeek == WrongDateTime.DayOfWeek);
      });
    }

    [Test]
    public void ExtractTimeOfDayTicksTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql | StorageProvider.Oracle);

      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.TimeOfDay.Ticks == FirstDateTime.TimeOfDay.Ticks);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.TimeOfDay.Ticks < FirstDateTime.TimeOfDay.Ticks);
      });
    }
  }
}
