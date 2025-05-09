// Copyright (C) 2023-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{
  public class DateOnlyToStringTest : DateTimeBaseTest
  {
    [Test]
    public void ToStringTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.ToString("o") == FirstDateOnly.ToString("o"));
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.ToString("o") == FirstDateOnly.AddDays(1).ToString("o"));
      });
    }

    [Test]
    public void MinMaxToStringTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      ExecuteInsideSession((s) => {
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MinValue.ToString("o") == DateOnly.MinValue.ToString("o"));
        RunTest<MinMaxDateOnlyEntity>(s, c => c.MaxValue.ToString("o") == DateOnly.MaxValue.ToString("o"));
      });
    }
  }
}
