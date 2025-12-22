// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.24

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Services
{
  #region Model

  [HierarchyRoot]
  public class Simple : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Value { get; set; }

    public Simple(Session session)
      : base(session)
    {}
  }

  [HierarchyRoot]
  public class Container : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Simple> Entities { get; private set; }

    public Container(Session session)
      : base(session)
    {}
  }

  #endregion

  [TestFixture]
  public sealed class DirectStateAccessorTest
  {
    [Test]
    public void SessionStateAccessorTest()
    {
      var domain = BuildDomain();

      using (var session = domain.OpenSession()) {
        Key entityKey;
        string originalValue;
        using (var tx = session.OpenTransaction()) {
          var entity = new Simple (session) {
            Value = Guid.NewGuid().ToString()
          };
          entityKey = entity.Key;
          session.SaveChanges();

          // Let's get session cache accessor
          var sessionState = DirectStateAccessor.Get(session);

          // Get the number of cached entities
          Assert.That(sessionState.Count, Is.EqualTo(1));

          // Enumerate cached entites
          foreach (var e in sessionState) {
            if (e==entity)
              continue;
            Assert.Fail();
          }

          // Let's remember the value & change it
          originalValue = entity.Value;
          entity.Value += "Modified";

          // Clear cached state of all the entities in session
          sessionState.Invalidate();

          // The field value hasn't been modified due to invalidation
          Assert.That(entity.Value, Is.EqualTo(originalValue));

          tx.Complete();
        }
      }
    }

    [Test]
    public void PersistentStateAccessorTest()
    {
      var domain = BuildDomain();

      using (var session = domain.OpenSession()) {
        Key entityKey;
        using (var tx = session.OpenTransaction()) {
          var entity = new Simple (session) {Value = Guid.NewGuid().ToString()};
          entityKey = entity.Key;
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var entity = session.Query.Single<Simple>(entityKey);
          var entityState = DirectStateAccessor.Get(entity);

          var valueFieldState = entityState.GetFieldState("Value");
          Assert.That(valueFieldState, Is.EqualTo(PersistentFieldState.Loaded));

          entity.Value += "Modified";
          valueFieldState = entityState.GetFieldState("Value");
          Assert.That(valueFieldState, Is.EqualTo(PersistentFieldState.Modified));

          tx.Complete();
        }
      }
    }

    [Test]
    public void EntitySetAccessorTest()
    {
      var domain = BuildDomain();

      using (var session = domain.OpenSession()) {
        Key containerKey;
        Key firstContainedKey;
        using (var tx = session.OpenTransaction()) {
          var container = new Container(session);
          containerKey = container.Key;
          container.Entities.Add(new Simple(session));
          firstContainedKey = container.Entities.Single().Key;
          container.Entities.Add(new Simple(session));
          container.Entities.Add(new Simple(session));
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var container = session.Query.Single<Container>(containerKey);
          var entitySetState = DirectStateAccessor.Get(container.Entities);
          Assert.That(entitySetState.IsFullyLoaded, Is.False);
          foreach (var simple in container.Entities) {
            break; // To do nothing, but just call GetEnumerator()
          }
          Assert.That(entitySetState.IsFullyLoaded, Is.True); // Enumeration attempt leads to full loading
          Assert.That(entitySetState.IsCountAvailable, Is.True); // So Count is available as well
          Assert.That(entitySetState.Count, Is.EqualTo(3));
          Assert.That(entitySetState.Contains(firstContainedKey), Is.True);
        }
      }
    }

    private static Domain BuildDomain()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.Types.Register(typeof(Simple).Assembly, typeof(Simple).Namespace);
      return Domain.Build(config);
    }
  }
}