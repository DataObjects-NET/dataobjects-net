// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using NUnit.Framework;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  [Ignore("Not implemented")]
  public class GroupByTest : NorthwindDOModelTest
  {
    /*public void TestGroupBy()
        {
            TestQuery(
                db.Customers.GroupBy(c => c.City)
                );
        }

        public void TestGroupBySelectMany()
        {
            TestQuery(
                db.Customers.GroupBy(c => c.City).SelectMany(g => g)
                );
        }

        public void TestGroupBySum()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID))
                );
        }

        public void TestGroupByCount()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g => g.Count())
                );
        }

        public void TestGroupBySumMinMaxAvg()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Select(g =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    })
                );
        }

        public void TestGroupByWithResultSelector()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, (k, g) =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = g.Average(o => o.OrderID)
                    })
                );
        }

        public void TestGroupByWithElementSelectorSum()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum())
                );
        }

        public void TestGroupByWithElementSelector()
        {
            // note: groups are retrieved through a separately execute subquery per row
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID)
                );
        }

        public void TestGroupByWithElementSelectorSumMax()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => new { Sum = g.Sum(), Max = g.Max() })
                );
        }

        public void TestGroupByWithAnonymousElement()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID, o => new { o.OrderID }).Select(g => g.Sum(x => x.OrderID))
                );
        }

        public void TestGroupByWithTwoPartKey()
        {
            TestQuery(
                db.Orders.GroupBy(o => new { o.CustomerID, o.OrderDate }).Select(g => g.Sum(o => o.OrderID))
                );
        }

        public void TestOrderByGroupBy()
        {
            // note: order-by is lost when group-by is applied (the sequence of groups is not ordered)
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID))
                );
        }

        public void TestOrderByGroupBySelectMany()
        {
            // note: order-by is preserved within grouped sub-collections
            TestQuery(
                db.Orders.OrderBy(o => o.OrderID).GroupBy(o => o.CustomerID).SelectMany(g => g)
                );
        }
*/
  }
}