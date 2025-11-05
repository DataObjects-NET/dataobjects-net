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
  public class GroupByTest : DateTimeBaseTest
  {
    [Test]
    public void DateTimeGroupByTest() =>
      ExecuteInsideSession((s) => GroupByPrivate<DateTimeEntity, DateTime, long>(s, c => c.DateTime, c => c.Id));

    [Test]
    public void MillisecondDateTimeGroupByTest() =>
      ExecuteInsideSession((s) => GroupByPrivate<MillisecondDateTimeEntity, DateTime, long>(s, c => c.DateTime, c => c.Id));

    [Test]
    public void NullableDateTimeGroupByTest() =>
      ExecuteInsideSession((s) => GroupByPrivate<NullableDateTimeEntity, DateTime?, long>(s, c => c.DateTime, c => c.Id));

    private static void GroupByPrivate<T, TK1, TK2>(Session session, Expression<Func<T, TK1>> groupByExpression, Expression<Func<T, TK2>> orderByExpression)
      where T : Entity
    {
      var compiledGroupByExpression = groupByExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();
      var groupByLocal = session.Query.All<T>().ToArray().GroupBy(compiledGroupByExpression).ToArray();
      var groupByServer = session.Query.All<T>().GroupBy(groupByExpression);
      foreach (var group in groupByServer) {
        Assert.Contains(group, groupByLocal);
        var localGroup = groupByLocal.Single(c => c.Key.Equals(group.Key));
        Assert.IsTrue(group.OrderBy(compiledOrderByExpression).SequenceEqual(localGroup.OrderBy(compiledOrderByExpression)));
      }
    }
  }
}