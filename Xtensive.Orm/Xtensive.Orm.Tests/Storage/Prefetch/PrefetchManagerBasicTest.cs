// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.07

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Helpers;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public class PrefetchManagerBasicTest : PrefetchManagerTestBase
  {
    private static int instanceCount;

    #region Nested class

    public class MemoryLeakTester
    {
      ~MemoryLeakTester()
      {
        instanceCount--;
      }
    }

    #endregion

    [Test]
    public void EntityByKeyWithKnownTypePrefetchTest()
    {
      Key customerKey;
      Key orderKey;
      Key productKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        customerKey = GetFirstKeyInCurrentSession<Customer>();
        orderKey = GetFirstKeyInCurrentSession<Order>();
        productKey = GetFirstKeyInCurrentSession<Product>();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        AssertEntityStateIsNotLoaded(customerKey, session);
        AssertEntityStateIsNotLoaded(orderKey, session);
        AssertEntityStateIsNotLoaded(productKey, session);
        PrefetchIntrinsicFields(prefetchManager, customerKey, typeof(Customer));
        PrefetchIntrinsicFields(prefetchManager, orderKey, typeof(Order));
        PrefetchIntrinsicFields(prefetchManager, productKey, typeof(Product));
        prefetchManager.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, CustomerType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(productKey, ProductType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void ReferencedEntitiesByUnknownForeignKeysPrefetchTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      Key orderKey0;
      Key orderKey1;
      Key orderKey2;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        orderKey0 = session.Query.All<Order>().OrderBy(o => o.Id).First().Key;
        orderKey1 = session.Query.All<Order>().OrderBy(o => o.Id).Skip(1).First().Key;
        orderKey2 = session.Query.All<Order>().OrderBy(o => o.Id).Skip(2).First().Key;
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);

        prefetchManager.InvokePrefetch(orderKey0, null, new PrefetchFieldDescriptor(CustomerField, true, true));
        prefetchManager.InvokePrefetch(orderKey0, null, new PrefetchFieldDescriptor(EmployeeField, true, true));

        prefetchManager.InvokePrefetch(orderKey1, null, new PrefetchFieldDescriptor(CustomerField, true, true));

        prefetchManager.InvokePrefetch(orderKey2, null, new PrefetchFieldDescriptor(EmployeeField, true, true));
        prefetchManager.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey0, OrderType, session, field =>
          field==CustomerField || field==EmployeeField || IsFieldKeyOrSystem(field)
            || (field.Parent != null && (field.Parent==CustomerField || field.Parent==EmployeeField)));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey1, OrderType, session, field =>
          field==CustomerField || IsFieldKeyOrSystem(field)
            || (field.Parent != null && field.Parent==CustomerField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey2, OrderType, session, field =>
          field==EmployeeField || IsFieldKeyOrSystem(field)
            || (field.Parent != null && field.Parent==EmployeeField));

        PrefetchTestHelper.AssertReferencedEntityIsLoaded(orderKey0, session, CustomerField);
        PrefetchTestHelper.AssertReferencedEntityIsLoaded(orderKey0, session, EmployeeField);

        PrefetchTestHelper.AssertReferencedEntityIsLoaded(orderKey1, session, CustomerField);

        PrefetchTestHelper.AssertReferencedEntityIsLoaded(orderKey2, session, EmployeeField);
      }
    }
    
    [Test]
    public void EntityByKeyWithUnknownTypePrefetchTest()
    {
      var customerKey = GetFirstKey<Customer>();

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var supplierType = Domain.Model.Types[typeof (Supplier)];
        var keyWithoutType = Key.Create(Domain, CustomerType.Hierarchy.Root,
          TypeReferenceAccuracy.BaseType, customerKey.Value);
        AssertEx.Throws<ArgumentNullException>(() =>
          prefetchManager.InvokePrefetch(keyWithoutType, null, new PrefetchFieldDescriptor(AgeField)));
        AssertEx.Throws<InvalidOperationException>(() =>
          prefetchManager.InvokePrefetch(keyWithoutType, supplierType, new PrefetchFieldDescriptor(CityField)));
        prefetchManager.InvokePrefetch(keyWithoutType, CustomerType, new PrefetchFieldDescriptor(CityField));
        prefetchManager.InvokePrefetch(keyWithoutType, supplierType, new PrefetchFieldDescriptor(AgeField));
        prefetchManager.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, CustomerType, session,
          field => IsFieldKeyOrSystem(field)
            || field == CityField || field.DeclaringType == CustomerType.Hierarchy.Root);
      }
    }

    [Test]
    public void DirectEntitySetTypePrefetchTest()
    {
      Key orderKey;
      Key[] orderDetailKeys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var order = session.Query.All<Order>().OrderBy(c => c.Id).First();
        orderKey = order.Key;
        orderDetailKeys = order.Details.Select(detail => detail.Key).ToArray();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(OrderType.Fields["Details"]));
        prefetchManager.ExecuteTasks();

        var orderDetailsType = typeof (OrderDetail).GetTypeInfo();
        Assert.AreEqual(prevEntityStateCount + orderDetailKeys.Length + 1, session.EntityStateCache.Count);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType, session, IsFieldKeyOrSystem);
        Assert.IsTrue(session.EntityStateCache.ContainsKey(orderKey));
        foreach (var key in orderDetailKeys) {
          Assert.IsTrue(session.EntityStateCache.ContainsKey(key));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, orderDetailsType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test]
    public void IndirectEntitySetPrefetchTest()
    {
      TypeInfo authorType;
      Key bookKey;
      Key authorKey;
      Key author0Key;
      Key author2Key;
      Key author4Key;
      Key book3Key;
      Key book4Key;
      Key book5Key;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        authorType = typeof (Author).GetTypeInfo();

        var book0 = new Book();
        bookKey = book0.Key;
        var book1 = new Book();
        var book2 = new Book();
        var book3 = new Book();
        book3Key = book3.Key;
        var book4 = new Book();
        book4Key = book4.Key;
        var book5 = new Book();
        book5Key = book5.Key;

        var author0 = new Author();
        author0Key = author0.Key;
        var author1 = new Author();
        var author2 = new Author();
        author2Key = author2.Key;
        var author3 = new Author();
        var author4 = new Author();
        author4Key = author4.Key;
        var author5 = new Author();
        authorKey = author5.Key;

        author0.Books.Add(book0);
        author0.Books.Add(book1);
        author0.Books.Add(book2);

        author1.Books.Add(book3);
        author1.Books.Add(book4);
        author1.Books.Add(book5);

        author2.Books.Add(book0);
        author2.Books.Add(book1);
        author2.Books.Add(book2);

        author3.Books.Add(book3);
        author3.Books.Add(book4);
        author3.Books.Add(book5);

        author4.Books.Add(book0);
        author4.Books.Add(book1);
        author4.Books.Add(book2);

        author5.Books.Add(book3);
        author5.Books.Add(book4);
        author5.Books.Add(book5);

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchManager.InvokePrefetch(bookKey, null, new PrefetchFieldDescriptor(bookKey.TypeInfo.Fields["Authors"]));
        prefetchManager.ExecuteTasks();

        Assert.AreEqual(prevEntityStateCount + 4, session.EntityStateCache.Count);
        Assert.IsTrue(session.EntityStateCache.ContainsKey(bookKey));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(author0Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(author2Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(author4Key));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey, BookType, session, IsFieldKeyOrSystem);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(author0Key, authorType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(author2Key, authorType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(author4Key, authorType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchManager.InvokePrefetch(authorKey, null, new PrefetchFieldDescriptor(BooksField));
        prefetchManager.ExecuteTasks();

        Assert.AreEqual(prevEntityStateCount + 4, session.EntityStateCache.Count);
        Assert.IsTrue(session.EntityStateCache.ContainsKey(authorKey));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(book3Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(book4Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(book5Key));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(authorKey, authorType, session,
          IsFieldKeyOrSystem);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(book3Key, BookType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(book4Key, BookType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(book5Key, BookType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void EntitySetLimitedPrefetchTest()
    {
      Key order0Key;
      Key author0Key;
      Key order1Key;
      Key author1Key;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var order0 = new Order {Number = 1, Customer = null, Employee = null};
        order0Key = order0.Key;
        var order0Detail1 = new OrderDetail {Order = order0, Product = null, Count = 100};
        var order0Detail2 = new OrderDetail {Order = order0, Product = null, Count = 200};
        var order0Detail3 = new OrderDetail {Order = order0, Product = null, Count = 300};
        var order0Detail4 = new OrderDetail {Order = order0, Product = null, Count = 400};

        var order1 = new Order {Number = 1, Customer = null, Employee = null};
        order1Key = order1.Key;
        var order1Detail1 = new OrderDetail {Order = order1, Product = null, Count = 100};
        var order1Detail2 = new OrderDetail {Order = order1, Product = null, Count = 200};
        var order1Detail3 = new OrderDetail {Order = order1, Product = null, Count = 300};
        var order1Detail4 = new OrderDetail {Order = order1, Product = null, Count = 400};

        var book0 = new Book();
        var book1 = new Book();
        var book2 = new Book();
        var book3 = new Book();
        var book4 = new Book();
        var book5 = new Book();

        var author0 = new Author();
        author0Key = author0.Key;

        author0.Books.Add(book0);
        author0.Books.Add(book1);
        author0.Books.Add(book2);
        author0.Books.Add(book3);
        author0.Books.Add(book4);
        author0.Books.Add(book5);

        var author1 = new Author();
        author1Key = author1.Key;

        author1.Books.Add(book1);
        author1.Books.Add(book2);
        author1.Books.Add(book3);
        author1.Books.Add(book4);
        author1.Books.Add(book5);

        tx.Complete();
      }

      const int itemCount = 3;

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(author0Key, null, new PrefetchFieldDescriptor(BooksField));
        prefetchManager.InvokePrefetch(order0Key, null, new PrefetchFieldDescriptor(DetailsField));
        prefetchManager.InvokePrefetch(author1Key, null, new PrefetchFieldDescriptor(BooksField, itemCount));
        prefetchManager.InvokePrefetch(order1Key, null, new PrefetchFieldDescriptor(DetailsField, itemCount));
        prefetchManager.ExecuteTasks();

        ValidateLoadedEntitySet(author0Key, BooksField, 6, true, session);
        ValidateLoadedEntitySet(order0Key, DetailsField, 4, true, session);
        ValidateLoadedEntitySet(author1Key, BooksField, itemCount, false,  session);
        ValidateLoadedEntitySet(order1Key, DetailsField, itemCount, false,  session);
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(author0Key, null, new PrefetchFieldDescriptor(BooksField, itemCount - 1));
        prefetchManager.InvokePrefetch(author1Key, null, new PrefetchFieldDescriptor(BooksField, itemCount));
        prefetchManager.InvokePrefetch(order0Key, null, new PrefetchFieldDescriptor(DetailsField, itemCount - 1));
        prefetchManager.InvokePrefetch(order1Key, null, new PrefetchFieldDescriptor(DetailsField, itemCount));
        prefetchManager.ExecuteTasks();

        ValidateLoadedEntitySet(author0Key, BooksField, itemCount - 1, false,  session);
        ValidateLoadedEntitySet(author1Key, BooksField, itemCount, false,  session);
        ValidateLoadedEntitySet(order0Key, DetailsField, itemCount - 1, false,  session);
        ValidateLoadedEntitySet(order1Key, DetailsField, itemCount, false,  session);
      }
    }

    [Test]
    public void ReferencedEntityByKnownForeignKeyPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var orderPrimaryIndex = OrderType.Indexes.PrimaryIndex;
        var selectedColumns = orderPrimaryIndex.ColumnIndexMap.System
          .Concat(EmployeeField.Columns.Select(column => orderPrimaryIndex.Columns.IndexOf(column))).ToArray();
        var orderQuery = OrderType.Indexes.PrimaryIndex.ToRecordQuery()
          .Filter(t => t.GetValue<int>(0)==orderKey.Value.GetValue<int>(0)).Select(selectedColumns);
        orderQuery.ToRecordSet(session).ToEntities(0).Single();
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);

        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField, true, true));
        var graphContainers = (SetSlim<GraphContainer>) GraphContainersField.GetValue(prefetchManager);
        Assert.AreEqual(2, graphContainers.Count);
        foreach (var container in graphContainers)
          Assert.IsNull(container.ReferencedEntityContainers);
        var orderContainer = graphContainers.Where(container => container.Key==orderKey).SingleOrDefault();
        var employeeContainer = graphContainers.Where(container => container.Key!=orderKey).SingleOrDefault();
        Assert.IsNotNull(orderContainer);
        Assert.IsNotNull(employeeContainer);
        prefetchManager.ExecuteTasks();
        Assert.IsNull(orderContainer.RootEntityContainer.Task);
        Assert.IsNotNull(employeeContainer.RootEntityContainer.Task);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType,
          session, field => IsFieldKeyOrSystem(field) || field == EmployeeField
            || field.Parent == EmployeeField);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeContainer.Key, Domain.Model.Types[typeof (Employee)],
          session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void ReferencedEntityByNullAsForeignKeyPrefetchTest()
    {
      Key orderKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var order = session.Query.All<Order>().OrderBy(o => o.Id).First();
        var newOrder = new Order {Employee = null, Customer = order.Customer};
        orderKey = newOrder.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField, true, true));
        var graphContainer = GetSingleGraphContainer(prefetchManager);
        Assert.AreEqual(1, graphContainer.ReferencedEntityContainers.Count());
        var referencedEntityContainer = graphContainer.ReferencedEntityContainers.Single();
        prefetchManager.ExecuteTasks();
        Assert.IsNull(referencedEntityContainer.Task);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderKey.TypeInfo, session,
          field => IsFieldKeyOrSystem(field) || field == EmployeeField
            || field.Parent == EmployeeField);
        Assert.IsNull(session.Query.Single<Order>(orderKey).Employee);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        session.Handler.FetchEntityState(orderKey);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField, true, true));
        var taskContainers = (SetSlim<GraphContainer>) GraphContainersField.GetValue(prefetchManager);
        Assert.AreEqual(1, taskContainers.Count);
        Assert.AreEqual(orderKey, taskContainers.Single().Key);
      }
    }

    [Test]
    public void MergingOfRequestsInTaskContainerTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField));
        var originalGraphContainer = GetSingleGraphContainer(prefetchManager);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField));
        Assert.AreSame(originalGraphContainer, GetSingleGraphContainer(prefetchManager));
        prefetchManager.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderKey.TypeInfo, session,
          field => IsFieldKeyOrSystem(field) || field == CustomerField || field == EmployeeField
            || (field.Parent != null && (field.Parent == CustomerField || field.Parent == EmployeeField)));
      }
    }

    [Test]
    public void FieldDeclaredInInterfacePrefetchTest()
    {
      Key bookKey0;
      Key bookKey1;
      Key bookKey2;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        bookKey0 = new Book {Category = "0"}.Key;
        bookKey1 = new Book {Category = "1"}.Key;
        bookKey2 = new Book {Category = "2"}.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var iHasCategoryType = Domain.Model.Types[typeof (IHasCategory)];
        var categoryField = iHasCategoryType.Fields["Category"];
        var bookCategoryField = BookType.FieldMap[categoryField];
        prefetchManager.InvokePrefetch(bookKey0, null, new PrefetchFieldDescriptor(categoryField, false, false));
        var interfaceKey = Key.Create(Domain, iHasCategoryType, TypeReferenceAccuracy.BaseType, bookKey1.Value);
        prefetchManager.InvokePrefetch(interfaceKey, iHasCategoryType,
          new PrefetchFieldDescriptor(categoryField, false, false));
        prefetchManager.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey0, BookType, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(bookCategoryField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(interfaceKey, BookType, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(bookCategoryField));

        AssertEx.Throws<InvalidOperationException>(
          () => prefetchManager.InvokePrefetch(Key
            .Create(Domain, iHasCategoryType, TypeReferenceAccuracy.BaseType, bookKey2.Value),
          iHasCategoryType, new PrefetchFieldDescriptor(bookCategoryField, false, false)));
      }
    }

    [Test]
    public void FieldReferencingToInterfacePrefetchTest()
    {
      Key bookKey;
      Key titleKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var title = new Title {Text = "abc"};
        titleKey = title.Key;
        var book = new Book {Category = "1", Title = title};
        bookKey = book.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var titleField = bookKey.TypeInfo.Fields["Title"];
        var titleType = typeof (Title);
        var textField = titleType.GetTypeInfo().Fields["Text"];
        var bookField = titleType.GetTypeInfo().Fields["Book"];
        prefetchManager.InvokePrefetch(bookKey, null, new PrefetchFieldDescriptor(titleField, true, true));
        prefetchManager.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey, bookKey.TypeInfo, session,
          field => IsFieldKeyOrSystem(field) || field == titleField
            || (field.Parent != null && field.Parent == titleField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(titleKey, titleKey.TypeInfo, session,
          field => IsFieldKeyOrSystem(field) || field == textField || field == bookField
            || field.Parent == bookField);
      }
    }

    [Test]
    public void DirectEntitySetContainingReferencesToInterfacePrefetchTest()
    {
      const int instanceCount = 40;
      Key bookKey;
      Key titleKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Action<Book, int> titlesGenerator = (b, seed) => {
          for (int i = seed; i < seed + instanceCount; i++)
            b.TranslationTitles.Add(new Title {Text = i.ToString()});
        };
        var book0 = new Book {Category = "0", Title = new Title {Text = "abc"}};
        titlesGenerator.Invoke(book0, 0);
        var book1 = new Book {Category = "1", Title = new Title {Text = "def"}};
        titlesGenerator.Invoke(book1, 100);
        bookKey = book1.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var translationTitlesField = BookType.Fields["TranslationTitles"];
        prefetchManager.InvokePrefetch(bookKey, null, new PrefetchFieldDescriptor(translationTitlesField));
        prefetchManager.ExecuteTasks();

        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(bookKey, translationTitlesField, out setState));
        Assert.IsTrue(setState.IsFullyLoaded);
        Assert.AreEqual(instanceCount, setState.TotalItemCount);
        var iTitleType = typeof (ITitle).GetTypeInfo();
        foreach (var key in setState)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, iTitleType, session, field => true);
      }
    }

    [Test]
    public void IndirectEntitySetContainingReferencesToInterfacePrefetchTest()
    {
      Key publisherKey0;
      Key publisherKey1;
      Key publisherKey2;
      Key publisherKey3;
      Key publisherKey4;

      Key bookShopKey0;
      Key bookShopKey1;
      Key bookShopKey2;
      Key bookShopKey3;
      Key bookShopKey4;

      TypeInfo bookShopType;
      TypeInfo publisherType;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        publisherType = typeof (Publisher).GetTypeInfo();
        bookShopType = typeof (BookShop).GetTypeInfo();

        CreatePublishersAndBookShops(out publisherKey0, out bookShopKey0, out bookShopKey1, out bookShopKey2,
          out bookShopKey3, out publisherKey1, out publisherKey2, out publisherKey3);
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var distributorsField = publisherType.Fields["Distributors"];
        var urlField = bookShopType.Fields["Url"];
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(publisherKey0, null, new PrefetchFieldDescriptor(distributorsField));
        prefetchManager.ExecuteTasks();

        var iBookShopType = typeof (IBookShop).GetTypeInfo();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(publisherKey0, publisherType, session,
          IsFieldKeyOrSystem);
        var bookShopKeys = new[] {bookShopKey0, bookShopKey1, bookShopKey2, bookShopKey3};
        foreach (var bookShopKey in bookShopKeys)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookShopKey, bookShopKey.TypeInfo, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(urlField));

        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(publisherKey0, distributorsField, out setState));
        Assert.IsTrue(setState.IsFullyLoaded);
        Assert.AreEqual(4, setState.TotalItemCount);
        foreach (var bookShopKey in bookShopKeys)
          Assert.IsTrue(setState.Contains(bookShopKey));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var suppliersField = bookShopType.Fields["Suppliers"];
        var trademarkField = publisherType.Fields["Trademark"];
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(bookShopKey0, null, new PrefetchFieldDescriptor(suppliersField));
        prefetchManager.ExecuteTasks();

        var iPublisherType = typeof (IBookShop).GetTypeInfo();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookShopKey0, bookShopType, session,
          IsFieldKeyOrSystem);
        var publisherKeys = new[] {publisherKey0, publisherKey1, publisherKey2, publisherKey3};
        foreach (var publisherKey in publisherKeys)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(publisherKey, publisherKey.TypeInfo, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(trademarkField));

        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(bookShopKey0, suppliersField, out setState));
        Assert.IsTrue(setState.IsFullyLoaded);
        Assert.AreEqual(4, setState.TotalItemCount);
        foreach (var publisherKey in publisherKeys)
          Assert.IsTrue(setState.Contains(publisherKey));
      }
    }

    [Test]
    public void EntitySetWhichOwnerHasBeenRemovedPrefetchTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        using (var tNested = session.OpenTransaction(TransactionOpenMode.New)) {
          session.Query.Single<Order>(orderKey).Remove();
          tNested.Complete();
        }

        // Ensures cache invalidation
        var ssa = DirectStateAccessor.Get(session);
        ssa.Invalidate();

        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField));
        prefetchManager.ExecuteTasks();

        EntitySetState setState;
        Assert.IsFalse(session.Handler.TryGetEntitySetState(orderKey, DetailsField, out setState));
      }
    }

    [Test]
    public void NotificationAboutForeignKeyExtractionTest()
    {
      Key book0Key;
      Key book1Key;
      Key book2Key;
      Key book3Key;
      Key orderKey;
      Key title0Key;
      Key title1Key;
      Key title2Key;
      Key title3Key;
      Key customerKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var title0 = new Title {Text = "text0", Language = "En"};
        title0Key = title0.Key;
        book0Key = new Book {Category = "a", Title = title0}.Key;
        var title1 = new Title {Text = "text1", Language = "En"};
        title1Key = title1.Key;
        book1Key = new Book {Category = "b", Title = title1}.Key;
        var title2 = new Title {Text = "text2", Language = "En"};
        title2Key = title2.Key;
        book2Key = new Book {Category = "c", Title = title2}.Key;
        var title3 = new Title {Text = "text3", Language = "En"};
        title3Key = title3.Key;
        book3Key = new Book {Category = "e", Title = title3}.Key;
        var order = session.Query.All<Order>().OrderBy(o => o.Key).First();
        orderKey = order.Key;
        customerKey = order.Customer.Key;
        tx.Complete();
      }

      var notificationCount = 0;

      Action<Key, FieldInfo, Key, int, Key, FieldInfo, Key> notificationValidator =
        (expOwnerKey, expField, expKey, increment, ownerKey, field, key) => {
          Assert.AreEqual(expOwnerKey, ownerKey);
          Assert.AreEqual(expField, field);
          Assert.AreEqual(expKey, key);
          notificationCount += increment;
        };
      Action<Key, FieldInfo, Key> failingValidator = (ownerKey, field, key) => Assert.Fail();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var titleIdField = title0Key.TypeInfo.Fields["Id"];
        prefetchManager.InvokePrefetch(title0Key, null, new PrefetchFieldDescriptor(titleIdField));
        prefetchManager.InvokePrefetch(book2Key, null, new PrefetchFieldDescriptor(book2Key.TypeInfo.Fields["Id"]));
        prefetchManager.InvokePrefetch(title3Key, null, new PrefetchFieldDescriptor(titleIdField));
        prefetchManager.ExecuteTasks();

        prefetchManager.InvokePrefetch(book0Key, null, new PrefetchFieldDescriptor(BookTitleField, null, true, true,
          failingValidator));
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField, null, true, true,
          failingValidator));
        prefetchManager.InvokePrefetch(book1Key, null, new PrefetchFieldDescriptor(BookTitleField, null, true, true,
          notificationValidator.Bind(book1Key).Bind(BookTitleField).Bind(title1Key).Bind(1)));
        prefetchManager.InvokePrefetch(book2Key, null, new PrefetchFieldDescriptor(BookTitleField, null, true, true,
          notificationValidator.Bind(book2Key).Bind(BookTitleField).Bind(title2Key).Bind(2)));
        prefetchManager.InvokePrefetch(book3Key, null, new PrefetchFieldDescriptor(BookTitleField, null, true, true,
          failingValidator));
        prefetchManager.ExecuteTasks();

        Assert.AreEqual(3, notificationCount);
        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(title0Key, TitleType, session);
        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(customerKey, CustomerType, session);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(title1Key, TitleType, session,
          field => IsFieldKeyOrSystem(field) || ITitleType.Fields.Contains(field.Name));
      }
    }

    [Test]
    public void NotificationAboutExtractionOfEntitySetElementKeyTest()
    {
      Key publisherKey;
      Key orderKey;
      List<Key> bookShopKeys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        if (session.Query.All<Publisher>().Count() < 3) {
          Key stub;
          CreatePublishersAndBookShops(out stub, out stub, out stub, out stub, out stub, out stub,
            out stub, out stub);
        }
        var publisher = session.Query.All<Publisher>().Where(p => p.Distributors.Count > 3).First();
        publisherKey = publisher.Key;
        bookShopKeys = publisher.Distributors.Select(d => d.Key).ToList();
        var order = session.Query.All<Order>().Where(o => o.Details.Count==4).First();
        orderKey = order.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var distributorsField = typeof (Publisher).GetTypeInfo().Fields["Distributors"];
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var notificationCount = 0;
        prefetchManager.InvokePrefetch(publisherKey, null, new PrefetchFieldDescriptor(distributorsField, null,
          false, false, (ownerKey, field, key) => {
            Assert.AreEqual(publisherKey, ownerKey);
            Assert.AreEqual(distributorsField, field);
            bookShopKeys.Contains(key);
            notificationCount++;
          }));
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField, null,
          false, false, (ownerKey, field, key) => Assert.Fail()));
        prefetchManager.ExecuteTasks();
        
        Assert.AreEqual(bookShopKeys.Count, notificationCount);
        EntitySetState setState;
        session.Handler.TryGetEntitySetState(publisherKey, distributorsField, out setState);
        var bookShopType = typeof (BookShop).GetTypeInfo(Domain);
        var iBookShopType = typeof (IBookShop).GetTypeInfo(Domain);
        Assert.AreEqual(bookShopKeys.Count, setState.TotalItemCount);
        var actualCount = 0;
        foreach (var key in setState) {
          actualCount++;
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, bookShopType, session,
            field => IsFieldKeyOrSystem(field) || iBookShopType.Fields.Contains(field.Name));
        }
        Assert.AreEqual(bookShopKeys.Count, actualCount);
      }
    }

    [Test]
    public void StructurePrefetchTest()
    {
      Key contaierKey;
      Key bookShop0Key;
      Key book0Key;
      Key bookShop1Key;
      Key book1Key;
      PrefetchTestHelper.CreateOfferContainer(Domain, out contaierKey, out book0Key, out bookShop0Key,
        out book1Key, out bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        const string intermediateOfferName = "IntermediateOffer";
        var realOfferField = OfferContainerType.Fields["RealOffer"];
        var realOfferBookField = OfferContainerType.Fields["RealOffer.Book"];
        var intermediateOfferField = OfferContainerType.Fields[intermediateOfferName];
        var auxField = OfferContainerType.Fields["AuxField"];
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(realOfferField),
          new PrefetchFieldDescriptor(intermediateOfferField), new PrefetchFieldDescriptor(auxField));
        prefetchManager.ExecuteTasks();

        EntityState state;
        Assert.IsFalse(session.EntityStateCache.TryGetItem(book0Key, true, out state));
        Assert.IsFalse(session.EntityStateCache.TryGetItem(bookShop0Key, true, out state));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(contaierKey, contaierKey.TypeInfo, session,
          field => PrefetchTestHelper.IsFieldToBeLoadedByDefault(field)
            || field.Name.StartsWith(intermediateOfferName) || field.Name == "RealOffer.Lazy");

        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(realOfferBookField));
        prefetchManager.ExecuteTasks();

        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book0Key, book0Key.TypeInfo, session);
      }
    }

    [Test]
    public void ReferencesFromStructurePrefetchTest()
    {
      Key contaierKey;
      Key bookShop0Key;
      Key book0Key;
      Key bookShop1Key;
      Key book1Key;
      PrefetchTestHelper.CreateOfferContainer(Domain, out contaierKey, out book0Key, out bookShop0Key,
        out book1Key, out bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var realOfferBookField = OfferContainerType.Fields["RealOffer.Book"];
        var realOfferBookShopField = OfferContainerType.Fields["RealOffer.BookShop"];
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(realOfferBookField));
        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(realOfferBookShopField));
        prefetchManager.ExecuteTasks();

        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book0Key, book0Key.TypeInfo, session);
        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(bookShop0Key,
          typeof (IBookShop).GetTypeInfo(Domain), session);
      }
    }

    [Test]
    public void ReferencesFromStructureContainedByAnotherStructurePrefetchTest()
    {
      Key contaierKey;
      Key bookShop0Key;
      Key book0Key;
      Key bookShop1Key;
      Key book1Key;
      PrefetchTestHelper.CreateOfferContainer(Domain, out contaierKey, out book0Key, out bookShop0Key,
        out book1Key, out bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var realOfferBookField = OfferContainerType.Fields["IntermediateOffer.RealOffer.Book"];
        var realOfferBookShopField = OfferContainerType.Fields["IntermediateOffer.RealOffer.BookShop"];
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(realOfferBookField));
        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(realOfferBookShopField));
        prefetchManager.ExecuteTasks();

        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book1Key, book1Key.TypeInfo, session);
        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(bookShop1Key,
          typeof (IBookShop).GetTypeInfo(Domain), session);
      }
    }

    [Test]
    public void LazyLoadFieldPrefetchTest()
    {
      Key contaierKey;
      Key bookShop0Key;
      Key book0Key;
      Key bookShop1Key;
      Key book1Key;
      PrefetchTestHelper.CreateOfferContainer(Domain, out contaierKey, out book0Key, out bookShop0Key,
        out book1Key, out bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var idField = contaierKey.TypeInfo.Fields["Id"];
        var lazyField = contaierKey.TypeInfo.Fields["Lazy"];
        var intermediateOfferField = contaierKey.TypeInfo.Fields["IntermediateOffer"];
        var intermediateOfferRealOfferLazyField = contaierKey.TypeInfo.Fields["IntermediateOffer.RealOffer.Lazy"];
        var realOfferField = contaierKey.TypeInfo.Fields["RealOffer"];
        var realOfferLazyField = contaierKey.TypeInfo.Fields["RealOffer.Lazy"];
        var container = session.Query.Single<OfferContainer>(contaierKey);

        var conatinerPrimitiveFields = contaierKey.TypeInfo.Fields.Where(field => field.IsPrimitive);
        foreach (var fieldInfo in conatinerPrimitiveFields)
          Assert.AreEqual(!fieldInfo.Equals(lazyField) && !fieldInfo.Equals(realOfferLazyField)
            && !fieldInfo.Equals(intermediateOfferRealOfferLazyField),
            container.State.Tuple.GetFieldState(fieldInfo.MappingInfo.Offset).IsAvailable());
        PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(contaierKey, contaierKey.TypeInfo, session);

        prefetchManager.InvokePrefetch(contaierKey, null, new PrefetchFieldDescriptor(lazyField),
          new PrefetchFieldDescriptor(intermediateOfferField), new PrefetchFieldDescriptor(realOfferField));
        prefetchManager.ExecuteTasks();
        foreach (var fieldInfo in conatinerPrimitiveFields)
          Assert.IsTrue(container.State.Tuple.GetFieldState(fieldInfo.MappingInfo.Offset).IsAvailable());
      }
    }

    [Test]
    public void RemoveTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var order = session.Query.All<Order>().Prefetch(session, o => o.Details).First();
        var detail = order.Details.First();
        order.Details.Remove(detail);
        detail.Remove();
        //session.Query.All<Order>().Prefetch(o => o.Details).First();
        t.Complete();
      }
    }

    [Test]
    public void ReferenceToSessionIsNotPreservedInCacheTest()
    {
      instanceCount = 10;
      for (int i = 0; i < instanceCount; i++) {
        using (var session = Session.Open(Domain))
        using (var t = Transaction.Open()) {
          session.Extensions.Set(new MemoryLeakTester());
          var newOrder = new Order();
          var orderDetail = new OrderDetail {Product = new Product()};
          session.Persist();
          var order = EnumerableUtils.One(newOrder).Prefetch(o => o.Details).First();
          Assert.That(order, Is.Not.Null);
          var product = EnumerableUtils.One(orderDetail).Prefetch(d => d.Product).First();
          Assert.That(product, Is.Not.Null);
          //Query.All<Order>().Prefetch(o => o.Details).First();
          t.Complete();
        }
      }
      GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
      GC.WaitForPendingFinalizers();
      Assert.That(instanceCount, Is.EqualTo(0));
    }

    private void PrefetchIntrinsicFields(PrefetchManager prefetchManager, Key key, Type type)
    {
      var typeInfo = Domain.Model.Types[type];
      prefetchManager.InvokePrefetch(key, typeInfo,
        typeInfo.Fields.Where(PrefetchHelper.IsFieldToBeLoadedByDefault)
          .Select(field => new PrefetchFieldDescriptor(field)).ToArray());
    }

    private static void AssertEntityStateIsNotLoaded(Key key, Session session)
    {
      var state = session.EntityStateCache[key, true];
      Assert.IsNull(state);
    }

    private Key GetFirstKeyInCurrentSession<T>()
      where T : Entity
    {
      return Session.Demand().Query.All<T>().OrderBy(o => o.Key).First().Key;
    }

    private static void CreatePublishersAndBookShops(out Key publisherKey0, out Key bookShopKey0,
      out Key bookShopKey1, out Key bookShopKey2, out Key bookShopKey3, out Key publisherKey1,
      out Key publisherKey2, out Key publisherKey3)
    {
      var publisher0 = new Publisher {Country = "A"};
      publisherKey0 = publisher0.Key;
      var publisher1 = new Publisher {Country = "A"};
      publisherKey1 = publisher1.Key;
      var publisher2 = new Publisher {Country = "B"};
      publisherKey2 = publisher2.Key;
      var publisher3 = new Publisher {Country = "B"};
      publisherKey3 = publisher3.Key;
      var publisher4 = new Publisher {Country = "C"};

      var bookShop0 = new BookShop {Url = "0"};
      bookShopKey0 = bookShop0.Key;
      var bookShop1 = new BookShop {Url = "0"};
      bookShopKey1 = bookShop1.Key;
      var bookShop2 = new BookShop {Url = "1"};
      bookShopKey2 = bookShop2.Key;
      var bookShop3 = new BookShop {Url = "1"};
      bookShopKey3 = bookShop3.Key;
      var bookShop4 = new BookShop {Url = "2"};

      publisher0.Distributors.Add(bookShop0);
      publisher0.Distributors.Add(bookShop1);
      publisher0.Distributors.Add(bookShop2);
      publisher0.Distributors.Add(bookShop3);

      publisher1.Distributors.Add(bookShop4);
      publisher1.Distributors.Add(bookShop0);
      publisher1.Distributors.Add(bookShop1);
      publisher1.Distributors.Add(bookShop2);

      publisher2.Distributors.Add(bookShop3);
      publisher2.Distributors.Add(bookShop4);
      publisher2.Distributors.Add(bookShop0);
      publisher2.Distributors.Add(bookShop1);

      publisher3.Distributors.Add(bookShop2);
      publisher3.Distributors.Add(bookShop3);
      publisher3.Distributors.Add(bookShop4);
      publisher3.Distributors.Add(bookShop0);

      publisher4.Distributors.Add(bookShop1);
      publisher4.Distributors.Add(bookShop2);
      publisher4.Distributors.Add(bookShop3);
      publisher4.Distributors.Add(bookShop4);
    }
  }
}