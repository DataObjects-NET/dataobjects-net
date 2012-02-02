// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.31

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueJira0275_FilterComputedColumnModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0275_FilterComputedColumnModel
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

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        new FilterTarget {Code = "AA123"};
        new FilterTarget {Code = "BB123"};
        new FilterTarget {Code = "AA321"};
        new FilterTarget {Code = "BB321"};
        new FilterTarget {Code = "AA777"};
        new FilterTarget {Code = "BB777"};
      }
    }

    [Test]
    public void ContainsWithProjectionTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Select(t => new {Target = t, Subcode = t.Code.Substring(3, 2)})
          .Where(item => codes.Contains(item.Subcode))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void ContainsTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Where(t => codes.Contains(t.Code.Substring(3, 2)))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void InWithProjectionTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Select(t => new {Target = t, Subcode = t.Code.Substring(3, 2)})
          .Where(item => item.Subcode.In(codes))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }

    [Test]
    public void InTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var codes = new[] {"123", "321"};
        var q = Query.All<FilterTarget>()
          .Where(t => t.Code.Substring(3, 2).In(codes))
          .ToList();
        Assert.That(q.Count, Is.EqualTo(4));
      }
    }
  }
}