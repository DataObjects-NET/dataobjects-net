// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class OperationsTest : DateTimeBaseTest
  {
    [Test]
    public void AddHoursTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddHours(1) == FirstTimeOnly.AddHours(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddHours(33) == NullableTimeOnly.AddHours(33));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddHours(1) == FirstTimeOnly.AddHours(2));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddHours(33) == NullableTimeOnly.AddHours(44));

        if(StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.MySql)
          || StorageProviderInfo.Instance.CheckProviderVersionIsAtLeast(new Version(5, 6))) {
          RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddHours(-2) == FirstMillisecondTimeOnly.AddHours(-2));
          RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddHours(-1) == FirstMillisecondTimeOnly.AddHours(-2));
        }
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddMinutes(1) == FirstTimeOnly.AddMinutes(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddMinutes(33) == NullableTimeOnly.AddMinutes(33));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddMinutes(1) == FirstTimeOnly.AddMinutes(2));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddMinutes(33) == NullableTimeOnly.AddMinutes(44));

        if (StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.MySql)
          || StorageProviderInfo.Instance.CheckProviderVersionIsAtLeast(new Version(5, 6))) {
          RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddMinutes(-2) == FirstMillisecondTimeOnly.AddMinutes(-2));
          RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddMinutes(-1) == FirstMillisecondTimeOnly.AddMinutes(-2));
        }
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Add(SecondOffset) == FirstMillisecondTimeOnly.Add(SecondOffset));
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Add(FirstOffset) == FirstTimeOnly.Add(FirstOffset));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Add(FirstOffset) == NullableTimeOnly.Add(FirstOffset));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Add(FirstOffset) == FirstTimeOnly.Add(WrongOffset));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Add(FirstOffset) == NullableTimeOnly.Add(WrongOffset));

        if (StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.MySql)
          || StorageProviderInfo.Instance.CheckProviderVersionIsAtLeast(new Version(5, 6))) {
          RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Add(SecondOffset) == FirstMillisecondTimeOnly.Add(SecondOffset));
          RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Add(SecondOffset) == FirstMillisecondTimeOnly.Add(WrongOffset));
        }
      });
    }

    [Test]
    public void MinusTimeOnlyTest()
    {
      var inteval = FirstTimeOnly - SecondTimeOnly;

      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - SecondTimeOnly == FirstTimeOnly - SecondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - SecondTimeOnly == NullableTimeOnly - SecondTimeOnly);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - SecondTimeOnly == FirstTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - SecondTimeOnly == NullableTimeOnly - WrongTimeOnly);

        if (StorageProviderInfo.Instance.CheckProviderIsNot(StorageProvider.MySql)
          || StorageProviderInfo.Instance.CheckProviderVersionIsAtLeast(new Version(5, 6))) {
          RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - SecondTimeOnly == FirstMillisecondTimeOnly - SecondTimeOnly);
          RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - SecondTimeOnly == FirstMillisecondTimeOnly - WrongTimeOnly);
        }
      });
    }

    [Test]
    public void MysqlMinisTimeOnlyTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        var firstTimeOnly = FirstTimeOnly.AdjustTimeOnlyForCurrentProvider();
        var firstMillisecondTimeOnly = FirstMillisecondTimeOnly.AdjustTimeOnlyForCurrentProvider();
        var secondTimeOnly = SecondTimeOnly.AdjustTimeOnlyForCurrentProvider();
        var nullableTimeOnly = NullableTimeOnly.AdjustTimeOnlyForCurrentProvider();

        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - secondTimeOnly == firstTimeOnly - secondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - secondTimeOnly == firstMillisecondTimeOnly - secondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - secondTimeOnly == NullableTimeOnly - secondTimeOnly);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - secondTimeOnly == secondTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - secondTimeOnly == firstMillisecondTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - secondTimeOnly == nullableTimeOnly - WrongTimeOnly);
      });
    }
  }
}