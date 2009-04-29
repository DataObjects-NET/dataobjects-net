// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.29

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
    public void OrderByAnonymousEntityTest()
    {
      var result = Query<OrderDetails>.All
        .Select(od => new {Details = od, Order = od.Order})
        .OrderBy(x => new {x, x.Order.Customer})
        .Select(x =>  new {x, x.Order.Customer});
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByAnonymousTest()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      var result = customers
        .OrderBy(c => new {c.Fax, c.Phone})
        .Select(c => new {c.Fax, c.Phone});
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByAnonymous2Test()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      var result = customers
        .Select(c => new {c.Address.Country, c})
        .OrderBy(x => x);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByDescendingTest()
    {
      IQueryable<string> result = Query<Customer>.All.OrderByDescending(c => c.CompanyName).ThenByDescending(c => c.Address.Country).Select(c => c.Address.City);
      List<string> list = result.ToList();
    }

    [Test]
    public void OrderByEntityTest()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      IOrderedQueryable<Customer> result = customers.OrderBy(c => c);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByExpressionTest()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      List<string> original = customers.Select(c => c.ContactName).AsEnumerable().Select(s => s.ToUpper()).ToList();
      original.Sort();
      Assert.IsTrue(original.SequenceEqual(
        customers
          .OrderBy(c => c.ContactName.ToUpper())
          .AsEnumerable()
          .Select(c => c.ContactName.ToUpper())));
    }

    [Test]
    public void OrderByJoinTest()
    {
      var result =
        from c in Query<Customer>.All.OrderBy(c => c.ContactName)
        join o in Query<Order>.All.OrderBy(o => o.OrderDate) on c equals o.Customer
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
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
          .AsEnumerable()
          .Select(c => c.ContactName)));
    }

    [Test]
    public void OrderBySelectAnonymousTest()
    {
      IQueryable<Customer> customers = Query<Customer>.All;
      var result = customers
        .OrderBy(c => c.Phone)
        .Select(c => new {c.Fax, c.Phone});
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderBySelectManyTest()
    {
      var result =
        from c in Query<Customer>.All.OrderBy(c => c.ContactName)
        from o in Query<Order>.All.OrderBy(o => o.OrderDate)
        where c == o.Customer
        select new {c.ContactName, o.OrderDate};
      var list = result.ToList();
    }

    [Test]
    public void OrderBySelectTest()
    {
      IQueryable<string> result = Query<Customer>.All.OrderBy(c => c.CompanyName).OrderBy(c => c.Address.Country).Select(c => c.Address.City);
      List<string> list = result.ToList();
    }

    [Test]
    public void DistinctTest()
    {
      var result = Query<Order>.All.Select(o => o.Employee).Distinct();
      var expected = Query<Order>.All.AsEnumerable().Select(o => o.Employee).Distinct();
      var i = 0;
      Assert.IsTrue(result.AsEnumerable().SequenceEqual(expected));
    }

    [Test]
    public void OrderByTakeSkipTest()
    {
      IEnumerable<Order> original = Query<Order>.All.AsEnumerable()
        .OrderBy(o => o.OrderDate)
        .Skip(100)
        .Take(50)
        .OrderBy(o => o.RequiredDate)
        .Where(o => o.OrderDate != null)
        .Skip(10);
      IQueryable<Order> result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .Skip(100)
        .Take(50)
        .OrderBy(o => o.RequiredDate)
        .Where(o => o.OrderDate != null)
        .Skip(10);
      List<Order> originalList = original.ToList();
      List<Order> resultList = result.ToList();
      QueryDumper.Dump(originalList);
      QueryDumper.Dump(resultList);
      Assert.AreEqual(originalList.Count, resultList.Count);
      Assert.IsTrue(originalList.SequenceEqual(resultList));
    }

    [Test]
    public void PredicateTest()
    {
      IQueryable<int> result = Query<Order>.All.OrderBy(o => o.Freight > 0 && o.ShippedDate != null).ThenBy(o => o.Id).Select(o => o.Id);
      List<int> list = result.ToList();
      List<int> original = Query<Order>.All.AsEnumerable().OrderBy(o => o.Freight > 0 && o.ShippedDate != null).ThenBy(o => o.Id).Select(o => o.Id).ToList();
      Assert.IsTrue(list.SequenceEqual(original));
    }

    [Test]
    public void SelectTest()
    {
      IQueryable<string> result = Query<Customer>.All.OrderBy(c => c.CompanyName).Select(c => c.ContactName);
      List<string> list = result.ToList();
    }

    [Test]
    public void ThenByTest()
    {
      IQueryable<string> result = Query<Customer>.All.OrderBy(c => c.CompanyName).ThenBy(c => c.Address.Country).Select(c => c.Address.City);
      List<string> list = result.ToList();
    }
  }
}