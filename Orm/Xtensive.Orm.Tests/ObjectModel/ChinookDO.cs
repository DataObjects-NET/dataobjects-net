// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.09.19

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Xtensive.Orm.Configuration;
using IdEntity = System.ValueTuple<int,Xtensive.Orm.Entity>;

namespace Xtensive.Orm.Tests.ObjectModel.ChinookDO
{
  public sealed class ImportContext
  {
    private readonly Dictionary<Type, Dictionary<int, Entity>> entities = new Dictionary<Type, Dictionary<int, Entity>>();

    public T GetEntity<T>(int id) where T : Entity => (T) entities[typeof (T)][id];

    public void AddEntity<T>(int id, T entity)
      where T: Entity
    {
      Dictionary<int, Entity> values;
      if (!entities.TryGetValue(typeof (T), out values)) {
        values = new Dictionary<int, Entity>();
        entities.Add(typeof (T), values);
      }

      values[id] = entity;
    }
  }

  public class DataBaseFiller
  {
    private abstract class Importer
    {
      public abstract void Import(Dictionary<string, object> fields, ImportContext context);
    }

    private class ArtistImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new Artist {Name = (string) fields["Name"]};
        context.AddEntity((int) fields["ArtistId"], entity);
      }
    }

    private class GenreImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new Genre {Name = (string) fields["Name"]};
        context.AddEntity((int) fields["GenreId"], entity);
      }
    }

    private class MediaTypeImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new MediaType {Name = (string) fields["Name"]};
        context.AddEntity((int) fields["MediaTypeId"], entity);
      }
    }

    private class PlaylistImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new Playlist {Name = (string) fields["Name"]};
        context.AddEntity((int) fields["PlaylistId"], entity);
      }
    }

    private class AlbumImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new Album {
          Title = (string) fields["Title"],
          Artist = context.GetEntity<Artist>((int) fields["ArtistId"])
        };
        context.AddEntity((int) fields["AlbumId"], entity);
      }
    }

    private class TrackImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var mediaType = context.GetEntity<MediaType>((int) fields["MediaTypeId"]);
        var entity = CreateTrack(mediaType);
        entity.Name = (string) fields["Name"];
        entity.Composer = (string) fields["Composer"];
        entity.Milliseconds = (int) fields["Milliseconds"];
        entity.Bytes = (byte[]) fields["Bytes"];
        entity.UnitPrice = (decimal) fields["UnitPrice"];
        entity.Album = context.GetEntity<Album>((int) fields["AlbumId"]);
        entity.Genre = context.GetEntity<Genre>((int) fields["GenreId"]);

        context.AddEntity((int) fields["TrackId"], entity);
      }

      private Track CreateTrack(MediaType mediaType)
      {
        if (mediaType.Name=="Protected MPEG-4 video file")
          return new VideoTrack {MediaType = mediaType};
        return new AudioTrack {MediaType = mediaType};
      }
    }

    private class EmployeeImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var reportsTo = (int?) fields["ReportsTo"];
        var entity = new Employee{
          LastName = (string) fields["LastName"],
          FirstName = (string) fields["FirstName"],
          Title = (string) fields["Title"],
          BirthDate = (DateTime?) fields["BirthDate"],
          HireDate = (DateTime?) fields["HireDate"],
          Address = new Address {
            StreetAddress = (string) fields["StreetAddress"],
            City = (string) fields["City"],
            State = (string) fields["State"],
            Country = (string) fields["Country"],
            PostalCode = (string) fields["PostalCode"],
          },
          Phone = (string) fields["Phone"],
          Fax = (string) fields["Fax"],
          Email = (string) fields["Email"],
          ReportsToManager = reportsTo.HasValue ? context.GetEntity<Employee>(reportsTo.Value) : null
        };

        context.AddEntity((int) fields["EmployeeId"], entity);
      }
    }

    private class CustomerImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new Customer {
          FirstName = (string) fields["FirstName"],
          LastName = (string) fields["LastName"],
          CompanyName = (string) fields["CompanyName"],
          Address = new Address {
            StreetAddress = (string) fields["StreetAddress"],
            City = (string) fields["City"],
            State = (string) fields["State"],
            Country = (string) fields["Country"],
            PostalCode = (string) fields["PostalCode"],
          },
          Phone = (string) fields["Phone"],
          Fax = (string) fields["Fax"],
          Email = (string) fields["Email"],
          SupportRep = context.GetEntity<Employee>((int) fields["SupportRepId"]),
        };

        context.AddEntity((int) fields["CustomerId"], entity);
      }
    }

    private class InvoiceImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var entity = new Invoice {
          InvoiceDate = (DateTime) fields["InvoiceDate"],
          PaymentDate = (DateTime?) fields["PaymentDate"],
          Status = (InvoiceStatus) fields["Status"],
          ProcessingTime = (TimeSpan?) fields["ProcessingTime"],
          BillingAddress = new Address {
            StreetAddress = (string) fields["BillingStreetAddress"],
            City = (string) fields["BillingCity"],
            State = (string) fields["BillingState"],
            Country = (string) fields["BillingCountry"],
            PostalCode = (string) fields["BillingPostalCode"],
          },
          Total = (decimal) fields["Total"],
          Commission = (decimal) fields["Commission"],
          Customer = context.GetEntity<Customer>((int) fields["CustomerId"]),
          DesignatedEmployee = context.GetEntity<Employee>((int) fields["DesignatedEmployeeId"]),
        };
        context.AddEntity((int) fields["InvoiceId"], entity);
      }
    }

    private class InvoiceLineImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var invoiceLine = new InvoiceLine {
          UnitPrice = (decimal) fields["UnitPrice"],
          Quantity = (int) fields["Quantity"],
          Invoice = context.GetEntity<Invoice>((int) fields["InvoiceId"]),
          Track = context.GetEntity<Track>((int) fields["TrackId"])
        };
        context.AddEntity((int) fields["InvoiceLineId"], invoiceLine);
      }
    }

    private class PlaylistTrackImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext context)
      {
        var playlist = context.GetEntity<Playlist>((int) fields["PlaylistId"]);
        var track = context.GetEntity<Track>((int) fields["TrackId"]);
        playlist.Tracks.Add(track);
      }
    }

    public static void Fill(Domain domain)
    {
      var path = @"Chinook.xml";
      var xmlTables = ReadXml(path);

      var importContext = new ImportContext();

      using (var session = domain.OpenSession(new SessionConfiguration("Legacy", SessionOptions.ServerProfile | SessionOptions.AutoActivation)))
      using (var tr = session.OpenTransaction(System.Transactions.IsolationLevel.ReadCommitted)) {
        Import(xmlTables["Artist"], importContext, new ArtistImporter());
        Import(xmlTables["Genre"], importContext, new GenreImporter());
        Import(xmlTables["MediaType"], importContext, new MediaTypeImporter());
        Import(xmlTables["Playlist"], importContext, new PlaylistImporter());
        Import(xmlTables["Album"], importContext, new AlbumImporter());
        Import(xmlTables["Track"], importContext, new TrackImporter());
        Import(xmlTables["Employee"], importContext, new EmployeeImporter());
        Import(xmlTables["Customer"], importContext, new CustomerImporter());
        Import(xmlTables["Invoice"], importContext, new InvoiceImporter());
        Import(xmlTables["InvoiceLine"], importContext, new InvoiceLineImporter());
        Import(xmlTables["PlaylistTrack"], importContext, new PlaylistTrackImporter());

        session.SaveChanges();
        tr.Complete();
      }
    }

    private static void Import(XmlTable node, ImportContext importContext, Importer importer)
    {
      foreach (var row in node.Rows) {
        var fields = GetFields(row, node.ColumnTypes);
        importer.Import(fields, importContext);
      }
    }

    private static Dictionary<string, object> GetFields(XElement row, Dictionary<string, string> columnTypes)
    {
      var fields = new Dictionary<string, object>();
      var elements = row.Elements().ToList();
      for (int i = 0; i < elements.Count(); i++) {
        var value = elements[i].Value;
        object obj = null;
        if (!string.IsNullOrEmpty(value))
          obj = ConvertFieldType(columnTypes[elements[i].Name.LocalName], elements[i].Value);
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

    private static Dictionary<string, XmlTable> ReadXml(string path)
    {
      var doc = XDocument.Load(path);
      var root = doc.Element("root");
      if (root == null)
        throw new Exception("Read xml error");
      var tables = root.Elements();
      var tableMap = new Dictionary<string, XmlTable>();

      foreach (var table in tables) {
        var xmlTable = new XmlTable();
        xmlTable.Name = table.Name.LocalName;
        xmlTable.ColumnTypes = table.Element("Columns").Elements().ToDictionary(key => key.Name.LocalName, value => value.Value);
        xmlTable.Rows = table.Element("Rows").Elements();
        tableMap.Add(xmlTable.Name, xmlTable);
      }
      return tableMap;
    }

    private class XmlTable
    {
      public string Name { get; set; }
      public Dictionary<string, string> ColumnTypes { get; set; }
      public IEnumerable<XElement> Rows { get; set; }
    }
  }
}