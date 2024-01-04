// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{
  public class GroupByTest : DateTimeBaseTest
  {
    [Test]
    public void DateOnlyGroupByTest() =>
      ExecuteInsideSession((s) => GroupByPrivate<DateOnlyEntity, DateOnly, long>(s, c => c.DateOnly, c => c.Id));

    [Test]
    public void NullableDateOnlyGroupByTest() =>
      ExecuteInsideSession((s) => GroupByPrivate<DateOnlyEntity, DateOnly?, long>(s, c => c.NullableDateOnly, c => c.Id));

    private void GroupByPrivate<T, TK1, TK2>(Session session, Expression<Func<T, TK1>> groupByExpression, Expression<Func<T, TK2>> orderByExpression)
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