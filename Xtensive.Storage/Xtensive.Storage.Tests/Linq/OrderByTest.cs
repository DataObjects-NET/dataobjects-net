// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.29

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
          var contacts = Query<Customer>.All;
          var original = contacts.Select(c => c.ContactName).ToList();
          Assert.Greater(original.Count, 0);
          original.Sort();

          var test = new List<string>(original.Count);
          foreach (var item in contacts.OrderBy(c => c.ContactName))
            test.Add(item.ContactName);

          Assert.IsTrue(original.SequenceEqual(test));
        }
      }
    }

    [Test]
    public void OrderByExpressionTest()
    {
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var contacts = Query<Customer>.All;
          var original = contacts.Select(c => c.ContactName).AsEnumerable().Select(s =>s.ToUpper()).ToList();
          Assert.Greater(original.Count, 0);
          original.Sort();

          var test = new List<string>(original.Count);
          foreach (var item in contacts.OrderBy(c => c.ContactName.ToUpper()))
            test.Add(item.ContactName.ToUpper());

          Assert.IsTrue(original.SequenceEqual(test));
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