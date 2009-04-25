// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.16

using System.Collections;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  [Category("Linq")]
  public class NestedCollectionsTest : NorthwindDOModelTest
  {
    private int numberOfCustomers;
    private int numberOfOrders;
    private int numberOfEmployees;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        numberOfCustomers = Query<Customer>.All.Count();
        numberOfOrders = Query<Order>.All.Count();
        numberOfEmployees = Query<Employee>.All.Count();
        t.Complete();
      }
    }

    [Test]
    public void SelectNestedTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All)
        .Select(os => os);
      Assert.AreEqual(numberOfCustomers * numberOfOrders, Count(result));
    }

    [Test]
    public void SelectDoubleNestedTest()
    {
      var result = Query<Customer>.All
        .Take(10)
        .Select(c => Query<Order>.All
          .Take(10)
          .Select(o => Query<Employee>.All))
        .Select(os => os);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexSubqueryTest()
    {
      var result = Query<Customer>.All
        .Take(10)
        .Select(c => Query<Order>.All
          .Select(o => Query<Employee>.All
            .Take(10)
            .Where(e=>e.Orders.Contains(o)))
            .Where(o=>o.Count()>0))
        .Select(os => os);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer == c))
        .Select(os => os);
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectAnonymousTest()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer == c)})
        .Select(os => os);
      foreach (var item in result)
        Assert.AreEqual(item.Customer.Orders.Count, item.Orders.Count());
      foreach (var item in result)
        Assert.AreEqual(item.Customer.Orders.Count, item.Orders.ToList().Count);
    }

    [Test]
    public void SelectTwoCollectionsTest()
    {
      var result = Query<Order>.All
        .Select(o => new
          {
            Customers = Query<Customer>.All.Where(c => c==o.Customer),
            Employees = Query<Employee>.All.Where(e => e==o.Employee)
          })
        .Select(os => os);
      var list = result.ToList();
      Assert.AreEqual(numberOfOrders, list.Count);
      foreach (var i in list) {
        Assert.AreEqual(1, i.Customers.Count());
        Assert.AreEqual(1, i.Employees.Count());
        Assert.AreEqual(1, i.Customers.ToList().Count);
        Assert.AreEqual(1, i.Employees.ToList().Count);
      }
    }

    [Test]
    public void SelectNestedSelectManyTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All)
        .SelectMany(i => i);
      Assert.AreEqual(numberOfCustomers * numberOfOrders, Count(result));
    }

    [Test]
    public void SelectDoubleNestedSelectManyTest()
    {
      var result = Query<Customer>.All
        .Take(10)
        .Select(c => Query<Order>.All
          .Take(10)
          .Select(o => Query<Employee>.All))
        .SelectMany(i => i)
        .SelectMany(i => i);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationSelectManyTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer == c))
        .SelectMany(i => i);
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectAnonymousSelectManyTest()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => new {i.Customer, Order = o}));
      Assert.AreEqual(numberOfOrders, result.ToList().Count);
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Where(c => c==p.Category));
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWithSelectTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Where(c => c==p.Category))
        .Select(q=>q);
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryScalarTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryWhereEntitySetTest()
    {
      var result = Query<Category>.All
        .Where(c => c.Products.Count > 0);
      QueryDumper.Dump(result);
    }


    private static int Count(IEnumerable result)
    {
      int count = 0;
      bool? nested = null;
      foreach (var item in result) {
        if (nested == null)
          nested = item is IEnumerable;
        if (nested.Value)
          count += Count((IEnumerable)item);
        else
          count++;
      }
      return count;
    }
  }
}