// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using DataObjects.NET;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Linq;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture, Category("Linq")]
  public class PrefetchTest: NorthwindDOModelTest
  {
    [Test]
    public void EntitySimpleSetTest()
    {
      var result = Query<Customer>.All.Prefetch(c=>c.Orders);
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySimpleTest()
    {
      var result = Query<Order>.All.Prefetch(o=>o.ShipVia);
      QueryDumper.Dump(result);
    }

    [Test]
    public void WrongSubqueryTest()
    {
      // Must fail.
      var result = Query<Order>.All.Prefetch(o=>Query<Shipper>.All.Where(s=>s==o.ShipVia));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Query<Order>.All
        .Select(o=>new{o, Shippers =  Query<Shipper>.All.Where(s=>s==o.ShipVia)})
        .Prefetch(x=>x.Shippers);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryEntityTest()
    {
      var result = Query<Order>.All
        .Select(o=>new{o, Shipper =  Query<Shipper>.All.FirstOrDefault()})
        .Prefetch(x=>x.Shipper);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryInternalEntitySetTest()
    {
      var result = Query<Order>.All
        .Select(o => new {
          Customers = Query<Customer>.All.Where(c => c==o.Customer).Prefetch(customer=>customer.Orders),
        });
      var list = result.ToList();
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryInternalEntityTest()
    {
      var result = Query<Order>.All
        .Select(o => new {
          Employees = Query<Employee>.All.Where(e => e==o.Employee).Prefetch(employee=>employee.ReportsTo)
        });
      var list = result.ToList();
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryComplexTest()
    {
      var result = Query<Order>.All
        .Select(o => new {
          Customers = Query<Customer>.All.Where(c => c==o.Customer).Prefetch(customer=>customer.Orders),
          Employees = Query<Employee>.All.Where(e => e==o.Employee).Prefetch(employee=>employee.ReportsTo)
        })
        .Prefetch(a=>a.Customers)
        .Prefetch(a=>a.Employees);
      var list = result.ToList();
      QueryDumper.Dump(result);
    }
  }
}