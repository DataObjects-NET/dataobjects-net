// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.24

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;

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
  }

  [HierarchyRoot]
  public class Container : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Simple> Entities { get; private set; }
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
          var entity = new Simple {
            Value = Guid.NewGuid().ToString()
          };
          entityKey = entity.Key;
          session.SaveChanges();

          // Let's get session cache accessor
          var sessionState = DirectStateAccessor.Get(session);

          // Get the number of cached entities
          Assert.AreEqual(1, sessionState.Count);

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
          Assert.AreEqual(originalValue, entity.Value);

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
          var entity = new Simple {Value = Guid.NewGuid().ToString()};
          entityKey = entity.Key;
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var entity = session.Query.Single<Simple>(entityKey);
          var entityState = DirectStateAccessor.Get(entity);

          var valueFieldState = entityState.GetFieldState("Value");
          Assert.AreEqual(PersistentFieldState.Loaded, valueFieldState);

          entity.Value += "Modified";
          valueFieldState = entityState.GetFieldState("Value");
          Assert.AreEqual(PersistentFieldState.Modified, valueFieldState);

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
          var container = new Container();
          containerKey = container.Key;
          container.Entities.Add(new Simple());
          firstContainedKey = container.Entities.Single().Key;
          container.Entities.Add(new Simple());
          container.Entities.Add(new Simple());
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var container = session.Query.Single<Container>(containerKey);
          var entitySetState = DirectStateAccessor.Get(container.Entities);
          Assert.IsFalse(entitySetState.IsFullyLoaded);
          foreach (var simple in container.Entities) {
            break; // To do nothing, but just call GetEnumerator()
          }
          Assert.IsTrue(entitySetState.IsFullyLoaded); // Enumeration attempt leads to full loading
          Assert.IsTrue(entitySetState.IsCountAvailable); // So Count is available as well
          Assert.AreEqual(3, entitySetState.Count);
          Assert.IsTrue(entitySetState.Contains(firstContainedKey));
        }
      }
    }

    private static Domain BuildDomain()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof(Simple).Assembly, typeof(Simple).Namespace);
      return Domain.Build(config);
    }
  }
}