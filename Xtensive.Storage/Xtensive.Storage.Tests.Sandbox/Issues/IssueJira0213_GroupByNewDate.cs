// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.25

using System;
using System.Linq;
using NUnit.Framework;
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
  }

}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0213_GroupByNewDate : AutoBuildTest
  {

    protected override global::Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithDate).Assembly, typeof (EntityWithDate).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var now = DateTime.Now;
        for (int i = 0; i < 50; i++)
          new EntityWithDate {Date = new DateTime(now.Year, now.Month, now.Day, 12, i, 0)};

        var query =
          from i in Query.All<EntityWithDate>()
          let startMonth = new DateTime(i.Date.Year, i.Date.Month, 1)
          group i by startMonth
          into g
          select g;

        Assert.That(query.Count(), Is.EqualTo(1));
      }
    }
  }
}