// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class ContainsAnyAllTest : NorthwindDOModelTest
  {
    [Test]
    public void AnyWithSubqueryTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => Query<Order>.All.Where(o => o.Customer == c).Any(o => o.Freight > 0));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void AnyWithSubqueryNoPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => Query<Order>.All.Where(o => o.Customer == c).Any());
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void AnyWithLocalCollectionTest()
    {
      var ids = new[] { "ABCDE", "ALFKI" };
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => ids.Any(id => c.Id == id));
        var list = result.ToList();
        Assert.Greater(list.Count , 0);
        t.Complete();
      }
    }

    [Test]
    public void AnyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Any();
        Assert.IsTrue(result);
        t.Complete();
      }
    }

    [Test]
    public void AllWithSubqueryTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => Query<Order>.All.Where(o => o.Customer == c).All(o => o.Freight > 0));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void AllWithLocalCollectionTest()
    {
      var patterns = new[] { "a", "e" };
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => patterns.All(p => c.ContactName.Contains(p)));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void AllTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.All(c => c.ContactName.StartsWith("a"));
        Assert.IsFalse(result);
        t.Complete();
      }
    }

    [Test]
    public void ContainsWithSubqueryTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => Query<Order>.All.Select(o => o.Customer).Contains(c));
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }

    [Test]
    public void ContainsWithLocalCollectionTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customerIDs = new[] {"ALFKI", "ANATR", "AROUT", "BERGS"};
          var orders = Query<Order>.All;
          var order = orders.Where(o => customerIDs.Contains(o.Customer.Id)).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void ContainsTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Select(c => c.Id).Contains("ALFKI");
        Assert.IsTrue(result);
        t.Complete();
      }
    }

    [Test]
    public void AllAndNotAllTest()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var result =
          from o in Query<Order>.All
          where Query<Customer>.All.Where(c => c==o.Customer).All(c => c.CompanyName.StartsWith("A"))
             && !Query<Employee>.All.Where(e => e==o.Employee).All(e => e.FirstName.EndsWith("t"))
          select o;
        var list = result.ToList();
        Assert.AreEqual(list.Count, 11);
      }
    }

    [Test]
    public void AllOrAllTest()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var result =
          from o in Query<Order>.All
          where Query<Customer>.All.Where(c => c == o.Customer).All(c => c.CompanyName.StartsWith("A"))
             || Query<Employee>.All.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
          select o;
        var list = result.ToList();
        Assert.AreEqual(list.Count, 366);
      }      
    }

    [Test]
    public void NotAnyAndAnyTest()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var result =
          from o in Query<Order>.All
          where !Query<Customer>.All.Where(c => c == o.Customer).Any(c => c.CompanyName.StartsWith("A"))
             && Query<Employee>.All.Where(e => e == o.Employee).Any(e => e.FirstName.EndsWith("t"))
          select o;
        var list = result.ToList();
        Assert.AreEqual(list.Count, 336);
      }      
    }

    [Test]
    public void AnyOrAnyTest()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var result =
          from o in Query<Order>.All
          where Query<Customer>.All.Where(c => c == o.Customer).Any(c => c.CompanyName.StartsWith("A"))
             || Query<Employee>.All.Where(e => e == o.Employee).Any(e => e.FirstName.EndsWith("t"))
          select o;
        var list = result.ToList();
        Assert.AreEqual(list.Count, 366);
      }
    }

    [Test]
    public void AnyAndNotAllTest()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var result =
          from o in Query<Order>.All
          where Query<Customer>.All.Where(c => c == o.Customer).Any(c => c.CompanyName.StartsWith("A"))
             && !Query<Employee>.All.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
          select o;
        var list = result.ToList();
        Assert.AreEqual(list.Count, 11);
      }      
    }

    [Test]
    public void NotAnyOrAllTest()
    {
      using (Domain.OpenSession())
      using (Transaction.Open()) {
        var result =
          from o in Query<Order>.All
          where !Query<Customer>.All.Where(c => c == o.Customer).Any(c => c.CompanyName.StartsWith("A"))
             || Query<Employee>.All.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
          select o;
        var list = result.ToList();
        Assert.AreEqual(list.Count, 819);
      }
    }
  }
}