// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.02

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Linq.ConvariantQueriesTestModel;

namespace Xtensive.Storage.Tests.Linq.ConvariantQueriesTestModel
{
  [HierarchyRoot]
  public class MyBaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class MyChildEntity : MyBaseEntity
  {
    [Field]
    public int Field { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Linq
{
  public class ConvariantQueriesTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(MyBaseEntity).Assembly, typeof(MyBaseEntity).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
    }

    #if NET40

    [Test]
    public void ConcatTest()
    {
      var q1 = Query.All<MyBaseEntity>().Concat(Query.All<MyChildEntity>()).ToList();
      var q2 = Query.All<MyChildEntity>().Concat(Query.All<MyBaseEntity>()).ToList();
    }

    [Test]
    public void UnionTest()
    {
      var q1 = Query.All<MyBaseEntity>().Union(Query.All<MyChildEntity>()).ToList();
      var q2 = Query.All<MyChildEntity>().Union(Query.All<MyBaseEntity>()).ToList();
    }

    [Test]
    public void IntersectTest()
    {
      var q1 = Query.All<MyBaseEntity>().Intersect(Query.All<MyChildEntity>()).ToList();
      var q2 = Query.All<MyChildEntity>().Intersect(Query.All<MyBaseEntity>()).ToList();
    }

    [Test]
    public void ExceptTest()
    {
      var q1 = Query.All<MyBaseEntity>().Except(Query.All<MyChildEntity>()).ToList();
      var q2 = Query.All<MyChildEntity>().Except(Query.All<MyBaseEntity>()).ToList();
    }

    [Test]
    public void ContainsTest()
    {
      var q1 = Query.All<MyBaseEntity>()
        .Where(baseEntity => Query.All<MyChildEntity>().Contains(baseEntity))
        .ToList();
      var q2 = Query.All<MyChildEntity>()
        .Where(childEntity => Query.All<MyBaseEntity>().Contains(childEntity))
        .ToList();
    }

    [Test]
    public void InTest()
    {
      var q1 = Query.All<MyBaseEntity>()
        .Where(baseEntity => baseEntity.In(Query.All<MyChildEntity>()))
        .ToList();
      var q2 = Query.All<MyChildEntity>()
        .Where(childEntity => childEntity.In(Query.All<MyBaseEntity>()))
        .ToList();
    }

    #endif
  }
}