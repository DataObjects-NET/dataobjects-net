using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;


namespace Xtensive.Orm.Tests.Linq.Samples
{
  [Category("Linq")]
  [TestFixture]
  public class LinqToEntitiesSamples : ChinookDOModelTest
  {
    #region Restriction Operators

    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 1")]
    [Description("This sample uses WHERE to find all customers in Seattle.")]
    public void LinqToEntities1()
    {
      var query = from cust in Session.Query.All<Customer>()
      where cust.Address.City=="Seattle"
      select cust;
      QueryDumper.Dump(query);
    }


    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 2")]
    [Description("This sample uses WHERE to find all orders placed in 2005.")]
    public void LinqToEntities2()
    {
      DateTime dt = new DateTime(2005, 1, 1);
      var query = from invoice in Session.Query.All<Invoice>()
      where invoice.InvoiceDate > dt
      select invoice;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 3")]
    [Description("This sample uses WHERE to filter for Tracks that have duration less that 3 minutes and which Genre is Latin.")]
    public void LinqToEntities3()
    {
      var query = from t in Session.Query.All<Track>()
      where t.Milliseconds < 180000 && t.Genre.GenreId==282
      select t;

      QueryDumper.Dump(query);
    }


    [Category("Restriction Operators")]
    [Test(Description = "Where - Simple 4")]
    [Description("This sample uses WHERE to filter out Tracks that have a UnitPrice less than 1.")]
    public void LinqToEntities4()
    {
      var query = from p in Session.Query.All<Track>()
      where p.UnitPrice < 1
      select p;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Where - Related Entities 1")]
    [Description("This sample uses WHERE to get invoices for Customers in Mexico.")]
    public void LinqToEntities5()
    {
      var query = from i in Session.Query.All<Invoice>()
      where i.Customer.Address.Country=="Mexico"
      select i;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Where - Related Entities 2")]
    [Description("This sample uses WHERE to get invoices which is tracked by employees in the UK.")]
    public void LinqToEntities6()
    {
      var query = from i in Session.Query.All<Invoice>()
      where i.DesignatedEmployee.Address.Country=="UK"
      select i;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Any - 1")]
    [Description("This sample uses ANY to get employees have any invoices to track.")]
    public void LinqToEntities7()
    {
      var query = from e in Session.Query.All<Employee>()
      where e.Invoices.Any(i => i!=null)
      select e;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "Any - 2")]
    [Description("This sample uses ANY to check for any playlists which have any Pop tracks.")]
    public void LinqToEntities8()
    {
      var query = Session.Query.All<Playlist>()
        .Where(s => s.Tracks
          .Any(p => p.MediaType.MediaTypeId==284))
        .Select(s => s);

      QueryDumper.Dump(query);
    }


    [Category("Restriction Operators")]
    [Test(Description = "Any - Related Entities")]
    [Description("This sample uses WHERE and ANY to get employees who invoiced any customer in Mexico.")]
    public void LinqToEntities9()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var query = from e in Session.Query.All<Employee>()
      where e.Invoices.Any(i => i.Customer.Address.Country=="Mexico")
      select e;

      QueryDumper.Dump(query);
    }

    [Category("Restriction Operators")]
    [Test(Description = "All - Simple")]
    [Description("This sample uses ALL to get employees who invoiced customers not in Canada.")]
    public void LinqToEntities10()
    {
      Require.ProviderIsNot(StorageProvider.Oracle);
      var query = from e in Session.Query.All<Employee>()
      where e.Invoices.All(i => i.Customer.Address.Country!="Canada")
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
      var query = from c in Session.Query.All<Customer>()
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Simple 2")]
    [Description("This samples uses SELECT to get all Customer Company Names as Strings.")]
    public void LinqToEntities12()
    {
      var query = from c in Session.Query.All<Customer>()
      select c.CompanyName;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Anonymous 1")]
    [Description("This samples uses SELECT to get all Customer Company Names as an anonoymous type.")]
    public void LinqToEntities13()
    {
      var query = from c in Session.Query.All<Customer>()
      select new {c.CompanyName};

      QueryDumper.Dump(query);
    }


    [Category("Projection Operators")]
    [Test(Description = "Select - Anonymous 2")]
    [Description("This sample uses SELECT to get all Invoices as anonymous type")]
    public void LinqToEntities14()
    {
      var query = from i in Session.Query.All<Invoice>()
      select new {i};

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Anonymous 3")]
    [Description("This sample uses SELECT to get all Invoices and associated Employees as anonymous type")]
    public void LinqToEntities15()
    {
      var query = from i in Session.Query.All<Invoice>()
      select new {i, i.DesignatedEmployee};

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "Select - Nested Collection ")]
    [Description("This sample uses SELECT to get all Customers, and those Invoiices for each customer with a commission > 0.05")]
    public void LinqToEntities15a()
    {
      var query = Session.Query.All<Customer>().Select(c => new {Customer = c, Invoices = c.Invoices.Where(i => i.Commission > 0.05m)});

      QueryDumper.Dump(query);
    }


    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 1")]
    [Description("This sample uses SELECTMANY to get all Invoices for a Customer as a flat result")]
    public void LinqToEntities16()
    {
      var query = from c in Session.Query.All<Customer>()
      where c.CustomerId==4200
      from i in c.Invoices
      select i;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 2")]
    [Description("This sample uses SELECTMANY to get all Invoices for a Customer as a flat result using LINQ operators")]
    public void LinqToEntities17()
    {
      var query = Session.Query.All<Customer>().Where(cust => cust.CustomerId==4200)
        .SelectMany(cust => cust.Invoices);

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 3")]
    [Description("This sample uses SELECTMANY to get all Invoices for Customers in Denmark as a flat result")]
    public void LinqToEntities18()
    {
      var query = from c in Session.Query.All<Customer>()
      where c.Address.Country=="Denmark"
      from i in c.Invoices
      select i;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Simple 4")]
    [Description("This sample uses SELECTMANY to get all Invoices for Customers in Denmark as a flat result using LINQ operators")]
    public void LinqToEntities19()
    {
      var query = Session.Query.All<Customer>().Where(cust => cust.Address.Country=="Denmark")
        .SelectMany(cust => cust.Invoices);

      QueryDumper.Dump(query);
    }


    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Predicate 1")]
    [Description("This sample uses SELECTMANY to get all Invoices for Customers in Denmark as a flat result")]
    public void LinqToEntities20()
    {
      var query = from c in Session.Query.All<Customer>()
      where c.Address.Country=="Denmark"
      from i in c.Invoices
      where i.Commission > 0.05m
      select i;

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Predicate 2")]
    [Description("This sample uses SELECTMANY to get all Invoices for Customers in Denmark as an anonymous type containing the Invoice and Customer flat result")]
    public void LinqToEntities21()
    {
      var query = from c in Session.Query.All<Customer>()
      where c.Address.Country=="Denmark"
      from i in c.Invoices
      where i.Commission > 0.05m
      select new {c, i};

      QueryDumper.Dump(query);
    }

    [Category("Projection Operators")]
    [Test(Description = "SelectMany - Predicate 3")]
    [Description("This sample uses SELECTMANY to get all Invoices for Customers in Denmark as a flat result using LINQ opeartors")]
    public void LinqToEntities22()
    {
      var query = Session.Query.All<Customer>().Where(cust => cust.Address.Country=="Denmark")
        .SelectMany(cust => cust.Invoices.Where(i => i.Commission > 0.05m));

      QueryDumper.Dump(query);
    }

    #endregion

    #region Aggregate Operators

    [Category("Aggregate Operators")]
    [Test(Description = "Count - Simple")]
    [Description("This sample uses COUNT to get the number of Invoices.")]
    public void LinqToEntities23()
    {
      var query = Session.Query.All<Invoice>().Count();

      QueryDumper.Dump(query);
    }


    [Category("Aggregate Operators")]
    [Test(Description = "Count - Predicate 1")]
    [Description("This sample uses COUNT to get the number of Invoices placed by Customers in Mexico.")]
    public void LinqToEntities24()
    {
      var query = Session.Query.All<Invoice>().Where(i => i.Customer.Address.Country=="Mexico").Count();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Count - Predicate 2")]
    [Description("This sample uses COUNT to get the number of Invoices shipped to Mexico.")]
    public void LinqToEntities25()
    {
      var query = Session.Query.All<Invoice>()
        .Where(i => i.BillingAddress.Country=="Mexico").Count();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Sum - Simple 1")]
    [Description("This sample uses SUM to find the total Commission over all Invoices.")]
    public void LinqToEntities26()
    {
      var query = Session.Query.All<Invoice>().Select(i => i.Commission).Sum();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Sum - Simple 2")]
    [Description("This sample uses SUM to find the total duration of units on order over all Tracks.")]
    public void LinqToEntities27()
    {
      var query = Session.Query.All<Track>().Sum(p => p.Milliseconds);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Sum - Simple 3")]
    [Description("This sample uses SUM to find the total duration of units on order over all Tracks which are Classic.")]
    public void LinqToEntities28()
    {
      var query = Session.Query.All<Track>().Where(p => p.Genre.GenreId==299).Sum(p => p.Milliseconds);

      QueryDumper.Dump(query);
    }


    [Category("Aggregate Operators")]
    [Test(Description = "Min - Simple 1")]
    [Description("This sample uses MIN to find the lowest unit price of any Tracks.")]
    public void LinqToEntities29()
    {
      var query = Session.Query.All<Track>().Select(p => p.UnitPrice).Min();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Min - Simple 2")]
    [Description("This sample uses MIN to find the lowest commission of any Invoice.")]
    public void LinqToEntities30()
    {
      var query = Session.Query.All<Invoice>().Min(i => i.Commission);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Min - Predicate")]
    [Description("This sample uses MIN to find the lowest commission of any invoice billed from Mexico.")]
    public void LinqToEntities31()
    {
      var query = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Mexico").Min(i => i.Commission);

      QueryDumper.Dump(query);
    }


    [Category("Aggregate Operators")]
    [Test(Description = "Min - Grouping")]
    [Description("This sample uses Min to find the Tracks that have the lowest unit price for each media type, and returns the result as an anonoymous type.")]
    public void LinqToEntities32()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Track>()
        .GroupBy(p => p.MediaType)
        .Select(g => new {
        MediaTypeId = g.Key,
        CheapestTracks =
          g.Where(p2 => p2.UnitPrice==g.Min(p3 => p3.UnitPrice))
      });

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Simple 1")]
    [Description("This sample uses MAX to find the latest hire date of any Employee.")]
    public void LinqToEntities33()
    {
      var query = Session.Query.All<Employee>().Select(e => e.HireDate).Max();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Simple 2")]
    [Description("This sample uses MAX to find the longest units of any Tracks.")]
    public void LinqToEntities34()
    {
      var query = Session.Query.All<Track>().Max(p => p.Milliseconds);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Predicate")]
    [Description("This sample uses MAX to find the longes units of any Track with MediaTypeId = 305.")]
    public void LinqToEntities35()
    {
      var query = Session.Query.All<Track>().Where(p => p.MediaType.MediaTypeId==305).Max(p => p.Milliseconds);
      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Max - Grouping")]
    [Description("This sample uses MAX to find the Tracks that have the highest unit price for each media type, and returns the result as an anonoymous type.")]
    public void LinqToEntities36()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = from p in Session.Query.All<Track>()
      group p by p.MediaType
      into g
        select new {
          g.Key,
          MostExpensiveTracks =
            from t2 in g
            where t2.UnitPrice==g.Max(t3 => t3.UnitPrice)
            select t2
        };

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Simple 1")]
    [Description("This sample uses AVERAGE to find the average commission of all Invoices.")]
    public void LinqToEntities37()
    {
      var query = Session.Query.All<Invoice>().Select(i => i.Commission).Average();

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Simple 2")]
    [Description("This sample uses AVERAGE to find the average unit price of all Tracks.")]
    public void LinqToEntities38()
    {
      var query = Session.Query.All<Track>().Average(p => p.UnitPrice);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Predicate")]
    [Description("This sample uses AVERAGE to find the average unit price of all Tracks with MediaTypeId = 305.")]
    public void LinqToEntities39()
    {
      var query = Session.Query.All<Track>().Where(p => p.MediaType.MediaTypeId==305).Average(p => p.UnitPrice);

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Grouping 1")]
    [Description("This sample uses AVERAGE to find the Products that have unit price higher than the average unit price of the category for each category.")]
    public void LinqToEntities40()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = from p in Session.Query.All<Track>()
      group p by p.MediaType
      into g
        select new {
          g.Key,
          ExpensiveProducts =
            from t2 in g
            where t2.UnitPrice > g.Average(t3 => t3.UnitPrice)
            select t2
        };

      QueryDumper.Dump(query);
    }

    [Category("Aggregate Operators")]
    [Test(Description = "Average - Grouping 2")]
    [Description("This sample uses AVERAGE to find the average unit price of each media type.")]
    public void LinqToEntities41()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Track>()
        .GroupBy(t => t.MediaType)
        .Select(g => new {
          g.Key,
          Average = g.Average(t => t.UnitPrice)
        });

      QueryDumper.Dump(query);
    }

    #endregion

    #region Set Operators

    [Category("Set And Element Operators")]
    [Test(Description = "First - Simple")]
    [Description("This sample uses FIRST and WHERE to get the first (database order) order that is ordered from Paris. The WHERE predicate is evaluated on the server.")]
    public void LinqToEntities42()
    {
      var query = from i in Session.Query.All<Invoice>()
      where i.BillingAddress.City=="Paris"
      select i;

      // Feb CTP requires AsEnumerable()
      var result = query.ToList().First();

      QueryDumper.Dump(result);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "First - Predicate")]
    [Description("This sample uses FIRST to get the first (database order) order that is ordered from Dublin. The predicate is evaluated on the client.")]
    public void LinqToEntities43()
    {
      var query = from o in Session.Query.All<Invoice>()
      select o;

      // Feb CTP requires AsEnumerable()
      var result = query
        .ToList()
        .First(x => x.BillingAddress.City=="Dublin");

      QueryDumper.Dump(result);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "First - Ordered")]
    [Description("This sample uses FIRST, WHERE and ORDER BY to get the first order that is ordered from Oslo, ordered by date. The predicate is evaluated on the server.")]
    public void LinqToEntities44()
    {
      var query = from i in Session.Query.All<Invoice>()
      where i.BillingAddress.City=="Oslo"
      orderby i.InvoiceDate
      select i;

      // Feb CTP requires AsEnumerable()
      var result = query.ToList().First();

      QueryDumper.Dump(result);
    }


    [Category("Set And Element Operators")]
    [Test(Description = "Distinct - Simple")]
    [Description("This sample uses DISTINCT to get all the media types of tracks.")]
    public void LinqToEntities45()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Session.Query.All<Track>().Select(i => i.MediaType).Distinct();

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Union - Simple")]
    [Description("This sample uses UNION to get all the invoices where the order country was Mexico or Canada.")]
    public void LinqToEntities46()
    {
      var mexico = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Mexico").Select(i => i);
      var canada = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Canada").Select(i => i);
      var query = mexico.Union(canada);

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Union - With Distinct")]
    [Description("This sample uses UNION and DISTINCT to get all the employees from invoices where the order country was Mexico or Canada.")]
    public void LinqToEntities47()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var mexico = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Mexico").Select(i => i);
      var canada = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Canada").Select(i => i);
      var union = mexico.Union(canada).Select(i => i.DesignatedEmployee);

      var query = union.Distinct();

      var actualMexico = Session.Query.All<Invoice>().ToList()
        .Where(i => i.BillingAddress.Country=="Mexico").Select(i => i);
      var actualCanada = Session.Query.All<Invoice>().ToList()
        .Where(i => i.BillingAddress.Country=="Canada").Select(i => i);
      var actualUnion = actualMexico.Union(actualCanada).Select(i => i.DesignatedEmployee);

      var actual = actualUnion.Distinct();

      Assert.AreEqual(0, actual.Select(e => e.EmployeeId).Except(query.ToList().Select(e => e.EmployeeId)).Count());

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Concat - Simple")]
    [Description("This sample uses CONCAT to get all invoices where the shipping country was Mexico or Canada.")]
    public void LinqToEntities48()
    {
      var mexico = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Mexico").Select(i => i);
      var canada = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Canada").Select(i => i);

      var query = mexico.Concat(canada);

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Intersect - Simple 1")]
    [Description("This sample uses INTERSECT to get common employees where an invoices was ordered from Mexico or Canada.")]
    public void LinqToEntities49()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var mexico = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Mexico").Select(i => i.DesignatedEmployee);
      var canada = Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Canada").Select(i => i.DesignatedEmployee);

      var query = mexico.Intersect(canada);

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Intersect - Simple 2")]
    [Description("This sample uses INTERSECT to get common employees where an invoice was ordered from Mexico or Canada in one consolidated query.")]
    public void LinqToEntities50()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var query = Session.Query.All<Invoice>()
        .Where(i => i.BillingAddress.Country=="Mexico")
        .Select(i => i.DesignatedEmployee)
        .Intersect(Session.Query.All<Invoice>()
          .Where(i => i.BillingAddress.Country=="Canada")
          .Select(i => i.DesignatedEmployee));

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Except - Simple 1")]
    [Description("This sample uses EXCEPT to get employees who manages invoices to Mexico but not Canada.")]
    public void LinqToEntities51()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var query = Session.Query.All<Invoice>()
        .Where(i => i.BillingAddress.Country=="Mexico")
        .Select(i => i.DesignatedEmployee)
        .Except(Session.Query.All<Invoice>()
          .Where(i => i.BillingAddress.Country=="Canada")
          .Select(i => i.DesignatedEmployee));

      QueryDumper.Dump(query);
    }

    [Category("Set And Element Operators")]
    [Test(Description = "Except - Simple 2")]
    [Description("This sample uses EXCEPT to get employees with no invoices sent to Mexico.")]
    public void LinqToEntities52()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var query = Session.Query.All<Employee>().Select(e => e)
        .Except(Session.Query.All<Invoice>().Where(i => i.BillingAddress.Country=="Mexico").Select(i => i.DesignatedEmployee));

      QueryDumper.Dump(query);
    }

    #endregion

    #region Ordering and Grouping

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 1")]
    [Description("Select all customers ordered by CompanyName.")]
    public void LinqToEntities53()
    {
      var query = from c in Session.Query.All<Customer>()
      orderby c.CompanyName
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 2")]
    [Description("Select all customers ordered by CompanyName descending.")]
    public void LinqToEntities54()
    {
      var query = from c in Session.Query.All<Customer>()
      orderby c.CompanyName descending
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 3")]
    [Description("Select an anonoymous type with all track Ids ordered by Milliseconds.")]
    public void LinqToEntities55()
    {
      var query = from p in Session.Query.All<Track>()
      orderby p.Milliseconds
      select new {p.TrackId};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - Simple 4")]
    [Description("Select an anonoymous type with all track Ids ordered by Milliseconds using LINQ operators.")]
    public void LinqToEntities56()
    {
      var query = Session.Query.All<Track>().OrderBy(p => p.Milliseconds)
        .Select(p2 => new {p2.TrackId});

      QueryDumper.Dump(query);
    }


    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending - Simple 1")]
    [Description("Select all customers ordered by the descending state.")]
    public void LinqToEntities57()
    {
      var query = from c in Session.Query.All<Customer>()
      orderby c.Address.State descending
      select c;

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending - Simple 2")]
    [Description("Select all customers ordered by the descending state using LINQ operators.")]
    public void LinqToEntities58()
    {
      var query = Session.Query.All<Customer>().Select(c => c).OrderByDescending(c2 => c2.Address.State);

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy with ThenBy")]
    [Description("Select all customers ordered by the region, then the last name.")]
    public void LinqToEntities59()
    {
      var query = Session.Query.All<Customer>().Select(c => c).OrderBy(c => c.Address.State).ThenBy(c => c.LastName);

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending with ThenBy")]
    [Description("Select all customers ordered by the state in descending order, then the last name.")]
    public void LinqToEntities60()
    {
      var query = Session.Query.All<Customer>().Select(c => c).OrderByDescending(c => c.Address.State).ThenBy(c => c.LastName);

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy with ThenByDescending")]
    [Description("Select all customers ordered by the state then the last name in descending order.")]
    public void LinqToEntities61()
    {
      var query = Session.Query.All<Customer>().Select(c => c).OrderBy(c => c.Address.State).ThenByDescending(c => c.LastName);

      QueryDumper.Dump(query);
    }


    [Category("Ordering and Grouping")]
    [Test(Description = "OrderByDescending - Simple 3")]
    [Description("Select all tracks ordered by the descending unit price.")]
    public void LinqToEntities62()
    {
      var query = from p in Session.Query.All<Track>()
      orderby p.UnitPrice descending
      select p;

      QueryDumper.Dump(query);
    }


    [Category("Ordering and Grouping")]
    [Test(Description = "OrderBy - FK Collection")]
    [Description("Select all orders for a customer ordered by date that the invoice was placed.")]
    public void LinqToEntities63()
    {
      var query = Session.Query.All<Customer>().Where(cust => cust.CustomerId==4233)
        .SelectMany(c => c.Invoices.Select(i => i))
        .OrderBy(i2 => i2.InvoiceDate);

      foreach (var order in query) {
        QueryDumper.Dump(order);
      }
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping - Simple 1")]
    [Description("Select all states with a customer.")]
    public void LinqToEntities64()
    {
      var query = from c in Session.Query.All<Customer>()
      group c by c.Address.State
      into states
        select new {states.Key};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping - Simple 2")]
    [Description("Select all dates with orders placed.")]
    public void LinqToEntities65()
    {
      var query = from i in Session.Query.All<Invoice>()
      group i by i.InvoiceDate
      into dates
        select new {dates.Key};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping - Join 1")]
    [Description("Select all States and customer count for each state.")]
    public void LinqToEntities66()
    {
      var query = from c in Session.Query.All<Customer>()
      group c by c.Address.State
      into states
        select new {State = states.Key, Count = states.Count()};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping on Key")]
    [Description("Select all States and customer count for each state using LINQ operator.")]
    public void LinqToEntities67()
    {
      var query = Session.Query.All<Customer>().GroupBy(c => c.Address.State).Select(r => new {state = r.Key, count = r.Count()});

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping with a join on Key 1")]
    [Description("Select all Customer states with the total on all invoices for Customers in that state.")]
    public void LinqToEntities68()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var query = from c in Session.Query.All<Customer>()
      group c by c.Address.State
      into states
        join c2 in Session.Query.All<Customer>() on states.Key equals c2.Address.State
        select new {state = states.Key, total = c2.Invoices.Sum(i => i.Total)};

      QueryDumper.Dump(query);
    }

    [Category("Ordering and Grouping")]
    [Test(Description = "Grouping with a Key 2")]
    [Description("Select all Customer State with the total Commission on all orders for Customers in that State using LINQ operators.")]
    public void LinqToEntities69()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var query = Session.Query.All<Customer>().GroupBy(c => c.Address.State)
        .Select(g => new {
          Region = g.Key, CommissionTotal = g
            .SelectMany(c2 => c2.Invoices)
            .Sum(i => i.Commission)
        });

      QueryDumper.Dump(query);
    }

    #endregion

    #region Relationship Navigation

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection 1")]
    [Description("Select a sequence of all the invoices for a customer using Select.")]
    public void LinqToEntities70()
    {
      var query = Session.Query.All<Customer>().Where(cust => cust.CustomerId==4200)
        .Select(c => c.Invoices.Select(i => i));

      foreach (var order in query) {
        QueryDumper.Dump(order);
      }
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection 2")]
    [Description("Select all the invoices for a customer using SelectMany.")]
    public void LinqToEntities71()
    {
      var query = Session.Query.All<Customer>().Where(cust => cust.CustomerId==4200).SelectMany(c => c.Invoices);

      QueryDumper.Dump(query);
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection property")]
    [Description("Select all Employee Ids, and the count of the their invoices.")]
    public void LinqToEntities72()
    {
      var query = from e in Session.Query.All<Employee>()
      select new {e, orders = e.Invoices.Select(i => i)};

      QueryDumper.Dump(query);
    }

    /* not enabled for Feb CTP
        [Category("Relationship Navigation")]
        [Test(Description = "Select - FK Collection property 2")]
        [Description("Select number of orders placed in 2002 for a customer.")]
        public void LinqToEntities74()
        {
            var query = Session.Query.All<Customer>()
                .Where(cust => cust.CustomerID == "ALFKI")
                .SelectMany(c => c.Orders)
                .Where(i => i.OrderDate.Year == 2002);

            QueryDumper.Dump(query);
        }
        */

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK Collection Aggregate property")]
    [Description("Select a customer and the sum of the commission of thier invoices.")]
    public void LinqToEntities73()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var query = Session.Query.All<Customer>().Where(cust => cust.CustomerId==4233)
        .Select(c => c.Invoices.Sum(i => i.Commission));

      QueryDumper.Dump(query);
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK collection predicate")]
    [Description("Select customers with an invoice where the billing address is the same as the customers.")]
    public void LinqToEntities75()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Session.Query.All<Customer>().Where(cust => cust.Invoices.Any(i => i.BillingAddress==cust.Address)).Select(c2 => c2);

      QueryDumper.Dump(query);
    }

    [Category("Relationship Navigation")]
    [Test(Description = "Select - FK collection Grouping")]
    [Description("Selects all states with a customer, and shows the sum of invoices for customers for each state.")]
    public void LinqToEntities76()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var query = from c in Session.Query.All<Customer>()
      group c by c.Address.State
      into states
        join c2 in Session.Query.All<Customer>() on states.Key equals c2.Address.State
        select new {state = states.Key, total = c2.Invoices.Sum(i => i.Total)};

      QueryDumper.Dump(query);
    }

    #endregion

    #region Inheritance

    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - Simple")]
    [Description("Select all tracks, both audio and video trackss, and shows the type.")]
    public void LinqToEntities77()
    {
      var query = Session.Query.All<Track>()
        .Select(p => p);

      // we need AsEnumerable to force execution, as GetType is not defined in store
      var query2 = query
        .ToList()
        .Select(p => new {type = p.GetType().ToString(), prod = p});

      QueryDumper.Dump(query2);
    }


    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - OfType - Simple 1")]
    [Description("Select only video tracks.")]
    public void LinqToEntities78()
    {
      var query = Session.Query.All<Track>().OfType<VideoTrack>().Select(p => p);

      QueryDumper.Dump(query);
    }


    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - OfType - Simple 2")]
    [Description("Select only products, which will reutrn all Tracks and subtypes of Tracks (video and audio tracks).")]
    public void LinqToEntities79()
    {
      var query = Session.Query.All<Track>().OfType<Track>().Select(p => p);

      QueryDumper.Dump(query);
    }

    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - Getting Supertype - OfType")]
    [Description("Select only audio tracks.")]
    public void LinqToEntities80()
    {
      var query = Session.Query.All<Track>().OfType<AudioTrack>();

      QueryDumper.Dump(query);
    }

    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "One Level Hierarchy - Getting Supertype - Local")]
    [Description("Select only video tracks.")]
    public void LinqToEntities81()
    {
      var query = Session.Query.All<Track>().Where(p => p is VideoTrack);

      QueryDumper.Dump(query);
    }


    // Modified according to DO model.
    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "Complex Hierarchy - Simple")]
    [Description("Select all contacts and show the type of each.")]
    public void LinqToEntities82()
    {
      var query = Session.Query.All<Person>().Select(c => c);

      // we need AsEnumerable to force execution, as GetType is not defined in store
      Assert.Throws<QueryTranslationException>(() => {
        var query2 = query
          .ToList()
          .Select(c => new { type = c.GetType().ToString() });

        QueryDumper.Dump(query2);
      });

    }

    // Modified according to DO model.
    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "Complex Hierarchy - OfType 1")]
    [Description("Select all Customer contacts.")]
    public void LinqToEntities83()
    {
      var query = Session.Query.All<Person>().OfType<Customer>().Select(c => c);

      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(query));
    }

    // Modified according to DO model.
    [Category("Table per Hierarchy Inheritance")]
    [Test(Description = "Complex Hierarchy - OfType 2")]
    [Description("Select all Full contacts, which includes customers and employees.")]
    public void LinqToEntities84()
    {
      var query = Session.Query.All<Person>().OfType<BusinessContact>().Select(c => c);

      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(query));
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
            var query = Session.Query.All<Product>()Fedarated.ToList().Select(p => new { type = p.GetType().ToString(), p });

            QueryDumper.Dump(query);
        }

        [Category("Table per Concrete Type Inheritance")]
        [Test(Description = "OfType")]
        [Description("Select all discontinued federated products.")]
        public void LinqToEntities87()
        {
            var query = Session.Query.All<Product>()Fedarated.OfType<DiscontinuedProductFedarated>().ToList().Select(p => new { type = p.GetType().ToString(), p });

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
      var query = Session.Query.All<Invoice>().Where(i => i.Commission > MyClass.Val).Select(i => new {i.Commission, i});

      QueryDumper.Dump(query);
    }

    [Category("Runtime behavior example")]
    [Test(Description = "Query Parameters")]
    [Description("Uses a the value of the local variable at query execution time.")]
    public void LinqToEntities92()
    {
      decimal x = 50;

      var query = Session.Query.All<Invoice>().Where(i => i.Commission > x).Select(i => new {i.Commission, i});

      x = 100;

      QueryDumper.Dump(query);
    }

    [Category("Runtime behavior example")]
    [Test(Description = "Deferred Execution and Query Parameters")]
    [Description("Each execution uses the current value of the local variable.")]
    public void LinqToEntities93()
    {
      decimal x = 100;

      var query = Session.Query.All<Invoice>().Where(i => i.Commission > x).Select(i => new {i.Commission, i});

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