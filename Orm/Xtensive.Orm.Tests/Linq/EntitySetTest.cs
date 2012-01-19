// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class EntitySetTest : NorthwindDOModelTest
  {
    [Test]
    public void EntitySetAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {OrdersFiled = c.Orders});
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(c => new {OrdersFiled = c.Orders});
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySetSelectManyAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {OrdersFiled = c.Orders})
        .SelectMany(i => i.OrdersFiled);
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(c => new {OrdersFiled = c.Orders})
        .SelectMany(i => i.OrdersFiled);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntitySetSelectTest()
    {
      var result = Session.Query.All<Customer>().OrderBy(c=>c.Id).Select(c => c.Orders).ToList();
      var expected = Session.Query.All<Customer>().AsEnumerable().OrderBy(c=>c.Id).Select(c => c.Orders).ToList();
      Assert.Greater(result.Count, 0);
      Assert.AreEqual(expected.Count, result.Count);
      for (int i = 0; i < result.Count; i++)
        Assert.AreSame(expected[i], result[i]);
    }

    [Test]
    public void QueryTest()
    {
      var customer = GetCustomer();
      var expected = customer
        .Orders
        .ToList()
        .OrderBy(o => o.Id)
        .Select(o => o.Id)
        .ToList();
      var actual = customer
        .Orders
        .OrderBy(o => o.Id)
        .Select(o => o.Id)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void UnsupportedMethodsTest()
    {
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Customer>().Where(c => c.Orders.Add(null)).ToList());
      AssertEx.Throws<QueryTranslationException>(() => Session.Query.All<Customer>().Where(c => c.Orders.Remove(null)).ToList());
    }

    [Test]
    public void CountTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var expected = Session.Query.All<Order>().Count();
      var count = Session.Query.All<Customer>()
        .Select(c => c.Orders.Count)
        .ToList()
        .Sum();
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void ContainsTest()
    {
      var bestOrder = Session.Query.All<Order>()
        .OrderBy(o => o.Freight)
        .First();
      var result = Session.Query.All<Customer>()
        .Where(c => c.Orders.Contains(bestOrder));
      Assert.AreEqual(bestOrder.Customer.Id, result.ToList().Single().Id);
    }

    [Test]
    public void OuterEntitySetTest()
    {
      var customer = GetCustomer();
      var result = Session.Query.All<Order>().Where(o => customer.Orders.Contains(o));
      Assert.AreEqual(customer.Orders.Count, result.ToList().Count);
    }

    [Test]
    public void JoinWithEntitySetTest()
    {
      var customer = GetCustomer();
      var result =
        from o in customer.Orders
        join e in Session.Query.All<Employee>() on o.Employee equals e
        select e;
      Assert.AreEqual(customer.Orders.Count, result.ToList().Count);
    }

    private static Customer GetCustomer()
    {
      return Session.Demand().Query.All<Customer>().Where(c => c.Id=="LACOR").Single();
    }
  }
}