// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.02

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.QueryRootOverridingModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace QueryRootOverridingModel
  {
    public interface IHidden : IEntity
    {
      [Field]
      bool IsHidden { get; }
    }

    [HierarchyRoot]
    public class NormalEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }
    }

    [HierarchyRoot]
    public class HiddenEntity : Entity, IHidden
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public bool IsHidden { get; set; }
    }

    public abstract class QueryFilter
    {
      public abstract IQueryable Apply(IQueryable root);
    }

    public class HiddenEntityFilter<T> : QueryFilter
      where T : IHidden
    {
      public override IQueryable Apply(IQueryable root)
      {
        var typedRoot = (IQueryable<T>) root;
        return typedRoot.Where(t => !t.IsHidden);
      }
    }

    public class FilteringRootBuilder : IQueryRootBuilder
    {
      private readonly QueryEndpoint query;

      public Expression BuildRootExpression(Type entityType)
      {
        if (!typeof (IHidden).IsAssignableFrom(entityType))
          return query.All(entityType).Expression;
        return CreateFilter(entityType).Apply(query.All(entityType)).Expression;
      }

      private static QueryFilter CreateFilter(Type entityType)
      {
        return (QueryFilter) Activator.CreateInstance(typeof (HiddenEntityFilter<>).MakeGenericType(entityType));
      }

      public FilteringRootBuilder(QueryEndpoint query)
      {
        this.query = query;
      }
    }
  }

  public class QueryRootOverriding : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (NormalEntity).Assembly, typeof (NormalEntity).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new NormalEntity();
        new HiddenEntity();
        new HiddenEntity {IsHidden = true};
        tx.Complete();
      }
    }

    [Test]
    public void NormalEntityQuery()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var count = session.Query.All<NormalEntity>().Count();
        Assert.That(count, Is.EqualTo(1));
        tx.Complete();
      }
    }

    [Test]
    public void HiddenEntityQueryWithoutFilter()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var count = session.Query.All<HiddenEntity>().Count();
        Assert.That(count, Is.EqualTo(2));
        tx.Complete();
      }
    }

    [Test]
    public void HiddenEntityQueryWithFilterViaSystemQuery()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
      using (session.OverrideQueryRoot(new FilteringRootBuilder(session.Query))) {
        var count = session.SystemQuery.All<HiddenEntity>().Count();
        Assert.That(count, Is.EqualTo(2));
        tx.Complete();
      }
    }

    [Test]
    public void HiddenEntityQueryWithFilter()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
      using (session.OverrideQueryRoot(new FilteringRootBuilder(session.Query))) {
        var count = session.Query.All<HiddenEntity>().Count();
        Assert.That(count, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}