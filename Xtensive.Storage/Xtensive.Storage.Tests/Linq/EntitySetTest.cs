// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class EntitySetTest : NorthwindDOModelTest
  {
    [Test]
    public void QueryTest()
    {
      var customer = GetCustomer();
      var expected = customer.Orders.AsEnumerable().OrderBy(o => o.Id).Select(o => o.Id).ToList();
      var actual = customer.Orders.OrderBy(o => o.Id).Select(o => o.Id).ToList();
      Assert.IsTrue(expected.SequenceEqual(actual));
    }

    [Test]
    public void UnsupportedMethodsTest()
    {
      AssertEx.ThrowsNotSupportedException(() => Query<Customer>.All.Where(c => c.Orders.Add(null)).ToList());
      AssertEx.ThrowsNotSupportedException(() => Query<Customer>.All.Where(c => c.Orders.Remove(null)).ToList());
    }

    [Test]
    public void CountTest()
    {
      var expected = Query<Order>.All.Count();
      var count = Query<Customer>.All.Select(c => c.Orders.Count).AsEnumerable().Sum();
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void ContainsTest()
    {
      var bestOrder = Query<Order>.All.OrderBy(o => o.Freight).First();
      var result = Query<Customer>.All.Where(c => c.Orders.Contains(bestOrder));
      Assert.AreEqual(bestOrder.Customer.Id, result.ToList().Single().Id);
    }

    [Test]
    public void OuterEntitySetTest()
    {
      var customer = GetCustomer();
      var result = Query<Order>.All.Where(o => customer.Orders.Contains(o));
      Assert.AreEqual(customer.Orders.Count, result.ToList().Count);
    }

    [Test]
    public void JoinWithEntitySetTest()
    {
      var customer = GetCustomer();
      var result =
        from o in customer.Orders
        join e in Query<Employee>.All on o.Employee equals e
        select e;
      Assert.AreEqual(customer.Orders.Count, result.ToList().Count);
     }

    private static Customer GetCustomer()
    {
      return Query<Customer>.All.Where(c => c.Id=="LACOR").Single();
    }
  }
}
