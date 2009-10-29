// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.07

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Prefetch.Model;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Tests.Storage.Prefetch
{
  [TestFixture]
  public class PrefetchProcessorBasicTest : PrefetchProcessorTestBase
  {
    [Test]
    public void PrefetchEntityByKeyWithKnownTypeTest()
    {
      Key customerKey;
      Key orderKey;
      Key productKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        customerKey = GetFirstKeyInCurrentSession<Customer>();
        orderKey = GetFirstKeyInCurrentSession<Order>();
        productKey = GetFirstKeyInCurrentSession<Product>();
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        AssertEntityStateIsNotLoaded(customerKey, session);
        AssertEntityStateIsNotLoaded(orderKey, session);
        AssertEntityStateIsNotLoaded(productKey, session);
        PrefetchIntrinsicFields(prefetchProcessor, customerKey, typeof(Customer));
        PrefetchIntrinsicFields(prefetchProcessor, orderKey, typeof(Order));
        PrefetchIntrinsicFields(prefetchProcessor, productKey, typeof(Product));
        prefetchProcessor.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, CustomerType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(productKey, ProductType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void PrefetchReferencedEntitiesByUnknownForeignKeys()
    {
      Key orderKey0;
      Key orderKey1;
      Key orderKey2;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        orderKey0 = Query<Order>.All.OrderBy(o => o.Id).First().Key;
        orderKey1 = Query<Order>.All.OrderBy(o => o.Id).Skip(1).First().Key;
        orderKey2 = Query<Order>.All.OrderBy(o => o.Id).Skip(2).First().Key;
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());

        prefetchProcessor.Prefetch(orderKey0, null, new PrefetchFieldDescriptor(CustomerField, true));
        prefetchProcessor.Prefetch(orderKey0, null, new PrefetchFieldDescriptor(EmployeeField, true));

        prefetchProcessor.Prefetch(orderKey1, null, new PrefetchFieldDescriptor(CustomerField, true));

        prefetchProcessor.Prefetch(orderKey2, null, new PrefetchFieldDescriptor(EmployeeField, true));
        prefetchProcessor.ExecuteTasks();

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
    public void PrefetchEntityByKeyWithUnknownTypeTest()
    {
      var customerKey = GetFirstKey<Customer>();

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var supplierType = Domain.Model.Types[typeof (Supplier)];
        var keyWithoutType = Key.Create(Domain, CustomerType.Hierarchy.Root,
          TypeReferenceAccuracy.BaseType, customerKey.Value);
        AssertEx.Throws<ArgumentNullException>(() =>
          prefetchProcessor.Prefetch(keyWithoutType, null, new PrefetchFieldDescriptor(AgeField)));
        prefetchProcessor.Prefetch(keyWithoutType, CustomerType, new PrefetchFieldDescriptor(CityField));
        AssertEx.Throws<InvalidOperationException>(() =>
          prefetchProcessor.Prefetch(keyWithoutType, supplierType, new PrefetchFieldDescriptor(CityField)));
        prefetchProcessor.Prefetch(keyWithoutType, supplierType, new PrefetchFieldDescriptor(AgeField));
        prefetchProcessor.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, CustomerType, session,
          field => IsFieldKeyOrSystem(field)
            || field == CityField || field.DeclaringType == CustomerType.Hierarchy.Root);
      }
    }

    [Test]
    public void EntitySetOneToManyPrefetchTest()
    {
      Key orderKey;
      Key[] orderDetailKeys;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var order = Query<Order>.All.OrderBy(c => c.Id).First();
        orderKey = order.Key;
        orderDetailKeys = order.Details.Select(detail => detail.Key).ToArray();
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchProcessor.Prefetch(orderKey, null, new PrefetchFieldDescriptor(OrderType.Fields["Details"]));
        prefetchProcessor.ExecuteTasks();

        var orderDetailsType = typeof (OrderDetail).GetTypeInfo();
        Assert.AreEqual(prevEntityStateCount + orderDetailKeys.Length + 1, session.EntityStateCache.Count);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType, session, IsFieldKeyOrSystem);
        Assert.IsTrue(session.EntityStateCache.ContainsKey(orderKey));
        foreach (var key in orderDetailKeys) {
          Assert.IsTrue(session.EntityStateCache.ContainsKey(key));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, orderDetailsType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test]
    public void EntitySetManyToManyPrefetchTest()
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
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
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

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchProcessor.Prefetch(bookKey, null, new PrefetchFieldDescriptor(bookKey.Type.Fields["Authors"]));
        prefetchProcessor.ExecuteTasks();

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

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchProcessor.Prefetch(authorKey, null, new PrefetchFieldDescriptor(BooksField));
        prefetchProcessor.ExecuteTasks();

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
    public void EntitySetLimitedPrefetchingTest()
    {
      Key order0Key;
      Key author0Key;
      Key order1Key;
      Key author1Key;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
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

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        prefetchProcessor.Prefetch(author0Key, null, new PrefetchFieldDescriptor(BooksField));
        prefetchProcessor.Prefetch(order0Key, null, new PrefetchFieldDescriptor(DetailsField));
        prefetchProcessor.Prefetch(author1Key, null, new PrefetchFieldDescriptor(BooksField, itemCount));
        prefetchProcessor.Prefetch(order1Key, null, new PrefetchFieldDescriptor(DetailsField, itemCount));
        prefetchProcessor.ExecuteTasks();

        ValidateLoadedEntitySet(author0Key, BooksField, 6, true, session);
        ValidateLoadedEntitySet(order0Key, DetailsField, 4, true, session);
        ValidateLoadedEntitySet(author1Key, BooksField, itemCount, false,  session);
        ValidateLoadedEntitySet(order1Key, DetailsField, itemCount, false,  session);
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        prefetchProcessor.Prefetch(author0Key, null, new PrefetchFieldDescriptor(BooksField, itemCount - 1));
        prefetchProcessor.Prefetch(author1Key, null, new PrefetchFieldDescriptor(BooksField, itemCount));
        prefetchProcessor.Prefetch(order0Key, null, new PrefetchFieldDescriptor(DetailsField, itemCount - 1));
        prefetchProcessor.Prefetch(order1Key, null, new PrefetchFieldDescriptor(DetailsField, itemCount));
        prefetchProcessor.ExecuteTasks();

        ValidateLoadedEntitySet(author0Key, BooksField, itemCount - 1, false,  session);
        ValidateLoadedEntitySet(author1Key, BooksField, itemCount, false,  session);
        ValidateLoadedEntitySet(order0Key, DetailsField, itemCount - 1, false,  session);
        ValidateLoadedEntitySet(order1Key, DetailsField, itemCount, false,  session);
      }
    }

    [Test]
    public void PrefetchReferencedEntityByKnownForeignKeyTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var orderPrimaryIndex = OrderType.Indexes.PrimaryIndex;
        var selectedColumns = orderPrimaryIndex.ColumnIndexMap.System
          .Concat(EmployeeField.Columns.Select(column => orderPrimaryIndex.Columns.IndexOf(column))).ToArray();
        var orderQuery = OrderType.Indexes.PrimaryIndex.ToRecordSet()
          .Filter(t => t.GetValue<int>(0)==orderKey.Value.GetValue<int>(0)).Select(selectedColumns);
        orderQuery.ToEntities(0).Single();
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());

        prefetchProcessor.Prefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField, true));
        var taskContainers = (SetSlim<GraphContainer>) GraphContainersField.GetValue(prefetchProcessor);
        Assert.AreEqual(2, taskContainers.Count);
        foreach (var container in taskContainers)
          Assert.IsNull(container.ReferencedEntityContainers);
        var orderTask = taskContainers.Where(container => container.Key==orderKey).SingleOrDefault();
        var employeeTask = taskContainers.Where(container => container.Key!=orderKey).SingleOrDefault();
        Assert.IsNotNull(orderTask);
        Assert.IsNotNull(employeeTask);
        prefetchProcessor.ExecuteTasks();
        Assert.IsNull(orderTask.RootEntityContainer);
        Assert.IsNotNull(employeeTask.RootEntityContainer.Task);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType,
          session, field => IsFieldKeyOrSystem(field) || field == EmployeeField
            || field.Parent == EmployeeField);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeTask.Key, Domain.Model.Types[typeof (Employee)],
          session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void PrefetchReferencedEntityByNullAsForeignKeyTest()
    {
      Key orderKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var order = Query<Order>.All.OrderBy(o => o.Id).First();
        var newOrder = new Order {Employee = null, Customer = order.Customer};
        orderKey = newOrder.Key;
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        prefetchProcessor.Prefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField, true));
        var taskContainer = GetSingleTaskContainer(prefetchProcessor);
        Assert.AreEqual(1, taskContainer.ReferencedEntityContainers.Count());
        var task = taskContainer.ReferencedEntityContainers.Single();
        prefetchProcessor.ExecuteTasks();
        Assert.IsNull(task.Task);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field == EmployeeField
            || field.Parent == EmployeeField);
        Assert.IsNull(Query<Order>.Single(orderKey).Employee);
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = (PrefetchProcessor) PrefetchProcessorField.GetValue(session.Handler);
        session.Handler.FetchInstance(orderKey);
        prefetchProcessor.Prefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField, true));
        var taskContainers = (SetSlim<GraphContainer>) GraphContainersField.GetValue(prefetchProcessor);
        Assert.AreEqual(1, taskContainers.Count);
        Assert.AreEqual(orderKey, taskContainers.Single().Key);
      }
    }

    [Test]
    public void MergingOfRequestsInTaskContainer()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        prefetchProcessor.Prefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField));
        var originalTaskContainer = GetSingleTaskContainer(prefetchProcessor);
        prefetchProcessor.Prefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField));
        Assert.AreSame(originalTaskContainer, GetSingleTaskContainer(prefetchProcessor));
        prefetchProcessor.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field == CustomerField || field == EmployeeField
            || (field.Parent != null && (field.Parent == CustomerField || field.Parent == EmployeeField)));
      }
    }

    [Test]
    public void PrefetchFieldDeclaredInInterfaceTest()
    {
      Key bookKey0;
      Key bookKey1;
      Key bookKey2;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        bookKey0 = new Book {Category = "0"}.Key;
        bookKey1 = new Book {Category = "1"}.Key;
        bookKey2 = new Book {Category = "2"}.Key;
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var iHasCategoryType = Domain.Model.Types[typeof (IHasCategory)];
        var categoryField = iHasCategoryType.Fields["Category"];
        var bookCategoryField = BookType.FieldMap[categoryField];
        prefetchProcessor.Prefetch(bookKey0, null, new PrefetchFieldDescriptor(categoryField, false));
        var interfaceKey = Key.Create(Domain, iHasCategoryType, TypeReferenceAccuracy.BaseType, bookKey1.Value);
        prefetchProcessor.Prefetch(interfaceKey, iHasCategoryType,
          new PrefetchFieldDescriptor(categoryField, false));
        prefetchProcessor.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey0, BookType, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(bookCategoryField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(interfaceKey, BookType, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(bookCategoryField));

        AssertEx.Throws<InvalidOperationException>(
          () => prefetchProcessor.Prefetch(Key
            .Create(Domain, iHasCategoryType, TypeReferenceAccuracy.BaseType, bookKey2.Value),
          iHasCategoryType, new PrefetchFieldDescriptor(bookCategoryField, false)));
      }
    }

    [Test]
    public void PrefetchFieldReferencingToInterfaceTest()
    {
      Key bookKey;
      Key titleKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var title = new Title {Text = "abc"};
        titleKey = title.Key;
        var book = new Book {Category = "1", Title = title};
        bookKey = book.Key;
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var titleField = bookKey.Type.Fields["Title"];
        prefetchProcessor.Prefetch(bookKey, null, new PrefetchFieldDescriptor(titleField, true));
        prefetchProcessor.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey, bookKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field == titleField
            || (field.Parent != null && field.Parent == titleField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(titleKey, titleKey.Type, session,
          field => true);
      }
    }

    [Test]
    public void PrefetchOneToManyEntitySetContainingReferencesToInterfaceTest()
    {
      const int instanceCount = 40;
      Key bookKey;
      Key titleKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
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

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        var translationTitlesField = BookType.Fields["TranslationTitles"];
        prefetchProcessor.Prefetch(bookKey, null, new PrefetchFieldDescriptor(translationTitlesField));
        prefetchProcessor.ExecuteTasks();

        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(bookKey, translationTitlesField, out setState));
        Assert.IsTrue(setState.IsFullyLoaded);
        Assert.AreEqual(instanceCount, setState.count);
        var iTitleType = typeof (ITitle).GetTypeInfo();
        foreach (var key in setState)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, iTitleType, session, field => true);
      }
    }

    [Test]
    public void PrefetchManyToManyEntitySetContainingReferencesToInterfaceTest()
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
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        publisherType = typeof (Publisher).GetTypeInfo();
        bookShopType = typeof (BookShop).GetTypeInfo();

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
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var distributorsField = publisherType.Fields["Distributors"];
        var urlField = bookShopType.Fields["Url"];
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        prefetchProcessor.Prefetch(publisherKey0, null, new PrefetchFieldDescriptor(distributorsField));
        prefetchProcessor.ExecuteTasks();

        var iBookShopType = typeof (IBookShop).GetTypeInfo();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(publisherKey0, publisherType, session,
          IsFieldKeyOrSystem);
        var bookShopKeys = new[] {bookShopKey0, bookShopKey1, bookShopKey2, bookShopKey3};
        foreach (var bookShopKey in bookShopKeys)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookShopKey, bookShopKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(urlField));

        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(publisherKey0, distributorsField, out setState));
        Assert.IsTrue(setState.IsFullyLoaded);
        Assert.AreEqual(4, setState.count);
        foreach (var bookShopKey in bookShopKeys)
          Assert.IsTrue(setState.Contains(bookShopKey));
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var suppliersField = bookShopType.Fields["Suppliers"];
        var trademarkField = publisherType.Fields["Trademark"];
        var prefetchProcessor = new PrefetchProcessor(Session.Demand());
        prefetchProcessor.Prefetch(bookShopKey0, null, new PrefetchFieldDescriptor(suppliersField));
        prefetchProcessor.ExecuteTasks();

        var iPublisherType = typeof (IBookShop).GetTypeInfo();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookShopKey0, bookShopType, session,
          IsFieldKeyOrSystem);
        var publisherKeys = new[] {publisherKey0, publisherKey1, publisherKey2, publisherKey3};
        foreach (var publisherKey in publisherKeys)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(publisherKey, publisherKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(trademarkField));

        EntitySetState setState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(bookShopKey0, suppliersField, out setState));
        Assert.IsTrue(setState.IsFullyLoaded);
        Assert.AreEqual(4, setState.count);
        foreach (var publisherKey in publisherKeys)
          Assert.IsTrue(setState.Contains(publisherKey));
      }
    }

    private void PrefetchIntrinsicFields(PrefetchProcessor prefetchProcessor, Key key, Type type)
    {
      var typeInfo = Domain.Model.Types[type];
      prefetchProcessor.Prefetch(key, typeInfo,
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
      return Query<T>.All.OrderBy(o => o.Key).First().Key;
    }
  }
}