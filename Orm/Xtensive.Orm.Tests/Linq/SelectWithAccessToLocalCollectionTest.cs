// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.12.16

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.SelectWithAccessToLocalCollectionTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace SelectWithAccessToLocalCollectionTestModel
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public int Value { get; set; }
    }
  }

  [TestFixture]
  public class SelectWithAccessToLocalCollectionTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestEntity));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new TestEntity {Value = 1};
        new TestEntity {Value = 2};
        new TestEntity {Value = 3};
        new TestEntity {Value = 4};
        new TestEntity {Value = 5};
        tx.Complete();
      }
    }

    [Test]
    public void SelectDirectTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new[] {1, 2, 3};
        var query = session.Query.All<TestEntity>()
          .Select(e => items.Contains(e.Value));
        TestQuery(query, v => v, 3);
      }
    }

    [Test]
    public void SelectViaAnonymousType1Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new[] {1, 2, 3};
        var query = session.Query.All<TestEntity>()
          .Select(e => new {e.Id, Value = items.Contains(e.Value)});
        TestQuery(query, v => v.Value, 3);
      }
    }

    [Test]
    public void SelectViaAnonymousType2Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new[] {1, 2, 3};
        var query = session.Query.All<TestEntity>()
          .Select(e => new {Dummy = e.Value, Value = items.Contains(e.Value)});
        TestQuery(query, v => v.Value, 3);
      }
    }


    [Test]
    public void SelectViaAnonymousType3Test()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var items = new[] {1, 2, 3};
        var query = session.Query.All<TestEntity>()
          .Select(e => new {e, Value = items.Contains(e.Value)});
        TestQuery(query, v => v.Value, 3);
      }
    }

    private void TestQuery<T>(IEnumerable<T> query, Func<T, bool> valueSelector, int expectedCount)
    {
      var result = query.ToList();
      var count = result.Count(valueSelector);
      Assert.That(count, Is.EqualTo(expectedCount));
    }
  }
}