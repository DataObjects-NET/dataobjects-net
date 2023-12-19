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
    public void AddDaysTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddDays(1) == FirstDateOnly.AddDays(1));
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddDays(33) == NullableDateOnly.AddDays(33));

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.AddDays(1) == FirstDateOnly.AddDays(2));
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly.Value.AddDays(33) == NullableDateOnly.AddDays(44));
      });
    }
  }
}