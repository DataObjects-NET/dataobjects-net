using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;


namespace Xtensive.Sql.Tests.MySQL.v5
{
    public class SakilaExtractorTest : Sakila
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
            sqlDriver = SqlDriver.Create(TestUrl.MySQL50);
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
            var p = sqlCommand.CreateParameter();
            p.ParameterName = "p1";
            p.DbType = DbType.String;
            p.Value = "ADAM";
            sqlCommand.Parameters.Add(p);

            SqlTableRef actor = SqlDml.TableRef(schema.Tables["actor"]);
            SqlSelect select = SqlDml.Select(actor);
            select.Columns.AddRange(actor["actor_id"], actor["first_name"], actor["last_name"], actor["last_update"]);
            select.Where = actor["first_name"] == SqlDml.ParameterRef(p.ParameterName);
            select.OrderBy.Add(actor["last_name"]);

            sqlCommand.CommandText = Compile(select).GetCommandText();
            sqlCommand.Prepare();

            DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
            Assert.AreEqual(r.RowCount, 2);
            Console.WriteLine(r);
        }


        [Test]
        public void Test001()
        {
            var p = sqlCommand.CreateParameter();
            p.ParameterName = "p1";
            p.DbType = DbType.String;
            p.Value = "ADAM";
            sqlCommand.Parameters.Add(p);

            SqlTableRef actor = SqlDml.TableRef(schema.Tables["actor"]);
            SqlSelect select = SqlDml.Select(actor);
            select.Distinct = true;
            select.Columns.AddRange(actor["first_name"]);
            select.Where = actor["first_name"] == SqlDml.ParameterRef(p.ParameterName);

            sqlCommand.CommandText = Compile(select).GetCommandText();
            sqlCommand.Prepare();

            DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
            Assert.AreEqual(r.RowCount, 1);
            Console.WriteLine(r);
        }

        [Test]
        public void Test002()
        {
            string nativeSql = "SELECT * FROM `city` `a`";

            SqlTableRef city = SqlDml.TableRef(Catalog.Schemas["SAKILA"].Tables["city"]);
            SqlSelect select = SqlDml.Select(city);
            select.Columns.Add(SqlDml.Asterisk);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test003()
        {
            string nativeSql = "SELECT `a`.`customer_id`, `a`.`first_name`, `a`.`last_name` FROM `customer` `a` WHERE (`a`.`store_id` < 2) ORDER BY `a`.`first_name` ASC, 3 DESC";

            SqlTableRef customer = SqlDml.TableRef(Catalog.Schemas["SAKILA"].Tables["customer"]);
            SqlSelect select = SqlDml.Select(customer);
            select.Columns.AddRange(customer["customer_id"], customer["first_name"], customer["last_name"]);
            select.Where = customer["store_id"] < 2;
            select.OrderBy.Add(customer["first_name"]);
            select.OrderBy.Add(3, false);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test006()
        {
            SqlTableRef payment = SqlDml.TableRef(schema.Tables["payment"]);
            SqlSelect select = SqlDml.Select(payment);
            select.Columns.Add(SqlDml.Asterisk);
            select.Where = payment["payment_date"] > DateTime.Now;
            select.OrderBy.Add(payment["payment_date"], false);

            sqlCommand.CommandText = Compile(select).GetCommandText();
            sqlCommand.Prepare();

            DbCommandExecutionResult r = GetExecuteDataReaderResult(sqlCommand);
            Assert.AreEqual(r.RowCount, 2);
        }

        [Test]
        public void Test007()
        {
            SqlTableRef payment = SqlDml.TableRef(schema.Tables["payment"]);
            SqlSelect select = SqlDml.Select(payment);
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
                                      t.city_id,
                                      t.city,
                                      c.country
                                    FROM
                                      city t
                                      INNER JOIN country c ON (t.country_id = c.country_id)
                                    ORDER BY
                                      t.city_id";

            SqlTableRef city = SqlDml.TableRef(schema.Tables["city"], "t");
            SqlTableRef country = SqlDml.TableRef(schema.Tables["country"], "c");

            SqlSelect select = SqlDml.Select(city.InnerJoin(country, city["country_id"] == country["country_id"]));

            select.Columns.Add(city["city_id"]);
            select.Columns.Add(city["city"]);
            select.Columns.Add(country["country"]);

            select.OrderBy.Add(city["city_id"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

        [Test]
        public void Test009() //TODO : Validate this query (Malisa)
        {

            string nativeSql = @"SELECT 
                                  c.customer_id,
                                  c.first_name,
                                  c.last_name,
                                  c.email,
                                  r.rental_date,
                                  f.title,
                                  f.description,
                                  f.release_year,
                                  r.return_date
                                FROM
                                  customer c
                                  INNER JOIN rental r ON (c.customer_id = r.customer_id)
                                  INNER JOIN inventory i ON (r.inventory_id = i.inventory_id)
                                  INNER JOIN film f ON (i.film_id = f.film_id)";

            SqlTableRef customer = SqlDml.TableRef(schema.Tables["customer"], "e");
            SqlTableRef rental = SqlDml.TableRef(schema.Tables["rental"], "r");
            SqlTableRef inventory = SqlDml.TableRef(schema.Tables["inventory"], "i");
            SqlTableRef film = SqlDml.TableRef(schema.Tables["film"], "f");


            SqlSelect select = SqlDml.Select(customer.InnerJoin(rental, customer["customer_id"] == rental["customer_id"])
                                                     .InnerJoin(inventory, rental["inventory_id"] == inventory["inventory_id"])
                                                     .InnerJoin(film, inventory["inventory"] == film["film_id"])
                                                     );
            select.Columns.Add(customer["customer_id"]);
            select.Columns.Add(customer["first_name"]);
            select.Columns.Add(customer["last_name"]);
            select.Columns.Add(customer["email"]);
            select.Columns.Add(rental["rental_date"]);
            select.Columns.Add(film["title"]);
            select.Columns.Add(film["description"]);
            select.Columns.Add(film["release_year"]);
            select.Columns.Add(rental["return_date"]);

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }


        [Test]
        public void Test010()
        {

            string nativeSql = @"SELECT 
                                      p.payment_id,
                                      round(p.amount * 12, 2) Rounded
                                FROM 
                                      payment p
                                WHERE p.payment_id = 12";

            SqlTableRef payment = SqlDml.TableRef(schema.Tables["payment"], "p");

            SqlSelect select = SqlDml.Select(payment);
            select.Columns.Add(payment["payment_id"]);
            select.Columns.Add(SqlDml.Round(payment["amount"] * 12, 2), "Rounded");
            select.Where = payment["payment_id"] == 12;

            Assert.IsTrue(CompareExecuteDataReader(nativeSql, select));
        }

    }
}
