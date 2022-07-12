// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.PostgreSql
{
  [Explicit]
  public sealed class ChinookInMemoryTest : ChinookTest
  {
    protected override bool InMemory => true;

#if DEBUG
    protected override bool PerformanceCheck => true;
#else
    protected override bool PerformanceCheck => true;
#endif
  }

  [TestFixture]
  public class ChinookTest : ChinookTestBase
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

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.PostgreSql);

    #region Internals

    protected virtual bool CompareExecuteDataReader(string commandText, ISqlCompileUnit statement)
    {
      var compiledCommandText = PerformanceCheck
        ? CompileWithPerformanceCheck(statement)
        : CompileRegular(statement);

      Console.WriteLine(compiledCommandText);
      if (InMemory) {
        return true;
      }

      sqlCommand.CommandText = compiledCommandText;
      sqlCommand.Prepare();

      Console.WriteLine(commandText);
      dbCommand.CommandText = commandText;

      DbCommandExecutionResult r1, r2;
      r1 = GetExecuteDataReaderResult(dbCommand);
      r2 = GetExecuteDataReaderResult(sqlCommand);

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine(r1);
      Console.WriteLine(r2);

      if (r1.RowCount != r2.RowCount)
        return false;
      if (r1.FieldCount != r2.FieldCount)
        return false;
      for (var i = 0; i < r1.FieldCount; i++) {
        if (r1.FieldNames[i] != r2.FieldNames[i]) {
          return false;
        }
      }
      return true;
    }

    protected virtual bool CompareExecuteNonQuery(string commandText, ISqlCompileUnit statement)
    {
      var compiledCommandText = PerformanceCheck
        ? CompileWithPerformanceCheck(statement)
        : CompileRegular(statement);

      Console.WriteLine(compiledCommandText);
      if (InMemory) {
        return true;
      }

      sqlCommand.CommandText = compiledCommandText;
      sqlCommand.Prepare();

      Console.WriteLine(commandText);
      dbCommand.CommandText = commandText;

      DbCommandExecutionResult r1, r2;
      r1 = GetExecuteNonQueryResult(dbCommand);
      r2 = GetExecuteNonQueryResult(sqlCommand);

      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine(r1);
      Console.WriteLine(r2);

      return r1.RowCount == r2.RowCount;
    }

    #endregion

    [Test]
    public void TestExtractCatalog()
    {
      Assert.GreaterOrEqual(Catalog.Schemas.Count, 1);
    }

    [Test]
    public void Test000()
    {
      var nativeSql = "SELECT \"a\".\"EmployeeId\", \"a\".\"FirstName\", \"a\".\"LastName\", \"a\".\"BirthDate\" " +
                      "FROM \"Employee\" \"a\" " +
                      "WHERE \"a\".\"FirstName\" = 'Robert' " +
                      "ORDER BY \"a\".\"LastName\"";

      var p = sqlCommand.CreateParameter();
      p.ParameterName = "p1";

      sqlDriver.TypeMappings.Mappings[typeof(string)].BindValue(p, "Robert");
      _ = sqlCommand.Parameters.Add(p);

      SqlTableRef employees = SqlDml.TableRef(schema.Tables["employee"]);
      SqlSelect select = SqlDml.Select(employees);
      select.Columns.AddRange(employees["EmployeeId"], employees["FirstName"], employees["LastName"], employees["BirthDate"]);
      select.Where = employees["FirstName"] == SqlDml.ParameterRef(p.ParameterName);
      select.OrderBy.Add(employees["LastName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test001()
    {
      var nativeSql = "SELECT DISTINCT \"a\".\"FirstName\" " + 
                      "FROM \"Employee\" \"a\" " +
                      "WHERE \"a\".\"Title\" = 'IT Staff'";

      var p = sqlCommand.CreateParameter();
      sqlDriver.TypeMappings.Mappings[typeof(string)].BindValue(p, "IT Staff");
      p.ParameterName = "p10";
      _ = sqlCommand.Parameters.Add(p);

      var employees = SqlDml.TableRef(schema.Tables["employee"]);
      var select = SqlDml.Select(employees);
      select.Distinct = true;
      select.Columns.AddRange(employees["FirstName"]);
      select.Where = employees["Title"] == SqlDml.ParameterRef(p.ParameterName);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test002()
    {
      var nativeSql = "SELECT * FROM \"Genre\" \"a\"";

      var region = SqlDml.TableRef(schema.Tables["genre"]);
      var select = SqlDml.Select(region);
      select.Columns.Add(SqlDml.Asterisk);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test003()
    {
      var nativeSql = "SELECT \"a\".\"EmployeeId\", \"a\".\"FirstName\", \"a\".\"LastName\" " +
                      "FROM \"Employee\" \"a\" " +
                      "WHERE (\"a\".\"EmployeeId\" < 2) " +
                      "ORDER BY \"a\".\"FirstName\" ASC, 3 DESC";

      var customer = SqlDml.TableRef(schema.Tables["Employee"]);
      var select = SqlDml.Select(customer);
      select.Columns.AddRange(customer["EmployeeId"], customer["FirstName"], customer["LastName"]);
      select.Where = customer["EmployeeId"] < 2;
      select.OrderBy.Add(customer["FirstName"]);
      select.OrderBy.Add(3, false);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test004()
    {
      var orders = SqlDml.TableRef(schema.Tables["invoice"]);
      var select = SqlDml.Select(orders);
      select.Columns.Add(SqlDml.Asterisk);
      select.Where = orders["PaymentDate"] > new DateTime(2013, 8, 1);
      select.OrderBy.Add(orders["PaymentDate"], false);

      var compiledText = Compile(select);
      Console.WriteLine(compiledText);

      if (InMemory)
        return;

      sqlCommand.CommandText = compiledText;
      sqlCommand.Prepare();

      var r = GetExecuteDataReaderResult(sqlCommand);
      Assert.AreEqual(14, r.RowCount);
    }

    [Test]
    public void Test005()
    {
      var orders = SqlDml.TableRef(schema.Tables["invoice"]);
      var select = SqlDml.Select(orders);
      select.Limit = 10;
      select.Columns.Add(SqlDml.Asterisk);

      var compiledText = Compile(select);
      Console.WriteLine(compiledText);

      if (InMemory)
        return;

      sqlCommand.CommandText = compiledText;
      sqlCommand.Prepare();

      var r = GetExecuteDataReaderResult(sqlCommand);
      Assert.AreEqual(10, r.RowCount);
    }

    [Test]
    public void Test006()
    {
      var nativeSql = "SELECT \"t\".\"TrackId\", \"t\".\"Name\", \"a\".\"Title\" " +
                      "FROM \"Track\" \"t\" " +
                      "INNER JOIN \"Album\" \"a\" ON (\"t\".\"AlbumId\" = \"a\".\"AlbumId\") " +
                      "ORDER BY \"t\".\"TrackId\"";

      var track = SqlDml.TableRef(schema.Tables["Track"], "t");
      var album = SqlDml.TableRef(schema.Tables["Album"], "a");

      var select = SqlDml.Select(track.InnerJoin(album, track["AlbumId"] == album["AlbumId"]));

      select.Columns.Add(track["TrackId"]);
      select.Columns.Add(track["Name"]);
      select.Columns.Add(album["Title"]);

      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test007()
    {
      var nativeSql =
        "SELECT \"c\".\"CustomerId\"," +
              " \"c\".\"CompanyName\"," +
              " \"c\".\"FirstName\"," +
              " \"c\".\"LastName\", \"i\".\"InvoiceDate\", \"il\".\"TrackId\"," +
              " \"t\".\"Name\", \"il\".\"UnitPrice\", \"il\".\"Quantity\" " +
        "FROM \"Customer\" \"c\" " +
        "INNER JOIN \"Invoice\" \"i\" ON (\"c\".\"CustomerId\" = \"i\".\"CustomerId\") " +
        "INNER JOIN \"InvoiceLine\" \"il\" ON (\"i\".\"InvoiceId\" = \"il\".\"InvoiceId\") "+
        "INNER JOIN \"Track\" \"t\" ON (\"t\".\"TrackId\" = \"il\".\"TrackId\")";

      var customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");
      var invoiceLine = SqlDml.TableRef(schema.Tables["invoiceline"], "il");
      var track = SqlDml.TableRef(schema.Tables["track"], "t");

      var select = SqlDml.Select(customer
        .InnerJoin(invoice, customer["CustomerId"] == invoice["CustomerId"])
          .InnerJoin(invoiceLine, invoiceLine["InvoiceId"] == invoice["InvoiceId"])
            .InnerJoin(track, track["TrackId"] == invoiceLine["TrackId"]));
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
    public void Test008()
    {
      var nativeSql = "SELECT \"i\".\"InvoiceId\", round(\"i\".\"Commission\" * 12, 1) as \"Rounded\" " +
                      "FROM \"Invoice\" \"i\" " +
                      "WHERE \"i\".\"InvoiceId\" = 412";

      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");

      var select = SqlDml.Select(invoice);
      select.Columns.Add(invoice["InvoiceId"]);
      select.Columns.Add(SqlDml.Round(invoice["Commission"] * 12, 1), "Rounded");
      select.Where = invoice["invoiceId"]==412;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test009()
    {
      var nativeSql =
        "SELECT" +
                "\"c\".\"CustomerId\", " +
                "\"c\".\"CompanyName\", " +
                "\"c\".\"LastName\", " +
                "SUM(\"i\".\"Commission\") AS \"Total\" " +
        "FROM \"Customer\" \"c\" " +
        "INNER JOIN \"Invoice\" \"i\" ON (\"c\".\"CustomerId\" = \"i\".\"CustomerId\") " +
        "GROUP BY \"c\".\"CustomerId\", \"c\".\"CompanyName\", \"c\".\"LastName\" " +
        "ORDER BY \"c\".\"CustomerId\"";

      var customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");

      var select = SqlDml.Select(customer.InnerJoin(invoice, customer["CustomerId"] == invoice["CustomerId"]));

      select.Columns.AddRange(customer["CustomerId"], customer["CompanyName"], customer["LastName"]);
      select.Columns.Add(SqlDml.Sum(invoice["Commission"]), "Total");
      select.GroupBy.AddRange(customer["CustomerId"], customer["CompanyName"], customer["LastName"]);

      select.OrderBy.Add(customer["CustomerId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test010()
    {
      var nativeSql = 
        "SELECT " +
                "CASE \"il\".\"TrackId\" WHEN 1 THEN 'STAFF_1' WHEN 2 THEN 'STAFF_2' ELSE 'STAFF_OTHER' END AS \"shippers\"," +
                "SUM(\"il\".\"UnitPrice\") AS \"TotalUnits\" " +
                "FROM \"InvoiceLine\" \"il\" " +
                "GROUP BY \"il\".\"TrackId\"";

      var invoiceLine = SqlDml.TableRef(schema.Tables["invoiceline"], "il");

      var select = SqlDml.Select(invoiceLine);
      var totalPayment = SqlDml.Case(invoiceLine["TrackId"]);
      totalPayment[1] = SqlDml.Literal("STAFF_1");
      totalPayment[2] = SqlDml.Literal("STAFF_2");
      totalPayment.Else = SqlDml.Literal("STAFF_OTHER");
      select.Columns.Add(totalPayment, "shippers");

      select.Columns.Add(SqlDml.Sum(invoiceLine["UnitPrice"]), "TotalUnits");
      select.GroupBy.AddRange(invoiceLine["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test011()
    {
      var nativeSql =
        "SELECT " +
                "\"i\".\"InvoiceId\", " +
                "\"i\".\"DesignatedEmployeeId\", " +
                "\"i\".\"Status\" " +
         "FROM \"Invoice\" \"i\" " +
         "WHERE \"i\".\"BillingState\" IS NOT NULL " +
         "ORDER BY \"i\".\"InvoiceId\"";

      var invoice = SqlDml.TableRef(schema.Tables["Invoice"], "i");
      var select = SqlDml.Select(invoice);
      select.Columns.AddRange(invoice["InvoiceId"], invoice["DesignatedEmployeeId"], invoice["Status"]);

      select.Where = SqlDml.IsNotNull(invoice["BillingState"]);
      select.OrderBy.Add(invoice["InvoiceId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test012()
    {
      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");
      var select = SqlDml.Select(invoice);
      select.Columns.Add(SqlDml.FunctionCall("EXTRACT", SqlDml.Native("MONTH"), 1 + 1, invoice["PaymentDate"]), "TimeToToday");
      select.Where = SqlDml.IsNotNull(invoice["PaymentDate"]);

      Console.WriteLine(Compile(select));
    }

    [Test]
    public void Test013()
    {
      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");
      var select = SqlDml.Select(invoice);
      select.Columns.Add(SqlDml.DateTimeAddMonths(invoice["PaymentDate"], 1 + 1), "TimeToToday");
      select.Where = SqlDml.IsNotNull(invoice["PaymentDate"]);

      Console.WriteLine(Compile(select));
    }

    [Test]
    public void Test014()
    {
      var nativeSql = "SELECT (CURRENT_TIMESTAMP + (-15) * interval '1 month') AS \"Days\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddMonths(SqlDml.Native("CURRENT_TIMESTAMP"), -15), "Days");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test015()
    {
      var nativeSql = "SELECT (CURRENT_TIMESTAMP + (1 + 1) * interval '1 month') AS \"Days\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeAddMonths(SqlDml.Native("CURRENT_TIMESTAMP"), SqlDml.Add(1, 1)), "Days");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test016()
    {
      var nativeSql = "SELECT EXTRACT(DAY FROM CURRENT_TIMESTAMP) AS \"DayOfMonth\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Day, SqlDml.Native("CURRENT_TIMESTAMP")), "DayOfMonth");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test017()
    {
      var nativeSql = "SELECT EXTRACT(MONTH FROM CURRENT_TIMESTAMP) AS \"Month\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Month, SqlDml.Native("CURRENT_TIMESTAMP")), "Month");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test018()
    {
      var nativeSql = "select EXTRACT(YEAR FROM CURRENT_TIMESTAMP) AS \"Year\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, SqlDml.Native("CURRENT_TIMESTAMP")), "Year");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test019()
    {
      var nativeSql = "SELECT EXTRACT(HOUR FROM CURRENT_TIMESTAMP) AS \"Hour\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Hour, SqlDml.Native("CURRENT_TIMESTAMP")), "Hour");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test020()
    {
      var nativeSql = "SELECT EXTRACT(MINUTE FROM CURRENT_TIMESTAMP) AS \"Minutes\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Minute, SqlDml.Native("CURRENT_TIMESTAMP")), "Minutes");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test021()
    {
      var nativeSql = "SELECT EXTRACT(SECOND FROM CURRENT_TIMESTAMP) AS \"Seconds\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Second, SqlDml.Native("CURRENT_TIMESTAMP")), "Seconds");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test022()
    {
      var nativeSql = "SELECT EXTRACT(MILLISECOND FROM CURRENT_TIMESTAMP) AS \"Milliseconds\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Millisecond, SqlDml.Native("CURRENT_TIMESTAMP")), "Milliseconds");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test023()
    {
      var nativeSql = "SELECT cast('2011-11-16' as date) AS \"BirthDay\"";

      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeConstruct(2011, 11, 16), "BirthDay");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    //[Ignore("A close inspection needed")]
    public void Test024()
    {
      var select = SqlDml.Select();
      select.Columns.Add(SqlDml.DateTimeMinusDateTime(DateTime.Now, DateTime.Now.AddDays(-4)), "FewDaysAgo");

      Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
    }

    [Test]
    public void Test025()
    {
      var nativeSql = "SELECT SUM(\"i\".\"Commission\") AS \"sum\" FROM \"Invoice\" \"i\"";

      var invoices = SqlDml.TableRef(schema.Tables["Invoice"], "i");
      var select = SqlDml.Select(invoices);
      select.Columns.Add(SqlDml.Sum(invoices["Commission"]), "sum");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test026()
    {
      var customer = SqlDml.TableRef(schema.Tables["Customer"], "c");

      var select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(SqlDml.RawConcat("Mr. ", customer["LastName"]), "FullName");
      Console.WriteLine(sqlDriver.Compile(select).GetCommandText());
    }

    [Test]
    public void Test027()
    {
      var nativeSql = "SELECT \"c\".\"CustomerId\", \"c\".\"FirstName\" || ', ' || \"c\".\"LastName\" as \"FullName\" FROM \"Customer\" \"c\"";

      var customer = SqlDml.TableRef(schema.Tables["Customer"], "c");

      var select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(SqlDml.Concat(SqlDml.Concat(customer["FirstName"], SqlDml.Literal(", ")), customer["LastName"]), "FullName");
      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test028()
    {
      var nativeSql =
        "SELECT " +
                "\"c\".\"CustomerId\", " +
                "\"c\".\"CompanyName\", " +
                "\"c\".\"LastName\", " +
                "SUM(\"i\".\"Commission\") AS \"Total\" " +
        "FROM \"Customer\" \"c\" " +
        "INNER JOIN \"Invoice\" \"i\" ON (\"c\".\"CustomerId\" = \"i\".\"CustomerId\") " +
        "GROUP BY " +
                  "\"c\".\"CustomerId\", " +
                  "\"c\".\"CompanyName\", " +
                  "\"c\".\"LastName\" " +
        "HAVING SUM(\"i\".\"Commission\") > 140";

      var customer = SqlDml.TableRef(schema.Tables["Customer"], "c");
      var invoice = SqlDml.TableRef(schema.Tables["Invoice"], "i");
      var select = SqlDml.Select(customer.InnerJoin(invoice, customer["CustomerId"] == invoice["CustomerId"]));

      select.Columns.AddRange(customer["CustomerID"], customer["CompanyName"], customer["LastName"]);
      select.Columns.Add(SqlDml.Sum(invoice["Commission"]), "Total");

      select.GroupBy.AddRange(customer["CustomerID"], customer["CompanyName"], customer["LastName"]);

      select.Having = SqlDml.Sum(invoice["Commission"]) > 140;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test029()
    {
      var nativeSql =
        "SELECT " +
                "\"c\".\"CustomerId\", " +
                "\"c\".\"CompanyName\", " +
                "\"c\".\"LastName\" " +
        "FROM \"Customer\" \"c\" " +
        "WHERE \"c\".\"CustomerId\" IN (SELECT \"i\".\"CustomerId\" FROM \"Invoice\" \"i\" WHERE \"i\".\"DesignatedEmployeeId\" = 8) " +
        "GROUP BY \"c\".\"CustomerId\", \"c\".\"CompanyName\", \"c\".\"LastName\"";

      var customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");

      var innerSelect = SqlDml.Select(invoice);
      innerSelect.Columns.Add(invoice["CustomerId"]);
      innerSelect.Where = invoice["DesignatedEmployeeId"] == 8;

      var select = SqlDml.Select(customer);

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
    public void Test030()
    {
      var nativeSql =
        "SELECT " +
                "\"t\".\"TrackId\", " +
                "\"t\".\"Name\", " +
                "\"t\".\"UnitPrice\" " +
        "FROM \"Track\" \"t\" " +
        "WHERE \"t\".\"Milliseconds\" BETWEEN 50 AND 40000 " +
        "ORDER BY \"t\".\"TrackId\"";

      var track = SqlDml.TableRef(schema.Tables["track"], "t");
      var select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"]);
      select.Where = SqlDml.Between(track["Milliseconds"], 50, 40000);
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test031()
    {
      var nativeSql = @"SELECT" +
                               "\"t\".\"TrackId\", " +
                               "\"t\".\"Name\", " +
                               "\"t\".\"UnitPrice\", " +
                               "\"t\".\"AlbumId\" " +
                        "FROM \"Track\" \"t\" " +
                        "WHERE \"t\".\"AlbumId\" IN (2, 8) " +
                        "ORDER BY \"t\".\"TrackId\"";

      var track = SqlDml.TableRef(schema.Tables["track"], "f");
      var select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"], track["AlbumId"]);
      select.Where = SqlDml.In(track["AlbumId"], SqlDml.Row(2, 8));
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test032()
    {
      var nativeSql = "SELECT " +
                               "\"t\".\"TrackId\", " +
                               "\"t\".\"Name\", " +
                               "\"t\".\"UnitPrice\", " +
                               "\"t\".\"AlbumId\" " +
                        "FROM \"Track\" \"t\" " +
                        "WHERE \"t\".\"Name\" LIKE 'R%' " +
                        "ORDER BY \"t\".\"TrackId\"";

      var track = SqlDml.TableRef(schema.Tables["Track"], "t");
      var select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"], track["AlbumId"]);
      select.Where = SqlDml.Like(track["Name"], "R%");
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test033()
    {
      var nativeSql =
        @"SELECT " +
                 "\"t\".\"TrackId\", " +
                 "\"t\".\"Name\", " +
                 "\"t\".\"UnitPrice\" " +
         "FROM \"Track\" \"t\" " +
         "WHERE (\"t\".\"AlbumId\" = 3 OR \"t\".\"AlbumId\" = 8) AND \"t\".\"UnitPrice\" < 1 " +
         "ORDER BY \"t\".\"TrackId\"";

      var track = SqlDml.TableRef(schema.Tables["track"], "t");
      var select = SqlDml.Select(track);
      select.Columns.AddRange(track["TrackId"], track["Name"], track["UnitPrice"]);
      select.Where = (track["AlbumId"] == 3 || track["AlbumId"] == 8) && track["UnitPrice"] < 1;
      select.OrderBy.Add(track["TrackId"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test034()
    {
      var nativeSql =
        "SELECT " +
                "EXTRACT(YEAR FROM \"PaymentDate\") AS \"Year\", " +
                 "COUNT(*) AS \"Required\" " +
        "FROM \"Invoice\" \"r\" " +
        "GROUP BY EXTRACT(YEAR FROM \"PaymentDate\")";

      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "r");

      var select = SqlDml.Select(invoice);
      select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, invoice["PaymentDate"]), "Year");
      select.Columns.Add(SqlDml.Count(), "Required");

      select.GroupBy.Add(SqlDml.Extract(SqlDateTimePart.Year, invoice["PaymentDate"]));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test035()
    {
      var nativeSql =
        "SELECT " +
                "\"p\".\"CustomerId\"," +
                "\"p\".\"Commission\" " +
        "FROM \"Invoice\" \"p\" " +
        "WHERE \"p\".\"Commission\" = (SELECT MIN(\"Commission\") AS \"LowestCommission\" FROM \"Invoice\")";

      var invoice1 = SqlDml.TableRef(schema.Tables["invoice"], "p1");
      var invoice2 = SqlDml.TableRef(schema.Tables["invoice"], "p2");

      var innerSelect = SqlDml.Select(invoice2);
      innerSelect.Columns.Add(SqlDml.Min(invoice2["Commission"]));

      var select = SqlDml.Select(invoice1);
      select.Columns.Add(invoice1["CustomerId"]);
      select.Columns.Add(invoice1["Commission"]);

      select.Where = SqlDml.Equals(invoice1["Commission"], innerSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test036()
    {
      var nativeSql =
        "SELECT" +
                "\"c\".\"CustomerId\", " +
                "\"c\".\"CompanyName\" " +
        "FROM \"Customer\" \"c\" " +
        "WHERE EXISTS " +
                     "(SELECT * FROM \"Invoice\" \"i\" " +
                     " WHERE \"i\".\"Commission\" < 1.00 AND \"i\".\"CustomerId\" = \"c\".\"CustomerId\" )";

      var customer = SqlDml.TableRef(schema.Tables["customer"], "c");
      var invoice = SqlDml.TableRef(schema.Tables["invoice"], "i");

      var innerSelect = SqlDml.Select(invoice);
      var select = SqlDml.Select(customer);

      innerSelect.Columns.Add(SqlDml.Asterisk);
      innerSelect.Where = invoice["Commission"] < 11.00 && invoice["CustomerId"] == customer["CustomerId"];

      select.Columns.Add(customer["CustomerId"]);
      select.Columns.Add(customer["CompanyName"]);
      select.Where = SqlDml.Exists(innerSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test037()
    {
      var nativeSql = "SELECT * FROM \"Customer\" \"c\" LIMIT 10 OFFSET 0";

      var customer = SqlDml.TableRef(schema.Tables["customer"]);
      var select = SqlDml.Select(customer);
      select.Limit = 10;
      select.Offset = 0;
      select.Columns.Add(SqlDml.Asterisk);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test038()
    {
      var nativeSql = "UPDATE \"Invoice\" " +
                      "SET \"Total\" = \"Total\" * 1 " +
                      "WHERE \"InvoiceId\" = 10;";

      var invoice = SqlDml.TableRef(schema.Tables["invoice"]);
      var update = SqlDml.Update(invoice);
      update.Values[invoice["Total"]] = invoice["Total"] * 1;
      update.Where = invoice["InvoiceId"] == 10;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test039()
    {
      var create = SqlDdl.Create(schema.Tables["customer"]);
      Console.Write(Compile(create));
    }

    [Test]
    public void Test040()
    {
      var drop = SqlDdl.Drop(schema.Tables["customer"]);
      Console.Write(Compile(drop));
    }

    [Test]
    public void Test041()
    {
      var drop = SqlDdl.Drop(schema);
      Console.Write(Compile(drop));
    }

    [Test]
    public void Test042()
    {
      //var create = SqlDdl.Create(schema.Views["Invoice Subtotals"]);
      //Console.Write(Compile(create));
    }

    [Test]
    public void Test043()
    {
      var create = SqlDdl.Create(Catalog.DefaultSchema);
      Console.Write(Compile(create));
    }

    [Test]
    public void Test044()
    {
      var alter = SqlDdl.Alter(schema.Tables["customer"], SqlDdl.AddColumn(schema.Tables["customer"].TableColumns["CompanyName"]));

      Console.Write(Compile(alter));
    }

    [Test]
    public void Test045()
    {
      var alter = SqlDdl.Alter(schema.Tables["customer"], SqlDdl.DropColumn(schema.Tables["customer"].TableColumns["CompanyName"]));

      Console.Write(Compile(alter));
    }

    [Test]
    public void Test046()
    {
      var renameColumn = SqlDdl.Rename(schema.Tables["customer"].TableColumns["LastName"], "LastName1");

      Console.Write(Compile(renameColumn));
    }

    [Test]
    public void Test047()
    {
      var t = schema.Tables["customer"];
      var uc = t.CreateUniqueConstraint("newUniqueConstraint", t.TableColumns["Phone"]);
      var stmt = SqlDdl.Alter(t, SqlDdl.AddConstraint(uc));

      Console.Write(Compile(stmt));
    }

    [Test]
    public void Test048()
    {
      var t = schema.Tables["customer"];
      var index = t.CreateIndex("MegaIndex195");
      _ = index.CreateIndexColumn(t.TableColumns[0]);
      var create = SqlDdl.Create(index);

      Console.Write(Compile(create));
    }

    [Test]
    public void Test049()
    {
      var t = schema.Tables["customer"];
      var index = t.CreateIndex("MegaIndex196");
      _ = index.CreateIndexColumn(t.TableColumns[0]);
      var drop = SqlDdl.Drop(index);

      Console.Write(Compile(drop));
    }

    [Test]
    public void Test050()
    {
      var alter = SqlDdl.Rename(schema.Tables["customer"], "SomeWierdTableName");
      Console.Write(Compile(alter));
    }

    [Test]
    public void Test051()
    {
      var t = schema.CreateTable("SomeWierdTableName");
      _ = t.CreateColumn("Field01", new SqlValueType(SqlType.Int32));
      _ = t.CreateColumn("Field02", new SqlValueType(SqlType.Int32));

      var uc = t.CreatePrimaryKey(string.Empty, t.TableColumns["Field02"]);
      var stmt = SqlDdl.Alter(t, SqlDdl.AddConstraint(uc));

      Console.Write(Compile(stmt));
    }

    [Test]
    public void Test052()
    {
      var nativeSql =
        "SELECT \"a\".\"f\" " +
        "FROM (" +
               "SELECT 1 as \"f\" " +
               "UNION SELECT 2 " +
               "UNION SELECT 3 " +
               "UNION SELECT 4) \"a\"";

      var s1 = SqlDml.Select();
      var s2 = SqlDml.Select();
      var s3 = SqlDml.Select();
      var s4 = SqlDml.Select();
      s1.Columns.Add(1, "f");
      s2.Columns.Add(2);
      s3.Columns.Add(3);
      s4.Columns.Add(4);
      var qr = SqlDml.QueryRef(s1.Union(s2).Union(s3.Union(s4)), "a");
      var select = SqlDml.Select(qr);
      select.Columns.Add(qr["f"]);

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, select));
    }
  }
}
