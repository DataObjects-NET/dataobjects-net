// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Malisa Ncube
// Created:    2011.03.17

using System.Data;
using NUnit.Framework;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;
using Constraint = Xtensive.Sql.Model.Constraint;

namespace Xtensive.Orm.Tests.Sql.MySQL
{
  [TestFixture]
  public abstract class Sakila
  {
    protected struct DbCommandExecutionResult
    {
      public int FieldCount;
      public string[] FieldNames;
      public int RowCount;

      public override string ToString()
      {
        if (FieldNames==null)
          FieldNames = new string[0];
        return string.Format("Fields: '{0}'; Rows: {1}", string.Join("', '", FieldNames), RowCount);
      }
    }

    protected static DbCommandExecutionResult GetExecuteDataReaderResult(IDbCommand cmd)
    {
      DbCommandExecutionResult result = new DbCommandExecutionResult();
      try {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        int rowCount = 0;
        int fieldCount = 0;
        string[] fieldNames = new string[0];
        using (IDataReader reader = cmd.ExecuteReader()) {
          while (reader.Read()) {
            if (rowCount==0) {
              fieldCount = reader.FieldCount;
              fieldNames = new string[fieldCount];
              for (int i = 0; i<fieldCount; i++) {
                fieldNames[i] = reader.GetName(i);
              }
            }
            rowCount++;
          }
        }
        result.RowCount = rowCount;
        result.FieldCount = fieldCount;
        result.FieldNames = fieldNames;
      }
      //      catch (Exception e) {
      //        Console.WriteLine(e);
      //      }
      finally {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    protected static DbCommandExecutionResult GetExecuteNonQueryResult(IDbCommand cmd)
    {
      DbCommandExecutionResult result = new DbCommandExecutionResult();
      try
      {
        cmd.Transaction = cmd.Connection.BeginTransaction();
        result.RowCount = cmd.ExecuteNonQuery();
      }
      //      catch (Exception e) {
      //        Console.WriteLine(e);
      //      }
      finally
      {
        cmd.Transaction.Rollback();
      }
      return result;
    }

    public void IgnoreMe(string message)
    {
      throw new IgnoreException(message);
    }

    public Catalog Catalog { get; protected set; }

    [TestFixtureSetUp]
    public virtual void SetUp()
    {
      //      BinaryModelProvider bmp = new BinaryModelProvider(@"C:/Debug/AdventureWorks.bin");
      //      model = Database.Model.Build(bmp);
      //
      //      if (model!=null)
      //        return;
      var connectionInfo = TestConfiguration.Instance.GetConnectionInfo(TestConfiguration.Instance.Storage);
      var sqlDriver = TestSqlDriver.Create(connectionInfo.ConnectionUrl.Url);
      var sqlConnection = sqlDriver.CreateConnection();
      sqlConnection.Open();
      Catalog = sqlDriver.ExtractCatalog(sqlConnection);
      var droppedSchema = Catalog.Schemas["dotest"];
      Catalog.Schemas.Remove(droppedSchema);

      Catalog.CreateSchema("dotest");
      Schema sch = Catalog.Schemas["dotest"];
      Table t;
      View v;
      TableColumn c;
      Constraint cs;
      

      t = sch.CreateTable("actor");
      t.CreateColumn("actor_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("first_name", new SqlValueType(SqlType.VarChar, 45));
      t.CreateColumn("last_name", new SqlValueType(SqlType.VarChar, 45));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP")).DefaultValue = SqlDml.Native("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");// add default value and on update event
      t.CreatePrimaryKey("pk_actor_actor_id", t.TableColumns["actor_id"]);
      t.CreateIndex("inx_actor_last_name").CreateIndexColumn(t.TableColumns["last_name"]);
      

      t = sch.CreateTable("country");
      t.CreateColumn("country_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("country", new SqlValueType(SqlType.VarChar, 50));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_country_country_id", t.TableColumns["country_id"]);

      t = sch.CreateTable("city");
      t.CreateColumn("city_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("city", new SqlValueType(SqlType.VarChar, 50));
      t.CreateColumn("country_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_city_city_id", t.TableColumns["city_id"]);
      var foreignKey = t.CreateForeignKey("fk_city_country");
      foreignKey.ReferencedTable = sch.Tables["country"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["country_id"]);
      foreignKey.Columns.Add(t.TableColumns["country_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      t.CreateIndex("inx_city_country_id").CreateIndexColumn(t.TableColumns["country_id"]);

      t = sch.CreateTable("address");
      t.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("address", new SqlValueType(SqlType.VarChar, 50));
      var column = t.CreateColumn("address2", new SqlValueType(SqlType.VarChar, 50));
      column.IsNullable = true;
      column.DefaultValue = SqlDml.Native("NULL");
      t.CreateColumn("district", new SqlValueType(SqlType.VarChar, 20));
      t.CreateColumn("city_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = t.CreateColumn("postal_code", new SqlValueType(SqlType.VarChar, 20));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      t.CreateColumn("phone", new SqlValueType(SqlType.VarChar, 20));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));// add default value and on update event
      t.CreatePrimaryKey("pk_address_address_id", t.TableColumns["address_id"]);
      foreignKey = t.CreateForeignKey("fk_address_city");
      foreignKey.ReferencedTable = sch.Tables["city"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["city_id"]);
      foreignKey.Columns.Add(t.TableColumns["city_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      t.CreateIndex("inx_address_city_id").CreateIndexColumn(t.TableColumns["city_id"]);

      t = sch.CreateTable("category");
      t.CreateColumn("category_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("name", new SqlValueType(SqlType.VarChar, 25));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_category_category_id", t.TableColumns["category_id"]);

      t = sch.CreateTable("film");
      t.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("title", new SqlValueType(SqlType.VarChar, 255));
      column = t.CreateColumn("description", new SqlValueType(SqlType.VarCharMax));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      column = t.CreateColumn("release_year", new SqlValueType("YEAR"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      column = t.CreateColumn("rental_duration", new SqlValueType("TINYINT UNSIGNED"));
      column.DefaultValue = 3;
      column = t.CreateColumn("rental_rate", new SqlValueType(SqlType.Decimal, 5, 2));
      column.DefaultValue = 4.99;
      column = t.CreateColumn("length", new SqlValueType("SMALLINT UNSIGNED"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      column = t.CreateColumn("replacement_cost", new SqlValueType(SqlType.Decimal, 5, 2));
      column.DefaultValue = 19.99;
      t.CreateColumn("rating", new SqlValueType("ENUM('G', 'PG', 'PG-13', 'R', 'NC-17')")).DefaultValue = 'G';
      t.CreateColumn("special_features", new SqlValueType("SET('Trailers','Commentaries','Deleted Scenes','Behind the Scenes')")).DefaultValue = null;
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_film", t.TableColumns["film_id"]);
      t.CreateIndex("inx_film_film_id").CreateIndexColumn(t.TableColumns["film_id"]);
      
      t = sch.CreateTable("film_actor");
      t.CreateColumn("actor_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_film_actor", t.TableColumns["actor_id"], t.TableColumns["film_id"]);
      foreignKey = t.CreateForeignKey("fk_film_actor_actor");
      foreignKey.ReferencedTable = sch.Tables["actor"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["actor_id"]);
      foreignKey.Columns.Add(t.TableColumns["actor_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_film_actor_film");
      foreignKey.ReferencedTable = sch.Tables["film"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["film_id"]);
      foreignKey.Columns.Add(t.TableColumns["film_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      t = sch.CreateTable("film_category");
      t.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("category_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_film_category", t.TableColumns["film_id"], t.TableColumns["category_id"]);
      foreignKey = t.CreateForeignKey("fk_film_category_film");
      foreignKey.ReferencedTable = sch.Tables["film"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["film_id"]);
      foreignKey.Columns.Add(t.TableColumns["film_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_film_category_category");
      foreignKey.ReferencedTable = sch.Tables["category"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["category_id"]);
      foreignKey.Columns.Add(t.TableColumns["category_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      t = sch.CreateTable("store");
      t.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      t.CreateColumn("manager_staff_id", new SqlValueType("TINYINT UNSIGNED"));
      t.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_store", t.TableColumns["store_id"]);
      t.CreateUniqueConstraint("idx_unique_manager", t.TableColumns["manager_staff_id"]);
      t.CreateIndex("idx_store_address_id").CreateIndexColumn(t.TableColumns["address_id"]);
      foreignKey = t.CreateForeignKey("fk_store_address_id");
      foreignKey.ReferencedTable = sch.Tables["address"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["address_id"]);
      foreignKey.Columns.Add(t.TableColumns["address_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      t = sch.CreateTable("staff");
      t.CreateColumn("staff_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("first_name", new SqlValueType(SqlType.VarChar, 45));
      t.CreateColumn("last_name", new SqlValueType(SqlType.VarChar, 45));
      t.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = t.CreateColumn("picture", new SqlValueType("BLOB"));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      t.CreateColumn("email", new SqlValueType(SqlType.VarChar, 50));
      t.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      column = t.CreateColumn("active", new SqlValueType(SqlType.Boolean));
      column.DefaultValue = SqlDml.Native("TRUE");
      t.CreateColumn("username", new SqlValueType(SqlType.VarChar, 16));
      t.CreateColumn("password", new SqlValueType(SqlType.VarChar, 40));// column must be binary
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_staff", t.TableColumns["staff_id"]);
      t.CreateIndex("idx_staff_address_id").CreateIndexColumn(t.TableColumns["address_id"]);
      t.CreateIndex("idx_staff_stpre_id").CreateIndexColumn(t.TableColumns["store_id"]);
      foreignKey = t.CreateForeignKey("fk_staff_store");
      foreignKey.ReferencedTable = sch.Tables["store"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["store_id"]);
      foreignKey.Columns.Add(t.TableColumns["store_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_staff_address");
      foreignKey.ReferencedTable = sch.Tables["address"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["address_id"]);
      foreignKey.Columns.Add(t.TableColumns["address_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      foreignKey = sch.Tables["store"].CreateForeignKey("fk_store_staff");
      foreignKey.ReferencedTable = sch.Tables["staff"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["staff_id"]);
      foreignKey.Columns.Add(sch.Tables["store"].TableColumns["manager_staff_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      t = sch.CreateTable("customer");
      t.CreateColumn("customer_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      t.CreateColumn("first_name", new SqlValueType(SqlType.VarChar, 45));
      t.CreateColumn("last_name", new SqlValueType(SqlType.VarChar, 45));
      column = t.CreateColumn("email", new SqlValueType(SqlType.VarChar, 50));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      t.CreateColumn("address_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = t.CreateColumn("active", new SqlValueType(SqlType.Boolean));
      column.DefaultValue = SqlDml.Native("TRUE");
      t.CreateColumn("create_date", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_customer", t.TableColumns["customer_id"]);
      t.CreateIndex("idx_customer_store_id").CreateIndexColumn(t.TableColumns["store_id"]);
      t.CreateIndex("idx_customer_address_id").CreateIndexColumn(t.TableColumns["address_id"]);
      t.CreateIndex("idx_customer_last_name").CreateIndexColumn(t.TableColumns["last_name"]);
      foreignKey = t.CreateForeignKey("fk_customer_address");
      foreignKey.ReferencedTable = sch.Tables["address"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["address_id"]);
      foreignKey.Columns.Add(t.TableColumns["address_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      t = sch.CreateTable("inventory");
      t.CreateColumn("inventory_id", new SqlValueType("MEDIUMINT UNSIGNED"));
      t.CreateColumn("film_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("store_id", new SqlValueType("TINYINT UNSIGNED"));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_inventory", t.TableColumns["inventory_id"]);
      t.CreateIndex("idx_inventory_film_id").CreateIndexColumn(t.TableColumns["film_id"]);
      var index = t.CreateIndex("idx_inventory_store_id_film_id");
      index.CreateIndexColumn(t.TableColumns["store_id"]);
      index.CreateIndexColumn(t.TableColumns["film_id"]);
      foreignKey = t.CreateForeignKey("fk_inventory_store");
      foreignKey.ReferencedTable = sch.Tables["store"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["store_id"]);
      foreignKey.Columns.Add(t.TableColumns["store_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_inventory_film");
      foreignKey.ReferencedTable = sch.Tables["film"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["film_id"]);
      foreignKey.Columns.Add(t.TableColumns["film_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;

      t = sch.CreateTable("rental");
      t.CreateColumn("rental_id", new SqlValueType(SqlType.Int32));
      t.CreateColumn("rental_date", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("inventory_id", new SqlValueType("MEDIUMINT UNSIGNED"));
      t.CreateColumn("customer_id", new SqlValueType("SMALLINT UNSIGNED"));
      column = t.CreateColumn("return_date", new SqlValueType(SqlType.DateTime));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      t.CreateColumn("staff_id", new SqlValueType("TINYINT UNSIGNED"));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_rental", t.TableColumns["rental_id"]);
      t.CreateUniqueConstraint("unique_rental_date_inventory_id_customer_id", t.TableColumns["rental_date"], t.TableColumns["inventory_id"], t.TableColumns["customer_id"]);
      t.CreateIndex("idx_rental_inventory_id").CreateIndexColumn(t.TableColumns["inventory_id"]);
      t.CreateIndex("idx_rental_customer_id").CreateIndexColumn(t.TableColumns["customer_id"]);
      t.CreateIndex("idx_rental_staff_id").CreateIndexColumn(t.TableColumns["staff_id"]);
      foreignKey = t.CreateForeignKey("fk_rental_staff");
      foreignKey.ReferencedTable = sch.Tables["staff"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["staff_id"]);
      foreignKey.Columns.Add(t.TableColumns["staff_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_rental_inventory");
      foreignKey.ReferencedTable = sch.Tables["inventory"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["inventory_id"]);
      foreignKey.Columns.Add(t.TableColumns["inventory_id"]);

      t = sch.CreateTable("payment");
      t.CreateColumn("payment_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("customer_id", new SqlValueType("SMALLINT UNSIGNED"));
      t.CreateColumn("staff_id", new SqlValueType("TINYINT UNSIGNED"));
      column = t.CreateColumn("rental_id", new SqlValueType(SqlType.Int32));
      column.DefaultValue = SqlDml.Native("NULL");
      column.IsNullable = true;
      t.CreateColumn("amount", new SqlValueType(SqlType.Decimal, 5, 2));
      t.CreateColumn("payment_date", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("last_update", new SqlValueType("TIMESTAMP"));
      t.CreatePrimaryKey("pk_payment", t.TableColumns["payment_id"]);
      t.CreateIndex("idx_payment_staff_id").CreateIndexColumn(t.TableColumns["staff_id"]);
      t.CreateIndex("idx_payment_customer_id").CreateIndexColumn(t.TableColumns["customer_id"]);
      foreignKey = t.CreateForeignKey("fk_payment_staff");
      foreignKey.ReferencedTable = sch.Tables["staff"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["staff_id"]);
      foreignKey.Columns.Add(t.TableColumns["staff_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_payment_customer");
      foreignKey.ReferencedTable = sch.Tables["customer"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["customer_id"]);
      foreignKey.Columns.Add(t.TableColumns["customer_id"]);
      foreignKey.OnUpdate = ReferentialAction.Cascade;
      foreignKey.OnDelete = ReferentialAction.Restrict;
      foreignKey = t.CreateForeignKey("fk_payment_rental");
      foreignKey.ReferencedTable = sch.Tables["rental"];
      foreignKey.ReferencedColumns.Add(foreignKey.ReferencedTable.TableColumns["rental_id"]);
      foreignKey.Columns.Add(t.TableColumns["rental_id"]);

      v = sch.CreateView("customer_list", SqlDml.Native(@"SELECT cu.customer_id AS ID, CONCAT(cu.first_name, _utf8' ', cu.last_name) AS name, a.address AS address, a.postal_code AS `zip code`,
a.phone AS phone, city.city AS city, country.country AS country, IF(cu.active, _utf8'active',_utf8'') AS notes, cu.store_id AS SID
FROM customer AS cu JOIN address AS a ON cu.address_id = a.address_id JOIN city ON a.city_id = city.city_id
	JOIN country ON city.country_id = country.country_id"));

      using (var cmd = sqlConnection.CreateCommand())
      {
        SqlBatch batch = SqlDml.Batch();
        batch.Add(SqlDdl.Drop(droppedSchema));
        //batch.Add(SqlDdl.Create(sch));
        cmd.CommandText = sqlDriver.Compile(SqlDdl.Drop(droppedSchema)).GetCommandText();
        cmd.ExecuteNonQuery();
        batch.Clear();
        batch.Add(SqlDdl.Create(sch));
        cmd.CommandText = sqlDriver.Compile(batch).GetCommandText();
        cmd.ExecuteNonQuery();
      }
      sqlConnection.Close();


      t = Catalog.Schemas["Sakila"].CreateTable("Malisa");
      t.CreateColumn("TransactionID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ProductID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ReferenceOrderLineID", new SqlValueType(SqlType.Int32));
      t.CreateColumn("TransactionDate", new SqlValueType(SqlType.DateTime));
      t.CreateColumn("TransactionType", new SqlValueType(SqlType.VarChar));
      t.CreateColumn("Quantity", new SqlValueType(SqlType.Int32));
      t.CreateColumn("ActualCost", new SqlValueType(SqlType.Double));
      t.CreateColumn("ModifiedDate", new SqlValueType(SqlType.DateTime));


      //SqlDriver mssqlDriver = new MssqlDriver(new MssqlVersionInfo(new Version()));
      //v = Catalog.Schemas["HumanResources"].CreateView("vEmployee",
      //        Sql.Native(mssqlDriver.Compile(select).CommandText));
      //      bmp.Save(model);
    }
  }
}
