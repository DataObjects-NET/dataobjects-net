// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.04.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0442_AsQueryableExpressionInQueryModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0442_AsQueryableExpressionInQueryModel
  {
    [HierarchyRoot]
    public class ObjectWithId : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }
  }

  [TestFixture]
  public class IssueJira0442_AsQueryableExpressionInQuery : AutoBuildTest
  {
    private List<long> keys = new List<long>();

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ObjectWithId));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var o1 = new ObjectWithId();
        var o2 = new ObjectWithId();
        var o3 = new ObjectWithId();
        keys.Add(o1.Id);
        keys.Add(o3.Id);
        tx.Complete();
      }
    }

    [Test]
    public void AsQueryableTest()
    {
      RunTest(keys.AsQueryable());
    }

#if NET40

    [Test]
    public void EnumerableQueryTest()
    {
      RunTest(new EnumerableQuery<long>(keys));
    }

#endif

    private void RunTest(IQueryable<long> keysQuery)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<ObjectWithId>()
          .Where(BuildIdFilter(keysQuery.Expression))
          .Select(i => i.Id)
          .OrderBy(i => i)
          .ToList();
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.SequenceEqual(keys));
        tx.Complete();
      }
    }

    private Expression<Func<ObjectWithId, bool>> BuildIdFilter(Expression expression)
    {
      var method = typeof (Queryable).GetMethods().Single(m => m.Name=="Contains" && m.GetParameters().Length==2);
      var p = Expression.Parameter(typeof (ObjectWithId), "o");
      var id = Expression.Property(p, "Id");
      var call = Expression.Call(method.MakeGenericMethod(typeof (long)), expression, id);
      return Expression.Lambda<Func<ObjectWithId, bool>>(call, p);
    }
  }
}