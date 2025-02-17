// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{
  public class OperationsTest : DateTimeBaseTest
  {
    [Test]
    public void AddYearsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddYears(1) == FirstDateOnly.AddYears(1));
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddYears(33) == NullableDateOnly.AddYears(33));

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddYears(1) == FirstDateOnly.AddYears(2));
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddYears(33) == NullableDateOnly.AddYears(44));
      });
    }

    [Test]
    public void AddYearsToMinMaxValuesTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.AddYears(1) == DateOnly.MinValue.AddYears(1));
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.AddYears(-33) == DateOnly.MaxValue.AddYears(-33));

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.AddYears(1) == DateOnly.MinValue.AddYears(2));
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.AddYears(-33) == DateOnly.MaxValue.AddYears(-34));
      });
    }

    [Test]
    public void AddMonthsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddMonths(1) == FirstDateOnly.AddMonths(1));
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddMonths(33) == NullableDateOnly.AddMonths(33));

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddMonths(1) == FirstDateOnly.AddMonths(2));
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddMonths(33) == NullableDateOnly.AddMonths(44));
      });
    }

    [Test]
    public void AddMonthsToMinMaxValues()
    {
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.AddMonths(1) == DateOnly.MinValue.AddMonths(1));
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.AddMonths(-33) == DateOnly.MaxValue.AddMonths(-33));

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.AddMonths(1) == DateOnly.MinValue.AddMonths(2));
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.AddMonths(-33) == DateOnly.MaxValue.AddMonths(-34));
      });
    }

    [Test]
    public void AddDaysTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddDays(1) == FirstDateOnly.AddDays(1));
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddDays(33) == NullableDateOnly.AddDays(33));

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddDays(1) == FirstDateOnly.AddDays(2));
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddDays(33) == NullableDateOnly.AddDays(44));
      });
    }

    [Test]
    public void AddDaysToMinMaxValues()
    {
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.AddDays(1) == DateOnly.MinValue.AddDays(1));
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.AddDays(-33) == DateOnly.MaxValue.AddDays(-33));

        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.AddDays(1) == DateOnly.MinValue.AddDays(2));
        RunWrongTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.AddDays(-33) == DateOnly.MaxValue.AddDays(-34));
      });
    }
  }
}