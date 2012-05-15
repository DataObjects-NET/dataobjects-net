// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.07

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchDelayedElementsTest : AutoBuildTest
  {
    private TypeInfo orderType;
    private TypeInfo customerType;

    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
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
        orderType = typeof (Order).GetTypeInfo();
        customerType = typeof (Customer).GetTypeInfo();
        transactionScope.Complete();
      }
    }

    [Test]
    public void SimpleTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Person>().AsEnumerable().Select(p => Key.Create<Person>(Domain, p.Key.Value)).ToList();
        Assert.IsTrue(keys.All(key => !key.HasExactType));
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var persons =  session.Query.Many<Person>(keys);
        var count = 0;
        foreach (var person in persons) {
          count++;
          Key cachedKey;
          Assert.IsTrue(Domain.KeyCache.TryGetItem(person.Key, true, out cachedKey));
          Assert.IsTrue(cachedKey.HasExactType);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(person.Key, cachedKey.TypeInfo, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
        Assert.AreEqual(keys.Count, count);
      }
    }

    [Test]
    public void PrefetchManyTest()
    {
      List<Key> keys;
      int actualEmployeeCount;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Customer>().Where(c => c.Name == "Customer1").AsEnumerable()
          .Select(p => Key.Create<Person>(Domain, p.Key.Value)).ToList();
        actualEmployeeCount = session.Query.All<Employee>().Where(e => e.Name == "Employee1").Count();
        Assert.IsTrue(keys.All(key => !key.HasExactType));
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ordersField = customerType.Fields["Orders"];
        var employeeField = orderType.Fields["Employee"];
        var employeeType = typeof (Employee).GetTypeInfo();
        session.Query.Many<Customer>(keys)
          .Prefetch(c => c.Orders.Prefetch(o => o.Employee))
          .Run();
        var customerCount = 0;
        var expectedEmployeeCount = 0;
        foreach (var key in keys) {
          customerCount++;
          var cachedKey = GetCachedKey(key, session);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, cachedKey.TypeInfo, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          EntitySetState state;
          session.Handler.LookupState(key, ordersField, out state);
          Assert.IsTrue(state.IsFullyLoaded);
          Assert.Greater(state.TotalItemCount, 0);
          foreach (var orderKey in state) {
            expectedEmployeeCount++;
            var orderState = session.EntityStateCache[orderKey, true];
            Assert.IsNotNull(orderState);
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
            var employeeKey = Key.Create<Person>(Domain, employeeField.Associations.Last()
              .ExtractForeignKey(orderState.Type, orderState.Tuple));
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          }
        }
        Assert.AreEqual(keys.Count, customerCount);
        Assert.AreEqual(expectedEmployeeCount / 2, actualEmployeeCount);
      }
    }

    [Test]
    public void PrefetchViaEntityTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Customer>().AsEnumerable().Select(p => Key.Create<Person>(Domain, p.Key.Value)).ToList();
        Assert.IsTrue(keys.All(key => !key.HasExactType));
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Query.Many<Entity>(keys).Run();
        foreach (var key in keys) {
          var cachedKey = GetCachedKey(key, session);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(cachedKey, customerType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test]
    public void ThreeStepsFetchTest()
    {
      var keys = GetKeys<Person>(20);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchCount = session.Handler.PrefetchTaskExecutionCount;
        session.Query.Many<AdvancedPerson>(keys).Run();
        foreach (var key in keys) {
          var cachedKey = GetCachedKey(key, session);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(cachedKey, cachedKey.TypeInfo, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
        Assert.AreEqual(prefetchCount + 2, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void NotFullPrefetchBatchTest()
    {
      var keys = GetKeys<Person>(120);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var count = 0;
        var enumerable = keys.Take(15);
        session.Query.Many<AdvancedPerson>(enumerable).Run();
        foreach (var key in enumerable) {
          count++;
          var cachedKey = GetCachedKey(key, session);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(cachedKey, cachedKey.TypeInfo, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
        Assert.AreEqual(15, count);
        var prefetchCount = session.Handler.PrefetchTaskExecutionCount;
        session.Query.Many<AdvancedPerson>(keys).Run();
        count = 0;
        foreach (var key in keys) {
          count++;
          var cachedKey = GetCachedKey(key, session);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(cachedKey, cachedKey.TypeInfo, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
        Assert.AreEqual(keys.Count, count);
        Assert.AreEqual(prefetchCount + 4, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void FullLoadingOfReferencedEntityTest()
    {
      var keys = new List<Key>();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (int i = 0; i < 151; i++) {
          var title = new Title {Text = "abc", Language = "En"};
          keys.Add(new Book {Category = (i % 10).ToString(), Title = title}.Key);
          keys.Add(new Book {Category = (i % 10).ToString()}.Key);
        }
        tx.Complete();
      }

      TestFullLoadingOfReferencedEntities(keys);
      TestFullLoadingOfReferencedEntities(keys
        .Select(key => Key.Create(Domain, typeof (IHasCategory), key.Value)));
      var hasExactType = true;
      TestFullLoadingOfReferencedEntities(keys
        .Select(key => {
          var result = hasExactType ? key : Key.Create(Domain, typeof (IHasCategory), key.Value);
          hasExactType = !hasExactType;
          return result;
        }));
    }

    [Test]
    public void FullLoadingOfDirectEntitySetItemsTest()
    {
      var keys = new List<Key>();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Action<Book, int> titlesGenerator = (b, count) => {
          var subcount = count / 2;
          var countleft = count - subcount;
          for (var i = 0; i < subcount; i++)
            b.TranslationTitles.Add(new Title {Text = i.ToString()});
          for (var i = 0; i < countleft; i++)
            b.TranslationTitles.Add(new AnotherTitle { Text = "A_"+i.ToString() });
        };
        for (var i = 0; i < 155; i++) {
          var book = new Book {
            Category = (i % 10).ToString(), Title = new Title {Text = "abc", Language = "En"}
          };
          titlesGenerator.Invoke(book, i % 20);
          keys.Add(book.Key);
        }
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Query.Many<Book>(keys)
          .Prefetch(b => b.TranslationTitles)
          .Run();
        var bookType = typeof (Book).GetTypeInfo();
        var translationTitlesField = bookType.Fields["TranslationTitles"];
        var titleType = typeof (Title).GetTypeInfo();
        var isOneItemPresentAtLeast = false;
        foreach (var key in keys)
          AssertEntitySetItemsAreFullyLoaded(key, bookType, translationTitlesField, session, ref isOneItemPresentAtLeast);
        Assert.IsTrue(isOneItemPresentAtLeast);
      }
    }

    [Test]
    public void FullLoadingOfIndirectEntitySetItemsTest()
    {
      var publisherKeys = new List<Key>();
      var bookShopKeys = new List<Key>();
      TypeInfo publisherType;
      TypeInfo bookShopType;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        publisherType = typeof (Publisher).GetTypeInfo();
        bookShopType = typeof (BookShop).GetTypeInfo();
        Action<Publisher, int> titlesGenerator = (p, count) => {
          for (var i = 0; i < count; i++) {
            var bookShop = new BookShop {Url = "http://" + i.ToString(), Name = i.ToString()};
            bookShopKeys.Add(bookShop.Key);
            p.Distributors.Add(bookShop);
          }
        };
        for (var i = 0; i < 155; i++) {
          var publisher = new Publisher {Country = "C"};
          titlesGenerator.Invoke(publisher, i % 20);
          publisherKeys.Add(publisher.Key);
        }
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Query.Many<Publisher>(publisherKeys)
          .Prefetch(p => p.Distributors)
          .Run();
        var distributorsField = publisherType.Fields["Distributors"];
        var isOneItemPresentAtLeast = false;
        foreach (var key in publisherKeys)
          AssertEntitySetItemsAreFullyLoaded(key, publisherType, distributorsField,
            session, ref isOneItemPresentAtLeast);
        Assert.IsTrue(isOneItemPresentAtLeast);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Query.Many<BookShop>(bookShopKeys)
          .Prefetch(p => p.Suppliers)
          .Run();
        var suppliersField = bookShopType.Fields["Suppliers"];
        var isOneItemPresentAtLeast = false;
        foreach (var key in bookShopKeys)
          AssertEntitySetItemsAreFullyLoaded(key, bookShopType, suppliersField,
            session, ref isOneItemPresentAtLeast);
        Assert.IsTrue(isOneItemPresentAtLeast);
      }
    }

    [Test]
    public void ReferenceToSelfPrefetchTest()
    {
      TypeInfo referenceToSelfType;
      var keys = new List<Key>();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        referenceToSelfType = typeof (ReferenceToSelf).GetTypeInfo();
        var reference = new ReferenceToSelf {AuxField = 3};
        keys.Add(reference.Key);
        reference.Reference = reference;
        reference = new ReferenceToSelf {AuxField = 5};
        keys.Add(Key.Create(Domain, typeof (IReferenceToSelf), reference.Key.Value));
        reference.Reference = reference;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Query.Many<IReferenceToSelf>(keys)
          .Prefetch(r => r.Reference)
          .Run();
        foreach (var key in keys) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, referenceToSelfType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    public static Key GetCachedKey(Key key, Session session)
    {
      EntityState state;
      Assert.IsFalse(key.HasExactType);
      Assert.IsTrue(session.EntityStateCache.TryGetItem(key, true, out state));
      Assert.IsTrue(state.Key.HasExactType);
      return state.Key;
    }

    private List<Key> GetKeys<T>(int count)
      where T : Entity
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<T>().Take(count).AsEnumerable().Select(p => Key.Create<T>(Domain, p.Key.Value)).ToList();
        Assert.IsTrue(keys.All(key => !key.HasExactType));
        Assert.Greater(keys.Count, 0);
      }
      return keys;
    }

    private void TestFullLoadingOfReferencedEntities(IEnumerable<Key> keys)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        session.Query.Many<Book>(keys)
          .Prefetch(b => b.Title)
          .Run();
        var bookType = typeof (Book).GetTypeInfo();
        var titleField = bookType.Fields["Title"];
        var titleType = typeof (Title).GetTypeInfo();
        foreach (var key in keys) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, bookType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
          var ownerState = session.EntityStateCache[key, true];
          var titleKeyValue = titleField.Associations.Last().ExtractForeignKey(ownerState.Type, ownerState.Tuple);
          if ((titleKeyValue.GetFieldState(0) & TupleFieldState.Null) == TupleFieldState.Null)
            continue;
          var titleKey = Key.Create(Domain, typeof (Title), titleKeyValue);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(titleKey, titleType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    private static void AssertEntitySetItemsAreFullyLoaded(Key ownerKey, TypeInfo ownerType,
      FieldInfo referencingField, Session session, ref bool isOneItemPresentAtLeast)
    {
      PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(ownerKey, ownerType, session,
        PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      EntitySetState setState;
      session.Handler.LookupState(ownerKey, referencingField, out setState);
      Assert.IsTrue(setState.IsFullyLoaded);
      foreach (var itemKey in setState) {
        isOneItemPresentAtLeast = true;
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(itemKey, itemKey.TypeInfo, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }
  }
}