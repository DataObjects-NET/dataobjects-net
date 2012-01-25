// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueJira0071_DistinctDoesNotWorkProperlyModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0071_DistinctDoesNotWorkProperlyModel
{
  [HierarchyRoot]
  public class Jira0071Entity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public int? Index { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{

  public class IssueJira0071_DistinctDoesNotWorkProperly : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Jira0071Entity).Assembly, typeof (Jira0071Entity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        new Jira0071Entity {Text = "Some text", Index = 1};
        new Jira0071Entity {Text = "Some text", Index = 1};
        new Jira0071Entity {Text = "Some text", Index = 2};
        new Jira0071Entity {Text = "Some text", Index = null};
        new Jira0071Entity {Text = "Some text", Index = 1};
        new Jira0071Entity {Text = "Other text", Index = 1};
        new Jira0071Entity {Text = "Other text", Index = null};
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {

        var q =
          from x in Query.All<Jira0071Entity>()
          select new {x.Text, OneOrNull = x.Index==null ? true : x.Index==1 ? true : false};

        var r = q.Distinct().OrderBy(x => x.Text).ThenBy(x => x.OneOrNull).ToList();

        Assert.That(r.Count, Is.EqualTo(3));
        Assert.That(r[0].Text, Is.EqualTo("Other text"));
        Assert.That(r[0].OneOrNull, Is.EqualTo(true));
        Assert.That(r[1].Text, Is.EqualTo("Some text"));
        Assert.That(r[1].OneOrNull, Is.EqualTo(false));
        Assert.That(r[2].Text, Is.EqualTo("Some text"));
        Assert.That(r[2].OneOrNull, Is.EqualTo(true));
      }
    }
  }
}