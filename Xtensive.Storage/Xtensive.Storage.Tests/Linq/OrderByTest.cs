// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.29

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class OrderByTest : NorthwindDOModelTest
  {
    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var contacts = Enumerable.OrderBy(Session.Current.All<Customer>(), s => s.ContactName);
          foreach (var item in contacts)
            Console.WriteLine(item.ContactName);
          foreach (var item in contacts.OrderByDescending(s => s.ContactName))
            Console.WriteLine(item.ContactName);
        }
      }
    }

    /*public void TestOrderBy()
            {
                TestQuery(
                    db.Customers.OrderBy(c => c.CustomerID)
                    );
            }

            public void TestOrderBySelect()
            {
                TestQuery(
                    db.Customers.OrderBy(c => c.CustomerID).Select(c => c.ContactName)
                    );
            }

            public void TestOrderByOrderBy()
            {
                TestQuery(
                    db.Customers.OrderBy(c => c.CustomerID).OrderBy(c => c.Country).Select(c => c.City)
                    );
            }

            public void TestOrderByThenBy()
            {
                TestQuery(
                    db.Customers.OrderBy(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City)
                    );
            }

            public void TestOrderByDescending()
            {
                TestQuery(
                    db.Customers.OrderByDescending(c => c.CustomerID).Select(c => c.City)
                    );
            }

            public void TestOrderByDescendingThenBy()
            {
                TestQuery(
                    db.Customers.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City)
                    );
            }

            public void TestOrderByDescendingThenByDescending()
            {
                TestQuery(
                    db.Customers.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).Select(c => c.City)
                    );
            }

            public void TestOrderByJoin()
            {
                TestQuery(
                    from c in db.Customers.OrderBy(c => c.CustomerID)
                    join o in db.Orders.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                    select new { c.CustomerID, o.OrderID }
                    );
            }

            public void TestOrderBySelectMany()
            {
                TestQuery(
                    from c in db.Customers.OrderBy(c => c.CustomerID)
                    from o in db.Orders.OrderBy(o => o.OrderID)
                    where c.CustomerID == o.CustomerID
                    select new { c.ContactName, o.OrderID }
                    );

            }*/

  }
}