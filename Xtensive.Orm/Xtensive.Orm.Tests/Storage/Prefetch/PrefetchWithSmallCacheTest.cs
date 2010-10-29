// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.08

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchWithSmallCacheTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Sessions.Add(new SessionConfiguration(WellKnown.Sessions.Default));
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
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.Employee);
        var orderType = typeof (Order).GetTypeInfo();
        var employeeType = typeof (Employee).GetTypeInfo();
        var employeeField = orderType.Fields["Employee"];
        foreach (var key in prefetcher) {
          GC.Collect(2, GCCollectionMode.Forced);
          GC.WaitForPendingFinalizers();
          var orderState = session.EntityStateCache[key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, orderType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          var employeeKey = Key.Create<Person>(employeeField.Associations.Last()
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
        keys = session.Query.All<Person>().Take(221).AsEnumerable().Select(p => Key.Create<Person>(p.Key.Value))
          .ToList();
        Assert.IsTrue(keys.All(key => !key.HasExactType));
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetcher = keys.Prefetch<Person, Key>(key => key)/*.Prefetch(p => p.Name)*/
          .PrefetchSingle(p => new Customer {Name = p.Name}, customer => {
            customer.First().Remove();
            CollectGarbadge();
            return customer;
          });
        foreach (var key in prefetcher) {
          var orderState = session.EntityStateCache[key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderState.Key, orderState.Key.Type,
            session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test, Category("Performance")]
    public void PrefetchEntitySetTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Order>().Take(221).AsEnumerable().Select(p => Key.Create<Order>(p.Key.Value))
          .ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.Details);
        var orderType = typeof (Order).GetTypeInfo();
        var detailsField = orderType.Fields["Details"];
        foreach (var key in prefetcher) {
          CollectGarbadge();
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, orderType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          EntitySetState state;
          session.Handler.TryGetEntitySetState(key, detailsField, out state);
          Assert.IsTrue(state.IsFullyLoaded);
          foreach (var detailKey in state) {
            Assert.IsTrue(detailKey.HasExactType);
            var detailState = session.EntityStateCache[detailKey, false];
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(detailKey, detailKey.Type, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
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