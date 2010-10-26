// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.10

using System;
using System.Linq;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.Storage.Prefetch.Model;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  public static class PrefetchTestHelper
  {
    public static void AssertOnlyDefaultColumnsAreLoaded(Key key, TypeInfo type, Session session)
    {
      AssertOnlySpecifiedColumnsAreLoaded(key, type, session, IsFieldToBeLoadedByDefault);
    }

    public static void AssertOnlySpecifiedColumnsAreLoaded(Key key, TypeInfo type, Session session,
      Func<FieldInfo, bool> fieldSelector)
    {
      var state = session.EntityStateCache[key, true];
      var realType = state.Key.Type;
      Assert.IsTrue(realType.Equals(type) 
        || realType.GetAncestors().Contains(type) 
        || (type.IsInterface && realType.GetInterfaces(true).Contains(type)));
      var tuple = state.Tuple;
      Assert.IsNotNull(tuple);
      foreach (var field in type.Fields) {
        var isFieldSelected = false;
        if (!field.IsStructure)
          isFieldSelected = fieldSelector.Invoke(field);
        foreach (var column in field.Columns) {
          if (field.IsStructure)
            isFieldSelected = fieldSelector.Invoke(column.Field);
          var isAvailable = tuple.GetFieldState(type.Columns.IndexOf(column)).IsAvailable();
          if (isFieldSelected)
            Assert.IsTrue(isAvailable);
          else
            Assert.IsFalse(isAvailable);
        }
      }
    }

    public static bool IsFieldToBeLoadedByDefault(FieldInfo field)
    {
      return field.IsPrimaryKey || field.IsSystem || !field.IsLazyLoad && !field.IsEntitySet;
    }

    public static void AssertReferencedEntityIsLoaded(Key key, Session session, FieldInfo referencingField)
    {
      var state = session.EntityStateCache[key, true];
      var foreignKeyValue = referencingField.Associations.Last().ExtractForeignKey(state.Type, state.Tuple);
      var foreignKey = Key.Create(session.Domain, referencingField.Associations.Last().TargetType,
        TypeReferenceAccuracy.BaseType, foreignKeyValue);
      AssertOnlySpecifiedColumnsAreLoaded(foreignKey, referencingField.Associations.Last().TargetType,
        session, IsFieldToBeLoadedByDefault);
    }

    public static void AssertEntitySetIsFullyLoaded(Key ownerKey, FieldInfo referencingField,
      int expectedCount, Session session)
    {
      EntitySetState setState;
      session.Handler.TryGetEntitySetState(ownerKey, referencingField, out setState);
      Assert.IsTrue(setState.IsFullyLoaded);
      Assert.AreEqual(expectedCount, setState.TotalItemCount);
    }

    public static void FillDataBase(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        FillDataBase(session);
        transactionScope.Complete();
      }
    }

    internal static void InvokePrefetch(this PrefetchManager prefetchManager, Key key, TypeInfo type,
      params PrefetchFieldDescriptor[] descriptors)
    {
      prefetchManager.Prefetch(key, type, new FieldDescriptorCollection(descriptors));
    }

    public static void InvokePrefetch(this SessionHandler sessionHandler, Key key, TypeInfo type,
      params PrefetchFieldDescriptor[] descriptors)
    {
      sessionHandler.Prefetch(key, type, new FieldDescriptorCollection(descriptors));
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

    public static void CreateOfferContainer(Domain domain, out Key contaierKey, out Key book0Key,
      out Key bookShop0Key, out Key book1Key, out Key bookShop1Key)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var book0 = new Book {Category = "abc", Title = new Title {Text = "title"}};
        book0Key = book0.Key;
        var bookShop0 = new BookShop {Name = "a"};
        bookShop0Key = bookShop0.Key;
        var offer0 = new Offer {Book = book0, BookShop = bookShop0, Number = 3};
        var book1 = new Book {Category = "abc", Title = new Title {Text = "title"}};
        book1Key = book1.Key;
        var bookShop1 = new BookShop {Name = "a"};
        bookShop1Key = bookShop1.Key;
        var offer1 = new Offer {Book = book1, BookShop = bookShop1, Number = 5};
        var intermediateOffer = new IntermediateOffer {RealOffer = offer1, Number = 15};
        contaierKey = new OfferContainer {
          RealOffer = offer0, IntermediateOffer = intermediateOffer, AuxField = "test"
        }.Key;
        tx.Complete();
      }
    }
  }
}