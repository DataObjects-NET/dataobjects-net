// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetcherTest : NorthwindDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.NamingConvention.NamespacePolicy = NamespacePolicy.AsIs;
      config.Types.Register(typeof (Model.Offer).Assembly, typeof (Model.Offer).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var recreateConfig = configuration.Clone();
      var domain = Domain.Build(configuration);
      DataBaseFiller.Fill(domain);
      return domain;
    }

    [Test]
    public void EnumerableOfNonEntityTest()
    {
      List<Key> keys;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        keys = Query.All<Order>().Select(o => o.Key).ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.Employee);
        var orderType = typeof (Order).GetTypeInfo();
        var employeeField = orderType.Fields["Employee"];
        var employeeType = typeof (Employee).GetTypeInfo();
        Func<FieldInfo, bool> fieldSelector = field => field.IsPrimaryKey || field.IsSystem
          || !field.IsLazyLoad && !field.IsEntitySet;
        foreach (var key in prefetcher) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, key.Type, session, fieldSelector);
          var orderState = session.EntityStateCache[key, true];
          var employeeKey = Key.Create(Domain, typeof(Employee).GetTypeInfo(Domain),
            TypeReferenceAccuracy.ExactType, employeeField.Association
              .ExtractForeignKey(orderState.Type, orderState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session, fieldSelector);
        }
      }
    }

    [Test]
    public void EnumerableOfEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = Query.All<Order>().Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails);
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
        var expected = Query.All<Order>().ToList();
        var actual = expected.Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PrefetchManyNotFullBatchTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var employeesField = typeof (Territory).GetTypeInfo().Fields["Employees"];
        var ordersField = typeof (Employee).GetTypeInfo().Fields["Orders"];
        var prefetcher = Query.All<Territory>().Prefetch(t => t.Employees,
          employees => employees.Prefetch(e => e.Orders));
        foreach (var territory in prefetcher) {
          var entitySetState = GetFullyLoadedEntitySet(session, territory.Key, employeesField);
          foreach (var employeeKey in entitySetState)
            GetFullyLoadedEntitySet(session, employeeKey, ordersField);
        }
        Assert.AreEqual(2, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchManySeveralBatchesTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var detailsField = typeof (Order).GetTypeInfo().Fields["OrderDetails"];
        var productField = typeof (OrderDetails).GetTypeInfo().Fields["Product"];
        var prefetcher = Query.All<Order>().Take(500).Prefetch(o => o.OrderDetails)
          .PrefetchMany(o => o.OrderDetails,
          details => details.Prefetch(d => d.Product));
        foreach (var order in prefetcher) {
          var entitySetState = GetFullyLoadedEntitySet(session, order.Key, detailsField);
          foreach (var detailKey in entitySetState) {
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(detailKey, detailKey.Type, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
            PrefetchTestHelper.AssertReferencedEntityIsLoaded(detailKey, session, productField);
          }
        }
        Assert.AreEqual(24, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchSingleTest()
    {
      List<Key> keys;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open())
        keys = Query.All<Order>().Select(o => o.Key).ToList();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var orderType = typeof (Order).GetTypeInfo();
        var employeeType = typeof (Employee).GetTypeInfo();
        var employeeField = typeof (Order).GetTypeInfo().Fields["Employee"];
        var ordersField = typeof (Employee).GetTypeInfo().Fields["Orders"];
        var prefetcher = keys.Prefetch<Order, Key>(key => key).Prefetch(o => o.Employee)
          .PrefetchSingle(o => o.Employee, employee => employee.Prefetch(e => e.Orders));
        var count = 0;
        foreach (var orderKey in prefetcher) {
          Assert.AreEqual(keys[count], orderKey);
          count++;
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType, session,
            field => PrefetchHelper.IsFieldToBeLoadedByDefault(field)
              || field.Equals(employeeField) || (field.Parent != null && field.Parent.Equals(employeeField)));
          var state = session.EntityStateCache[orderKey, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(
            state.Entity.GetFieldValue<Employee>(employeeField).Key,
            employeeType, session, field =>
              PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(ordersField));
        }
        Assert.AreEqual(keys.Count, count);
        Assert.GreaterOrEqual(11, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PreservingOrderInPrefetchManyNotFullBatchTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query.All<Territory>().ToList();
        var actual = expected./*Prefetch(t => t.Employees).*/PrefetchMany(t => t.Employees,
          employees => employees.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreserveOrderingInPrefetchManySeveralBatchesTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query.All<Order>().ToList();
        var actual = expected./*Prefetch(o => o.OrderDetails).*/PrefetchMany(o => o.OrderDetails,
          details => details.Prefetch(d => d.Product)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleNotFullBatchTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query.All<Order>().Take(53).ToList();
        var actual = expected./*Prefetch(o => o.Employee).*/PrefetchSingle(o => o.Employee,
          employee => employee.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleSeveralBatchesTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var expected = Query.All<Order>().ToList();
        var actual = expected./*Prefetch(o => o.Employee).*/PrefetchSingle(o => o.Employee,
          employee => employee.Prefetch(e => e.Orders)).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void InvalidArgumentsTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        AssertEx.Throws<ArgumentException>(() => Query.All<Territory>().Prefetch(t => new {t.Id, t.Region}));
        AssertEx.Throws<ArgumentException>(() => Query.All<Territory>().Prefetch(t => t.Region.RegionDescription));
        AssertEx.Throws<ArgumentException>(() => Query.All<Territory>().Prefetch(t => t.PersistenceState));
        AssertEx.Throws<ArgumentException>(() => EnumerableUtils
          .One(Key.Create<Model.OfferContainer>(Domain, 1))
          .Prefetch<Model.OfferContainer, Key>(key => key)
          .Prefetch(oc => oc.IntermediateOffer.AnotherContainer.RealOffer.Book));
      }
    }

    [Test]
    public void SimultaneouslyUsageOfMultipleEnumeratorsTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var source = Query.All<Order>().ToList();
        var prefetcher = source.Prefetch(o => o.OrderDetails);
        using (var enumerator0 = prefetcher.GetEnumerator()) {
          enumerator0.MoveNext();
          enumerator0.MoveNext();
          enumerator0.MoveNext();
          Assert.IsTrue(source.SequenceEqual(prefetcher));
          var index = 3;
          while (enumerator0.MoveNext())
            Assert.AreSame(source[index++], enumerator0.Current);
          Assert.AreEqual(source.Count, index);
        }
      }
    }

    [Test]
    public void StructureFieldsPrefetchTest()
    {
      Key containerKey;
      Key bookShop0Key;
      Key book0Key;
      Key bookShop1Key;
      Key book1Key;
      PrefetchTestHelper.CreateOfferContainer(Domain, out containerKey, out book0Key, out bookShop0Key,
        out book1Key, out bookShop1Key);

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = EnumerableUtils.One(containerKey).Prefetch<Model.OfferContainer, Key>(key => key)
          .Prefetch(oc => oc.RealOffer.Book).Prefetch(oc => oc.IntermediateOffer.RealOffer.BookShop);
        foreach (var key in prefetcher) {
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book0Key, book0Key.Type, session);
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(bookShop1Key, bookShop1Key.Type, session);
        }
      }
    }

    [Test]
    public void StructurePrefetchTest()
    {
      Key containerKey;
      Key bookShop0Key;
      Key book0Key;
      Key bookShop1Key;
      Key book1Key;
      PrefetchTestHelper.CreateOfferContainer(Domain, out containerKey, out book0Key, out bookShop0Key,
        out book1Key, out bookShop1Key);

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetcher = EnumerableUtils.One(containerKey).Prefetch<Model.OfferContainer, Key>(key => key)
          .Prefetch(oc => oc.IntermediateOffer);
        foreach (var key in prefetcher) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(containerKey, containerKey.Type, session,
            field => PrefetchTestHelper.IsFieldToBeLoadedByDefault(field)
              || field.Name.StartsWith("IntermediateOffer"));
        }
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
  }
}