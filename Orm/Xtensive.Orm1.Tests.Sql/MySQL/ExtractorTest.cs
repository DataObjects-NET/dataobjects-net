// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.02.25

using System;
using System.Data.Common;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Model;
    
namespace Xtensive.Orm.Tests.Sql.MySQL
{
    [TestFixture]
    public class ExtractorTest : SqlTest
    {
        #region Test DDL

        const string DropBadSetTableQuery = @"drop table if exists dataTypesBadSetTable";
        private const string CreateBadSetTableQuery =
            @"CREATE TABLE dataTypesBadSetTable (
                    set_162 SET('a', 'b', 'c', 'd')
            )";

        const string DropBadEnumTableQuery = @"drop table if exists dataTypesBadEnumTable";
        private const string CreateBadEnumTableQuery =
            @"CREATE TABLE dataTypesBadEnumTable (
                            num_231 ENUM('small', 'medium', 'large')
                        )";

        const string DropBadBitTableQuery = @"drop table if exists dataTypesBadBitTable";
        private const string CreateBadBitTableQuery =
            @"CREATE TABLE dataTypesBadBitTable (
                            bit_l1 bit NULL
            )";

        const string DropGoodTableQuery = @"drop table if exists dataTypesGoodTable";
        const string CreateGoodTableQuery =
            @"CREATE TABLE dataTypesGoodTable (
                    int_l4 int NULL ,
                    binary_l50 binary (50) NULL ,
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
                    varchar_l50 varchar (50) COLLATE utf8_unicode_ci NULL
               )";

        #endregion

        protected override void CheckRequirements()
        {
            Require.ProviderIs(StorageProvider.MySql);
        }
        
        private void DropBadTables()
        {
            this.ExecuteNonQuery(DropBadSetTableQuery);
            this.ExecuteNonQuery(DropBadEnumTableQuery);
            this.ExecuteNonQuery(DropBadBitTableQuery);
        }

        [Test]
        public void DefaultSchemaIsAvailable()
        {
            this.DropBadTables();

            var schema = this.ExtractDefaultSchema();
            Assert.IsNotNull(schema);
        }

        [Test]
        public void TestCatalogExtraction()
        {
            var catalog = ExtractCatalog();
            Assert.GreaterOrEqual(catalog.Schemas.Count, 1);
        }

        [Test]
        public void ExtractObjectsFromDefaultSchema()
        {
            this.DropBadTables();

            var schema = this.ExtractDefaultSchema();
            var catalog = this.ExtractSchema(schema.Name);
            Assert.IsNotNull(catalog);
        }

        [Test]
        public void TestForSupportedDataTypes()
        {
            this.DropBadTables();

            ExecuteNonQuery(DropGoodTableQuery);
            ExecuteNonQuery(CreateGoodTableQuery);

            var schema = ExtractDefaultSchema();
            var catalog = schema.Catalog;

            Table table = catalog.DefaultSchema.Tables["dataTypesGoodTable"];
            Assert.IsTrue(table.TableColumns["int_l4"].DataType.Type == SqlType.Int32);
        }

        [Test]
        //[ExpectedException(typeof(NotSupportedException))]
        [Ignore("")]
        public void TestForUnsupportedSETDatatypes()
        {
            this.DropBadTables();

            ExecuteNonQuery(DropBadSetTableQuery);
            ExecuteNonQuery(CreateBadSetTableQuery);

            var schema = ExtractDefaultSchema();
            var catalog = schema.Catalog;

            Table table = catalog.DefaultSchema.Tables["dataTypesBadSetTable"];
            Assert.IsNotNull(table);
        }

        [Test]
        //[ExpectedException(typeof(NotSupportedException))]
        [Ignore("")]
        public void TestForUnsupportedENUMDatatypes()
        {
            this.DropBadTables();

            ExecuteNonQuery(DropBadEnumTableQuery);
            ExecuteNonQuery(CreateBadEnumTableQuery);

            var schema = ExtractDefaultSchema();
            var catalog = schema.Catalog;

            Table table = catalog.DefaultSchema.Tables["dataTypesBadEnumTable"];
            Assert.IsNotNull(table);
        }

        [Test]
        //[ExpectedException(typeof(NotSupportedException))]
        [Ignore("")]
        public void TestForUnsupportedBITDatatypes()
        {
            this.DropBadTables();

            ExecuteNonQuery(DropBadBitTableQuery);
            ExecuteNonQuery(CreateBadBitTableQuery);

            var schema = ExtractDefaultSchema();
            var catalog = schema.Catalog;

            Table table = catalog.DefaultSchema.Tables["dataTypesBadBitTable"];
            Assert.IsNotNull(table);
        }
    }
}
