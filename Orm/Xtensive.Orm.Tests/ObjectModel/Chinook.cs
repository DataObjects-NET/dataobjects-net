// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.09.19

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.ObjectModel.Chinook
{
  [HierarchyRoot]
  [DebuggerDisplay("{Title} (AlbumId = {AlbumId})")]
  public class Album : Entity
  {
    [Field, Key]
    public int AlbumId { get; private set; }

    [Field(Nullable = false, Length = 160)]
    public string Title { get; set; }

    [Field(Nullable = false)]
    public Artist Artist { get; set; }

    public Album()
    {
    }

    public Album(int albumId)
      : base(albumId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (ArtistId = {ArtistId})")]
  public class Artist : Entity
  {
    [Field, Key]
    public int ArtistId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }

    public Artist()
    {
    }

    public Artist(int artistId)
      : base(artistId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{FirstName} {LastName} (CustomerId = {CustomerId})")]
  public class Customer : Entity
  {
    [Field, Key]
    public int CustomerId { get; private set; }

    [Field(Nullable = false, Length = 20)]
    public string FirstName { get; set; }

    [Field(Nullable = false, Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 80)]
    public string Company { get; set; }

    [Field(Length = 70)]
    public string Address { get; set; }

    [Field(Length = 40)]
    public string City { get; set; }

    [Field(Length = 40)]
    public string State { get; set; }

    [Field(Length = 40)]
    public string Country { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }

    [Field(Length = 60)]
    public string Email { get; set; }

    [Field]
    public Employee SupportRep { get; set; }

    public Customer()
    {
    }

    public Customer(int customerId)
      : base(customerId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{FirstName} {LastName} (EmployeeId = {EmployeeId})")]
  public class Employee : Entity
  {
    [Field, Key]
    public int EmployeeId { get; private set; }

    [Field(Nullable = false, Length = 20)]
    public string LastName { get; set; }

    [Field(Nullable = false, Length = 20)]
    public string FirstName { get; set; }

    [Field(Length = 30)]
    public string Title { get; set; }

    [Field]
    public DateTime BirthDate { get; set; }

    [Field]
    public DateTime HireDate { get; set; }

    [Field(Length = 70)]
    public string Address { get; set; }

    [Field(Length = 40)]
    public string City { get; set; }

    [Field(Length = 40)]
    public new string State { get; set; }

    [Field(Length = 40)]
    public string Country { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }

    [Field(Length = 60)]
    public string Email { get; set; }

    [Field]
    public Employee ReportsToManager { get; set; }

    public Employee()
    {
    }

    public Employee(int employeeId)
      : base(employeeId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (GenreId = {GenreId})")]
  public class Genre : Entity
  {
    [Field, Key]
    public int GenreId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }

    public Genre()
    {
    }

    public Genre(int genreId)
      : base(genreId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("InvoiceId = {InvoiceId}")]
  public class Invoice : Entity
  {
    [Field, Key]
    public int InvoiceId { get; private set; }

    [Field(Nullable = false)]
    public DateTime InvoiceDate { get; set; }

    [Field(Length = 70)]
    public string BillingAddress { get; set; }

    [Field(Length = 40)]
    public string BillingCity { get; set; }

    [Field(Length = 40)]
    public string BillingState { get; set; }

    [Field(Length = 40)]
    public string BillingCountry { get; set; }

    [Field(Length = 10)]
    public string BillingPostalCode { get; set; }

    [Field(Nullable = false)]
    public decimal Total { get; set; }

    [Field, Association(PairTo = "Invoice")]
    public EntitySet<InvoiceLine> InvoiceLines { get; private set; }

    [Field(Nullable = false)]
    public Customer Customer { get; set; }

    public Invoice()
    {
    }

    public Invoice(int invoiceId)
      : base(invoiceId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("InvoiceLineId = {InvoiceLineId}")]
  public class InvoiceLine : Entity
  {
    [Field, Key]
    public int InvoiceLineId { get; private set; }

    [Field(Nullable = false)]
    public decimal UnitPrice { get; set; }

    [Field(Nullable = false)]
    public int Quantity { get; set; }

    [Field(Nullable = false)]
    public Invoice Invoice { get; set; }

    [Field(Nullable = false)]
    public Track Track { get; set; }

    public InvoiceLine()
    {
    }

    public InvoiceLine(int invoiceLineId)
      : base(invoiceLineId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (MediaTypeId = {MediaTypeId})")]
  public class MediaType : Entity
  {
    [Field, Key]
    public int MediaTypeId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }

    public MediaType()
    {
    }

    public MediaType(int mediaTypeId)
      : base(mediaTypeId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (PlaylistId = {PlaylistId})")]
  public class Playlist : Entity
  {
    [Field, Key]
    public int PlaylistId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }

    [Field]
    public EntitySet<Track> Tracks { get; private set; }

    public Playlist()
    {
    }

    public Playlist(int playlistId)
      : base(playlistId)
    {
    }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (TrackId = {TrackId})")]
  public class Track : Entity
  {
    [Field, Key]
    public int TrackId { get; private set; }

    [Field(Nullable = false, Length = 200)]
    public string Name { get; set; }

    [Field(Length = 220)]
    public string Composer { get; set; }

    [Field]
    public int Milliseconds { get; set; }

    [Field]
    public int Bytes { get; set; }

    [Field]
    public decimal UnitPrice { get; set; }

    [Field]
    public Album Album { get; set; }

    [Field]
    public MediaType MediaType { get; set; }

    [Field]
    public Genre Genre { get; set; }

    public Track()
    {
    }

    public Track(int trackId)
      : base(trackId)
    {
    }
  }
}