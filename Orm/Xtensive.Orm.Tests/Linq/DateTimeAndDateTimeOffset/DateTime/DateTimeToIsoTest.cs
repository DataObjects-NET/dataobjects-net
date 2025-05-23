// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class DateTimeToIsoTest : DateTimeBaseTest
  {
    [Test]
    public void ToIsoStringTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateTimeEntity>(s, c => c.DateTime.ToString("s") == FirstDateTime.ToString("s"));
        RunWrongTest<SingleDateTimeEntity>(s, c => c.DateTime.ToString("s") == FirstDateTime.AddMinutes(1).ToString("s"));
      });
    }


    [Test]
    public void MinMaxValuesToIsoStringTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateTimeEntity>(s, c => c.MinValue.ToString("s") == DateTime.MinValue.ToString("s"));
        RunTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.ToString("s") == DateTime.MaxValue.ToString("s"));

        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MinValue.ToString("s") == FirstDateTime.AddMinutes(1).ToString("s"));
        RunWrongTest<MinMaxDateTimeEntity>(s, c => c.MaxValue.ToString("s") == FirstDateTime.AddMinutes(1).ToString("s"));
      });
    }
  }
}
