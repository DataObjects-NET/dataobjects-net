// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.09.19

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.ObjectModel.ChinookDO
{
  [HierarchyRoot]
  [DebuggerDisplay("{Title} (AlbumId = {AlbumId})")]
  public class Album : Entity
  {
    [Field, Key]
    public int AlbumId { get; private set; }

    [FullText("English")]
    [Field(Nullable = false, Length = 160)]
    public string Title { get; set; }

    [Field(Nullable = false)]
    public Artist Artist { get; set; }
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (ArtistId = {ArtistId})")]
  public class Artist : Entity
  {
    [Field, Key]
    public int ArtistId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }
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

    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }

    [Field(Length = 60)]
    public string Email { get; set; }

    [Field]
    public Employee SupportRep { get; set; }

    [Field, Association(PairTo = "Customer")]
    public EntitySet<Invoice> Invoices { get; private set; }
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
    public DateTime? BirthDate { get; set; }

    [Field]
    public DateTime? HireDate { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }

    [Field(Length = 60)]
    public string Email { get; set; }

    [Field]
    public Employee ReportsToManager { get; set; }

    public int? GetAge() => BirthDate.HasValue ? DateTime.Now.Year - BirthDate.Value.Year : default;
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (GenreId = {GenreId})")]
  public class Genre : Entity
  {
    [Field, Key]
    public int GenreId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  [DebuggerDisplay("InvoiceId = {InvoiceId}")]
  public class Invoice : Entity
  {
    [Field, Key]
    public int InvoiceId { get; private set; }

    [Field(Nullable = false)]
    public DateTime InvoiceDate { get; set; }

    [Field(Nullable = false)]
    public InvoiceStatus Status { get; set; }

    [Field]
    public TimeSpan? ProcessingTime { get; set; }

    [Field]
    public Address BillingAddress { get; set; }

    [Field(Nullable = false)]
    public decimal Total { get; set; }

    [Field]
    public decimal? Commission { get; set; }

    [Field, Association(PairTo = "Invoice")]
    public EntitySet<InvoiceLine> InvoiceLines { get; private set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Employee DesignatedEmployee { get; set; }
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
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (MediaTypeId = {MediaTypeId})")]
  public class MediaType : Entity
  {
    [Field, Key]
    public int MediaTypeId { get; private set; }

    [Field(Nullable = false, Length = 120)]
    public string Name { get; set; }
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
  }

  [HierarchyRoot]
  [DebuggerDisplay("{Name} (TrackId = {TrackId})")]
  public class Track : Entity
  {
    [Field, Key]
    public int TrackId { get; private set; }

    [FullText("English")]
    [Field(Nullable = false, Length = 200)]
    public string Name { get; set; }

    [FullText("English")]
    [Field(Length = 220)]
    public string Composer { get; set; }

    [Field]
    public int Milliseconds { get; set; }

    [Field(Nullable = false)]
    public byte[] Bytes { get; set; }

    [Field]
    public decimal UnitPrice { get; set; }

    [Field]
    public Album Album { get; set; }

    [Field]
    public MediaType MediaType { get; set; }

    [Field]
    public Genre Genre { get; set; }

    [Field, Association(PairTo = "Tracks")]
    public EntitySet<Playlist> Playlists { get; private set; }
  }

  public class Address : Structure
  {
    [FullText("English")]
    [Field(Length = 70)]
    public string StreetAddress { get; set; }

    [Field(Length = 40)]
    public string City { get; set; }

    [Field(Length = 40)]
    public string State { get; set; }

    [Field(Length = 40)]
    public string Country { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    public string JustAProperty { get; set; }
  }

  public enum InvoiceStatus
  {
    Created = 0,
    Unpaid = 1,
    Paid = 2,
    Completed = 3
  }
}