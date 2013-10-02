// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.05.21

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0445_QueryForEntityWithEnumKeyModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0445_QueryForEntityWithEnumKeyModel
  {
    public enum EntityKey
    {
      Zero,
      One,
      Two
    }

    [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
    public class EntityWithEnumKey : Entity
    {
      [Key, Field]
      public EntityKey Id { get; private set; }

      public string Value { get; set; }

      public EntityWithEnumKey(Session session, EntityKey id)
        : base(session, id)
      {
      }
    }
  }

  [TestFixture]
  public class IssueJira0445_QueryForEntityWithEnumKey : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithEnumKey));
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      Key entityKey;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // Create
        entityKey = new EntityWithEnumKey(session, EntityKey.One).Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // Fetch
        var entity1 = session.Query.Single<EntityWithEnumKey>(entityKey);

        // Query using LINQ
        var entity2 = session.Query.All<EntityWithEnumKey>().Single(e => e.Key==entityKey);

        Assert.That(entity1, Is.SameAs(entity2));

        // Update
        entity1.Value = "Hello";
        session.SaveChanges();

        // Remove
        entity1.Remove();

        tx.Complete();
      }
    }
  }
}