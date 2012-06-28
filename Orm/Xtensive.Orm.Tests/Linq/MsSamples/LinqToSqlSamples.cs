//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;

using Xtensive.Testing;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Tests.Linq.MsSamples
{
  [Category("Linq")]
  public class LinqToSqlSamples : NorthwindDOModelTest
  {
    [Category("WHERE")]
    [Test(Description = "Where - 1")]
    [Description("This sample uses WHERE to filter for Customers in London.")]
    public void DLinq1()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.Address.City=="London"
        select c;
      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 2")]
    [Description("This sample uses WHERE to filter for Employees hired " +
      "during or after 1994.")]
    public void DLinq2()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HireDate >= new DateTime(1994, 1, 1)
        select e;

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 3")]
    [Description("This sample uses WHERE to filter for Products that have stock below their " +
      "reorder level and are not discontinued.")]
    public void DLinq3()
    {
      var q =
        from p in Session.Query.All<Product>()
        where p.UnitsInStock <= p.ReorderLevel && !(p is DiscontinuedProduct)
        select p;

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 4")]
    [Description("This sample uses WHERE to filter out Products that are either " +
      "UnitPrice is greater than 10 or is discontinued.")]
    public void DLinq4()
    {
      var q =
        from p in Session.Query.All<Product>()
        where p.UnitPrice > 10m || (p is DiscontinuedProduct)
        select p;

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 5")]
    [Description("This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10" +
      " and is discontinued.")]
    public void DLinq5()
    {
      var q =
        Session.Query.All<Product>().Where(p => p.UnitPrice > 10m).Where(p => (p is DiscontinuedProduct));

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "First - Simple")]
    [Description("This sample uses First to select the first Shipper in the table.")]
    public void DLinq6()
    {
      Shipper shipper = Session.Query.All<Shipper>().First();
      QueryDumper.Dump(shipper);
    }

    [Category("WHERE")]
    [Test(Description = "First - Element")]
    [Description("This sample uses First to select the single Customer with Id 'BONAP'.")]
    public void DLinq7()
    {
      Customer cust = Session.Query.All<Customer>().First(c => c.Id=="BONAP");
      QueryDumper.Dump(cust);
    }

    [Category("WHERE")]
    [Test(Description = "First - Condition")]
    [Description("This sample uses First to select an Order with freight greater than 10.00.")]
    public void DLinq8()
    {
      Order ord = Session.Query.All<Order>().First(o => o.Freight > 10.00M);
      QueryDumper.Dump(ord);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Simple")]
    [Description("This sample uses SELECT to return a sequence of just the " +
      "Customers' contact names.")]
    public void DLinq9()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select c.ContactName;

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Anonymous Type 1")]
    [Description("This sample uses SELECT and anonymous types to return " +
      "a sequence of just the Customers' contact names and phone numbers.")]
    public void DLinq10()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select new {c.ContactName, c.Phone};

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Anonymous Type 2")]
    [Description("This sample uses SELECT and anonymous types to return " +
      "a sequence of just the Employees' names and phone numbers, " +
        "with the FirstName and LastName fields combined into a single field, 'Name', " +
          "and the HomePhone field renamed to Phone in the resulting sequence.")]
    public void DLinq11()
    {
      var q =
        from e in Session.Query.All<Employee>()
        select new {Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone};

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Anonymous Type 3")]
    [Description("This sample uses SELECT and anonymous types to return " +
      "a sequence of all Products' IDs and a calculated value " +
        "called HalfPrice which is set to the Product's UnitPrice " +
          "divided by 2.")]
    public void DLinq12()
    {
      var q =
        from p in Session.Query.All<Product>()
        select new {p.Id, HalfPrice = p.UnitPrice / 2};
      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Conditional ")]
    [Description("This sample uses SELECT and a conditional statment to return a sequence of product " +
      " name and product availability.")]
    public void DLinq13()
    {
      var q =
        from p in Session.Query.All<Product>()
        select new {p.ProductName, Availability = p.UnitsInStock - p.UnitsOnOrder < 0 ? "Out Of Stock" : "In Stock"};

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Named Type")]
    [Description("This sample uses SELECT and a known type to return a sequence of employees' names.")]
    public void DLinq14()
    {
      var q =
        from e in Session.Query.All<Employee>()
        select new Name {FirstName = e.FirstName, LastName = e.LastName};

      QueryDumper.Dump(q);
    }

    public class Name
    {
      public string FirstName;
      public string LastName;
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Filtered")]
    [Description("This sample uses SELECT and WHERE to return a sequence of " +
      "just the London Customers' contact names.")]
    public void DLinq15()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.Address.City=="London"
        select c.ContactName;

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Shaped")]
    [Description("This sample uses SELECT and anonymous types to return " +
      "a shaped subset of the data about Customers.")]
    public void DLinq16()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select new {
          c.Id,
          CompanyInfo = new {c.CompanyName, c.Address.City, c.Address.Country},
          ContactInfo = new {c.ContactName, c.ContactTitle}
        };

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Nested ")]
    [Description("This sample uses nested queries to return a sequence of " +
      "all orders containing their OrderID, a subsequence of the " +
        "items in the order where there is a discount, and the money " +
          "saved if shipping is not included.")]
    public void DLinq17()
    {
      var q =
        from o in Session.Query.All<Order>()
        select new {
          o.Id,
          DiscountedProducts =
            from od in o.OrderDetails
            where od.Discount > 0.0
            select od,
          FreeShippingDiscount = o.Freight
        };

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Distinct")]
    [Description("This sample uses Distinct to select a sequence of the unique cities " +
      "that have Customers.")]
    public void DLinq18()
    {
      var q = (
        from c in Session.Query.All<Customer>()
        select c.Address.City)
        .Distinct();

      QueryDumper.Dump(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Count - Simple")]
    [Description("This sample uses Count to find the number of Customers in the database.")]
    public void DLinq19()
    {
      var q = Session.Query.All<Customer>().Count();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Count - Conditional")]
    [Description("This sample uses Count to find the number of Products in the database " +
      "that are not discontinued.")]
    public void DLinq20()
    {
      var q = Session.Query.All<Product>().Count(p => !(p is DiscontinuedProduct));
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Sum - Simple")]
    [Description("This sample uses Sum to find the total freight over all Orders.")]
    public void DLinq21()
    {
      var q = Session.Query.All<Order>().Select(o => o.Freight).Sum();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Sum - Mapped")]
    [Description("This sample uses Sum to find the total number of units on order over all Products.")]
    public void DLinq22()
    {
      var q = Session.Query.All<Product>().Sum(p => p.UnitsOnOrder);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Min - Simple")]
    [Description("This sample uses Min to find the lowest unit price of any Product.")]
    public void DLinq23()
    {
      var q = Session.Query.All<Product>().Select(p => p.UnitPrice).Min();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Min - Mapped")]
    [Description("This sample uses Min to find the lowest freight of any Order.")]
    public void DLinq24()
    {
      var q = Session.Query.All<Order>().Min(o => o.Freight);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Min - Elements")]
    [Description("This sample uses Min to find the Products that have the lowest unit price " +
      "in each category.")]
    public void DLinq25()
    {
      var categories =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            Id = g.Key,
            CheapestProducts =
              from p2 in g
              where p2.UnitPrice==g.Min(p3 => p3.UnitPrice)
              select p2
          };

      QueryDumper.Dump(categories);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Max - Simple")]
    [Description("This sample uses Max to find the latest hire date of any Employee.")]
    public void DLinq26()
    {
      var q = Session.Query.All<Employee>().Select(e => e.HireDate).Max();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Max - Mapped")]
    [Description("This sample uses Max to find the most units in stock of any Product.")]
    public void DLinq27()
    {
      var q = Session.Query.All<Product>().Max(p => p.UnitsInStock);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Max - Elements")]
    [Description("This sample uses Max to find the Products that have the highest unit price " +
      "in each category.")]
    public void DLinq28()
    {
      var categories =
        Session.Query.All<Product>()
          .GroupBy(p => p.Id)
          .Select(g => new {
            g.Key,
            MostExpensiveProducts = g.Where(p2 => p2.UnitPrice==g.Max(p3 => p3.UnitPrice))
          });

      QueryDumper.Dump(categories);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Average - Simple")]
    [Description("This sample uses Average to find the average freight of all Orders.")]
    public void DLinq29()
    {
      var q = Session.Query.All<Order>().Select(o => o.Freight).Average();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Average - Mapped")]
    [Description("This sample uses Average to find the average unit price of all Products.")]
    public void DLinq30()
    {
      var q = Session.Query.All<Product>().Average(p => p.UnitPrice);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Average - Elements")]
    [Description("This sample uses Average to find the Products that have unit price higher than " +
      "the average unit price of the category for each category.")]
    public void DLinq31()
    {
      var categories =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            ExpensiveProducts =
              from p2 in g
              where p2.UnitPrice > g.Average(p3 => p3.UnitPrice)
              select p2
          };

      QueryDumper.Dump(categories);
    }


    [Category("JOIN")]
    [Test(Description = "SelectMany - 1 to Many - 1")]
    [Description("This sample uses foreign key navigation in the " +
      "from clause to select all orders for customers in London.")]
    public void DLinqJoin1()
    {
      var q =
        from c in Session.Query.All<Customer>()
        from o in c.Orders
        where c.Address.City=="London"
        select o;

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "SelectMany - 1 to Many - 2")]
    [Description("This sample uses foreign key navigation in the " +
      "where clause to filter for Products whose Supplier is in the USA " +
        "that are out of stock.")]
    public void DLinqJoin2()
    {
      var q =
        from p in Session.Query.All<Product>()
        where p.Supplier.Address.Country=="USA" && p.UnitsInStock==0
        select p;

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "SelectMany - Many to Many")]
    [Description("This sample uses foreign key navigation in the " +
      "from clause to filter for employees in Seattle, " +
        "and also list their territories.")]
    public void DLinqJoin3()
    {
      var q =
        from e in Session.Query.All<Employee>()
        from et in e.Territories
        where e.Address.City=="Seattle"
        select new {e.FirstName, e.LastName, et.TerritoryDescription};

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "SelectMany - Self-Join")]
    [Description("This sample uses foreign key navigation in the " +
      "select clause to filter for pairs of employees where " +
        "one employee reports to the other and where " +
          "both employees are from the same City.")]
    public void DLinqJoin4()
    {
      var q =
        from e1 in Session.Query.All<Employee>()
        from e2 in e1.ReportingEmployees
        where e1.Address.City==e2.Address.City
        select new {
          FirstName1 = e1.FirstName, LastName1 = e1.LastName,
          FirstName2 = e2.FirstName, LastName2 = e2.LastName,
          e1.Address.City
        };

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - Two way join")]
    [Description("This sample explictly joins two tables and projects results from both tables.")]
    public void DLinqJoin5()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var q =
        from c in Session.Query.All<Customer>()
        join o in Session.Query.All<Order>() on c.Id equals o.Customer.Id into orders
        select new {c.ContactName, OrderCount = orders.Count()};

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - Three way join")]
    [Description("This sample explictly joins three tables and projects results from each of them.")]
    public void DLinqJoin6()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var q =
        from c in Session.Query.All<Customer>()
        join o in Session.Query.All<Order>() on c.Id equals o.Customer.Id into ords
        join e in Session.Query.All<Employee>() on c.Address.City equals e.Address.City into emps
        select new {c.ContactName, ords = ords.Count(), emps = emps.Count()};

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - LEFT OUTER JOIN")]
    [Description("This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty(). The DefaultIfEmpty() method returns null when there is no Order for the Employee.")]
    public void DLinqJoin7()
    {
      var query =
        from employee in Session.Query.All<Employee>()
        join order in Session.Query.All<Order>() on employee equals order.Employee into orderJoins
        from orderJoin in orderJoins.DefaultIfEmpty()
        select new {employee.FirstName, employee.LastName, Order = orderJoin};

      QueryDumper.Dump(query);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - Projected let assignment")]
    [Description("This sample projects a 'let' expression resulting from a join.")]
    public void DLinqJoin8()
    {
      var q =
        from c in Session.Query.All<Customer>()
        join o in Session.Query.All<Order>() on c.Id equals o.Customer.Id into ords
        let z = c.Address.City + c.Address.Country
        from o in ords
        select new {c.ContactName, o.Id, z};

      QueryDumper.Dump(q);
    }

    [Ignore("Too slow")]
    [Category("JOIN")]
    [Test(Description = "GroupJoin - Composite Key")]
    [Description("This sample shows a join with a composite key.")]
    public void DLinqJoin9()
    {
      var q =
        from o in Session.Query.All<Order>()
        from p in Session.Query.All<Product>()
        join d in Session.Query.All<OrderDetails>()
          on new {OrderId = o.Id, ProductId = p.Id} equals new {OrderId = d.Order.Id, ProductId = d.Product.Id}
          into details
        from d in details
        select new {OrderId = o.Id, ProductId = p.Id, d.UnitPrice};

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - Nullable\\Nonnullable Key Relationship")]
    [Description("This sample shows how to construct a join where one side is nullable and the other isn't.")]
    public void DLinqJoin10()
    {
      var q =
        from o in Session.Query.All<Order>()
        join e in Session.Query.All<Employee>()
          on o.Id equals (int?) e.Id into emps
        from e in emps
        select new {o.Id, e.FirstName};

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "OrderBy - Simple")]
    [Description("This sample uses orderby to sort Employees " +
      "by hire date.")]
    public void DLinq36()
    {
      var q =
        from e in Session.Query.All<Employee>()
        orderby e.HireDate
        select e;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "OrderBy - With Where")]
    [Description("This sample uses where and orderby to sort Orders " +
      "shipped to London by freight.")]
    public void DLinq37()
    {
      var q =
        from o in Session.Query.All<Order>()
        where o.ShippingAddress.City=="London"
        orderby o.Freight
        select o;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "OrderByDescending")]
    [Description("This sample uses orderby to sort Products " +
      "by unit price from highest to lowest.")]
    public void DLinq38()
    {
      var q =
        from p in Session.Query.All<Product>()
        orderby p.UnitPrice descending
        select p;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "ThenBy")]
    [Description("This sample uses a compound orderby to sort Customers " +
      "by city and then contact name.")]
    public void DLinq39()
    {
      var q =
        from c in Session.Query.All<Customer>()
        orderby c.Address.City , c.ContactName
        select c;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "ThenByDescending")]
    [Description("This sample uses orderby to sort Orders from Id 1 " +
      "by ship-to country, and then by freight from highest to lowest.")]
    public void DLinq40()
    {
      var q =
        from o in Session.Query.All<Order>()
        where o.Id==1
        orderby o.ShippingAddress.Country , o.Freight descending
        select o;

      QueryDumper.Dump(q);
    }


    [Category("ORDER BY")]
    [Test(Description = "OrderBy - Group By")]
    [Description("This sample uses Orderby, Max and Group By to find the Products that have " +
      "the highest unit price in each category, and sorts the group by category id.")]
    public void DLinq41()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var categories =
        Session.Query.All<Product>()
          .GroupBy(p => p.Id)
          .OrderBy(g => g.Key)
          .Select(g => new {
            g.Key,
            MostExpensiveProducts =
              g.Where(p2 => p2.UnitPrice==g.Max(p3 => p3.UnitPrice))
          });

      QueryDumper.Dump(categories);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Simple")]
    [Description("This sample uses group by to partition Products by " +
      "Id.")]
    public void DLinq42()
    {
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select g;

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Max")]
    [Description("This sample uses group by and Max " +
      "to find the maximum unit price for each Id.")]
    public void DLinq43()
    {
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            MaxPrice = g.Max(p => p.UnitPrice)
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Min")]
    [Description("This sample uses group by and Min " +
      "to find the minimum unit price for each Id.")]
    public void DLinq44()
    {
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            MinPrice = g.Min(p => p.UnitPrice)
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Average")]
    [Description("This sample uses group by and Average " +
      "to find the average UnitPrice for each Id.")]
    public void DLinq45()
    {
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            AveragePrice = g.Average(p => p.UnitPrice)
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Sum")]
    [Description("This sample uses group by and Sum " +
      "to find the total UnitPrice for each Id.")]
    public void DLinq46()
    {
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            TotalPrice = g.Sum(p => p.UnitPrice)
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Count")]
    [Description("This sample uses group by and Count " +
      "to find the number of Products in each Id.")]
    public void DLinq47()
    {
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            NumProducts = g.Count()
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Count - Conditional")]
    [Description("This sample uses group by and Count " +
      "to find the number of Products in each Id " +
        "that are discontinued.")]
    public void DLinq48()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          select new {
            g.Key,
            NumProducts = g.Count(p => (p is DiscontinuedProduct))
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - followed by Where")]
    [Description("This sample uses a where clause after a group by clause " +
      "to find all categories that have at least 10 products.")]
    public void DLinq49()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var q =
        from p in Session.Query.All<Product>()
        group p by p.Id
        into g
          where g.Count() >= 10
          select new {
            g.Key,
            ProductCount = g.Count()
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Multiple Columns")]
    [Description("This sample uses Group By to group products by Id and SupplierID.")]
    public void DLinq50()
    {
      var categories =
        from p in Session.Query.All<Product>()
        group p by new {p.Id, SupplierId = p.Supplier.Id}
        into g
          select new {g.Key, g};

      QueryDumper.Dump(categories);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Expression")]
    [Description("This sample uses Group By to return two sequences of products. " +
      "The first sequence contains products with unit price " +
        "greater than 10. The second sequence contains products " +
          "with unit price less than or equal to 10.")]
    public void DLinq51()
    {
      var categories =
        from p in Session.Query.All<Product>()
        group p by new {Criterion = p.UnitPrice > 10}
        into g
          select g;

      QueryDumper.Dump(categories);
    }

    [Category("EXISTS/IN/ANY/ALL")]
    [Test(Description = "Any - Simple")]
    [Description("This sample uses Any to return only Customers that have no Orders.")]
    public void DLinq52()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where !c.Orders.Any()
        select c;

      QueryDumper.Dump(q);
    }

    [Category("EXISTS/IN/ANY/ALL")]
    [Test(Description = "Any - Conditional")]
    [Description("This sample uses Any to return only Categories that have " +
      "at least one Discontinued product.")]
    public void DLinq53()
    {
      var q =
        from c in Session.Query.All<Category>()
        where c.Products.Any(p => (p is DiscontinuedProduct))
        select c;

      QueryDumper.Dump(q);
    }

    [Category("EXISTS/IN/ANY/ALL")]
    [Test(Description = "All - Conditional")]
    [Description("This sample uses All to return Customers whom all of their orders " +
      "have been shipped to their own city or whom have no orders.")]
    public void DLinq54()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.Orders.All(o => o.ShippingAddress.City==c.Address.City)
        select c;

      QueryDumper.Dump(q);
    }

    [Category("UNION ALL/UNION/INTERSECT")]
    [Test(Description = "Concat - Simple")]
    [Description("This sample uses Concat to return a sequence of all Customer and Employee " +
      "phone/fax numbers.")]
    public void DLinq55()
    {
      var q = (
        from c in Session.Query.All<Customer>()
        select c.Phone
        ).Concat(
        from c in Session.Query.All<Customer>()
        select c.Fax
        ).Concat(
        from e in Session.Query.All<Employee>()
        select e.HomePhone
        );

      QueryDumper.Dump(q);
    }

    [Category("UNION ALL/UNION/INTERSECT")]
    [Test(Description = "Concat - Compound")]
    [Description("This sample uses Concat to return a sequence of all Customer and Employee " +
      "name and phone number mappings.")]
    public void DLinq56()
    {
      var q = (
        from c in Session.Query.All<Customer>()
        select new {Name = c.CompanyName, c.Phone}
        ).Concat(
        from e in Session.Query.All<Employee>()
        select new {Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone}
        );

      QueryDumper.Dump(q);
    }

    [Category("UNION ALL/UNION/INTERSECT")]
    [Test(Description = "Union")]
    [Description("This sample uses Union to return a sequence of all countries that either " +
      "Customers or Employees are in.")]
    public void DLinq57()
    {
      var q = (
        from c in Session.Query.All<Customer>()
        select c.Address.Country
        ).Union(
        from e in Session.Query.All<Employee>()
        select e.Address.Country
        );

      QueryDumper.Dump(q);
    }

    [Category("UNION ALL/UNION/INTERSECT")]
    [Test(Description = "Intersect")]
    [Description("This sample uses Intersect to return a sequence of all countries that both " +
      "Customers and Employees live in.")]
    public void DLinq58()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var q = (
        from c in Session.Query.All<Customer>()
        select c.Address.Country
        ).Intersect(
        from e in Session.Query.All<Employee>()
        select e.Address.Country
        );

      QueryDumper.Dump(q);
    }

    [Category("UNION ALL/UNION/INTERSECT")]
    [Test(Description = "Except")]
    [Description("This sample uses Except to return a sequence of all countries that " +
      "Customers live in but no Employees live in.")]
    public void DLinq59()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var q = (
        from c in Session.Query.All<Customer>()
        select c.Address.Country
        ).Except(
        from e in Session.Query.All<Employee>()
        select e.Address.Country
        );

      QueryDumper.Dump(q);
    }

    [Category("TOP/BOTTOM")]
    [Test(Description = "Take")]
    [Description("This sample uses Take to select the first 5 Employees hired.")]
    public void DLinq60()
    {
      var q = (
        from e in Session.Query.All<Employee>()
        orderby e.HireDate
        select e)
        .Take(5);

      QueryDumper.Dump(q);
    }

    [Category("TOP/BOTTOM")]
    [Test(Description = "Skip")]
    [Description("This sample uses Skip to select all but the 10 most expensive Products.")]
    public void DLinq61()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var q = (
        from p in Session.Query.All<Product>()
        orderby p.UnitPrice descending
        select p)
        .Skip(10);

      QueryDumper.Dump(q);
    }

    [Category("Paging")]
    [Test(Description = "Paging - Index")]
    [Description("This sample uses the Skip and Take operators to do paging by " +
      "skipping the first 50 records and then returning the next 10, thereby " +
        "providing the data for page 6 of the Products table.")]
    public void DLinq62()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var q = (
        from c in Session.Query.All<Customer>()
        orderby c.ContactName
        select c)
        .Skip(50)
        .Take(10);

      QueryDumper.Dump(q);
    }

    [Category("Paging")]
    [Test(Description = "Paging - Ordered Unique Key")]
    [Description("This sample uses a where clause and the Take operator to do paging by, " +
      "first filtering to get only the Ids above 50 (the last Id " +
        "from page 5), then ordering by Id, and finally taking the first 10 results, " +
          "thereby providing the data for page 6 of the Products table.  " +
            "Note that this method only works when ordering by a unique key.")]
    public void DLinq63()
    {
      var q = (
        from p in Session.Query.All<Product>()
        where p.Id > 50
        orderby p.Id
        select p)
        .Take(10);

      QueryDumper.Dump(q);
    }


    [Category("NULL")]
    [Test(Description = "null")]
    [Description("This sample uses the null value to find Employees " +
      "that do not report to another Employee.")]
    public void DLinq75()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.ReportsTo==null
        select e;

      QueryDumper.Dump(q);
    }

    [Category("NULL")]
    [Test(Description = "Nullable<T>.HasValue")]
    [Description("This sample uses Nullable<T>.HasValue to find Employees " +
      "that do not report to another Employee.")]
    public void DLinq76()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where !e.BirthDate.HasValue
        select e;

      QueryDumper.Dump(q);
    }

    [Category("NULL")]
    [Test(Description = "Nullable<T>.Value")]
    [Description("This sample uses Nullable<T>.Value for Employees " +
      "that report to another Employee to return the " +
        "Id number of that employee.  Note that " +
          "the .Value is optional.")]
    public void DLinq77()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.BirthDate.HasValue
        select new {e.FirstName, e.LastName, BirthDate = e.BirthDate.Value};

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String Concatenation")]
    [Description("This sample uses the + operator to concatenate string fields " +
      "and string literals in forming the Customers' calculated " +
        "Location value.")]
    public void DLinq78()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select new {c.Id, Location = c.Address.City + ", " + c.Address.Country};

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Length")]
    [Description("This sample uses the Length property to find all Products whose " +
      "name is shorter than 10 characters.")]
    public void DLinq79()
    {
      var q =
        from p in Session.Query.All<Product>()
        where p.ProductName.Length < 10
        select p;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Contains(substring)")]
    [Description("This sample uses the Contains method to find all Customers whose " +
      "contact name contains 'Anders'.")]
    public void DLinq80()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.ContactName.Contains("Anders")
        select c;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.IndexOf(substring)")]
    [Description("This sample uses the IndexOf method to find the first instance of " +
      "a space in each Customer's contact name.")]
    public void DLinq81()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select new {c.ContactName, SpacePos = c.ContactName.IndexOf(" ")};

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.StartsWith(prefix)")]
    [Description("This sample uses the StartsWith method to find Customers whose " +
      "contact name starts with 'Maria'.")]
    public void DLinq82()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.ContactName.StartsWith("Maria")
        select c;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.EndsWith(suffix)")]
    [Description("This sample uses the StartsWith method to find Customers whose " +
      "contact name ends with 'Anders'.")]
    public void DLinq83()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.ContactName.EndsWith("Anders")
        select c;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Substring(start)")]
    [Description("This sample uses the Substring method to return Product names starting " +
      "from the fourth letter.")]
    public void DLinq84()
    {
      var q =
        from p in Session.Query.All<Product>()
        select p.ProductName.Substring(3);

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Substring(start, length)")]
    [Description("This sample uses the Substring method to find Employees whose " +
      "home phone numbers have '555' as the seventh through ninth digits.")]
    public void DLinq85()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HomePhone.Substring(6, 3)=="555"
        select e;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.ToUpper()")]
    [Description("This sample uses the ToUpper method to return Employee names " +
      "where the last name has been converted to uppercase.")]
    public void DLinq86()
    {
      var q =
        from e in Session.Query.All<Employee>()
        select new {LastName = e.LastName.ToUpper(), e.FirstName};

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.ToLower()")]
    [Description("This sample uses the ToLower method to return Category names " +
      "that have been converted to lowercase.")]
    public void DLinq87()
    {
      var q =
        from c in Session.Query.All<Category>()
        select c.CategoryName.ToLower();

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Trim()")]
    [Description("This sample uses the Trim method to return the first five " +
      "digits of Employee home phone numbers, with leading and " +
        "trailing spaces removed.")]
    public void DLinq88()
    {
      var q =
        from e in Session.Query.All<Employee>()
        select e.HomePhone.Substring(0, 5).Trim();

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Insert(pos, str)")]
    [Description("This sample uses the Insert method to return a sequence of " +
      "employee phone numbers that have a ) in the fifth position, " +
        "inserting a : after the ).")]
    public void DLinq89()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HomePhone.Substring(4, 1)==")"
        select e.HomePhone.Insert(5, ":");

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Remove(start)")]
    [Description("This sample uses the Insert method to return a sequence of " +
      "employee phone numbers that have a ) in the fifth position, " +
        "removing all characters starting from the tenth character.")]
    public void DLinq90()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HomePhone.Substring(4, 1)==")"
        select e.HomePhone.Remove(9);

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Remove(start, length)")]
    [Description("This sample uses the Insert method to return a sequence of " +
      "employee phone numbers that have a ) in the fifth position, " +
        "removing the first six characters.")]
    public void DLinq91()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HomePhone.Substring(4, 1)==")"
        select e.HomePhone.Remove(0, 6);

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Replace(find, replace)")]
    [Description("This sample uses the Replace method to return a sequence of " +
      "Supplier information where the Country field has had " +
        "UK replaced with United Kingdom and USA replaced with " +
          "United States of America.")]
    public void DLinq92()
    {
      var q =
        from s in Session.Query.All<Supplier>()
        select new {
          s.CompanyName,
          Country = s.Address.Country.Replace("UK", "United Kingdom")
            .Replace("USA", "United States of America")
        };

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "DateTime.Year")]
    [Description("This sample uses the DateTime's Year property to " +
      "find Orders placed in 1997.")]
    public void DLinq93()
    {
      var q =
        from o in Session.Query.All<Order>()
        where o.OrderDate.Value.Year==1997
        select o;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "DateTime.Month")]
    [Description("This sample uses the DateTime's Month property to " +
      "find Orders placed in December.")]
    public void DLinq94()
    {
      var q =
        from o in Session.Query.All<Order>()
        where o.OrderDate.Value.Month==12
        select o;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "DateTime.Day")]
    [Description("This sample uses the DateTime's Day property to " +
      "find Orders placed on the 31st day of the month.")]
    public void DLinq95()
    {
      var q =
        from o in Session.Query.All<Order>()
        where o.OrderDate.Value.Day==31
        select o;

      QueryDumper.Dump(q);
    }

    [Category("Object Identity")]
    [Test(Description = "Object Caching - 1")]
    [Description("This sample demonstrates how, upon executing the same query twice, " +
      "you will receive a reference to the same object in memory each time.")]
    public void DLinq96()
    {
      Customer cust1 = Session.Query.All<Customer>().First(c => c.Id=="BONAP");
      Customer cust2 = Session.Query.All<Customer>().First(c => c.Id=="BONAP");

      Console.WriteLine("cust1 and cust2 refer to the same object in memory: {0}",
        Object.ReferenceEquals(cust1, cust2));
    }

    [Category("Object Identity")]
    [Test(Description = "Object Caching - 2")]
    [Description("This sample demonstrates how, upon executing different queries that " +
      "return the same row from the database, you will receive a " +
        "reference to the same object in memory each time.")]
    public void DLinq97()
    {
      Customer cust1 = Session.Query.All<Customer>().First(c => c.Id=="BONAP");
      Customer cust2 = (
        from o in Session.Query.All<Order>()
        where o.Customer.Id=="BONAP"
        select o)
        .First()
        .Customer;

      Console.WriteLine("cust1 and cust2 refer to the same object in memory: {0}",
        Object.ReferenceEquals(cust1, cust2));
    }

    [Category("Object Loading")]
    [Test(Description = "Deferred Loading - 1")]
    [Description("This sample demonstrates how navigating through relationships in " +
      "retrieved objects can end up triggering new queries to the database " +
        "if the data was not requested by the original query.")]
    public void DLinq98()
    {
      var custs =
        from c in Session.Query.All<Customer>()
        where c.Address.City=="Sao Paulo"
        select c;

      foreach (var cust in custs) {
        foreach (var ord in cust.Orders) {
          Console.WriteLine("Id {0} has an OrderID {1}.", cust.Id, ord.Id);
        }
      }
    }

    private bool isValidProduct(Product p)
    {
      return p.ProductName.LastIndexOf('C')==0;
    }

    [Category("Object Loading")]
    [Test(Description = "Deferred Loading - (1:M)")]
    [Description("This sample demonstrates how navigating through relationships in " +
      "retrieved objects can result in triggering new queries to the database " +
        "if the data was not requested by the original query.")]
    public void DLinq102()
    {
      var emps = from e in Session.Query.All<Employee>()
      select e;

      foreach (var emp in emps) {
        foreach (var man in emp.ReportingEmployees) {
          Console.WriteLine("Employee {0} reported to Manager {1}.", emp.FirstName, man.FirstName);
        }
      }
    }

    //[Category("Object Loading")]
    //[Test(Description = "Including - Eager Loading - (1:M)")]
    //[Description("This sample demonstrates how to use Including to request related " +
    //             "data during the original query so that additional roundtrips to the " +
    //             "database are not triggered while navigating through " +
    //             "the retrieved objects.")]
    //public void DLinq103() {
    //    var emps = (
    //        from e in Session.Query.All<Employee>()
    //        select e)
    //        .Including(e => e.Employees);

    //    foreach (var emp in emps)
    //    {
    //        foreach (var man in emp.Employees)
    //        {
    //            Console.WriteLine("Employee {0} reported to Manager {1}.", emp.FirstName, man.FirstName);
    //        }
    //    }
    //}


    [Category("Object Loading")]
    [Test(Description = "Deferred Loading - (BLOB)")]
    [Description("This sample demonstrates how navigating through Link in " +
      "retrieved objects can end up triggering new queries to the database " +
        "if the data type is Link.")]
    public void DLinq104()
    {
      var emps = from c in Session.Query.All<Employee>()
      select c;

      foreach (Employee emp in emps) {
        Console.WriteLine("{0}", emp.Notes);
      }
    }

    [Category("Conversion Operators")]
    [Test(Description = "AsEnumerable")]
    [Description("This sample uses AsEnumerable so that the client-side IEnumerable<T> " +
      "implementation of Where is used, instead of the default IQueryable<T> " +
        "implementation which would be converted to SQL and executed " +
          "on the server.  This is necessary because the where clause " +
            "references a user-defined client-side method, isValidProduct, " +
              "which cannot be converted to SQL.")]
//        [LinkedMethod("isValidProduct")]
    public void DLinq105()
    {
      var q =
        from p in Session.Query.All<Product>().ToList()
        where isValidProduct(p)
        select p;

      QueryDumper.Dump(q);
    }

    [Category("Conversion Operators")]
    [Test(Description = "ToArray")]
    [Description("This sample uses ToArray to immediately evaluate a query into an array " +
      "and get the 3rd element.")]
    public void DLinq106()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.Address.City=="London"
        select c;

      Customer[] qArray = q.ToArray();
      QueryDumper.Dump(qArray[3]);
    }

    [Category("Conversion Operators")]
    [Test(Description = "ToList")]
    [Description("This sample uses ToList to immediately evaluate a query into a List<T>.")]
    public void DLinq107()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HireDate >= new DateTime(1994, 1, 1)
        select e;

      List<Employee> qList = q.ToList();
      QueryDumper.Dump(qList);
    }

    [Category("Conversion Operators")]
    [Test(Description = "ToDictionary")]
    [Description("This sample uses ToDictionary to immediately evaluate a query and " +
      "a key expression into an Dictionary<K, T>.")]
    public void DLinq108()
    {
      var q =
        from p in Session.Query.All<Product>()
        where p.UnitsInStock <= p.ReorderLevel && !(p is DiscontinuedProduct)
        select p;

      Dictionary<int, Product> qDictionary = q.ToDictionary(p => p.Id);

      foreach (int key in qDictionary.Keys) {
        Console.WriteLine("Key {0}:", key);
        QueryDumper.Dump(qDictionary[key]);
        Console.WriteLine();
      }
    }

//
//        [Category("Stored Procedures")]
//        [Test(Description = "Scalar Return")]
//        [Description("This sample uses a stored procedure to return the number of Customers in the 'WA' Region.")]
//        public void DLinq113() {
//            int count = Query<Customer>.CustomersCountByRegion("WA");
//
//            Console.WriteLine(count);
//        }
//
//        [Category("Stored Procedures")]
//        [Test(Description = "Single CompilationResult-Set")]
//        [Description("This sample uses a stored procedure to return the Id, ContactName, CompanyName" +
//        " and City of customers who are in London.")]
//        public void DLinq114() {
//            IEnumerable<CustomersByCityResult> result = Session.Query.All<Customer>()ByCity("London");
//
//            QueryDumper.Dump(result);
//        }
//
//        [Category("Stored Procedures")]
//        [Test(Description = "Single CompilationResult-Set - Multiple Possible Shapes")]
//        [Description("This sample uses a stored procedure to return a set of " +
//        "Customers in the 'WA' Region.  The result set-shape returned depends on the parameter passed in. " +
//        "If the parameter equals 1, all Customer properties are returned. " +
//        "If the parameter equals 2, the Id, ContactName and CompanyName properties are returned.")]
//        public void DLinq115() {
//            Console.WriteLine("********** Whole Customer CompilationResult-set ***********");
//            IMultipleResults result = db.WholeOrPartialCustomersSet(1);
//            IEnumerable<WholeCustomersSetResult> shape1 = result.GetResult<WholeCustomersSetResult>();
//
//            QueryDumper.Dump(shape1);
//
//            Console.WriteLine();
//            Console.WriteLine("********** Partial Customer CompilationResult-set ***********");
//            result = db.WholeOrPartialCustomersSet(2);
//            IEnumerable<PartialCustomersSetResult> shape2 = result.GetResult<PartialCustomersSetResult>();
//
//            QueryDumper.Dump(shape2);
//        }
//
//        [Category("Stored Procedures")]
//        [Test(Description = "Multiple CompilationResult-Sets")]
//        [Description("This sample uses a stored procedure to return the Customer 'SEVES' and all it's Orders.")]
//        public void DLinq116() {
//            IMultipleResults result = db.GetCustomerAndOrders("SEVES");
//
//            Console.WriteLine("********** Customer CompilationResult-set ***********");
//            IEnumerable<CustomerResultSet> customer = result.GetResult<CustomerResultSet>();
//            QueryDumper.Dump(customer);
//            Console.WriteLine();
//
//            Console.WriteLine("********** Orders CompilationResult-set ***********");
//            IEnumerable<OrdersResultSet> orders = result.GetResult<OrdersResultSet>();
//            QueryDumper.Dump(orders);
//        }
//
//        [Category("Stored Procedures")]
//        [Test(Description = "Out parameters")]
//        [Description("This sample uses a stored procedure that returns an out parameter.")]
//        public void DLinq143() {
//            decimal? totalSales = 0;
//            string Id = "ALFKI";
//
//            // Out parameters are passed by ref, to support scenarios where
//            // the parameter is 'in/out'.  In this case, the parameter is only
//            // 'out'.
//            db.CustomerTotalSales(Id, ref totalSales);
//
//            Console.WriteLine("Total Sales for Customer '{0}' = {1:C}", Id, totalSales);
//        }
//
//
//        [Category("User-Defined Functions")]
//        [Test(Description = "Scalar Function - Select")]
//        [Description("This sample demonstrates using a scalar user-defined function in a projection.")]
//        public void DLinq117() {
//            var q = from c in db.Categories
//                    select new {c.Id, TotalUnitPrice = db.TotalProductUnitPriceByCategory(c.Id)};
//
//            QueryDumper.Dump(q);
//        }
//
//        [Category("User-Defined Functions")]
//        [Test(Description = "Scalar Function - Where")]
//        [Description("This sample demonstrates using a scalar user-defined function in a where clause.")]
//        public void DLinq118() {
//            var q = from p in Session.Query.All<Product>()
//                    where p.UnitPrice == db.MinUnitPriceByCategory(p.Id)
//                    select p;
//
//            QueryDumper.Dump(q);
//        }
//
//        [Category("User-Defined Functions")]
//        [Test(Description = "Table-Valued Function")]
//        [Description("This sample demonstrates selecting from a table-valued user-defined function.")]
//        public void DLinq119() {
//            var q = from p in Session.Query.All<Product>()UnderThisUnitPrice(10.25M)
//                    where !(p is DiscontinuedProduct)
//                    select p;
//
//            QueryDumper.Dump(q);
//        }
//
//        [Category("User-Defined Functions")]
//        [Test(Description = "Table-Valued Function - Join")]
//        [Description("This sample demonstrates joining to the results of a table-valued user-defined function.")]
//        public void DLinq151() {
//            var q = from c in db.Categories
//                    join p in Session.Query.All<Product>()UnderThisUnitPrice(8.50M) on c.Id equals p.Id into prods
//                    from p in prods
//                    select new {c.Id, c.CategoryName, p.ProductName, p.UnitPrice};
//
//            QueryDumper.Dump(q);
//        }


    [Category("Advanced")]
    [Test(Description = "Nested in FROM")]
    [Description("This sample uses orderbyDescending and Take to return the " +
      "discontinued products of the top 10 most expensive products.")]
    public void DLinq131()
    {
      var prods = from p in Session.Query.All<Product>().OrderByDescending(p => p.UnitPrice).Take(10)
      where p is DiscontinuedProduct
      select p;

      QueryDumper.Dump(prods);
    }


    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Inheritance")]
    [Test(Description = "Simple")]
    [Description("This sample returns all contacts where the city is London.")]
    public void DLinq135()
    {
      var cons = from c in Session.Query.All<BusinessContact>()
      select c;

      foreach (var con in cons) {
        Console.WriteLine("Company name: {0}", con.CompanyName);
        Console.WriteLine("Phone: {0}", con.Phone);
        Console.WriteLine("This is a {0}", con.GetType());
        Console.WriteLine();
      }
    }

    [Category("Inheritance")]
    [Test(Description = "OfType")]
    [Description("This sample uses OfType to return all customer contacts.")]
    [ExpectedException(typeof (QueryTranslationException))]
    public void DLinq136()
    {
      var cons = from c in Session.Query.All<Person>().OfType<Customer>()
      select c;

      QueryDumper.Dump(cons);
    }

    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Inheritance")]
    [Test(Description = "IS")]
    [Description("This sample uses IS to return all shipper contacts.")]
    public void DLinq137()
    {
      var cons = from c in Session.Query.All<Person>()
      where c is Customer
      select c;

      QueryDumper.Dump(cons);
    }

    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Inheritance")]
    [Test(Description = "AS")]
    [Description("This sample uses AS to return FullContact or null.")]
    public void DLinq138()
    {
      var cons = from c in Session.Query.All<Person>()
      select c as BusinessContact;

      QueryDumper.Dump(cons);
    }

    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Inheritance")]
    [Test(Description = "Cast")]
    [Description("This sample uses a cast to retrieve customer contacts who work in 'Around the Horn'.")]
    public void DLinq139()
    {
      var cons = from c in Session.Query.All<Person>()
      where c is Customer && ((Customer) c).CompanyName=="Around the Horn"
      select c;

      QueryDumper.Dump(cons);
    }
  }
}