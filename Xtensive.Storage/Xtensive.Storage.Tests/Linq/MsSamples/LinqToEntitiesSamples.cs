//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;


namespace Xtensive.Storage.Tests.Linq.MsSamples
{
  [Category("Linq")]
  [TestFixture]
  public class LinqToEntitiesSamples : NorthwindDOModelTest
  {
    #region Restriction Operators

    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 1")]
    [Description("This sample uses WHERE to find all customers in Seattle.")]
    public void LinqToEntities1()
    {
      var query = from cust in Query.All<Customer>()
      where cust.Address.City=="Seattle"
      select cust;
      QueryDumper.Dump(query);
    }


    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 2")]
    [Description("This sample uses WHERE to find all orders placed in 1994.")]
    public void LinqToEntities2()
    {
      DateTime dt = new DateTime(1994, 1, 1);
      var query = from order in Query.All<Order>()
      where order.OrderDate > dt
      select order;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 3")]
    [Description("This sample uses WHERE to filter for Products that have stock below their reorder level and have a units on order of zero.")]
    public void LinqToEntities3()
    {
      var query = from p in Query.All<Product>()
      where p.UnitsInStock < p.ReorderLevel && p.UnitsOnOrder==0
      select p;

      QueryDumper.Dump(query);
    }


    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 4")]
    [Description("This sample uses WHERE to filter out Products that have a UnitPrice less than 10.")]
    public void LinqToEntities4()
    {
      var query = from p in Query.All<Product>()
      where p.UnitPrice < 10
      select p;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Where - Related Entities 1")]
    [Description("This sample uses WHERE to get orders for Customers in Mexico.")]
    public void LinqToEntities5()
    {
      var query = from o in Query.All<Order>()
      where o.Customer.Address.Country=="Mexico"
      select o;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Where - Related Entities 2")]
    [Description("This sample uses WHERE to get orders sold by employees in the UK.")]
    public void LinqToEntities6()
    {
      var query = from o in Query.All<Order>()
      where o.Employee.Address.Country=="UK"
      select o;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Any - 1")]
    [Description("This sample uses ANY to get employees have sold an order.")]
    public void LinqToEntities7()
    {
      var query = from e in Query.All<Employee>()
      where e.Orders.Any(o => o!=null)
      select e;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Any - 2")]
    [Description("This sample uses ANY to check for any out-of-stock products.")]
    public void LinqToEntities8()
    {
      var query = Query.All<Supplier>()
        .Where(s => s.Products
          .Any(p => p.UnitsInStock==0))
        .Select(s => s);

      QueryDumper.Dump(query);
    }


    [Category("Restriction Operators")]
    [Test(Description = "Any - Related Entities")]
    [Description("This sample uses WHERE and ANY to get employees who sold an order to any customer in Mexico.")]
    public void LinqToEntities9()
    {
      var query = from e in Query.All<Employee>()
      where e.Orders.Any(o => o.Customer.Address.Country=="Mexico")
      select e;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "All - Simple")]
    [Description("This sample uses ALL to get employees who sold orders only to customers not in Canada.")]
    public void LinqToEntities10()
    {
      var query = from e in Query.All<Employee>()
      where e.Orders.All(o => o.Customer.Address.Country!="Canada")
      select e;

      QueryDumper.Dump(query);
    }

    #endregion

    #region Projection Operators

    [Category("Projection Operators")]
    [Test(Description = "Select - Simple 1")]
    [Description("This samples uses SELECT to get all Customers as Entity Objects.")]
    public void LinqToEntities11()
    {
      var query = from c in Query.All<Customer>()
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Simple 2")]
    [Description("This samples uses SELECT to get all Customer Contact Names as Strings.")]
    public void LinqToEntities12()
    {
      var query = from c in Query.All<Customer>()
      select c.ContactName;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Anonymous 1")]
    [Description("This samples uses SELECT to get all Customer Contact Names as an anonoymous type.")]
    public void LinqToEntities13()
    {
      var query = from c in Query.All<Customer>()
      select new {c.ContactName};

      QueryDumper.Dump(query);
    }


    [Category("Projection Operators")]
    [Test(Description = "Select - Anonymous 2")]
    [Description("This sample uses SELECT to get all Orders as anonymous type")]
    public void LinqToEntities14()
    {
      var query = from o in Query.All<Order>()
      select new {o};

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Anonymous 3")]
    [Description("This sample uses SELECT to get all Orders and associated Employees as anonymous type")]
    public void LinqToEntities15()
    {
      var query = from o in Query.All<Order>()
      select new {o, o.Employee};

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Nested Collection ")]
    [Description("This sample uses SELECT to get all Customers, and those Orders for each customer with a freight > 5")]
    public void LinqToEntities15a()
    {
      var query = Query.All<Customer>().Select(c => new {Customer = c, Orders = c.Orders.Where(o => o.Freight > 5)});

      QueryDumper.Dump(query);
    }


    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 1")]
    [Description("This sample uses SELECTMANY to get all Orders for a Customer as a flat result")]
    public void LinqToEntities16()
    {
      var query = from c in Query.All<Customer>()
      where c.Id=="ALFKI"
      from o in c.Orders
      select o;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 2")]
    [Description("This sample uses SELECTMANY to get all Orders for a Customer as a flat result using LINQ operators")]
    public void LinqToEntities17()
    {
      var query = Query.All<Customer>().Where(cust => cust.Id=="ALFKI")
        .SelectMany(cust => cust.Orders);

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 3")]
    [Description("This sample uses SELECTMANY to get all Orders for Customers in Denmark as a flat result")]
    public void LinqToEntities18()
    {
      var query = from c in Query.All<Customer>()
      where c.Address.Country=="Denmark"
      from o in c.Orders
      select o;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 4")]
    [Description("This sample uses SELECTMANY to get all Orders for Customers in Denmark as a flat result using LINQ operators")]
    public void LinqToEntities19()
    {
      var query = Query.All<Customer>().Where(cust => cust.Address.Country=="Denmark")
        .SelectMany(cust => cust.Orders);

      QueryDumper.Dump(query);
    }


    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Predicate 1")]
    [Description("This sample uses SELECTMANY to get all Orders for Customers in Denmark as a flat result")]
    public void LinqToEntities20()
    {
      var query = from c in Query.All<Customer>()
      where c.Address.Country=="Denmark"
      from o in c.Orders
      where o.Freight > 5
      select o;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Predicate 2")]
    [Description("This sample uses SELECTMANY to get all Orders for Customers in Denmark as an anonymous type containing the Orders and Customer flat result")]
    public void LinqToEntities21()
    {
      var query = from c in Query.All<Customer>()
      where c.Address.Country=="Denmark"
      from o in c.Orders
      where o.Freight > 5
      select new {c, o};

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Predicate 3")]
    [Description("This sample uses SELECTMANY to get all Orders for Customers in Denmark as a flat result using LINQ opeartors")]
    public void LinqToEntities22()
    {
      var query = Query.All<Customer>().Where(cust => cust.Address.Country=="Denmark")
        .SelectMany(cust => cust.Orders.Where(o => o.Freight > 5));

      QueryDumper.Dump(query);
    }

    #endregion

    #region Aggregate Operators

    [Category("Aggregate Operators")]
    [Test(Description = "Count - Simple")]
    [Description("This sample uses COUNT to get the number of Orders.")]
    public void LinqToEntities23()
    {
      var query = Query.All<Order>().Count();

      QueryDumper.Dump(query);
    }


    [Category("Aggregate Operators")]
    [Test(Description = "Count - Predicate 1")]
    [Description("This sample uses COUNT to get the number of Orders placed by Customers in Mexico.")]
    public void LinqToEntities24()
    {
      var query = Query.All<Order>().Where(o => o.Customer.Address.Country=="Mexico").Count();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Count - Predicate 2")]
    [Description("This sample uses COUNT to get the number of Orders shipped to Mexico.")]
    public void LinqToEntities25()
    {
      var query = Query.All<Order>()
        .Where(o => o.ShippingAddress.Country=="Mexico").Count();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Sum - Simple 1")]
    [Description("This sample uses SUM to find the total freight over all Orders.")]
    public void LinqToEntities26()
    {
      var query = Query.All<Order>().Select(o => o.Freight).Sum();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Sum - Simple 2")]
    [Description("This sample uses SUM to find the total number of units on order over all Products.")]
    public void LinqToEntities27()
    {
      var query = Query.All<Product>().Sum(p => p.UnitsOnOrder);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Sum - Simple 3")]
    [Description("This sample uses SUM to find the total number of units on order over all Products out-of-stock.")]
    public void LinqToEntities28()
    {
      var query = Query.All<Product>().Where(p => p.UnitsInStock==0).Sum(p => p.UnitsOnOrder);

      QueryDumper.Dump(query);
    }


    [Category("Aggregate Operators")]
    [Test(Description = "Min - Simple 1")]
    [Description("This sample uses MIN to find the lowest unit price of any Product.")]
    public void LinqToEntities29()
    {
      var query = Query.All<Product>().Select(p => p.UnitPrice).Min();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Min - Simple 2")]
    [Description("This sample uses MIN to find the lowest freight of any Order.")]
    public void LinqToEntities30()
    {
      var query = Query.All<Order>().Min(o => o.Freight);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Min - Predicate")]
    [Description("This sample uses MIN to find the lowest freight of any Order shipped to Mexico.")]
    public void LinqToEntities31()
    {
      var query = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Mexico").Min(o => o.Freight);

      QueryDumper.Dump(query);
    }


    [Category("Aggregate Operators")]
    [Test(Description = "Min - Grouping")]
    [Description("This sample uses Min to find the Products that have the lowest unit price in each category, and returns the result as an anonoymous type.")]
    public void LinqToEntities32()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Query.All<Product>()
        .GroupBy(p => p.Category)
        .Select(g => new {
        CategoryID = g.Key,
        CheapestProducts =
          g.Where(p2 => p2.UnitPrice==g.Min(p3 => p3.UnitPrice))
      });

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Simple 1")]
    [Description("This sample uses MAX to find the latest hire date of any Employee.")]
    public void LinqToEntities33()
    {
      var query = Query.All<Employee>().Select(e => e.HireDate).Max();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Simple 2")]
    [Description("This sample uses MAX to find the most units in stock of any Product.")]
    public void LinqToEntities34()
    {
      var query = Query.All<Product>().Max(p => p.UnitsInStock);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Predicate")]
    [Description("This sample uses MAX to find the most units in stock of any Product with CategoryID = 1.")]
    public void LinqToEntities35()
    {
      var query = Query.All<Product>().Where(p => p.Category.Id==1).Max(p => p.UnitsInStock);
      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Grouping")]
    [Description("This sample uses MAX to find the Products that have the highest unit price in each category, and returns the result as an anonoymous type.")]
    public void LinqToEntities36()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = from p in Query.All<Product>()
      group p by p.Category
      into g
        select new {
          g.Key,
          MostExpensiveProducts =
            from p2 in g
            where p2.UnitPrice==g.Max(p3 => p3.UnitPrice)
            select p2
        };

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Simple 1")]
    [Description("This sample uses AVERAGE to find the average freight of all Orders.")]
    public void LinqToEntities37()
    {
      var query = Query.All<Order>().Select(o => o.Freight).Average();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Simple 2")]
    [Description("This sample uses AVERAGE to find the average unit price of all Products.")]
    public void LinqToEntities38()
    {
      var query = Query.All<Product>().Average(p => p.UnitPrice);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Predicate")]
    [Description("This sample uses AVERAGE to find the average unit price of all Products with CategoryID = 1.")]
    public void LinqToEntities39()
    {
      var query = Query.All<Product>().Where(p => p.Category.Id==1).Average(p => p.UnitPrice);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Grouping 1")]
    [Description("This sample uses AVERAGE to find the Products that have unit price higher than the average unit price of the category for each category.")]
    public void LinqToEntities40()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = from p in Query.All<Product>()
      group p by p.Category
      into g
        select new {
          g.Key,
          ExpensiveProducts =
            from p2 in g
            where p2.UnitPrice > g.Average(p3 => p3.UnitPrice)
            select p2
        };

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Grouping 2")]
    [Description("This sample uses AVERAGE to find the average unit price of each category.")]
    public void LinqToEntities41()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Query.All<Product>()
        .GroupBy(p => p.Category)
        .Select(g => new {
          g.Key,
          Average = g.Average(p => p.UnitPrice)
        });

      QueryDumper.Dump(query);
    }

    #endregion

    #region Set Operators

    [Category("Set And Element Operators")]
    [Test(Description = "First - Simple")]
    [Description("This sample uses FIRST and WHERE to get the first (database order) order that is shipped to Seattle. The WHERE predicate is evaluated on the server.")]
    public void LinqToEntities42()
    {
      var query = from o in Query.All<Order>()
      where o.ShippingAddress.City=="Seattle"
      select o;

      // Feb CTP requires AsEnumerable()
      var result = query.ToList().First();

      QueryDumper.Dump(result);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "First - Predicate")]
    [Description("This sample uses FIRST to get the first (database order) order that is shipped to Seattle. The predicate is evaluated on the client.")]
    public void LinqToEntities43()
    {
      var query = from o in Query.All<Order>()
      select o;

      // Feb CTP requires AsEnumerable()
      var result = query
        .ToList()
        .First(x => x.ShippingAddress.City=="Seattle");

      QueryDumper.Dump(result);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "First - Ordered")]
    [Description("This sample uses FIRST, WHERE and ORDER BY to get the first order that is shipped to Seattle, ordered by date. The predicate is evaluated on the server.")]
    public void LinqToEntities44()
    {
      var query = from o in Query.All<Order>()
      where o.ShippingAddress.City=="Seattle"
      orderby o.OrderDate
      select o;

      // Feb CTP requires AsEnumerable()
      var result = query.ToList().First();

      QueryDumper.Dump(result);
    }


    [Category("Set And Element Operators")]
    [Test(Description = "Distinct - Simple")]
    [Description("This sample uses DISTINCT to get all the categories of products.")]
    public void LinqToEntities45()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Product>().Select(o => o.Category).Distinct();

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Union - Simple")]
    [Description("This sample uses UNION to get all the orders where the shipping country was Mexico or Canada.")]
    public void LinqToEntities46()
    {
      var mexico = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Mexico").Select(o => o);
      var canada = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Canada").Select(o => o);
      var query = mexico.Union(canada);

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Union - With Distinct")]
    [Description("This sample uses UNION and DISTINCT to get all the employees from orders where the shipping country was Mexico or Canada.")]
    public void LinqToEntities47()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var mexico = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Mexico").Select(o => o);
      var canada = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Canada").Select(o => o);
      var union = mexico.Union(canada).Select(o => o.Employee);

      var query = union.Distinct();

      var actualMexico = Query.All<Order>().ToList()
        .Where(o => o.ShippingAddress.Country=="Mexico").Select(o => o);
      var actualCanada = Query.All<Order>().ToList()
        .Where(o => o.ShippingAddress.Country=="Canada").Select(o => o);
      var actualUnion = actualMexico.Union(actualCanada).Select(o => o.Employee);

      var actual = actualUnion.Distinct();

      Assert.AreEqual(0, actual.Select(o => o.Id).Except(query.ToList().Select(o => o.Id)).Count());

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Concat - Simple")]
    [Description("This sample uses CONCAT to get all orders where the shipping country was Mexico or Canada.")]
    public void LinqToEntities48()
    {
      var mexico = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Mexico").Select(o => o);
      var canada = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Canada").Select(o => o);

      var query = mexico.Concat(canada);

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Intersect - Simple 1")]
    [Description("This sample uses INTERSECT to get common employees where an order was shipped to Mexico or Canada.")]
    public void LinqToEntities49()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var mexico = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Mexico").Select(o => o.Employee);
      var canada = Query.All<Order>().Where(o => o.ShippingAddress.Country=="Canada").Select(o => o.Employee);

      var query = mexico.Intersect(canada);

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Intersect - Simple 2")]
    [Description("This sample uses INTERSECT to get common employees where an order was shipped to Mexico or Canada in one consolidated query.")]
    public void LinqToEntities50()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Order>()
        .Where(o => o.ShippingAddress.Country=="Mexico")
        .Select(o => o.Employee)
        .Intersect(Query.All<Order>()
          .Where(o => o.ShippingAddress.Country=="Canada")
          .Select(o => o.Employee));

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Except - Simple 1")]
    [Description("This sample uses EXCEPT to get employees who shipped orders to Mexico but not Canada.")]
    public void LinqToEntities51()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Order>()
        .Where(o => o.ShippingAddress.Country=="Mexico")
        .Select(o => o.Employee)
        .Except(Query.All<Order>()
          .Where(o => o.ShippingAddress.Country=="Canada")
          .Select(o => o.Employee));

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Except - Simple 2")]
    [Description("This sample uses EXCEPT to get employees with no orders sent to Mexico.")]
    public void LinqToEntities52()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Employee>().Select(e => e)
        .Except(Query.All<Order>().Where(o => o.ShippingAddress.Country=="Mexico").Select(o => o.Employee));

      QueryDumper.Dump(query);
    }

    #endregion

    #region Ordering and Grouping

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 1")]
    [Description("Select all customers ordered by ContactName.")]
    public void LinqToEntities53()
    {
      var query = from c in Query.All<Customer>()
      orderby c.ContactName
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 2")]
    [Description("Select all customers ordered by ContactName descending.")]
    public void LinqToEntities54()
    {
      var query = from c in Query.All<Customer>()
      orderby c.CompanyName descending
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 3")]
    [Description("Select an anonoymous type with all product IDs ordered by UnitInStock.")]
    public void LinqToEntities55()
    {
      var query = from p in Query.All<Product>()
      orderby p.UnitsInStock
      select new {p.Id};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 4")]
    [Description("Select an anonoymous type with all product IDs ordered by UnitInStock using LINQ operators.")]
    public void LinqToEntities56()
    {
      var query = Query.All<Product>().OrderBy(p => p.UnitsInStock)
        .Select(p2 => new {p2.Id});

      QueryDumper.Dump(query);
    }


    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending - Simple 1")]
    [Description("Select all customers ordered by the descending region.")]
    public void LinqToEntities57()
    {
      var query = from c in Query.All<Customer>()
      orderby c.Address.Region descending
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending - Simple 2")]
    [Description("Select all customers ordered by the descending region using LINQ operators.")]
    public void LinqToEntities58()
    {
      var query = Query.All<Customer>().Select(c => c).OrderByDescending(c2 => c2.Address.Region);

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy with ThenBy")]
    [Description("Select all customers ordered by the region, then the contact name.")]
    public void LinqToEntities59()
    {
      var query = Query.All<Customer>().Select(c => c).OrderBy(c => c.Address.Region).ThenBy(c => c.ContactName);

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending with ThenBy")]
    [Description("Select all customers ordered by the region in descending order, then the contact name.")]
    public void LinqToEntities60()
    {
      var query = Query.All<Customer>().Select(c => c).OrderByDescending(c => c.Address.Region).ThenBy(c => c.ContactName);

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy with ThenByDescending")]
    [Description("Select all customers ordered by the region then the contact name in descending order.")]
    public void LinqToEntities61()
    {
      var query = Query.All<Customer>().Select(c => c).OrderBy(c => c.Address.Region).ThenByDescending(c => c.ContactName);

      QueryDumper.Dump(query);
    }


    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending - Simple 3")]
    [Description("Select all products ordered by the descending unit price.")]
    public void LinqToEntities62()
    {
      var query = from p in Query.All<Product>()
      orderby p.UnitPrice descending
      select p;

      QueryDumper.Dump(query);
    }


    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - FK Collection")]
    [Description("Select all orders for a customer ordered by date that the order was placed.")]
    public void LinqToEntities63()
    {
      var query = Query.All<Customer>().Where(cust => cust.Id=="ALFKI")
        .SelectMany(c => c.Orders.Select(o => o))
        .OrderBy(o2 => o2.OrderDate);

      foreach (var order in query) {
        QueryDumper.Dump(order);
      }
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping - Simple 1")]
    [Description("Select all Regions with a customer.")]
    public void LinqToEntities64()
    {
      var query = from c in Query.All<Customer>()
      group c by c.Address.Region
      into regions
        select new {regions.Key};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping - Simple 2")]
    [Description("Select all dates with orders placed.")]
    public void LinqToEntities65()
    {
      var query = from o in Query.All<Order>()
      group o by o.OrderDate
      into dates
        select new {dates.Key};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping - Join 1")]
    [Description("Select all Regions and customer count for each region.")]
    public void LinqToEntities66()
    {
      var query = from c in Query.All<Customer>()
      group c by c.Address.Region
      into regions
        select new {Region = regions.Key, Count = regions.Count()};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping on Key")]
    [Description("Select all Regions and customer count for each region using LINQ operator.")]
    public void LinqToEntities67()
    {
      var query = Query.All<Customer>().GroupBy(c => c.Address.Region).Select(r => new {region = r.Key, count = r.Count()});

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping with a join on Key 1")]
    [Description("Select all Customer Regions with the total Freight on all orders for Customers in that Region.")]
    public void LinqToEntities68()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = from c in Query.All<Customer>()
      group c by c.Address.Region
      into regions
        join c2 in Query.All<Customer>() on regions.Key equals c2.Address.Region
        select new {region = regions.Key, total = c2.Orders.Sum(o => o.Freight)};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping with a Key 2")]
    [Description("Select all Customer Regions with the total Freight on all orders for Customers in that Region using LINQ operators.")]
    public void LinqToEntities69()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Customer>().GroupBy(c => c.Address.Region)
        .Select(g => new {
          Region = g.Key, FreightTotal = g
            .SelectMany(c2 => c2.Orders)
            .Sum(o => o.Freight)
        });

      QueryDumper.Dump(query);
    }

    #endregion

    #region Relationship Navigation

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection 1")]
    [Description("Select a sequence of all the orders for a customer using Select.")]
    public void LinqToEntities70()
    {
      var query = Query.All<Customer>().Where(cust => cust.Id=="ALFKI")
        .Select(c => c.Orders.Select(o => o));

      foreach (var order in query) {
        QueryDumper.Dump(order);
      }
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection 2")]
    [Description("Select all the orders for a customer using SelectMany.")]
    public void LinqToEntities71()
    {
      var query = Query.All<Customer>().Where(cust => cust.Id=="ALFKI").SelectMany(c => c.Orders);

      QueryDumper.Dump(query);
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection property")]
    [Description("Select all Employee IDs, and the count of the their orders.")]
    public void LinqToEntities72()
    {
      var query = from e in Query.All<Employee>()
      select new {e, orders = e.Orders.Select(o => o)};

      QueryDumper.Dump(query);
    }

    /* not enabled for Feb CTP
        [Category("Relationship Navigation")]
        [Test(Description = "Select - FK Collection property 2")]
        [Description("Select number of orders placed in 2002 for a customer.")]
        public void LinqToEntities74()
        {
            var query = Query.All<Customer>()
                .Where(cust => cust.CustomerID == "ALFKI")
                .SelectMany(c => c.Orders)
                .Where(o => o.OrderDate.Year == 2002);

            QueryDumper.Dump(query);
        }
        */

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection Aggregate property")]
    [Description("Select a customer and the sum of the freight of thier orders.")]
    public void LinqToEntities73()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Customer>().Where(cust => cust.Id=="ALFKI")
        .Select(c => c.Orders.Sum(o => o.Freight));

      QueryDumper.Dump(query);
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK collection predicate")]
    [Description("Select customers with an order where the shipping address is the same as the customers.")]
    public void LinqToEntities75()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Customer>().Where(cust => cust.Orders.Any(o => o.ShippingAddress==cust.Address)).Select(c2 => c2);

      QueryDumper.Dump(query);
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK collection Grouping")]
    [Description("Selects all regions with a customer, and shows the sum of orders for customers for each region.")]
    public void LinqToEntities76()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = from c in Query.All<Customer>()
      group c by c.Address.Region
      into regions
        join c2 in Query.All<Customer>() on regions.Key equals c2.Address.Region
        select new {region = regions.Key, total = c2.Orders.Sum(o => o.Freight)};

      QueryDumper.Dump(query);
    }

    #endregion

    #region Inheritance

    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - Simple")]
    [Description("Select all products, both active and discontinued products, and shows the type.")]
    public void LinqToEntities77()
    {
      var query = Query.All<Product>()
        .Select(p => p);

      // we need AsEnumerable to force execution, as GetType is not defined in store
      var query2 = query
        .ToList()
        .Select(p => new {type = p.GetType().ToString(), prod = p});

      QueryDumper.Dump(query2);
    }


    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - OfType - Simple 1")]
    [Description("Select only discontinued products.")]
    public void LinqToEntities78()
    {
      var query = Query.All<Product>().OfType<DiscontinuedProduct>().Select(p => p);

      QueryDumper.Dump(query);
    }


    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - OfType - Simple 2")]
    [Description("Select only products, which will reutrn all Products and subtypes of Products (DiscontinuedProducts and ActiveProducts).")]
    public void LinqToEntities79()
    {
      var query = Query.All<Product>().OfType<Product>().Select(p => p);

      QueryDumper.Dump(query);
    }

    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - Getting Supertype - OfType")]
    [Description("Select only active products.")]
    public void LinqToEntities80()
    {
      var query = Query.All<Product>().OfType<ActiveProduct>();

      QueryDumper.Dump(query);
    }

    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - Getting Supertype - Local")]
    [Description("Select only discontinued products.")]
    public void LinqToEntities81()
    {
      var query = Query.All<Product>().Where(p => p is DiscontinuedProduct);

      QueryDumper.Dump(query);
    }


    // Modified according to DO model.

    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "Complex Hierarchy - Simple")]
    [Description("Select all contacts and show the type of each.")]
    public void LinqToEntities82()
    {
      var query = Query.All<Person>().Select(c => c);

      // we need AsEnumerable to force execution, as GetType is not defined in store
      var query2 = query
        .ToList()
        .Select(c => new {type = c.GetType().ToString()});

      QueryDumper.Dump(query2);
    }

    // Modified according to DO model.

    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "Complex Hierarchy - OfType 1")]
    [Description("Select all Shipper contacts.")]
    public void LinqToEntities83()
    {
      var query = Query.All<Person>().OfType<Customer>().Select(c => c);

      QueryDumper.Dump(query);
    }

    // Modified according to DO model.

    [ExpectedException(typeof (QueryTranslationException))]
    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "Complex Hierarchy - OfType 2")]
    [Description("Select all Full contacts, which includes suppliers, customers, and employees.")]
    public void LinqToEntities84()
    {
      var query = Query.All<Person>().OfType<BusinessContact>().Select(c => c);

      QueryDumper.Dump(query);
    }

    /* not enabled for Feb CTP
        [Category("Table per Hierarchy Inheritance")]
        [Test(Description = "Complex Hierarchy - using supertype")]
        [Description("Select all Customers and Employees, cast as FullContacts to allow join.")]
        public void LinqToEntities85()
        {
            var query = northwindContext
                .Contacts
                .OfType<CustomerContact>()
                .Cast<FullContact>()
                .Union(northwindContext.Contacts.OfType<EmployeeContact>().Cast<FullContact>().Select(ec => ec ))
                .ToList()
                .Select(c => new {type = c.GetType().ToString(), companyName =  c.CompanyName } );

            QueryDumper.Dump(query);
        }*/

    // Disabled

    /*

        [Category("Table per Concrete Type Inheritance")]
        [Test(Description = "Simple")]
        [Description("Select all federated products and display thier types.")]
        public void LinqToEntities86()
        {
            var query = Query.All<Product>()Fedarated.ToList().Select(p => new { type = p.GetType().ToString(), p });

            QueryDumper.Dump(query);
        }

        [Category("Table per Concrete Type Inheritance")]
        [Test(Description = "OfType")]
        [Description("Select all discontinued federated products.")]
        public void LinqToEntities87()
        {
            var query = Query.All<Product>()Fedarated.OfType<DiscontinuedProductFedarated>().ToList().Select(p => new { type = p.GetType().ToString(), p });

            QueryDumper.Dump(query);
        }

        [Category("Table per Type Inheritance")]
        [Test(Description = "Simple")]
        [Description("Select all contacts and shows their types.")]
        public void LinqToEntities88()
        {
            var query = northwindContext.ContactsSplit.ToList().Select(c => new { type = c.GetType().ToString(), c });

            QueryDumper.Dump(query);
        }

        [Category("Table per Type Inheritance")]
        [Test(Description = "OfType 1")]
        [Description("Select all Customers.")]
        public void LinqToEntities89()
        {
            var query = northwindContext
                .ContactsSplit
                .OfType<CustomerContactSplit>()
                .ToList()
                .Select(c => new { type = c.GetType().ToString(), c });

            QueryDumper.Dump(query);
        }
*/


    /*
        [Category("Table per Type Inheritance")]
        [Test(Description = "OfType 2")]
        [Description("Select all Customers who are also employees, both as the base ContactSplit type (empty set).")]
        public void LinqToEntities90()
        {
            var query = northwindContext
                .ContactsSplit
                .OfType<CustomerContactSplit>()
                .Cast<ContactSplit>()
                .Intersect(northwindContext.ContactsSplit.OfType<EmployeeContactSplit>().Cast<ContactSplit>())
                .ToList()
                .Select(c => new { type = c.GetType().ToString(), c });

            QueryDumper.Dump(query);
        }
        */

    #endregion

    #region Runtime behavior closure

    private class MyClass
    {
      public static decimal Val = 50;

      public decimal GetVal()
      {
        return MyClass.Val;
      }
    }

    [Category("Runtime behavior example")]
    [Test(Description = "Static variable reference")]
    [Description("Uses a local variable as a query parameter.")]
    public void LinqToEntities91()
    {
      MyClass c = new MyClass();
      var query = Query.All<Order>().Where(o => o.Freight > MyClass.Val).Select(o => new {o.Freight, o});

      QueryDumper.Dump(query);
    }

    [Category("Runtime behavior example")]
    [Test(Description = "Query Parameters")]
    [Description("Uses a the value of the local variable at query execution time.")]
    public void LinqToEntities92()
    {
      decimal x = 50;

      var query = Query.All<Order>().Where(o => o.Freight > x).Select(o => new {o.Freight, o});

      x = 100;

      QueryDumper.Dump(query);
    }

    [Category("Runtime behavior example")]
    [Test(Description = "Deferred Execution and Query Parameters")]
    [Description("Each execution uses the current value of the local variable.")]
    public void LinqToEntities93()
    {
      decimal x = 100;

      var query = Query.All<Order>().Where(o => o.Freight > x).Select(o => new {o.Freight, o});

      QueryDumper.Dump(x);
      QueryDumper.Dump(query);

      x = 200;
      QueryDumper.Dump(x);
      QueryDumper.Dump(query);
    }

    #endregion

    // Data manipulations region removed

    // Object context region removed
  }
}