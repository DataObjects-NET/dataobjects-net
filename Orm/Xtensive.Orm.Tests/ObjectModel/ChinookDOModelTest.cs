// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.09.19

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.Chinook;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.ObjectModel
{
  [TestFixture]
  public abstract class ChinookDOModelTest : AutoBuildTest
  {
    private DisposableSet disposables;
    protected Session Session;

    private Album[] albums;
    private Artist[] artists;
    private Customer[] customers;
    private Employee[] employees;
    private Genre[] genres;
    private Invoice[] invoices;
    private InvoiceLine[] invoiceLines;
    private MediaType[] mediaTypes;
    private Playlist[] playlists;
    private Track[] tracks;

    protected IEnumerable<Album>       Albums       => GetEntities(ref albums);
    protected IEnumerable<Artist>      Artists      => GetEntities(ref artists);
    protected IEnumerable<Customer>    Customers    => GetEntities(ref customers);
    protected IEnumerable<Employee>    Employees    => GetEntities(ref employees);
    protected IEnumerable<Genre>       Genres       => GetEntities(ref genres);
    protected IEnumerable<Invoice>     Invoices     => GetEntities(ref invoices);
    protected IEnumerable<InvoiceLine> InvoiceLines => GetEntities(ref invoiceLines);
    protected IEnumerable<MediaType>   MediaTypes   => GetEntities(ref mediaTypes);
    protected IEnumerable<Playlist>    Playlists    => GetEntities(ref playlists);
    protected IEnumerable<Track>       Tracks       => GetEntities(ref tracks);

    [SetUp]
    public virtual void SetUp()
    {
      disposables = new DisposableSet();
      disposables.Add(Session = Domain.OpenSession());
      disposables.Add(Session.OpenTransaction());
    }

    [TearDown]
    public virtual void TearDown()
    {
      disposables.DisposeSafely();
      albums = null;
      artists = null;
      customers = null;
      employees = null;
      genres = null;
      invoices = null;
      invoiceLines = null;
      mediaTypes = null;
      playlists = null;
      tracks = null;
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Playlist).Assembly, typeof (Playlist).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      Domain domain;
      try {
        throw new ApplicationException("Don't validate, just recreate ;)");
        var validateConfig = configuration.Clone();
        validateConfig.UpgradeMode = DomainUpgradeMode.Validate;
        domain = Domain.Build(validateConfig);
      }
      catch (Exception ex) {
        var recreateConfig = configuration.Clone();
        recreateConfig.UpgradeMode = DomainUpgradeMode.Recreate;
        domain = base.BuildDomain(recreateConfig);
      }

      var shouldFill = false;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var count = session.Query.All<Playlist>().Count();
        if (count==0)
          shouldFill = true;
      }

      if (shouldFill)
        DatabaseFiller.Fill(domain);
      return domain;
    }

    private T[] GetEntities<T>(ref T[] source) where T : Entity => source ?? (source = Session.Query.All<T>().ToArray());
  }
}