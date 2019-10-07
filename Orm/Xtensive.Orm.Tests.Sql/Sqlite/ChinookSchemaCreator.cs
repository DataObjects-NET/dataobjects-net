// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.03

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql.Sqlite
{
  internal class ChinookSchemaCreator
  {
    private class XmlTable
    {
      public string Name { get; set; }
      public Dictionary<string, string> ColumnTypes { get; set; }
      public IEnumerable<XElement> Rows { get; set; }
    }

    private readonly SqlDriver driver;

    public void DropSchemaContent(SqlConnection connection, Schema defaultSchema)
    {
      DropTables(connection, defaultSchema);
    }

    public void CreateSchemaContent(SqlConnection connection, Schema defaultSchema)
    {
      CreateTables(connection, defaultSchema);
      CreateViews(connection, defaultSchema);
      FillTables(connection, defaultSchema);
    }

    private void DropTables(SqlConnection connection, Schema defaultSchema)
    {
      foreach (var tableName in GetTableNamesToDrop()) {
        var existingTable = defaultSchema.Tables[tableName];
        if (existingTable==null)
          continue;
        ExecuteDropTable(connection, existingTable);
        defaultSchema.Tables.Remove(existingTable);
      }
    }

    private void DropViews(SqlConnection connection, Schema defaultSchema)
    {
      var existingView = defaultSchema.Views["Invoice Subtotals"];
      if (existingView==null)
        return;
      ExecuteDropView(connection, existingView);
      defaultSchema.Views.Remove(existingView);

    }

    private void CreateTables(SqlConnection connection, Schema defaultSchema)
    {
      var t = defaultSchema.CreateTable("Employee");

      CreateInt32Column(t, "EmployeeId", false);
      CreateStringColumn(t, "Title", true, 30);
      CreateDateTimeColumn(t, "BirthDate", true);
      CreateDateTimeColumn(t, "HireDate", true);
      CreateInt32Column(t, "ReportsTo", true);
      CreateStringColumn(t, "LastName", false, 20);
      CreateStringColumn(t, "FirstName", false, 20);
      CreateStringColumn(t, "StreetAddress", true, 70);
      CreateStringColumn(t, "City", true, 40);
      CreateStringColumn(t, "State", true, 40);
      CreateStringColumn(t, "Country", true, 40);
      CreateStringColumn(t, "PostalCode", true, 10);
      CreateStringColumn(t, "Phone", true, 24);
      CreateStringColumn(t, "Fax", true, 24);
      CreateStringColumn(t, "Email", true, 60);
      t.CreatePrimaryKey("PK_Employee", t.TableColumns["EmployeeId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Customer");
      CreateInt32Column(t, "CustomerId", false);
      CreateInt32Column(t, "SupportRepId", false);
      CreateStringColumn(t, "CompanyName", true, 80);
      CreateStringColumn(t, "LastName", true, 20);
      CreateStringColumn(t, "FirstName", true, 20);
      CreateStringColumn(t, "StreetAddress", true, 70);
      CreateStringColumn(t, "City", true, 40);
      CreateStringColumn(t, "State", true, 40);
      CreateStringColumn(t, "Country", true, 40);
      CreateStringColumn(t, "PostalCode", true, 10);
      CreateStringColumn(t, "Phone", true, 24);
      CreateStringColumn(t, "Fax", true, 24);
      CreateStringColumn(t, "Email", true, 60);
      t.CreatePrimaryKey("PK_Customer", t.TableColumns["CustomerId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Album");
      CreateInt32Column(t, "AlbumId", false);
      CreateStringColumn(t, "Title", false, 160);
      CreateInt32Column(t, "ArtistId", true);
      t.CreatePrimaryKey("PK_Album", t.TableColumns["AlbumId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Artist");
      CreateInt32Column(t, "ArtistId", false);
      CreateStringColumn(t, "Name", false, 120);
      t.CreatePrimaryKey("PK_Artist", t.TableColumns["ArtistId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Genre");
      CreateInt32Column(t, "GenreId", false);
      CreateStringColumn(t, "Name", false, 120);
      t.CreatePrimaryKey("PK_Genre", t.TableColumns["GenreId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Invoice");
      CreateInt32Column(t, "InvoiceId", false);
      CreateDateTimeColumn(t, "InvoiceDate", false);
      CreateDateTimeColumn(t, "PaymentDate", true);
      CreateInt32Column(t, "Status", false);
      CreateInt64Column(t, "ProcessingTime", true);
      CreateStringColumn(t, "BillingStreetAddress", true, 70);
      CreateStringColumn(t, "BillingCity", true, 40);
      CreateStringColumn(t, "BillingState", true, 40);
      CreateStringColumn(t, "BillingCountry", true, 40);
      CreateStringColumn(t, "BillingPostalCode", true, 10);
      CreateDecimalColumn(t, "Total", false, 38, 19);
      CreateDecimalColumn(t, "Commission", true, 38, 19);
      CreateInt32Column(t, "CustomerId", true);
      CreateInt32Column(t, "DesignatedEmployeeId", true);
      t.CreatePrimaryKey("PK_Invoice", t.TableColumns["InvoiceId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("InvoiceLine");
      CreateInt32Column(t, "InvoiceLineId", false);
      CreateDecimalColumn(t, "UnitPrice", false, 38, 19);
      CreateInt16Column(t, "Quantity", false);
      CreateInt32Column(t, "InvoiceId", false);
      CreateInt32Column(t, "TrackId", false);

      t.CreatePrimaryKey("PK_InvoiceLine", t.TableColumns["InvoiceLineId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("MediaType");
      CreateInt32Column(t, "MediaTypeId", false);
      CreateStringColumn(t, "Name", false, 120);
      t.CreatePrimaryKey("PK_MediaType", t.TableColumns["MediaTypeId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Playlist");
      CreateInt32Column(t, "PlaylistId", false);
      CreateStringColumn(t, "Name", false, 120);
      t.CreatePrimaryKey("PK_Playlist", t.TableColumns["PlaylistId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("Track");
      CreateInt32Column(t, "TrackId", false);
      CreateStringColumn(t, "Name", false, 200);
      CreateStringColumn(t, "Composer", true, 220);
      CreateInt32Column(t, "Milliseconds", false);
      CreateByteArrayColumn(t, "Bytes", true, 4000);
      CreateDecimalColumn(t, "UnitPrice", true, 38, 19);
      CreateInt32Column(t, "AlbumId", true);
      CreateInt32Column(t, "MediaTypeId", true);
      CreateInt32Column(t, "GenreId", true);
      CreateInt32Column(t, "MediaFormat", true);
      t.CreatePrimaryKey("PK_Track", t.TableColumns["TrackId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);

      t = defaultSchema.CreateTable("PlaylistTrack");
      CreateInt32Column(t, "PlaylistId", false);
      CreateInt32Column(t, "TrackId", false);
      t.CreatePrimaryKey("PK_PlaylistTrack", t.TableColumns["PlaylistId"], t.TableColumns["TrackId"]).IsClustered = true;
      ExecuteCreateTable(connection, t);
    }

    private void CreateViews(SqlConnection connection, Schema defaultSchema)
    {
      var v = defaultSchema.CreateView("Invoice Subtotals");
      v.Definition = new SqlNative(
        @"SELECT [InvoiceLine].InvoiceId, 
                 Sum([InvoiceLine].UnitPrice) AS Subtotal
          FROM [InvoiceLine]
          GROUP BY [InvoiceLine].InvoiceID");

      ExecuteCreateView(connection, v);
    }

    private void FillTables(SqlConnection connection, Schema defaultSchema)
    {
      // as long as Xtensive.Orm.Tests project contains Chinook.xml we don't need separate xml file
      var path = @"Chinook.xml";
      var xmlTables = ReadXml(path);
      foreach (var xmlTable in xmlTables) {
        var sqlTable = defaultSchema.Tables[xmlTable.Name];
        if (sqlTable==null)
          continue;
        foreach (var row in xmlTable.Rows)
          ExecuteInsert(connection, sqlTable, GetColumnValues(row, xmlTable.ColumnTypes));
      }
    }

    private static Dictionary<string, object> GetColumnValues(XElement row, Dictionary<string, string> columnTypes)
    {
      var fields = new Dictionary<string, object>();
      var elements = row.Elements().ToList();
      for (int i = 0; i < elements.Count(); i++) {
        var value = elements[i].Value;
        object obj = null;
        if (!string.IsNullOrEmpty(value)) {
          obj = ConvertFieldType(columnTypes[elements[i].Name.LocalName], elements[i].Value);
        }
        fields.Add(elements[i].Name.LocalName, obj);
      }
      return fields;
    }

    private static object ConvertFieldType(string columnType, string text)
    {
      var type = Type.GetType(columnType);
      switch (columnType) {
        case "System.Byte[]":
          return Convert.FromBase64String(text);
        case "System.Decimal":
          return Decimal.Parse(text, CultureInfo.InvariantCulture);
        case "System.Single":
          return Single.Parse(text, CultureInfo.InvariantCulture);
        case "System.DateTime":
          return DateTime.Parse(text);
        case "System.TimeSpan":
          return TimeSpan.FromTicks(Int64.Parse(text, CultureInfo.InvariantCulture));
        default:
          return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
      }
    }

    private static List<XmlTable> ReadXml(string path)
    {
      var doc = XDocument.Load(path);
      var root = doc.Element("root");
      if (root==null)
        throw new Exception("Read xml error");
      var tables = root.Elements();
      var list = new List<XmlTable>();

      foreach (var table in tables) {
        var xmlTable = new XmlTable();
        xmlTable.Name = table.Name.LocalName;
        xmlTable.ColumnTypes = table.Element("Columns").Elements().ToDictionary(key => key.Name.LocalName, value => value.Value);
        xmlTable.Rows = table.Element("Rows").Elements();
        list.Add(xmlTable);
      }
      return list;
    }

    private void ExecuteCreateTable(SqlConnection connection, Table table)
    {
      var createTableQuery = SqlDdl.Create(table);
      using (var command = connection.CreateCommand(createTableQuery))
        command.ExecuteNonQuery();
    }

    private void ExecuteCreateView(SqlConnection connection, View view)
    {
      var createViewQuery = SqlDdl.Create(view);
      using (var command = connection.CreateCommand(createViewQuery))
        command.ExecuteNonQuery();
    }

    private void ExecuteDropTable(SqlConnection connection, Table table)
    {
      var createTableQuery = SqlDdl.Drop(table);
      using (var command = connection.CreateCommand(createTableQuery))
        command.ExecuteNonQuery();
    }

    private void ExecuteDropView(SqlConnection connection, View view)
    {
      var createViewQuery = SqlDdl.Drop(view);
      using (var command = connection.CreateCommand(createViewQuery))
        command.ExecuteNonQuery();
    }

    private void ExecuteInsert(SqlConnection connection, Table table, Dictionary<string, object> values)
    {
      var tableRef = SqlDml.TableRef(table);
      SqlInsert insertQuery = SqlDml.Insert(tableRef);
      foreach (var nameValue in values) {
        if (nameValue.Value!=null)
          insertQuery.Values.Add(tableRef[nameValue.Key], SqlDml.Literal(nameValue.Value));
        else
          insertQuery.Values.Add(tableRef[nameValue.Key], SqlDml.Null);
      }

      using (var command = connection.CreateCommand(insertQuery)) {
        Console.WriteLine(command.CommandText);
        command.ExecuteNonQuery();
      }
    }


    private void CreateInt16Column(Table table, string name, bool nullable)
    {
      CreateTableColumn<short>(table, name, nullable, null, null, null);
    }

    private void CreateInt32Column(Table table, string name, bool nullable)
    {
      CreateTableColumn<int>(table, name, nullable, null, null, null);
    }

    private void CreateInt64Column(Table table, string name, bool nullable)
    {
      CreateTableColumn<long>(table, name, nullable, null, null, null);
    }

    private void CreateStringColumn(Table table, string name, bool nullable, int? length)
    {
      CreateTableColumn<string>(table, name, nullable, length, null, null);
    }

    private void CreateDateTimeColumn(Table table, string name, bool nullable)
    {
      CreateTableColumn<DateTime>(table, name, nullable, null, null, null);
    }

    private void CreateByteArrayColumn(Table table, string name, bool nullable, int length)
    {
      CreateTableColumn<byte[]>(table, name, nullable, length, null, null);
    }

    private void CreateDecimalColumn(Table table, string name, bool nullable, int? precision, int? scale)
    {
      CreateTableColumn<decimal>(table, name, nullable, null, precision, scale);
    }

    private TableColumn CreateTableColumn<T>(Table table, string name, bool isNullable, int? lenght, int? precision, int? scale)
    {
      var type = typeof(T);
      var sqlType = driver.TypeMappings.GetMapping(type).MapType(lenght, precision, scale);
      var column = table.CreateColumn(name, sqlType);
      column.IsNullable = isNullable;
      return column;
    }

    private IEnumerable<string> GetTableNamesToDrop()
    {
      return new string[] {
        "Album",
        "Artist",
        "Genre",
        "InvoiceLine",
        "Track",
        "MediaType",
        "Playlist",
        "Invoice",
        "Employee",
        "Customer",
        "Region",
        "PlaylistTrack",
      };
    }

    public ChinookSchemaCreator(SqlDriver driver)
    {
      this.driver = driver;
    }
  }
}