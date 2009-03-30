// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class DistinctTest : NorthwindDOModelTest
  {
    [Test]
    public void DefaultTest()
    {
        var result = Query<Customer>.All.Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void ScalarTest()
    {
        var result = Query<Customer>.All.Select(c => c.Address.City).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void OrderByTest()
    {
        var result = Query<Customer>.All.OrderBy(c => c.Id).Select(c => c.Address.City).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }


    [Test]
    public void DistinctOrderByTest()
    {
        var result = Query<Customer>.All.Select(c => c.Address.City).Distinct().OrderBy(c => c);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void CountTest()
    {
        var result = Query<Customer>.All.Distinct().Count();
        Assert.Greater(result, 0);
    }

    [Test]
    public void SelectDistinctCountTest()
    {
        var result = Query<Customer>.All.Select(c => c.Address.City).Distinct().Count();
        Assert.Greater(result, 0);
    }

    [Test]
    public void NestedSelectDistinctCountTest()
    {
        var result = Query<Customer>.All.Select(c => c.Address).Select(a => a.City).Distinct().Count();
        Assert.Greater(result, 0);
    }

    [Test]
    public void CountPredicateTest()
    {
        var result = Query<Customer>.All.Distinct().Count(c => c.Id == "ALFKI");
        Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
        var result = Query<Order>.All.Distinct().Sum(o => o.Id);
        Assert.Greater(result, 0);
    }

    [Test]
    public void SumTest()
    {
        var result = Query<Order>.All.Select(o => o.Id).Distinct().Sum();
        Assert.Greater(result, 0);
    }

    [Test]
    public void TakeTest()
    {
        // NOTE: distinct must be forced to apply after top has been computed
        var result = Query<Order>.All.Take(5).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void DistinctTakeTest()
    {
        // NOTE: top must be forced to apply after distinct has been computed
        var result = Query<Order>.All.Distinct().Take(5);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void TakeCountTest()
    {
        var result = Query<Order>.All.Distinct().Take(5).Count();
        Assert.Greater(result, 0);
    }

    [Test]
    public void TakeDistinctCountTest()
    {
        var result = Query<Order>.All.Take(5).Distinct().Count();
        Assert.Greater(result, 0);
    }

    [Test]
    public void SkipTest()
    {
        var result = Query<Customer>.All.OrderBy(c => c.ContactName).Skip(5).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void DistinctSkipTest()
    {
        var result = Query<Customer>.All.Distinct().OrderBy(c => c.ContactName).Skip(5);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void SkipTakeTest()
    {
        var result = Query<Customer>.All.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void TakeSkipTest()
    {
        var result = Query<Customer>.All.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct();
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
    }

    [Test]
    public void DistinctSkipTakeTest()
    {
        var result = Query<Customer>.All.Select(c => c.ContactName).Distinct().OrderBy(c => c).Skip(5).Take(10);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
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