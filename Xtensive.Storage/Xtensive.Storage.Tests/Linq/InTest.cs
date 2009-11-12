// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class InTest : NorthwindDOModelTest
  {
    [Test]
    public void StringContainsTest()
    {
      var list = new List<string> {"FISSA", "PARIS"};
      var query1 = from c in Query<Customer>.All
      where !c.Id.In(list)
      select c.Orders;
      var query2 = from c in Query<Customer>.All
      where !c.Id.In("FISSA", "PARIS")
      select c.Orders;
      var expected1 = from c in Query<Customer>.All.AsEnumerable()
      where !list.Contains(c.Id)
      select c.Orders;
      var expected2 = from c in Query<Customer>.All.AsEnumerable()
      where !c.Id.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected1.Except(query1).Count());
      Assert.AreEqual(0, expected2.Except(query1).Count());
      Assert.AreEqual(0, expected1.Except(query2).Count());
      Assert.AreEqual(0, expected2.Except(query2).Count());
      QueryDumper.Dump(query1);
      QueryDumper.Dump(query2);
    }

    [Test]
    public void IntAndDecimalContains1Test()
    {
      // casts int to decimal
      var list = new List<int> {7, 22, 46};
      var query = from order in Query<Order>.All
      where !((int) order.Freight).In(list)
      select order;
      var expected = from order in Query<Order>.All.AsEnumerable()
      where !((int) order.Freight).In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleIntContainsTest()
    {
      var list = new List<int> {7, 22, 46};
      var query = from order in Query<Order>.All
      where order.Id.In(list)
      select order;
      var expected = from order in Query<Order>.All.AsEnumerable()
      where order.Id.In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ReuseIntContainsTest()
    {
      var list = new List<int> {7, 22};
      var orders = GetOrders(list);
      foreach (var order in orders)
        Assert.IsTrue(order.Id.In(list));

      list = new List<int> {46};
      orders = GetOrders(list);
      foreach (var order in orders)
        Assert.IsTrue(order.Id.In(list));
    }

    [Test]
    public void IntAndDecimalContains2Test()
    {
      // casts decimal to int
      var list = new List<decimal> {7, 22, 46};
      var query = from order in Query<Order>.All
      where !((decimal) order.Id).In(list)
      select order;
      var expected = from order in Query<Order>.All.AsEnumerable()
      where !((decimal) order.Id).In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void EntityContainsTest()
    {
      var list = Query<Customer>.All.Take(5).ToList();
      var query = from c in Query<Customer>.All
      where !c.In(list)
      select c.Orders;
      var expected = from c in Query<Customer>.All.AsEnumerable()
      where !c.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void StructContainsTest()
    {
      var list = Query<Customer>.All.Take(5).Select(c => c.Address).ToList();
      var query = from c in Query<Customer>.All
      where !c.Address.In(list)
      select c.Orders;
      var expected = from c in Query<Customer>.All.AsEnumerable()
      where !c.Address.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void StructSimpleContainsTest()
    {
      var list = Query<Customer>.All.Take(5).Select(c => c.Address).ToList();
      var query = Query<Customer>.All.Select(c => c.Address).Where(c => c.In(list));
      QueryDumper.Dump(query);
    }

    [Test]
    public void AnonimousContainsTest()
    {
      var list = new[] {new {Id = "FISSA"}, new {Id = "PARIS"}};
      var query = Query<Customer>.All.Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      var expected = Query<Customer>.All.AsEnumerable().Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableDoubleContainsTest()
    {
      var list = Query<Order>.All
        .Select(o => o.Freight)
        .Distinct()
        .Take(10);
      var query = from order in Query<Order>.All
      where !order.Freight.In(list)
      select order;
      var expected = from order in Query<Order>.All.AsEnumerable()
      where !order.Freight.In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableEntityContainsTest()
    {
      var list = Query<Customer>.All.Take(5);
      var query = from c in Query<Customer>.All
      where !c.In(list)
      select c.Orders;
      var expected = from c in Query<Customer>.All.AsEnumerable()
      where !c.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableStructContainsTest()
    {
      var list = Query<Customer>.All
        .Take(5)
        .Select(c => c.Address);
      var query = from c in Query<Customer>.All
      where !c.Address.In(list)
      select c.Orders;
      var expected = from c in Query<Customer>.All.AsEnumerable()
      where !c.Address.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableAnonimousContainsTest()
    {
      var list = Query<Customer>.All.Take(10).Select(c => new {c.Id});
      var query = Query<Customer>.All.Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      var expected = Query<Customer>.All.AsEnumerable().Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    private IEnumerable<Order> GetOrders(IEnumerable<int> ids)
    {
      return Query.Execute(() =>
        from order in Query<Order>.All
        where order.Id.In(ids)
        select order);
    }
  }
}