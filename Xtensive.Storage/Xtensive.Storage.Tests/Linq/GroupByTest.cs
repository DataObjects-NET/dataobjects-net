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
  public class GroupByTest : NorthwindDOModelTest
  {
    [Test]
    public void EntityGroupTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Product>.All.GroupBy(p => p.Category);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void EntityKeyGroupTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Product>.All.GroupBy(p => p.Category.Key);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void EntityFieldGroupTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Product>.All.GroupBy(p => p.Category.CategoryName);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void StructureGroupTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Customer>.All.GroupBy(p => p.Address);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void AnonimousTypeGroupTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Customer>.All.GroupBy(c => new { c.Address.City, c.Address.Country });
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        foreach (var grouping in list)
        {
          Assert.IsNotNull(grouping.Key);
          Assert.Greater(grouping.Count(), 0);
        }
        t.Complete();
      }
    }


    [Test]
    public void AnonimousTypeEntityAndStructureGroupTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Employee>.All.GroupBy(e => new
                                                      {
                                                        e.Address.City, 
                                                        e.Address.Country, 
                                                        e.Address, 
                                                        e.ReportsTo, 
                                                        e.Phone, 
                                                        ReporterAddress = e.ReportsTo.Address,
                                                        ReporterAddressCountry = e.ReportsTo.Address.Country
                                                      });
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        foreach (var grouping in list)
        {
          Assert.IsNotNull(grouping.Key);
          Assert.Greater(grouping.Count(), 0);
        }
        t.Complete();
      }
    }


    [Test]
    public void DefaultTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.GroupBy(c => c.Address.City);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        foreach (var grouping in list) {
          Assert.IsNotNull(grouping.Key);
          Assert.Greater(grouping.Count(), 0);
        }
        t.Complete();
      }
    }

    [Test]
    public void GroupBySelectManyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.GroupBy(c => c.Address.City).SelectMany(g => g);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupBySumTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer.Id).Select(g => g.Sum(o => o.Freight));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByCountTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer).Select(g => new {Customer = g.Key, OrdersCount = g.Count()});
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupBySumMinMaxAvgTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer).Select(g =>
          new
          {
            Sum = g.Sum(o => o.Freight),
            Min = g.Min(o => o.Freight),
            Max = g.Max(o => o.Freight),
            Avg = g.Average(o => o.Freight)
          });

        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByWithResultSelectorTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer, (c,g) =>
          new
          {
            Customer = c,
            Sum = g.Sum(o => o.Freight),
            Min = g.Min(o => o.Freight),
            Max = g.Max(o => o.Freight),
            Avg = g.Average(o => o.Freight)
          });

        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByWithElementSelectorSumTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer, o => o.Freight).Select(g => g.Sum());
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByWithElementSelectorTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open())
      {
        var result = Query<Order>.All.GroupBy(o => o.Customer, o => o.Freight);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByWithElementSelectorSumMaxTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer.Id, o => o.Freight).Select(g => new {Sum = g.Sum(), Max = g.Max()});
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByWithAnonymousElementTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => o.Customer, o=> new {o.Freight}).Select(g => g.Sum(x => x.Freight));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void GroupByWithTwoPartKeyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.GroupBy(o => new {o.Customer.Id, o.OrderDate}).Select(g => g.Sum(o => o.Freight));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void OrderByGroupByTest()
    {
      // NOTE: order-by is lost when group-by is applied (the sequence of groups is not ordered)
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.OrderBy(o => o.OrderDate).GroupBy(o => o.Customer.Id).Select(g => g.Sum(o => o.Freight));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void OrderByGroupBySelectManyTest()
    {
      // NOTE: order-by is preserved within grouped sub-collections
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Order>.All.OrderBy(o => o.OrderDate).GroupBy(o => o.Customer.Id).SelectMany(g => g);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }
  }
}