// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class DistinctTest : NorthwindDOModelTest
  {
    [Test]
    public void DefaultTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void ScalarTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Select(c => c.Address.City).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void OrderByTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderBy(c => c.Id).Select(c => c.Address.City).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }


    [Test]
    public void DistinctOrderByTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Select(c => c.Address.City).Distinct().OrderBy(c => c);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void CountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Distinct().Count();
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void SelectDistinctCountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Select(c => c.Address.City).Distinct().Count();
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void NestedSelectDistinctCountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Select(c => c.Address).Select(a => a.City).Distinct().Count();
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void CountPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Distinct().Count(c => c.Id == "ALFKI");
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void SumWithArgTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.Distinct().Sum(o => o.Id);
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void SumTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.Select(o => o.Id).Distinct().Sum();
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void TakeTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        // NOTE: distinct must be forced to apply after top has been computed
        var result = Query<Order>.All.Take(5).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void DistinctTakeTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        // NOTE: top must be forced to apply after distinct has been computed
        var result = Query<Order>.All.Distinct().Take(5);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void TakeCountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.Distinct().Take(5).Count();
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void TakeDistinctCountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.Take(5).Distinct().Count();
        Assert.Greater(result, 0);
        t.Complete();
      }
    }

    [Test]
    public void SkipTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderBy(c => c.ContactName).Skip(5).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void DistinctSkipTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Distinct().OrderBy(c => c.ContactName).Skip(5);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void SkipTakeTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void TakeSkipTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void DistinctSkipTakeTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    /*
     * 
     * public void TestSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5)
                );
        }

        public void TestSkipTake()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }

        public void TestTakeSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5)
                );
        }
     * 
     * public void TestDistinctGroupBy()
        {
            TestQuery(
                db.Orders.Distinct().GroupBy(o => o.CustomerID)
                );
        }

        public void TestGroupByDistinct()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Distinct()
                );

        }

*/
  }
}