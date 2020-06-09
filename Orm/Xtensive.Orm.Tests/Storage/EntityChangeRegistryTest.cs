// Copyright (C) 2020 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2020.02.14

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.EntityChangeRegistryTestModel;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Storage.EntityChangeRegistryTestModel
{
  [HierarchyRoot]
  public class DummyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Field { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public sealed class EntityChangeRegistryTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (DummyEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    [Test]
    public void UnsupportedStateContainerTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        var state1 = CreateDummyState(session, 100, null);
        state1.SetPersistenceState(PersistenceState.New);
        registry.Register(state1);
        Assert.Throws<ArgumentOutOfRangeException>(() => registry.GetItems(PersistenceState.Synchronized).Run());
      }
    }

    [Test]
    public void NewItemsRegistrationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.New);
        state2.SetPersistenceState(PersistenceState.New);
        state3.SetPersistenceState(PersistenceState.New);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state2);
        Assert.That(registry.Count, Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state1 || i == state2));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state3);
        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state1 || i == state2 || i == state3));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    [Test]
    public void ModifiedItemsRegistrationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.Modified);
        state2.SetPersistenceState(PersistenceState.Modified);
        state3.SetPersistenceState(PersistenceState.Modified);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(i => i == state1 ));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state2);
        Assert.That(registry.Count, Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1 || i == state2));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state3);
        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1 || i == state2 || i == state3));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    [Test]
    public void RemovedItemsRegistrationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.Removed);
        state2.SetPersistenceState(PersistenceState.Removed);
        state3.SetPersistenceState(PersistenceState.Removed);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1));

        registry.Register(state2);
        Assert.That(registry.Count, Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1 || i == state2));

        registry.Register(state3);
        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1 || i == state2 || i == state3));
      }
    }

    [Test]
    public void RemoveNewItemRegistrationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.New);
        state2.SetPersistenceState(PersistenceState.New);
        state3.SetPersistenceState(PersistenceState.New);
        registry.Register(state1);
        registry.Register(state2);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state1 || i == state2 || i == state3));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        state1.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state1);

        Assert.That(registry.Count, Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state2 || i == state3));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        state2.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state2);

        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(i => i == state3));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        state3.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    [Test]
    public void RemoveModifiedItemRegistrationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.Modified);
        state2.SetPersistenceState(PersistenceState.Modified);
        state3.SetPersistenceState(PersistenceState.Modified);
        registry.Register(state1);
        registry.Register(state2);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1 || i == state2 || i == state3));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        state1.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state1);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state2 || i == state3), Is.True);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1), Is.True);

        state2.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state2);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state3), Is.True);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(2));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1 || i==state2), Is.True);

        state3.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1 || i == state2 || i == state3), Is.True);
      }
    }

    [Test]
    public void RemoveRemovedItemRegistrationTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.Removed);
        state2.SetPersistenceState(PersistenceState.Removed);
        state3.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state1);
        registry.Register(state2);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1 || i == state2 || i == state3));

        registry.Register(state1);
        registry.Register(state2);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1 || i == state2 || i == state3));
      }
    }

    [Test]
    public void ClearTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = CreateDummyState(session, 100, null);
        var state2 = CreateDummyState(session, 101, null);
        var state3 = CreateDummyState(session, 102, null);
        state1.SetPersistenceState(PersistenceState.New);
        state2.SetPersistenceState(PersistenceState.New);
        state3.SetPersistenceState(PersistenceState.New);

        registry.Register(state1);
        registry.Register(state2);
        registry.Register(state3);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Clear();

        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state4 = CreateDummyState(session, 100, null);
        var state5 = CreateDummyState(session, 101, null);
        var state6 = CreateDummyState(session, 102, null);
        state4.SetPersistenceState(PersistenceState.Modified);
        state5.SetPersistenceState(PersistenceState.Modified);
        state6.SetPersistenceState(PersistenceState.Modified);

        registry.Register(state4);
        registry.Register(state5);
        registry.Register(state6);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Clear();

        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state7 = new EntityState(session, Key.Create(Domain, typeof(DummyEntity), 100), null);
        var state8 = new EntityState(session, Key.Create(Domain, typeof(DummyEntity), 101), null);
        var state9 = new EntityState(session, Key.Create(Domain, typeof(DummyEntity), 102), null);
        state7.SetPersistenceState(PersistenceState.Removed);
        state8.SetPersistenceState(PersistenceState.Removed);
        state9.SetPersistenceState(PersistenceState.Removed);

        registry.Register(state7);
        registry.Register(state8);
        registry.Register(state9);

        Assert.That(registry.Count, Is.EqualTo(3));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(3));
      }
    }

    [Test]
    public void MultipleAddsOfSameNewItem()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = CreateDummyState(session, 100, null);
        state1.SetPersistenceState(PersistenceState.New);
        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).All(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    [Test]
    public void MultipleAddsOfSameModifiedItems()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = CreateDummyState(session, 100, null);
        state1.SetPersistenceState(PersistenceState.Modified);
        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1));
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    [Test]
    public void MultipleAddsOfSameRemovedItem()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var state1 = new EntityState(session, Key.Create(Domain, typeof (DummyEntity), 100), null);
        state1.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1));

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1));

        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1));
      }
    }

    [Test]
    public void RegisterRevivedItemWithNoDifferenceTupleTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var dTuple = new DifferentialTuple(Tuple.Create<int, string>(0, null), null);
        var state1 = CreateDummyState(session, 100, dTuple);
        state1.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1));

        state1.SetPersistenceState(PersistenceState.New);
        registry.Register(state1);
        Assert.That(state1.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    [Test]
    public void RegisterRevivedItemWithDifferenceTupleTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var registry = session.EntityChangeRegistry;
        Assert.That(registry.Count, Is.EqualTo(0));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);

        var dTuple = new DifferentialTuple(
          Tuple.Create<int, string>(0, null),
          Tuple.Create<int, string>(0, "here is some changes"));
        var state1 = CreateDummyState(session, 100, dTuple);
        state1.SetPersistenceState(PersistenceState.Removed);
        registry.Register(state1);
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Removed).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Removed).All(i => i == state1));

        state1.SetPersistenceState(PersistenceState.New);
        registry.Register(state1);
        Assert.That(state1.PersistenceState, Is.EqualTo(PersistenceState.Modified));
        Assert.That(registry.Count, Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.New).Any(), Is.False);
        Assert.That(registry.GetItems(PersistenceState.Modified).Count(), Is.EqualTo(1));
        Assert.That(registry.GetItems(PersistenceState.Modified).All(i => i == state1), Is.True);
        Assert.That(registry.GetItems(PersistenceState.Removed).Any(), Is.False);
      }
    }

    private EntityState CreateDummyState(Session session, int id, Tuple data)
    {
      return new EntityState(session, Key.Create(Domain, typeof(DummyEntity), id), data);
    }
  }
}
