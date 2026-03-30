// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class JoinTest : DateTimeBaseTest
  {
    [Test]
    public void TimeOnlyJoinTest()
    {
      ExecuteInsideSession((s) => JoinPrivate<TimeOnlyEntity, TimeOnlyEntity, JoinResult<TimeOnly>, TimeOnly, long>(s,
        left => left.TimeOnly,
        right => right.TimeOnly,
        (left, right) => new JoinResult<TimeOnly> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.TimeOnly, RightDateTime = right.TimeOnly },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void MillisecondTimeOnlyJoinTest()
    {
      ExecuteInsideSession((s) => JoinPrivate<TimeOnlyEntity, TimeOnlyEntity, JoinResult<TimeOnly>, TimeOnly, long>(s,
        left => left.MillisecondTimeOnly,
        right => right.MillisecondTimeOnly,
        (left, right) => new JoinResult<TimeOnly> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.MillisecondTimeOnly, RightDateTime = right.MillisecondTimeOnly },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void NullableTimeOnlyJoinTest()
    {
      ExecuteInsideSession((s) => JoinPrivate<TimeOnlyEntity, TimeOnlyEntity, JoinResult<TimeOnly?>, TimeOnly?, long>(s,
        left => left.NullableTimeOnly,
        right => right.NullableTimeOnly,
        (left, right) => new JoinResult<TimeOnly?> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.NullableTimeOnly, RightDateTime = right.NullableTimeOnly },
        c => c.LeftId,
        c => c.RightId));
    }

    private static void JoinPrivate<T1, T2, T3, TK1, TK3>(Session session,
      Expression<Func<T1, TK1>> leftJoinExpression, Expression<Func<T2, TK1>> rightJoinExpression,
      Expression<Func<T1, T2, T3>> joinResultExpression, Expression<Func<T3, TK3>> orderByExpression, Expression<Func<T3, TK3>> thenByExpression)
      where T1 : Entity
      where T2 : Entity
    {
      var compiledLeftJoinExpression = leftJoinExpression.Compile();
      var compiledRightJoinExpression = rightJoinExpression.Compile();
      var compiledJoinResultExpression = joinResultExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();
      var compiledThenByExpression = thenByExpression.Compile();
      var joinLocal = session.Query.All<T1>().ToArray()
        .Join(session.Query.All<T2>().ToArray(), compiledLeftJoinExpression, compiledRightJoinExpression, compiledJoinResultExpression)
        .OrderBy(compiledOrderByExpression)
        .ThenBy(compiledThenByExpression);

      var joinServer = session.Query.All<T1>()
        .Join(session.Query.All<T2>(), leftJoinExpression, rightJoinExpression, joinResultExpression)
        .OrderBy(orderByExpression)
        .ThenBy(thenByExpression);

      Assert.IsTrue(joinLocal.SequenceEqual(joinServer));

      joinServer = session.Query.All<T1>()
        .Join(session.Query.All<T2>(), leftJoinExpression, rightJoinExpression, joinResultExpression)
        .OrderByDescending(orderByExpression)
        .ThenBy(thenByExpression);
      Assert.IsFalse(joinLocal.SequenceEqual(joinServer));
    }
  }
}