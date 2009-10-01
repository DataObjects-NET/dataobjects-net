// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public sealed class PrefetcherTest : NorthwindDOModelTest
  {
    [Test]
    public void EnumerableOfNonEntityTest()
    {
      List<Key> keys;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open())
        keys = Query<Order>.All.Select(o => o.Key).ToList();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.OrderDate);
        var orderType = typeof (Order).GetTypeInfo();
        var orderDateField = orderType.Fields["OrderDate"];
        foreach (var key in prefetcher)
          AssertOnlySpecifiedColumnsAreLoaded(key, key.Type, session, field => field.IsPrimaryKey
            || field.IsSystem || field.Equals(orderDateField));
      }
    }

    [Test]
    public void EnumerableOfEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = Query<Order>.All.Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails);
        var orderDetailsField = typeof (Order).GetTypeInfo().Fields["OrderDetails"];
        foreach (var order in prefetcher) {
          EntitySetState entitySetState;
          Assert.IsTrue(session.Handler.TryGetEntitySetState(order.Key, orderDetailsField, out entitySetState));
          Assert.IsTrue(entitySetState.IsFullyLoaded);
        }
      }
    }

    [Test]
    public void PreservingOriginalOrderOfElementsTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Order>.All.ToList();
        var actual = expected.Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PrefetchManyEntitySetTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var employeesField = typeof (Territory).GetTypeInfo().Fields["Employees"];
        var ordersField = typeof (Employee).GetTypeInfo().Fields["Orders"];
        var prefetcher = Query<Territory>.All.PrefetchMany(t => t.Employees, employees => employees,
          employees => employees.Prefetch(e => e.Orders));
        foreach (var territory in prefetcher) {
          var entitySetState = GetFullyLoadedEntitySet(session, territory.Key, employeesField);
          foreach (var employeeKey in entitySetState)
            GetFullyLoadedEntitySet(session, employeeKey, ordersField);
        }
      }
    }

    [Test]
    public void PreservingOrderInPrefetchManyTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query<Territory>.All.ToList();
        var actual = expected.PrefetchMany(t => t.Employees, employees => employees,
          employees => employees.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void InvalidArgumentsTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        AssertEx.Throws<ArgumentException>(() => Query<Territory>.All.Prefetch(t => new {t.Id, t.Region}));
        AssertEx.Throws<ArgumentException>(() => Query<Territory>.All.Prefetch(t => t.Region.RegionDescription));
        AssertEx.Throws<ArgumentException>(() => Query<Territory>.All.Prefetch(t => t.PersistenceState));
        AssertEx.Throws<InvalidOperationException>(() =>
          Query<Territory>.All.Prefetch(t => t.Employees).Prefetch(t => t.Employees));
      }
    }

    private static EntitySetState GetFullyLoadedEntitySet(Session session, Key key,
      FieldInfo employeesField)
    {
      EntitySetState entitySetState;
      Assert.IsTrue(session.Handler.TryGetEntitySetState(key, employeesField, out entitySetState));
      Assert.IsTrue(entitySetState.IsFullyLoaded);
      return entitySetState;
    }

    private void AssertOnlySpecifiedColumnsAreLoaded(Key key, TypeInfo type, Session session,
      Func<FieldInfo, bool> fieldSelector)
    {
      var tuple = session.EntityStateCache[key, true].Tuple;
      Assert.IsNotNull(tuple);
      foreach (var field in type.Fields) {
        var isFieldSelected = fieldSelector.Invoke(field);
        foreach (var column in field.Columns) {
          var isAvailable = tuple.GetFieldState(type.Columns.IndexOf(column)).IsAvailable();
          if (isFieldSelected)
            Assert.IsTrue(isAvailable);
          else
            Assert.IsFalse(isAvailable);
        }
      }
    }
  }
}