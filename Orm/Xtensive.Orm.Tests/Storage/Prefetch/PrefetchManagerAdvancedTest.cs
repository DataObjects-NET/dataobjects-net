// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;
using GraphContainerDictionary = System.Collections.Generic.Dictionary<(Xtensive.Orm.Key key, Xtensive.Orm.Model.TypeInfo type), Xtensive.Orm.Internals.Prefetch.GraphContainer>;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchManagerAdvancedTest : PrefetchManagerTestBase
  {
    [Test]
    public void OwnerOfReferencedEntitiesIsNotFoundTest()
    {
      Key orderKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var customer = new Customer {Age = 25, City = "A", Name = "test"};
        orderKey = new Order {Number = 999, Customer = customer}.Key;
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField, true, true));
        var graphContainer = GetSingleGraphContainer(prefetchManager);
        prefetchManager.ExecuteTasks(true);
        var referencedEntityContainer = graphContainer.ReferencedEntityContainers.Single();
        Assert.That(graphContainer.RootEntityContainer.Task, Is.Not.Null);
        Assert.That(referencedEntityContainer.Task, Is.Null);
        var state = session.EntityStateCache[orderKey, true];
        Assert.That(state, Is.Not.Null);
        Assert.That(state.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        Assert.That(state.Tuple, Is.Null);
      }
    }

    [Test]
    public void EntityHaveBeenLoadedBeforeTaskActivationTest()
    {
      var customerKey = GetFirstKey<Customer>();

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(customerKey, null, new PrefetchFieldDescriptor(CityField));
        prefetchManager.ExecuteTasks(true);

        prefetchManager.InvokePrefetch(customerKey, null, new PrefetchFieldDescriptor(PersonIdField),
          new PrefetchFieldDescriptor(CityField));
        var graphContainer = GetSingleGraphContainer(prefetchManager);
        graphContainer.RootEntityContainer.GetTask();
        Assert.That(graphContainer.RootEntityContainer.Task, Is.Null);
      }
    }

    [Test]
    public void ReferencedEntityHasBeenFullyLoadedBeforeTaskActivationTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      Key order0Key;
      Key employee0Key;
      Key order1Key;
      Key employee1Key;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var order0 = session.Query.All<Order>().OrderBy(o => o.Id).First();
        var order1 = session.Query.All<Order>().OrderBy(o => o.Id).Skip(1).First();
        order0Key = order0.Key;
        employee0Key = order0.Employee.Key;
        order1Key = order1.Key;
        employee1Key = order1.Employee.Key;
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var employeeNameField = Domain.Model.Types[typeof (Person)].Fields["Name"];
        var employeeAgeField = employee1Key.TypeInfo.Fields["Age"];
        prefetchManager.InvokePrefetch(employee0Key, null,
          new PrefetchFieldDescriptor(employeeNameField), new PrefetchFieldDescriptor(AgeField));
        prefetchManager.InvokePrefetch(employee1Key, null, new PrefetchFieldDescriptor(AgeField));
        prefetchManager.InvokePrefetch(order0Key, null, new PrefetchFieldDescriptor(OrderIdField));
        prefetchManager.InvokePrefetch(order1Key, null, new PrefetchFieldDescriptor(OrderIdField));
        prefetchManager.ExecuteTasks(true);

        prefetchManager.InvokePrefetch(order0Key, null, new PrefetchFieldDescriptor(EmployeeField, true, true));
        prefetchManager.InvokePrefetch(order1Key, null, new PrefetchFieldDescriptor(EmployeeField, true, true));
        var graphContainers = (GraphContainerDictionary) GraphContainersField.GetValue(prefetchManager);
        Assert.That(graphContainers.Count, Is.EqualTo(2));
        Func<Key, ReferencedEntityContainer> taskSelector = containerKey => graphContainers.Values
          .Where(container => container.Key==containerKey)
          .SelectMany(container => container.ReferencedEntityContainers).Single();
        var entityContainer0 = taskSelector.Invoke(order0Key);
        var entityContainer1 = taskSelector.Invoke(order1Key);
        prefetchManager.ExecuteTasks(true);
        Assert.That(entityContainer0.Task, Is.Null);
        Assert.That(entityContainer1.Task, Is.Not.Null);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employee0Key, employee0Key.TypeInfo, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employee1Key, employee1Key.TypeInfo, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void BaseClassFieldsHaveBeenLoadedBeforeActivationOfReferencedEntityTaskTest()
    {
      Key titleKey;
      Key bookKey;
      CreateBookAndTitle(out titleKey, out bookKey);

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(titleKey, null, new PrefetchFieldDescriptor(TextField, false, false),
          new PrefetchFieldDescriptor(TitleBookField, false, false));
        prefetchManager.ExecuteTasks(true);

        prefetchManager.InvokePrefetch(bookKey, null, new PrefetchFieldDescriptor(BookTitleField));
        prefetchManager.ExecuteTasks(true);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(titleKey, titleKey.TypeInfo, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void BaseClassFieldsHaveBeenLoadedBeforeActivationOfReferencedEntityTaskWhenItsOwnerIsLoadedTest()
    {
      Key titleKey;
      Key bookKey;
      CreateBookAndTitle(out titleKey, out bookKey);

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(bookKey, null, new PrefetchFieldDescriptor(BookTitleField, false, false));
        prefetchManager.InvokePrefetch(titleKey, null, new PrefetchFieldDescriptor(TextField, false, false),
          new PrefetchFieldDescriptor(TitleBookField, false, false));
        prefetchManager.ExecuteTasks(true);

        prefetchManager.InvokePrefetch(bookKey, null, new PrefetchFieldDescriptor(BookTitleField));
        prefetchManager.ExecuteTasks(true);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(titleKey, titleKey.TypeInfo, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }
    
    [Test]
    public void PrefetchEmptyEntitySetTest()
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
        session.Handler.FetchEntityState(orderKey);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField, null));
        var graphContainers = (GraphContainerDictionary) GraphContainersField.GetValue(prefetchManager);
        Assert.That(graphContainers.Count, Is.EqualTo(1));
        prefetchManager.ExecuteTasks(true);
        EntitySetState actualState;
        session.Handler.LookupState(orderKey, DetailsField, out actualState);
        Assert.That(actualState.TotalItemCount, Is.EqualTo(0));
        Assert.That(actualState.IsFullyLoaded, Is.True);
      }
    }

    [Test]
    public void PrefetchReferencedEntityWhenTypeSpecifiedForOwnerIsInvalidTest()
    {
      var productKey = GetFirstKey<Product>();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var keyWithoutType = Key.Create(Domain, typeof (PersonalProduct), productKey.Value);
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(keyWithoutType, Domain.Model.Types[typeof (PersonalProduct)],
          new PrefetchFieldDescriptor(Domain.Model.Types[typeof (PersonalProduct)].Fields["Employee"],
            true, true));
        var graphContainers = (GraphContainerDictionary) GraphContainersField.GetValue(prefetchManager);
        var referencedEntityContainer = graphContainers.Values
          .Where(container => container.ReferencedEntityContainers!=null).Single()
          .ReferencedEntityContainers.Single();
        prefetchManager.ExecuteTasks(true);
        Assert.That(referencedEntityContainer.Task, Is.Null);
      }
    }

    [Test]
    public void DeletingOfTasksAtTransactionCommitOrRollbackTest()
    {
      Key orderKey = GetFirstKey<Order>();

      using (var session = Domain.OpenSession()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        GraphContainerDictionary graphContainers;
        using (var tx = session.OpenTransaction()) {
          prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField));
          graphContainers = (GraphContainerDictionary) GraphContainersField.GetValue(prefetchManager);
          Assert.That(graphContainers.Count, Is.EqualTo(1));
          tx.Complete();
        }
        Assert.That(graphContainers.Count, Is.EqualTo(0));

        using (var tx = session.OpenTransaction()) {
          prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(EmployeeField));
          Assert.That(graphContainers.Count, Is.EqualTo(1));
          // tx.Complete();
        }
        Assert.That(graphContainers.Count, Is.EqualTo(0));
      }
    }

    [Test]
    public void TasksAreExecutedAutomaticallyWhenCountLimitIsReachedTest()
    {
      const int entityCount = 120;
      var keys = new List<Key>(entityCount);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (int i = 0; i < entityCount; i++)
          keys.Add(new Book().Key);
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var graphContainers = (GraphContainerDictionary) GraphContainersField.GetValue(prefetchManager);
        var idField = BookType.Fields["Id"];
        for (var i = 1; i < keys.Count; i++) {
          prefetchManager.InvokePrefetch(keys[i - 1], null, new PrefetchFieldDescriptor(idField));
          Assert.That(graphContainers.Count, Is.EqualTo(i % entityCount));
        }
        prefetchManager.InvokePrefetch(keys[entityCount - 1], null, new PrefetchFieldDescriptor(idField));
        Assert.That(graphContainers.Count, Is.EqualTo(0));
        for (var i = 0; i < entityCount; i++)
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(keys[i], BookType, session,
            IsFieldKeyOrSystem);
      }
    }

    [Test]
    public void RepeatedRegistrationOfReferencingFieldTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
          prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField));
          prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField, true, true));
          prefetchManager.ExecuteTasks(true);
          var orderState = session.EntityStateCache[orderKey, true];
          var customerKey = Key.Create(Domain, WellKnown.DefaultNodeId, Domain.Model.Types[typeof(Customer)],
            TypeReferenceAccuracy.ExactType, CustomerField.Associations.Last()
              .ExtractForeignKey(orderState.Type, orderState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(customerKey, CustomerType, session,
            PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        }
      }
    }

    [Test]
    public void RepeatedRegistrationOfEntitySetFieldTest()
    {
      Key orderKey;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var order = new Order {Number = 1, Customer = null, Employee = null};
        orderKey = order.Key;
        new OrderDetail {Order = order, Product = null, Count = 100};
        new OrderDetail {Order = order, Product = null, Count = 200};
        new OrderDetail {Order = order, Product = null, Count = 300};
        new OrderDetail {Order = order, Product = null, Count = 400};
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField, null));
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField, 1));
        prefetchManager.ExecuteTasks(true);
        EntitySetState entitySetState;
        Assert.That(session.Handler.LookupState(orderKey, DetailsField, out entitySetState), Is.True);
        Assert.That(entitySetState.IsFullyLoaded, Is.True);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField, 2));
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField, 1));
        prefetchManager.ExecuteTasks(true);
        EntitySetState entitySetState;
        Assert.That(session.Handler.LookupState(orderKey, DetailsField, out entitySetState), Is.True);
        Assert.That(entitySetState.Count(), Is.EqualTo(2));
        Assert.That(entitySetState.IsFullyLoaded, Is.False);
      }
    }

    [Test]
    public void FetchInstanceTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var fetchedEntityState = session.Handler.FetchEntityState(orderKey);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(orderKey, OrderType, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
      }
    }

    [Test]
    public void QueryPlanReusingTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      Key customer0Key;
      Key customer1Key;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        customer0Key = session.Query.All<Customer>().OrderBy(c => c.Id).First().Key;
        customer1Key = session.Query.All<Customer>().OrderBy(c => c.Id).Skip(1).First().Key;
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(customer0Key, null, new PrefetchFieldDescriptor(CityField));
        prefetchManager.ExecuteTasks(true);
        
        prefetchManager.InvokePrefetch(customer1Key, null, new PrefetchFieldDescriptor(CityField));
        prefetchManager.ExecuteTasks(true);
      }
    }

    [Test]
    public void EntitySetQueryPlanReusingTest()
    {
      Key order0Key;
      Key order1Key;
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

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(order0Key, null, new PrefetchFieldDescriptor(DetailsField, 3));
        prefetchManager.ExecuteTasks(true);
        
        prefetchManager.InvokePrefetch(order1Key, null, new PrefetchFieldDescriptor(DetailsField, 2));
        prefetchManager.ExecuteTasks(true);
        ValidateLoadedEntitySet(order0Key, DetailsField, 3, false, session);
        ValidateLoadedEntitySet(order1Key, DetailsField, 2, false, session);
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(order0Key, null, new PrefetchFieldDescriptor(DetailsField));
        prefetchManager.ExecuteTasks(true);
        
        prefetchManager.InvokePrefetch(order1Key, null, new PrefetchFieldDescriptor(DetailsField));
        prefetchManager.ExecuteTasks(true);
        ValidateLoadedEntitySet(order0Key, DetailsField, 4, true, session);
        ValidateLoadedEntitySet(order1Key, DetailsField, 4, true, session);
      }
    }

    [Test]
    public void PutNullInCacheIfEntityIsNotFoundTest()
    {
      Key orderKey0;
      Key orderKey1;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var customer0 = new Customer {Age = 25, City = "A", Name = "test"};
        orderKey0 = new Order {Number = 999, Customer = customer0}.Key;
        var customer1 = new Customer {Age = 25, City = "B", Name = "test"};
        orderKey1 = new Order {Number = 1000, Customer = customer0}.Key;
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var numberField = Domain.Model.Types[typeof (Order)].Fields["Number"];
        prefetchManager.InvokePrefetch(orderKey0, null, new PrefetchFieldDescriptor(numberField));
        prefetchManager.InvokePrefetch(orderKey1, null, new PrefetchFieldDescriptor(numberField));
        prefetchManager.ExecuteTasks(true);

        Action<Key> validator = key => {
          var state = session.EntityStateCache[key, true];
          Assert.That(state.Tuple, Is.Null);
          Assert.That(state.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        };
        validator.Invoke(orderKey0);
        validator.Invoke(orderKey1);
      }
    }

    [Test]
    public void PrefetchViaReferenceToSelfTest()
    {
      Key key;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var referenceToSelf = new ReferenceToSelf {AuxField = 100};
        key = referenceToSelf.Key;
        referenceToSelf.Reference = referenceToSelf;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        var referenceField = Domain.Model.Types[typeof (ReferenceToSelf)].Fields["Reference"];
        prefetchManager.InvokePrefetch(key, null, new PrefetchFieldDescriptor(referenceField, true, true));
        prefetchManager.ExecuteTasks(true);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, key.TypeInfo, session,
          PrefetchTestHelper.IsFieldToBeLoadedByDefault);
        PrefetchTestHelper.AssertReferencedEntityIsLoaded(key, session, referenceField);
      }
    }

    [Test]
    public void EntitySetWhenItsOwnerHasAlreadyBeenFetchedInAnotherTransactionPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction())
          session.Query.Single<Order>(orderKey);

        using (session.OpenTransaction()) {
          session.Handler.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField));
          session.Handler.ExecutePrefetchTasks();

          PrefetchTestHelper.AssertEntitySetIsFullyLoaded(orderKey, DetailsField, 4, session);

          /*EntitySetState setState;
          session.Handler.TryGetEntitySetState(orderKey, DetailsField, out setState);
          Assert.IsTrue(setState.IsFullyLoaded);
          Assert.Less(0, setState.Count);*/
        }
      }
    }

    [Test]
    public void EntitySetInNestedSessionWhenItsOwnerHasAlreadyBeenFetchedInAnotherSessionPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var order = session.Query.Single<Order>(orderKey);
        using (var nestedSession = Domain.OpenSession()) {
          using (session.OpenTransaction()) {
            var count = 0;
            Assert.That(Session.Current, Is.SameAs(nestedSession));
            using (Session.Deactivate()) { // Prevents Session switching check error
              Assert.That(Session.Current, Is.SameAs(null));
              foreach (var orderDetail in order.Details) {
                Assert.That(session, Is.SameAs(order.Details.Session));
                Assert.That(orderDetail.Session, Is.SameAs(session));
                Assert.That(orderDetail.Order.Session, Is.SameAs(session));
                Assert.That(orderDetail.Order, Is.SameAs(order));
                count++;
              }
              Assert.That(order.Details.Session, Is.SameAs(session));
              Assert.That(order.Session, Is.SameAs(session));
              Assert.That(count, Is.EqualTo(4));
            }
          }
        }
      }
    }

    [Test]
    public void EntitySetWhenThereIsNotActiveTransactionPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction())
          session.Query.Single<Order>(orderKey);
        
        AssertEx.Throws<InvalidOperationException>(() => {
          session.Handler.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField));
          session.Handler.ExecutePrefetchTasks();
        });
      }
    }

    [Test]
    public void EntitySetWhichOwnerHasAlreadyBeenFetchedInAnotherTransactionPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction())
          session.Query.Single<Order>(orderKey);

        using (session.OpenTransaction()) {
          session.Handler.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(DetailsField));
          session.Handler.ExecutePrefetchTasks();
          PrefetchTestHelper.AssertEntitySetIsFullyLoaded(orderKey, DetailsField, 4, session);
        }
      }
    }

    [Test]
    public void ReferencedEntityWhenThereIsNotActiveTransactionPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction())
          session.Query.Single<Order>(orderKey);

        AssertEx.Throws<InvalidOperationException>(() => {
          session.Handler.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField));
          session.Handler.ExecutePrefetchTasks();
        });
      }
    }

    [Test]
    public void ReferencedEntityWhichOwnerHasAlreadyBeenFetchedInAnotherTransactionPrefetchTest()
    {
      var orderKey = GetFirstKey<Order>();
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction())
          session.Query.Single<Order>(orderKey);

        using (session.OpenTransaction()) {
          session.Handler.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField));
          session.Handler.ExecutePrefetchTasks();
          PrefetchTestHelper.AssertReferencedEntityIsLoaded(orderKey, session, CustomerField);
        }
      }
    }

    [Test]
    public void NotificationAboutUnknownForeignKeyWhenItsEntityHasBeenLoadedInAnotherTransactionTest()
    {
      Key book0Key;
      Key title0Key;
      Key book1Key;
      Key title1Key;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        CreateBookAndTitleInExistingSession(out title0Key, out book0Key);
        CreateBookAndTitleInExistingSession(out title1Key, out book1Key);
        tx.Complete();
      }

      using (var session = Domain.OpenSession()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        using (var tx = session.OpenTransaction()) {
          prefetchManager.InvokePrefetch(title0Key, null, new PrefetchFieldDescriptor(title0Key.TypeInfo.Fields["Id"]));
          prefetchManager.InvokePrefetch(book1Key, null, new PrefetchFieldDescriptor(book1Key.TypeInfo.Fields["Id"]));
          prefetchManager.InvokePrefetch(title1Key, null, new PrefetchFieldDescriptor(title1Key.TypeInfo.Fields["Id"]));
          prefetchManager.ExecuteTasks(true);
        }
        using (var tx = session.OpenTransaction()) {
          prefetchManager.InvokePrefetch(book0Key, null, new PrefetchFieldDescriptor(BookTitleField, null,
            true, true, (ownerKey, field, key) => Assert.Fail()));
          prefetchManager.InvokePrefetch(book1Key, null, new PrefetchFieldDescriptor(BookTitleField, null,
            true, true, (ownerKey, field, key) => Assert.Fail()));
          prefetchManager.ExecuteTasks(true);

          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(title0Key, ITitleType, session);
        }
      }
    }

    [Test]
    public void FetchEntityWhenItsStateHasBeenMarkedAsNotFoundInPreviousTransactionTest()
    {
      if (IncludeTypeIdModifier.IsEnabled)
        Assert.Ignore("This test is meaningless when TypeIds of entities are being included in the corresponding key values.");
      const int idValue = int.MaxValue - 1;
      var key = Key.Create(Domain, WellKnown.DefaultNodeId, typeof (Person), TypeReferenceAccuracy.BaseType, idValue);
      Domain domain1 = Domain;
      var personType = domain1.Model.Types[typeof (Person)];
      using (var session = Domain.OpenSession()) {
        EntityState previousState;
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        using (var tx = session.OpenTransaction()) {
          prefetchManager.InvokePrefetch(key, personType, new PrefetchFieldDescriptor(PersonIdField));
          prefetchManager.ExecuteTasks(true);
          previousState = session.EntityStateCache[key, true];
          Assert.That(previousState.Key, Is.SameAs(key));
          Assert.That(previousState.Key.HasExactType, Is.False);
          Assert.That(previousState.IsTupleLoaded, Is.True);
          Assert.That(previousState.Tuple, Is.Null);
        }
        using (var nestedSession = Domain.OpenSession())
        using (var tx = nestedSession.OpenTransaction()) {
          new Customer(idValue) {Age = 25, City = "A"};
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          prefetchManager.InvokePrefetch(key, personType, new PrefetchFieldDescriptor(PersonIdField));
          prefetchManager.ExecuteTasks(true);
          var state = session.EntityStateCache[key, true];
          Assert.That(previousState, Is.Not.SameAs(state));
          Assert.That(key, Is.Not.SameAs(state.Key));
          Assert.That(state.Key.HasExactType, Is.True);
          Assert.That(state.IsTupleLoaded, Is.True);
          Assert.That(state.Tuple, Is.Not.Null);
        }
      }
    }

    [Test]
    public void LazyLoadFlagShouldBeIgnoredForFieldReferencingToEntityTest()
    {
      var orderKey = GetFirstKey<Order>();

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var prefetchManager = (PrefetchManager) PrefetchProcessorField.GetValue(session.Handler);
        prefetchManager.InvokePrefetch(orderKey, null, new PrefetchFieldDescriptor(CustomerField, false, false));
        prefetchManager.ExecuteTasks(true);

        var tuple = session.EntityStateCache[orderKey, true].Tuple;
        foreach (var field in CustomerField.Fields)
          Assert.That(tuple.GetFieldState(field.MappingInfo.Offset).IsAvailable(), Is.True);
      }
    }

    [Test]
    public void EntityContainingOnlyLazyFieldsPrefetchTestTest()
    {
      Key key;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        key = new LazyClass {LazyInt = 3, LazyString = "a"}.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        Assert.That(session.Query.Single<LazyClass>(key), Is.Not.Null);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, key.TypeInfo, session, IsFieldKeyOrSystem);
      }
    }

    [Test]
    public void EntityContainingOnlyIdFieldPrefetchTest()
    {
      Key key;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        key = new IdOnly().Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        Assert.That(session.Query.Single<IdOnly>(key), Is.Not.Null);
        PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(key, key.TypeInfo, session, IsFieldKeyOrSystem);
      }
    }

    private void CreateBookAndTitle(out Key titleKey, out Key bookKey)
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        CreateBookAndTitleInExistingSession(out titleKey, out bookKey);
        tx.Complete();
      }
    }

    private static void CreateBookAndTitleInExistingSession(out Key titleKey, out Key bookKey)
    {
      var title = new Title {Text = "abc", Language = "En"};
      titleKey = title.Key;
      var book = new Book {Category = "1", Title = title};
      bookKey = book.Key;
    }
  }
}