// Copyright (C) 2016-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class MinMaxTest : DateTimeBaseTest
  {
    [Test]
    public void DateTimeMinMaxTest()
    {
      ExecuteInsideSession((s) => MinMaxPrivate<DateTimeEntity, DateTime>(s, c => c.DateTime));
    }

    [Test]
    public void MillisecondDateTimeMinMaxTest()
    {
      ExecuteInsideSession((s) => MinMaxPrivate<MillisecondDateTimeEntity, DateTime>(s, c => c.DateTime));
    }

    [Test]
    public void NullableDateTimeMinMaxTest()
    {
      ExecuteInsideSession((s) => MinMaxPrivate<NullableDateTimeEntity, DateTime?>(s, c => c.DateTime));
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