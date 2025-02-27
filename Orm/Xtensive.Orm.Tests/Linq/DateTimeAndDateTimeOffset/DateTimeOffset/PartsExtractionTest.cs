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
  public class PartsExtractionTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void ExtractYearTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s ,c => c.DateTimeOffset.Year==FirstDateTimeOffset.Year);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Year==FirstMillisecondDateTimeOffset.Year);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Year==NullableDateTimeOffset.Year);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Year==WrongDateTimeOffset.Year);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Year==WrongMillisecondDateTimeOffset.Year);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Year==WrongDateTimeOffset.Year);
      });
    }

    [Test]
    public void ExtractYearMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Year == DateTimeOffset.MinValue.Year);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Year == DateTimeOffset.MinValue.AddYears(1).Year);
      });
    }

    [Test]
    public void ExtractYearMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Year == DateTimeOffset.MaxValue.Year + 1);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Year == DateTimeOffset.MaxValue.AddYears(-1).Year);
      });
    }

    [Test]
    public void ExtractMonthTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Month==FirstDateTimeOffset.Month);
        RunTest<SingleDateTimeOffsetEntity>(s ,c => c.MillisecondDateTimeOffset.Month==FirstMillisecondDateTimeOffset.Month);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Month==NullableDateTimeOffset.Month);

        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Month==WrongDateTimeOffset.Month);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Month==WrongMillisecondDateTimeOffset.Month);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Month==WrongDateTimeOffset.Month);
      });
    }

    [Test]
    public void ExtractMonthMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Month == DateTimeOffset.MinValue.Month);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Month == DateTimeOffset.MinValue.AddMonths(1).Month);
      });
    }

    [Test]
    public void ExtractMonthMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Month == DateTimeOffset.MaxValue.Month - 11);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Month == DateTimeOffset.MaxValue.AddMonths(-1).Month);
      });
    }

    [Test]
    public void ExtractDayTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Day==firstDateTimeOffset.Day);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Day==firstMillisecondDateTimeOffset.Day);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Day==nullableDateTimeOffset.Day);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Day==wrongDateTimeOffset.Day);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Day==wrongMillisecondDateTimeOffset.Day);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Day==wrongDateTimeOffset.Day);
      });
    }

    [Test]
    public void ExtractDayMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Day == DateTimeOffset.MinValue.Day);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Day == DateTimeOffset.MinValue.AddDays(1).Day);
      });
    }

    [Test]
    public void ExtractDayMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        // year overflow happens on server side because of timezone
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Day == 1);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Day == DateTimeOffset.MaxValue.AddDays(-1).Day);
      });
    }

    [Test]
    public void ExtractHourTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);

        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Hour==firstDateTimeOffset.Hour);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Hour==firstMillisecondDateTimeOffset.Hour);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Hour==nullableDateTimeOffset.Hour);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Hour==wrongDateTimeOffset.Hour);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Hour==wrongMillisecondDateTimeOffset.Hour);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Hour==wrongDateTimeOffset.Hour);
      });
    }

    [Test]
    public void ExtractHourMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);

      ExecuteInsideSession((s) => {
        var service = s.Services.Get<Xtensive.Orm.Services.DirectSqlAccessor>();

        var command = service.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM public.\"MinMaxDateTimeOffsetEntity\" WHERE EXTRACT (HOUR FROM \"MinValue\") = 4";

        using (command)
        using (var reader = command.ExecuteReader()) {

          while (reader.Read()) {
            var rowCount = reader.GetInt32(0);
            Console.WriteLine($"Rows with HOUR 4 : {rowCount}");
          }
        }

        command = service.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM public.\"MinMaxDateTimeOffsetEntity\" WHERE EXTRACT (HOUR FROM \"MinValue\") = 5";

        using (command)
        using (var reader = command.ExecuteReader()) {

          while (reader.Read()) {
            var rowCount = reader.GetInt32(0);
            Console.WriteLine($"Rows with HOUR 5 : {rowCount}");
          }
        }

        command = service.CreateCommand();
        command.CommandText = $"SELECT (EXTRACT (TIMEZONE FROM \"MinValue\"))::integer FROM public.\"MinMaxDateTimeOffsetEntity\"";

        using (command)
        using (var reader = command.ExecuteReader()) {

          while (reader.Read()) {
            var timezoneInSeconds = reader.GetDouble(0);
            Console.WriteLine($"Timezone : {TimeSpan.FromSeconds(timezoneInSeconds)}");
          }
        }
      });


      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Hour == 5);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Hour == DateTimeOffset.MinValue.AddHours(1).Hour);
      });
    }

    [Test]
    public void ExtractHourMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);

      ExecuteInsideSession((s) => {
        var service = s.Services.Get<Xtensive.Orm.Services.DirectSqlAccessor>();

        var command = service.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM public.\"MinMaxDateTimeOffsetEntity\" WHERE EXTRACT (HOUR FROM \"MaxValue\") = 4";

        using (command)
        using (var reader = command.ExecuteReader()) {

          while (reader.Read()) {
            var rowCount = reader.GetInt32(0);
            Console.WriteLine($"Rows with HOUR 4 : {rowCount}");
          }
        }

        command = service.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM public.\"MinMaxDateTimeOffsetEntity\" WHERE EXTRACT (HOUR FROM \"MaxValue\") = 5";

        using (command)
        using (var reader = command.ExecuteReader()) {

          while (reader.Read()) {
            var rowCount = reader.GetInt32(0);
            Console.WriteLine($"Rows with HOUR 5 : {rowCount}");
          }
        }

        command = service.CreateCommand();
        command.CommandText = $"SELECT (EXTRACT (TIMEZONE FROM \"MaxValue\"))::integer FROM public.\"MinMaxDateTimeOffsetEntity\"";

        using (command)
        using (var reader = command.ExecuteReader()) {

          while (reader.Read()) {
            var timezoneInSeconds = reader.GetDouble(0);
            Console.WriteLine($"Timezone : {TimeSpan.FromSeconds(timezoneInSeconds)}");
          }
        }
      });

      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Hour == 4);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Hour == DateTimeOffset.MaxValue.AddHours(-1).Hour);
      });
    }

    [Test]
    public void ExtractMinuteTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Minute==firstDateTimeOffset.Minute);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Minute==firstMillisecondDateTimeOffset.Minute);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Minute==nullableDateTimeOffset.Minute);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Minute==wrongDateTimeOffset.Minute);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Minute==wrongMillisecondDateTimeOffset.Minute);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Minute==wrongDateTimeOffset.Minute);
      });
    }

    [Test]
    public void ExtractMinuteMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Minute == DateTimeOffset.MinValue.Minute);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Minute == DateTimeOffset.MinValue.AddMinutes(1).Minute);
      });
    }

    [Test]
    public void ExtractMinuteMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Minute == DateTimeOffset.MaxValue.Minute);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Minute == DateTimeOffset.MaxValue.AddMinutes(-1).Minute);
      });
    }

    [Test]
    public void ExtractSecondTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Second==firstDateTimeOffset.Second);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Second==firstMillisecondDateTimeOffset.Second);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Second==nullableDateTimeOffset.Second);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Second==wrongDateTimeOffset.Second);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Second==wrongMillisecondDateTimeOffset.Second);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Second==wrongDateTimeOffset.Second);
      });
    }

    [Test]
    public void ExtractSecondMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Second == DateTimeOffset.MinValue.Second);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Second == DateTimeOffset.MinValue.AddSeconds(10).Second);
      });
    }

    [Test]
    public void ExtractSecondMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Second == DateTimeOffset.MaxValue.Second);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Second == DateTimeOffset.MaxValue.AddSeconds(-10).Second);
      });
    }

    [Test]
    public void ExtractMillisecondTest()
    {
      ExecuteInsideSession((s) => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Millisecond==firstMillisecondDateTimeOffset.Millisecond);

        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Second==wrongMillisecondDateTimeOffset.Millisecond);
      });
    }

    [Test]
    public void ExtractDateTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Date==firstDateTimeOffset.Date);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Date==firstMillisecondDateTimeOffset.Date);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Date==nullableDateTimeOffset.Date);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Date==wrongDateTimeOffset.Date);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Date==wrongMillisecondDateTimeOffset.Date);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Date==wrongDateTimeOffset.Date);
      });
    }

    [Test]
    public void ExtractDateMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Date == DateTimeOffset.MinValue.Date);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.Date == DateTimeOffset.MinValue.AddDays(1).Date);
      });
    }

    [Test]
    public void ExtractDateMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        // overflow of year from 9999-12-31 to 10000-01-01 because of how postgre works with timezones
        // can't validate
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DateTime == DateTimeOffset.MaxValue.DateTime);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.Date == DateTimeOffset.MaxValue.AddDays(-1).Date);
      });
    }

    [Test]
    [TestCase("2018-10-30 12:15:32.123 +05:10")]
    [TestCase("2018-10-30 12:15:32.1234 +05:10")]
    [TestCase("2018-10-30 12:15:32.12345 +05:10")]
    [TestCase("2018-10-30 12:15:32.123456 +05:10")]
    [TestCase("2018-10-30 12:15:32.1234567 +05:10")]
    public void ExtractDateFromMicrosecondsTest(string testValueString)
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      ExecuteInsideSession((s) => {
        var testDateTimeOffset =  DateTimeOffset.Parse(testValueString);
        _ = new SingleDateTimeOffsetEntity(s) { MillisecondDateTimeOffset = testDateTimeOffset };
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Date == testDateTimeOffset.Date);
      });
    }

    [Test]
    public void ExtractTimeOfDayTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.TimeOfDay==firstDateTimeOffset.TimeOfDay);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.TimeOfDay==wrongDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractTimeOfDayMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        var minValueAdjusted = TryMoveToLocalTimeZone(DateTimeOffset.MinValue);
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.TimeOfDay == minValueAdjusted.TimeOfDay);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.TimeOfDay == DateTimeOffset.MinValue.AddMinutes(10).TimeOfDay);
      });
    }

    [Test]
    public void ExtractTimeOfDayMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        // year overflow on server side
        var adjustedMaxValue = TryMoveToLocalTimeZone(DateTimeOffset.MaxValue.AddYears(-1).AddTicks(-9));
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.TimeOfDay == adjustedMaxValue.TimeOfDay);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.TimeOfDay == DateTimeOffset.MaxValue.AddMinutes(-10).TimeOfDay);
      });
    }

    [Test]
    public void ExtractTimeOfDayWithMillisecondsTest()
    {
      ExecuteInsideSession((s) => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.TimeOfDay==firstMillisecondDateTimeOffset.TimeOfDay);

        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.TimeOfDay==wrongMillisecondDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractTimeOfDayTicksTest()
    {
      Require.ProviderIsNot(StorageProvider.PostgreSql | StorageProvider.Oracle);

      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.TimeOfDay.Ticks == firstDateTimeOffset.TimeOfDay.Ticks);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.TimeOfDay.Ticks == wrongDateTimeOffset.TimeOfDay.Ticks);
      });
    }

    [Test]
    public void ExtractTimeOfDayOfNullableValueTest()
    {
      ExecuteInsideSession((s) => {
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.TimeOfDay==nullableDateTimeOffset.TimeOfDay);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.TimeOfDay==wrongDateTimeOffset.TimeOfDay);
      });
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.DayOfYear==firstDateTimeOffset.DayOfYear);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.DayOfYear==firstMillisecondDateTimeOffset.DayOfYear);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.DayOfYear==nullableDateTimeOffset.DayOfYear);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.DayOfYear==wrongDateTimeOffset.DayOfYear);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.DayOfYear==wrongMillisecondDateTimeOffset.DayOfYear);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.DayOfYear==wrongDateTimeOffset.DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfYearMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.DayOfYear == DateTimeOffset.MinValue.DayOfYear);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.DayOfYear == DateTimeOffset.MinValue.AddDays(1).DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfYearMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DayOfYear == 1);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DayOfYear == DateTimeOffset.MaxValue.AddDays(-1).DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.DayOfWeek==firstDateTimeOffset.DayOfWeek);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.DayOfWeek==firstMillisecondDateTimeOffset.DayOfWeek);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.DayOfWeek==nullableDateTimeOffset.DayOfWeek);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.DayOfWeek==wrongDateTimeOffset.DayOfWeek);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.DayOfWeek==wrongMillisecondDateTimeOffset.DayOfWeek);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.DayOfWeek==wrongDateTimeOffset.DayOfWeek);
      });
    }

    [Test]
    public void ExtractDayOfWeekMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.DayOfWeek == DateTimeOffset.MinValue.DayOfWeek);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.DayOfWeek == DateTimeOffset.MinValue.AddDays(1).DayOfWeek);
      });
    }

    [Test]
    public void ExtractDayOfWeekMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DayOfWeek == DateTimeOffset.MaxValue.DayOfWeek + 1);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DayOfWeek == DateTimeOffset.MaxValue.AddDays(-1).DayOfWeek);
      });
    }

    [Test]
    public void ExtractDateTimeTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.DateTime==firstDateTimeOffset.DateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.DateTime==firstMillisecondDateTimeOffset.DateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.DateTime==nullableDateTimeOffset.DateTime);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.DateTime==wrongDateTimeOffset.DateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.DateTime==wrongMillisecondDateTimeOffset.DateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.DateTime==wrongDateTimeOffset.DateTime);
      });
    }

    [Test]
    public void ExtractDateTimeMinValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        var minValueAdjusted = TryMoveToLocalTimeZone(DateTimeOffset.MinValue);
        RunTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.DateTime == minValueAdjusted.DateTime);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MinValue.DateTime == DateTimeOffset.MinValue.AddDays(1).DateTime);
      });
    }

    [Test]
    public void ExtractDateTimeMaxValueTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        // overflow of year from 9999-12-31 to 10000-01-01 because of how postgre works with timezones
        // can't validate
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DateTime == DateTimeOffset.MaxValue.DateTime);
        RunWrongTest<MinMaxDateTimeOffsetEntity>(s, c => c.MaxValue.DateTime == DateTimeOffset.MaxValue.AddDays(-1).DateTime);
      });
    }

    [Test]
    public void ExtractLocalDateTimeTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.LocalDateTime==firstDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.LocalDateTime==firstMillisecondDateTimeOffset.LocalDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.LocalDateTime==nullableDateTimeOffset.LocalDateTime);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.LocalDateTime==WrongDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.LocalDateTime==wrongMillisecondDateTimeOffset.LocalDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.LocalDateTime==wrongDateTimeOffset.LocalDateTime);
      });
    }

    [Test]
    public void ExtractUtcDateTimeTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.UtcDateTime==firstDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.UtcDateTime==firstMillisecondDateTimeOffset.UtcDateTime);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.UtcDateTime==nullableDateTimeOffset.UtcDateTime);

        var wrongDateTimeOffset = TryMoveToLocalTimeZone(WrongDateTimeOffset);
        var wrongMillisecondDateTimeOffset = TryMoveToLocalTimeZone(WrongMillisecondDateTimeOffset);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.UtcDateTime==wrongDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.UtcDateTime==wrongMillisecondDateTimeOffset.UtcDateTime);
        RunWrongTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.UtcDateTime==wrongDateTimeOffset.UtcDateTime);
      });
    }

    [Test]
    public void ExtractOffsetTest()
    {
      ExecuteInsideSession((s) => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        var nullableDateTimeOffset = TryMoveToLocalTimeZone(NullableDateTimeOffset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.DateTimeOffset.Offset==firstDateTimeOffset.Offset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.MillisecondDateTimeOffset.Offset==firstMillisecondDateTimeOffset.Offset);
        RunTest<SingleDateTimeOffsetEntity>(s, c => c.NullableDateTimeOffset.Value.Offset==nullableDateTimeOffset.Offset);
      });
    }
  }
}
