// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.30

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public abstract class BaseDateTimeAndDateTimeOffsetTest
    : AutoBuildTest
  {
    [Test(Description = "Might be failed on SQLite on reverse ordering because of certain restrictions of work with milliseconds")]
    public void OrderByTest()
    {
      OpenSessionAndAction(OrderByProtected);
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DistinctTest()
    {
      OpenSessionAndAction(DistinctProtected);
    }

    [Test]
    public void MinMaxTest()
    {
      OpenSessionAndAction(MinMaxProtected);
    }

    [Test(Description = "Might be failed on SQLite on reverse ordering because of certain restrictions of work with milliseconds")]
    public void GroupByTest()
    {
      OpenSessionAndAction(GroupByProtected);
    }

    [Test(Description = "Might be failed on SQLite on reverse ordering because of certain restrictions of work with milliseconds")]
    public void JoinTest()
    {
      OpenSessionAndAction(JoinProtected);
    }

    [Test]
    public void SkipTakeTest()
    {
      OpenSessionAndAction(SkipTakeProtected);
    }

    [Test]
    public void EqualsTest()
    {
      OpenSessionAndAction(EqualsProtected);
    }

    [Test]
    public void DifferentCulturesTest()
    {
      var oldCulture = Thread.CurrentThread.CurrentCulture;
      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        EqualsTest();
      }
      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
        EqualsTest();
      }

      using (new Disposable(c => Thread.CurrentThread.CurrentCulture = oldCulture)) {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        EqualsTest();
      }
    }

    protected abstract void PopulateDataProtected();

    protected abstract void OrderByProtected();
    protected abstract void DistinctProtected();
    protected abstract void MinMaxProtected();
    protected abstract void GroupByProtected();
    protected abstract void JoinProtected();
    protected abstract void SkipTakeProtected();
    protected abstract void EqualsProtected();

    protected override void PopulateData()
    {
      OpenSessionAndAction(PopulateDataProtected, true);
    }

    #region Implementation of base tests

    protected void RunTest<T>(Expression<Func<T, bool>> filter, int rightCount = 1)
      where T : Entity
    {
      var count = Query.All<T>().Count(filter);
      Assert.AreEqual(count, rightCount);
    }

    protected void RunWrongTest<T>(Expression<Func<T, bool>> filter)
      where T : Entity
    {
      RunTest(filter, 0);
    }

    protected void OpenSessionAndAction(Action action, bool commitTransaction = false)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        action();
        if (commitTransaction)
          tx.Complete();
      }
    }

    protected void OrderByProtected<T, TK1, TK2>(Expression<Func<T, TK1>> orderByExpression, Expression<Func<T, TK2>> thenByExpression)
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

    protected void OrderByProtected<T1, T2, T3>(Expression<Func<T1, T2>> selectorExpression, Expression<Func<T2, T3>> orderByExpression)
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

    protected void DistinctProtected<T, TK>(Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var distinctLocal = Query.All<T>().ToArray().Select(compiledSelectExpression).Distinct().OrderBy(c => c);
      var distinctByServer = Query.All<T>().Select(selectExpression).Distinct().OrderBy(c => c);
      Assert.IsTrue(distinctLocal.SequenceEqual(distinctByServer));

      distinctByServer = Query.All<T>().Select(selectExpression).Distinct().OrderByDescending(c => c);
      Assert.IsFalse(distinctLocal.SequenceEqual(distinctByServer));
    }

    protected void MinMaxProtected<T, TK>(Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var minLocal = Query.All<T>().ToArray().Min(compiledSelectExpression);
      var maxLocal = Query.All<T>().ToArray().Max(compiledSelectExpression);
      var minServer = Query.All<T>().Min(selectExpression);
      var maxServer = Query.All<T>().Max(selectExpression);

      Assert.AreEqual(minLocal, minServer);
      Assert.AreEqual(maxLocal, maxServer);
      Assert.AreNotEqual(minLocal, maxServer);
      Assert.AreNotEqual(maxLocal, minServer);
    }

    protected void GroupByProtected<T, TK1, TK2>(Expression<Func<T, TK1>> groupByExpression, Expression<Func<T, TK2>> orderByExpression)
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

    protected void SkipTakeProctected<T, TK1, TK2>(Expression<Func<T, TK1>> orderByExpression, Expression<Func<T, TK2>> thenByExpression, int skipCount, int takeCount)
      where T : Entity
    {
      var compiledOrderByExpression = orderByExpression.Compile();
      var compiledThenByExpression = thenByExpression.Compile();
      var skipTakeLocal = Query.All<T>().ToArray().OrderBy(compiledOrderByExpression).ThenBy(compiledThenByExpression).Skip(skipCount).Take(takeCount).ToArray();
      var skipTakeServer = Query.All<T>().OrderBy(orderByExpression).ThenBy(thenByExpression).Skip(skipCount).Take(takeCount);
      Assert.IsTrue(skipTakeLocal.SequenceEqual(skipTakeServer));

      skipTakeServer = Query.All<T>().OrderByDescending(orderByExpression).Skip(skipCount).Take(takeCount);
      Assert.IsFalse(skipTakeLocal.SequenceEqual(skipTakeServer));
    }

    protected void JoinProtected<T1, T2, T3, TK1, TK3>(Expression<Func<T1, TK1>> leftJoinExpression, Expression<Func<T2, TK1>> rightJoinExpression,
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

    #endregion
  }
}
