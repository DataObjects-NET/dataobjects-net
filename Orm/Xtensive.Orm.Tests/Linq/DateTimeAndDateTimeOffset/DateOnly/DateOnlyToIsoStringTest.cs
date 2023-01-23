// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER //DO_DATEONLY

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{
  public class DateOnlyToIsoStringTest : DateTimeBaseTest
  {
    [Test]
    public void ToIsoStringTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleDateOnlyEntity>(s, c => c.DateOnly.ToString("s") == FirstDateOnly.ToString("s"));
        RunWrongTest<SingleDateOnlyEntity>(s, c => c.DateOnly.ToString("s") == FirstDateOnly.AddDays(1).ToString("s"));
      });
    }
  }
}
#endif
