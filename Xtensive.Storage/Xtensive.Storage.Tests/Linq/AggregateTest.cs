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
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Max());
        AssertEx.ThrowsNotSupportedException(() => Query<Order>.All.Min());
        t.Complete();
      }
    }

    [Test]
    public void SumWithNoArgTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var sum = Query<Order>.All.Select(o => o.Freight).Sum();
        Assert.Greater(sum, 0);
        t.Complete();
      }
    }

    [Test]
    public void SumWithArgTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var sum = Query<Order>.All.Sum(o => o.Id);
        Assert.Greater(sum, 0);
        t.Complete();
      }
    }

    [Test]
    public void CountWithNoPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var count = Query<Order>.All.Count();
        Assert.Greater(count, 0);
        t.Complete();
      }
    }

    [Test]
    public void CountWithPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var count = Query<Order>.All.Count(o => o.Id > 10);
        Assert.Greater(count, 0);
        t.Complete();
      }
    }

    [Test]
    public void WhereCountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result =
          from c in Query<Customer>.All
          where Query<Order>.All.Where(o => o.Customer == c).Count() > 10
          select c;
        Assert.Greater(result.ToList().Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void WhereCountWithPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All
          where Query<Order>.All.Count(o => o.Customer == c) > 10
          select c;
        Assert.Greater(result.ToList().Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void WhereMaxWithSelectorTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All
          where Query<Order>.All.Where(o => o.Customer==c).Max(o => o.OrderDate) < new DateTime(1999, 1, 1)
          select c;
        Assert.Greater(result.ToList().Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void WhereMinWithSelectorTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All
          where Query<Order>.All.Where(o => o.Customer==c).Min(o => o.Freight) > 5
          select c;
        Assert.Greater(result.ToList().Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void WhereAverageWithSelectorTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All
          where Query<Order>.All.Where(o => o.Customer == c).Average(o => o.Freight) < 5
          select c;
        Assert.Greater(result.ToList().Count, 0);
        t.Complete();
      }
    }
  }
}