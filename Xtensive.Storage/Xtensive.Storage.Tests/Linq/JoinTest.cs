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
  [TestFixture]
  public class JoinTest : NorthwindDOModelTest
  {
    [Test]
    public void SingleTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var productsCount = products.Count();
          var suppliers = Query<Supplier>.All;
          var result = from p in products
                       join s in suppliers on p.Supplier.Id equals s.Id
                       select new {p.ProductName, s.ContactName, s.Phone};
          var list = result.ToList();
          Assert.AreEqual(productsCount, list.Count);
          t.Complete();
        }
      }
    }

    [Test]
    public void SeveralTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var productsCount = products.Count();
          var suppliers = Query<Supplier>.All;
          var categories = Query<Category>.All;
          var result = from p in products
                       join s in suppliers on p.Supplier.Id equals s.Id
                       join c in categories on p.Category.Id equals c.Id
                       select new { p, s, c.CategoryName };
          var list = result.ToList();
          Assert.AreEqual(productsCount, list.Count);
          t.Complete();
        }
      }
    }

    [Test]
    public void OneToManyTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var products = Query<Product>.All;
        var productsCount = products.Count();
        var suppliers = Query<Supplier>.All;
        var result =  from s in suppliers
                      join p in products on s.Id equals p.Supplier.Id
                      select new { p.ProductName, s.ContactName };
        var list = result.ToList();
        Assert.AreEqual(productsCount, list.Count);
        t.Complete();
      }
    }

    [Ignore("Not implemented.")]
    [Test]
    public void SelectManyTest()
    {
      using (Domain.OpenSession())
      {
        using (var t = Transaction.Open())
        {
          var products = Query<Product>.All;
          var suppliers = Query<Supplier>.All;
          var categories = Query<Category>.All;
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



    /*
     *  void GroupJoin()
        {
            // This is a demonstration query to show the output
            // of a "raw" group join. A more typical group join
            // is shown in the GroupInnerJoin method.
            var groupJoinQuery =
               from category in categories
               join prod in products on category.ID equals prod.CategoryID into prodGroup
               select prodGroup;

            // Store the count of total items (for demonstration only).
            int totalItems = 0;

            Console.WriteLine("Simple GroupJoin:");

            // A nested foreach statement is required to access group items.
            foreach (var prodGrouping in groupJoinQuery)
            {
                Console.WriteLine("Group:");
                foreach (var item in prodGrouping)
                {
                    totalItems++;
                    Console.WriteLine("   {0,-10}{1}", item.Name, item.CategoryID);
                }
            }
            Console.WriteLine("Unshaped GroupJoin: {0} items in {1} unnamed groups", totalItems, groupJoinQuery.Count());
            Console.WriteLine(System.Environment.NewLine);
        }

        void GroupInnerJoin()
        {
            var groupJoinQuery2 =
                from category in categories
                orderby category.ID
                join prod in products on category.ID equals prod.CategoryID into prodGroup
                select new
                {
                    Category = category.Name,
                    Products = from prod2 in prodGroup
                               orderby prod2.Name
                               select prod2
                };

            //Console.WriteLine("GroupInnerJoin:");
            int totalItems = 0;

            Console.WriteLine("GroupInnerJoin:");
            foreach (var productGroup in groupJoinQuery2)
            {
                Console.WriteLine(productGroup.Category);
                foreach (var prodItem in productGroup.Products)
                {
                    totalItems++;
                    Console.WriteLine("  {0,-10} {1}", prodItem.Name, prodItem.CategoryID);
                }
            }
            Console.WriteLine("GroupInnerJoin: {0} items in {1} named groups", totalItems, groupJoinQuery2.Count());
            Console.WriteLine(System.Environment.NewLine);
        }

        void GroupJoin3()
        {

            var groupJoinQuery3 =
                from category in categories
                join product in products on category.ID equals product.CategoryID into prodGroup
                from prod in prodGroup
                orderby prod.CategoryID
                select new { Category = prod.CategoryID, ProductName = prod.Name };

            //Console.WriteLine("GroupInnerJoin:");
            int totalItems = 0;

            Console.WriteLine("GroupJoin3:");
            foreach (var item in groupJoinQuery3)
            {
                totalItems++;
                Console.WriteLine("   {0}:{1}", item.ProductName, item.Category);
            }

            Console.WriteLine("GroupJoin3: {0} items in 1 group", totalItems, groupJoinQuery3.Count());
            Console.WriteLine(System.Environment.NewLine);
        }

        void LeftOuterJoin()
        {
            // Create the query.
            var leftOuterQuery =
               from category in categories
               join prod in products on category.ID equals prod.CategoryID into prodGroup
               select prodGroup.DefaultIfEmpty(new Product() { Name = "Nothing!", CategoryID = category.ID });

            // Store the count of total items (for demonstration only).
            int totalItems = 0;

            Console.WriteLine("Left Outer Join:");

            // A nested foreach statement  is required to access group items
            foreach (var prodGrouping in leftOuterQuery)
            {
                Console.WriteLine("Group:", prodGrouping.Count());
                foreach (var item in prodGrouping)
                {
                    totalItems++;
                    Console.WriteLine("  {0,-10}{1}", item.Name, item.CategoryID);
                }
            }
            Console.WriteLine("LeftOuterJoin: {0} items in {1} groups", totalItems, leftOuterQuery.Count());
            Console.WriteLine(System.Environment.NewLine);
        }

        void LeftOuterJoin2()
        {
            // Create the query.
            var leftOuterQuery2 =
               from category in categories
               join prod in products on category.ID equals prod.CategoryID into prodGroup
               from item in prodGroup.DefaultIfEmpty()
               select new { Name = item == null ? "Nothing!" : item.Name, CategoryID = category.ID };

            Console.WriteLine("LeftOuterJoin2: {0} items in 1 group", leftOuterQuery2.Count());
            // Store the count of total items
            int totalItems = 0;

            Console.WriteLine("Left Outer Join 2:");

            // Groups have been flattened.
            foreach (var item in leftOuterQuery2)
            {
                totalItems++;
                Console.WriteLine("{0,-10}{1}", item.Name, item.CategoryID);
            }
            Console.WriteLine("LeftOuterJoin2: {0} items in 1 group", totalItems);

     * 
     * public void TestSelectManyJoined()
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