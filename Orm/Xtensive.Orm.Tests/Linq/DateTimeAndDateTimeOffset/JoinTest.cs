// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class JoinTest : BaseDateTimeAndDateTimeOffsetTest
  {
    private class JoinResult<T>
    {
      public long LeftId { get; set; }
      public long RightId { get; set; }
      public T LeftDateTime { get; set; }
      public T RightDateTime { get; set; }

      public override bool Equals(object obj)
      {
        var equalTo = obj as JoinResult<T>;
        if (equalTo==null)
          return false;
        return LeftId==equalTo.LeftId && RightId==equalTo.RightId && LeftDateTime.Equals(equalTo.LeftDateTime) && RightDateTime.Equals(equalTo.RightDateTime);
      }
    }

    [Test]
    public void DateTimeJoinTest()
    {
      OpenSessionAndAction(() => JoinPrivate<DateTimeEntity, DateTimeEntity, JoinResult<DateTime>, DateTime, long>(
        left => left.DateTime,
        right => right.DateTime,
        (left, right) => new JoinResult<DateTime> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTime, RightDateTime = right.DateTime },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void MillisecondDateTimeJoinTest()
    {
      OpenSessionAndAction(() => JoinPrivate<MillisecondDateTimeEntity, MillisecondDateTimeEntity, JoinResult<DateTime>, DateTime, long>(
        left => left.DateTime,
        right => right.DateTime,
        (left, right) => new JoinResult<DateTime> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTime, RightDateTime = right.DateTime },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void NullableDateTimeJoinTest()
    {
      OpenSessionAndAction(() => JoinPrivate<NullableDateTimeEntity, NullableDateTimeEntity, JoinResult<DateTime?>, DateTime?, long>(
        left => left.DateTime,
        right => right.DateTime,
        (left, right) => new JoinResult<DateTime?> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTime, RightDateTime = right.DateTime },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void DateTimeOffsetJoinTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => JoinPrivate<DateTimeOffsetEntity, DateTimeOffsetEntity, JoinResult<DateTimeOffset>, DateTimeOffset, long>(
        left => left.DateTimeOffset,
        right => right.DateTimeOffset,
        (left, right) => new JoinResult<DateTimeOffset> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTimeOffset, RightDateTime = right.DateTimeOffset },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test(Description = "Might be failed on SQLite because of certain restrictions of work with milliseconds")]
    public void MillisecondDateTimeOffsetJoinTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => JoinPrivate<MillisecondDateTimeOffsetEntity, MillisecondDateTimeOffsetEntity, JoinResult<DateTimeOffset>, DateTimeOffset, long>(
        left => left.DateTimeOffset,
        right => right.DateTimeOffset,
        (left, right) => new JoinResult<DateTimeOffset> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateTimeOffset, RightDateTime = right.DateTimeOffset },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void NullableDateTimeOffsetJoinTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => JoinPrivate<NullableDateTimeOffsetEntity, NullableDateTimeOffsetEntity, JoinResult<DateTimeOffset?>, DateTimeOffset?, long>(
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
