// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class AggregateTest : NorthwindDOModelTest
  {
    [Test]
    public void EntityNotSupportedTest()
    {
      AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Max());
      AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Min());
    }

    [Test]
    public void SumWithNoArgTest()
    {
      var sum = Query<Order>.All.Select(o => o.Freight).Sum();
      Assert.Greater(sum, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var sum = Query<Order>.All.Sum(o => o.Id);
      Assert.Greater(sum, 0);
    }

    [Test]
    public void CountWithNoPredicateTest()
    {
      var count = Query<Order>.All.Count();
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountWithPredicateTest()
    {
      var count = Query<Order>.All.Count(o => o.Id > 10);
      Assert.Greater(count, 0);
    }

    [Test]
    public void CountAfterFilterTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Count() > 10
        select c;
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereCountTest()
    {
      var result = Query<Customer>.All.Where(c => Query<Order>.All.Count(o => o.Customer == c) > 5);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void WhereCountWithPredicateTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Count(o => o.Customer==c) > 10
        select c;
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMaxWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Max(o => o.OrderDate) < new DateTime(1999, 1, 1)
        select c;
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereMinWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Min(o => o.Freight) > 5
        select c;
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void WhereAverageWithSelectorTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c).Average(o => o.Freight) < 5
        select c;
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectCountTest()
    {
      var result =
        from c in Query<Customer>.All
        select new {Customer = c, NumberOfOrders = Query<Order>.All.Count(o => o.Customer==c)};
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SelectMaxTest()
    {
      var products = Query<Product>.All;
      var suppliers = Query<Supplier>.All;
      var result = from p in products
                   select new { Product = p, MaxID = suppliers.Where(s => s == p.Supplier).Max(s => s.Id) };
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void SumCountTest()
    {
      var expected = Query<Order>.All.Count();
      var count = Query<Customer>.All.Sum(c => Query<Order>.All.Count(o => o.Customer == c));
      Assert.AreEqual(expected, count);
    }
  }
}