// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class MinMaxTest : DateTimeBaseTest
  {
    [Test]
    public void TimeOnlyMinMaxTest()
    {
      ExecuteInsideSession((s) => MinMaxPrivate<TimeOnlyEntity, TimeOnly>(s, c => c.TimeOnly));
    }

    [Test]
    public void MillisecondTimeOnlyMinMaxTest()
    {
      ExecuteInsideSession((s) => MinMaxPrivate<TimeOnlyEntity, TimeOnly>(s, c => c.MillisecondTimeOnly));
    }

    [Test]
    public void NullableTimeOnlyMinMaxTest()
    {
      ExecuteInsideSession((s) => MinMaxPrivate<TimeOnlyEntity, TimeOnly?>(s, c => c.NullableTimeOnly));
    }

    private static void MinMaxPrivate<T, TK>(Session session, Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var minLocal = session.Query.All<T>().ToArray().Min(compiledSelectExpression);
      var maxLocal = session.Query.All<T>().ToArray().Max(compiledSelectExpression);
      var minServer = session.Query.All<T>().Min(selectExpression);
      var maxServer = session.Query.All<T>().Max(selectExpression);

      Assert.AreEqual(minLocal, minServer);
      Assert.AreEqual(maxLocal, maxServer);
      Assert.AreNotEqual(minLocal, maxServer);
      Assert.AreNotEqual(maxLocal, minServer);
    }
  }
}
#endif