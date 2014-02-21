// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.MySQL
{
  [TestFixture]
  public abstract class Sakila
  {
    private readonly string sakilaDataBackupPath = Environment.CurrentDirectory + @"\MySQL\SakilaDb\sakila-data.sql";
    protected ConnectionInfo ConnectionInfo = TestConfiguration.Instance.GetConnectionInfo(TestConfiguration.Instance.Storage);
    protected SqlDriver SqlDriver;
    protected SqlConnection SqlConnection;

    public Catalog Catalog { get; protected set; }

    [TestFixtureSetUp]
    public virtual void SetUp()
    {
      CheckRequirements();
      SqlDriver = TestSqlDriver.Create(ConnectionInfo.ConnectionUrl.Url);
      SqlConnection = SqlDriver.CreateConnection();
      SqlConnection.Open();
      Catalog = SqlDriver.ExtractCatalog(SqlConnection);
      DropAllTables(Catalog.DefaultSchema, true);
      CreateSchema();
      InsertTestData();
    }

    [TestFixtureTearDown]
    public virtual void TearDown()
    {
      if (Catalog!=null)
        DropAllTables(Catalog.DefaultSchema, false);
      if (SqlConnection!=null && SqlConnection.State!=ConnectionState.Closed)
        SqlConnection.Close();
    }

    protected virtual void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.MySql);
    }

    private void CreateSchema()
    {
      var schema = Catalog.DefaultSchema;
      
      var batch = SqlDml.Batch();
      var table = schema.CreateTable("actor");
      table.CreateColumn("actor_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("first_name", new SqlValueType(SqlType.VarChar, 45));
      table.CreateColumn("last_name", new SqlValueType(SqlType.VarChar, 45));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");// add default value and on update event
      table.CreatePrimaryKey("pk_actor_actor_id", table.TableColumns["actor_id"]);
      var index = table.CreateIndex("inx_actor_last_name");
      index.CreateIndexColumn(table.TableColumns["last_name"]);
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));


      table = schema.CreateTable("country");
      table.CreateColumn("country_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("country", new SqlValueType(SqlType.VarChar, 50));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"); ;
      table.CreatePrimaryKey("pk_country_country_id", table.TableColumns["country_id"]);
      batch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("city");
      table.CreateColumn("city_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("city", new SqlValueType(SqlType.VarChar, 50));
      table.CreateColumn("country_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"); ;
      table.CreatePrimaryKey("pk_city_city_id", table.TableColumns["city_id"]);
      var foreignKey = table.CreateForeignKey("fk_city_country");
      foreignKey.ReferencedTable = schema.Tables["country"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["country_id"]);
      foreignKey.Columns.Add(table.TableColumns["country_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      index = table.CreateIndex("inx_city_country_id");
      index.CreateIndexColumn(table.TableColumns["country_id"]);
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));

      table = schema.CreateTable("address");
      table.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("address", new SqlValueType(SqlType.VarChar, 50));
      var column = table.CreateColumn("address2", new SqlValueType(SqlType.VarChar, 50));
      column.IsNullable = true;
      column.DefaultValue = SqlDml.Native("NULL");
      table.CreateColumn("district", new SqlValueType(SqlType.VarChar, 20));
      table.CreateColumn("city_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = table.CreateColumn("postal_code", new SqlValueType(SqlType.VarChar, 20));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("phone", new SqlValueType(SqlType.VarChar, 20));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_address_address_id", table.TableColumns["address_id"]);
      foreignKey = table.CreateForeignKey("fk_address_city");
      foreignKey.ReferencedTable = schema.Tables["city"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["city_id"]);
      foreignKey.Columns.Add(table.TableColumns["city_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      index = table.CreateIndex("inx_address_city_id");
      index.CreateIndexColumn(table.TableColumns["city_id"]);
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));
      
      table = schema.CreateTable("category");
      table.CreateColumn("category_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("name", new SqlValueType(SqlType.VarChar, 25));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"); ;
      table.CreatePrimaryKey("pk_category_category_id", table.TableColumns["category_id"]);
      batch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("film_text");
      table.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("title", new SqlValueType(SqlType.VarChar, 255));
      table.CreateColumn("text", new SqlValueType(SqlType.VarCharMax));
      table.CreatePrimaryKey("pk_film_text_film_id", table.TableColumns["film_id"]);
      var fullTextIndex = table.CreateFullTextIndex("idx_film_text_title_text");
      fullTextIndex.CreateIndexColumn(table.TableColumns["title"]);
      fullTextIndex.CreateIndexColumn(table.TableColumns["text"]);
      batch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("language");
      table.CreateColumn("language_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("name", new SqlValueType(SqlType.Char, 20));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_language_language_id", table.TableColumns["language_id"]);
      batch.Add(SqlDdl.Create(table));
      
      table = schema.CreateTable("film");
      table.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("title", new SqlValueType(SqlType.VarChar, 255));
      column = table.CreateColumn("description", new SqlValueType(SqlType.VarCharMax));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      column = table.CreateColumn("release_year", new SqlValueType("YEAR"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("language_id", new SqlValueType("TINYINT UNSIGNED"));
      column = table.CreateColumn("original_language_id", new SqlValueType("TINYINT UNSIGNED"));
      column.IsNullable = true;
      column.DefaultValue = SqlDml.Native("NULL");
      column = table.CreateColumn("rental_duration", new SqlValueType("TINYINT UNSIGNED"));
      column.DefaultValue = 3;
      column = table.CreateColumn("rental_rate", new SqlValueType(SqlType.Decimal, 5, 2));
      column.DefaultValue = 4.99;
      column = table.CreateColumn("length", new SqlValueType("SMALLINT UNSIGNED"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      column = table.CreateColumn("replacement_cost", new SqlValueType(SqlType.Decimal, 5, 2));
      column.DefaultValue = 19.99;
      table.CreateColumn("rating", new SqlValueType("ENUM('G', 'PG', 'PG-13', 'R', 'NC-17')")).DefaultValue = 'G';
      table.CreateColumn("special_features", new SqlValueType("SET('Trailers','Commentaries','Deleted Scenes','Behind the Scenes')")).DefaultValue = null;
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"); ;
      table.CreatePrimaryKey("pk_film", table.TableColumns["film_id"]);
      index = table.CreateIndex("idx_film_film_id");
      index.CreateIndexColumn(table.TableColumns["film_id"]);
      foreignKey = table.CreateForeignKey("fk_film_language");
      foreignKey.ReferencedTable = schema.Tables["language"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["language_id"]);
      foreignKey.Columns.Add(table.TableColumns["language_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_film_language_1");
      foreignKey.ReferencedTable = schema.Tables["language"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["language_id"]);
      foreignKey.Columns.Add(table.TableColumns["original_language_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));

      table = schema.CreateTable("film_actor");
      table.CreateColumn("actor_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_film_actor", table.TableColumns["actor_id"], table.TableColumns["film_id"]);
      foreignKey = table.CreateForeignKey("fk_film_actor_actor");
      foreignKey.ReferencedTable = schema.Tables["actor"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["actor_id"]);
      foreignKey.Columns.Add(table.TableColumns["actor_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_film_actor_film");
      foreignKey.ReferencedTable = schema.Tables["film"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["film_id"]);
      foreignKey.Columns.Add(table.TableColumns["film_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("film_category");
      table.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("category_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_film_category", table.TableColumns["film_id"], table.TableColumns["category_id"]);
      foreignKey = table.CreateForeignKey("fk_film_category_film");
      foreignKey.ReferencedTable = schema.Tables["film"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["film_id"]);
      foreignKey.Columns.Add(table.TableColumns["film_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_film_category_category");
      foreignKey.ReferencedTable = schema.Tables["category"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["category_id"]);
      foreignKey.Columns.Add(table.TableColumns["category_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Create(table));

      table = schema.CreateTable("store");
      table.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("manager_staff_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_store", table.TableColumns["store_id"]);
      table.CreateUniqueConstraint("idx_unique_manager", table.TableColumns["manager_staff_id"]);
      index = table.CreateIndex("idx_store_address_id");
      index.CreateIndexColumn(table.TableColumns["address_id"]);
      foreignKey = table.CreateForeignKey("fk_store_address_id");
      foreignKey.ReferencedTable = schema.Tables["address"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["address_id"]);
      foreignKey.Columns.Add(table.TableColumns["address_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));

      table = schema.CreateTable("staff");
      table.CreateColumn("staff_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("first_name", new SqlValueType(SqlType.VarChar, 45));
      table.CreateColumn("last_name", new SqlValueType(SqlType.VarChar, 45));
      table.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = table.CreateColumn("picture", new SqlValueType("BLOB"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("email", new SqlValueType(SqlType.VarChar, 50));
      table.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      column = table.CreateColumn("active", new SqlValueType(SqlType.Boolean));
      column.DefaultValue = SqlDml.Native("TRUE");
      table.CreateColumn("username", new SqlValueType(SqlType.VarChar, 16));
      column = table.CreateColumn("password", new SqlValueType("VARCHAR(40) BINARY"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_staff", table.TableColumns["staff_id"]);
      index = table.CreateIndex("idx_staff_address_id");
      index.CreateIndexColumn(table.TableColumns["address_id"]);
      var secondIndex = table.CreateIndex("idx_staff_store_id");
      secondIndex.CreateIndexColumn(table.TableColumns["store_id"]);
      foreignKey = table.CreateForeignKey("fk_staff_store");
      foreignKey.ReferencedTable = schema.Tables["store"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["store_id"]);
      foreignKey.Columns.Add(table.TableColumns["store_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_staff_address");
      foreignKey.ReferencedTable = schema.Tables["address"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["address_id"]);
      foreignKey.Columns.Add(table.TableColumns["address_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));
      batch.Add(SqlDdl.Create(secondIndex));

      foreignKey = schema.Tables["store"].CreateForeignKey("fk_store_staff");
      schema.Tables["store"].TableConstraints.Remove(foreignKey);
      foreignKey.ReferencedTable = schema.Tables["staff"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["staff_id"]);
      foreignKey.Columns.Add(schema.Tables["store"].TableColumns["manager_staff_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Alter(schema.Tables["store"], SqlDdl.AddConstraint(foreignKey)));
      
      table = schema.CreateTable("customer");
      table.CreateColumn("customer_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("first_name", new SqlValueType(SqlType.VarChar, 45));
      table.CreateColumn("last_name", new SqlValueType(SqlType.VarChar, 45));
      column = table.CreateColumn("email", new SqlValueType(SqlType.VarChar, 50));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = table.CreateColumn("active", new SqlValueType(SqlType.Boolean));
      column.DefaultValue = SqlDml.Native("TRUE");
      table.CreateColumn("create_date", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_customer", table.TableColumns["customer_id"]);
      index = table.CreateIndex("idx_store_id");
      index.CreateIndexColumn(table.TableColumns["store_id"]);
      secondIndex = table.CreateIndex("idx_address_id");
      secondIndex.CreateIndexColumn(table.TableColumns["address_id"]);
      var thirdIndex = table.CreateIndex("idx_last_name");
      thirdIndex.CreateIndexColumn(table.TableColumns["last_name"]);
      foreignKey = table.CreateForeignKey("fk_customer_address");
      foreignKey.ReferencedTable = schema.Tables["address"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["address_id"]);
      foreignKey.Columns.Add(table.TableColumns["address_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));
      batch.Add(SqlDdl.Create(secondIndex));
      batch.Add(SqlDdl.Create(thirdIndex));

      table = schema.CreateTable("inventory");
      table.CreateColumn("inventory_id", new SqlValueType("MEDIUMINT UNSIGNED"));
      table.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_inventory", table.TableColumns["inventory_id"]);
      table.CreateIndex("idx_inventory_film_id").CreateIndexColumn(table.TableColumns["film_id"]);
      index = table.CreateIndex("idx_inventory_store_id_film_id");
      index.CreateIndexColumn(table.TableColumns["store_id"]);
      index.CreateIndexColumn(table.TableColumns["film_id"]);
      foreignKey = table.CreateForeignKey("fk_inventory_store");
      foreignKey.ReferencedTable = schema.Tables["store"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["store_id"]);
      foreignKey.Columns.Add(table.TableColumns["store_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_inventory_film");
      foreignKey.ReferencedTable = schema.Tables["film"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["film_id"]);
      foreignKey.Columns.Add(table.TableColumns["film_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));

      table = schema.CreateTable("rental");
      table.CreateColumn("rental_id", new SqlValueType(SqlType.Int32));
      table.CreateColumn("rental_date", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("inventory_id", new SqlValueType("MEDIUMINT UNSIGNED"));
      table.CreateColumn("customer_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = table.CreateColumn("return_date", new SqlValueType(SqlType.DateTime));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("staff_id", new SqlValueType("TINYINT UNSIGNED"));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_rental", table.TableColumns["rental_id"]);
      table.CreateUniqueConstraint("unique_rental_date_inventory_id_customer_id", table.TableColumns["rental_date"], table.TableColumns["inventory_id"], table.TableColumns["customer_id"]);
      index = table.CreateIndex("idx_rental_inventory_id");
      index.CreateIndexColumn(table.TableColumns["inventory_id"]);
      secondIndex = table.CreateIndex("idx_rental_customer_id");
      secondIndex.CreateIndexColumn(table.TableColumns["customer_id"]);
      thirdIndex = table.CreateIndex("idx_rental_staff_id");
      thirdIndex.CreateIndexColumn(table.TableColumns["staff_id"]);
      foreignKey = table.CreateForeignKey("fk_rental_staff");
      foreignKey.ReferencedTable = schema.Tables["staff"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["staff_id"]);
      foreignKey.Columns.Add(table.TableColumns["staff_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_rental_inventory");
      foreignKey.ReferencedTable = schema.Tables["inventory"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["inventory_id"]);
      foreignKey.Columns.Add(table.TableColumns["inventory_id"]);
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));
      batch.Add(SqlDdl.Create(secondIndex));
      batch.Add(SqlDdl.Create(thirdIndex));

      table = schema.CreateTable("payment");
      table.CreateColumn("payment_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("customer_id", new SqlValueType("SMALLINT UNSIGNED"));
      table.CreateColumn("staff_id", new SqlValueType("TINYINT UNSIGNED"));
      column = table.CreateColumn("rental_id", new SqlValueType(SqlType.Int32));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      table.CreateColumn("amount", new SqlValueType(SqlType.Decimal, 5, 2));
      table.CreateColumn("payment_date", new SqlValueType(SqlType.DateTime));
      table.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
      table.CreatePrimaryKey("pk_payment", table.TableColumns["payment_id"]);
      index = table.CreateIndex("idx_payment_staff_id");
      index.CreateIndexColumn(table.TableColumns["staff_id"]);
      secondIndex = table.CreateIndex("idx_payment_customer_id");
      secondIndex.CreateIndexColumn(table.TableColumns["customer_id"]);
      foreignKey = table.CreateForeignKey("fk_payment_staff");
      foreignKey.ReferencedTable = schema.Tables["staff"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["staff_id"]);
      foreignKey.Columns.Add(table.TableColumns["staff_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_payment_customer");
      foreignKey.ReferencedTable = schema.Tables["customer"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["customer_id"]);
      foreignKey.Columns.Add(table.TableColumns["customer_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = table.CreateForeignKey("fk_payment_rental");
      foreignKey.ReferencedTable = schema.Tables["rental"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["rental_id"]);
      foreignKey.Columns.Add(table.TableColumns["rental_id"]);
      batch.Add(SqlDdl.Create(table));
      batch.Add(SqlDdl.Create(index));
      batch.Add(SqlDdl.Create(secondIndex));

      table = schema.CreateTable("table1");
      table.CreateColumn("field1", new SqlValueType(SqlType.Int32));
      table.CreateColumn("field2", new SqlValueType(SqlType.VarChar, 20)).IsNullable = true;
      batch.Add(SqlDdl.Create(table));

      var view = schema.CreateView("customer_list", SqlDml.Native(@"SELECT cu.customer_id AS ID, CONCAT(cu.first_name, _utf8' ', cu.last_name) AS name, a.address AS address, a.postal_code AS `zip code`,
        a.phone AS phone, city.city AS city, country.country AS country, IF(cu.active, _utf8'active',_utf8'') AS notes, cu.store_id AS SID
        FROM customer AS cu JOIN address AS a ON cu.address_id = a.address_id JOIN city ON a.city_id = city.city_id
        JOIN country ON city.country_id = country.country_id"));

      batch.Add(SqlDdl.Create(view));

      using (var cmd = SqlConnection.CreateCommand()) {
        cmd.CommandText = SqlDriver.Compile(batch).GetCommandText();
        cmd.ExecuteNonQuery();
      }
      Catalog = SqlDriver.ExtractCatalog(SqlConnection);
    }

    private void InsertTestData()
    {
      DisableConstraintChecks();
      ParseDataDump();
      EnableConstraintChecks();
    }

    private void ParseDataDump()
    {
      using (var reader = new StreamReader(sakilaDataBackupPath)) {
        string line;
        while ((line = reader.ReadLine())!=null) {
          if (line.StartsWith("--") || string.IsNullOrWhiteSpace(line) || 
              line.StartsWith("SET @") || line.StartsWith("SET SQL") || 
              line.StartsWith("SET FOREIGN") || line.StartsWith("SET UNIQUE") || 
              line.StartsWith("USE"))
            continue;
          ExecuteCommand(line);
        }
      }
    }

    private void DropAllTables(Schema schema, bool extractCatalogAfterDropping)
    {
      var batch = SqlDml.Batch();
      var foreignKeys = schema.Tables.SelectMany(el => el.TableConstraints.OfType<ForeignKey>());
      foreach (var foreignKey in foreignKeys)
        batch.Add(SqlDdl.Alter(foreignKey.Table, SqlDdl.DropConstraint(foreignKey)));
      foreach (var view in schema.Views) {
        batch.Add(SqlDdl.Drop(view));
        schema.Tables.Remove(schema.Tables[view.Name]);
      }
      foreach (var table in schema.Tables)
        batch.Add(SqlDdl.Drop(table));
      if (batch.Count<=0)
        return;
      var sqlCommand = SqlConnection.CreateCommand();
      sqlCommand.CommandText = SqlDriver.Compile(batch).GetCommandText();
      sqlCommand.ExecuteNonQuery();
      if (extractCatalogAfterDropping)
        Catalog = SqlDriver.ExtractCatalog(SqlConnection);
    }
    
    private void DisableConstraintChecks()
    {
      using (var sqlCommand = SqlConnection.CreateCommand()) {
        sqlCommand.CommandText = @"SET @'OLD_UNIQUE_CHECKS'=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
          SET @'OLD_FOREIGN_KEY_CHECKS'=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
          SET @'OLD_SQL_MODE'=@@SQL_MODE, SQL_MODE='TRADITIONAL';";
        sqlCommand.ExecuteNonQuery();
      }
    }

    private void EnableConstraintChecks()
    {
      using (var sqlCommand = SqlConnection.CreateCommand()) {
        sqlCommand.CommandText = @"SET SQL_MODE=@'OLD_SQL_MODE';
          SET FOREIGN_KEY_CHECKS=@'OLD_FOREIGN_KEY_CHECKS';
          SET UNIQUE_CHECKS=@'OLD_UNIQUE_CHECKS';";
        sqlCommand.ExecuteNonQuery();
      }
    }
    
    private void ExecuteCommand(string commandText)
    {
      using (var sqlCommand = SqlConnection.CreateCommand()) {
        sqlCommand.CommandText = commandText;
        sqlCommand.ExecuteNonQuery();
      }
    }
  }
}
