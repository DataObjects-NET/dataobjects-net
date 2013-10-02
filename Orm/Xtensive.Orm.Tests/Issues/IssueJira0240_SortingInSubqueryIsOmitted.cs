﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.25

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0240_SortingInSubqueryIsOmittedModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0240_SortingInSubqueryIsOmittedModel
{
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class StoredContainer : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Container Container { get; set; }

    [Field]
    public string Address { get; set; }

    [Field]
    public DateTime CreationTime { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0240_SortingInSubqueryIsOmitted : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {
        var c = new Container();
        new StoredContainer {Container = c, CreationTime = new DateTime(2012, 1, 1), Address = "1"};
        new StoredContainer {Container = c, CreationTime = new DateTime(2012, 1, 2), Address = "2"};
        new StoredContainer {Container = c, CreationTime = new DateTime(2012, 1, 3), Address = "3"};
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {
        var r = Query.All<Container>()
          .Select(c => new {
            c.Id,
            LastLocation = Query.All<StoredContainer>()
              .Where(s => s.Container==c)
              .OrderByDescending(s => s.CreationTime)
              .Select(s => s.Address)
              .FirstOrDefault()
          })
          .OrderBy(c => c.Id)
          .Single();
        Assert.That(r.LastLocation, Is.EqualTo("3"));
      }
    }

    [Test]
    public void RegressionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var container = Query.All<Container>().Single();
        var now = DateTime.Now;
        var lastStoredContainer = Query.All<StoredContainer>()
          .Where(s => s.Container==container && s.CreationTime <= now)
          .GroupBy(s => s.CreationTime)
          .OrderByDescending(s => s.Key)
          .FirstOrDefault();

        Assert.That(lastStoredContainer, Is.Not.Null);
        Assert.That(lastStoredContainer.Key.Day, Is.EqualTo(3));
      }
    }
  }
}