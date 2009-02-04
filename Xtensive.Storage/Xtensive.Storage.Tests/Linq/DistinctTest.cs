// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

namespace Xtensive.Storage.Tests.Linq
{
  public class DistinctTest : NorthwindDOModelTest
  {
    /*public void TestDistinct()
        {
            TestQuery(
                db.Customers.Distinct()
                );
        }

        public void TestDistinctScalar()
        {
            TestQuery(
                db.Customers.Select(c => c.City).Distinct()
                );
        }

        public void TestOrderByDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct()
                );
        }

        public void TestDistinctOrderBy()
        {
            TestQuery(
                db.Customers.Select(c => c.City).Distinct().OrderBy(c => c)
                );
        }

        public void TestDistinctGroupBy()
        {
            TestQuery(
                db.Orders.Distinct().GroupBy(o => o.CustomerID)
                );
        }

        public void TestGroupByDistinct()
        {
            TestQuery(
                db.Orders.GroupBy(o => o.CustomerID).Distinct()
                );

        }

        public void TestDistinctCount()
        {
            TestQuery(
                () => db.Customers.Distinct().Count()
                );
        }

        public void TestSelectDistinctCount()
        {
            // cannot do: SELECT COUNT(DISTINCT some-colum) FROM some-table
            // because COUNT(DISTINCT some-column) does not count nulls
            TestQuery(
                () => db.Customers.Select(c => c.City).Distinct().Count()
                );
        }

        public void TestSelectSelectDistinctCount()
        {
            TestQuery(
                () => db.Customers.Select(c => c.City).Select(c => c).Distinct().Count()
                );
        }

        public void TestDistinctCountPredicate()
        {
            TestQuery(
                () => db.Customers.Distinct().Count(c => c.CustomerID == "ALFKI")
                );
        }

        public void TestDistinctSumWithArg()
        {
            TestQuery(
                () => db.Orders.Distinct().Sum(o => o.OrderID)
                );
        }

        public void TestSelectDistinctSum()
        {
            TestQuery(
                () => db.Orders.Select(o => o.OrderID).Distinct().Sum()
                );
        }

        public void TestTake()
        {
            TestQuery(
                db.Orders.Take(5)
                );
        }

        public void TestTakeDistinct()
        {
            // distinct must be forced to apply after top has been computed
            TestQuery(
                db.Orders.Take(5).Distinct()
                );
        }

        public void TestDistinctTake()
        {
            // top must be forced to apply after distinct has been computed
            TestQuery(
                db.Orders.Distinct().Take(5)
                );
        }

        public void TestDistinctTakeCount()
        {
            TestQuery(
                () => db.Orders.Distinct().Take(5).Count()
                );
        }

        public void TestTakeDistinctCount()
        {
            TestQuery(
                () => db.Orders.Take(5).Distinct().Count()
                );
        }

        public void TestSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5)
                );
        }

        public void TestSkipTake()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }

        public void TestTakeSkip()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5)
                );
        }

        public void TestSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Distinct()
                );
        }

        public void TestDistinctSkip()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5)
                );
        }

        public void TestSkipTakeDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Skip(5).Take(10).Distinct()
                );
        }

        public void TestTakeSkipDistinct()
        {
            TestQuery(
                db.Customers.OrderBy(c => c.ContactName).Take(10).Skip(5).Distinct()
                );
        }

        public void TestDistinctSkipTake()
        {
            TestQuery(
                db.Customers.Distinct().OrderBy(c => c.ContactName).Skip(5).Take(10)
                );
        }
*/
  }
}