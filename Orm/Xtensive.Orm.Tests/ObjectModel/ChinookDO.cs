// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.09.19

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Xtensive.Orm.Configuration;
using IdEntity = System.ValueTuple<int,Xtensive.Orm.Entity>;

namespace Xtensive.Orm.Tests.ObjectModel.ChinookDO
{
  internal abstract class Importer
  {
    protected readonly ImportContext context;

    public void Import(dynamic data)
    {
      foreach (object entryData in data) {
        var (id, entity) = ImportEntry(entryData);
        if (entity!=null)
          context.AddEntity(id, entity);
      }
    }

    protected abstract IdEntity ImportEntry(dynamic data);

    protected Importer(ImportContext context)
    {
      this.context = context;
    }
  }

  internal class ArtistImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var artistId = (int) data.ArtistId;
      return (artistId, new Artist(artistId) {
        Name = data.Name
      });
    }

    public ArtistImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class GenreImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var genreId = (int) data.GenreId;
      return (genreId, new Genre(genreId) {
        Name = data.Name
      });
    }

    public GenreImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class MediaTypeImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var mediaTypeId = (int) data.MediaTypeId;
      return (mediaTypeId, new MediaType(mediaTypeId) {
        Name = data.Name
      });
    }

    public MediaTypeImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class PlaylistImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var playlistId = (int) data.PlaylistId;
      return (playlistId, new Playlist(playlistId) {
        Name = data.Name
      });
    }

    public PlaylistImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class AlbumImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var albumId = (int) data.AlbumId;
      return (albumId, new Album(albumId) {
        Title = data.Title,
        Artist = context.GetEntity<Artist>((int) data.ArtistId)
      });
    }

    public AlbumImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class TrackImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var trackId = (int) data.TrackId;
      return (trackId, new Track(trackId) {
        Name = data.Name,
        Composer = data.Composer,
        Milliseconds = data.Milliseconds,
        Bytes = data.Bytes,
        UnitPrice = data.UnitPrice,
        Album = context.GetEntity<Album>((int) data.AlbumId),
        MediaType = context.GetEntity<MediaType>((int) data.MediaTypeId),
        Genre = context.GetEntity<Genre>((int) data.GenreId)
      });
    }

    public TrackImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class EmployeeImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var employeeId = (int) data.EmployeeId;
      var reportsTo = (int?) data.ReportsTo;
      return (employeeId, new Employee(employeeId) {
        LastName = data.LastName,
        FirstName = data.FirstName,
        Title = data.Title,
        BirthDate = data.BirthDate,
        HireDate = data.HireDate,
        Address = new Address {
          StreetAddress = data.Address,
          City = data.City,
          State = data.State,
          Country = data.Country,
          PostalCode = data.PostalCode,
        },
        Phone = data.Phone,
        Fax = data.Fax,
        Email = data.Email,
        ReportsToManager = reportsTo.HasValue ? context.GetEntity<Employee>(reportsTo.Value) : null
      });
    }

    public EmployeeImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class CustomerImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var customerId = (int) data.CustomerId;
      return (customerId, new Customer(customerId) {
        FirstName = data.FirstName,
        LastName = data.LastName,
        Company = data.Company,
        Address = new Address {
          StreetAddress = data.Address,
          City = data.City,
          State = data.State,
          Country = data.Country,
          PostalCode = data.PostalCode,
        },
        Phone = data.Phone,
        Fax = data.Fax,
        Email = data.Email,
        SupportRep = context.GetEntity<Employee>((int) data.SupportRepId),
      });
    }

    public CustomerImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class InvoiceImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var invoiceId = (int) data.InvoiceId;
      return (invoiceId, new Invoice(invoiceId) {
        InvoiceDate = data.InvoiceDate,
        BillingAddress = new Address {
          StreetAddress = data.BillingAddress,
          City = data.BillingCity,
          State = data.BillingState,
          Country = data.BillingCountry,
          PostalCode = data.BillingPostalCode,
        },
        Total = data.Total,
        Commission = data.Commission,
        Customer = context.GetEntity<Customer>((int) data.CustomerId),
        Employee = context.GetEntity<Employee>((int) data.EmployeeId),
      });
    }

    public InvoiceImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class InvoiceLineImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var invoiceLineId = (int) data.InvoiceLineId;
      return (invoiceLineId, new InvoiceLine(invoiceLineId) {
        UnitPrice = data.UnitPrice,
        Quantity = data.Quantity,
        Invoice = context.GetEntity<Invoice>((int) data.InvoiceId),
        Track = context.GetEntity<Track>((int) data.TrackId),
      });
    }

    public InvoiceLineImporter(ImportContext context)
      : base(context)
    {
    }
  }

  internal class PlaylistTrackImporter : Importer
  {
    protected override IdEntity ImportEntry(dynamic data)
    {
      var playlist = context.GetEntity<Playlist>((int) data.PlaylistId);
      var track = context.GetEntity<Track>((int) data.TrackId);
      playlist.Tracks.Add(track);
      return default;
    }

    public PlaylistTrackImporter(ImportContext context)
      : base(context)
    {
    }
  }

  public sealed class ImportContext
  {
    private readonly Dictionary<Type, Dictionary<int, Entity>> entities = new Dictionary<Type, Dictionary<int, Entity>>();

    public T GetEntity<T>(int id) where T : Entity => (T) entities[typeof (T)][id];

    public void AddEntity(int id, Entity entity)
    {
      var entityType = entity.GetType();
      if (!entities.TryGetValue(entityType, out var values))
        entities.Add(entityType, values = new Dictionary<int, Entity>());
      values.Add(id, entity);
    }
  }

  public class DatabaseFiller
  {
    public static void Fill(Domain domain)
    {
      const string chinookJsonFileName = "Chinook.json";

      using (var session = domain.OpenSession(new SessionConfiguration("Legacy", SessionOptions.ServerProfile | SessionOptions.AutoActivation)))
      using (var tr = session.OpenTransaction(System.Transactions.IsolationLevel.ReadCommitted)) {
        var context = new ImportContext();
        var json = File.ReadAllText(chinookJsonFileName);
        dynamic data = JsonConvert.DeserializeObject(json);

        new ArtistImporter(context).Import(data.Artist);
        new GenreImporter(context).Import(data.Genre);
        new MediaTypeImporter(context).Import(data.MediaType);
        new PlaylistImporter(context).Import(data.Playlist);
        new AlbumImporter(context).Import(data.Album);
        new TrackImporter(context).Import(data.Track);
        new EmployeeImporter(context).Import(data.Employee);
        new CustomerImporter(context).Import(data.Customer);
        new InvoiceImporter(context).Import(data.Invoice);
        new InvoiceLineImporter(context).Import(data.InvoiceLine);
        new PlaylistTrackImporter(context).Import(data.PlaylistTrack);

        Session.Current.SaveChanges();
        tr.Complete();
      }
    }
  }
}