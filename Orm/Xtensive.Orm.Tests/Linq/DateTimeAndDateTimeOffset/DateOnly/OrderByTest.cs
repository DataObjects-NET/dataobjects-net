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
  public class OrderByTest : DateTimeBaseTest
  {
    [Test]
    public void DateOnlyOrderByTest()
    {
      ExecuteInsideSession((s) => {
        OrderByPrivate<DateOnlyEntity, DateOnly, long>(s, c => c.DateOnly, c => c.Id);
        OrderByPrivate<DateOnlyEntity, DateOnly, DateOnly>(s, c => c.DateOnly, c => c);
      });
    }

    [Test]
    public void NullableDateOnlyOrderByTest()
    {
      ExecuteInsideSession((s) => {
        OrderByPrivate<DateOnlyEntity, DateOnly?, long>(s, c => c.NullableDateOnly, c => c.Id);
        OrderByPrivate<DateOnlyEntity, DateOnly?, DateOnly?>(s, c => c.NullableDateOnly, c => c);
      });
    }

    private static void OrderByPrivate<T, TK1, TK2>(Session session, Expression<Func<T, TK1>> orderByExpression, Expression<Func<T, TK2>> thenByExpression)
      where T : Entity
    {
      var compiledOrderByExpression = orderByExpression.Compile();
      var compiledThenByExpression = thenByExpression.Compile();
      var notOrderedLocal = session.Query.All<T>().ToArray();
      var orderedLocal = notOrderedLocal.OrderBy(compiledOrderByExpression).ThenBy(compiledThenByExpression);
      var orderedLocalDescending = notOrderedLocal.OrderByDescending(compiledOrderByExpression).ThenBy(compiledThenByExpression);
      var orderedByServer = session.Query.All<T>().OrderBy(orderByExpression).ThenBy(thenByExpression);
      var orderedByServerDescending = session.Query.All<T>().OrderByDescending(orderByExpression).ThenBy(thenByExpression);

      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedLocal));
      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocalDescending.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocal.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocalDescending.SequenceEqual(orderedByServer));
    }

    protected static void OrderByPrivate<T1, T2, T3>(Session session, Expression<Func<T1, T2>> selectorExpression, Expression<Func<T2, T3>> orderByExpression)
      where T1 : Entity
    {
      var compiledOrderByExpression = orderByExpression.Compile();

      var notOrderedLocal = session.Query.All<T1>().Select(selectorExpression).ToArray();
      var orderedLocal = notOrderedLocal.OrderBy(compiledOrderByExpression);
      var orderedLocalDescending = notOrderedLocal.OrderByDescending(compiledOrderByExpression);
      var orderedByServer = session.Query.All<T1>().Select(selectorExpression).OrderBy(orderByExpression);
      var orderedByServerDescending = session.Query.All<T1>().Select(selectorExpression).OrderByDescending(orderByExpression);

      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedLocal));
      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocalDescending.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocal.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocalDescending.SequenceEqual(orderedByServer));
    }
  }
}