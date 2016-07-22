// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.16

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0636_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0636_Model
  {
    [HierarchyRoot]
    public class BaseEntity : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      public BaseEntity()
      {
      }

      public BaseEntity(Session session, long id)
        : base(session, id)
      {
      }
    }

    public class FirstSuccessor : BaseEntity
    {
      [Field]
      public string StringField1 { get; set; }

      public FirstSuccessor()
      {
      }

      public FirstSuccessor(Session session, long id)
        : base(session, id)
      {
      }
    }

    public class SecondSuccessor : BaseEntity
    {
      [Field]
      public string StringField2 { get; set; }

      public SecondSuccessor()
      {
      }

      public SecondSuccessor(Session session, long id)
        : base(session, id)
      {
      }
    }
  }

  public class IssueJira0636_SessionCacheTest : AutoBuildTest
  {
    private const long WrongId = int.MaxValue - 1;
    private long baseEntityId;
    private long firstSuccessorId;
    private long secondSuccessorId;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (BaseEntity).Assembly, typeof (BaseEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = new BaseEntity();
        baseEntityId = entity.Id;
        entity = new FirstSuccessor();
        firstSuccessorId = entity.Id;
        entity = new SecondSuccessor();
        secondSuccessorId = entity.Id;
        transaction.Complete();
      }
    }

    [Test]
    public void SelectBaseEntityFirstTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<BaseEntity>(WrongId, true);
        GetEntity<BaseEntity>(baseEntityId, false);
        GetEntity<BaseEntity>(firstSuccessorId, false);
        GetEntity<BaseEntity>(secondSuccessorId, false);

        GetEntity<FirstSuccessor>(WrongId, true);
        GetEntity<FirstSuccessor>(baseEntityId, true);
        GetEntity<FirstSuccessor>(firstSuccessorId, false);
        GetEntity<FirstSuccessor>(secondSuccessorId, true);

        GetEntity<SecondSuccessor>(WrongId, true);
        GetEntity<SecondSuccessor>(baseEntityId, true);
        GetEntity<SecondSuccessor>(firstSuccessorId, true);
        GetEntity<SecondSuccessor>(secondSuccessorId, false);
      }
    }

    [Test]
    public void SelectBaseEntityInTheMiddleTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<FirstSuccessor>(WrongId, true);
        GetEntity<FirstSuccessor>(baseEntityId, true);
        GetEntity<FirstSuccessor>(firstSuccessorId, false);
        GetEntity<FirstSuccessor>(secondSuccessorId, true);

        GetEntity<BaseEntity>(WrongId, true);
        GetEntity<BaseEntity>(baseEntityId, false);
        GetEntity<BaseEntity>(firstSuccessorId, false);
        GetEntity<BaseEntity>(secondSuccessorId, false);

        GetEntity<SecondSuccessor>(WrongId, true);
        GetEntity<SecondSuccessor>(baseEntityId, true);
        GetEntity<SecondSuccessor>(firstSuccessorId, true);
        GetEntity<SecondSuccessor>(secondSuccessorId, false);
      }
    }

    [Test]
    public void SelectBaseEntityLastTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<FirstSuccessor>(WrongId, true);
        GetEntity<FirstSuccessor>(baseEntityId, true);
        GetEntity<FirstSuccessor>(firstSuccessorId, false);
        GetEntity<FirstSuccessor>(secondSuccessorId, true);

        GetEntity<SecondSuccessor>(WrongId, true);
        GetEntity<SecondSuccessor>(baseEntityId, true);
        GetEntity<SecondSuccessor>(firstSuccessorId, true);
        GetEntity<SecondSuccessor>(secondSuccessorId, false);

        GetEntity<BaseEntity>(WrongId, true);
        GetEntity<BaseEntity>(baseEntityId, false);
        GetEntity<BaseEntity>(firstSuccessorId, false);
        GetEntity<BaseEntity>(secondSuccessorId, false);
      }
    }

    [Test]
    public void SelectBeforeCreateTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var id = 5001L;
        var entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNull(entity);
        new BaseEntity(session, id);
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity.Remove();
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        new FirstSuccessor(session, id);
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNotNull(entity);
        entity.Remove();
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);
        new SecondSuccessor(session, id);
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNotNull(entity);
        entity.Remove();
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        new BaseEntity(session, id);
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.Single<BaseEntity>(id);
        entity.Remove();
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);

        ++id;
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);
        new BaseEntity(session, id);
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNotNull(entity);
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.Single<BaseEntity>(id);
        entity.Remove();
        entity = Query.SingleOrDefault<FirstSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<SecondSuccessor>(id);
        Assert.IsNull(entity);
        entity = Query.SingleOrDefault<BaseEntity>(id);
        Assert.IsNull(entity);
      }
    }

    private T GetEntity<T>(long keyValue, bool expectedNull)
      where T : Entity
    {
      var entity = Query.SingleOrDefault<T>(keyValue);
      if (expectedNull)
        Assert.IsNull(entity);
      else
        Assert.IsNotNull(entity);
      return entity;
    }
  }
}
