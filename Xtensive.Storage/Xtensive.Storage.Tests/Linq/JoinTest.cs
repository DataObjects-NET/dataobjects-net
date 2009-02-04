// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.17

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class JoinTest : NorthwindDOModelTest
  {
    [Test]
    public void OneToOneTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var result = from p in products
                       join s in suppliers on p.Supplier.Id equals s.Id
                       select s.Phone;
          var list = result.ToList();
//          Assert.AreEqual(1000, list.Count);
          t.Complete();
        }
      }
    }

    [Test]
    public void MultipleTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var categories = Session.Current.All<Category>();
          var result = from p in products
                       join s in suppliers on p.Supplier.Id equals s.Id
                       join c in categories on p.Category.Id equals c.Id
                       select new { p, s, c.CategoryName };
          var list = result.ToList();
//          Assert.AreEqual(1000, list.Count);
          t.Complete();
        }
      }
    }

    [Test]
    public void SelectManyTest()
    {
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          var products = Session.Current.All<Product>();
          var suppliers = Session.Current.All<Supplier>();
          var categories = Session.Current.All<Category>();
          var result = from p in products
                       from s in suppliers
                       from c in categories
                       where p.Supplier == s && p.Category == c
                       select new { p, s, c.CategoryName };
          var list = result.ToList();
          Assert.AreEqual(1000, list.Count);
          t.Complete();
        }
      }
    }

    /*public void TestSelectManyJoined()
            {
                TestQuery(
                    from c in db.Customers
                    from o in db.Orders.Where(o => o.CustomerID == c.CustomerID)
                    select new { c.ContactName, o.OrderDate }
                    );
            }

            public void TestSelectManyJoinedDefaultIfEmpty()
            {
                TestQuery(
                    from c in db.Customers
                    from o in db.Orders.Where(o => o.CustomerID == c.CustomerID).DefaultIfEmpty()
                    select new { c.ContactName, o.OrderDate }
                    );
            }
    */


  }
}