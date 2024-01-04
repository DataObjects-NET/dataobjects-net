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
  public class JoinTest : DateTimeBaseTest
  {
    [Test]
    public void DateOnlyJoinTest()
    {
      ExecuteInsideSession((s) => JoinPrivate<DateOnlyEntity, DateOnlyEntity, JoinResult<DateOnly>, DateOnly, long>(s,
        left => left.DateOnly,
        right => right.DateOnly,
        (left, right) => new JoinResult<DateOnly> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.DateOnly, RightDateTime = right.DateOnly },
        c => c.LeftId,
        c => c.RightId));
    }

    [Test]
    public void NullableDateOnlyJoinTest()
    {
      ExecuteInsideSession((s) => JoinPrivate<DateOnlyEntity, DateOnlyEntity, JoinResult<DateOnly?>, DateOnly?, long>(s,
        left => left.NullableDateOnly,
        right => right.NullableDateOnly,
        (left, right) => new JoinResult<DateOnly?> { LeftId = left.Id, RightId = right.Id, LeftDateTime = left.NullableDateOnly, RightDateTime = right.NullableDateOnly },
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