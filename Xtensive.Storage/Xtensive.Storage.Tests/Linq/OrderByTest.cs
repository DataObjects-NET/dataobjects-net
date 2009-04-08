// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.29

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class OrderByTest : NorthwindDOModelTest
  {
    [Test]
    public void OrderByPersistentPropertyTest()
    {
          var customers = Query<Customer>.All;
          var original = customers.Select(c => c.ContactName).ToList();
          original.Sort();

          Assert.IsTrue(original.SequenceEqual(
            customers
            .OrderBy(c => c.ContactName)
            .AsEnumerable()
            .Select(c => c.ContactName)));
    }

    [Test]
    public void OrderByExpressionTest()
    {
          var customers = Query<Customer>.All;
          var original = customers.Select(c => c.ContactName).AsEnumerable().Select(s =>s.ToUpper()).ToList();
          original.Sort();
          Assert.IsTrue(original.SequenceEqual(
            customers
            .OrderBy(c => c.ContactName.ToUpper())
            .AsEnumerable()
            .Select(c => c.ContactName.ToUpper())));
    }

    [Test]
    public void SelectTest()
    {
        var customers = Query<Customer>.All;
        var result = Query<Customer>.All.OrderBy(c => c.CompanyName).Select(c => c.ContactName);
        var list = result.ToList();
    }

    [Test]
    public void OrderBySelectTest()
    {
        var result = Query<Customer>.All.OrderBy(c => c.CompanyName).OrderBy(c => c.Address.Country).Select(c => c.Address.City);
        var list = result.ToList();
    }

    [Test]
    public void ThenByTest()
    {
        var result = Query<Customer>.All.OrderBy(c => c.CompanyName).ThenBy(c => c.Address.Country).Select(c => c.Address.City);
        var list = result.ToList();
    }

    [Test]
    public void OrderByDescendingTest()
    {
        var result = Query<Customer>.All.OrderByDescending(c => c.CompanyName).ThenByDescending(c => c.Address.Country).Select(c => c.Address.City);
        var list = result.ToList();
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
    public void OrderBySelectManyTest()
    {
        var result =
          from c in Query<Customer>.All.OrderBy(c => c.ContactName)
          from o in Query<Order>.All.OrderBy(o => o.OrderDate)
          where c == o.Customer
          select new { c.ContactName, o.OrderDate };
        var list = result.ToList();
    }

    [Test]
    public void PredicateTest()
    {
      var result = Query<Order>.All.OrderBy(o => o.Freight > 0 && o.ShippedDate != null).ThenBy(o => o.Id).Select(o => o.Id);
      var list = result.ToList();
      var original = Query<Order>.All.AsEnumerable().OrderBy(o => o.Freight > 0 && o.ShippedDate != null).ThenBy(o => o.Id).Select(o => o.Id).ToList();
      Assert.IsTrue(list.SequenceEqual(original));
    }

    [Test]
    public void OrderByTakeSkipTest()
    {
      var original = Query<Order>.All.AsEnumerable()
        .OrderBy(o => o.OrderDate)
        .Skip(100)
        .Take(50)
        .OrderBy(o => o.RequiredDate)
        .Where(o => o.OrderDate != null)
        .Skip(10);
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .Skip(100)
        .Take(50)
        .OrderBy(o => o.RequiredDate)
        .Where(o => o.OrderDate != null)
        .Skip(10);
      var originalList = original.ToList();
      var resultList = result.ToList();
      Assert.AreEqual(originalList.Count, resultList.Count);
      using (Log.DebugRegion("Dubugging order results.")) {
        foreach (var order in originalList) {
          Log.Info(order.ToString());
        }
        Log.Info("");
        foreach (var order in resultList) {
          Log.Info(order.ToString());
        }
        Assert.IsTrue(originalList.SequenceEqual(resultList));
      }
    }
  }
}