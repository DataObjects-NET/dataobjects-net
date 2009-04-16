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
  public class NestedCollectionsTest : NorthwindDOModelTest
  {
    private int numberOfCustomers;
    private int numberOfOrders;
    private int numberOfEmployees;
    private int numberOfCustomersWithoutOrders;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        numberOfCustomers = Query<Customer>.All.Count();
        numberOfOrders = Query<Order>.All.Count();
        numberOfEmployees = Query<Employee>.All.Count();
        numberOfCustomersWithoutOrders = Query<Customer>.All.Where(c => c.Orders.Count==0).Count();
        t.Complete();
      }
    }

    [Test]
    public void SelectNestedTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All);
      Assert.AreEqual(numberOfCustomers * numberOfOrders, Count(result));
    }

    [Test]
    public void SelectDoubleNestedTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Select(o => Query<Employee>.All));
      Assert.AreEqual(numberOfCustomers * numberOfOrders * numberOfEmployees, Count(result));
    }

    [Test]
    public void SelectNestedWithCorrelationTest()
    {
      var result = Query<Customer>.All
        .Select(c => Query<Order>.All.Where(o => o.Customer == c));
      Assert.AreEqual(numberOfOrders, Count(result));
    }

    [Test]
    public void SelectAnonymousTest()
    {
      var result = Query<Customer>.All
        .Select(c => new {Customer = c, Orders = Query<Order>.All.Where(o => o.Customer == c)});
      foreach (var item in result)
        Assert.AreEqual(item.Customer.Orders.Count, item.Orders.Count());
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