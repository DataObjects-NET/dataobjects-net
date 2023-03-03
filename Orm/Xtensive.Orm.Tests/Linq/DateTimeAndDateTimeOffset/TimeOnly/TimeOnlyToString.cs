// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class TimeOnlyToStringTest : DateTimeBaseTest
  {
    [Test]
    public void ToStringTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.ToString("o") == FirstTimeOnly.ToString("o"));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.ToString("o") == FirstTimeOnly.AddHours(1).ToString("o"));
      });
    }
  }
}
#endif