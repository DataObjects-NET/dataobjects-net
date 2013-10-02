// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class SortExpressionExtractorTest
  {
    public class Entity1
    {
      public string Description { get; private set; }
    }

    public class Entity2
    {
      public string Name { get; private set; }

      public int Age { get; private set; }

      public Entity1 Other { get; private set; }
    }

    [Test]
    public void Test1()
    {
      var extractor = new SortExpressionExtractor();
      var baseQuery = GetQuery();
      var query = baseQuery.OrderBy(e => e.Age).ThenBy(e => e.Other.Description);
      Assert.That(extractor.Extract(query.Expression));
      Assert.That(extractor.BaseExpression, Is.SameAs(baseQuery.Expression));
      Assert.That(extractor.SortExpressions.Count, Is.EqualTo(2));
      AssertSortExpressionIs(extractor.SortExpressions[0], Direction.Positive, e => e.Age);
      AssertSortExpressionIs(extractor.SortExpressions[1], Direction.Positive, e => e.Other.Description);
    }

    [Test]
    public void Test2()
    {
      var extractor = new SortExpressionExtractor();
      var baseQuery = GetQuery();
      var query = baseQuery.OrderByDescending(e => e.Age).ThenBy(e => e.Other.Description);
      Assert.That(extractor.Extract(query.Expression));
      Assert.That(extractor.BaseExpression, Is.SameAs(baseQuery.Expression));
      Assert.That(extractor.SortExpressions.Count, Is.EqualTo(2));
      AssertSortExpressionIs(extractor.SortExpressions[0], Direction.Negative, e => e.Age);
      AssertSortExpressionIs(extractor.SortExpressions[1], Direction.Positive, e => e.Other.Description);
    }

    [Test]
    public void Test3()
    {
      var extractor = new SortExpressionExtractor();
      var baseQuery = GetQuery();
      var query = baseQuery.OrderBy(e => e.Age).ThenByDescending(e => e.Other.Description);
      Assert.That(extractor.Extract(query.Expression));
      Assert.That(extractor.BaseExpression, Is.SameAs(baseQuery.Expression));
      Assert.That(extractor.SortExpressions.Count, Is.EqualTo(2));
      AssertSortExpressionIs(extractor.SortExpressions[0], Direction.Positive, e => e.Age);
      AssertSortExpressionIs(extractor.SortExpressions[1], Direction.Negative, e => e.Other.Description);
    }

    [Test]
    public void Test4()
    {
      var extractor = new SortExpressionExtractor();
      var baseQuery = GetQuery();
      var query = baseQuery.OrderBy(e => e.Other.Description);
      Assert.That(extractor.Extract(query.Expression));
      Assert.That(extractor.BaseExpression, Is.SameAs(baseQuery.Expression));
      Assert.That(extractor.SortExpressions.Count, Is.EqualTo(1));
      AssertSortExpressionIs(extractor.SortExpressions[0], Direction.Positive, e => e.Other.Description);
    }

    [Test]
    public void Test5()
    {
      var extractor = new SortExpressionExtractor();
      var baseQuery = GetQuery();
      Assert.That(extractor.Extract(baseQuery.Expression), Is.False);
    }

    private void AssertSortExpressionIs<T>(KeyValuePair<LambdaExpression, Direction> item,
      Direction exprectedDirection, Expression<Func<Entity2, T>> expectedExpression)
    {
      var actualExpression = item.Key;
      var actualDirection = item.Value;

      AssertExpressionEquals(expectedExpression, actualExpression);

      Assert.That(actualDirection, Is.EqualTo(exprectedDirection));
    }

    private static void AssertExpressionEquals(LambdaExpression expectedExpression, LambdaExpression actualExpression)
    {
      var equals = ExpressionTree.Equals(actualExpression, expectedExpression);
      Assert.That(equals, "Expected expression '{0}' got '{1}'", expectedExpression.ToString(true), actualExpression.ToString(true));
    }

    private IQueryable<Entity2> GetQuery()
    {
      return new Entity2[0].AsQueryable();
    }
  }
}