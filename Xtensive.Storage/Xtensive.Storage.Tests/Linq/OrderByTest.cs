// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.29

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class OrderByTest : NorthwindDOModelTest
  {
    [Test]
    public void OrderByTakeTest()
    {
      var result = Query<Order>.All.OrderBy(o => o.Id).Take(10);
      var list = result.ToList();
      var expected = Orders.OrderBy(o => o.Id).Take(10);
      Assert.AreEqual(10, list.Count);
      Assert.IsTrue(expected.SequenceEqual(list));
    }

    [Test]
    public void EntityFieldTest()
    {
      var result = Query<Product>.All.Select(p => p).OrderBy(g => g.Id);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByAnonymousEntityTest()
    {
      var result = Query<OrderDetails>.All
        .Select(od => new {Details = od, Order = od.Order})
        .OrderBy(x => new {x, x.Order.Customer})
        .Select(x => new {x, x.Order.Customer});
      var expected = Query<OrderDetails>.All.ToList()
        .Select(od => new {Details = od, Order = od.Order})
        .OrderBy(x => x.Details.Order.Id)
        .ThenBy(x => x.Details.Product.Id)
        .ThenBy(x => x.Details.Order.Id)
        .ThenBy(x => x.Order.Customer.Id)
        .Select(x => new {x, x.Order.Customer});
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByAnonymousTest()
    {
      var result = Query<Order>.All
        .OrderBy(o => new {o.OrderDate, o.Freight})
        .Select(o => new {o.OrderDate, o.Freight});
      var expected = Orders
        .OrderBy(o => o.OrderDate)
        .ThenBy(o => o.Freight)
        .Select(o => new {o.OrderDate, o.Freight});
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByAnonymous2Test()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      var result = customers
        .Select(c => new {c.Address.Country, c})
        .OrderBy(x => x);
      var expected = customers.ToList()
        .Select(c => new {c.Address.Country, c})
        .OrderBy(x => x.Country)
        .ThenBy(x => x.c.Id);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByDescendingTest()
    {
      var result = Query<Customer>.All.OrderByDescending(c => c.Address.Country)
        .ThenByDescending(c => c.Id).Select(c => c.Address.City);
      var expected = Customers.OrderByDescending(c => c.Address.Country)
        .ThenByDescending(c => c.Id).Select(c => c.Address.City);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByEntityTest()
    {
      var customers = Query<Customer>.All;
      var result = customers.OrderBy(c => c);
      var expected = customers.ToList().OrderBy(c => c.Id);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByExpressionTest()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      List<string> original = customers.Select(c => c.ContactName).ToList().Select(s => s.ToUpper()).ToList();
      original.Sort();
      Assert.IsTrue(original.SequenceEqual(
        customers
          .OrderBy(c => c.ContactName.ToUpper())
          .ToList()
          .Select(c => c.ContactName.ToUpper())));
    }

    [Test]
    public void OrderByJoinTest()
    {
      var result =
        from c in Query<Customer>.All.OrderBy(c => c.ContactName)
        join o in Query<Order>.All.OrderBy(o => o.OrderDate) on c equals o.Customer
        select new {c.ContactName, o.OrderDate};
      var expected =
        from c in Query<Customer>.All.ToList().OrderBy(c => c.ContactName)
        join o in Query<Order>.All.ToList().OrderBy(o => o.OrderDate) on c equals o.Customer
        select new {c.ContactName, o.OrderDate};
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderByPersistentPropertyTest()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      List<string> original = customers.Select(c => c.ContactName).ToList();
      original.Sort();

      Assert.IsTrue(original.SequenceEqual(
        customers
          .OrderBy(c => c.ContactName)
          .ToList()
          .Select(c => c.ContactName)));
    }

    [Test]
    public void OrderBySelectAnonymousTest()
    {
      var result = Query<Customer>.All
        .OrderBy(c => c.Id)
        .Select(c => new {c.Fax, c.Phone});
      var expected = Customers.ToList()
        .OrderBy(c => c.Id)
        .Select(c => new {c.Fax, c.Phone});
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderBySelectManyTest()
    {
      var result =
        from c in Query<Customer>.All.OrderBy(c => c.ContactName)
        from o in Query<Order>.All.OrderBy(o => o.OrderDate)
        where c==o.Customer
        select new {c.ContactName, o.OrderDate};
      var expected =
        from c in Query<Customer>.All.ToList().OrderBy(c => c.ContactName)
        from o in Query<Order>.All.ToList().OrderBy(o => o.OrderDate)
        where c==o.Customer
        select new {c.ContactName, o.OrderDate};
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void OrderBySelectTest()
    {
      IQueryable<string> result = Query<Customer>.All.OrderBy(c => c.CompanyName)
        .OrderBy(c => c.Address.Country).Select(c => c.Address.City);
      var expected = Query<Customer>.All.ToList().OrderBy(c => c.CompanyName)
        .OrderBy(c => c.Address.Country).Select(c => c.Address.City);
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void SequentialOrderByTest()
    {
      IQueryable<string> result = Query<Customer>.All
        .OrderBy(c => c.CompanyName)
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c)
        .Select(c => c);
      var expected = Query<Customer>.All
        .ToList()
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c)
        .Select(c => c);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void DistinctTest()
    {
      var result = Query<Order>.All.Select(o => o.Employee).Distinct();
      var expected = Query<Order>.All.ToList().Select(o => o.Employee).Distinct();
      Assert.AreEqual(0, result.ToList().Except(expected).Count());
    }

    [Test]
    public void OrderByTakeSkipTest()
    {
      var original = Query<Order>.All.ToList()
        .OrderBy(o => o.OrderDate)
        .Skip(100)
        .Take(50)
        .OrderBy(o => o.RequiredDate)
        .Where(o => o.OrderDate!=null)
        .Select(o => o.RequiredDate)
        .Distinct()
        .Skip(10);
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .Skip(100)
        .Take(50)
        .OrderBy(o => o.RequiredDate)
        .Where(o => o.OrderDate!=null)
        .Select(o => o.RequiredDate)
        .Distinct()
        .Skip(10);
      var originalList = original.ToList();
      var resultList = result.ToList();
      QueryDumper.Dump(originalList);
      QueryDumper.Dump(resultList);
      Assert.AreEqual(originalList.Count, resultList.Count);
      Assert.IsTrue(originalList.SequenceEqual(resultList));
    }

    [Test]
    public void PredicateTest()
    {
      IQueryable<int> result = Query<Order>.All.OrderBy(o => o.Freight > 100).ThenBy(o => o.Id).Select(o => o.Id);
      List<int> list = result.ToList();
      var queryNullDate = Query<Order>.All.Where(o => o.ShippedDate == null).ToList();
      var order = queryNullDate[0];
      var dateTime = order.ShippedDate;
      Assert.IsFalse(dateTime.HasValue);
      var listNullDate = queryNullDate.Where(o => o.ShippedDate == null).ToList();
      Assert.AreEqual(queryNullDate.Count, listNullDate.Count);
      List<int> original = Query<Order>.All.ToList().OrderBy(o => o.Freight > 100).ThenBy(o => o.Id).Select(o => o.Id).ToList();
      Assert.IsTrue(list.SequenceEqual(original));
    }

    [Test]
    public void SelectTest()
    {
      var result = Query<Customer>.All.OrderBy(c => c.Id)
        .Select(c => c.ContactName);
      var expected = Customers.OrderBy(c => c.Id)
        .Select(c => c.ContactName);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void ThenByTest()
    {
      var result = Query<Customer>.All.OrderBy(c => c.Address.Country)
        .ThenBy(c => c.Id).Select(c => c.Address.City);
      var expected = Customers.OrderBy(c => c.Address.Country)
        .ThenBy(c => c.Id).Select(c => c.Address.City);
      Assert.IsTrue(expected.SequenceEqual(result));
    }
  }
}