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
      return ((int) data.ArtistId, new Artist {
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
      return ((int) data.GenreId, new Genre {
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
      return ((int) data.MediaTypeId, new MediaType {
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
      return ((int) data.PlaylistId, new Playlist {
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
      return ((int) data.AlbumId, new Album {
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
      var mediaType = context.GetEntity<MediaType>((int) data.MediaTypeId);
      var track = CreateTrack(mediaType);
      track.Name = data.Name;
      track.Composer = data.Composer;
      track.Milliseconds = data.Milliseconds;
      track.Bytes = data.Bytes;
      track.UnitPrice = data.UnitPrice;
      track.Album = context.GetEntity<Album>((int) data.AlbumId);
      track.MediaType = mediaType;
      track.Genre = context.GetEntity<Genre>((int) data.GenreId);
      return ((int) data.TrackId, track);
    }

    private Track CreateTrack(MediaType mediaType)
    {
      if (mediaType.Name=="Protected MPEG-4 video file")
        return new VideoTrack();
      else
        return new AudioTrack();
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
      var reportsTo = (int?) data.ReportsTo;
      return ((int) data.EmployeeId, new Employee {
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
      return ((int) data.CustomerId, new Customer {
        FirstName = data.FirstName,
        LastName = data.LastName,
        CompanyName = data.Company,
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
      return ((int) data.InvoiceId, new Invoice() {
        InvoiceDate = data.InvoiceDate,
        PaymentDate = (DateTime?)data.PaymentDate,
        Status = (InvoiceStatus)data.Status,
        ProcessingTime = (TimeSpan?)data.ProcessingTime,
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
        DesignatedEmployee = context.GetEntity<Employee>((int) data.EmployeeId),
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
      return ((int) data.InvoiceLineId, new InvoiceLine {
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
      return default(IdEntity);
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
      using (var enumerator = GetAllEntityTypes(entity.GetType()).Reverse().GetEnumerator()) {
        enumerator.MoveNext();
        if (!entities.TryGetValue(enumerator.Current, out var values)) {
          entities.Add(enumerator.Current, values = new Dictionary<int, Entity>());
          while (enumerator.MoveNext())
            entities.Add(enumerator.Current, values);
        }

        values.Add(id, entity);
      }
    }

    private IEnumerable<Type> GetAllEntityTypes(Type type)
    {
      while (type!=typeof (Entity)) {
        yield return type;
        if(Attribute.IsDefined(type, typeof(HierarchyRootAttribute)))
          yield break;
        type = type.BaseType;
      }
    }
  }

  public class DataBaseFiller
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