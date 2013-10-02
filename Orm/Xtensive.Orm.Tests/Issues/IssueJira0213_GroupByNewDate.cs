// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.25

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0213_GroupByNewDateModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0213_GroupByNewDateModel
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

namespace Xtensive.Orm.Tests.Issues
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
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {
        var now = DateTime.Now;
        for (int i = 1; i <= 10; i++)
          new EntityWithDate {Date = new DateTime(now.Year, now.Month, i)};
        tx.Complete();
      }
    }

    [Test]
    public void GroupByNewDateDirectly()
    {
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {

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
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {

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
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {

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
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {

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

    [Test]
    public void WhereNewDateDirectly()
    {
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {

        var now = DateTime.Now;
        var firstDay = new DateTime(now.Year, now.Month, 1);

        var q =
          from i in Query.All<EntityWithDate>()
          where new DateTime(i.Date.Year, i.Date.Month, 11 - i.Date.Day)==firstDay 
          select i;

        Assert.That(q.Single().Date.Day, Is.EqualTo(10));
      }
    }

    [Test]
    public void WhereNewDateWithIntermediateProjection()
    {
      using (Domain.OpenSession())
      using (var tx = Session.Current.OpenTransaction()) {

        var now = DateTime.Now;
        var firstDay = new DateTime(now.Year, now.Month, 1);

        var q =
          from i in Query.All<EntityWithDate>()
          let x = new DateTime(i.Date.Year, i.Date.Month, 11 - i.Date.Day)
          where x==firstDay
          select i;

        Assert.That(q.Single().Date.Day, Is.EqualTo(10));
      }
    }
  }
}