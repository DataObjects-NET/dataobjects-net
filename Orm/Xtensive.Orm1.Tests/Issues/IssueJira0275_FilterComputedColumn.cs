// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.31

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0275_FilterComputedColumnModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0275_FilterComputedColumnModel
{
  [HierarchyRoot]
  public sealed class FilterTarget : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Code { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0275_FilterComputedColumn : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (FilterTarget).Assembly, typeof (FilterTarget).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new FilterTarget {Code = "AA123"};
        new FilterTarget {Code = "BB123"};
        new FilterTarget {Code = "AA321"};
        new FilterTarget {Code = "BB321"};
        new FilterTarget {Code = "AA777"};
        new FilterTarget {Code = "BB777"};
        tx.Complete();
      }
    }

    [Test]
    public void ContainsWithProjectionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Select(t => new {Target = t, Subcode = t.Code.Substring(2, 3)})
          .Where(item => codes.Contains(item.Subcode))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void ContainsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Where(t => codes.Contains(t.Code.Substring(2, 3)))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void InWithProjectionTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Select(t => new {Target = t, Subcode = t.Code.Substring(2, 3)})
          .Where(item => item.Subcode.In(codes))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void InTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Where(t => t.Code.Substring(2, 3).In(codes))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }
  }
}