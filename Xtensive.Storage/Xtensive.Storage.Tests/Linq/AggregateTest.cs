// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class AggregateTest : NorthwindDOModelTest
  {
    [Test]
    public void SumWithNoArgTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var sum = Query<Order>.All.Select(o => o.Id).Sum();
        Assert.Greater(sum, 0);
      }
    }

    [Test]
    public void TestSumWithArg()
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
  }
}