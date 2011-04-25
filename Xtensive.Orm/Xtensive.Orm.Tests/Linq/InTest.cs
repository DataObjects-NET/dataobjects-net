// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class InTest : NorthwindDOModelTest
  {
    [Test]
    public void StringContainsTest()
    {
      var list = new List<string> {"FISSA", "PARIS"};
      var query1 = from c in Session.Query.All<Customer>()
      where !c.Id.In(list)
      select c.Orders;
      var query2 = from c in Session.Query.All<Customer>()
      where !c.Id.In("FISSA", "PARIS")
      select c.Orders;
      var expected1 = from c in Session.Query.All<Customer>().AsEnumerable()
      where !list.Contains(c.Id)
      select c.Orders;
      var expected2 = from c in Session.Query.All<Customer>().AsEnumerable()
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
    public void LongSequenceIntTest()
    {
      // Wrong JOIN mapping for temptable version of .In
      var list1 = new List<decimal?> {7, 22, 46};
      var list2 = new List<decimal?>();
      for (int i = 0; i < 100; i++) 
        list2.AddRange(list1);
      var query1 = from order in Session.Query.All<Order>()
      where (order.Freight).In(list1)
      select order;
      var query2 = from order in Session.Query.All<Order>()
      where (order.Freight).In(list2)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
      where (order.Freight).In(list1)
      select order;
      Assert.AreEqual(0, expected.Except(query1).Count());
      Assert.AreEqual(0, expected.Except(query2).Count());
    }

    [Test]
    public void IntAndDecimalContains1Test()
    {
      // casts int to decimal
      var list = new List<int> {7, 22, 46};
      var query = from order in Session.Query.All<Order>()
      where !((int) order.Freight).In(list)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
      where !((int) order.Freight).In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleIntContainsTest()
    {
      var list = new List<int> {7, 22, 46};
      var query = from order in Session.Query.All<Order>()
      where order.Id.In(list)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
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
      var query = from order in Session.Query.All<Order>()
      where !((decimal) order.Id).In(list)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
      where !((decimal) order.Id).In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void EntityContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5).ToList();
      var query = from c in Session.Query.All<Customer>()
      where !c.In(list)
      select c.Orders;
      var expected = from c in Session.Query.All<Customer>().AsEnumerable()
      where !c.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void StructContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5).Select(c => c.Address).ToList();
      var query = from c in Session.Query.All<Customer>()
      where !c.Address.In(list)
      select c.Orders;
      var expected = from c in Session.Query.All<Customer>().AsEnumerable()
      where !c.Address.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void StructSimpleContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5).Select(c => c.Address).ToList();
      var query = Session.Query.All<Customer>().Select(c => c.Address).Where(c => c.In(list));
      QueryDumper.Dump(query);
    }

    [Test]
    public void AnonimousContainsTest()
    {
      var list = new[] {new {Id = "FISSA"}, new {Id = "PARIS"}};
      var query = Session.Query.All<Customer>().Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      var expected = Session.Query.All<Customer>().AsEnumerable().Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void AnonimousContains2Test()
    {
      var list = new[] {new {Id1 = "FISSA", Id2 = "FISSA"}, new {Id1 = "PARIS", Id2 = "PARIS"}};
      var query = Session.Query.All<Customer>().Where(c => new {Id1 = c.Id, Id2 = c.Id}.In(list)).Select(c => c.Orders);
      var expected = Session.Query.All<Customer>().AsEnumerable().Where(c => new {Id1 = c.Id, Id2 = c.Id}.In(list)).Select(c => c.Orders);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableDoubleContainsTest()
    {
      var list = Session.Query.All<Order>()
        .Select(o => o.Freight)
        .Distinct()
        .Take(10);
      var query = from order in Session.Query.All<Order>()
      where !order.Freight.In(list)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
      where !order.Freight.In(list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableEntityContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5);
      var query = from c in Session.Query.All<Customer>()
      where !c.In(list)
      select c.Orders;
      var expected = from c in Session.Query.All<Customer>().AsEnumerable()
      where !c.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableStructContainsTest()
    {
      var list = Session.Query.All<Customer>()
        .Take(5)
        .Select(c => c.Address);
      var query = from c in Session.Query.All<Customer>()
      where !c.Address.In(list)
      select c.Orders;
      var expected = from c in Session.Query.All<Customer>().AsEnumerable()
      where !c.Address.In(list)
      select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableAnonimousContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(10).Select(c => new {c.Id});
      var query = Session.Query.All<Customer>().Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      var expected = Session.Query.All<Customer>().AsEnumerable().Where(c => new {c.Id}.In(list)).Select(c => c.Orders);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    private IEnumerable<Order> GetOrders(IEnumerable<int> ids)
    {
      return Session.Query.Execute(qe =>
        from order in qe.All<Order>()
        where order.Id.In(ids)
        select order);
    }

    [Test]
    public void ComplexCondition1Test()
    {
      var includeAlgorithm = IncludeAlgorithm.TemporaryTable;
      var list = new List<int> {7, 22, 46};
      var query = from order in Session.Query.All<Order>()
      where order.Id.In(includeAlgorithm, list)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
      where order.Id.In(includeAlgorithm, list)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ComplexCondition2Test()
    {
      var includeAlgorithm = IncludeAlgorithm.TemporaryTable;
      var query = from order in Session.Query.All<Order>()
      where order.Id.In(includeAlgorithm, 7, 22, 46)
      select order;
      var expected = from order in Session.Query.All<Order>().AsEnumerable()
      where order.Id.In(includeAlgorithm, 7, 22, 46)
      select order;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void CompiledInTest()
    {
      var result1 = GetCustomers("ANTON", "LACOR")
        .Select(customer => customer.Id)
        .ToList();
      Assert.AreEqual(2, result1.Count);
      Assert.IsTrue(result1.Contains("ANTON"));
      Assert.IsTrue(result1.Contains("LACOR"));

      var result2 = GetCustomers("BERGS")
        .Select(customer => customer.Id)
        .ToList();
      Assert.AreEqual(1, result2.Count);
      Assert.AreEqual("BERGS", result2[0]);
    }

    private static IEnumerable<Customer> GetCustomers(params string[] customerIds)
    {
      return Session.Demand().Query.Execute(qe => qe.All<Customer>().Where(customer => customer.Id.In(customerIds)));
    }
  }
}