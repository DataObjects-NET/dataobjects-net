// Copyright (C) 2013-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public static DateTime GetUserTime(this DateTime dateTime, TimeZoneInfo _)
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
        var zone = TimeZoneInfo.Local;
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
        var zone = TimeZoneInfo.Local;
        var query = session.Query.CreateDelayedQuery(
          q => q.All<TestEntity>().Select(e => e.Value.GetUserTime(zone)));
        var result = query.ToList();
        tx.Complete();
      }
    }
  }
}