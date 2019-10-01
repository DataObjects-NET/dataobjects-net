using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq.Samples
{
  [Category("Linq")]
  public class LinqToSqlSamples : ChinookDOModelTest
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
      "during or after 2006.")]
    public void DLinq2()
    {
      var q =
        from e in Session.Query.All<Employee>()
        where e.HireDate >= new DateTime(2006, 1, 1)
        select e;

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 3")]
    [Description("This sample uses WHERE to filter for tracks that not longer than 3 minutes " +
      "and are not videos.")]
    public void DLinq3()
    {
      var q =
        from t in Session.Query.All<Track>()
        where t.Milliseconds <= 180000 && !(t is VideoTrack)
        select t;

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 4")]
    [Description("This sample uses WHERE to filter out Products that are either " +
      "UnitPrice is greater than 10 or is video.")]
    public void DLinq4()
    {
      var q =
        from t in Session.Query.All<Track>()
        where t.UnitPrice > 10m || (t is VideoTrack)
        select t;

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "Where - 5")]
    [Description("This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10" +
      " and is video.")]
    public void DLinq5()
    {
      var q =
        Session.Query.All<Track>().Where(t => t.UnitPrice > 10m).Where(t => (t is VideoTrack));

      QueryDumper.Dump(q);
    }

    [Category("WHERE")]
    [Test(Description = "First - Simple")]
    [Description("This sample uses First to select the first Playlist in the table.")]
    public void DLinq6()
    {
      Playlist playlist = Session.Query.All<Playlist>().First();
      QueryDumper.Dump(playlist);
    }

    [Category("WHERE")]
    [Test(Description = "First - Element")]
    [Description("This sample uses First to select the single Customer with Id '4200'.")]
    public void DLinq7()
    {
      Customer cust = Session.Query.All<Customer>().First(c => c.CustomerId==4200);
      QueryDumper.Dump(cust);
    }

    [Category("WHERE")]
    [Test(Description = "First - Condition")]
    [Description("This sample uses First to select an Order with Commission greater than 0.10.")]
    public void DLinq8()
    {
      Invoice inv = Session.Query.All<Invoice>().First(i => i.Commission > 0.10M);
      QueryDumper.Dump(inv);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Simple")]
    [Description("This sample uses SELECT to return a sequence of just the " +
      "Customers' company names.")]
    public void DLinq9()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select c.CompanyName;

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Anonymous Type 1")]
    [Description("This sample uses SELECT and anonymous types to return " +
      "a sequence of just the Customers' last names and phone numbers.")]
    public void DLinq10()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select new {c.LastName, c.Phone};

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
        select new {Name = e.FirstName + " " + e.LastName, Phone = e.Phone};

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Anonymous Type 3")]
    [Description("This sample uses SELECT and anonymous types to return " +
      "a sequence of all Tracks' IDs and a calculated value " +
        "called HalfPrice which is set to the Trask's UnitPrice " +
          "divided by 2.")]
    public void DLinq12()
    {
      var q =
        from t in Session.Query.All<Track>()
        select new {t.TrackId, HalfPrice = t.UnitPrice / 2};
      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Conditional ")]
    [Description("This sample uses SELECT and a conditional statment to return a sequence of invoices " +
      " Id and payment status.")]
    public void DLinq13()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        select new {i.InvoiceId, PaymentStatus = i.PaymentDate.HasValue && i.PaymentDate.Value < DateTime.UtcNow ? "Paid" : "Not yet paid"};

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
      "just the London Customers' last names.")]
    public void DLinq15()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.Address.City=="London"
        select c.LastName;

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
          c.CustomerId,
          CustomerInfo = new {c.CompanyName, c.Address.City, c.Address.Country},
          ContactInfo = new {c.Phone, c.Email, c.Fax}
        };

      QueryDumper.Dump(q);
    }

    [Category("SELECT/DISTINCT")]
    [Test(Description = "Select - Nested ")]
    [Description("This sample uses nested queries to return a sequence of " +
      "all Invoices containing their InvoiceID, a subsequence of the " +
        "items in the invoice with cost less than 1 dollar, and commission.")]
    public void DLinq17()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        select new {
          i.InvoiceId,
          DiscountedProducts =
            from il in i.InvoiceLines
            where il.UnitPrice < 1.0m
            select il,
          i.Commission
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
    [Description("This sample uses Count to find the number of Track in the database " +
      "that are not videos.")]
    public void DLinq20()
    {
      var q = Session.Query.All<Track>().Count(t => !(t is VideoTrack));
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Sum - Simple")]
    [Description("This sample uses Sum to find the Commission over all Orders.")]
    public void DLinq21()
    {
      var q = Session.Query.All<Invoice>().Select(o => o.Commission).Sum();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Sum - Mapped")]
    [Description("This sample uses Sum to find the total number of milliseconds over all Tracks.")]
    public void DLinq22()
    {
      var q = Session.Query.All<Track>().Sum(t => t.Milliseconds);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Min - Simple")]
    [Description("This sample uses Min to find the lowest unit price of any Track.")]
    public void DLinq23()
    {
      var q = Session.Query.All<Track>().Select(t => t.UnitPrice).Min();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Min - Mapped")]
    [Description("This sample uses Min to find the lowest Commission of any Invoice.")]
    public void DLinq24()
    {
      var q = Session.Query.All<Invoice>().Min(i => i.Commission);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Min - Elements")]
    [Description("This sample uses Min to find the Tracks that have the lowest unit price " +
      "in each category.")]
    public void DLinq25()
    {
      var categories =
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            Id = g.Key,
            CheapestTracks =
              from t2 in g
              where t2.UnitPrice==g.Min(t3 => t3.UnitPrice)
              select t2
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
    [Description("This sample uses Max to find the longest duration of Tracks.")]
    public void DLinq27()
    {
      var q = Session.Query.All<Track>().Max(t => t.Milliseconds);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Max - Elements")]
    [Description("This sample uses Max to find the Tracks that have the highest unit price " +
      "in each category.")]
    public void DLinq28()
    {
      var categories =
        Session.Query.All<Track>()
          .GroupBy(t => t.TrackId)
          .Select(g => new {
            g.Key,
            MostExpensiveTracks = g.Where(p2 => p2.UnitPrice==g.Max(p3 => p3.UnitPrice))
          });

      QueryDumper.Dump(categories);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Average - Simple")]
    [Description("This sample uses Average to find the average Commission of all Invoices.")]
    public void DLinq29()
    {
      var q = Session.Query.All<Invoice>().Select(i => i.Commission).Average();
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Average - Mapped")]
    [Description("This sample uses Average to find the average unit price of all Tracks.")]
    public void DLinq30()
    {
      var q = Session.Query.All<Track>().Average(t => t.UnitPrice);
      Console.WriteLine(q);
    }

    [Category("COUNT/SUM/MIN/MAX/AVG")]
    [Test(Description = "Average - Elements")]
    [Description("This sample uses Average to find the Tracjs that have unit price higher than " +
      "the average unit price of the category for each category.")]
    public void DLinq31()
    {
      var categories =
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            ExpensiveTracks =
              from t2 in g
              where t2.UnitPrice > g.Average(t3 => t3.UnitPrice)
              select t2
          };

      QueryDumper.Dump(categories);
    }


    [Category("JOIN")]
    [Test(Description = "SelectMany - 1 to Many - 1")]
    [Description("This sample uses foreign key navigation in the " +
      "from clause to select all Invoices for Customers in London.")]
    public void DLinqJoin1()
    {
      var q =
        from c in Session.Query.All<Customer>()
        from i in c.Invoices
        where c.Address.City=="London"
        select i;

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "SelectMany - 1 to Many - 2")]
    [Description("This sample uses foreign key navigation in the " +
      "where clause to filter for Invoices whose Customer is in the USA " +
        "that are paid.")]
    public void DLinqJoin2()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        where i.Customer.Address.Country=="USA" && i.Status==InvoiceStatus.Paid
        select i;

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "SelectMany - Many to Many")]
    [Description("This sample uses foreign key navigation in the " +
      "from clause to filter for Tracks by Jon Bon Jovi, " +
        "and also list their price and prlaylist which includes them.")]
    public void DLinqJoin3()
    {
      var q =
        from t in Session.Query.All<Track>()
        from pl in t.Playlists
        where t.Composer=="Jon Bon Jovi"
        select new {TrackName = t.Name, t.UnitPrice, Playlist = pl.Name};

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
        join i in Session.Query.All<Invoice>() on c.CustomerId equals i.Customer.CustomerId into invoices
        select new {c.LastName, OrderCount = invoices.Count()};

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
        join i in Session.Query.All<Invoice>() on c.CustomerId equals i.Customer.CustomerId into invoices
        join e in Session.Query.All<Employee>() on c.Address.City equals e.Address.City into emps
        select new {c.LastName, invoices = invoices.Count(), emps = emps.Count()};

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - LEFT OUTER JOIN")]
    [Description("This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty(). The DefaultIfEmpty() method returns null when there is no Invoice for the Employee.")]
    public void DLinqJoin7()
    {
      var query =
        from employee in Session.Query.All<Employee>()
        join invoice in Session.Query.All<Invoice>() on employee equals invoice.DesignatedEmployee into invoiceJoins
        from invoiceJoin in invoiceJoins.DefaultIfEmpty()
        select new {employee.FirstName, employee.LastName, Order = invoiceJoin};

      QueryDumper.Dump(query);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - Projected let assignment")]
    [Description("This sample projects a 'let' expression resulting from a join.")]
    public void DLinqJoin8()
    {
      var q =
        from c in Session.Query.All<Customer>()
        join i in Session.Query.All<Invoice>() on c.CustomerId equals i.Customer.CustomerId into invoices
        let z = c.Address.City + c.Address.Country
        from i in invoices
        select new {c.LastName, i.InvoiceId, z};

      QueryDumper.Dump(q);
    }

    [Ignore("Too slow")]
    [Category("JOIN")]
    [Test(Description = "GroupJoin - Composite Key")]
    [Description("This sample shows a join with a composite key.")]
    public void DLinqJoin9()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        from t in Session.Query.All<Track>()
        join il in Session.Query.All<InvoiceLine>()
          on new {InvoiceId = i.InvoiceId, TrackId = t.TrackId} equals new {InvoiceId = il.Invoice.InvoiceId, TrackId = il.Track.TrackId}
          into details
        from d in details
        select new {InvoiceId = i.InvoiceId, ProductId = t.TrackId, d.UnitPrice};

      QueryDumper.Dump(q);
    }

    [Category("JOIN")]
    [Test(Description = "GroupJoin - Nullable\\Nonnullable Key Relationship")]
    [Description("This sample shows how to construct a join where one side is nullable and the other isn't.")]
    public void DLinqJoin10()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        join e in Session.Query.All<Employee>()
          on i.DesignatedEmployee.EmployeeId equals (int?) e.EmployeeId into emps
        from e in emps
        select new {i.InvoiceId, e.FirstName};

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
      "billed to London by Commission.")]
    public void DLinq37()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        where i.BillingAddress.City=="London"
        orderby i.Commission
        select i;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "OrderByDescending")]
    [Description("This sample uses orderby to sort Tracks " +
      "by unit price from highest to lowest.")]
    public void DLinq38()
    {
      var q =
        from t in Session.Query.All<Track>()
        orderby t.UnitPrice descending
        select t;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "ThenBy")]
    [Description("This sample uses a compound orderby to sort Customers " +
      "by city and then last name.")]
    public void DLinq39()
    {
      var q =
        from c in Session.Query.All<Customer>()
        orderby c.Address.City , c.LastName
        select c;

      QueryDumper.Dump(q);
    }

    [Category("ORDER BY")]
    [Test(Description = "ThenByDescending")]
    [Description("This sample uses orderby to sort Invoices from Id 1 " +
      "by billing country, and then by Commission from highest to lowest.")]
    public void DLinq40()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        where i.InvoiceId==1
        orderby i.BillingAddress.Country , i.Commission descending
        select i;

      QueryDumper.Dump(q);
    }


    [Category("ORDER BY")]
    [Test(Description = "OrderBy - Group By")]
    [Description("This sample uses Orderby, Max and Group By to find the Tracks that have " +
      "the highest unit price in each category, and sorts the group by category id.")]
    public void DLinq41()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var categories =
        Session.Query.All<Track>()
          .GroupBy(t => t.TrackId)
          .OrderBy(g => g.Key)
          .Select(g => new {
            g.Key,
            MostExpensiveTrack =
              g.Where(t2 => t2.UnitPrice==g.Max(t3 => t3.UnitPrice))
          });

      QueryDumper.Dump(categories);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Simple")]
    [Description("This sample uses group by to partition Tracks by " +
      "Id.")]
    public void DLinq42()
    {
      var q =
        from t in Session.Query.All<Track>()
        group t by t.TrackId
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
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            MaxPrice = g.Max(t => t.UnitPrice)
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
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            MinPrice = g.Min(t => t.UnitPrice)
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
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            AveragePrice = g.Average(t => t.UnitPrice)
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
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            TotalPrice = g.Sum(t => t.UnitPrice)
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Count")]
    [Description("This sample uses group by and Count " +
      "to find the number of Tracks in each Id.")]
    public void DLinq47()
    {
      var q =
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            NumTracks = g.Count()
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Count - Conditional")]
    [Description("This sample uses group by and Count " +
      "to find the number of Track in each Id " +
        "that are VideoTracks.")]
    public void DLinq48()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var q =
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          select new {
            g.Key,
            NumTracks = g.Count(t => (t is VideoTrack))
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - followed by Where")]
    [Description("This sample uses a where clause after a group by clause " +
      "to find all categories that have at least 10 track.")]
    public void DLinq49()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var q =
        from t in Session.Query.All<Track>()
        group t by t.TrackId
        into g
          where g.Count() >= 10
          select new {
            g.Key,
            TrackCount = g.Count()
          };

      QueryDumper.Dump(q);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Multiple Columns")]
    [Description("This sample uses Group By to group tracks by TrackId and MediaTypeId.")]
    public void DLinq50()
    {
      var categories =
        from t in Session.Query.All<Track>()
        group t by new {t.TrackId, SupplierId = t.MediaType.MediaTypeId}
        into g
          select new {g.Key, g};

      QueryDumper.Dump(categories);
    }

    [Category("GROUP BY/HAVING")]
    [Test(Description = "GroupBy - Expression")]
    [Description("This sample uses Group By to return two sequences of tracks. " +
      "The first sequence contains tracks with unit price " +
        "greater than 10. The second sequence contains tracks " +
          "with unit price less than or equal to 10.")]
    public void DLinq51()
    {
      var categories =
        from t in Session.Query.All<Track>()
        group t by new {Criterion = t.UnitPrice > 10}
        into g
          select g;

      QueryDumper.Dump(categories);
    }

    [Category("EXISTS/IN/ANY/ALL")]
    [Test(Description = "Any - Simple")]
    [Description("This sample uses Any to return only Customers that have no Invoices.")]
    public void DLinq52()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where !c.Invoices.Any()
        select c;

      QueryDumper.Dump(q);
    }

    [Category("EXISTS/IN/ANY/ALL")]
    [Test(Description = "Any - Conditional")]
    [Description("This sample uses Any to return only Playlists that have " +
      "at least one VideoTrack.")]
    public void DLinq53()
    {
      var q =
        from p in Session.Query.All<Playlist>()
        where p.Tracks.Any(t => (t is VideoTrack))
        select p;

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
        where c.Invoices.All(i => i.BillingAddress.City==c.Address.City)
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
        select e.Phone
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
        select new {Name = e.FirstName + " " + e.LastName, Phone = e.Phone}
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
    [Description("This sample uses Skip to select all but the 10 most expensive Tracks.")]
    public void DLinq61()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var q = (
        from t in Session.Query.All<Track>()
        orderby t.UnitPrice descending
        select t)
        .Skip(10);

      QueryDumper.Dump(q);
    }

    [Category("Paging")]
    [Test(Description = "Paging - Index")]
    [Description("This sample uses the Skip and Take operators to do paging by " +
      "skipping the first 50 records and then returning the next 10, thereby " +
        "providing the data for page 6 of the Customers table.")]
    public void DLinq62()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var q = (
        from c in Session.Query.All<Customer>()
        orderby c.LastName
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
          "thereby providing the data for page 6 of the Tracks table.  " +
            "Note that this method only works when ordering by a unique key.")]
    public void DLinq63()
    {
      var q = (
        from t in Session.Query.All<Track>()
        where t.TrackId > 50
        orderby t.TrackId
        select t)
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
        where e.ReportsToManager==null
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
        select new {c.CustomerId, Location = c.Address.City + ", " + c.Address.Country};

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Length")]
    [Description("This sample uses the Length property to find all Tracks whose " +
      "name is shorter than 10 characters.")]
    public void DLinq79()
    {
      var q =
        from t in Session.Query.All<Track>()
        where t.Name.Length < 10
        select t;

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
        where c.LastName.Contains("Anders")
        select c;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.IndexOf(substring)")]
    [Description("This sample uses the IndexOf method to find the first instance of " +
      " '@' in each Customer's e-mail.")]
    public void DLinq81()
    {
      var q =
        from c in Session.Query.All<Customer>()
        select new {c.Email, AtPos = c.Email.IndexOf("@")};

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.StartsWith(prefix)")]
    [Description("This sample uses the StartsWith method to find Customers whose " +
      "first name starts with 'Maria'.")]
    public void DLinq82()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.FirstName.StartsWith("Maria")
        select c;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.EndsWith(suffix)")]
    [Description("This sample uses the StartsWith method to find Customers whose " +
      "last name ends with 'Anders'.")]
    public void DLinq83()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.LastName.EndsWith("Anders")
        select c;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "String.Substring(start)")]
    [Description("This sample uses the Substring method to return track names starting " +
      "from the third letter.")]
    public void DLinq84()
    {
      var q =
        from t in Session.Query.All<Track>()
        select t.Name.Substring(2);

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
        where e.Phone.Substring(6, 3)=="555"
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
    [Description("This sample uses the ToLower method to return MediaType names " +
      "that have been converted to lowercase.")]
    public void DLinq87()
    {
      var q =
        from c in Session.Query.All<MediaType>()
        select c.Name.ToLower();

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
        select e.Phone.Substring(0, 5).Trim();

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
        where e.Phone.Substring(4, 1)==")"
        select e.Phone.Insert(5, ":");

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
        where e.Phone.Substring(4, 1)==")"
        select e.Phone.Remove(9);

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
        where e.Phone.Substring(4, 1)==")"
        select e.Phone.Remove(0, 6);

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
        from s in Session.Query.All<Customer>()
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
      "find Orders placed in 2005.")]
    public void DLinq93()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        where i.InvoiceDate.Year==2005
        select i;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "DateTime.Month")]
    [Description("This sample uses the DateTime's Month property to " +
      "find Invoices placed in December.")]
    public void DLinq94()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        where i.InvoiceDate.Month==12
        select i;

      QueryDumper.Dump(q);
    }

    [Category("String/Date Functions")]
    [Test(Description = "DateTime.Day")]
    [Description("This sample uses the DateTime's Day property to " +
      "find Invoices placed on the 31st day of the month.")]
    public void DLinq95()
    {
      var q =
        from i in Session.Query.All<Invoice>()
        where i.InvoiceDate.Day==31
        select i;

      QueryDumper.Dump(q);
    }

    [Category("Object Identity")]
    [Test(Description = "Object Caching - 1")]
    [Description("This sample demonstrates how, upon executing the same query twice, " +
      "you will receive a reference to the same object in memory each time.")]
    public void DLinq96()
    {
      Customer cust1 = Session.Query.All<Customer>().First(c => c.CustomerId==4200);
      Customer cust2 = Session.Query.All<Customer>().First(c => c.CustomerId==4200);

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
      Customer cust1 = Session.Query.All<Customer>().First(c => c.CustomerId==4200);
      Customer cust2 = (
        from i in Session.Query.All<Invoice>()
        where i.Customer.CustomerId==4200
        select i)
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
        foreach (var inv in cust.Invoices) {
          Console.WriteLine("Id {0} has an InvoiceID {1}.", cust.CustomerId, inv.InvoiceId);
        }
      }
    }

    private bool isValidTrack(Track t)
    {
      return t.Name.LastIndexOf('C')==0;
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
        foreach (var inv in emp.Invoices) {
          Console.WriteLine("Employee {0} is responsible for invoice #{1}.", emp.FirstName, inv.InvoiceId);
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
        Console.WriteLine("{0}", emp.LastName);
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
        from t in Session.Query.All<Track>().ToList()
        where isValidTrack(t)
        select t;

      QueryDumper.Dump(q);
    }

    [Category("Conversion Operators")]
    [Test(Description = "ToArray")]
    [Description("This sample uses ToArray to immediately evaluate a query into an array " +
      "and get the 2nd element.")]
    public void DLinq106()
    {
      var q =
        from c in Session.Query.All<Customer>()
        where c.Address.City=="London"
        select c;

      Customer[] qArray = q.ToArray();
      QueryDumper.Dump(qArray[1]);
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
        from p in Session.Query.All<Track>()
        where p.UnitPrice <= 10 && !(p is VideoTrack)
        select p;

      Dictionary<int, Track> qDictionary = q.ToDictionary(t => t.TrackId);

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
      "video tracks of the top 10 most expensive tracks.")]
    public void DLinq131()
    {
      var tracks = from t in Session.Query.All<Track>().OrderByDescending(t => t.UnitPrice).Take(10)
      where t is VideoTrack
      select t;

      QueryDumper.Dump(tracks);
    }

    [Category("Inheritance")]
    [Test(Description = "Simple")]
    [Description("This sample returns all contacts where the city is London.")]
    public void DLinq135()
    {
      var cons = from c in Session.Query.All<BusinessContact>()
      select c;
      Assert.Throws<QueryTranslationException>(() => {
        foreach (var con in cons) {
          Console.WriteLine("Company name: {0}", con.CompanyName);
          Console.WriteLine("Phone: {0}", con.Phone);
          Console.WriteLine("This is a {0}", con.GetType());
          Console.WriteLine();
        }
      });
    }

    [Category("Inheritance")]
    [Test(Description = "OfType")]
    [Description("This sample uses OfType to return all customer contacts.")]
    public void DLinq136()
    {
      var cons = from c in Session.Query.All<Person>().OfType<Customer>()
      select c;

     Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(cons)) ;
    }

    [Category("Inheritance")]
    [Test(Description = "IS")]
    [Description("This sample uses IS to return all Customers.")]
    public void DLinq137()
    {
      var cons = from c in Session.Query.All<Person>()
      where c is Customer
      select c;

     Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(cons));
    }

    [Category("Inheritance")]
    [Test(Description = "AS")]
    [Description("This sample uses AS to return FullContact or null.")]
    public void DLinq138()
    {
      var cons = from c in Session.Query.All<Person>()
      select c as BusinessContact;

      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(cons));
    }

    [Category("Inheritance")]
    [Test(Description = "Cast")]
    [Description("This sample uses a cast to retrieve customer contacts who work in 'Around the Horn'.")]
    public void DLinq139()
    {
      var cons = from c in Session.Query.All<Person>()
      where c is Customer && ((Customer) c).CompanyName=="Around the Horn"
      select c;

      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(cons));
    }
  }
}