// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.16

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using CompiledQueriesWithClosureTestModel;

namespace CompiledQueriesWithClosureTestModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public DateTime Value { get; set; }
  }

  public static class Extensions
  {
    public static DateTime GetUserTime(this DateTime dateTime, TimeZone zone)
    {
      return dateTime;
    }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class CompiledQueriesWithClosureTest : AutoBuildTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entity = new TestEntity {Value = DateTime.Now};
        tx.Complete();
      }
    }

    [Test]
    public void CompiledQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var zone = TimeZone.CurrentTimeZone;
        var query = session.Query.Execute(
          q => q.All<TestEntity>().Select(e => e.Value.GetUserTime(zone)));
        var result = query.ToList();
        tx.Complete();
      }
    }

    [Test]
    public void DelayedQueryTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var zone = TimeZone.CurrentTimeZone;
        var query = session.Query.ExecuteDelayed(
          q => q.All<TestEntity>().Select(e => e.Value.GetUserTime(zone)));
        var result = query.ToList();
        tx.Complete();
      }
    }
  }
}