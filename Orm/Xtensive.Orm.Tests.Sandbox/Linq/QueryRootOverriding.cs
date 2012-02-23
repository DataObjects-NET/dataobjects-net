// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.11.02

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
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

    public class HiddenEntityFilter : IQueryRootBuilder
    {
      private readonly QueryEndpoint query;

      public Expression GetRootExpression<T>() where T : class, IEntity
      {
        if (!typeof(IHidden).IsAssignableFrom(typeof(T)))
          return query.All<T>().Expression;
        Expression<Func<IHidden, bool>> filter = t => !t.IsHidden;
        var p = Expression.Parameter(typeof (T), "t");
        var typedFilter = (Expression<Func<T, bool>>) filter.BindParameters(p);
        return query.All<T>().Where<T>(typedFilter).Expression;
      }

      public HiddenEntityFilter(QueryEndpoint query)
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
      config.Types.Register(typeof (NormalEntity));
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenAutoTransaction()) {
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
    public void HiddenEntityQueryWithFilter()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
      using (session.OverrideQueryRoot(new HiddenEntityFilter(session.Query))) {
        var count = session.Query.All<HiddenEntity>().Count();
        Assert.That(count, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}