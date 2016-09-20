// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class GroupByTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void DateTimeOffsetGroupByTest()
    {
      ExecuteInsideSession(() => GroupByPrivate<DateTimeOffsetEntity, DateTimeOffset, long>(c => c.DateTimeOffset, c => c.Id));
    }

    [Test(Description = "Might be failed on SQLite because of certain restrictions of work with milliseconds")]
    public void MillisecondDateTimeOffsetGroupByTest()
    {
      ExecuteInsideSession(() => GroupByPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset, long>(c => c.DateTimeOffset, c => c.Id));
    }

    [Test]
    public void NullableDateTimeOffsetGroupByTest()
    {
      ExecuteInsideSession(() => GroupByPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?, long>(c => c.DateTimeOffset, c => c.Id));
    }

    private void GroupByPrivate<T, TK1, TK2>(Expression<Func<T, TK1>> groupByExpression, Expression<Func<T, TK2>> orderByExpression)
      where T : Entity
    {
      var compiledGroupByExpression = groupByExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();
      var groupByLocal = Query.All<T>().ToArray().GroupBy(compiledGroupByExpression).ToArray();
      var groupByServer = Query.All<T>().GroupBy(groupByExpression);
      foreach (var group in groupByServer) {
        Assert.Contains(group, groupByLocal);
        var localGroup = groupByLocal.Single(c => c.Key.Equals(group.Key));
        Assert.IsTrue(group.OrderBy(compiledOrderByExpression).SequenceEqual(localGroup.OrderBy(compiledOrderByExpression)));
      }
    }
  }
}
