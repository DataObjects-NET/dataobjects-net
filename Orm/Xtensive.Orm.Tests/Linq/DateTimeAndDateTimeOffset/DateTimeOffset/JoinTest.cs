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
  public class JoinTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void DateTimeOffsetJoinTest()
    {
      ExecuteInsideSession(() => JoinPrivate<DateTimeOffsetEntity, DateTimeOffsetEntity, JoinResult<DateTimeOffset>, DateTimeOffset, long>(
        left => left.DateTimeOffset,
        right => right.DateTimeOffset,
        (left, right) => new JoinResult<DateTimeOffset> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTimeOffset, RightDateTime = right.DateTimeOffset },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test(Description = "Might be failed on SQLite because of certain restrictions of work with milliseconds")]
    public void MillisecondDateTimeOffsetJoinTest()
    {
      ExecuteInsideSession(() => JoinPrivate<MillisecondDateTimeOffsetEntity, MillisecondDateTimeOffsetEntity, JoinResult<DateTimeOffset>, DateTimeOffset, long>(
        left => left.DateTimeOffset,
        right => right.DateTimeOffset,
        (left, right) => new JoinResult<DateTimeOffset> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTimeOffset, RightDateTime = right.DateTimeOffset },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void NullableDateTimeOffsetJoinTest()
    {
      ExecuteInsideSession(() => JoinPrivate<NullableDateTimeOffsetEntity, NullableDateTimeOffsetEntity, JoinResult<DateTimeOffset?>, DateTimeOffset?, long>(
        left => left.DateTimeOffset,
        right => right.DateTimeOffset,
        (left, right) => new JoinResult<DateTimeOffset?> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTimeOffset, RightDateTime = right.DateTimeOffset },
        c => c.LeftId,
        c => c.RightId));
    }

    private void JoinPrivate<T1, T2, T3, TK1, TK3>(Expression<Func<T1, TK1>> leftJoinExpression, Expression<Func<T2, TK1>> rightJoinExpression,
      Expression<Func<T1, T2, T3>> joinResultExpression, Expression<Func<T3, TK3>> orderByExpression, Expression<Func<T3, TK3>> thenByExpression)
      where T1 : Entity
      where T2 : Entity
    {
      var compiledLeftJoinExpression = leftJoinExpression.Compile();
      var compiledRightJoinExpression = rightJoinExpression.Compile();
      var compiledJoinResultExpression = joinResultExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();
      var compiledThenByExpression = thenByExpression.Compile();
      var joinLocal = Query.All<T1>().ToArray()
        .Join(Query.All<T2>().ToArray(), compiledLeftJoinExpression, compiledRightJoinExpression, compiledJoinResultExpression)
        .OrderBy(compiledOrderByExpression)
        .ThenBy(compiledThenByExpression);

      var joinServer = Query.All<T1>()
        .Join(Query.All<T2>(), leftJoinExpression, rightJoinExpression, joinResultExpression)
        .OrderBy(orderByExpression)
        .ThenBy(thenByExpression);

      Assert.IsTrue(joinLocal.SequenceEqual(joinServer));

      joinServer = Query.All<T1>()
        .Join(Query.All<T2>(), leftJoinExpression, rightJoinExpression, joinResultExpression)
        .OrderByDescending(orderByExpression)
        .ThenBy(thenByExpression);
      Assert.IsFalse(joinLocal.SequenceEqual(joinServer));
    }
  }
}