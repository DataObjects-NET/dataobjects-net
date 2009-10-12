// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Testing;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Prefetch.Model;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchProcessorTest : AutoBuildTest
  {
    private TypeInfo customerType;
    private TypeInfo orderType;
    private TypeInfo productType;
    private FieldInfo personIdField;
    private FieldInfo ageField;
    private FieldInfo cityField;
    private FieldInfo customerField;
    private FieldInfo employeeField;
    private FieldInfo orderIdField;
    private FieldInfo detailsField;
    private FieldInfo booksField;
    private System.Reflection.FieldInfo taskContainersField;
    private System.Reflection.FieldInfo prefetchProcessorField;

    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
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
      customerType = Domain.Model.Types[typeof (Customer)];
      orderType = Domain.Model.Types[typeof (Order)];
      productType = Domain.Model.Types[typeof (Product)];
      personIdField = Domain.Model.Types[typeof (Person)].Fields["Id"];
      orderIdField = Domain.Model.Types[typeof (Order)].Fields["Id"];
      cityField = customerType.Fields["City"];
      ageField = Domain.Model.Types[typeof (AdvancedPerson)].Fields["Age"];
      customerField = orderType.Fields["Customer"];
      employeeField = orderType.Fields["Employee"];
      detailsField = orderType.Fields["Details"];
      booksField = Domain.Model.Types[typeof (Author)].Fields["Books"];
      taskContainersField = typeof (PrefetchProcessor).GetField("taskContainers",
        BindingFlags.NonPublic | BindingFlags.Instance);
      prefetchProcessorField = typeof (SessionHandler).GetField("prefetchProcessor",
        BindingFlags.NonPublic | BindingFlags.Instance);
      FillDataBase(Domain);
    }

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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        AssertEntityStateIsNotLoaded(customerKey, session);
        AssertEntityStateIsNotLoaded(orderKey, session);
        AssertEntityStateIsNotLoaded(productKey, session);
        PrefetchIntrinsicFields(prefetchProcessor, customerKey, typeof(Customer));
        PrefetchIntrinsicFields(prefetchProcessor, orderKey, typeof(Order));
        PrefetchIntrinsicFields(prefetchProcessor, productKey, typeof(Product));
        prefetchProcessor.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, customerType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(productKey, productType, session,
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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);

        prefetchProcessor.Prefetch(orderKey0, orderType, new PrefetchFieldDescriptor(customerField, true));
        prefetchProcessor.Prefetch(orderKey0, orderType, new PrefetchFieldDescriptor(employeeField, true));

        prefetchProcessor.Prefetch(orderKey1, orderType, new PrefetchFieldDescriptor(customerField, true));

        prefetchProcessor.Prefetch(orderKey2, orderType, new PrefetchFieldDescriptor(employeeField, true));
        prefetchProcessor.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey0, orderType, session, field =>
          field==customerField || field==employeeField || IsFieldKeyOrSystem(field)
            || (field.Parent != null && (field.Parent==customerField || field.Parent==employeeField)));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey1, orderType, session, field =>
          field==customerField || IsFieldKeyOrSystem(field)
            || (field.Parent != null && field.Parent==customerField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey2, orderType, session, field =>
          field==employeeField || IsFieldKeyOrSystem(field)
            || (field.Parent != null && field.Parent==employeeField));

        AssertReferencedEntityIsLoaded(orderKey0, session, customerField);
        AssertReferencedEntityIsLoaded(orderKey0, session, employeeField);

        AssertReferencedEntityIsLoaded(orderKey1, session, customerField);

        AssertReferencedEntityIsLoaded(orderKey2, session, employeeField);
      }
    }

    [Test]
    public void OwnerOfReferencedEntitiesIsNotFoundTest()
    {
      Key orderKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var order = Query<Order>.All.OrderBy(c => c.Id).First();
        orderKey = order.Key;
        order.Remove();
        tx.Complete();
      }
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(orderKey, orderType, new PrefetchFieldDescriptor(customerField, true));
        var taskContainer = GetSingleTaskContainer(prefetchProcessor);
        prefetchProcessor.ExecuteTasks();
        var referncedEntityTask = taskContainer.ReferencedEntityPrefetchTasks.Single();
        Assert.IsTrue(taskContainer.EntityPrefetchTask.IsActive);
        Assert.IsFalse(referncedEntityTask.IsActive);
        var state = session.EntityStateCache[orderKey, true];
        Assert.IsNotNull(state);
        Assert.AreEqual(PersistenceState.Synchronized, state.PersistenceState);
        Assert.IsNull(state.Tuple);
      }
    }

    [Test]
    public void PrefetchEntityByKeyWithUnknownTypeTest()
    {
      var customerKey = GetFirstKey<Customer>();

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var supplierType = Domain.Model.Types[typeof (Supplier)];
        var keyWithoutType = Key.Create(customerType.Hierarchy.Root, customerKey.Value, TypeReferenceAccuracy.Hierarchy);
        AssertEx.Throws<ArgumentNullException>(() =>
          prefetchProcessor.Prefetch(keyWithoutType, null, new PrefetchFieldDescriptor(ageField)));
        prefetchProcessor.Prefetch(keyWithoutType, customerType, new PrefetchFieldDescriptor(cityField));
        AssertEx.Throws<InvalidOperationException>(() =>
          prefetchProcessor.Prefetch(keyWithoutType, supplierType, new PrefetchFieldDescriptor(cityField)));
        prefetchProcessor.Prefetch(keyWithoutType, supplierType, new PrefetchFieldDescriptor(ageField));
        prefetchProcessor.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, customerType, session,
          field => IsFieldKeyOrSystem(field)
            || field == cityField || field.DeclaringType == customerType.Hierarchy.Root);
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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchProcessor.Prefetch(orderKey, orderType,
          new PrefetchFieldDescriptor(orderType.Fields["Details"]));
        prefetchProcessor.ExecuteTasks();

        var orderDetailsType = typeof (OrderDetail).GetTypeInfo();
        Assert.AreEqual(prevEntityStateCount + orderDetailKeys.Length + 1, session.EntityStateCache.Count);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType, session, IsFieldKeyOrSystem);
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
      TypeInfo bookType;
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
        bookType = typeof (Book).GetTypeInfo();
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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchProcessor.Prefetch(bookKey, bookKey.Type,
          new PrefetchFieldDescriptor(bookKey.Type.Fields["Authors"]));
        prefetchProcessor.ExecuteTasks();

        Assert.AreEqual(prevEntityStateCount + 4, session.EntityStateCache.Count);
        Assert.IsTrue(session.EntityStateCache.ContainsKey(bookKey));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(author0Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(author2Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(author4Key));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey, bookType, session, IsFieldKeyOrSystem);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(author0Key, authorType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(author2Key, authorType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(author4Key, authorType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var prevEntityStateCount = session.EntityStateCache.Count;
        prefetchProcessor.Prefetch(authorKey, authorKey.Type, new PrefetchFieldDescriptor(booksField));
        prefetchProcessor.ExecuteTasks();

        Assert.AreEqual(prevEntityStateCount + 4, session.EntityStateCache.Count);
        Assert.IsTrue(session.EntityStateCache.ContainsKey(authorKey));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(book3Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(book4Key));
        Assert.IsTrue(session.EntityStateCache.ContainsKey(book5Key));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(authorKey, authorType, session, IsFieldKeyOrSystem);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(book3Key, bookType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(book4Key, bookType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(book5Key, bookType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void EntitySetLimitedPrefetchingTest()
    {
      Key orderKey;
      Key authorKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var order = new Order {Number = 1, Customer = null, Employee = null};
        var orderDetail1 = new OrderDetail {Order = order, Product = null, Count = 100};
        var orderDetail2 = new OrderDetail {Order = order, Product = null, Count = 200};
        var orderDetail3 = new OrderDetail {Order = order, Product = null, Count = 300};
        var orderDetail4 = new OrderDetail {Order = order, Product = null, Count = 400};
        orderKey = order.Key;

        var book0 = new Book();
        var book1 = new Book();
        var book2 = new Book();
        var book3 = new Book();
        var book4 = new Book();
        var book5 = new Book();

        var author0 = new Author();
        authorKey = author0.Key;

        author0.Books.Add(book0);
        author0.Books.Add(book1);
        author0.Books.Add(book2);
        author0.Books.Add(book3);
        author0.Books.Add(book4);
        author0.Books.Add(book5);

        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(authorKey, authorKey.Type, new PrefetchFieldDescriptor(booksField));
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField));
        prefetchProcessor.ExecuteTasks();

        EntitySetState state;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(authorKey, booksField, out state));
        Assert.AreEqual(6, state.Count());
        Assert.IsTrue(state.IsFullyLoaded);
        Assert.IsTrue(session.Handler.TryGetEntitySetState(orderKey, detailsField, out state));
        Assert.AreEqual(4, state.Count());
        Assert.IsTrue(state.IsFullyLoaded);
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        const int itemCount = 3;
        prefetchProcessor.Prefetch(authorKey, authorKey.Type, new PrefetchFieldDescriptor(booksField, itemCount));
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField, itemCount));
        prefetchProcessor.ExecuteTasks();

        EntitySetState state;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(authorKey, booksField, out state));
        Assert.AreEqual(itemCount, state.Count());
        Assert.IsFalse(state.IsFullyLoaded);
        Assert.IsTrue(session.Handler.TryGetEntitySetState(orderKey, detailsField, out state));
        Assert.AreEqual(itemCount, state.Count());
        Assert.IsFalse(state.IsFullyLoaded);
      }
    }

    [Test]
    public void QueryPlanReusingTest()
    {
      Key customer0Key;
      Key customer1Key;
      Key order0Key;
      Key order1Key;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        customer0Key = Query<Customer>.All.OrderBy(c => c.Id).First().Key;
        customer1Key = Query<Customer>.All.OrderBy(c => c.Id).Skip(1).First().Key;
        order0Key = Query<Order>.All.OrderBy(o => o.Id).First().Key;
        order1Key = Query<Order>.All.OrderBy(o => o.Id).Skip(1).First().Key;
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(customer0Key, customer0Key.Type, new PrefetchFieldDescriptor(cityField));
        var taskContainer = GetSingleTaskContainer(prefetchProcessor);
        taskContainer.EntityPrefetchTask.RegisterQueryTask();
        var originalRecordSet = taskContainer.EntityPrefetchTask.RecordSet;
        prefetchProcessor.ExecuteTasks();
        
        prefetchProcessor.Prefetch(customer1Key, customer1Key.Type, new PrefetchFieldDescriptor(cityField));
        prefetchProcessor.Prefetch(order0Key, order0Key.Type, new PrefetchFieldDescriptor(customerField));
        taskContainer = ((IEnumerable<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor))
          .Where(container => container.Key == customer1Key).Single();
        taskContainer.EntityPrefetchTask.RegisterQueryTask();
        Assert.AreSame(originalRecordSet, taskContainer.EntityPrefetchTask.RecordSet);
        prefetchProcessor.ExecuteTasks();
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(order0Key, order0Key.Type, new PrefetchFieldDescriptor(detailsField));
        var taskContainer = GetSingleTaskContainer(prefetchProcessor);
        var task = taskContainer.EntitySetPrefetchTasks.Single();
        task.RegisterQueryTask();
        var originalRecordSet = task.RecordSet;
        prefetchProcessor.ExecuteTasks();
        
        prefetchProcessor.Prefetch(order1Key, order1Key.Type, new PrefetchFieldDescriptor(detailsField));
        var bookKey = Key.Create<Book>(TypeReferenceAccuracy.ExactType, 1);
        prefetchProcessor.Prefetch(bookKey, bookKey.Type,
          new PrefetchFieldDescriptor(bookKey.Type.Fields["Authors"]));
        taskContainer = ((IEnumerable<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor))
          .Where(container => container.Key == order1Key).Single();
        task = taskContainer.EntitySetPrefetchTasks.Single();
        task.RegisterQueryTask();
        Assert.AreSame(originalRecordSet, task.RecordSet);
        prefetchProcessor.ExecuteTasks();
      }
    }

    [Test]
    public void EntityHaveBeenLoadedBeforeTaskActivationTest()
    {
      var customerKey = GetFirstKey<Customer>();

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(customerKey, customerKey.Type, new PrefetchFieldDescriptor(cityField));
        prefetchProcessor.ExecuteTasks();

        prefetchProcessor.Prefetch(customerKey, customerKey.Type, new PrefetchFieldDescriptor(personIdField),
          new PrefetchFieldDescriptor(cityField));
        var taskContainer = GetSingleTaskContainer(prefetchProcessor);
        taskContainer.EntityPrefetchTask.RegisterQueryTask();
        Assert.IsFalse(taskContainer.EntityPrefetchTask.IsActive);
      }
    }

    [Test]
    public void ReferencedEntityHaveBeenLoadedBeforeTaskActivationTest()
    {
      Key order0Key;
      Key employee0Key;
      Key order1Key;
      Key employee1Key;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var order0 = Query<Order>.All.OrderBy(o => o.Id).First();
        var order1 = Query<Order>.All.OrderBy(o => o.Id).Skip(1).First();
        order0Key = order0.Key;
        employee0Key = order0.Employee.Key;
        order1Key = order1.Key;
        employee1Key = order1.Employee.Key;
      }

      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var employeeNameField = Domain.Model.Types[typeof (Person)].Fields["Name"];
        var employeeAgeField = employee1Key.Type.Fields["Age"];
        prefetchProcessor.Prefetch(employee0Key, employee0Key.Type,
          new PrefetchFieldDescriptor(employeeNameField), new PrefetchFieldDescriptor(ageField));
        prefetchProcessor.Prefetch(employee1Key, employee1Key.Type, new PrefetchFieldDescriptor(ageField));
        prefetchProcessor.Prefetch(order0Key, order0Key.Type, new PrefetchFieldDescriptor(orderIdField));
        prefetchProcessor.Prefetch(order1Key, order1Key.Type, new PrefetchFieldDescriptor(orderIdField));
        prefetchProcessor.ExecuteTasks();

        prefetchProcessor.Prefetch(order0Key, order0Key.Type, new PrefetchFieldDescriptor(employeeField, true));
        prefetchProcessor.Prefetch(order1Key, order1Key.Type, new PrefetchFieldDescriptor(employeeField, true));
        var taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
        Assert.AreEqual(2, taskContainers.Count);
        Func<Key, ReferencedEntityPrefetchTask> taskSelector = containerKey => taskContainers
          .Where(container => container.Key==containerKey)
          .SelectMany(container => container.ReferencedEntityPrefetchTasks).Single();
        var task0 = taskSelector.Invoke(order0Key);
        var task1 = taskSelector.Invoke(order1Key);
        prefetchProcessor.ExecuteTasks();
        Assert.IsFalse(task0.IsActive);
        Assert.IsTrue(task1.IsActive);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employee0Key, employee0Key.Type, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employee1Key, employee1Key.Type, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void PrefetchReferencedEntityByKnownForeignKeyTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var orderPrimaryIndex = orderType.Indexes.PrimaryIndex;
        var selectedColumns = orderPrimaryIndex.ColumnIndexMap.System
          .Concat(employeeField.Columns.Select(column => orderPrimaryIndex.Columns.IndexOf(column))).ToArray();
        var orderQuery = orderType.Indexes.PrimaryIndex.ToRecordSet()
          .Filter(t => t.GetValue<int>(0)==orderKey.Value.GetValue<int>(0)).Select(selectedColumns);
        orderQuery.ToEntities(0).Single();
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);

        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(employeeField, true));
        var taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
        Assert.AreEqual(2, taskContainers.Count);
        foreach (var container in taskContainers)
          Assert.IsNull(container.ReferencedEntityPrefetchTasks);
        var orderTask = taskContainers.Where(container => container.Key==orderKey).SingleOrDefault();
        var employeeTask = taskContainers.Where(container => container.Key!=orderKey).SingleOrDefault();
        Assert.IsNotNull(orderTask);
        Assert.IsNotNull(employeeTask);
        prefetchProcessor.ExecuteTasks();
        Assert.IsNull(orderTask.EntityPrefetchTask);
        Assert.IsTrue(employeeTask.EntityPrefetchTask.IsActive);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType,
          session, field => IsFieldKeyOrSystem(field) || field == employeeField
            || field.Parent == employeeField);
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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(employeeField, true));
        var taskContainer = GetSingleTaskContainer(prefetchProcessor);
        Assert.AreEqual(1, taskContainer.ReferencedEntityPrefetchTasks.Count());
        var task = taskContainer.ReferencedEntityPrefetchTasks.Single();
        prefetchProcessor.ExecuteTasks();
        Assert.IsFalse(task.IsActive);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field == employeeField
            || field.Parent == employeeField);
        Assert.IsNull(Query<Order>.Single(orderKey).Employee);
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = (PrefetchProcessor) prefetchProcessorField.GetValue(session.Handler);
        session.Handler.FetchInstance(orderKey);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(employeeField, true));
        var taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
        Assert.AreEqual(1, taskContainers.Count);
        Assert.AreEqual(orderKey, taskContainers.Single().Key);
      }
    }

    [Test]
    public void PrefetchEmptyEntitySetTest()
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
        var prefetchProcessor = (PrefetchProcessor) prefetchProcessorField.GetValue(session.Handler);
        session.Handler.FetchInstance(orderKey);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField, null));
        var taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
        Assert.AreEqual(1, taskContainers.Count);
        prefetchProcessor.ExecuteTasks();
        EntitySetState actualState;
        session.Handler.TryGetEntitySetState(orderKey, detailsField, out actualState);
        Assert.AreEqual(0, actualState.Count);
        Assert.IsTrue(actualState.IsFullyLoaded);
      }
    }

    [Test]
    public void PrefetchReferencedEntityWhenTypeSpecifiedForOwnerIsInvalidTest()
    {
      var productKey = GetFirstKey<Product>();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var keyWithoutType = Key.Create(productKey.TypeRef.Type, productKey.Value, TypeReferenceAccuracy.Hierarchy);
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(keyWithoutType, Domain.Model.Types[typeof (PersonalProduct)],
          new PrefetchFieldDescriptor(Domain.Model.Types[typeof (PersonalProduct)].Fields["Employee"], true));
        var taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
        var task = taskContainers.Where(container => container.ReferencedEntityPrefetchTasks!=null)
          .Single().ReferencedEntityPrefetchTasks.Single();
        prefetchProcessor.ExecuteTasks();
        Assert.IsFalse(task.IsActive);
      }
    }

    [Test]
    public void MergingOfRequestsInTaskContainer()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(customerField));
        var originalTaskContainer = GetSingleTaskContainer(prefetchProcessor);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(employeeField));
        Assert.AreSame(originalTaskContainer, GetSingleTaskContainer(prefetchProcessor));
        prefetchProcessor.ExecuteTasks();
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field == customerField || field == employeeField
            || (field.Parent != null && (field.Parent == customerField || field.Parent == employeeField)));
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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var iHasCategoryType = Domain.Model.Types[typeof (IHasCategory)];
        var bookType = typeof (Book).GetTypeInfo();
        var categoryField = iHasCategoryType.Fields["Category"];
        var bookCategoryField = bookType.FieldMap[categoryField];
        prefetchProcessor.Prefetch(bookKey0, iHasCategoryType,
          new PrefetchFieldDescriptor(categoryField, false));
        var interfaceKey = Key.Create(iHasCategoryType, bookKey1.Value, TypeReferenceAccuracy.BaseType);
        prefetchProcessor.Prefetch(interfaceKey, iHasCategoryType,
          new PrefetchFieldDescriptor(categoryField, false));
        prefetchProcessor.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey0, bookType, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(bookCategoryField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(interfaceKey, bookType, session,
          field => IsFieldKeyOrSystem(field) || field.Equals(bookCategoryField));

        AssertEx.Throws<InvalidOperationException>(
          () => prefetchProcessor.Prefetch(Key
            .Create(iHasCategoryType, bookKey2.Value, TypeReferenceAccuracy.BaseType),
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
        var prefetchProcessor = new PrefetchProcessor(Session.Demand().Handler);
        var titleField = bookKey.Type.Fields["Title"];
        prefetchProcessor.Prefetch(bookKey, bookKey.Type, new PrefetchFieldDescriptor(titleField, true));
        prefetchProcessor.ExecuteTasks();

        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(bookKey, bookKey.Type, session,
          field => IsFieldKeyOrSystem(field) || field == titleField
            || (field.Parent != null && field.Parent == titleField));
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(titleKey, titleKey.Type, session,
          field => true);
      }
    }

    [Test]
    public void DeletingOfTasksAtTransactionCommitOrRollbackTest()
    {
      Key orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain)) {
        var prefetchProcessor = (PrefetchProcessor) prefetchProcessorField.GetValue(session.Handler);
        SetSlim<PrefetchTaskContainer> taskContainers;
        using (var tx = Transaction.Open()) {
          prefetchProcessor.Prefetch(orderKey, orderKey.Type,
            new PrefetchFieldDescriptor(customerField));
          tx.Complete();
          taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
          Assert.AreEqual(1, taskContainers.Count);
        }
        Assert.AreEqual(0, taskContainers.Count);

        using (var tx = Transaction.Open()) {
          prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(employeeField));
          Assert.AreEqual(1, taskContainers.Count);
        }
        Assert.AreEqual(0, taskContainers.Count);
      }
    }

    [Test]
    public void TasksAreExecutedAutomaticallyWhenTheirCountIsReachedLimitTest()
    {
      const int entityCount = 101;
      var keys = new List<Key>(entityCount);
      using (Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        for (int i = 0; i < entityCount; i++)
          keys.Add(new Book().Key);
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(session.Handler);
        var taskContainers = (SetSlim<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor);
        var bookType = typeof (Book).GetTypeInfo();
        var idField = bookType.Fields["Id"];
        for (var i = 1; i < keys.Count; i++) {
          prefetchProcessor.Prefetch(keys[i - 1], bookType, new PrefetchFieldDescriptor(idField));
          Assert.AreEqual(i % entityCount, taskContainers.Count);
        }
        prefetchProcessor.Prefetch(keys[entityCount - 1], bookType, new PrefetchFieldDescriptor(idField));
        Assert.AreEqual(1, taskContainers.Count);
        for (var i = 0; i < entityCount - 1; i++)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(keys[i], bookType, session, IsFieldKeyOrSystem);
      }
    }

    [Test]
    public void RepeatedRegistrationOfReferencingFieldTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain)) {
        using (var tx = Transaction.Open()) {
          var prefetchProcessor = new PrefetchProcessor(session.Handler);
          prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(customerField));
          prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(customerField, true));
          prefetchProcessor.ExecuteTasks();
          var orderState = session.EntityStateCache[orderKey, true];
          var customerKey = Key.Create<Customer>(customerField.Association.ExtractForeignKey(orderState.Tuple),
            TypeReferenceAccuracy.ExactType);
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, customerType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test]
    public void RepeatedRegistrationOfEntitySetFieldTest()
    {
      Key orderKey;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open())
        orderKey = Query<Order>.All.AsEnumerable().Where(o => o.Details.Count > 2).First().Key;

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(session.Handler);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField, 1));
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField, null));
        prefetchProcessor.ExecuteTasks();
        EntitySetState entitySetState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(orderKey, detailsField, out entitySetState));
        Assert.IsTrue(entitySetState.IsFullyLoaded);
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var prefetchProcessor = new PrefetchProcessor(session.Handler);
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField, 1));
        prefetchProcessor.Prefetch(orderKey, orderKey.Type, new PrefetchFieldDescriptor(detailsField, 2));
        prefetchProcessor.ExecuteTasks();
        EntitySetState entitySetState;
        Assert.IsTrue(session.Handler.TryGetEntitySetState(orderKey, detailsField, out entitySetState));
        Assert.AreEqual(2, entitySetState.Count());
        Assert.IsFalse(entitySetState.IsFullyLoaded);
      }
    }

    [Test]
    public void FetchInstanceTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var fetchedEntityState = session.Handler.FetchInstance(orderKey);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, orderType, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    public static void FillDataBase(Domain domain)
    {
      using (var session = Session.Open(domain))
      using (var transactionScope = Transaction.Open()) {
        FillDataBase(session);
        transactionScope.Complete();
      }
    }

    public static void FillDataBase(Session session)
    {
      var customer1 = new Customer {Name = "Customer1", Age = 25, City = "A"};
      var customer2 = new Customer {Name = "Customer2", Age = 30, City = "B"};
      var supplier1 = new Supplier {Name = "Supplier1", Age = 27};
      var supplier2 = new Supplier {Name = "Supplier2", Age = 35};
      var employee1 = new Employee {Name = "Employee1"};
      var employee2 = new Employee {Name = "Employee2"};
      var product1 = new Product {Name = "Product1", Supplier = supplier1};
      var product2 = new Product {Name = "Product2", Supplier = supplier1};
      var product3 = new Product {Name = "Product3", Supplier = supplier2};
      var product4 = new Product {Name = "Product4", Supplier = supplier2};
      var order1 = new Order {Number = 1, Customer = customer1, Employee = employee1};
      var order1Detail1 = new OrderDetail {Order = order1, Product = product1, Count = 100};
      var order1Detail2 = new OrderDetail {Order = order1, Product = product2, Count = 200};
      var order1Detail3 = new OrderDetail {Order = order1, Product = product3, Count = 300};
      var order1Detail4 = new OrderDetail {Order = order1, Product = product4, Count = 400};
      var order2 = new Order {Number = 2, Customer = customer2, Employee = employee2};
      var order2Detail1 = new OrderDetail {Order = order2, Product = product3, Count = 300};
      var order2Detail2 = new OrderDetail {Order = order2, Product = product4, Count = 400};
      var order3 = new Order {Number = 3, Customer = customer1, Employee = employee1};
      var order3Detail1 = new OrderDetail {Order = order3, Product = product1, Count = 50};
      var order3Detail2 = new OrderDetail {Order = order3, Product = product4, Count = 200};
    }

    private PrefetchTaskContainer GetSingleTaskContainer(PrefetchProcessor prefetchProcessor)
    {
      return ((IEnumerable<PrefetchTaskContainer>) taskContainersField.GetValue(prefetchProcessor)).Single();
    }

    private static void AssertReferencedEntityIsLoaded(Key key, Session session, FieldInfo referencingField)
    {
      var tuple = session.EntityStateCache[key, true].Tuple;
      var foreignKeyValue = referencingField.Association.ExtractForeignKey(tuple);
      var foreignKey = Key.Create(referencingField.Association.TargetType.Hierarchy.Root,
        foreignKeyValue, TypeReferenceAccuracy.Hierarchy);
      PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(foreignKey,
        referencingField.Association.TargetType.Hierarchy.Root, session, PrefetchTestHelper.IsFieldToBeLoadedByDefault);
    }
      
    private void PrefetchIntrinsicFields(PrefetchProcessor prefetchProcessor, Key customerKey, Type type)
    {
      var typeInfo = Domain.Model.Types[type];
      prefetchProcessor.Prefetch(customerKey, typeInfo,
        typeInfo.Fields.Where(PrefetchTask.IsFieldToBeLoadedByDefault)
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

    private Key GetFirstKey<T>()
      where T : Entity
    {
      Key result;
      using (Session.Open(Domain))
      using (var tx = Transaction.Open())
        result = Query<T>.All.OrderBy(o => o.Key).First().Key;
      return result;
    }

    private static bool IsFieldKeyOrSystem(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem;
    }
  }
}