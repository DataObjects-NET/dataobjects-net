// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.07.29

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class OrderByTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void DateTimeOffsetOrderByTest()
    {
      ExecuteInsideSession(() => {
        OrderByPrivate<DateTimeOffsetEntity, DateTimeOffset, long>(c => c.DateTimeOffset, c => c.Id);
        OrderByPrivate<DateTimeOffsetEntity, DateTimeOffset, DateTimeOffset>(c => c.DateTimeOffset, c => c);
      });
    }

    [Test]
    public void MillisecondDateTimeOffsetOrderByTest()
    {
      ExecuteInsideSession(() => {
        OrderByPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset, long>(c => c.DateTimeOffset, c => c.Id);
        OrderByPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset, DateTimeOffset>(c => c.DateTimeOffset, c => c);
      });
    }

    [Test]
    public void NullableDateTimeOffsetOrderByTest()
    {
      ExecuteInsideSession(() => {
        OrderByPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?, long>(c => c.DateTimeOffset, c => c.Id);
        OrderByPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?, DateTimeOffset?>(c => c.DateTimeOffset, c => c);
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

      Assert.That(notOrderedLocal.SequenceEqual(orderedLocal), Is.False);
      Assert.That(notOrderedLocal.SequenceEqual(orderedByServer), Is.False);
      Assert.That(orderedLocal.SequenceEqual(orderedByServer), Is.True);
      Assert.That(orderedLocalDescending.SequenceEqual(orderedByServerDescending), Is.True);
      Assert.That(orderedLocal.SequenceEqual(orderedByServerDescending), Is.False);
      Assert.That(orderedLocalDescending.SequenceEqual(orderedByServer), Is.False);
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

      Assert.That(notOrderedLocal.SequenceEqual(orderedLocal), Is.False);
      Assert.That(notOrderedLocal.SequenceEqual(orderedByServer), Is.False);
      Assert.That(orderedLocal.SequenceEqual(orderedByServer), Is.True);
      Assert.That(orderedLocalDescending.SequenceEqual(orderedByServerDescending), Is.True);
      Assert.That(orderedLocal.SequenceEqual(orderedByServerDescending), Is.False);
      Assert.That(orderedLocalDescending.SequenceEqual(orderedByServer), Is.False);
    }
  }
}
