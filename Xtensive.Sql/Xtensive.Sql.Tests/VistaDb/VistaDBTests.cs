using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql.Info;
using Xtensive.Sql.Model;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Tests.VistaDb;

namespace Xtensive.Sql.Tests.VistaDb
{
  [TestFixture, Explicit]
  public class VistaDBTests : VistaDBAdventureWorks
  {
    private SqlDriver SqlDriver;
    private SqlConnection sqlConnection;
    private IDbCommand dbCommand;
    private DbCommand sqlCommand;

    private string Compile(ISqlCompileUnit statement)
    {
      return SqlDriver.Compile(statement).GetCommandText();
    }

    private bool CompareExecuteDataReader(string commandText, ISqlCompileUnit statement)
    {
      sqlCommand.CommandText = Compile(statement);
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

      if (r1.RowCount != r2.RowCount)
        return false;
      if (r1.FieldCount != r2.FieldCount)
        return false;
      for (int i = 0; i < r1.FieldCount; i++) {
        if (r1.FieldNames[i] != r2.FieldNames[i]) {
          return false;
        }
      }
      return true;
    }

    private bool CompareExecuteNonQuery(string commandText, ISqlCompileUnit statement)
    {
      sqlCommand.CommandText = Compile(statement);
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

      if (r1.RowCount != r2.RowCount)
        return false;
      return true;
    }

    [TestFixtureSetUp]
    public override void SetUp()
    {
      base.SetUp();
      SqlDriver = SqlDriver.Create(TestUrl.VistaDbAW);
      sqlConnection = SqlDriver.CreateConnection(TestUrl.VistaDbAW);
      dbCommand = sqlConnection.CreateCommand();
      sqlCommand = sqlConnection.CreateCommand();
      try {
        sqlConnection.Open();
      }
      catch (SystemException e) {
        Console.WriteLine(sqlConnection.Url);
        Console.WriteLine(e);
      }
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      try {
        if ((sqlConnection != null) && (sqlConnection.State != ConnectionState.Closed))
          sqlConnection.Close();
      }
      catch (Exception ex) {
        Console.WriteLine(ex.Message);
      }
    }

    [Test]
    public void Test000()
    {
      SqlSelect select = SqlDml.Select();
      SqlExpression l = SqlDml.Literal(DateTime.Now);
      select.Where = (l + l) > l;

      sqlCommand.CommandText = SqlDriver.Compile(select).GetCommandText();
      sqlCommand.Prepare();
      Console.WriteLine(sqlCommand.CommandText);
    }

    [Test]
    public void Test0000()
    {
      var p = sqlCommand.CreateParameter();
      p.DbType = DbType.Int32;
      p.Value = 40;
      p.ParameterName = "p1";
      sqlCommand.Parameters.Add(p);

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"], product["ListPrice"]);
      select.Where = product["ListPrice"] > SqlDml.ParameterRef(p.ParameterName);
      select.OrderBy.Add(product["ListPrice"]);

      sqlCommand.CommandText = SqlDriver.Compile(select).GetCommandText();
      sqlCommand.Prepare();

      DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
      Console.WriteLine(r);
    }

    [Test]
    public void Test001()
    {
      string nativeSql = "SELECT ProductID, Name, ListPrice "
        + "FROM Production_Product "
          + "WHERE ListPrice > 40 "
            + "ORDER BY ListPrice ASC";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"], product["ListPrice"]);
      select.Where = product["ListPrice"] > 40;
      select.OrderBy.Add(product["ListPrice"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test002()
    {
      string nativeSql = "SELECT * "
        +"FROM Purchasing_ShipMethod";

      SqlTableRef purchasing = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ShipMethod"]);
      SqlSelect select = SqlDml.Select(purchasing);
      select.Columns.Add(SqlDml.Asterisk);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test003()
    {
      string nativeSql = "SELECT DISTINCT Sales_Customer.CustomerID, Sales_Store.Name "
        +"FROM Sales_Customer JOIN Sales_Store ON "
          +"(Sales_Customer.CustomerID = Sales_Store.CustomerID) "
            +"WHERE Sales_Customer.TerritoryID = 1";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlTableRef store = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Distinct = true;
      select.Columns.AddRange(customer["CustomerID"], store["Name"]);
      select.From = select.From.InnerJoin(store, customer["CustomerID"]==store["CustomerID"]);
      select.Where = customer["TerritoryID"]==1;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test004()
    {
      string nativeSql = "SELECT DISTINCT c.CustomerID, s.Name "
        +"FROM Sales_Customer AS c "
          +"JOIN "
            +"Sales_Store AS s "
              +"ON ( c.CustomerID = s.CustomerID) "
                +"WHERE c.TerritoryID = 1";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"], "c");
      SqlTableRef store = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "s");
      SqlSelect select =
        SqlDml.Select(customer.InnerJoin(store, customer["CustomerID"]==store["CustomerID"]));
      select.Distinct = true;
      select.Columns.AddRange(customer["CustomerID"], store["Name"]);
      select.Where = customer["TerritoryID"]==1;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test005()
    {
      string nativeSql = "SELECT DISTINCT ShipToAddressID, TerritoryID "
        +"FROM Sales_SalesOrderHeader "
          +"ORDER BY TerritoryID";

      SqlTableRef salesOrderHeader = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"]);
      SqlSelect select = SqlDml.Select(salesOrderHeader);
      select.Distinct = true;
      select.Columns.AddRange(salesOrderHeader["ShipToAddressID"], salesOrderHeader["TerritoryID"]);
      select.OrderBy.Add(salesOrderHeader["TerritoryID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test006()
    {
      string nativeSql = "SELECT TOP 10 SalesOrderID, OrderDate "
        +"FROM Sales_SalesOrderHeader "
          +"WHERE OrderDate < '2007-01-01T01:01:01.012'"
            +"ORDER BY OrderDate DESC";

      SqlTableRef salesOrderHeader = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"]);
      SqlSelect select = SqlDml.Select(salesOrderHeader);
      select.Limit = 10;
      select.Columns.AddRange(salesOrderHeader["SalesOrderID"], salesOrderHeader["OrderDate"]);
      select.Where = salesOrderHeader["OrderDate"]<DateTime.Now;
      select.OrderBy.Add(salesOrderHeader["OrderDate"], false);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test007()
    {
      string nativeSql = "SELECT e.EmployeeID AS \"Employee ID\", "
        +"c.FirstName + ' ' + c.LastName AS \"Employee Name\", "
          +"c.Phone "
            +"FROM HumanResources_Employee e "
              +"JOIN Person_Contact c "
                +"ON e.ContactID = c.ContactID "
                  +"ORDER BY LastName, FirstName ASC";

      SqlTableRef employee = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlSelect select =
        SqlDml.Select(employee.InnerJoin(contact, employee["ContactID"]==contact["ContactID"]));
      select.Columns.Add(employee["EmployeeID"], "Employee ID");
      select.Columns.Add(contact["FirstName"]+"."+contact["LastName"], "Employee Name");
      select.Columns.Add(contact["Phone"]);
      select.OrderBy.Add(contact["LastName"]);
      select.OrderBy.Add(contact["FirstName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test008()
    {
      string nativeSql = "SELECT * "
        +"FROM Production_Product "
          +"ORDER BY Name";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(SqlDml.Asterisk);
      select.OrderBy.Add(product["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test009()
    {
      string nativeSql = "SELECT TOP 10 s.UnitPrice, p.* "
        +"FROM Production_Product p "
          +"JOIN "
            +"Sales_SalesOrderDetail s "
              +"ON (p.ProductID = s.ProductID) "
                +"ORDER BY p.ProductID";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "s");
      SqlSelect select = SqlDml.Select(product.InnerJoin(salesOrderDetail, product["ProductID"]==salesOrderDetail["ProductID"]));
      select.Limit = 10;
      select.Columns.Add(salesOrderDetail["UnitPrice"]);
      select.Columns.Add(product.Asterisk);
      select.OrderBy.Add(product["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test010()
    {
      string nativeSql = "SELECT * "
        +"FROM Sales_Customer "
          +"ORDER BY CustomerID ASC";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.Add(SqlDml.Asterisk);
      select.OrderBy.Add(customer["CustomerID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test011()
    {
      string nativeSql = ""
        +
        "SELECT CustomerID, TerritoryID, AccountNumber, CustomerType, ModifiedDate "
          +"FROM Sales_Customer "
            +"ORDER BY CustomerID ASC";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerID"]);
      select.Columns.Add(customer["TerritoryID"]);
      select.Columns.Add(customer["AccountNumber"]);
      select.Columns.Add(customer["CustomerType"]);
      select.Columns.Add(customer["ModifiedDate"]);
      select.OrderBy.Add(customer["CustomerID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test012()
    {
      string nativeSql = "SELECT c.FirstName, c.Phone "
        +"FROM HumanResources_Employee e "
          +"JOIN Person_Contact c "
            +"ON e.ContactID = c.ContactID "
              +"ORDER BY FirstName ASC";

      SqlTableRef employee = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlSelect select =
        SqlDml.Select(employee.InnerJoin(contact, employee["ContactID"]==contact["ContactID"]));
      select.Columns.AddRange(contact["FirstName"], contact["Phone"]);
      select.OrderBy.Add(contact["FirstName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test013()
    {
      string nativeSql = "SELECT LastName + ', ' + FirstName AS ContactName "
        +"FROM Person_Contact "
          +"ORDER BY LastName, FirstName ASC";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.Add(contact["LastName"]+", "+contact["FirstName"], "ContactName");
      select.OrderBy.Add(contact["LastName"]);
      select.OrderBy.Add(contact["FirstName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test014()
    {
      string nativeSql = "SELECT ROUND( (ListPrice * .9), 2) AS DiscountPrice "
        +"FROM Production_Product "
          +"WHERE ProductID = 58";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(SqlDml.Round(product["ListPrice"]*0.9, 2), "DiscountPrice");
      select.Where = product["ProductID"]==58;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test015()
    {
      string nativeSql = "SELECT ( CAST(ProductID AS VARCHAR(10)) + ': ' "
        +"+ Name ) AS ProductIDName "
          +"FROM Production_Product";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(
        SqlDml.Cast(product["ProductID"], new SqlValueType("varchar(10)")), "ProductIDName");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test016()
    {
      string nativeSql = "SELECT ProductID, Name, "
        +"CASE Class "
          +"WHEN 'H' THEN ROUND( (ListPrice * .6), 2) "
            +"WHEN 'L' THEN ROUND( (ListPrice * .7), 2) "
              +"WHEN 'M' THEN ROUND( (ListPrice * .8), 2) "
                +"ELSE ROUND( (ListPrice * .9), 2) "
                  +"END AS DiscountPrice "
                    +"FROM Production_Product";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      SqlCase discountPrice = SqlDml.Case(product["Class"]);
      discountPrice['H'] = SqlDml.Round(product["ListPrice"]*0.6, 2);
      discountPrice['L'] = SqlDml.Round(product["ListPrice"]*0.7, 2);
      discountPrice['M'] = SqlDml.Round(product["ListPrice"]*0.8, 2);
      discountPrice.Else = SqlDml.Round(product["ListPrice"]*0.6, 2);
      select.Columns.Add(discountPrice, "DiscountPrice");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test017()
    {
      string nativeSql = "SELECT Prd.ProductID, Prd.Name, "
        +"(   SELECT SUM(OD.UnitPrice * OD.OrderQty) "
          +"FROM Sales_SalesOrderDetail AS OD "
            +"WHERE OD.ProductID = Prd.ProductID "
              +") AS SumOfSales "
                +"FROM Production_Product AS Prd "
                  +"ORDER BY Prd.ProductID";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "Prd");
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      SqlTableRef salesOrderDetail =
        SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "OD");
      SqlSelect sumOfSales = SqlDml.Select(salesOrderDetail);
      sumOfSales.Columns.Add(SqlDml.Sum(salesOrderDetail["UnitPrice"]*salesOrderDetail["OrderQty"]));
      sumOfSales.Where = salesOrderDetail["ProductID"]==product["ProductID"];
      select.Columns.Add(sumOfSales, "SumOfSales");
      select.OrderBy.Add(product["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test018()
    {
      string nativeSql = "SELECT p.ProductID, p.Name, "
        +"SUM (p.ListPrice * i.Quantity) AS InventoryValue "
          +"FROM Production_Product p "
            +"JOIN Production_ProductInventory i "
              +"ON p.ProductID = i.ProductID "
                +"GROUP BY p.ProductID, p.Name "
                  +"ORDER BY p.ProductID";

      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef i = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductInventory"], "i");
      SqlSelect select = SqlDml.Select(p.InnerJoin(i, p["ProductID"]==i["ProductID"]));
      select.Columns.AddRange(p["ProductID"], p["Name"]);
      select.Columns.Add(SqlDml.Sum(p["ListPrice"]*i["Quantity"]), "InventoryValue");
      select.GroupBy.AddRange(p["ProductID"], p["Name"]);
      select.OrderBy.Add(p["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test019()
    {
      string nativeSql = "SELECT SalesOrderID, "
        +"DATEDIFF(dd, ShipDate, GETDATE() ) AS DaysSinceShipped "
          +"FROM Sales_SalesOrderHeader "
            +"WHERE ShipDate IS NOT NULL";

      SqlTableRef salesOrderHeader = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"]);
      SqlSelect select = SqlDml.Select(salesOrderHeader);
      select.Columns.Add(salesOrderHeader["SalesOrderID"]);
      select.Columns.Add(
        SqlDml.FunctionCall(
          "DATEDIFF", SqlDml.Native("dd"), salesOrderHeader["ShipDate"], SqlDml.CurrentDate()),
        "DaysSinceShipped");
      select.Where = SqlDml.IsNotNull(salesOrderHeader["ShipDate"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test020()
    {
      string nativeSql = "SELECT SalesOrderID, "
        +"DATEDIFF(dd, ShipDate, GETDATE() ) as DaysSinceShipped "
          +"FROM Sales_SalesOrderHeader "
            +"WHERE ShipDate IS NOT NULL";

      SqlTableRef salesOrderHeader = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"]);
      SqlSelect select = SqlDml.Select(salesOrderHeader);
      select.Columns.Add(salesOrderHeader["SalesOrderID"]);
      select.Columns.Add(
        SqlDml.FunctionCall(
          "DATEDIFF", SqlDml.Native("dd"), salesOrderHeader["ShipDate"], SqlDml.CurrentDate()),
        "DaysSinceShipped");
      select.Where = SqlDml.IsNotNull(salesOrderHeader["ShipDate"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test021()
    {
      string nativeSql = "SELECT Name AS \"Product Name\" "
        +"FROM Production_Product "
          +"ORDER BY Name ASC";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"], "Product Name");
      select.OrderBy.Add(product["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test022()
    {
      string nativeSql = "SELECT SUM(TotalDue) AS \"sum\" "
        +"FROM Sales_SalesOrderHeader";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(SqlDml.Sum(product["TotalDue"]), "sum");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test023()
    {
      string nativeSql = "SELECT DISTINCT ProductID "
        +"FROM Production_ProductInventory";

      SqlTableRef productInventory =
        SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductInventory"]);
      SqlSelect select = SqlDml.Select(productInventory);
      select.Distinct = true;
      select.Columns.Add(productInventory["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test024()
    {
      string nativeSql = "SELECT Cst.CustomerID, St.Name, Ord.ShipDate, Ord.Freight "
        +"FROM Sales_Store AS St "
          +"JOIN Sales_Customer AS Cst "
            +"ON St.CustomerID = Cst.CustomerID "
              +"JOIN Sales_SalesOrderHeader AS Ord "
                +"ON Cst.CustomerID = Ord.CustomerID";

      SqlTableRef st = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "St");
      SqlTableRef cst = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"], "Cst");
      SqlTableRef ord = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "Ord");
      SqlSelect select = SqlDml.Select(st);
      select.From = select.From.InnerJoin(cst, st["CustomerID"]==cst["CustomerID"]);
      select.From = select.From.InnerJoin(ord, cst["CustomerID"]==ord["CustomerID"]);
      select.Columns.AddRange(cst["CustomerID"], st["Name"], ord["ShipDate"], ord["Freight"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("")]
    public void Test025()
    {
      string nativeSql = "Not Valid Sql string "
        +"USE AdventureWorks ; "
          +"GO "
            +"SELECT RTRIM(c.FirstName) + ' ' + LTRIM(c.LastName) AS Name, "
              +"d.City "
                +"FROM Person.Contact c "
                  +
                  "INNER JOIN HumanResources.Employee e ON c.ContactID = e.ContactID "
                    +"INNER JOIN (SELECT AddressID, City FROM Person.Address) AS d "
                      +"ON e.AddressID = d.AddressID "
                        +"ORDER BY c.LastName, c.FirstName ;";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Contact"], "c");
      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["HumanResources"].Tables["Employee"], "e");
      SqlTableRef address = SqlDml.TableRef(Catalog.Schemas["Person"].Tables["Address"]);
      SqlSelect subSelect = SqlDml.Select(address);
      subSelect.Columns.AddRange(address["AddressID"], address["City"]);
      SqlQueryRef d = SqlDml.QueryRef(subSelect, "d");
      SqlSelect select = SqlDml.Select(c);
      select.From = select.From.InnerJoin(e, c["ContactID"]==e["ContactID"]);
      select.From = select.From.InnerJoin(d, e["AddressID"]==d["AddressID"]);
      select.Columns.Add(
        SqlDml.Trim(c["FirstName"], SqlTrimType.Trailing)+' '+
          SqlDml.Trim(c["LastName"], SqlTrimType.Leading), "Name");
      select.Columns.Add(d["City"]);
      select.OrderBy.Add(c["LastName"]);
      select.OrderBy.Add(c["FirstName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("")]
    public void Test026()
    {
      //      string nativeSql = "SELECT @MyIntVariable "
      //                                   +"SELECT @@VERSION "
      //                                   +"SELECT DB_ID('AdventureWorks')";
      //
      //      SqlVariable myIntVariable = Sql.Variable("MyIntVariable");
      //      SqlSelect select1 = Sql.Select();
      //      select1.Columns.Add(myIntVariable);
      //      SqlSelect select2 = Sql.Select();
      //      select2.Columns.Add(Sql.VariableRef("@VERSION"));
      //      SqlSelect select3 = Sql.Select();
      //      select3.Columns.Add(Sql.FunctionCall("DB_ID", "AdventureWorks"));
      //
      //      Console.Write(driver.Translate(select1));
      //      Console.WriteLine();
      //      Console.Write(driver.Translate(select2));
      //      Console.WriteLine();
      //      Console.Write(driver.Translate(select3));
    }

    [Test]
    public void Test027()
    {
      string nativeSql = "SELECT c.CustomerID, s.Name "
        +"FROM Sales_Customer AS c "
          +"JOIN Sales_Store AS s "
            +"ON c.CustomerID = s.CustomerID";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"], "c");
      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "s");
      SqlSelect select = SqlDml.Select(c.InnerJoin(s, c["CustomerID"]==s["CustomerID"]));
      select.Columns.AddRange(c["CustomerID"], s["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test028()
    {
      string nativeSql = "SELECT c.CustomerID, s.Name "
        +"FROM Sales_Customer c "
          +"JOIN Sales_Store s "
            +"ON s.CustomerID = c.CustomerID "
              +"WHERE c.TerritoryID = 1";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"], "c");
      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "s");
      SqlSelect select = SqlDml.Select(c.InnerJoin(s, c["CustomerID"]==s["CustomerID"]));
      select.Columns.AddRange(c["CustomerID"], s["Name"]);
      select.Where = c["TerritoryID"]==1;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Too slow.")]
    public void Test029()
    {
      string nativeSql = "SELECT OrdD1.SalesOrderID AS OrderID, "
        +"SUM(OrdD1.OrderQty) AS \"Units Sold\", "
          +"SUM(OrdD1.UnitPrice * OrdD1.OrderQty) AS Revenue "
            +"FROM Sales_SalesOrderDetail AS OrdD1 "
              +"WHERE OrdD1.SalesOrderID in (SELECT OrdD2.SalesOrderID "
                +"FROM Sales_SalesOrderDetail AS OrdD2 "
                  +"WHERE OrdD2.UnitPrice > 100) "
                    +"GROUP BY OrdD1.SalesOrderID "
                      +"HAVING SUM(OrdD1.OrderQty) > 100";

      SqlTableRef ordD1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "OrdD1");
      SqlTableRef ordD2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "OrdD2");
      SqlSelect subSelect = SqlDml.Select(ordD2);
      subSelect.Columns.Add(ordD2["SalesOrderID"]);
      subSelect.Where = ordD2["SalesOrderID"]>100;
      SqlSelect select = SqlDml.Select(ordD1);
      select.Columns.Add(ordD1["SalesOrderID"], "OrderID");
      select.Columns.Add(SqlDml.Sum(ordD1["OrderQty"]), "Units Sold");
      select.Columns.Add(SqlDml.Sum(ordD1["UnitPrice"]*ordD1["OrderQty"]), "Revenue");
      select.Where = SqlDml.In(ordD1["SalesOrderID"], subSelect);
      select.GroupBy.Add(ordD1["SalesOrderID"]);
      select.Having = SqlDml.Sum(ordD1["OrderQty"])>100;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test030()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE Class = 'H' "
            +"ORDER BY ProductID";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = product["Class"]=='H';
      select.OrderBy.Add(product["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test031()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ListPrice BETWEEN 100 and 500 "
            +"ORDER BY ListPrice";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = SqlDml.Between(product["ListPrice"], 100, 500);
      select.OrderBy.Add(product["ListPrice"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test032()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE Color IN ('Multi', 'Silver') "
            +"ORDER BY ProductID";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = SqlDml.In(product["Color"], SqlDml.Row("Multi", "Silver"));
      select.OrderBy.Add(product["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test033()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE Name LIKE 'Ch%' "
            +"ORDER BY ProductID";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = SqlDml.Like(product["Name"], "Ch%");
      select.OrderBy.Add(product["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test034()
    {
      string nativeSql = "SELECT s.Name "
        +"FROM Sales_Customer c "
          +"JOIN Sales_Store s "
            +"ON c.CustomerID = S.CustomerID "
              +"WHERE s.SalesPersonID IS NOT NULL "
                +"ORDER BY s.Name";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"], "c");
      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "s");
      SqlSelect select = SqlDml.Select(c.InnerJoin(s, c["CustomerID"]==s["CustomerID"]));
      select.Columns.Add(s["Name"]);
      select.Where = SqlDml.IsNotNull(s["SalesPersonID"]);
      select.OrderBy.Add(s["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test035()
    {
      string nativeSql = "SELECT OrdD1.SalesOrderID, OrdD1.ProductID "
        +"FROM Sales_SalesOrderDetail OrdD1 "
          +"WHERE OrdD1.OrderQty > ALL "
            +"(SELECT OrdD2.OrderQty "
              +"       FROM Sales_SalesOrderDetail OrdD2 JOIN Production_Product Prd "
                +"ON OrdD2.ProductID = Prd.ProductID "
                  +"WHERE Prd.Class = 'H')";

      SqlTableRef ordD1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "OrdD1");
      SqlTableRef ordD2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "OrdD2");
      SqlTableRef prd = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "Prd");
      SqlSelect subSelect = SqlDml.Select(ordD2.InnerJoin(prd, ordD2["ProductID"]==prd["ProductID"]));
      subSelect.Columns.Add(ordD2["OrderQty"]);
      subSelect.Where = prd["Class"]=='H';
      SqlSelect select = SqlDml.Select(ordD1);
      select.Columns.AddRange(ordD1["SalesOrderID"], ordD1["ProductID"]);
      select.Where = ordD1["OrderQty"]>SqlDml.All(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test036()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ListPrice < 500 "
            +"OR (Class = 'L' AND ProductLine = 'S')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = product["ListPrice"]<500 ||
        (product["Class"]=='L' && product["ProductLine"]=='S');

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test037()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ListPrice > 50.00";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = product["ListPrice"]>50;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test038()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ListPrice BETWEEN 15 AND 25";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = SqlDml.Between(product["ListPrice"], 15, 25);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test039()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ListPrice = 15 OR ListPrice = 25";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = product["ListPrice"]==15 || product["ListPrice"]==25;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test040()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ListPrice > 15 AND ListPrice < 25";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = product["ListPrice"]>15 && product["ListPrice"]<25;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test041()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ListPrice NOT BETWEEN 15 AND 25";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = !SqlDml.Between(product["ListPrice"], 15, 25);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test042()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ProductSubcategoryID = 12 OR ProductSubcategoryID = 14 "
            +"OR ProductSubcategoryID = 16";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = product["ProductSubcategoryID"]==12 || product["ProductSubcategoryID"]==14 ||
        product["ProductSubcategoryID"]==16;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test043()
    {
      string nativeSql = "SELECT ProductID, Name "
        +"FROM Production_Product "
          +"WHERE ProductSubCategoryID IN (12, 14, 16)";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"]);
      select.Where = SqlDml.In(product["ProductSubcategoryID"], SqlDml.Row(12, 14, 16));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test044()
    {
      string nativeSql = "SELECT DISTINCT Name "
        +"FROM Production_Product "
          +"WHERE ProductModelID IN "
            +"(SELECT ProductModelID "
              +"FROM Production_ProductModel "
                +"WHERE Name = 'Long-sleeve logo jersey');";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productModel = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductModel"]);
      SqlSelect subSelect = SqlDml.Select(productModel);
      subSelect.Columns.Add(productModel["ProductModelID"]);
      subSelect.Where = productModel["Name"]=="Long-sleeve logo jersey";
      SqlSelect select = SqlDml.Select(product);
      select.Distinct = true;
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.In(product["ProductModelID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test045()
    {
      string nativeSql = "SELECT DISTINCT Name "
        +"FROM Production_Product "
          +"WHERE ProductModelID NOT IN "
            +"(SELECT ProductModelID "
              +"FROM Production_ProductModel "
                +"WHERE Name = 'Long-sleeve logo jersey');";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productModel = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductModel"]);
      SqlSelect subSelect = SqlDml.Select(productModel);
      subSelect.Columns.Add(productModel["ProductModelID"]);
      subSelect.Where = productModel["Name"]=="Long-sleeve logo jersey";
      SqlSelect select = SqlDml.Select(product);
      select.Distinct = true;
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.NotIn(product["ProductModelID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test046()
    {
      string nativeSql = "SELECT Phone "
        +"FROM Person_Contact "
          +"WHERE Phone LIKE '415%'";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.Add(contact["Phone"]);
      select.Where = SqlDml.Like(contact["Phone"], "415%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test047()
    {
      string nativeSql = "SELECT Phone "
        +"FROM Person_Contact "
          +"WHERE Phone NOT LIKE '415%'";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.Add(contact["Phone"]);
      select.Where = !SqlDml.Like(contact["Phone"], "415%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test048()
    {
      string nativeSql = "SELECT Phone "
        +"FROM Person_Contact "
          +"WHERE Phone LIKE '415%' and Phone IS NOT NULL";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.Add(contact["Phone"]);
      select.Where = SqlDml.Like(contact["Phone"], "415%") && SqlDml.IsNotNull(contact["Phone"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test049()
    {
      #region Like
      string nativeSql = "SELECT Phone "
        +"FROM Person_Contact "
          +"WHERE Phone LIKE '%5/%%' ESCAPE '/'";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.Add(contact["Phone"]);
      select.Where = SqlDml.Like(contact["Phone"], "%5/%%", '/');

      #endregion
      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test050()
    {
      string nativeSql = "SELECT ProductID, Name, Color "
        +"FROM Production_Product "
          +"WHERE Color IS NULL";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["Name"], product["Color"]);
      select.Where = SqlDml.IsNull(product["Color"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test051()
    {
      string nativeSql = "SELECT CustomerID, AccountNumber, TerritoryID "
        +"FROM Sales_Customer "
          +"WHERE TerritoryID IN (1, 2, 3) "
            +"OR TerritoryID IS NULL";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.AddRange(
        customer["CustomerID"], customer["AccountNumber"], customer["TerritoryID"]);
      select.Where = SqlDml.In(customer["TerritoryID"], SqlDml.Row(1, 2, 3)) ||
        SqlDml.IsNull(customer["TerritoryID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test052()
    {
      string nativeSql = "SELECT CustomerID, AccountNumber, TerritoryID "
        +"FROM Sales_Customer "
          +"WHERE TerritoryID = NULL";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.AddRange(
        customer["CustomerID"], customer["AccountNumber"], customer["TerritoryID"]);
      select.Where = customer["TerritoryID"]==SqlDml.Null;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test053()
    {
      string nativeSql = "SELECT CustomerID, Name "
        +"FROM Sales_Store "
          +"WHERE Cast(CustomerID AS NVarChar) LIKE '1%' AND Name LIKE 'Bicycle%'";

      SqlTableRef store = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"]);
      SqlSelect select = SqlDml.Select(store);
      select.Columns.AddRange(store["CustomerID"], store["Name"]);
      select.Where = SqlDml.Like(SqlDml.Cast(store["CustomerID"], SqlType.VarChar), "1%") && SqlDml.Like(store["Name"], "Bicycle%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test054()
    {
      string nativeSql = "SELECT CustomerID, Name "
        +"FROM Sales_Store "
          +"WHERE Cast(CustomerID AS NVarChar) Like '1%' OR Name LIKE 'Bicycle%'";

      SqlTableRef store = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"]);
      SqlSelect select = SqlDml.Select(store);
      select.Columns.AddRange(store["CustomerID"], store["Name"]);
      select.Where = SqlDml.Like(SqlDml.Cast(store["CustomerID"], SqlType.VarChar), "1%") || SqlDml.Like(store["Name"], "Bicycle%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test055()
    {
      string nativeSql = "SELECT ProductID, ProductModelID "
        +"FROM Production_Product "
          +"WHERE ProductModelID = 20 OR ProductModelID = 21 "
            +"AND Color = 'Red'";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["ProductModelID"]);
      select.Where = product["ProductModelID"]==20 ||
        product["ProductModelID"]==21 && product["Color"]=="RED";

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test056()
    {
      string nativeSql = "SELECT ProductID, ProductModelID "
        +"FROM Production_Product "
          +"WHERE (ProductModelID = 20 OR ProductModelID = 21) "
            +"AND Color = 'Red'";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["ProductModelID"]);
      select.Where = (product["ProductModelID"]==20 || product["ProductModelID"]==21) &&
        product["Color"]=="RED";

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test057()
    {
      string nativeSql = "SELECT ProductID, ProductModelID "
        +"FROM Production_Product "
          +"WHERE ProductModelID = 20 OR (ProductModelID = 21 "
            +"AND Color = 'Red')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["ProductModelID"]);
      select.Where = product["ProductModelID"]==20 ||
        (product["ProductModelID"]==21 && product["Color"]=="RED");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test058()
    {
      string nativeSql = "SELECT SalesOrderID, SUM(LineTotal) AS SubTotal "
        +"FROM Sales_SalesOrderDetail sod "
          +"GROUP BY SalesOrderID "
            +"ORDER BY SalesOrderID ;";

      SqlTableRef sod = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "sod");
      SqlSelect select = SqlDml.Select(sod);
      select.Columns.Add(sod["SalesOrderID"]);
      select.Columns.Add(SqlDml.Sum(sod["LineTotal"]), "SubTotal");
      select.GroupBy.Add(sod["SalesOrderID"]);
      select.OrderBy.Add(sod["SalesOrderID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test059()
    {
      string nativeSql = "SELECT DATEPART(yy, HireDate) AS Year, "
        +"COUNT(*) AS NumberOfHires "
          +"FROM HumanResources_Employee "
            +"GROUP BY DATEPART(yy, HireDate)";

      SqlTableRef employee = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);
      SqlSelect select = SqlDml.Select(employee);
      select.Columns.Add(
        SqlDml.Extract(SqlDateTimePart.Year, employee["HireDate"]), "Year");
      select.Columns.Add(SqlDml.Count(), "NumberOfHires");
      select.GroupBy.Add(SqlDml.Extract(SqlDateTimePart.Year, employee["HireDate"]));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test060()
    {
      string nativeSql = ""
        +"SELECT ProductID, SpecialOfferID, AVG(UnitPrice) AS 'Average Price', "
          +"SUM(LineTotal) AS SubTotal "
            +"FROM Sales_SalesOrderDetail "
              +"GROUP BY ProductID, SpecialOfferID "
                +"ORDER BY ProductID";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(salesOrderDetail["SpecialOfferID"]);
      select.Columns.Add(SqlDml.Avg(salesOrderDetail["UnitPrice"]), "Average Price");
      select.Columns.Add(SqlDml.Sum(salesOrderDetail["LineTotal"]), "SubTotal");
      select.GroupBy.AddRange(salesOrderDetail["ProductID"], salesOrderDetail["SpecialOfferID"]);
      select.OrderBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test061()
    {
      string nativeSql = ""
        +"SELECT ProductID, SpecialOfferID, AVG(UnitPrice) AS 'Average Price', "
          +"SUM(LineTotal) AS SubTotal "
            +"FROM Sales_SalesOrderDetail "
              +"GROUP BY ProductID, SpecialOfferID "
                +"ORDER BY ProductID";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(salesOrderDetail["SpecialOfferID"]);
      select.Columns.Add(SqlDml.Avg(salesOrderDetail["UnitPrice"]), "Average Price");
      select.Columns.Add(SqlDml.Sum(salesOrderDetail["LineTotal"]), "SubTotal");
      select.GroupBy.AddRange(salesOrderDetail["ProductID"], salesOrderDetail["SpecialOfferID"]);
      select.OrderBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test062()
    {
      string nativeSql = "SELECT ProductModelID, AVG(ListPrice) AS 'Average List Price' "
        +"FROM Production_Product "
          +"WHERE ListPrice > 1000 "
            +"GROUP BY ProductModelID "
              +"ORDER BY ProductModelID ;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["ProductModelID"]);
      select.Columns.Add(SqlDml.Avg(product["ListPrice"]), "Average List Price");
      select.Where = product["ListPrice"]>1000;
      select.GroupBy.Add(product["ProductModelID"]);
      select.OrderBy.Add(product["ProductModelID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test063()
    {
      string nativeSql = ""
        +
        "SELECT ProductID, AVG(OrderQty) AS AverageQuantity, SUM(LineTotal) AS Total "
          +"FROM Sales)SalesOrderDetail "
            +"GROUP BY ProductID "
              +"HAVING SUM(LineTotal) > 1000000.00 "
                +"AND AVG(OrderQty) < 3 ;";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(SqlDml.Avg(salesOrderDetail["OrderQty"]), "AverageQuantity");
      select.Columns.Add(SqlDml.Sum(salesOrderDetail["LineTotal"]), "Total");
      select.Having = SqlDml.Sum(salesOrderDetail["LineTotal"])>1000000 &&
        SqlDml.Avg(salesOrderDetail["OrderQty"])<3;
      select.GroupBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test064()
    {
      string nativeSql = "SELECT ProductID, SUM(LineTotal) AS Total "
        +"FROM Sales_SalesOrderDetail "
          +"GROUP BY ProductID "
            +"HAVING SUM(LineTotal) > 2000000.00 ;";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(SqlDml.Sum(salesOrderDetail["LineTotal"]), "Total");
      select.Having = SqlDml.Sum(salesOrderDetail["LineTotal"])>2000000;
      select.GroupBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test065()
    {
      string nativeSql = "SELECT ProductID, SUM(LineTotal) AS Total "
        +"FROM Sales_SalesOrderDetail "
          +"GROUP BY ProductID "
            +"HAVING COUNT(*) > 1500 ;";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(SqlDml.Sum(salesOrderDetail["LineTotal"]), "Total");
      select.Having = SqlDml.Count()>1500;
      select.GroupBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test066()
    {
      string nativeSql = "SELECT ProductID, AVG(OrderQty) "
        +"FROM Sales_SalesOrderDetail "
          +"GROUP BY ProductID "
            +"HAVING AVG(OrderQty) > 5 "
              +"ORDER BY ProductID ;";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(SqlDml.Avg(salesOrderDetail["OrderQty"]));
      select.GroupBy.Add(salesOrderDetail["ProductID"]);
      select.Having = SqlDml.Avg(salesOrderDetail["OrderQty"])>5;
      select.OrderBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test067()
    {
      string nativeSql = "SELECT pm.Name, AVG(ListPrice) AS 'Average List Price' "
        +"FROM Production_Product AS p "
          +"JOIN Production_ProductModel AS pm "
            +"ON p.ProductModelID = pm.ProductModelID "
              +"GROUP BY pm.Name "
                +"HAVING pm.Name LIKE 'Mountain%' "
                  +"ORDER BY pm.Name ;";

      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef pm = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductModel"], "pm");
      SqlSelect select = SqlDml.Select(p.InnerJoin(pm, p["ProductModelID"]==pm["ProductModelID"]));
      select.Columns.Add(pm["Name"]);
      select.Columns.Add(SqlDml.Avg(p["ListPrice"]), "Average List Price");
      select.GroupBy.Add(pm["Name"]);
      select.Having = SqlDml.Like(pm["Name"], "Mountain%");
      select.OrderBy.Add(pm["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test068()
    {
      string nativeSql = "SELECT ProductID, AVG(UnitPrice) AS 'Average Price' "
        +"FROM Sales_SalesOrderDetail "
          +"WHERE OrderQty > 10 "
            +"GROUP BY ProductID "
              +"ORDER BY ProductID ;";

      SqlTableRef salesOrderDetail = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"]);
      SqlSelect select = SqlDml.Select(salesOrderDetail);
      select.Columns.Add(salesOrderDetail["ProductID"]);
      select.Columns.Add(SqlDml.Avg(salesOrderDetail["UnitPrice"]), "Average Price");
      select.Where = salesOrderDetail["OrderQty"]>10;
      select.GroupBy.Add(salesOrderDetail["ProductID"]);
      select.OrderBy.Add(salesOrderDetail["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping.")]
    public void Test069()
    {
      string nativeSql = "SELECT Color, AVG (ListPrice) AS 'average list price' "
        +"FROM Production_Product "
          +"WHERE Color IS NOT NULL "
            +"GROUP BY Color "
              +"ORDER BY Color";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Color"]);
      select.Columns.Add(SqlDml.Avg(product["ListPrice"]), "average list price");
      select.Where = SqlDml.IsNotNull(product["Color"]);
      select.GroupBy.Add(product["Color"]);
      select.OrderBy.Add(product["Color"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test070()
    {
      string nativeSql = "SELECT ProductID, ProductSubcategoryID, ListPrice "
        +"FROM Production_Product "
          +"ORDER BY ProductSubcategoryID DESC, ListPrice";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductID"], product["ProductSubcategoryID"], product["ListPrice"]);
      select.OrderBy.Add(product["ProductSubcategoryID"], false);
      select.OrderBy.Add(product["ListPrice"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly collate not supported.")]
    public void Test071()
    {
      string nativeSql = "SELECT LastName FROM Person_Contact "
        +"ORDER BY LastName "
          +"COLLATE Traditional_Spanish_ci_ai ASC";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.Add(contact["LastName"]);
      select.OrderBy.Add(
        SqlDml.Collate(contact["LastName"], Catalog.Schemas["dbo"].Collations["Traditional_Spanish_CI_AI"]));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("VistaDB problem with average and grouping. Possibly 'ORDER BY [Alias]' not supported.")]
    public void Test072()
    {
      string nativeSql = "SELECT Color, AVG (ListPrice) AS 'average list price' "
        +"FROM Production_Product "
          +"GROUP BY Color "
            +"ORDER BY 'average list price'";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Color"]);
      select.Columns.Add(SqlDml.Avg(product["ListPrice"]), "average list price");
      select.GroupBy.Add(product["Color"]);
      select.OrderBy.Add(select.Columns["average list price"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test073()
    {
      string nativeSql = "SELECT Ord.SalesOrderID, Ord.OrderDate, "
        +"(SELECT MAX(OrdDet.UnitPrice) "
          +"FROM Sales_SalesOrderDetail AS OrdDet "
            +"     WHERE Ord.SalesOrderID = OrdDet.SalesOrderID) AS MaxUnitPrice "
              +"FROM Sales_SalesOrderHeader AS Ord";

      SqlTableRef ordDet = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "OrdDet");
      SqlTableRef ord = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "Ord");
      SqlSelect subSelect = SqlDml.Select(ordDet);
      subSelect.Columns.Add(SqlDml.Max(ordDet["UnitPrice"]));
      subSelect.Where = ord["SalesOrderID"]==ordDet["SalesOrderID"];
      SqlSelect select = SqlDml.Select(ord);
      select.Columns.AddRange(ord["SalesOrderID"], ord["OrderDate"]);
      select.Columns.Add(subSelect, "MaxUnitPrice");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test074()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ListPrice = "
            +"(SELECT ListPrice "
              +"FROM Production_Product "
                +"WHERE Name = 'Chainring Bolts' )";

      SqlTableRef product1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef product2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect subSelect = SqlDml.Select(product2);
      subSelect.Columns.Add(product2["ListPrice"]);
      subSelect.Where = product2["Name"]=="Chainring Bolts";
      SqlSelect select = SqlDml.Select(product1);
      select.Columns.AddRange(product1["Name"]);
      select.Where = product1["ListPrice"]==subSelect;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test075()
    {
      string nativeSql = "SELECT Prd1.Name "
        +"FROM Production_Product AS Prd1 "
          +"JOIN Production_Product AS Prd2 "
            +"ON (Prd1.ListPrice = Prd2.ListPrice) "
              +"WHERE Prd2.Name = 'Chainring Bolts'";

      SqlTableRef prd1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "Prd1");
      SqlTableRef prd2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "Prd2");
      SqlSelect select = SqlDml.Select(prd1.InnerJoin(prd2, prd1["ListPrice"]==prd2["ListPrice"]));
      select.Columns.Add(prd1["Name"]);
      select.Where = prd2["Name"]=="Chainring Bolts";

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test076()
    {
      string nativeSql = "SELECT Name "
        +"FROM Sales_Store "
          +"WHERE Sales_Store.CustomerID NOT IN "
            +"(SELECT Sales_Customer.CustomerID "
              +"FROM Sales_Customer "
                +"WHERE TerritoryID = 5)";

      SqlTableRef store = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"]);
      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlSelect subSelect = SqlDml.Select(customer);
      subSelect.Columns.Add(customer["CustomerID"]);
      subSelect.Where = customer["TerritoryID"]==5;
      SqlSelect select = SqlDml.Select(store);
      select.Columns.Add(store["Name"]);
      select.Where = SqlDml.NotIn(store["CustomerID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test077()
    {
      string nativeSql = "SELECT EmployeeID, ManagerID "
        +"FROM HumanResources_Employee "
          +"WHERE ManagerID IN "
            +"(SELECT ManagerID "
              +"FROM HumanResources_Employee "
                +"WHERE EmployeeID = 12)";

      SqlTableRef employee1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);
      SqlTableRef employee2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);
      SqlSelect subSelect = SqlDml.Select(employee2);
      subSelect.Columns.Add(employee2["ManagerID"]);
      subSelect.Where = employee2["EmployeeID"]==12;
      SqlSelect select = SqlDml.Select(employee1);
      select.Columns.AddRange(employee1["EmployeeID"], employee1["ManagerID"]);
      select.Where = SqlDml.In(employee1["ManagerID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test078()
    {
      string nativeSql = "SELECT e1.EmployeeID, e1.ManagerID "
        +"FROM HumanResources_Employee AS e1 "
          +"INNER JOIN HumanResources_Employee AS e2 "
            +"ON e1.ManagerID = e2.ManagerID "
              +"AND e2.EmployeeID = 12";

      SqlTableRef e1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e1");
      SqlTableRef e2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e2");
      SqlSelect select =
        SqlDml.Select(e1.InnerJoin(e2, e1["ManagerID"]==e2["ManagerID"] && e2["EmployeeID"]==12));
      select.Columns.AddRange(e1["EmployeeID"], e1["ManagerID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test079()
    {
      string nativeSql = "SELECT e1.EmployeeID, e1.ManagerID "
        +"FROM HumanResources_Employee AS e1 "
          +"WHERE e1.ManagerID IN "
            +"(SELECT e2.ManagerID "
              +"FROM HumanResources_Employee AS e2 "
                +"WHERE e2.EmployeeID = 12)";

      SqlTableRef e1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e1");
      SqlTableRef e2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e2");
      SqlSelect subSelect = SqlDml.Select(e2);
      subSelect.Columns.Add(e2["ManagerID"]);
      subSelect.Where = e2["EmployeeID"]==12;
      SqlSelect select = SqlDml.Select(e1);
      select.Columns.AddRange(e1["EmployeeID"], e1["ManagerID"]);
      select.Where = SqlDml.In(e1["ManagerID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test080()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ProductSubcategoryID IN "
            +"(SELECT ProductSubcategoryID "
              +"FROM Production_ProductSubcategory "
                +"WHERE Name = 'Wheels')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productSubcategory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"]);
      SqlSelect subSelect = SqlDml.Select(productSubcategory);
      subSelect.Columns.Add(productSubcategory["ProductSubcategoryID"]);
      subSelect.Where = productSubcategory["Name"]=="Wheels";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.In(product["ProductSubcategoryID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test081()
    {
      string nativeSql = "SELECT p.Name, s.Name "
        +"FROM Production_Product p "
          +"INNER JOIN Production_ProductSubcategory s "
            +"ON p.ProductSubcategoryID = s.ProductSubcategoryID "
              +"AND s.Name = 'Wheels'";

      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"], "s");
      SqlSelect select = SqlDml.Select(p.InnerJoin(s, p["ProductSubcategoryID"]==s["ProductSubcategoryID"] && s["Name"]=="Wheels"));
      select.Columns.AddRange(p["Name"], s["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test082()
    {
      string nativeSql = "SELECT Name "
        +"FROM Purchasing_Vendor "
          +"WHERE CreditRating = 1 "
            +"AND VendorID IN "
              +"(SELECT VendorID "
                +"FROM Purchasing_ProductVendor "
                  +"WHERE MinOrderQty >= 20 "
                    +"AND AverageLeadTime < 16)";

      SqlTableRef vendor = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_Vendor"]);
      SqlTableRef productVendor = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"]);
      SqlSelect subSelect = SqlDml.Select(productVendor);
      subSelect.Columns.Add(productVendor["VendorID"]);
      subSelect.Where = productVendor["MinOrderQty"]>=20 && productVendor["AverageLeadTime"]<16;
      SqlSelect select = SqlDml.Select(vendor);
      select.Columns.Add(vendor["Name"]);
      select.Where = vendor["CreditRating"]==1 && SqlDml.In(vendor["VendorID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test083()
    {
      string nativeSql = "SELECT DISTINCT Name "
        +"FROM Purchasing_Vendor v "
          +"INNER JOIN Purchasing_ProductVendor p "
            +"ON v.VendorID = p.VendorID "
              +"WHERE CreditRating = 1 "
                +"AND MinOrderQty >= 20 "
                  +"AND OnOrderQty IS NULL";

      SqlTableRef v = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_Vendor"], "v");
      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "p");
      SqlSelect select = SqlDml.Select(v.InnerJoin(p, v["VendorID"]==p["VendorID"]));
      select.Distinct = true;
      select.Columns.Add(v["Name"]);
      select.Where = v["CreditRating"]==1 && p["MinOrderQty"]>=20 && SqlDml.IsNull(p["OnOrderQty"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test084()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ProductSubcategoryID NOT IN "
            +"(SELECT ProductSubcategoryID "
              +"FROM Production_Product "
                +"WHERE Name = 'Mountain Bikes' "
                  +"OR Name = 'Road Bikes' "
                    +"OR Name = 'Touring Bikes')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef product2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect subSelect = SqlDml.Select(product2);
      subSelect.Columns.Add(product2["ProductSubcategoryID"]);
      subSelect.Where = product2["Name"]=="Mountain Bikes" || product2["Name"]=="Road Bikes" ||
        product2["Name"]=="Touring Bikes";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.NotIn(product["ProductSubcategoryID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test085()
    {
      string nativeSql = "UPDATE Production_Product "
        +"SET ListPrice = ListPrice * 2 "
          +"WHERE ProductID IN "
            +"(SELECT ProductID "
              +"FROM Purchasing_ProductVendor "
                +"WHERE VendorID = 51);";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productVendor = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"]);
      SqlSelect subSelect = SqlDml.Select(productVendor);
      subSelect.Columns.Add(productVendor["ProductID"]);
      subSelect.Where = productVendor["VendorID"]==51;
      SqlUpdate update = SqlDml.Update(product);
      update.Values[product["ListPrice"]] = product["ListPrice"]*2;
      update.Where = SqlDml.In(product["ProductID"], subSelect);

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test086()
    {
      string nativeSql = "UPDATE Production_Product "
        +"SET ListPrice = ListPrice * 2 "
          +"FROM Production_Product AS p "
            +"INNER JOIN Purchasing_ProductVendor AS pv "
              +"ON p.ProductID = pv.ProductID AND pv.VendorID = 51;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef pv = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv");

      SqlUpdate update = SqlDml.Update(product);
      update.Values[product["ListPrice"]] = p["ListPrice"]*2;
      update.From = p.InnerJoin(pv, p["ProductID"]==pv["ProductID"] && pv["VendorID"]==51);

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test087()
    {
      string nativeSql = "SELECT CustomerID "
        +"FROM Sales_Customer "
          +"WHERE TerritoryID = "
            +"(SELECT TerritoryID "
              +"FROM Sales_SalesPerson "
                +"WHERE SalesPersonID = 276)";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlSelect subSelect = SqlDml.Select(salesPerson);
      subSelect.Columns.Add(salesPerson["TerritoryID"]);
      subSelect.Where = salesPerson["SalesPersonID"]==276;
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerID"]);
      select.Where = customer["TerritoryID"]==subSelect;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test088()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ListPrice > "
            +"(SELECT AVG (ListPrice) "
              +"FROM Production_Product)";

      SqlTableRef product1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef product2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect subSelect = SqlDml.Select(product2);
      subSelect.Columns.Add(SqlDml.Avg(product2["ListPrice"]));
      SqlSelect select = SqlDml.Select(product1);
      select.Columns.Add(product1["Name"]);
      select.Where = product1["ListPrice"]>subSelect;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test089()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ListPrice > "
            +"(SELECT MIN (ListPrice) "
              +"FROM Production_Product "
                +"GROUP BY ProductSubcategoryID "
                  +"HAVING ProductSubcategoryID = 14)";

      SqlTableRef product1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef product2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect subSelect = SqlDml.Select(product2);
      subSelect.Columns.Add(SqlDml.Min(product2["ListPrice"]));
      subSelect.GroupBy.Add(product2["ProductSubcategoryID"]);
      subSelect.Having = product2["ProductSubcategoryID"]==14;
      SqlSelect select = SqlDml.Select(product1);
      select.Columns.Add(product1["Name"]);
      select.Where = product1["ListPrice"]>subSelect;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test090()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ListPrice >= ANY "
            +"(SELECT MAX (ListPrice) "
              +"FROM Production_Product "
                +"GROUP BY ProductSubcategoryID)";

      SqlTableRef product1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef product2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect subSelect = SqlDml.Select(product2);
      subSelect.Columns.Add(SqlDml.Max(product2["ListPrice"]));
      subSelect.GroupBy.Add(product2["ProductSubcategoryID"]);
      SqlSelect select = SqlDml.Select(product1);
      select.Columns.Add(product1["Name"]);
      select.Where = product1["ListPrice"]>=SqlDml.Any(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test091()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ProductSubcategoryID=ANY "
            +"(SELECT ProductSubcategoryID "
              +"FROM Production_ProductSubcategory "
                +"WHERE Name = 'Wheels')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productSubcategory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"]);
      SqlSelect subSelect = SqlDml.Select(productSubcategory);
      subSelect.Columns.Add(productSubcategory["ProductSubcategoryID"]);
      subSelect.Where = productSubcategory["Name"]=="Wheels";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = product["ProductSubcategoryID"]==SqlDml.Any(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test092()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ProductSubcategoryID IN "
            +"(SELECT ProductSubcategoryID "
              +"FROM Production_ProductSubcategory "
                +"WHERE Name = 'Wheels')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productSubcategory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"]);
      SqlSelect subSelect = SqlDml.Select(productSubcategory);
      subSelect.Columns.Add(productSubcategory["ProductSubcategoryID"]);
      subSelect.Where = productSubcategory["Name"]=="Wheels";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.In(product["ProductSubcategoryID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test093()
    {
      string nativeSql = "SELECT CustomerID "
        +"FROM Sales_Customer "
          +"WHERE TerritoryID <> ANY "
            +"(SELECT TerritoryID "
              +"FROM Sales_SalesPerson)";

      SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Customer"]);
      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlSelect subSelect = SqlDml.Select(salesPerson);
      subSelect.Columns.Add(salesPerson["TerritoryID"]);
      SqlSelect select = SqlDml.Select(customer);
      select.Columns.Add(customer["CustomerID"]);
      select.Where = customer["TerritoryID"]!=SqlDml.Any(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test094()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE EXISTS "
            +"(SELECT * "
              +"FROM Production_ProductSubcategory "
                +"WHERE ProductSubcategoryID = "
                  +"Production_Product.ProductSubcategoryID "
                    +"AND Name = 'Wheels')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productSubcategory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"]);
      SqlSelect subSelect = SqlDml.Select(productSubcategory);
      subSelect.Columns.Add(productSubcategory.Asterisk);
      subSelect.Where = productSubcategory["ProductSubcategoryID"]==product["ProductSubcategoryID"];
      subSelect.Where = subSelect.Where && productSubcategory["Name"]=="Wheels";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.Exists(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test095()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE ProductSubcategoryID IN "
            +"(SELECT ProductSubcategoryID "
              +"FROM Production_ProductSubcategory "
                +"WHERE Name = 'Wheels')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productSubcategory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"]);
      SqlSelect subSelect = SqlDml.Select(productSubcategory);
      subSelect.Columns.Add(productSubcategory["ProductSubcategoryID"]);
      subSelect.Where = productSubcategory["Name"]=="Wheels";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = SqlDml.In(product["ProductSubcategoryID"], subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test096()
    {
      string nativeSql = "SELECT Name "
        +"FROM Production_Product "
          +"WHERE NOT EXISTS "
            +"(SELECT * "
              +"FROM Production_ProductSubcategory "
                +"WHERE ProductSubcategoryID = "
                  +"Production_Product.ProductSubcategoryID "
                    +"AND Name = 'Wheels')";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef productSubcategory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductSubcategory"]);
      SqlSelect subSelect = SqlDml.Select(productSubcategory);
      subSelect.Columns.Add(productSubcategory.Asterisk);
      subSelect.Where = productSubcategory["ProductSubcategoryID"]==product["ProductSubcategoryID"];
      subSelect.Where = subSelect.Where && productSubcategory["Name"]=="Wheels";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.Add(product["Name"]);
      select.Where = !SqlDml.Exists(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test097()
    {
      string nativeSql = "SELECT Name, ListPrice, "
        +"(SELECT AVG(ListPrice) FROM Production_Product) AS Average, "
          +"    ListPrice - (SELECT AVG(ListPrice) FROM Production_Product) "
            +"AS Difference "
              +"FROM Production_Product "
                +"WHERE ProductSubcategoryID = 1";

      SqlTableRef product1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlTableRef product2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlSelect subSelect = SqlDml.Select(product2);
      subSelect.Columns.Add(SqlDml.Avg(product2["ListPrice"]));
      SqlSelect select = SqlDml.Select(product1);
      select.Columns.AddRange(product1["Name"], product1["ListPrice"]);
      select.Columns.Add(subSelect, "Average");
      select.Columns.Add(product1["ListPrice"]-subSelect, "Difference");
      select.Where = product1["ProductSubcategoryID"]==1;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test098()
    {
      string nativeSql = "SELECT LastName, FirstName "
        +"FROM Person_Contact "
          +"WHERE ContactID IN "
            +"(SELECT ContactID "
              +"FROM HumanResources_Employee "
                +"WHERE EmployeeID IN "
                  +"(SELECT SalesPersonID "
                    +"FROM Sales_SalesPerson))";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlTableRef employee = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);
      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlSelect subSelect2 = SqlDml.Select(salesPerson);
      subSelect2.Columns.Add(salesPerson["SalesPersonID"]);
      SqlSelect subSelect1 = SqlDml.Select(employee);
      subSelect1.Columns.Add(employee["ContactID"]);
      subSelect1.Where = SqlDml.In(employee["EmployeeID"], subSelect2);
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.AddRange(contact["LastName"], contact["FirstName"]);
      select.Where = SqlDml.In(contact["ContactID"], subSelect1);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test099()
    {
      string nativeSql = "SELECT LastName, FirstName "
        +"FROM Person_Contact c "
          +"INNER JOIN HumanResources_Employee e "
            +"ON c.ContactID = e.ContactID "
              +"JOIN Sales_SalesPerson s "
                +"ON e.EmployeeID = s.SalesPersonID";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"], "s");
      SqlSelect select = SqlDml.Select(c);
      select.From = select.From.InnerJoin(e, c["ContactID"]==e["ContactID"]);
      select.From = select.From.InnerJoin(s, e["EmployeeID"]==s["SalesPersonID"]);
      select.Columns.AddRange(c["LastName"], c["FirstName"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test100()
    {
      string nativeSql = "SELECT DISTINCT c.LastName, c.FirstName "
        +"FROM Person_Contact c JOIN HumanResources_Employee e "
          +"ON e.ContactID = c.ContactID "
            +"WHERE 5000.00 IN "
              +"(SELECT Bonus "
                +"FROM Sales_SalesPerson sp "
                  +"WHERE e.EmployeeID = sp.SalesPersonID);";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef sp = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"], "sp");
      SqlSelect subSelect = SqlDml.Select(sp);
      subSelect.Columns.Add(sp["Bonus"]);
      subSelect.Where = e["EmployeeID"]==sp["SalesPersonID"];
      SqlSelect select = SqlDml.Select(c.InnerJoin(e, c["ContactID"]==e["ContactID"]));
      select.Distinct = true;
      select.Columns.AddRange(c["LastName"], c["FirstName"]);
      select.Where = SqlDml.In(5000.00, subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test101()
    {
      string nativeSql = "SELECT LastName, FirstName "
        +"FROM Person_Contact c JOIN HumanResources_Employee e "
          +"ON e.ContactID = c.ContactID "
            +"WHERE 5000 IN (5000)";

      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlSelect select = SqlDml.Select(c.InnerJoin(e, c["ContactID"]==e["ContactID"]));
      select.Columns.AddRange(c["LastName"], c["FirstName"]);
      select.Where = SqlDml.In(5000, SqlDml.Row(5000));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test102()
    {
      string nativeSql = "SELECT DISTINCT pv1.ProductID, pv1.VendorID "
        +"FROM Purchasing_ProductVendor pv1 "
          +"WHERE ProductID IN "
            +"(SELECT pv2.ProductID "
              +"FROM Purchasing_ProductVendor pv2 "
                +"WHERE pv1.VendorID <> pv2.VendorID) "
                  +"ORDER  BY pv1.VendorID";

      SqlTableRef pv1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv1");
      SqlTableRef pv2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv2");
      SqlSelect subSelect = SqlDml.Select(pv2);
      subSelect.Columns.Add(pv2["ProductID"]);
      subSelect.Where = pv1["VendorID"]!=pv2["VendorID"];
      SqlSelect select = SqlDml.Select(pv1);
      select.Distinct = true;
      select.Columns.AddRange(pv1["ProductID"], pv1["VendorID"]);
      select.Where = SqlDml.In(pv1["ProductID"], subSelect);
      select.OrderBy.Add(pv1["VendorID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test103()
    {
      string nativeSql = "SELECT DISTINCT pv1.ProductID, pv1.VendorID "
        +"FROM Purchasing_ProductVendor pv1 "
          +"INNER JOIN Purchasing_ProductVendor pv2 "
            +"ON pv1.ProductID = pv2.ProductID "
              +"AND pv1.VendorID <> pv2.VendorID "
                +"ORDER BY pv1.VendorID";

      SqlTableRef pv1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv1");
      SqlTableRef pv2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv2");
      SqlSelect select = SqlDml.Select(pv1.InnerJoin(pv2, pv1["ProductID"]==pv2["ProductID"] && pv1["VendorID"]!=pv2["VendorID"]));
      select.Distinct = true;
      select.Columns.AddRange(pv1["ProductID"], pv1["VendorID"]);
      select.OrderBy.Add(pv1["VendorID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Too slow.")]
    public void Test104()
    {
      string nativeSql = "SELECT ProductID, OrderQty "
        +"FROM Sales_SalesOrderDetail s1 "
          +"WHERE s1.OrderQty < "
            +"(SELECT AVG (s2.OrderQty) "
              +"FROM Sales_SalesOrderDetail s2 "
                +"WHERE s2.ProductID = s1.ProductID)";

      SqlTableRef s1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "s1");
      SqlTableRef s2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "s2");
      SqlSelect subSelect = SqlDml.Select(s2);
      subSelect.Columns.Add(SqlDml.Avg(s2["OrderQty"]));
      subSelect.Where = s2["ProductID"]==s1["ProductID"];
      SqlSelect select = SqlDml.Select(s1);
      select.Columns.AddRange(s1["ProductID"], s1["OrderQty"]);
      select.Where = s1["OrderQty"]<subSelect;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test105()
    {
      string nativeSql = "SELECT p1.ProductSubcategoryID, p1.Name "
        +"FROM Production_Product p1 "
          +"WHERE p1.ListPrice > "
            +"(SELECT AVG (p2.ListPrice) "
              +"FROM Production_Product p2 "
                +"WHERE p1.ProductSubcategoryID = p2.ProductSubcategoryID)";

      SqlTableRef p1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p1");
      SqlTableRef p2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p2");
      SqlSelect subSelect = SqlDml.Select(p2);
      subSelect.Columns.Add(SqlDml.Avg(p2["ListPrice"]));
      subSelect.Where = p2["ProductSubcategoryID"]==p1["ProductSubcategoryID"];
      SqlSelect select = SqlDml.Select(p1);
      select.Columns.AddRange(p1["ProductSubcategoryID"], p1["Name"]);
      select.Where = p1["ListPrice"]>subSelect;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("It is necessary to alter.")]
    public void Test106()
    {
      string nativeSql = "SELECT p1.ProductModelID, MAX(p1.ListPrice) "
        +"FROM Production_Product p1 "
          +"GROUP BY p1.ProductModelID "
            +"HAVING MAX(p1.ListPrice) >= ALL "
              +"(SELECT 2 * AVG(p2.ListPrice) "
                +"FROM Production_Product p2 "
                  +"WHERE p1.ProductModelID = p2.ProductModelID) ;";

      SqlTableRef p1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p1");
      SqlTableRef p2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p2");
      SqlSelect subSelect = SqlDml.Select(p2);
      subSelect.Columns.Add(2*SqlDml.Avg(p2["ListPrice"]));
      subSelect.Where = p2["ProductModelID"]==p1["ProductModelID"];
      SqlSelect select = SqlDml.Select(p1);
      select.Columns.Add(p1["ProductModelID"]);
      select.Columns.Add(SqlDml.Max(p1["ListPrice"]));
      select.GroupBy.Add(p1["ProductModelID"]);
      select.Having = SqlDml.Max(p1["ListPrice"])>=SqlDml.All(subSelect);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test107()
    {
      string nativeSql = "SELECT ProductID, Purchasing_Vendor.VendorID, Name "
        +"FROM Purchasing_ProductVendor JOIN Purchasing_Vendor "
          +"    ON (Purchasing_ProductVendor.VendorID = Purchasing_Vendor.VendorID) "
            +"WHERE StandardPrice > 10 "
              +"AND Name LIKE 'F%'";

      SqlTableRef productVendor = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"]);
      SqlTableRef vendor = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_Vendor"]);
      SqlSelect select = SqlDml.Select(productVendor.InnerJoin(vendor, productVendor["VendorID"]==vendor["VendorID"]));
      select.Columns.AddRange(productVendor["ProductID"], vendor["VendorID"], vendor["Name"]);
      select.Where = productVendor["StandardPrice"]>10 && SqlDml.Like(vendor["Name"], "F%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test108()
    {
      string nativeSql = "SELECT pv.ProductID, v.VendorID, v.Name "
        +"FROM Purchasing_ProductVendor pv JOIN Purchasing_Vendor v "
          +"ON (pv.VendorID = v.VendorID) "
            +"WHERE StandardPrice > 10 "
              +"AND Name LIKE 'F%'";

      SqlTableRef pv = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv");
      SqlTableRef v = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_Vendor"], "v");
      SqlSelect select = SqlDml.Select(pv.InnerJoin(v, pv["VendorID"]==v["VendorID"]));
      select.Columns.AddRange(pv["ProductID"], v["VendorID"], v["Name"]);
      select.Where = pv["StandardPrice"]>10 && SqlDml.Like(v["Name"], "F%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test109()
    {
      string nativeSql = "SELECT pv.ProductID, v.VendorID, v.Name "
        +"FROM Purchasing_ProductVendor pv, Purchasing_Vendor v "
          +"WHERE pv.VendorID = v.VendorID "
            +"AND StandardPrice > 10 "
              +"AND Name LIKE 'F%'";

      SqlTableRef pv = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv");
      SqlTableRef v = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_Vendor"], "v");
      SqlSelect select = SqlDml.Select(pv.CrossJoin(v));
      select.Columns.AddRange(pv["ProductID"], v["VendorID"], v["Name"]);
      select.Where = pv["VendorID"]==v["VendorID"] && pv["StandardPrice"]>10 &&
        SqlDml.Like(v["Name"], "F%");

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test110()
    {
      string nativeSql = "SELECT e.EmployeeID "
        +"FROM HumanResources_Employee AS e "
          +"INNER JOIN Sales_SalesPerson AS s "
            +"ON e.EmployeeID = s.SalesPersonID";

      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"], "s");
      SqlSelect select = SqlDml.Select(e.InnerJoin(s, e["EmployeeID"]==s["SalesPersonID"]));
      select.Columns.Add(e["EmployeeID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test111()
    {
      string nativeSql = "SELECT * "
        +"FROM HumanResources_Employee AS e "
          +"INNER JOIN Person_Contact AS c "
            +"ON e.ContactID = c.ContactID "
              +"ORDER BY c.LastName";

      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"], "c");
      SqlSelect select = SqlDml.Select(e.InnerJoin(c, e["ContactID"]==c["ContactID"]));
      select.Columns.Add(SqlDml.Asterisk);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test112()
    {
      string nativeSql = ""
        +"SELECT DISTINCT p.ProductID, p.Name, p.ListPrice, sd.UnitPrice AS 'Selling Price' "
          +"FROM Sales_SalesOrderDetail AS sd "
            +"JOIN Production_Product AS p "
              +"    ON sd.ProductID = p.ProductID AND sd.UnitPrice < p.ListPrice "
                +"WHERE p.ProductID = 718;";

      SqlTableRef sd = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "sd");
      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlSelect select = SqlDml.Select(sd.InnerJoin(p, sd["ProductID"]==p["ProductID"] && sd["UnitPrice"]<p["ListPrice"]));
      select.Distinct = true;
      select.Columns.AddRange(p["ProductID"], p["Name"], p["ListPrice"]);
      select.Columns.Add(sd["UnitPrice"], "Selling Price");
      select.Where = p["ProductID"]==718;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test113()
    {
      string nativeSql = "SELECT DISTINCT p1.ProductSubcategoryID, p1.ListPrice "
        +"FROM Production_Product p1 "
          +"INNER JOIN Production_Product p2 "
            +"ON p1.ProductSubcategoryID = p2.ProductSubcategoryID "
              +"AND p1.ListPrice <> p2.ListPrice "
                +"WHERE p1.ListPrice < 15 AND p2.ListPrice < 15 "
                  +"ORDER BY ProductSubcategoryID;";

      SqlTableRef p1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p1");
      SqlTableRef p2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p2");
      SqlSelect select = SqlDml.Select(p1.InnerJoin(p2, p1["ProductSubcategoryID"]==p2["ProductSubcategoryID"] && p1["ListPrice"]!=p2["ListPrice"]));
      select.Distinct = true;
      select.Columns.AddRange(p1["ProductSubcategoryID"], p1["ListPrice"]);
      select.Where = p1["ListPrice"]<15 && p2["ListPrice"]<15;
      select.OrderBy.Add(p1["ProductSubcategoryID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test114()
    {
      string nativeSql = "SELECT DISTINCT p1.VendorID, p1.ProductID "
        +"FROM Purchasing_ProductVendor p1 "
          +"INNER JOIN Purchasing_ProductVendor p2 "
            +"ON p1.ProductID = p2.ProductID "
              +"WHERE p1.VendorID <> p2.VendorID "
                +"ORDER BY p1.VendorID";

      SqlTableRef p1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "p1");
      SqlTableRef p2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "p2");
      SqlSelect select = SqlDml.Select(p1.InnerJoin(p2, p1["ProductID"]==p2["ProductID"]));
      select.Distinct = true;
      select.Columns.AddRange(p1["VendorID"], p1["ProductID"]);
      select.Where = p1["VendorID"]!=p2["VendorID"];
      select.OrderBy.Add(p1["VendorID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test115()
    {
      string nativeSql = "SELECT p.Name, pr.ProductReviewID "
        +"FROM Production_Product p "
          +"LEFT OUTER JOIN Production_ProductReview pr "
            +"ON p.ProductID = pr.ProductID";

      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef pr = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_ProductReview"], "pr");
      SqlSelect select = SqlDml.Select(p.LeftOuterJoin(pr, p["ProductID"]==pr["ProductID"]));
      select.Columns.AddRange(p["Name"], pr["ProductReviewID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test116()
    {
      string nativeSql = "SELECT st.Name AS Territory, sp.SalesPersonID "
        +"FROM Sales_SalesTerritory st "
          +"RIGHT OUTER JOIN Sales_SalesPerson sp "
            +"ON st.TerritoryID = sp.TerritoryID ;";

      SqlTableRef st = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesTerritory"], "st");
      SqlTableRef sp = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"], "sp");
      SqlSelect select = SqlDml.Select(st.RightOuterJoin(sp, st["TerritoryID"]==sp["TerritoryID"]));
      select.Columns.Add(st["Name"], "Territory");
      select.Columns.Add(sp["SalesPersonID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test117()
    {
      string nativeSql = "SELECT st.Name AS Territory, sp.SalesPersonID "
        +"FROM Sales_SalesTerritory st "
          +"RIGHT OUTER JOIN Sales_SalesPerson sp "
            +"ON st.TerritoryID = sp.TerritoryID "
              +"WHERE st.SalesYTD < 2000000;";

      SqlTableRef st = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesTerritory"], "st");
      SqlTableRef sp = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"], "sp");
      SqlSelect select = SqlDml.Select(st.RightOuterJoin(sp, st["TerritoryID"]==sp["TerritoryID"]));
      select.Columns.Add(st["Name"], "Territory");
      select.Columns.Add(sp["SalesPersonID"]);
      select.Where = st["SalesYTD"]<2000000;

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("FULL [OUTER] JOIN are not supported.")]
    public void Test118()
    {
      string nativeSql = "SELECT p.Name, sod.SalesOrderID "
        +"FROM Production_Product p "
          +"FULL OUTER JOIN Sales_SalesOrderDetail sod "
            +"ON p.ProductID = sod.ProductID "
              +"WHERE p.ProductID IS NULL "
                +"OR sod.ProductID IS NULL "
                  +"ORDER BY p.Name ;";

      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef sod = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderDetail"], "sod");
      SqlSelect select = SqlDml.Select(p.FullOuterJoin(sod, p["ProductID"]==sod["ProductID"]));
      select.Columns.AddRange(p["Name"], sod["SalesOrderID"]);
      select.Where = SqlDml.IsNull(p["ProductID"]) || SqlDml.IsNull(sod["ProductID"]);
      select.OrderBy.Add(p["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test119()
    {
      string nativeSql = "SELECT e.EmployeeID, d.Name AS Department "
        +"FROM HumanResources_Employee e "
          +"CROSS JOIN HumanResources_Department d "
            +"ORDER BY e.EmployeeID, d.Name ;";

      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef d = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Department"], "d");
      SqlSelect select = SqlDml.Select(e.CrossJoin(d));
      select.Columns.Add(e["EmployeeID"]);
      select.Columns.Add(d["Name"], "Department");
      select.OrderBy.Add(e["EmployeeID"]);
      select.OrderBy.Add(d["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Invalid column")]
    public void Test120()
    {
      string nativeSql = "SELECT e.EmployeeID, d.Name AS Department "
        +"FROM HumanResources_Employee e "
          +"CROSS JOIN HumanResources_Department d "
            +"WHERE e.DepartmentID = d.DepartmentID "
              +"ORDER BY e.EmployeeID, d.Name ;";

      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef d = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Department"], "d");
      SqlSelect select = SqlDml.Select(e.CrossJoin(d));
      select.Columns.Add(e["EmployeeID"]);
      select.Columns.Add(d["Name"], "Department");
      select.Where = e["DepartmentID"]==d["DepartmentID"];
      select.OrderBy.Add(e["EmployeeID"]);
      select.OrderBy.Add(d["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Invalid column")]
    public void Test121()
    {
      string nativeSql = "SELECT e.EmployeeID, d.Name AS Department "
        +"FROM HumanResources_Employee e "
          +"INNER JOIN HumanResources_Department d "
            +"ON e.DepartmentID = d.DepartmentID "
              +"ORDER BY e.EmployeeID, d.Name ;";

      SqlTableRef e = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"], "e");
      SqlTableRef d = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Department"], "d");
      SqlSelect select = SqlDml.Select(e.InnerJoin(d, e["DepartmentID"]==d["DepartmentID"]));
      select.Columns.Add(e["EmployeeID"]);
      select.Columns.Add(d["Name"], "Department");
      select.OrderBy.Add(e["EmployeeID"]);
      select.OrderBy.Add(d["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test122()
    {
      string nativeSql = "SELECT DISTINCT pv1.ProductID, pv1.VendorID "
        +"FROM Purchasing_ProductVendor pv1 "
          +"INNER JOIN Purchasing_ProductVendor pv2 "
            +"ON pv1.ProductID = pv2.ProductID "
              +"AND pv1.VendorID <> pv2.VendorID "
                +"ORDER BY pv1.ProductID";

      SqlTableRef pv1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv1");
      SqlTableRef pv2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv2");
      SqlSelect select = SqlDml.Select(pv1.InnerJoin(pv2, pv1["ProductID"]==pv2["ProductID"] && pv1["VendorID"]!=pv2["VendorID"]));
      select.Distinct = true;
      select.Columns.AddRange(pv1["ProductID"], pv1["VendorID"]);
      select.OrderBy.Add(pv1["ProductID"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test123()
    {
      string nativeSql = "SELECT p.Name, v.Name "
        +"FROM Production_Product p "
          +"JOIN Purchasing_ProductVendor pv "
            +"ON p.ProductID = pv.ProductID "
              +"JOIN Purchasing_Vendor v "
                +"ON pv.VendorID = v.VendorID "
                  +"WHERE ProductSubcategoryID = 15 "
                    +"ORDER BY v.Name";

      SqlTableRef p = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"], "p");
      SqlTableRef pv = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_ProductVendor"], "pv");
      SqlTableRef v = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_Vendor"], "v");
      SqlSelect select = SqlDml.Select(p.InnerJoin(pv, p["ProductID"]==pv["ProductID"]).InnerJoin(v, pv["VendorID"]==v["VendorID"]));
      select.Columns.AddRange(p["Name"], v["Name"]);
      select.Where = p["ProductSubcategoryID"]==15;
      select.OrderBy.Add(v["Name"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test124()
    {
      string nativeSql = "INSERT INTO Production_UnitMeasure "
        +"VALUES ('F2', 'Square Feet', GETDATE());";

      SqlTableRef unitMeasure = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_UnitMeasure"]);
      SqlInsert insert = SqlDml.Insert(unitMeasure);
      insert.Values[unitMeasure[0]] = "F2";
      insert.Values[unitMeasure[1]] = "Square Feet";
      insert.Values[unitMeasure[2]] = SqlDml.CurrentDate();

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, insert));
    }

    [Test]
    public void Test125()
    {
      string nativeSql = "UPDATE Production_Product "
        +"SET ListPrice = ListPrice * 1.1 "
          +"WHERE ProductModelID = 37;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlUpdate update = SqlDml.Update(product);
      update.Values[product["ListPrice"]] = product["ListPrice"]*1.1;
      update.Where = product["ProductModelID"]==37;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test126()
    {
      string nativeSql = "UPDATE Person_Address "
        +"SET PostalCode = '98000' "
          +"WHERE City = 'Bothell';";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Address"]);
      SqlUpdate update = SqlDml.Update(product);
      update.Values[product["PostalCode"]] = "98000";
      update.Where = product["City"]=="Bothell";

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test127()
    {
      string nativeSql = "UPDATE Sales_SalesPerson "
        +"SET Bonus = 6000, CommissionPct = .10, SalesQuota = NULL;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlUpdate update = SqlDml.Update(product);
      update.Values[product["Bonus"]] = 6000;
      update.Values[product["CommissionPct"]] = .10;
      update.Values[product["SalesQuota"]] = SqlDml.Null;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test128()
    {
      string nativeSql = "UPDATE Production_Product "
        +"SET ListPrice = ListPrice * 2;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlUpdate update = SqlDml.Update(product);
      update.Values[product["ListPrice"]] = product["ListPrice"]*2;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test129()
    {
      string nativeSql = "UPDATE Sales_SalesPerson "
        +"SET SalesYTD = SalesYTD + "
          +"(SELECT SUM(so.SubTotal) "
            +"FROM Sales_SalesOrderHeader AS so "
              +"WHERE so.OrderDate = (SELECT MAX(OrderDate) "
                +"FROM Sales_SalesOrderHeader AS so2 "
                  +"WHERE so2.SalesPersonID = "
                    +"so.SalesPersonID) "
                      +"AND Sales_SalesPerson.SalesPersonID = so.SalesPersonID "
                        +"GROUP BY so.SalesPersonID);";

      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlTableRef so = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "so");
      SqlTableRef so2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "so2");
      SqlSelect subSelect = SqlDml.Select(so2);
      subSelect.Columns.Add(SqlDml.Max(so2["OrderDate"]));
      subSelect.Where = so2["SalesPersonID"]==so["SalesPersonID"];
      SqlSelect select = SqlDml.Select(so);
      select.Columns.Add(SqlDml.Sum(so["SubTotal"]));
      select.Where = so["OrderDate"]==subSelect && salesPerson["SalesPersonID"]==so["SalesPersonID"];
      select.GroupBy.Add(so["SalesPersonID"]);
      SqlUpdate update = SqlDml.Update(salesPerson);
      update.Values[salesPerson["SalesYTD"]] = salesPerson["SalesYTD"]+select;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test130()
    {
      string nativeSql = "UPDATE Sales_SalesReason "
        +"SET Name = 'Unknown' "
          +"WHERE Name = 'Other';";

      SqlTableRef salesReason = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesReason"]);
      SqlUpdate update = SqlDml.Update(salesReason);
      update.Values[salesReason["Name"]] = "Unknown";
      update.Where = salesReason["Name"]=="Other";

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    [Ignore]
    public void Test131()
    {
      string nativeSql = "UPDATE Sales_SalesPerson "
        +"SET SalesYTD = SalesYTD + SubTotal "
          +"FROM Sales_SalesPerson AS sp "
            +"JOIN Sales_SalesOrderHeader AS so "
              +"ON sp.SalesPersonID = so.SalesPersonID "
                +"AND so.OrderDate = (SELECT MAX(OrderDate) "
                  +"FROM Sales_SalesOrderHeader "
                    +"WHERE SalesPersonID = "
                      +"sp.SalesPersonID);";

      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlTableRef salesOrderHeader = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"]);
      SqlTableRef sp = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"], "sp");
      SqlTableRef so = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "so");
      SqlSelect subSelect = SqlDml.Select(salesOrderHeader);
      subSelect.Columns.Add(SqlDml.Max(salesOrderHeader["OrderDate"]));
      subSelect.Where = salesOrderHeader["SalesPersonID"]==sp["SalesPersonID"];
      SqlSelect select = SqlDml.Select(sp.InnerJoin(so, sp["SalesPersonID"]==so["SalesPersonID"] && so["OrderDate"]==subSelect));
      SqlUpdate update = SqlDml.Update(salesPerson);
      update.Values[salesPerson["SalesYTD"]] = salesPerson["SalesYTD"];

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test132()
    {
      string nativeSql = "UPDATE Sales_SalesPerson "
        +"SET SalesYTD = SalesYTD + "
          +"(SELECT SUM(so.SubTotal) "
            +"FROM Sales_SalesOrderHeader AS so "
              +"WHERE so.OrderDate = (SELECT MAX(OrderDate) "
                +"FROM Sales_SalesOrderHeader AS so2 "
                  +"WHERE so2.SalesPersonID = "
                    +"so.SalesPersonID) "
                      +"AND Sales_SalesPerson.SalesPersonID = so.SalesPersonID "
                        +"GROUP BY so.SalesPersonID);";

      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlTableRef so2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "so2");
      SqlTableRef so = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesOrderHeader"], "so");
      SqlSelect subSelect = SqlDml.Select(so2);
      subSelect.Columns.Add(SqlDml.Max(so2["OrderDate"]));
      subSelect.Where = so2["SalesPersonID"]==so["SalesPersonID"];
      SqlSelect select = SqlDml.Select(so);
      select.Columns.Add(SqlDml.Sum(so["SubTotal"]));
      select.Where = so["OrderDate"]==subSelect && salesPerson["SalesPersonID"]==so["SalesPersonID"];
      select.GroupBy.Add(so["SalesPersonID"]);
      SqlUpdate update = SqlDml.Update(salesPerson);
      update.Values[salesPerson["SalesYTD"]] = salesPerson["SalesYTD"]+select;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    [Ignore("VistaDB not supproted UPDATE TOP.")]
    public void Test133()
    {
      string nativeSql = "UPDATE TOP (10) Sales_Store "
        +"SET SalesPersonID = 276 "
          +"WHERE SalesPersonID = 275;";

      SqlTableRef store = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"]);
      SqlUpdate update = SqlDml.Update(store);
      update.Limit = 10;
      update.Values[store["SalesPersonID"]] = 276;
      update.Where = store["SalesPersonID"]==275;

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test134()
    {
      string nativeSql = "UPDATE HumanResources_Employee "
        +"SET VacationHours = VacationHours + 8 "
          +"FROM (SELECT TOP 10 EmployeeID FROM HumanResources_Employee "
            +"ORDER BY HireDate ASC) AS th "
              +"WHERE HumanResources_Employee.EmployeeID = th.EmployeeID;";

      SqlTableRef employee = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);
      SqlTableRef employee2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);

      SqlSelect select = SqlDml.Select(employee);
      select.Limit = 10;
      select.Columns.Add(employee["EmployeeID"]);
      select.OrderBy.Add(employee["HireDate"]);
      SqlQueryRef th = SqlDml.QueryRef(select, "th");

      SqlUpdate update = SqlDml.Update(employee2);
      update.Values[employee2["VacationHours"]] = employee2["VacationHours"]+8;
      update.From = th;
      update.Where = employee2["EmployeeID"]==th["EmployeeID"];

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
    }

    [Test]
    public void Test135()
    {
      string nativeSql = "DELETE FROM Sales_SalesPersonQuotaHistory "
        +"WHERE SalesPersonID IN "
          +"(SELECT SalesPersonID "
            +"FROM Sales_SalesPerson "
              +"WHERE SalesYTD > 2500000.00);";

      SqlTableRef salesPersonQuotaHistory =
        SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPersonQuotaHistory"]);
      SqlTableRef salesPerson = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlSelect subSelect = SqlDml.Select(salesPerson);
      subSelect.Columns.Add(salesPerson["SalesPersonID"]);
      subSelect.Where = salesPerson["SalesYTD"]>2500000.00;
      SqlDelete delete = SqlDml.Delete(salesPersonQuotaHistory);
      delete.Where = SqlDml.In(salesPersonQuotaHistory["SalesPersonID"], subSelect);

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, delete));
    }

    [Test]
    public void Test136()
    {
      string nativeSql = "DELETE FROM Sales_SalesPersonQuotaHistory "
        +"WHERE SalesPersonID IN  "
          +"(SELECT SalesPersonID "
            +"FROM Sales_SalesPerson "
              +"WHERE SalesYTD > 2500000.00);";

      SqlTableRef salesPersonQuotaHistory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPersonQuotaHistory"]);

      SqlTableRef sp = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_SalesPerson"]);
      SqlSelect subSelect = SqlDml.Select(sp);
      subSelect.Columns.Add(sp["SalesPersonID"]);
      subSelect.Where = sp["SalesYTD"]>2500000.00;

      SqlDelete delete = SqlDml.Delete(salesPersonQuotaHistory);
      delete.Where = SqlDml.In(salesPersonQuotaHistory["SalesPersonID"], subSelect);

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, delete));
    }

    [Test]
    [Ignore("Possibly VistaDB bug.")]
    public void Test137()
    {
      string nativeSql = "DELETE FROM Purchasing_PurchaseOrderDetail "
        +"WHERE PurchaseOrderDetailID IN "
          +"(SELECT TOP 10 PurchaseOrderDetailID "
            +"FROM Purchasing_PurchaseOrderDetail "
              +"ORDER BY DueDate ASC);";

      SqlTableRef purchaseOrderDetail1 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_PurchaseOrderDetail"]);
      SqlTableRef purchaseOrderDetail2 = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Purchasing_PurchaseOrderDetail"]);
      SqlSelect select = SqlDml.Select(purchaseOrderDetail2);
      select.Limit = 10;
      select.Columns.Add(purchaseOrderDetail2["PurchaseOrderDetailID"]);
      select.OrderBy.Add(purchaseOrderDetail2["DueDate"]);
      SqlDelete delete = SqlDml.Delete(purchaseOrderDetail1);
      delete.Where = SqlDml.In(purchaseOrderDetail1["PurchaseOrderDetailID"], select);

      Assert.IsTrue(CompareExecuteNonQuery(nativeSql, delete));
    }

    [Test]
    public void Test138()
    {
      string nativeSql = "DECLARE @EmpIDVar int; "
        +"SET @EmpIDVar = 1234; "
          +"SELECT * "
            +"FROM HumanResources_Employee "
              +"WHERE EmployeeID = @EmpIDVar;";

      SqlTableRef employee = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Employee"]);
      SqlVariable empIDVar = SqlDml.Variable("EmpIDVar", SqlType.Int32);
      SqlBatch batch = SqlDml.Batch();
      batch.Add(empIDVar.Declare());
      batch.Add(SqlDml.Assign(empIDVar, 1234));
      SqlSelect select = SqlDml.Select(employee);
      select.Columns.Add(employee.Asterisk);
      select.Where = employee["EmployeeID"]==empIDVar;
      batch.Add(select);

      Console.Write(SqlDriver.Compile(batch));
    }

    [Test]
    public void Test139()
    {
      string nativeSql = "SELECT Name, "
        +"CASE Name "
          +"WHEN 'Human Resources' THEN 'HR' "
            +"WHEN 'Finance' THEN 'FI' "
              +"WHEN 'Information Services' THEN 'IS' "
                +"WHEN 'Executive' THEN 'EX' "
                  +"WHEN 'Facilities and Maintenance' THEN 'FM' "
                    +"END AS Abbreviation "
                      +"FROM HumanResources_Department "
                        +"WHERE GroupName = 'Executive General and Administration'";

      SqlTableRef department = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_Department"]);
      SqlCase c = SqlDml.Case(department["Name"]);
      c["Human Resources"] = "HR";
      c["Finance"] = "FI";
      c["Information Services"] = "IS";
      c["Executive"] = "EX";
      c["Facilities and Maintenance"] = "FM";
      SqlSelect select = SqlDml.Select(department);
      select.Columns.AddRange(department["Name"]);
      select.Columns.Add(c, "Abbreviation");
      select.Where = department["GroupName"]=="Executive General and Administration";

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test140()
    {
      string nativeSql = "SELECT   ProductNumber, "
        +"CASE ProductLine "
          +"WHEN 'R' THEN 'Road' "
            +"WHEN 'M' THEN 'Mountain' "
              +"WHEN 'T' THEN 'Touring' "
                +"WHEN 'S' THEN 'Other sale items' "
                  +"ELSE 'Not for sale' "
                    +"END as Category, "
                      +"Name "
                        +"FROM Production_Product "
                          +"ORDER BY ProductNumber;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlCase c = SqlDml.Case(product["ProductLine"]);
      c["R"] = "Road";
      c["M"] = "Mountain";
      c["T"] = "Touring";
      c["S"] = "Other sale items";
      c.Else = "Not for sale";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductNumber"]);
      select.Columns.Add(c, "Category");
      select.Columns.Add(product["Name"]);
      select.OrderBy.Add(product["ProductNumber"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test141()
    {
      string nativeSql = "SELECT   ProductNumber, Name, "
        +"CASE "
          +"WHEN ListPrice =  0 THEN 'Mfg item - not for resale' "
            +"WHEN ListPrice < 50 THEN 'Under $50' "
              +
              "         WHEN ListPrice >= 50 and ListPrice < 250 THEN 'Under $250' "
                +
                "         WHEN ListPrice >= 250 and ListPrice < 1000 THEN 'Under $1000' "
                  +"ELSE 'Over $1000' "
                    +"END As 'Price Range'"
                      +"FROM Production_Product "
                        +"ORDER BY ProductNumber ;";

      SqlTableRef product = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Production_Product"]);
      SqlCase c = SqlDml.Case();
      c[product["ListPrice"]==0] = "Mfg item - not for resale";
      c[product["ListPrice"]<50] = "Under $50";
      c[product["ListPrice"]>=50 && product["ListPrice"]<250] = "Under $250";
      c[product["ListPrice"]>=250 && product["ListPrice"]<1000] = "Under $1000";
      c.Else = "Over $1000";
      SqlSelect select = SqlDml.Select(product);
      select.Columns.AddRange(product["ProductNumber"], product["Name"]);
      select.Columns.Add(c, "Price Range");
      select.OrderBy.Add(product["ProductNumber"]);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test142()
    {
      string nativeSql = "DECLARE @find varchar(30); "
        +"SET @find = 'Man%'; "
          +"SELECT LastName, FirstName, Phone "
            +"FROM Person_Contact "
              +"WHERE LastName LIKE @find;";

      SqlTableRef contact = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Person_Contact"]);
      SqlVariable find = SqlDml.Variable("find", new SqlValueType("varchar(30)"));
      SqlBatch batch = SqlDml.Batch();
      batch.Add(find.Declare());
      batch.Add(SqlDml.Assign(find, "Man%"));
      SqlSelect select = SqlDml.Select(contact);
      select.Columns.AddRange(contact["LastName"], contact["FirstName"], contact["Phone"]);
      select.Where = SqlDml.Like(contact["LastName"], find);
      batch.Add(select);

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, batch));
    }

    [Test]
    public void Test143()
    {
      string nativeSql = "SELECT * "
        +"FROM Sales_Store s "
          +"WHERE s.Name IN ('West Side Mart', 'West Wind Distributors', 'Westside IsCyclic Store')";

      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "s");
      SqlSelect select = SqlDml.Select(s);
      select.Columns.Add(SqlDml.Asterisk);
      select.Where = SqlDml.In(s["Name"], SqlDml.Array("West Side Mart", "West Wind Distributors", "Westside IsCyclic Store"));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    public void Test144()
    {
      string nativeSql = "SELECT * "
        +"FROM Sales_Store s "
          +"WHERE s.CustomerID IN (1, 2, 3)";

      SqlTableRef s = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["Sales_Store"], "s");
      SqlSelect select = SqlDml.Select(s);
      select.Columns.Add(SqlDml.Asterisk);
      select.Where = SqlDml.In(s["CustomerID"], SqlDml.Array(1, 2, 3));

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
    }

    [Test]
    [Ignore("Possibly VistaDB not supported cursors.")]
    public void Test145()
    {
      string nativeSql = "DECLARE complex_cursor CURSOR FOR "
        +"SELECT a.EmployeeID "
          +"FROM HumanResources_EmployeePayHistory AS a "
            +"WHERE RateChangeDate <> "
              +"(SELECT MAX(RateChangeDate) "
                +"FROM HumanResources_EmployeePayHistory AS b "
                  +"WHERE a.EmployeeID = b.EmployeeID); "
                    +"OPEN complex_cursor; "
                      +"FETCH FROM complex_cursor; "
                        +"UPDATE HumanResources_EmployeePayHistory "
                          +"SET PayFrequency = 2 "
                            +"WHERE CURRENT OF complex_cursor; "
                              +"CLOSE complex_cursor; "
                                +"DEALLOCATE complex_cursor;";

      SqlBatch batch = SqlDml.Batch();
      SqlTableRef employeePayHistory = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_EmployeePayHistory"]);
      SqlTableRef a = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_EmployeePayHistory"], "a");
      SqlTableRef b = SqlDml.TableRef(Catalog.Schemas["dbo"].Tables["HumanResources_EmployeePayHistory"], "b");
      SqlSelect selectInner = SqlDml.Select(b);
      selectInner.Columns.Add(SqlDml.Max(b["RateChangeDate"]));
      selectInner.Where = a["EmployeeID"]==b["EmployeeID"];
      SqlSelect select = SqlDml.Select(a);
      select.Columns.Add(a["EmployeeID"]);
      select.Where = a["RateChangeDate"]!=selectInner;
      SqlCursor cursor = SqlDml.Cursor("complex_cursor", select);
      batch.Add(cursor.Declare());
      batch.Add(cursor.Open());
      batch.Add(cursor.Fetch());
      SqlUpdate update = SqlDml.Update(employeePayHistory);
      update.Values[employeePayHistory["PayFrequency"]] = 2;
      update.Where = cursor;
      batch.Add(update);
      batch.Add(cursor.Close());

      Assert.IsTrue(CompareExecuteDataReader(nativeSql, batch));
    }

    [Test]
    public void Test146()
    {
      SqlDropTable drop = SqlDdl.Drop(Catalog.Schemas["dbo"].Tables["HumanResources_EmployeePayHistory"]);

      Console.Write(SqlDriver.Compile(drop));
    }

    [Test]
    public void Test147()
    {
      SqlCreateTable create = SqlDdl.Create(Catalog.Schemas["dbo"].Tables["Production_Product"]);

      Console.Write(SqlDriver.Compile(create));
    }

    [Test]
    [Ignore]
    public void Test148()
    {
      string nativeSql = "CREATE VIEW [HumanResources_vEmployee] "
        + "AS SELECT "
          + "e.[EmployeeID], "
            + "c.[Title], "
              + "c.[FirstName], "
                + "c.[MiddleName], "
                  + "c.[LastName], "
                    + "c.[Suffix], "
                      + "e.[Title] AS [JobTitle], "
                        + "c.[Phone], "
                          + "c.[EmailAddress], "
                            + "c.[EmailPromotion], "
                              + "a.[AddressLine1], "
                                + "a.[AddressLine2], "
                                  + "a.[City], "
                                    + "sp.[Name] AS [StateProvinceName], "
                                      + "a.[PostalCode], "
                                        + "cr.[Name] AS [CountryRegionName], "
                                          + "FROM [HumanResources_Employee] e "
                                            + "INNER JOIN [Person_Contact] c "
                                              + "ON c.[ContactID] = e.[ContactID] "
                                                + "INNER JOIN [HumanResources_EmployeeAddress] ea "
                                                  + "ON e.[EmployeeID] = ea.[EmployeeID] "
                                                    + "INNER JOIN [Person_Address] a "
                                                      + "ON ea.[AddressID] = a.[AddressID] "
                                                        + "INNER JOIN [Person_StateProvince] sp "
                                                          + "ON sp.[StateProvinceID] = a.[StateProvinceID] "
                                                            + "INNER JOIN [Person_CountryRegion] cr "
                                                              + "ON cr.[CountryRegionCode] = sp.[CountryRegionCode]";
      SqlCreateView create = SqlDdl.Create(Catalog.Schemas["dbo"].Views["HumanResources_vEmployee"]);

      Console.Write(SqlDriver.Compile(create));
    }

    [Test]
    public void Test149()
    {
      SqlAlterTable alter = SqlDdl.Alter(Catalog.Schemas["dbo"].Tables["Production_Product"],
        SqlDdl.AddColumn(Catalog.Schemas["dbo"].Tables["Production_Product"].TableColumns["Name"]));

      Console.Write(SqlDriver.Compile(alter));
    }

    [Test]
    public void Test150()
    {
      SqlAlterTable alter = SqlDdl.Alter(Catalog.Schemas["dbo"].Tables["Production_Product"],
        SqlDdl.DropColumn(Catalog.Schemas["dbo"].Tables["Production_Product"].TableColumns["Name"]));

      Console.Write(SqlDriver.Compile(alter));
    }

    [Test]
    public void Test151()
    {
      SqlAlterTable alter = SqlDdl.Alter(Catalog.Schemas["dbo"].Tables["Production_Product"],
        SqlDdl.AddConstraint(Catalog.Schemas["dbo"].Tables["Production_Product"].TableConstraints[0]));

      Console.Write(SqlDriver.Compile(alter));
    }

    [Test]
    public void Test152()
    {
      SqlAlterTable alter = SqlDdl.Alter(Catalog.Schemas["dbo"].Tables["Production_Product"],
        SqlDdl.DropConstraint(Catalog.Schemas["dbo"].Tables["Production_Product"].TableConstraints[0]));

      Console.Write(SqlDriver.Compile(alter));
    }

    [Test]
    public void Test153()
    {
      Table t = Catalog.Schemas["dbo"].Tables["Production_Product"];
      Index index = t.CreateIndex("MegaIndex");
      index.CreateIndexColumn(t.TableColumns[0]);
      index.CreateIndexColumn(t.TableColumns[1]);
      index.CreateIndexColumn(t.TableColumns[2], false);
      index.IsUnique = true;
      index.IsClustered = true;
      SqlCreateIndex create = SqlDdl.Create(index);

      Console.Write(SqlDriver.Compile(create));
    }

    [Test]
    public void RenameTest()
    {
      Schema schema = Catalog.DefaultSchema;
      Table table = schema.CreateTable("T1");
      table.CreateColumn("C1", new SqlValueType(SqlType.Int32));

      SqlConnection conn = null;
      DbTransaction trx = null;
      try {
        conn = SqlDriver.CreateConnection(TestUrl.VistaDb);
        conn.Open();
        trx = conn.BeginTransaction();

        using (var cmd = conn.CreateCommand()) {
          SqlBatch batch = SqlDml.Batch();
          batch.Add(SqlDdl.Create(table));
          cmd.CommandText = SqlDriver.Compile(batch).GetCommandText();
          cmd.Transaction = trx;
          cmd.ExecuteNonQuery();
        }

        var exModel1 = SqlDriver.ExtractModel(conn, trx);
        var exT1 = exModel1.Schemas[schema.DbName].Tables[table.DbName];
        Assert.IsNotNull(exT1);
        var exC1 = exT1.TableColumns["C1"];
        Assert.IsNotNull(exC1);

        using (var cmd = conn.CreateCommand()) {
          SqlBatch batch = SqlDml.Batch();
          batch.Add(SqlDdl.Rename(exC1, "C2"));
          batch.Add(SqlDdl.Rename(exT1, "T2"));
          cmd.CommandText = SqlDriver.Compile(batch).GetCommandText();
          cmd.Transaction = trx;
          cmd.ExecuteNonQuery();
        }

        var exModel2 = SqlDriver.ExtractModel(conn, trx);
        var exT2 = exModel2.Schemas[schema.DbName].Tables["T2"];
        Assert.IsNotNull(exT2);
        var exC2 = exT2.TableColumns["C2"];
        Assert.IsNotNull(exC2);

      } finally {
        trx.Rollback();
        conn.Close();
      }
    }
  }
}