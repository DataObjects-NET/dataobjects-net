﻿// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.06.10

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;


namespace Xtensive.Sql.Tests.Sqlite.v3
{
    public class NorthwindTest : Northwind
    {
        private SqlDriver sqlDriver;
        private SqlConnection sqlConnection;
        private DbCommand dbCommand;
        private DbCommand sqlCommand;

        private Schema schema = null;

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

            if (r1.RowCount != r2.RowCount)
                return false;
            if (r1.FieldCount != r2.FieldCount)
                return false;
            for (int i = 0; i < r1.FieldCount; i++)
            {
                if (r1.FieldNames[i] != r2.FieldNames[i])
                {
                    return false;
                }
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

            if (r1.RowCount != r2.RowCount)
                return false;
            return true;
        }

        private SqlCompilationResult Compile(ISqlCompileUnit statement)
        {
            return sqlDriver.Compile(statement);
        }

        #endregion

        #region Setup and TearDown

        [TestFixtureSetUp]
        public override void SetUp()
        {
            sqlDriver = SqlDriver.Create(TestUrl.Sqlite3);
            sqlConnection = sqlDriver.CreateConnection();

            dbCommand = sqlConnection.CreateCommand();
            sqlCommand = sqlConnection.CreateCommand();
            try
            {
                sqlConnection.Open();
            }
            catch (SystemException e)
            {
                Console.WriteLine(e);
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                sqlConnection.BeginTransaction();
                Catalog = sqlDriver.ExtractCatalog(sqlConnection);
                schema = sqlDriver.ExtractDefaultSchema(sqlConnection);
                sqlConnection.Commit();
            }
            catch
            {
                sqlConnection.Rollback();
                throw;
            }
            stopWatch.Stop();
            Console.WriteLine(stopWatch.Elapsed);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            try
            {
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed)
                    sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
            string nativeSql =
                        @"SELECT 
                          employees.EmployeeID,
                          employees.FirstName,
                          employees.LastName,
                          employees.BirthDate
                        FROM
                          employees
                        WHERE
                          employees.FirstName = 'Robert'
                        ORDER BY
                          employees.LastName";

            var p = sqlCommand.CreateParameter();
            p.ParameterName = "p1";
            p.DbType = DbType.String;
            p.Value = "Robert";
            sqlCommand.Parameters.Add(p);

            SqlTableRef employees = SqlDml.TableRef(schema.Tables["employees"]);
            SqlSelect select = SqlDml.Select(employees);
            select.Columns.AddRange(employees["EmployeeID"], employees["FirstName"], employees["LastName"], employees["BirthDate"]);
            select.Where = employees["FirstName"] == SqlDml.ParameterRef(p.ParameterName);
            select.OrderBy.Add(employees["LastName"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test001()
        {
            string nativeSql =
                @"SELECT DISTINCT
                      employees.FirstName
                    FROM
                      employees
                    WHERE
                      employees.TitleOfCourtesy = 'Mr.'";

            var p = sqlCommand.CreateParameter();
            p.ParameterName = "p10";
            p.DbType = DbType.String;
            p.Value = "Mr.";
            sqlCommand.Parameters.Add(p);

            SqlTableRef employees = SqlDml.TableRef(schema.Tables["employees"]);
            SqlSelect select = SqlDml.Select(employees);
            select.Distinct = true;
            select.Columns.AddRange(employees["FirstName"]);
            select.Where = employees["TitleOfCourtesy"] == SqlDml.ParameterRef(p.ParameterName);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test002()
        {
            string nativeSql = "SELECT * FROM [region] a";

            SqlTableRef region = SqlDml.TableRef(Catalog.Schemas["main"].Tables["region"]);
            SqlSelect select = SqlDml.Select(region);
            select.Columns.Add(SqlDml.Asterisk);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test003()
        {
            string nativeSql = "SELECT [a].[EmployeeID], [a].[FirstName], [a].[LastName] FROM [Employees] [a] WHERE ([a].[EmployeeID] < 2) ORDER BY [a].[FirstName] ASC, 3 DESC";

            SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["main"].Tables["Employees"]);
            SqlSelect select = SqlDml.Select(customer);
            select.Columns.AddRange(customer["EmployeeID"], customer["FirstName"], customer["LastName"]);
            select.Where = customer["EmployeeID"] < 2;
            select.OrderBy.Add(customer["FirstName"]);
            select.OrderBy.Add(3, false);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test006()
        {
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"]);
            SqlSelect select = SqlDml.Select(orders);
            select.Columns.Add(SqlDml.Asterisk);
            select.Where = orders["RequiredDate"] > DateTime.Now;
            select.OrderBy.Add(orders["RequiredDate"], false);

            sqlCommand.CommandText = Compile(select).GetCommandText();
            sqlCommand.Prepare();

            DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
            Assert.AreEqual(r.RowCount, 489);
        }

        [Test]
        public void Test007()
        {
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"]);
            SqlSelect select = SqlDml.Select(orders);
            select.Limit = 10;
            select.Columns.Add(SqlDml.Asterisk);

            sqlCommand.CommandText = Compile(select).GetCommandText();
            sqlCommand.Prepare();

            DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
            Assert.AreEqual(r.RowCount, 10);
        }

        [Test]
        public void Test008()
        {
            string nativeSql = @"SELECT 
                                  t.TerritoryID,
                                  t.TerritoryDescription,
                                  r.RegionDescription
                                FROM
                                  Territories t
                                  INNER JOIN Region r ON (t.RegionID = r.RegionID)
                                ORDER BY
                                  t.TerritoryID";

            SqlTableRef territories = SqlDml.TableRef(schema.Tables["Territories"], "t");
            SqlTableRef regions = SqlDml.TableRef(schema.Tables["Region"], "c");

            SqlSelect select = SqlDml.Select(territories.InnerJoin(regions, territories["RegionID"] == regions["RegionID"]));

            select.Columns.Add(territories["TerritoryID"]);
            select.Columns.Add(territories["TerritoryDescription"]);
            select.Columns.Add(regions["RegionDescription"]);

            select.OrderBy.Add(territories["TerritoryID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test009()
        {
            string nativeSql = @"SELECT 
                                  c.CustomerID,
                                  c.CompanyName,
                                  c.ContactName,
                                  c.ContactTitle,
                                  o.OrderDate,
                                  d.ProductID,
                                  p.ProductName,
                                  d.UnitPrice,
                                  d.Quantity,
                                  d.Discount
                                FROM
                                  Customers c
                                  INNER JOIN Orders o ON (c.CustomerID = o.CustomerID)
                                  INNER JOIN [Order Details] d ON (o.OrderID = d.OrderID)";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customers"], "c");
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "o");
            SqlTableRef orderDetails = SqlDml.TableRef(schema.Tables["Order Details"], "d");
            SqlTableRef products = SqlDml.TableRef(schema.Tables["products"], "p");


            SqlSelect select = SqlDml.Select(customer.InnerJoin(orders, customer["CustomerID"] == orders["CustomerID"])
                                                     .InnerJoin(orderDetails, orderDetails["OrderID"] == orders["EmployeeID"])
                                                     .InnerJoin(products, products["ProductID"] == orderDetails["ProductID"])

                                                     );
            select.Columns.Add(customer["CustomerID"]);
            select.Columns.Add(customer["CompanyName"]);
            select.Columns.Add(customer["ContactName"]);
            select.Columns.Add(customer["ContactName"]);
            select.Columns.Add(orders["OrderDate"]);
            select.Columns.Add(orderDetails["ProductID"]);
            select.Columns.Add(products["ProductName"]);
            select.Columns.Add(orderDetails["UnitPrice"]);
            select.Columns.Add(orderDetails["Quantity"]);
            select.Columns.Add(orderDetails["Discount"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test010()
        {
            string nativeSql = @"SELECT 
                                      p.orderid,
                                      round(p.freight * 12, 1) Rounded
                                FROM 
                                      orders p
                                WHERE p.orderid = 10255";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "o");

            SqlSelect select = SqlDml.Select(orders);
            select.Columns.Add(orders["orderid"]);
            select.Columns.Add(SqlDml.Round(orders["freight"] * 12, 1), "Rounded");
            select.Where = orders["orderid"] == 10255;

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test011()
        {
            string nativeSql = @"SELECT 
                                  c.CustomerID,
                                  c.FirstName,
                                  c.LastName,
                                  SUM(p.freight) AS Total
                                FROM
                                  customer c
                                  INNER JOIN orders p ON (c.CustomerID = p.CustomerID)
                                GROUP BY
                                  c.CustomerID,
                                  c.FirstName,
                                  c.LastName
                                ORDER BY c.CustomerID";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");

            SqlSelect select = SqlDml.Select(customer);
            select.Columns.AddRange(customer["CustomerID"], customer["FirstName"], customer["LastName"]);
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "p");
            SqlSelect sumOfPayment = SqlDml.Select(orders);
            sumOfPayment.Columns.Add(SqlDml.Sum(orders["freight"]));
            sumOfPayment.Where = customer["CustomerID"] == orders["CustomerID"];
            select.Columns.Add(sumOfPayment, "Total");

            select.OrderBy.Add(customer["CustomerID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test012()
        {
            string nativeSql = @"SELECT 
                                  CASE p.ProductID
                                  WHEN 1 THEN 'STAFF_1'
                                  WHEN 2 THEN 'STAFF_2'
                                  ELSE 'STAFF_OTHER'
                                  END AS shippers,
                                  SUM(p.UnitPrice) AS TotalUnits
                                FROM
                                  [order details] p
                                GROUP BY
                                  p.ProductID";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "p");

            SqlSelect select = SqlDml.Select(orders);
            SqlCase totalPayment = SqlDml.Case(orders["ProductID"]);
            totalPayment[1] = SqlDml.Literal("STAFF_1");
            totalPayment[2] = SqlDml.Literal("STAFF_2");
            totalPayment.Else = SqlDml.Literal("STAFF_OTHER");
            select.Columns.Add(totalPayment, "shippers");

            select.Columns.Add(SqlDml.Sum(orders["UnitPrice"]), "TotalUnits");
            select.GroupBy.AddRange(orders["ProductID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test013()
        {
            string nativeSql = @"SELECT 
                                  r.OrderID,
                                  r.EmployeeID, r.ShipName
                                FROM
                                  orders r
                                WHERE
                                  r.ShipRegion IS NOT NULL
                                ORDER BY
                                  r.OrderID";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "r");
            SqlSelect select = SqlDml.Select(orders);
            select.Columns.AddRange(orders["EmployeeID"], orders["return_date"], orders["rental_date"]);

            select.Where = SqlDml.IsNotNull(orders["return_date"]);
            select.OrderBy.Add(orders["EmployeeID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test014()
        {
            string nativeSql = @"SELECT 
                                  r.EmployeeID,
                                  r.return_date, r.rental_date,
                                  DATEDIFF(r.return_date, r.rental_date) Days
                                FROM
                                  orders r
                                WHERE
                                  r.return_date IS NOT NULL
                                ORDER BY
                                  r.EmployeeID";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "r");
            SqlSelect select = SqlDml.Select(orders);
            select.Columns.AddRange(orders["EmployeeID"], orders["return_date"], orders["rental_date"]);
            select.Columns.Add(
                              SqlDml.FunctionCall("DATEDIFF", orders["return_date"], orders["rental_date"]),
                              "Days"
                              );
            select.Where = SqlDml.IsNotNull(orders["return_date"]);
            select.OrderBy.Add(orders["EmployeeID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test015()
        {
            string nativeSql = @"SELECT  
                                  r.rental_id,
                                  DATEDIFF(CURDATE(), r.rental_date) TimeToToday
                                FROM 
                                  orders r
                                WHERE
                                  r.return_date IS NOT NULL";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "r");
            SqlSelect select = SqlDml.Select(orders);
            select.Columns.AddRange(orders["rental_id"]);
            select.Columns.Add(
                              SqlDml.FunctionCall("DATEDIFF", SqlDml.CurrentDate(), orders["rental_date"]),
                              "TimeToToday"
                              );
            select.Where = SqlDml.IsNotNull(orders["return_date"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test016()
        {
            string nativeSql = "SELECT SUM(p.freight) AS sum FROM orders p";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "p");

            SqlSelect select = SqlDml.Select(orders);
            select.Columns.Add(SqlDml.Sum(orders["freight"]), "sum");

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test017()
        {
            string nativeSql = "SELECT c.CustomerID, CONCAT(c.FirstName, CONCAT(', ', c.LastName)) as FullName FROM customer c";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");

            SqlSelect select = SqlDml.Select(customer);
            select.Columns.Add(customer["CustomerID"]);
            select.Columns.Add(SqlDml.Concat(customer["FirstName"], SqlDml.Concat(SqlDml.Literal(", "), customer["LastName"])),
                               "FullName"
                               );
            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test018()
        {
            string nativeSql = @"SELECT 
                                  c.CustomerID,
                                  c.FirstName,
                                  c.LastName,
                                  SUM(p.freight) AS Total
                                FROM
                                  customer c
                                  INNER JOIN orders p ON (c.CustomerID = p.CustomerID)
                                GROUP BY
                                  c.CustomerID,
                                  c.FirstName,
                                  c.LastName
                                HAVING SUM(p.freight) > 140";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "p");

            SqlSelect select = SqlDml.Select(customer.InnerJoin(orders, customer["CustomerID"] == orders["CustomerID"]));

            select.Columns.Add(customer["CustomerID"]);
            select.Columns.Add(customer["FirstName"]);
            select.Columns.Add(customer["LastName"]);
            select.Columns.Add(SqlDml.Sum(orders["freight"]), "Total");

            select.GroupBy.Add(customer["CustomerID"]);
            select.GroupBy.Add(customer["FirstName"]);
            select.GroupBy.Add(customer["LastName"]);

            select.Having = SqlDml.Sum(orders["freight"]) > 140;

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test019()
        {
            string nativeSql = @"SELECT 
                                    c.CustomerID,
                                    c.FirstName,
                                    c.LastName
                                FROM
                                    customer c
                                WHERE c.CustomerID IN (SELECT r.CustomerID FROM orders r WHERE r.EmployeeID = 239)
                                GROUP BY c.CustomerID,
                                    c.FirstName,
                                    c.LastName";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "r");

            SqlSelect innerSelect = SqlDml.Select(orders);
            innerSelect.Columns.Add(orders["CustomerID"]);
            innerSelect.Where = orders["EmployeeID"] == 239;

            SqlSelect select = SqlDml.Select(customer);

            select.Columns.Add(customer["CustomerID"]);
            select.Columns.Add(customer["FirstName"]);
            select.Columns.Add(customer["LastName"]);

            select.Where = SqlDml.In(customer["CustomerID"], innerSelect);

            select.GroupBy.Add(customer["CustomerID"]);
            select.GroupBy.Add(customer["FirstName"]);
            select.GroupBy.Add(customer["LastName"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test020()
        {
            string nativeSql = @"SELECT 
                                  f.ProductID,
                                  f.ProductName,
                                  f.QuantityPerUnit,
                                  f.UnitPrice,
                                  f.UnitsInStock
                                FROM
                                  products f
                                WHERE
                                  f.UnitsOnOrder BETWEEN 4 AND 5
                                ORDER BY f.ProductID";

            SqlTableRef products = SqlDml.TableRef(schema.Tables["products"], "f");
            SqlSelect select = SqlDml.Select(products);
            select.Columns.AddRange(products["ProductID"], products["title"], products["description"], products["UnitPrice"], products["UnitsInStock"]);
            select.Where = SqlDml.Between(products["UnitsOnOrder"], 4, 5);
            select.OrderBy.Add(products["ProductID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }


        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test021()
        {
            string nativeSql = @"SELECT 
                                  f.ProductID,
                                  f.ProductName,
                                  f.QuantityPerUnit,
                                  f.UnitPrice,
                                  f.CategoryID
                                FROM
                                  products f
                                WHERE
                                  f.CategoryID in ('PG', 'PG-13', 'R')
                                ORDER BY f.ProductID";

            SqlTableRef products = SqlDml.TableRef(schema.Tables["products"], "f");
            SqlSelect select = SqlDml.Select(products);
            select.Columns.AddRange(products["ProductID"], products["title"], products["description"], products["UnitPrice"], products["CategoryID"]);
            select.Where = SqlDml.In(products["CategoryID"], SqlDml.Row("PG", "PG-13", "R"));
            select.OrderBy.Add(products["ProductID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test022()
        {
            string nativeSql = @"SELECT 
                                  f.ProductID,
                                  f.ProductName,
                                  f.QuantityPerUnit,
                                  f.UnitPrice,
                                  f.CategoryID
                                FROM
                                  products f
                                WHERE
                                  f.ShipName LIKE 'R%'
                                ORDER BY f.ProductID";

            SqlTableRef products = SqlDml.TableRef(schema.Tables["products"], "f");
            SqlSelect select = SqlDml.Select(products);
            select.Columns.AddRange(products["ProductID"], products["title"], products["description"], products["UnitPrice"], products["CategoryID"]);
            select.Where = SqlDml.Like(products["ShipName"], "R%");
            select.OrderBy.Add(products["ProductID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test023()
        {
            string nativeSql = @"SELECT 
                                  f.ProductID,
                                  f.ProductName,
                                  f.QuantityPerUnit,
                                  f.UnitsInStock
                                FROM
                                  products f
                                WHERE
                                  (f.CategoryID = 'PG' OR 
                                  f.CategoryID = 'PG-13') AND 
                                  f.UnitsInStock < 100
                                ORDER BY
                                  f.ProductID";

            SqlTableRef products = SqlDml.TableRef(schema.Tables["products"], "f");
            SqlSelect select = SqlDml.Select(products);
            select.Columns.AddRange(products["ProductID"], products["title"], products["description"], products["UnitsInStock"]);
            select.Where = (products["CategoryID"] == "PG" || products["CategoryID"] == "PG-13") && products["UnitsInStock"] < 100;
            select.OrderBy.Add(products["ProductID"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test024()
        {
            string nativeSql = @"SELECT YEAR(r.rental_date) as Year, COUNT(*) Rented
                                    FROM orders r
                                    GROUP BY YEAR(r.rental_date)";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "r");

            SqlSelect select = SqlDml.Select(orders);
            select.Columns.Add(SqlDml.Extract(SqlDateTimePart.Year, orders["rental_date"]), "Year");
            select.Columns.Add(SqlDml.Count(), "Rented");

            select.GroupBy.Add(SqlDml.Extract(SqlDateTimePart.Year, orders["rental_date"]));

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test025()
        {
            string nativeSql = @"SELECT LastName FROM customer
                                    ORDER BY LastName 
                                    COLLATE utf8_general_ci ASC";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
            SqlSelect select = SqlDml.Select(customer);
            select.Columns.Add(customer["LastName"]);
            select.OrderBy.Add(SqlDml.Collate(customer["LastName"], Catalog.Schemas["main"].Collations["utf8_general_ci"]));

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test026()
        {
            string nativeSql = @"SELECT 
                                  p.CustomerID,
                                  p.freight
                                FROM
                                  orders p
                                WHERE
                                  p.freight = (SELECT MAX(freight) AS LowestPayment FROM orders)";

            SqlTableRef orders1 = SqlDml.TableRef(schema.Tables["orders"], "p1");
            SqlTableRef orders2 = SqlDml.TableRef(schema.Tables["orders"], "p2");

            SqlSelect innerSelect = SqlDml.Select(orders2);
            innerSelect.Columns.Add(SqlDml.Max(orders2["freight"]));

            SqlSelect select = SqlDml.Select(orders1);

            select.Columns.Add(orders1["CustomerID"]);
            select.Columns.Add(orders1["freight"]);

            select.Where = SqlDml.Equals(orders1["freight"], innerSelect);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test027()
        {
            string nativeSql = @"SELECT 
                                  c.CustomerID,
                                  c.FirstName
                                FROM
                                  customer c
                                WHERE EXISTS
                                    (SELECT * FROM orders p WHERE p.freight > 11.00 AND p.CustomerID = c.CustomerID )";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "c");
            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "p");

            SqlSelect innerSelect = SqlDml.Select(orders);
            SqlSelect select = SqlDml.Select(customer);

            innerSelect.Columns.Add(SqlDml.Asterisk);
            innerSelect.Where = orders["freight"] > 11.00 && orders["CustomerID"] == customer["CustomerID"];

            select.Columns.Add(customer["CustomerID"]);
            select.Columns.Add(customer["FirstName"]);
            select.Where = SqlDml.Exists(innerSelect);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test028()
        {
            string nativeSql = @"select * FROM customers c limit 0, 10";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customers"]);
            SqlSelect select = SqlDml.Select(customer);
            select.Limit = 10;
            select.Offset = 0;
            select.Columns.Add(SqlDml.Asterisk);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test029()
        {
            string nativeSql = "UPDATE products "
              + "SET UnitsInStock = UnitsInStock * 1.1 "
                + "WHERE ProductID = 1;";

            SqlTableRef products = SqlDml.TableRef(schema.Tables["products"], "f");
            SqlUpdate update = SqlDml.Update(products);
            update.Values[products["UnitsInStock"]] = products["UnitsInStock"] * 1.1;
            update.Where = products["ProductID"] == 1;

            Assert.IsTrue(CompareExecuteNonQuery(nativeSql, update));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test030()
        {
            string nativeSql = @"SELECT  
                                  r.rental_id,
                                  DATEDIFF(CURDATE(), r.rental_date) TimeToToday
                                FROM 
                                  orders r
                                WHERE
                                  r.return_date IS NOT NULL";

            SqlTableRef orders = SqlDml.TableRef(schema.Tables["orders"], "r");
            SqlSelect select = SqlDml.Select(orders);
            select.Columns.AddRange(orders["rental_id"]);
            select.Columns.Add(
                              SqlDml.FunctionCall("DATEDIFF", SqlDml.CurrentDate(), orders["rental_date"]),
                              "TimeToToday"
                              );
            select.Where = SqlDml.IsNotNull(orders["return_date"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test150()
        {
            SqlCreateTable create = SqlDdl.Create(Catalog.Schemas["main"].Tables["customers"]);
            Console.Write(Compile(create));
        }

        [Test]
        public void Test151()
        {
            SqlDropTable drop = SqlDdl.Drop(Catalog.Schemas["main"].Tables["customers"]);
            Console.Write(Compile(drop));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test152()
        {
            SqlDropSchema drop = SqlDdl.Drop(Catalog.Schemas["main"]);
            Console.Write(Compile(drop));
        }

        [Test]
        public void Test153()
        {
            SqlCreateView create = SqlDdl.Create(Catalog.Schemas["main"].Views["Order Subtotals"]);
            Console.Write(Compile(create));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test154()
        {
            SqlCreateSchema create = SqlDdl.Create(Catalog.Schemas["main"]);
            Console.Write(Compile(create));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test155()
        {
            SqlAlterTable alter =
              SqlDdl.Alter(
                Catalog.Schemas["main"].Tables["customer"],
                SqlDdl.AddColumn(Catalog.Schemas["main"].Tables["customer"].TableColumns["FirstName"]));

            Console.Write(Compile(alter));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test156()
        {
            SqlAlterTable alter =
              SqlDdl.Alter(
                Catalog.Schemas["main"].Tables["customer"],
                SqlDdl.DropColumn(Catalog.Schemas["main"].Tables["customer"].TableColumns["FirstName"]));

            Console.Write(Compile(alter));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test157()
        {
            var renameColumn = SqlDdl.Rename(Catalog.Schemas["main"].Tables["customer"].TableColumns["FirstName"], "FirstName");

            Console.Write(Compile(renameColumn));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test158()
        {
            var t = Catalog.Schemas["main"].Tables["customer"];
            Xtensive.Sql.Model.UniqueConstraint uc = t.CreateUniqueConstraint("newUniqueConstraint", t.TableColumns["email"]);
            SqlAlterTable stmt = SqlDdl.Alter(t, SqlDdl.AddConstraint(uc));

            Console.Write(Compile(stmt));
        }

        [Test]
        [Ignore("Works")]
        public void Test159()
        {
            var t = Catalog.Schemas["main"].Tables["customer"];
            Xtensive.Sql.Model.UniqueConstraint uc = t.CreateUniqueConstraint("newUniqueConstraint", t.TableColumns["email"]);
            SqlAlterTable stmt = SqlDdl.Alter(t, SqlDdl.DropConstraint(uc));

            Console.Write(Compile(stmt));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test160()
        {
            var t = Catalog.Schemas["main"].Tables["customer"];
            Index index = t.CreateIndex("MegaIndex195");
            index.CreateIndexColumn(t.TableColumns[0]);
            SqlCreateIndex create = SqlDdl.Create(index);

            Console.Write(Compile(create));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test161()
        {
            var t = Catalog.Schemas["main"].Tables["customer"];
            Index index = t.CreateIndex("MegaIndex196");
            index.CreateIndexColumn(t.TableColumns[0]);
            SqlDropIndex drop = SqlDdl.Drop(index);

            Console.Write(Compile(drop));
        }


        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test200()
        {
            string nativeSql =
              @"SELECT 
                  c.CustomerID,
                  c.store_id,
                  c.FirstName,
                  c.LastName,
                  c.email
                FROM 
                  customer c
                USE INDEX(idx_LastName)
                WHERE c.LastName > 'JOHNSON'";

            SqlTableRef c = SqlDml.TableRef(Catalog.Schemas["main"].Tables["customer"]);
            SqlSelect select = SqlDml.Select(c);
            select.Columns.AddRange(c["CustomerID"], c["store_id"], c["FirstName"], c["LastName"], c["email"]);
            select.Where = c["LastName"] > "JOHNSON";

            select.Hints.Add(SqlDml.NativeHint("idx_LastName"));

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        [ExpectedException(typeof(NotSupportedException))]
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

            Assert.IsTrue(CompareExecuteNonQuery(nativeSql, select));
        }

        [Test]
        [Ignore("ALTER Sequences are not supported")]
        public void Test177()
        {
            Sequence s = Catalog.Schemas["main"].CreateSequence("Generator177");
            SqlDropSequence drop = SqlDdl.Drop(s);

            Console.Write(Compile(drop));
        }

        [Test]
        [Ignore("Not yet prepared for tests")]
        public void Test165()
        {
            var t = Catalog.Schemas["main"].Tables["table1"];
            var uc = t.CreatePrimaryKey(string.Empty, t.TableColumns["field1"]);
            SqlAlterTable stmt = SqlDdl.Alter(t, SqlDdl.AddConstraint(uc));

            Console.Write(Compile(stmt));
        }

    }
}
