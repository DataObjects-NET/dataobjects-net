// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class ComparisonTestTimeOnly : DateTimeBaseTest
  {
    [Test]
    public void EqualsTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly == FirstTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly == FirstMillisecondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly == NullableTimeOnly);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly == WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly == WrongMillisecondTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly == WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly == null);
      });
    }

    [Test]
    public void NotEqualTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly != FirstTimeOnly.AddHours(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly != FirstMillisecondTimeOnly.AddHours(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly != NullableTimeOnly.AddHours(1));
      });
    }

    [Test]
    public void CompareTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly > FirstTimeOnly.AddHours(-1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly > FirstMillisecondTimeOnly.Add(new TimeSpan(0,0,0,0, -1)));

        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly < FirstTimeOnly.AddHours(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly < FirstMillisecondTimeOnly.Add(new TimeSpan(0, 0, 0, 0, 1)));

        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime > FirstDateTime);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime > FirstMillisecondDateTime);
        RunWrongTest<SingleDateTimeEntity>(s, c => c.MillisecondDateTime < FirstMillisecondDateTime.Date);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly > FirstTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly > FirstMillisecondTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly < FirstMillisecondTimeOnly.AddMinutes(-3));
      });
    }

    [Test]
    public void CompareMysqTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly > FirstTimeOnly.AddMinutes(-1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly < FirstTimeOnly.AddMinutes(1));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly > FirstTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly > FirstMillisecondTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly < FirstMillisecondTimeOnly.AddMinutes(-3));
      });
    }
  }
}