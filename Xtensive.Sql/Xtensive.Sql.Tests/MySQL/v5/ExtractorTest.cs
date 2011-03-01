
namespace Xtensive.Sql.Tests.MySQL.v5
{
    using System.Data.Common;

    using NUnit.Framework;

    using Xtensive.Sql.Model;

    [TestFixture]
    public class ExtractorTest : SqlTest
    {
        protected override string Url { get { return TestUrl.MySQL50; } }

        [Test]
        public void Extract()
        {
            var catalog = ExtractCatalog();
            Assert.IsNotNull(catalog);
        }

        [Test]
        public void MainTableTest()
        {
            string dropTableQuery = @"drop table if exists dataTypesTestTable";

            string createTableQuery =
                @"CREATE TABLE dataTypesTestTable (
                    int_l4 int NULL ,
                    binary_l50 binary (50) NULL ,
                    bit_l1 bit NULL , 
                    char_10 char (10) COLLATE utf8_unicode_ci NULL ,
                    datetime_l8 datetime NULL ,
                    decimal_p18_s0 decimal(18, 0) NULL ,
                    decimal_p12_s11_l9 decimal(12, 11) NULL ,
                    float_p53 float NULL ,
                    image_16 blob NULL ,
                    image_17 tinyblob NULL ,
                    image_18 mediumblob NULL ,
                    image_19 longblob NULL ,
                    money_p19_s4_l8 decimal(18,2) NULL ,
                    nchar_l100 nchar (100) COLLATE utf8_unicode_ci NULL ,
                    tiny_text_01  tinytext COLLATE utf8_unicode_ci NULL ,
                    long_text_01  longtext COLLATE utf8_unicode_ci NULL ,
                    medium_text_01  mediumtext COLLATE utf8_unicode_ci NULL ,
                    numeric_p5_s5 numeric(5, 5) NULL ,
                    nvarchar_l50 nvarchar (50) COLLATE utf8_unicode_ci NULL ,
                    real_p24_s0_l4 real NULL ,
                    smalldatetime_l4 datetime NULL ,
                    tinyint_l2 tinyint NULL ,
                    smallint_l2 smallint NULL ,
                    int_l2 int NULL ,
                    mediumint_148 mediumint null,
                    big_int_120 bigint null,
                    small_money_p10_s4_l4  decimal(18,2) NULL ,
                    text_16   varchar (50) COLLATE utf8_unicode_ci NULL ,
                    timestamp_l8 timestamp NULL ,
                    tinyint_1_p3_s0_l1 tinyint NULL ,
                    varbinary_l150 varbinary (150) NULL ,
                    enum_231 ENUM('small', 'medium', 'large'),
                    set_162 SET('a', 'b', 'c', 'd'),
                    varchar_l50 varchar (50) COLLATE utf8_unicode_ci NULL
               )";

            ExecuteNonQuery(dropTableQuery);
            ExecuteNonQuery(createTableQuery);

            var model = ExtractCatalog();

            Table table = model.DefaultSchema.Tables["dataTypesTestTable"];
            Assert.IsTrue(table.TableColumns["int_l4"].DataType.Type == SqlType.Int32);
        }

        [Test]
        public void TestExtractCatalog()
        {
            var catalog = ExtractCatalog();
            Assert.GreaterOrEqual(catalog.Schemas.Count, 1);
        }


    }

    public abstract class MySqlExtractorTestBase
    {
        private const string connectionUrl = TestUrl.MySQL50;

        public virtual string CleanUpScript
        {
            get { return null; }
        }

        protected SqlConnection CreateConnection()
        {
            var driver = SqlDriver.Create(connectionUrl);
            var connection = driver.CreateConnection();
            connection.Open();
            return connection;
        }

        protected void ExecuteQuery(string sqlQuery, SqlConnection connection)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = sqlQuery;
            command.ExecuteNonQuery();
        }

        protected void ExecuteQuery(string sqlQuery)
        {
            if (string.IsNullOrEmpty(sqlQuery))
                return;
            using (SqlConnection connection = CreateConnection())
            {
                ExecuteQuery(sqlQuery, connection);
            }
        }

        protected virtual Catalog ExtractModel()
        {
            var driver = SqlDriver.Create(connectionUrl);
            using (var connection = driver.CreateConnection())
            {
                connection.Open();
                try
                {
                    connection.BeginTransaction();
                    var result = driver.ExtractCatalog(connection);
                    connection.Commit();
                    return result;
                }
                catch
                {
                    connection.Rollback();
                    throw;
                }
            }
        }

        [TestFixtureTearDown]
        public virtual void TearDown()
        {
            ExecuteQuery(CleanUpScript);
        }
    }
}
