// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.02.24

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Manual.Services
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
  public class SimpleContainer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Simple> Set { get; private set; }
  }

  #endregion

  [TestFixture]
  public sealed class DirectStateAccessorTest
  {
    [Test]
    public void SessionStateAccessorTest()
    {
      var domain = BuildDomain();

      using (var session = Session.Open(domain)) {
        string originalValue;
        Key entity0Key;
        using (var tx = Transaction.Open()) {
          var entity0 = new Simple {Value = Guid.NewGuid().ToString()};
          entity0Key = entity0.Key;
          originalValue = entity0.Value;
          var entity1 = new Simple {Value = Guid.NewGuid().ToString()};
          var stateAccessor = DirectStateAccessor.Get(session);
          // Get the number of cached entities
          Assert.AreEqual(2, stateAccessor.Count);
          // Enumerate the cached entites
          foreach (var entity in stateAccessor) {
            if (entity==entity0 || entity==entity1)
              continue;
            Assert.Fail();
          }
          tx.Complete();
        }
        using (var tx = Transaction.Open()) {
          var entity0 = Query.Single<Simple>(entity0Key);
          entity0.Value += "Modified";
          var stateAccessor = DirectStateAccessor.Get(session);
          // Clear the cache of changed entities
          stateAccessor.Invalidate();
          tx.Complete();
        }
        using (var tx = Transaction.Open()) {
          var entity0 = Query.Single<Simple>(entity0Key);
          // The field value hasn't been modified due to the call to Session.Invalidate
          Assert.AreEqual(originalValue, entity0.Value);
        }
      }
    }

    [Test]
    public void PersistentStateAccessorTest()
    {
      var domain = BuildDomain();

      using (var session = Session.Open(domain)) {
        Key entity0Key;
        using (var tx = Transaction.Open()) {
          var entity0 = new Simple {Value = Guid.NewGuid().ToString()};
          entity0Key = entity0.Key;
          var entity1 = new Simple {Value = Guid.NewGuid().ToString()};
          tx.Complete();
        }
        using (var tx = Transaction.Open()) {
          var entity0 = Query.Single<Simple>(entity0Key);
          var stateAccessor = DirectStateAccessor.Get(entity0);
          const string valueFieldName = "Value";
          // Get the field state
          var valueFieldState = stateAccessor.GetFieldState(valueFieldName);
          Assert.AreEqual(PersistentFieldState.Loaded, valueFieldState);
          using (session.Pin(entity0)) {
            entity0.Value += "Modified";
            valueFieldState = stateAccessor.GetFieldState(valueFieldName);
            Assert.AreEqual(PersistentFieldState.Modified, valueFieldState);
          }
          tx.Complete();
        }
      }
    }

    [Test]
    public void EntitySetAccessorTest()
    {
      var domain = BuildDomain();

      using (var session = Session.Open(domain)) {
        Key containerKey;
        Key simpleKey;
        using (var tx = Transaction.Open()) {
          var container = new SimpleContainer();
          containerKey = container.Key;
          container.Set.Add(new Simple());
          simpleKey = container.Set.Single().Key;
          container.Set.Add(new Simple());
          container.Set.Add(new Simple());
          tx.Complete();
        }
        using (var tx = Transaction.Open()) {
          var container = Query.Single<SimpleContainer>(containerKey);
          var stateAccessor = DirectStateAccessor.Get(container.Set);
          Assert.IsFalse(stateAccessor.IsFullyLoaded);
          foreach (var simple in container.Set) {
            // Do nothing
          }
          Assert.IsTrue(stateAccessor.IsFullyLoaded);
          Assert.IsTrue(stateAccessor.IsCountAvailable);
          Assert.AreEqual(3, stateAccessor.Count);
          Assert.IsTrue(stateAccessor.Contains(simpleKey));
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