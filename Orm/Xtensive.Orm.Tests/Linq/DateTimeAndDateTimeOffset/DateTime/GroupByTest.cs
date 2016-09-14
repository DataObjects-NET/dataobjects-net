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
    public void DateTimeGroupByTest()
    {
      ExecuteInsideSession(() => GroupByPrivate<DateTimeEntity, DateTime, long>(c => c.DateTime, c => c.Id));
    }

    [Test]
    public void MillisecondDateTimeGroupByTest()
    {
      ExecuteInsideSession(() => GroupByPrivate<MillisecondDateTimeEntity, DateTime, long>(c => c.DateTime, c => c.Id));
    }

    [Test]
    public void NullableDateTimeGroupByTest()
    {
      ExecuteInsideSession(() => GroupByPrivate<NullableDateTimeEntity, DateTime?, long>(c => c.DateTime, c => c.Id));
    }

    private void GroupByPrivate<T, TK1, TK2>(Expression<Func<T, TK1>> groupByExpression, Expression<Func<T, TK2>> orderByExpression)
      where T : Entity
    {
      var compiledGroupByExpression = groupByExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();
      var groupByLocal = Query.All<T>().ToArray().GroupBy(compiledGroupByExpression).ToArray();
      var groupByServer = Query.All<T>().GroupBy(groupByExpression);
      foreach (var group in groupByServer)
      {
        Assert.Contains(group, groupByLocal);
        var localGroup = groupByLocal.Single(c => c.Key.Equals(group.Key));
        Assert.IsTrue(group.OrderBy(compiledOrderByExpression).SequenceEqual(localGroup.OrderBy(compiledOrderByExpression)));
      }
    }
  }
}