// Copyright (C) 2023-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{
  public class PartsExtractionTest : DateTimeBaseTest
  {
    [Test]
    public void ExtractYearTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Year == FirstDateOnly.Year);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Year == NullableDateOnly.Year);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Year == WrongDateOnly.Year);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Year == WrongDateOnly.Year);
      });
    }


    [Test]
    public void MinMaxExtractYearTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.Year == DateTime.MinValue.Year);
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.Year == DateTime.MaxValue.Year);

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.Year == WrongDateOnly.Year);
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.Year == WrongDateOnly.Year);
      });
    }

    [Test]
    public void ExtractMonthTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Month == FirstDateOnly.Month);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Month == NullableDateOnly.Month);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Month == WrongDateOnly.Month);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Month == WrongDateOnly.Month);

        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Month == FirstDateOnly.Month);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Month == NullableDateOnly.Month);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Month == WrongDateOnly.Month);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Month == WrongDateOnly.Month);

      });
    }

    [Test]
    public void MinMaxExtractMonthTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.Month == DateTime.MinValue.Month);
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.Month == DateTime.MaxValue.Month);

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.Month == WrongDateOnly.Month);
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.Month == WrongDateOnly.Month);
      });
    }

    [Test]
    public void ExtractDayTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Day == FirstDateOnly.Day);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Day == NullableDateOnly.Day);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.Day == WrongDateOnly.Day);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.Day == WrongDateOnly.Day);
      });
    }

    [Test]
    public void MinMaxExtractDayTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.Day == DateTime.MinValue.Day);
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.Day == DateTime.MaxValue.Day);

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.Day == WrongDateOnly.Day);
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.Day == WrongDateOnly.Day);
      });
    }

    [Test]
    public void ExtractDayOfYearTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.DayOfYear == FirstDateOnly.DayOfYear);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.DayOfYear == NullableDateOnly.DayOfYear);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.DayOfYear == WrongDateOnly.DayOfYear);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.DayOfYear == WrongDateOnly.DayOfYear);
      });
    }

    [Test]
    public void MinMaxExtractDayOfYearTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.DayOfYear == DateTime.MinValue.DayOfYear);
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.DayOfYear == DateTime.MaxValue.DayOfYear);

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.DayOfYear == WrongDateOnly.DayOfYear);
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.DayOfYear == WrongDateOnly.DayOfYear);
      });
    }

    [Test]
    public void ExtractDayOfWeekTest()
    {

      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.DayOfWeek == FirstDateOnly.DayOfWeek);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.DayOfWeek == NullableDateOnly.DayOfWeek);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.DayOfWeek == WrongDateOnly.DayOfWeek);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.DayOfWeek == WrongDateOnly.DayOfWeek);
      });
    }

    [Test]
    public void MinMaxExtractDayOfWeekTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.DayOfWeek == DateTime.MinValue.DayOfWeek);
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.DayOfWeek == DateTime.MaxValue.DayOfWeek);

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.DayOfWeek == DateTime.MinValue.AddDays(1).DayOfWeek);
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.DayOfWeek == DateTime.MaxValue.AddDays(-1).DayOfWeek);
      });
    }
  }
}