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

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class OrderByTest : DateTimeBaseTest
  {
    [Test]
    public void DateTimeOrderByTest()
    {
      ExecuteInsideSession(() => {
        OrderByPrivate<DateTimeEntity, DateTime, long>(c => c.DateTime, c => c.Id);
        OrderByPrivate<DateTimeEntity, DateTime, DateTime>(c => c.DateTime, c => c);
#if DO_DATEONLY
        OrderByPrivate<DateTimeEntity, DateOnly, long>(c => c.DateOnly, c => c.Id);
        OrderByPrivate<DateTimeEntity, DateOnly, DateOnly>(c => c.DateOnly, c => c);
        OrderByPrivate<DateTimeEntity, TimeOnly, long>(c => c.TimeOnly, c => c.Id );
#endif
      });
    }

    [Test]
    public void MillisecondDateTimeOrderByTest()
    {
      ExecuteInsideSession(() => {
        OrderByPrivate<MillisecondDateTimeEntity, DateTime, long>(c => c.DateTime, c => c.Id);
        OrderByPrivate<MillisecondDateTimeEntity, DateTime, DateTime>(c => c.DateTime, c => c);
      });
    }

    [Test]
    public void NullableDateTimeOrderByTest()
    {
      ExecuteInsideSession(() => {
        OrderByPrivate<NullableDateTimeEntity, DateTime?, long>(c => c.DateTime, c => c.Id);
        OrderByPrivate<NullableDateTimeEntity, DateTime?, DateTime?>(c => c.DateTime, c => c);
      });
    }

    private void OrderByPrivate<T, TK1, TK2>(Expression<Func<T, TK1>> orderByExpression, Expression<Func<T, TK2>> thenByExpression)
      where T : Entity
    {
      var compiledOrderByExpression = orderByExpression.Compile();
      var compiledThenByExpression = thenByExpression.Compile();
      var notOrderedLocal = Query.All<T>().ToArray();
      var orderedLocal = notOrderedLocal.OrderBy(compiledOrderByExpression).ThenBy(compiledThenByExpression);
      var orderedLocalDescending = notOrderedLocal.OrderByDescending(compiledOrderByExpression).ThenBy(compiledThenByExpression);
      var orderedByServer = Query.All<T>().OrderBy(orderByExpression).ThenBy(thenByExpression);
      var orderedByServerDescending = Query.All<T>().OrderByDescending(orderByExpression).ThenBy(thenByExpression);

      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedLocal));
      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocalDescending.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocal.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocalDescending.SequenceEqual(orderedByServer));
    }

    protected void OrderByPrivate<T1, T2, T3>(Expression<Func<T1, T2>> selectorExpression, Expression<Func<T2, T3>> orderByExpression)
      where T1 : Entity
    {
      var compiledOrderByExpression = orderByExpression.Compile();

      var notOrderedLocal = Query.All<T1>().Select(selectorExpression).ToArray();
      var orderedLocal = notOrderedLocal.OrderBy(compiledOrderByExpression);
      var orderedLocalDescending = notOrderedLocal.OrderByDescending(compiledOrderByExpression);
      var orderedByServer = Query.All<T1>().Select(selectorExpression).OrderBy(orderByExpression);
      var orderedByServerDescending = Query.All<T1>().Select(selectorExpression).OrderByDescending(orderByExpression);

      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedLocal));
      Assert.IsFalse(notOrderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocal.SequenceEqual(orderedByServer));
      Assert.IsTrue(orderedLocalDescending.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocal.SequenceEqual(orderedByServerDescending));
      Assert.IsFalse(orderedLocalDescending.SequenceEqual(orderedByServer));
    }
  }
}