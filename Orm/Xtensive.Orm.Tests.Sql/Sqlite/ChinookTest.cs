// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.06.10

using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  public class ChinookTest : Chinook
  {
    private DbCommand dbCommand;
    private DbCommand sqlCommand;

    private Schema schema = null;

    [OneTimeSetUp]
    public override void SetUp()
    {
      base.SetUp();
      dbCommand = sqlConnection.CreateCommand();
      sqlCommand = sqlConnection.CreateCommand();

      schema = Catalog.DefaultSchema;
    }

    #region Internals

    private bool CompareExecuteDataReader(string commandText, ISqlCompileUnit statement)
    {
      sqlCommand.CommandText = sqlDriver.Compile(statement).GetCommandText();
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);

      Console.WriteLine(commandText);
      dbCommand.CommandText = commandText;

      DbCommandExecutionResult r1, r2;
      r1 = GetExecuteDataReaderResult(dbCommand);
      r2 = GetExecuteDataReaderResult(sqlCommand);

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine(r1);
      Console.WriteLine(r2);

      if (r1.RowCount!=r2.RowCount)
        return false;
      if (r1.FieldCount!=r2.FieldCount)
        return false;
      for (int i = 0; i < r1.FieldCount; i++) {
        if (r1.FieldNames[i]!=r2.FieldNames[i])
          return false;
      }
      return true;
    }

    private bool CompareExecuteNonQuery(string commandText, ISqlCompileUnit statement)
    {
      sqlCommand.CommandText = sqlDriver.Compile(statement).GetCommandText();
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);

      Console.WriteLine(commandText);
      dbCommand.CommandText = commandText;

      DbCommandExecutionResult r1, r2;
      r1 = GetExecuteNonQueryResult(dbCommand);
      r2 = GetExecuteNonQueryResult(sqlCommand);

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine(r1);
      Console.WriteLine(r2);

      if (r1.RowCount!=r2.RowCount)
        return false;
      return true;
    }

    #endregion

    //SET CHARACTER SET utf8

    [Test]
    public void TestExtractCatalog()
    {
      Assert.GreaterOrEqual(Catalog.Schemas.Count, 1);
    }

    [Test]
    public void Test000()
    {
      string nativeSql = @"SELECT 
                          employee.EmployeeID,
                          employee.FirstName,
                          employee.LastName,
                          employee.BirthDate
                        FROM
                          employee
                        WHERE
                          employee.FirstName = 'Robert'
                        ORDER BY
                          employee.LastName";

      var p = sqlCommand.CreateParameter();
      p.ParameterName = "p1";
      p.DbType = DbType.String;
      p.Value = "Robert";
      sqlCommand.Parameters.Add(p);

      SqlTableRef employees = SqlDml.TableRef(schema.Tables["employee"]);
      SqlSelect select = SqlDml.Select(employees);
      select.Columns.AddRange(employees["EmployeeId"], employees["FirstName"], employees["LastName"], employees["BirthDate"]);
      select.Where = employees["FirstName"]==SqlDml.ParameterRef(p.ParameterName);
      select.OrderBy.Add(employees["LastName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test001()
    {
      string nativeSql = @"SELECT DISTINCT
                      employee.FirstName
                    FROM
                      employee
                    WHERE
                      employee.Title = 'IT Staff'";

      var p = sqlCommand.CreateParameter();
      p.ParameterName = "p10";
      p.DbType = DbType.String;
      p.Value = "IT Staff";
      sqlCommand.Parameters.Add(p);

      SqlTableRef employees = SqlDml.TableRef(schema.Tables["employee"]);
      SqlSelect select = SqlDml.Select(employees);
      select.Distinct = true;
      select.Columns.AddRange(employees["FirstName"]);
      select.Where = employees["Title"]==SqlDml.ParameterRef(p.ParameterName);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test002()
    {
      string nativeSql = "SELECT * FROM [genre] a";

      SqlTableRef region = SqlDml.TableRef(Catalog.Schemas["main"].Tables["genre"]);
      SqlSelect select = SqlDml.Select(region);
      select.Columns.Add(SqlDml.Asterisk);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test003()
    {
      string nativeSql = "SELECT [a].[EmployeeId], [a].[FirstName], [a].[LastName] FROM [Employee] [a] WHERE ([a].[EmployeeId] < 2) ORDER BY [a].[FirstName] ASC, 3 DESC";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["main"].Tables["Employee"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.AddRange(customer["EmployeeId"], customer["FirstName"], customer["LastName"]);
      select.Where = customer["EmployeeId"] < 2;
      select.OrderBy.Add(customer["FirstName"]);
      select.OrderBy.Add(3, false);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test006()
    {
      SqlTableRef orders = SqlDml.TableRef(schema.Tables["invoice"]);
      SqlSelect select = SqlDml.Select(orders);
      select.Columns.Add(SqlDml.Asterisk);
      select.Where = orders["PaymentDate"] > new DateTime(2013, 8, 1);
      select.OrderBy.Add(orders["PaymentDate"], false);

      sqlCommand.CommandText = Compile(select).GetCommandText();
      sqlCommand.Prepare();

      DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
      Assert.AreEqual(14, r.RowCount);
    }

    [Test]
    public void Test007()
    {
      SqlTableRef orders = SqlDml.TableRef(schema.Tables["invoice"]);
      SqlSelect select = SqlDml.Select(orders);
      select.Limit = 10;
      select.Columns.Add(SqlDml.Asterisk);

      sqlCommand.CommandText = Compile(select).GetCommandText();
      sqlCommand.Prepare();

      DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
      Assert.AreEqual(10, r.RowCount);
    }

    [Test]
    public void Test008()
    {
      string nativeSql = @"SELECT 
                                  t.TrackId,
                                  t.[Name],
                                  a.[Title]
                                FROM
                                  Track t
                                  INNER JOIN Album a ON (t.AlbumId = a.AlbumId)
                                ORDER BY
                                  t.TrackId";

      SqlTableRef track = SqlDml.TableRef(schema.Tables["Track"], "t");
      SqlTableRef album = SqlDml.TableRef(schema.Tables["Album"], "a");

      SqlSelect select = SqlDml.Select(track.InnerJoin(album, track["AlbumId"]==album["AlbumId"]));

      select.Columns.Add(track["TrackId"]);
      select.Columns.Add(track["Name"]);
      select.Columns.Add(album["Title"]);

      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test009()
    {
      string nativeSql = @"SELECT 
                                  c.CustomerId,
                                  c.CompanyName,
                                  c.FirstName,
                                  c.LastName,
                                  i.InvoiceDate,
                                  il.TrackId,
                                  t.Name,
                                  il.UnitPrice,
                                  il.Quantity
                                FROM
                                  Customer c
                                  INNER JOIN Invoice i ON (c.CustomerId = i.CustomerId)
                                  INNER JOIN [InvoiceLine] il ON (i.InvoiceId = il.InvoiceId)
                                  INNER JOIN [Track] t ON (t.TrackId = il.TrackId)";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");
      SqlTableRef invoiceLine = SqlDml.TableRef(schema.Tables["invoiceline"], "il");
      SqlTableRef track = SqlDml.TableRef(schema.Tables["track"], "t");

      SqlSelect select = SqlDml.Select(customer
        .InnerJoin(invoice, customer["CustomerId"]==invoice["CustomerId"])
          .InnerJoin(invoiceLine, invoiceLine["InvoiceId"]==invoice["InvoiceId"])
            .InnerJoin(track, track["TrackId"]==invoiceLine["TrackId"]));
      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(customer["CompanyName"]);
      select.Columns.Add(customer["FirstName"]);
      select.Columns.Add(customer["LastName"]);
      select.Columns.Add(invoice["InvoiceDate"]);
      select.Columns.Add(invoiceLine["TrackId"]);
      select.Columns.Add(track["Name"]);
      select.Columns.Add(invoiceLine["UnitPrice"]);
      select.Columns.Add(invoiceLine["Quantity"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test010()
    {
      string nativeSql = @"SELECT 
                                 i.invoiceid,
                                 round(i.Commission * 12, 1) Rounded
                           FROM invoice i
                           WHERE i.invoiceid = 412";

      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");

      SqlSelect select = SqlDml.Select(invoice);
      select.Columns.Add(invoice["InvoiceId"]);
      select.Columns.Add(SqlDml.Round(invoice["Commission"] * 12, 1), "Rounded");
      select.Where = invoice["invoiceId"]==412;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test011()
    {
      string nativeSql = @"SELECT 
                                  c.CustomerId,
                                  c.CompanyName,
                                  c.LastName,
                                  SUM(i.Commission) AS Total
                           FROM
                                  Customer c
                                  INNER JOIN invoice i ON (c.CustomerID = i.CustomerID)
                                GROUP BY
                                  c.CustomerID,
                                  c.CompanyName,
                                  c.LastName
                                ORDER BY c.CustomerID";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");

      SqlSelect select = SqlDml.Select(customer.InnerJoin(invoice, customer["CustomerId"]==invoice["CustomerId"]));

      select.Columns.AddRange(customer["CustomerId"], customer["CompanyName"], customer["LastName"]);
      select.Columns.Add(SqlDml.Sum(invoice["Commission"]), "Total");
      select.GroupBy.AddRange(customer["CustomerId"], customer["CompanyName"], customer["LastName"]);

      select.OrderBy.Add(customer["CustomerId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test012()
    {
      string nativeSql = @"SELECT 
                                  CASE il.TrackId
                                  WHEN 1 THEN 'STAFF_1'
                                  WHEN 2 THEN 'STAFF_2'
                                  ELSE 'STAFF_OTHER'
                                  END AS shippers,
                                  SUM(il.UnitPrice) AS TotalUnits
                           FROM [invoiceline] il
                           GROUP BY il.TrackId";

      SqlTableRef invoiceLine = SqlDml.TableRef(schema.Tables["invoiceline"], "il");

      SqlSelect select = SqlDml.Select(invoiceLine);
      SqlCase totalPayment = SqlDml.Case(invoiceLine["TrackId"]);
      totalPayment[1] = SqlDml.Literal("STAFF_1");
      totalPayment[2] = SqlDml.Literal("STAFF_2");
      totalPayment.Else = SqlDml.Literal("STAFF_OTHER");
      select.Columns.Add(totalPayment, "shippers");

      select.Columns.Add(SqlDml.Sum(invoiceLine["UnitPrice"]), "TotalUnits");
      select.GroupBy.AddRange(invoiceLine["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test013()
    {
      string nativeSql = @"SELECT 
                                  r.InvoiceId,
                                  r.DesignatedEmployeeId,
                                  r.Status
                           FROM invoice r
                           WHERE r.[BillingState] IS NOT NULL
                           ORDER BY r.InvoiceId";

      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");
      SqlSelect select = SqlDml.Select(invoice);
      select.Columns.AddRange(invoice["InvoiceId"], invoice["DesignatedEmployeeId"], invoice["Status"]);

      select.Where = SqlDml.IsNotNull(invoice["BillingState"]);
      select.OrderBy.Add(invoice["InvoiceId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_1()
    {
      string nativeSql = @"select date('NOW', '+1 MONTHS') AS Days ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddMonths("NOW", 1), "Days");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_2()
    {
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");
      SqlSelect select = SqlDml.Select(invoice);
      select.Columns.Add(SqlDml.FunctionCall("DATE", invoice["PaymentDate"], SqlDml.Native(string.Format("'{0} MONTHS'", 1 + 1))), "TimeToToday");
      select.Where = SqlDml.IsNotNull(invoice["PaymentDate"]);

      Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
    }

    [Test]
    public void Test014_3()
    {
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");
      SqlSelect select = SqlDml.Select(invoice);
      select.Columns.Add(SqlDml.DateTimeAddMonths(invoice["PaymentDate"], 1 + 1), "TimeToToday");
      select.Where = SqlDml.IsNotNull(invoice["PaymentDate"]);

      Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
    }

    [Test]
    public void Test014_4()
    {
      string nativeSql = @"select date('NOW', '+1 MONTHS') AS Days ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddMonths("NOW", -15), "Days");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_5()
    {
      string nativeSql = @"select date('NOW', '+1 MONTHS') AS Days ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddMonths("NOW", 1 + 1), "Days");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_6()
    {
      string nativeSql = @"select strftime('%d', 'now') AS DayOfMonth ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Day, "NOW"), "DayOfMonth");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_7()
    {
      string nativeSql = @"select strftime('%m', 'now') AS Month ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Month, "NOW"), "Month");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_8()
    {
      string nativeSql = @"select strftime('%Y', 'now') AS Year ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, "NOW"), "Year");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_9()
    {
      string nativeSql = @"select strftime('%H', 'now') AS Hour ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Hour, "NOW"), "Hour");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_10()
    {
      string nativeSql = @"select strftime('%M', 'now') AS Minutes ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Minute, "NOW"), "Minutes");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_11()
    {
      string nativeSql = @"select strftime('%S', 'now') AS Seconds ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Second, "NOW"), "Seconds");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_12()
    {
      string nativeSql = @"select strftime('%f', 'now') AS Milliseconds ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Millisecond, "NOW"), "Milliseconds");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014_13()
    {
      string nativeSql = @"select datetime('2011-11-16') AS BirthDay ";

      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeConstruct(2011, 11, 16), "BirthDay");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("A close inspection needed")]
    public void Test015()
    {
      SqlSelect select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeMinusDateTime(DateTime.Now, DateTime.Now.AddDays(-4)), "FewDaysAgo");

      Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
    }

    [Test]
    public void Test016()
    {
      string nativeSql = "SELECT SUM(p.commission) AS sum FROM invoice p";

      SqlTableRef orders = SqlDml.TableRef(schema.Tables["invoice"], "p");

      SqlSelect select = SqlDml.Select(orders);
      select.Columns.Add(SqlDml.Sum(orders["Commission"]), "sum");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test017_1()
    {
      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");

      SqlSelect select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(SqlDml.RawConcat("Mr. ", customer["LastName"]), "FullName");
      Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
    }

    [Test]
    public void Test017()
    {
      string nativeSql = "SELECT c.CustomerId, c.FirstName|| ', ' || c.LastName as FullName FROM customer c";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");

      SqlSelect select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(SqlDml.Concat(SqlDml.Concat(customer["FirstName"], SqlDml.Literal(", ")), customer["LastName"]), "FullName");
      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test018()
    {
      string nativeSql = @"SELECT 
                                  c.CustomerId,
                                  c.CompanyName,
                                  c.LastName,
                                  SUM(i.commission) AS Total
                           FROM
                                customer c
                           INNER JOIN invoice i ON (c.CustomerID = i.CustomerID)
                           GROUP BY
                                  c.CustomerId,
                                  c.CompanyName,
                                  c.LastName
                           HAVING SUM(i.commission) > 140";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");
      SqlSelect select = SqlDml.Select(customer.InnerJoin(invoice, customer["CustomerId"]==invoice["CustomerId"]));

      select.Columns.AddRange(customer["CustomerID"], customer["CompanyName"], customer["LastName"]);
      select.Columns.Add(SqlDml.Sum(invoice["Commission"]), "Total");

      select.GroupBy.AddRange(customer["CustomerID"], customer["CompanyName"], customer["LastName"]);

      select.Having = SqlDml.Sum(invoice["Commission"]) > 140;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test019()
    {
      string nativeSql = @"SELECT 
                                  c.CustomerId,
                                  c.CompanyName,
                                  c.LastName
                           FROM
                                customer c
                           WHERE c.CustomerId IN (SELECT r.CustomerId FROM invoice r WHERE r.DesignatedEmployeeId = 8)
                           GROUP BY c.CustomerID,
                                    c.CompanyName,
                                    c.LastName";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");

      SqlSelect innerSelect = SqlDml.Select(invoice);
      innerSelect.Columns.Add(invoice["CustomerId"]);
      innerSelect.Where = invoice["DesignatedEmployeeId"] ==8;

      SqlSelect select = SqlDml.Select(customer);

      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(customer["CompanyName"]);
      select.Columns.Add(customer["LastName"]);

      select.Where = SqlDml.In(customer["CustomerId"], innerSelect);

      select.GroupBy.Add(customer["CustomerID"]);
      select.GroupBy.Add(customer["CompanyName"]);
      select.GroupBy.Add(customer["LastName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test020()
    {
      string nativeSql = @"SELECT 
                                  f.TrackId,
                                  f.Name,
                                  f.UnitPrice
                                FROM
                                  track f
                                WHERE
                                  f.Milliseconds BETWEEN 50 AND 40000
                                ORDER BY f.TrackId";

      SqlTableRef track = SqlDml.TableRef(schema.Tables["track"], "f");
      SqlSelect select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"]);
      select.Where = SqlDml.Between(track["Milliseconds"], 50, 40000);
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test021()
    {
      string nativeSql = @"SELECT 
                                  f.TrackId,
                                  f.Name,
                                  f.UnitPrice,
                                  f.AlbumId
                           FROM track f
                           WHERE f.AlbumId in (2, 8)
                           ORDER BY f.TrackId";

      SqlTableRef track = SqlDml.TableRef(schema.Tables["track"], "f");
      SqlSelect select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"], track["AlbumId"]);
      select.Where = SqlDml.In(track["AlbumId"], SqlDml.Row(2, 8));
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test022()
    {
      string nativeSql = @"SELECT 
                                  f.TrackId,
                                  f.Name,
                                  f.UnitPrice,
                                  f.AlbumId
                                FROM
                                  track f
                                WHERE
                                  f.Name LIKE 'R%'
                                ORDER BY f.TrackId";

      SqlTableRef track = SqlDml.TableRef(schema.Tables["track"], "f");
      SqlSelect select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"], track["AlbumId"]);
      select.Where = SqlDml.Like(track["Name"], "R%");
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test023()
    {
      string nativeSql = @"SELECT 
                                  f.TrackId,
                                  f.Name,
                                  f.UnitPrice
                           FROM
                                  track f
                           WHERE
                                  (f.AlbumId = 3 OR 
                                  f.AlbumId = 8) AND 
                                  f.UnitPrice < 1
                           ORDER BY
                                  f.TrackId";

      SqlTableRef track = SqlDml.TableRef(schema.Tables["track"], "f");
      SqlSelect select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"]);
      select.Where = (track["AlbumId"] ==3 || track["AlbumId"] ==8) && track["UnitPrice"] < 1;
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test024()
    {
      string nativeSql = @"SELECT 
                                  strftime('%Y', PaymentDate) as Year,
                                  COUNT(*) Required
                           FROM invoice r
                           GROUP BY strftime('%Y', PaymentDate)";

      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");

      SqlSelect select = SqlDml.Select(invoice);
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, invoice["PaymentDate"]), "Year");
      select.Columns.Add(SqlDml.Count(), "Required");

      select.GroupBy.Add(SqlDml.Extract(SqlDateTimePart.Year, invoice["PaymentDate"]));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test026()
    {
      string nativeSql = @"SELECT 
                                  p.CustomerId,
                                  p.Commission
                           FROM
                                  invoice p
                           WHERE
                                  p.Commission = (SELECT MIN(Commission) AS LowestCommission FROM invoice)";

      SqlTableRef invoice1 = SqlDml.TableRef(schema.Tables["invoice"], "p1");
      SqlTableRef invoice2 = SqlDml.TableRef(schema.Tables["invoice"], "p2");

      SqlSelect innerSelect = SqlDml.Select(invoice2);
      innerSelect.Columns.Add(SqlDml.Min(invoice2["Commission"]));

      SqlSelect select = SqlDml.Select(invoice1);
      select.Columns.Add(invoice1["CustomerId"]);
      select.Columns.Add(invoice1["Commission"]);

      select.Where = SqlDml.Equals(invoice1["Commission"], innerSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test027()
    {
      string nativeSql = @"SELECT 
                                  c.CustomerId,
                                  c.CompanyName
                          FROM
                                  customer c
                          WHERE EXISTS
                                    (SELECT * FROM invoice i WHERE i.Commission < 1.00 AND i.CustomerId = c.CustomerId )";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");

      SqlSelect innerSelect = SqlDml.Select(invoice);
      SqlSelect select = SqlDml.Select(customer);

      innerSelect.Columns.Add(SqlDml.Asterisk);
      innerSelect.Where = invoice["Commission"] < 11.00 && invoice["CustomerId"]==customer["CustomerId"];

      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(customer["CompanyName"]);
      select.Where = SqlDml.Exists(innerSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test028()
    {
      string nativeSql = @"select * FROM customer c limit 0, 10";

      SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Limit = 10;
      select.Offset = 0;
      select.Columns.Add(SqlDml.Asterisk);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test029()
    {
      string nativeSql = "UPDATE invoice " + "SET Total = Total * 1 " + "WHERE InvoiceId = 10;";

      SqlTableRef invoice = SqlDml.TableRef(schema.Tables["invoice"]);
      SqlUpdate update = SqlDml.Update(invoice);
      update.Values[invoice["Total"]] = invoice["Total"] * 1;
      update.Where = invoice["InvoiceId"] ==10;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test150()
    {
      SqlCreateTable create = SqlDdl.Create(Catalog.Schemas["main"].Tables["customer"]);
      Console.Write(Compile(create));
    }

    [Test]
    public void Test151()
    {
      SqlDropTable drop = SqlDdl.Drop(Catalog.Schemas["main"].Tables["customer"]);
      Console.Write(Compile(drop));
    }

    [Test]
    public void Test152()
    {
      SqlDropSchema drop = SqlDdl.Drop(Catalog.Schemas["main"]);
      Assert.Throws<NotSupportedException>(() => Console.Write(Compile(drop)));
    }

    [Test]
    public void Test153()
    {
      SqlCreateView create = SqlDdl.Create(Catalog.Schemas["main"].Views["Invoice Subtotals"]);
      Console.Write(Compile(create));
    }

    [Test]
    public void Test154()
    {
      SqlCreateSchema create = SqlDdl.Create(Catalog.Schemas["main"]);
      Console.Write(Compile(create));
    }

    [Test]
    public void Test155()
    {
      SqlAlterTable alter = SqlDdl.Alter(Catalog.Schemas["main"].Tables["customer"], SqlDdl.AddColumn(Catalog.Schemas["main"].Tables["customer"].TableColumns["CompanyName"]));

      Console.Write(Compile(alter));
    }

    [Test]
    public void Test156()
    {
      SqlAlterTable alter = SqlDdl.Alter(Catalog.Schemas["main"].Tables["customer"], SqlDdl.DropColumn(Catalog.Schemas["main"].Tables["customer"].TableColumns["CompanyName"]));

      Assert.Throws<NotSupportedException>(() => Console.Write(Compile(alter)));
    }

    [Test]
    public void Test157()
    {
      var renameColumn = SqlDdl.Rename(Catalog.Schemas["main"].Tables["customer"].TableColumns["LastName"], "LastName1");

      Assert.Throws<NotSupportedException>(() => Console.Write(Compile(renameColumn)));
    }

    [Test]
    public void Test158()
    {
      var t = Catalog.Schemas["main"].Tables["customer"];
      Xtensive.Sql.Model.UniqueConstraint uc = t.CreateUniqueConstraint("newUniqueConstraint", t.TableColumns["Phone"]);
      SqlAlterTable stmt = SqlDdl.Alter(t, SqlDdl.AddConstraint(uc));

      Assert.Throws<NotSupportedException>(() => Console.Write(Compile(stmt)));
    }

    [Test]
    public void Test160()
    {
      var t = Catalog.Schemas["main"].Tables["customer"];
      Index index = t.CreateIndex("MegaIndex195");
      index.CreateIndexColumn(t.TableColumns[0]);
      SqlCreateIndex create = SqlDdl.Create(index);

      Console.Write(Compile(create));
    }

    [Test]
    public void Test161()
    {
      var t = Catalog.Schemas["main"].Tables["customer"];
      Index index = t.CreateIndex("MegaIndex196");
      index.CreateIndexColumn(t.TableColumns[0]);
      SqlDropIndex drop = SqlDdl.Drop(index);

      Console.Write(Compile(drop));
    }

    [Test]
    public void Test162()
    {
      var alter = SqlDdl.Rename(Catalog.Schemas["main"].Tables["customer"], "SomeWierdTableName");
      Console.Write(Compile(alter));
    }

    [Test]
    [Ignore("Not yet prepared for tests")]
    public void Test201()
    {
      string nativeSql = "SELECT a.f FROM ((SELECT 1 as f UNION SELECT 2) EXCEPT (SELECT 3 UNION SELECT 4)) a";

      SqlSelect s1 = SqlDml.Select();
      SqlSelect s2 = SqlDml.Select();
      SqlSelect s3 = SqlDml.Select();
      SqlSelect s4 = SqlDml.Select();
      SqlSelect select;
      s1.Columns.Add(1, "f");
      s2.Columns.Add(2);
      s3.Columns.Add(3);
      s4.Columns.Add(4);
      SqlQueryRef qr = SqlDml.QueryRef(s1.Union(s2).Except(s3.Union(s4)), "a");
      select = SqlDml.Select(qr);
      select.Columns.Add(qr["f"]);

     Assert.Throws<NotSupportedException>(() => Assert.IsTrue(CompareExecuteNonQuery(nativeSql, select)));
    }

    [Test]
    public void Test165()
    {
      var t = Catalog.Schemas["main"].CreateTable("SomeWierdTableName");
      t.CreateColumn("Field01", new SqlValueType(SqlType.Int32));
      t.CreateColumn("Field02", new SqlValueType(SqlType.Int32));

      var uc = t.CreatePrimaryKey(string.Empty, t.TableColumns["Field02"]);
      SqlAlterTable stmt = SqlDdl.Alter(t, SqlDdl.AddConstraint(uc));

      Assert.Throws<NotSupportedException>(() => Console.Write(Compile(stmt)));
    }
  }
}