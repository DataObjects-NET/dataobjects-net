// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.09.14

using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira604_SetOperationsWithDuplicateFieldsUsageFailsModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira604_SetOperationsWithDuplicateFieldsUsageFailsModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public String SomeText { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira604_SetOperationsWithDuplicateFieldsUsageFails : AutoBuildTest
  {
    [Test]
    public void UnionTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var query1 = session.Query.All<TestEntity>().Where(el=>el.Id > 1000).Select(el => new{el.Id, Text = el.SomeText, TextDublicate = el.SomeText});
        var query2 = session.Query.All<TestEntity>().Where(el=>el.Id < 1000).Select(el => new{el.Id, Text = el.SomeText, TextDublicate = el.SomeText});
        Assert.DoesNotThrow(()=>query1.Union(query2).Run());
        
        var query = query1.Union(query2).ToArray();
        Assert.That(query.Length, Is.EqualTo(1999));
        foreach (var entity in query) {
          Assert.That(string.IsNullOrEmpty(entity.Text), Is.False);
          Assert.That(string.IsNullOrEmpty(entity.TextDublicate), Is.False);
          Assert.That(entity.Text, Is.EqualTo(entity.TextDublicate));
        }
      }
    }

    [Test]
    public void ConcatTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var query1 = session.Query.All<TestEntity>().Where(el => el.Id > 1000).Select(el => new { el.Id, Text = el.SomeText, TextDublicate = el.SomeText });
        var query2 = session.Query.All<TestEntity>().Where(el => el.Id < 1000).Select(el => new { el.Id, Text = el.SomeText, TextDublicate = el.SomeText });
        Assert.DoesNotThrow(() => query1.Concat(query2).Run());

        var query = query1.Concat(query2).ToArray();
        Assert.That(query.Length, Is.EqualTo(1999));
        foreach (var entity in query) {
          Assert.That(string.IsNullOrEmpty(entity.Text), Is.False);
          Assert.That(string.IsNullOrEmpty(entity.TextDublicate), Is.False);
          Assert.That(entity.Text, Is.EqualTo(entity.TextDublicate));
        }
      }
    }

    [Test]
    public void IntersectTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var query1 = session.Query.All<TestEntity>().Where(el => el.Id <= 200).Select(el => new { el.Id, Text = el.SomeText, TextDublicate = el.SomeText });
        var query2 = session.Query.All<TestEntity>().Where(el => el.Id >= 100).Select(el => new { el.Id, Text = el.SomeText, TextDublicate = el.SomeText });
        Assert.DoesNotThrow(() => query1.Intersect(query2).Run());

        var query = query1.Intersect(query2).ToArray();
        Assert.That(query.Length, Is.EqualTo(101));
        foreach (var entity in query) {
          Assert.That(string.IsNullOrEmpty(entity.Text), Is.False);
          Assert.That(string.IsNullOrEmpty(entity.TextDublicate), Is.False);
          Assert.That(entity.Text, Is.EqualTo(entity.TextDublicate));
        }
      }
    }

    [Test]
    public void ExceptTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        var query1 = session.Query.All<TestEntity>().Select(el => new { el.Id, Text = el.SomeText, TextDublicate = el.SomeText });
        var query2 = session.Query.All<TestEntity>().Where(el => el.Id < 1000).Select(el => new { el.Id, Text = el.SomeText, TextDublicate = el.SomeText });
        Assert.DoesNotThrow(() => query1.Except(query2).Run());

        var query = query1.Except(query2).ToArray();
        Assert.That(query.Length, Is.EqualTo(1001));
        foreach (var entity in query) {
          Assert.That(string.IsNullOrEmpty(entity.Text), Is.False);
          Assert.That(string.IsNullOrEmpty(entity.TextDublicate), Is.False);
          Assert.That(entity.Text, Is.EqualTo(entity.TextDublicate));
        }
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (var i = 0; i < 2000; i++)
          new TestEntity(){SomeText = i.ToString(CultureInfo.InvariantCulture)};
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (TestEntity));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}
