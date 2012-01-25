// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.25

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueJira0213_GroupByNewDateModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0213_GroupByNewDateModel
{
  [HierarchyRoot]
  public class EntityWithDate : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public DateTime Date { get; set; }

    public override string ToString()
    {
      return Date.ToString("yyyy.MM.dd");
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0213_GroupByNewDate : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithDate).Assembly, typeof (EntityWithDate).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var now = DateTime.Now;
        for (int i = 1; i <= 10; i++)
          new EntityWithDate {Date = new DateTime(now.Year, now.Month, i)};
        tx.Complete();
      }
    }

    [Test]
    public void GroupByNewDateDirectly()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var q =
          from i in Query.All<EntityWithDate>()
          group i by new DateTime(i.Date.Year, i.Date.Month, 1) into g
          select g;
        Assert.That(q.Count(), Is.EqualTo(1));
      }
    }

    [Test]
    public void GroupByNewDateWithIntermediateProjection()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var q =
          from i in Query.All<EntityWithDate>()
          let x = new DateTime(i.Date.Year, i.Date.Month, 1)
          group i by x into g
          select g;
        Assert.That(q.Count(), Is.EqualTo(1));
      }
    }

    [Test]
    public void OrderByNewDateWithIntermediateProjection()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {

        var expected = Query.All<EntityWithDate>()
          .OrderBy(e => e.Date)
          .AsEnumerable()
          .Reverse()
          .ToList();

        var q =
          from i in Query.All<EntityWithDate>()
          let x = new DateTime(i.Date.Year, i.Date.Month, 11 - i.Date.Day) 
          orderby x
          select i;

        Assert.That(q.ToList(), Is.EqualTo(expected));
      }
    }

    [Test]
    public void OrderByNewDateDirectly()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {

        var expected = Query.All<EntityWithDate>()
          .OrderBy(e => e.Date)
          .AsEnumerable()
          .Reverse()
          .ToList();

        var q =
          from i in Query.All<EntityWithDate>() 
          orderby new DateTime(i.Date.Year, i.Date.Month, 11 - i.Date.Day)
          select i;

        Assert.That(q.ToList(), Is.EqualTo(expected));
      }
    }
  }
}