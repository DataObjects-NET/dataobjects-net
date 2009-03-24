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
  [TestFixture]
  public class OrderByTest : NorthwindDOModelTest
  {
    [Test]
    public void OrderByPersistentPropertyTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var customers = Query<Customer>.All;
          var original = customers.Select(c => c.ContactName).ToList();
          original.Sort();

          Assert.IsTrue(original.SequenceEqual(
            customers
            .OrderBy(c => c.ContactName)
            .AsEnumerable()
            .Select(c => c.ContactName)));
        }
      }
    }

    [Test]
    public void OrderByExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var customers = Query<Customer>.All;
          var original = customers.Select(c => c.ContactName).AsEnumerable().Select(s =>s.ToUpper()).ToList();
          original.Sort();
          Assert.IsTrue(original.SequenceEqual(
            customers
            .OrderBy(c => c.ContactName.ToUpper())
            .AsEnumerable()
            .Select(c => c.ContactName.ToUpper())));
        }
      }
    }

    [Test]
    public void SelectTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customers = Query<Customer>.All;
        var result = Query<Customer>.All.OrderBy(c => c.CompanyName).Select(c => c.ContactName);
        var list = result.ToList();
        t.Complete();
      }
    }

    [Test]
    public void OrderBySelectTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderBy(c => c.CompanyName).OrderBy(c => c.Address.Country).Select(c => c.Address.City);
        var list = result.ToList();
        t.Complete();
      }
    }

    [Test]
    public void ThenByTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderBy(c => c.CompanyName).ThenBy(c => c.Address.Country).Select(c => c.Address.City);
        var list = result.ToList();
        t.Complete();
      }
    }

    [Test]
    public void OrderByDescendingTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.OrderByDescending(c => c.CompanyName).ThenByDescending(c => c.Address.Country).Select(c => c.Address.City);
        var list = result.ToList();
        t.Complete();
      }
    }

    [Test]
    public void OrderByJoinTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All.OrderBy(c => c.ContactName)
          join o in Query<Order>.All.OrderBy(o => o.OrderDate) on c equals o.Customer
          select new {c.ContactName, o.OrderDate};
        var list = result.ToList();

        t.Complete();
      }
    }

    [Test]
    public void OrderBySelectManyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All.OrderBy(c => c.ContactName)
          from o in Query<Order>.All.OrderBy(o => o.OrderDate)
          where c == o.Customer
          select new { c.ContactName, o.OrderDate };
        var list = result.ToList();
        t.Complete();
      }
    }
  }
}