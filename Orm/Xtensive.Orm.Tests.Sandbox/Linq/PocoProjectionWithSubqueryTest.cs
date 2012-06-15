// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.13

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.PocoProjectionWithSubqueryTestModel;

namespace Xtensive.Orm.Tests.Linq
{
  namespace PocoProjectionWithSubqueryTestModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      public Owner(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Item1 : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Owner Owner { get; set; }

      public Item1(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    public class Item2 : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Owner Owner { get; set; }

      public Item2(Session session)
        : base(session)
      {
      }
    }

    public class QueryModel
    {
      public long Id { get; set; }

      public IEnumerable<long> Id2 { get; set; }
    }
  }

  public class PocoProjectionWithSubqueryTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var owner = new Owner(session);
        var item1 = new Item1(session) {Owner = owner};
        var item2 = new Item2(session) {Owner = owner};
        tx.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Item1>()
          .OrderByDescending(i1 => i1.Id)
          .Select(i1 => new QueryModel {
            Id = i1.Id,
            Id2 = session.Query.All<Item2>()
              .Where(i2 => i2.Owner.Id==i1.Owner.Id)
              .Select(i2 => i2.Id)
          });

        var count = query.Count();
        Assert.That(count, Is.EqualTo(1));

        foreach (var item in query)
          Assert.That(item.Id2.ToArray().Length, Is.EqualTo(1));

        tx.Complete();
      }
    }
  }
}