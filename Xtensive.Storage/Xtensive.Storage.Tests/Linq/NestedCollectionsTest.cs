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

      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        numberOfCustomers = Query<Customer>.All.Count();
        numberOfOrders = Query<Order>.All.Count();
        numberOfEmployees = Query<Employee>.All.Count();
        t.Complete();
      }
    }

    [Test]
    public void SubqueryWithThisTest()
    {
      var shipper = Query<Shipper>.All.First();
      var orders = shipper.Orders;
      QueryDumper.Dump(orders);
      var firstOrder = shipper.FirstOrder;
    }

    [Test]
    public void SelectOtherParameterTest()
    {
      var result = Query<Customer>.All.Select(c => Query<Order>.All.Select(o => c.Orders.Count()));
      result.ToList();
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
        .Take(2)
        .Select(c => Query<Order>.All
          .Take(2)
          .Select(o => Query<Employee>.All.Take(2)))
        .Select(os => os);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexSubqueryTest()
    {
      var result = Query<Customer>.All
        .Take(2)
        .Select(c => Query<Order>.All
          .Select(o => Query<Employee>.All
            .Take(2)
            .Where(e => e.Orders.Contains(o)))
          .Where(o => o.Count() > 0))
        .Select(os => os);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer==c))
        .Select(os => os);
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectNestedWithAggregateTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer==c).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousTest()
    {
      var result = Query<Customer>.All
        .Take(10)
        .Select(c => new {Orders = Query<Order>.All.Take(10)});
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryAsQuerySourceTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer==c));
      foreach (var orders in result) {
        var subQueryCount = orders.Count();
      }
    }

    [Test]
    public void SelectTwoCollectionsTest()
    {
      var result = Query<Order>.All
        .Select(o => new {
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
        .Select(c => Query<Order>.All.Where(o => o.Customer==c))
        .SelectMany(i => i);
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectNestedWithCorrelationSelectMany2Test()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer==c))
        .SelectMany(i => i.Select(x => x));
      Assert.AreEqual(numberOfOrders, Count(result));
    }


    [Test]
    public void SelectAnonymousSelectMany1Test()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders);
      Assert.AreEqual(numberOfOrders, result.ToList().Count);
    }

    [Test]
    public void SelectAnonymousSelectMany2Test()
    {
      var result = Query<Customer>.All
        .Select(c => new {Orders = Query<Order>.All.Take(10)})
        .SelectMany(i => i.Orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSubqueryTest()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All})
        .Select(i => i.Customer.Orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSelectMany3Test()
    {
      IQueryable<Customer> result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => i.Customer));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSelectMany4Test()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => new {i.Customer, Order = o}));
      Assert.AreEqual(numberOfOrders, result.ToList().Count);
    }

    [Test]
    public void SelectAnonymousSelectMany5Test()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => i.Customer.CompanyName));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All);
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWhereTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Where(c => c==p.Category));
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryParametrizedFieldTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Select(c => p.UnitPrice));

      foreach (var queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWithSelectTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Category>.All.Where(c => c==p.Category))
        .Select(q => q);
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
        if (nested==null)
          nested = item is IEnumerable;
        if (nested.Value)
          count += Count((IEnumerable) item);
        else
          count++;
      }
      return count;
    }
  }
}