// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.16

using System.Collections;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
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
        numberOfCustomers = Query.All<Customer>().Count();
        numberOfOrders = Query.All<Order>().Count();
        numberOfEmployees = Query.All<Employee>().Count();
        t.Complete();
      }
    }

    [Test]
    public void SubqueryTest()
    {
      var result =
        from c in Query.All<Customer>()
        select new {
          Customer = c,
          Orders = c.Orders
            .Select(o => new { Order = o, Details = o.OrderDetails })
        };
      foreach (var a in result)
        foreach (var b in a.Orders)
          foreach (var detail in b.Details) {
          }
    }

    [Test]
    public void SubqueryWithThisTest()
    {
      var shipper = Query.All<Shipper>().First();
      var orders = shipper.Orders;
      QueryDumper.Dump(orders);
      var firstOrder = shipper.FirstOrder;
    }

    [Test]
    public void SelectOtherParameterTest()
    {
      var result = Query.All<Customer>().Take(5).Select(c => Query.All<Order>().Select(o => c.Orders.Count()));
      result.ToList();
    }

    [Test]
    public void SelectNestedTest()
    {
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>())
        .Select(os => os);
      Assert.AreEqual(numberOfCustomers * numberOfOrders, Count(result));
    }

    [Test]
    public void SelectDoubleNestedTest()
    {
      var result = Query.All<Customer>()
        .Take(2)
        .Select(c => Query.All<Order>()
          .Take(2)
          .Select(o => Query.All<Employee>().Take(2)))
        .Select(os => os);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexSubqueryTest()
    {
      var result = Query.All<Customer>()
        .Take(2)
        .Select(c => Query.All<Order>()
          .Select(o => Query.All<Employee>()
            .Take(2)
            .Where(e => e.Orders.Contains(o)))
          .Where(o => o.Count() > 0))
        .Select(os => os);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationTest()
    {
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>().Where(o => o.Customer==c))
        .Select(os => os);
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectNestedWithAggregateTest()
    {
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>().Where(o => o.Customer==c).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousTest()
    {
      var result = Query.All<Customer>()
        .Take(10)
        .Select(c => new {Orders = Query.All<Order>().Take(10)});
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryAsQuerySourceTest()
    {
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>().Where(o => o.Customer==c));
      foreach (var orders in result) {
        var subQueryCount = orders.Count();
      }
    }

    [Test]
    public void SelectTwoCollectionsTest()
    {
      var result = Query.All<Order>()
        .Select(o => new {
          Customers = Query.All<Customer>().Where(c => c==o.Customer),
          Employees = Query.All<Employee>().Where(e => e==o.Employee)
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
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>())
        .SelectMany(i => i);
      Assert.AreEqual(numberOfCustomers * numberOfOrders, Count(result));
    }

    [Test]
    public void SelectDoubleNestedSelectManyTest()
    {
      var result = Query.All<Customer>()
        .Take(10)
        .Select(c => Query.All<Order>()
          .Take(10)
          .Select(o => Query.All<Employee>()))
        .SelectMany(i => i)
        .SelectMany(i => i);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectNestedWithCorrelationSelectManyTest()
    {
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>().Where(o => o.Customer==c))
        .SelectMany(i => i);
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectNestedWithCorrelationSelectMany2Test()
    {
      var result = Query.All<Customer>()
        .Select(c => Query.All<Order>().Where(o => o.Customer==c))
        .SelectMany(i => i.Select(x => x));
      Assert.AreEqual(numberOfOrders, Count(result));
    }


    [Test]
    public void SelectAnonymousSelectMany1Test()
    {
      var result = Query.All<Customer>()
        .Select(c => new {Customer = c, Orders = Query.All<Order>().Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders);
      Assert.AreEqual(numberOfOrders, result.ToList().Count);
    }

    [Test]
    public void SelectAnonymousSelectMany2Test()
    {
      var result = Query.All<Customer>()
        .Select(c => new {Orders = Query.All<Order>().Take(10)})
        .SelectMany(i => i.Orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSubqueryTest()
    {
      var result = Query.All<Customer>()
        .Select(c => new {Customer = c, Orders = Query.All<Order>()})
        .Select(i => i.Customer.Orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSelectMany3Test()
    {
      IQueryable<Customer> result = Query.All<Customer>()
        .Select(c => new {Customer = c, Orders = Query.All<Order>().Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => i.Customer));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectAnonymousSelectMany4Test()
    {
      var result = Query.All<Customer>()
        .Select(c => new {Customer = c, Orders = Query.All<Order>().Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => new {i.Customer, Order = o}));
      Assert.AreEqual(numberOfOrders, result.ToList().Count);
    }

    [Test]
    public void SelectAnonymousSelectMany5Test()
    {
      var result = Query.All<Customer>()
        .Select(c => new {Customer = c, Orders = Query.All<Order>().Where(o => o.Customer==c)})
        .SelectMany(i => i.Orders.Select(o => i.Customer.CompanyName));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Query.All<Product>()
        .Select(p => Query.All<Category>());
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWhereTest()
    {
      var result = Query.All<Product>()
        .Select(p => Query.All<Category>().Where(c => c==p.Category));
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryParametrizedFieldTest()
    {
      var result = Query.All<Product>()
        .Select(p => Query.All<Category>().Select(c => p.UnitPrice));

      foreach (var queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryWithSelectTest()
    {
      var result = Query.All<Product>()
        .Select(p => Query.All<Category>().Where(c => c==p.Category))
        .Select(q => q);
      foreach (IQueryable<Category> queryable in result) {
        QueryDumper.Dump(queryable);
      }
    }

    [Test]
    public void SubqueryScalarTest()
    {
      var result = Query.All<Product>()
        .Select(p => Query.All<Category>().Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryWhereEntitySetTest()
    {
      var result = Query.All<Category>()
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