// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class PartsExtractionTest : DateTimeBaseTest
  {
    [Test]
    public void ExtractHourTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Hour == FirstTimeOnly.Hour);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Hour == FirstMillisecondTimeOnly.Hour);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Hour == NullableTimeOnly.Hour);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Hour == WrongTimeOnly.Hour);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Hour == WrongMillisecondTimeOnly.Hour);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Hour == WrongTimeOnly.Hour);
      });
    }

    [Test]
    public void ExtractMinuteTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Minute == FirstTimeOnly.Minute);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Minute == FirstMillisecondTimeOnly.Minute);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Minute == NullableTimeOnly.Minute);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Minute == WrongTimeOnly.Minute);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Minute == WrongMillisecondTimeOnly.Minute);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Minute == WrongTimeOnly.Minute);
      });
    }

    [Test]
    public void ExtractSecondTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Second == FirstTimeOnly.Second);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Second == FirstMillisecondTimeOnly.Second);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Second == NullableTimeOnly.Second);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Second == WrongTimeOnly.Second);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Second == WrongMillisecondTimeOnly.Second);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Second == WrongTimeOnly.Second);
      });
    }

    [Test]
    public void ExtractMillisecondTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Millisecond == FirstMillisecondTimeOnly.Millisecond);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Second == WrongMillisecondTimeOnly.Millisecond);
      });
    }

    [Test]
    public void MysqlExtractMillisecondTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      Require.ProviderVersionAtLeast(new Version(5, 6));// no support for fractions below 5.6
      ExecuteInsideSession((s) => {
        var firstMillisecondTimeOnly = FirstMillisecondTimeOnly.AdjustTimeOnlyForCurrentProvider();
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Millisecond == firstMillisecondTimeOnly.Millisecond);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Second == WrongMillisecondTimeOnly.Millisecond);
      });
    }

    [Test]
    public void ExtractTicksTest()
    {
      ExecuteInsideSession((s) => {
        var firstMillisecondTimeOnly = FirstMillisecondTimeOnly.AdjustTimeOnlyForCurrentProvider();
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Ticks == firstMillisecondTimeOnly.Ticks);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Ticks < FirstTimeOnly.Ticks);
      });
    }
  }
}