// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.08

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchWithSmallCacheTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Sessions.Default.CacheType = SessionCacheType.LruWeak;
      config.Sessions.Default.CacheSize = 2;
      config.KeyCacheSize = 2;
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Supplier).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        for (int i = 0; i < 111; i++)
          PrefetchTestHelper.FillDataBase(session);
        transactionScope.Complete();
      }
    }

    [Test, Category("Performance")]
    public void SimpleTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Order>().Select(p => p.Key).ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var orders = session.Query.Many<Order>(keys)
          .Prefetch(o => o.Employee);
        var orderType = Domain.Model.Types[typeof (Order)];
        var employeeType = Domain.Model.Types[typeof (Employee)];
        var employeeField = orderType.Fields["Employee"];
        foreach (var order in orders) {
          GC.Collect(2, GCCollectionMode.Forced);
          GC.WaitForPendingFinalizers();
          var orderState = session.EntityStateCache[order.Key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(order.Key, orderType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          var employeeKey = Key.Create<Person>(Domain, employeeField.Associations.Last()
            .ExtractForeignKey(orderState.Type, orderState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test]
    public void UsingDelayedElementsTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Person>().Take(221).AsEnumerable().Select(p => Key.Create<Person>(Domain, p.Key.Value))
          .ToList();
        Assert.IsTrue(keys.All(key => !key.HasExactType));
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var persons = session.Query.Many<Person>(keys)
          .Prefetch(p => p.Name);
        foreach (var person in persons) {
          var orderState = session.EntityStateCache[person.Key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderState.Key, orderState.Key.TypeInfo, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test, Category("Performance")]
    public void PrefetchEntitySetTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Order>().Take(221).AsEnumerable().Select(p => Key.Create<Order>(Domain, p.Key.Value))
          .ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var orders = session.Query.Many<Order>(keys)
          .Prefetch(o => o.Details);
        var orderType = Domain.Model.Types[typeof (Order)];
        var detailsField = orderType.Fields["Details"];
        foreach (var order in orders) {
          CollectGarbadge();
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(order.Key, orderType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          EntitySetState state;
          session.Handler.LookupState(order.Key, detailsField, out state);
          Assert.IsTrue(state.IsFullyLoaded);
          foreach (var detailKey in state) {
            Assert.IsTrue(detailKey.HasExactType);
            var detailState = session.EntityStateCache[detailKey, false];
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(detailKey, detailKey.TypeInfo, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          }
        }
      }
    }

    private static void CollectGarbadge()
    {
      GC.Collect(2, GCCollectionMode.Forced);
      GC.WaitForPendingFinalizers();
    }
  }
}