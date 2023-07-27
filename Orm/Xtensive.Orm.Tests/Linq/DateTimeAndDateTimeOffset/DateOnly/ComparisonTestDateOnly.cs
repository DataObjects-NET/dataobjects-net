// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER

using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{

  public class ComparisonTestDateOnly : DateTimeBaseTest
  {
    [Test]
    public void EqualsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly == FirstDateOnly);
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly == NullableDateOnly);

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly == WrongDateOnly);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly == WrongDateOnly);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly == null);
      });
    }

    [Test]
    public void NotEqualTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly != FirstDateOnly.AddYears(1));
        RunTest<SingleDateOnlyEntity>(s, c => c.NullableDateOnly != NullableDateOnly.AddYears(1));
      });
    }

    [Test]
    public void CompareTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly > FirstDateOnly.AddDays(-1));
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly < FirstDateOnly.AddDays(1));

        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly > FirstDateOnly);
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly < FirstDateOnly);
      });
    }
  }
}
#endif