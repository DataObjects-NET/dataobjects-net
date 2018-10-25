// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.25.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0746_MethodLikeDifferentBehaviorInSQLAndMemoryModel;

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0746_MethodLikeDifferentBehaviorInSQLAndMemory : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {

        new SimpleEntity(Guid.NewGuid()) {Name = "&&%name"};
        new SimpleEntity(Guid.NewGuid()) {Name = "&%name"};

        AreEqual(session, q => q.Single(z => z.Name.Like("&&&%name", '&')));
      }
    }

    [Test]
    public void Case1Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        new SimpleEntity(Guid.NewGuid()) { Name = "azaza&%nameazaza" };
        new SimpleEntity(Guid.NewGuid()) { Name = "&%name" };
        new SimpleEntity(Guid.NewGuid()) { Name = "%name" };

        AreEqual(session, q => q.Single(z => z.Name.Like("&&%name", '&')));
        AreEqual(session, q => q.Single(z => z.Name.Like("&%name%", '&')));
      }
    }

    [Test]
    public void Case2Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        new SimpleEntity(Guid.NewGuid()) { Name = "azaza&%nameazaza" };
        new SimpleEntity(Guid.NewGuid()) { Name = "&%name" };
        new SimpleEntity(Guid.NewGuid()) { Name = "%name" };
        new SimpleEntity(Guid.NewGuid()) { Name = "name%" };

        Assert.Throws<InvalidOperationException>(() => session.Query.All<SimpleEntity>().ToArray().Single(z => z.Name.Like("name", '&')));
        Assert.Throws<InvalidOperationException>(() => session.Query.All<SimpleEntity>().Single(z => z.Name.Like("name", '&')));
      }
    }

    [Test]
    public void Case3Test()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction())
      {
        new SimpleEntity(Guid.NewGuid()) { Name = "azaza&%nameazaza" };
        new SimpleEntity(Guid.NewGuid()) { Name = "&%name" };
        new SimpleEntity(Guid.NewGuid()) { Name = "%name" };
        new SimpleEntity(Guid.NewGuid()) { Name = "name%" };
        new SimpleEntity(Guid.NewGuid()) { Name = "&name" };
        new SimpleEntity(Guid.NewGuid()) { Name = "name&" };
        new SimpleEntity(Guid.NewGuid()) { Name = "name" };

        session.Query.All<SimpleEntity>().ToArray().Single(z => z.Name.Like("name", '&'));
        session.Query.All<SimpleEntity>().Single(z => z.Name.Like("name", '&'));
      }
    }

    private void AreEqual(Session session, Func<IQueryable<SimpleEntity>, SimpleEntity> selectClause)
    {
      var queryable = session.Query.All<SimpleEntity>();
      var ent1 = selectClause(queryable);
      var ent2 = selectClause(queryable.ToArray().AsQueryable());
      Assert.AreEqual(ent1, ent2);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(SimpleEntity).Assembly, typeof(SimpleEntity).Namespace);
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0746_MethodLikeDifferentBehaviorInSQLAndMemoryModel
{
  [HierarchyRoot]
  public class SimpleEntity : Entity
  {
    public SimpleEntity(Guid id)
      : base(id)
    {
    }

    [Field]
    [Key]
    public Guid Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}
