// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchTest : NorthwindDOModelTest
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
    public void PrefetchGraphTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var orders = session.Query.All<Order>()
          .Prefetch(o => o.Customer.CompanyName)
          .Prefetch(o => o.Customer.Address)
          .Prefetch(o => o.OrderDetails
            .Prefetch(od => od.Product)
            .Prefetch(od => od.Product.Category)
            .Prefetch(od => od.Product.Category.Picture));
        var result = orders.ToList();
        foreach (var order in result) {
          Console.Out.WriteLine(string.Format("Order: {0}, Customer: {1}, City {2}, Items count: {3}", order.Id, order.Customer.CompanyName, order.Customer.Address.City, order.OrderDetails.Count));
          foreach (var orderDetail in order.OrderDetails)
            Console.Out.WriteLine(string.Format("\tProduct: {0}, Category: {1}, Pucture Length: {2}", 
              orderDetail.Product.ProductName,
              orderDetail.Product.Category.CategoryName,
              orderDetail.Product.Category.Picture.Length));
        }
        t.Complete();
      }
    }

    [Test]
    public void PrefetchGraphPerformanceTest()
    {
      using (var mx = new Measurement("Query graph of orders without prefetch."))
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var orders = session.Query.All<Order>();
        foreach (var order in orders) {
          var id = order.Id;
          var name = order.Customer.CompanyName;
          var city = order.Customer.Address.City;
          var count = order.OrderDetails.Count;
          foreach (var orderDetail in order.OrderDetails) {
            var productName = orderDetail.Product.ProductName;
            var category = orderDetail.Product.Category.CategoryName;
            var picture = orderDetail.Product.Category.Picture;
          }
        }
        mx.Complete();
        t.Complete();
        Console.Out.WriteLine(mx.ToString());
      }

      using (var mx = new Measurement("Query graph of orders with prefetch."))
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var orders = session.Query.All<Order>()
          .Prefetch(o => o.Customer.CompanyName)
          .Prefetch(o => o.Customer.Address)
          .Prefetch(o => o.OrderDetails
            .Prefetch(od => od.Product)
            .Prefetch(od => od.Product.Category)
            .Prefetch(od => od.Product.Category.Picture));
        foreach (var order in orders) {
          var id = order.Id;
          var name = order.Customer.CompanyName;
          var city = order.Customer.Address.City;
          var count = order.OrderDetails.Count;
          foreach (var orderDetail in order.OrderDetails) {
            var productName = orderDetail.Product.ProductName;
            var category = orderDetail.Product.Category.CategoryName;
            var picture = orderDetail.Product.Category.Picture;
          }
        }
        mx.Complete();
        t.Complete();
        Console.Out.WriteLine(mx.ToString());
      }
    }

    [Test]
    public void EnumerableOfNonEntityTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Order>().Select(o => o.Key).ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var orders = session.Query.Many<Order>(keys)
          .Prefetch(o => o.Employee);
        var orderType = Domain.Model.Types[typeof (Order)];
        var employeeField = orderType.Fields["Employee"];
        var employeeType = Domain.Model.Types[typeof (Employee)];
        Func<FieldInfo, bool> fieldSelector = field => field.IsPrimaryKey || field.IsSystem
          || !field.IsLazyLoad && !field.IsEntitySet;
        foreach (var order in orders) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(order.Key, order.Key.TypeInfo, session, fieldSelector);
          var orderState = session.EntityStateCache[order.Key, true];
          var employeeKey = Key.Create(Domain, Domain.Model.Types[typeof(Employee)],
            TypeReferenceAccuracy.ExactType, employeeField.Associations.Last()
              .ExtractForeignKey(orderState.Type, orderState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session, fieldSelector);
        }
      }
    }

    [Test]
    public void EnumerableOfEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetcher = session.Query.All<Order>()
          .Prefetch(o => o.ProcessingTime)
          .Prefetch(o => o.OrderDetails);
        var orderDetailsField = Domain.Model.Types[typeof (Order)].Fields["OrderDetails"];
        foreach (var order in prefetcher) {
          EntitySetState entitySetState;
          Assert.IsTrue(session.Handler.LookupState(order.Key, orderDetailsField, out entitySetState));
          Assert.IsTrue(entitySetState.IsFullyLoaded);
        }
      }
    }

    [Test]
    public void PreservingOriginalOrderOfElementsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Order>().ToList();
        var actual = expected.Prefetch(o => o.ProcessingTime).Prefetch(o => o.OrderDetails).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PrefetchManyNotFullBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var employeesField = Domain.Model.Types[typeof (Territory)].Fields["Employees"];
        var ordersField = Domain.Model.Types[typeof (Employee)].Fields["Orders"];
        var territories = session.Query.All<Territory>()
          .Prefetch(t => t.Employees.Prefetch(e => e.Orders));
        foreach (var territory in territories) {
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
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var detailsField = Domain.Model.Types[typeof (Order)].Fields["OrderDetails"];
        var productField = Domain.Model.Types[typeof (OrderDetails)].Fields["Product"];
        var orders = session.Query.All<Order>()
          .Take(500)
          .Prefetch(o => o.OrderDetails.Prefetch(od => od.Product));
        foreach (var order in orders) {
          var entitySetState = GetFullyLoadedEntitySet(session, order.Key, detailsField);
          foreach (var detailKey in entitySetState) {
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(detailKey, detailKey.TypeInfo, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
            PrefetchTestHelper.AssertReferencedEntityIsLoaded(detailKey, session, productField);
          }
        }
        Assert.AreEqual(11, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchSingleTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction())
        keys = session.Query.All<Order>().Select(o => o.Key).ToList();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var orderType = Domain.Model.Types[typeof (Order)];
        var employeeType = Domain.Model.Types[typeof (Employee)];
        var employeeField = Domain.Model.Types[typeof (Order)].Fields["Employee"];
        var ordersField = Domain.Model.Types[typeof (Employee)].Fields["Orders"];
        var orders = session.Query.Many<Order>(keys)
          .Prefetch(o => o.Employee.Orders);
        var count = 0;
        foreach (var order in orders) {
          Assert.AreEqual(keys[count], order.Key);
          count++;
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(order.Key, orderType, session,
            field => PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(employeeField) || (field.Parent != null && field.Parent.Equals(employeeField)));
          var state = session.EntityStateCache[order.Key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(
            state.Entity.GetFieldValue<Employee>(employeeField).Key,
            employeeType, session, field =>
              PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(ordersField));
        }
        Assert.AreEqual(keys.Count, count);
        Assert.AreEqual(15, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PreservingOrderInPrefetchManyNotFullBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Territory>()
          .ToList();
        var actual = expected
          .Prefetch(t => t.Employees.Prefetch(e => e.Orders))
          .ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreserveOrderingInPrefetchManySeveralBatchesTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Order>().ToList();
        var actual = expected
          .Prefetch(o => o.OrderDetails.Prefetch(od => od.Product))
          .ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleNotFullBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Order>().Take(53).ToList();
        var actual = expected.Prefetch(o => o.Employee.Orders).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleSeveralBatchesTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Order>().ToList();
        var actual = expected.Prefetch(o => o.Employee.Orders).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void ArgumentsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var a = session.Query.All<Territory>()
          .Prefetch(t => new {t.Id, t.Region})
          .ToList();
        var b = session.Query.All<Territory>()
          .Prefetch(t => t.Region.RegionDescription)
          .ToList();
        AssertEx.Throws<ArgumentException>(() => session.Query.All<Territory>()
          .Prefetch(t => t.PersistenceState)
          .ToList());
        var d = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(Key.Create<Model.OfferContainer>(Domain, 1)))
          .Prefetch(oc => oc.IntermediateOffer.AnotherContainer.RealOffer.Book)
          .ToList();
      }
    }

    [Test]
    public void SimultaneouslyUsageOfMultipleEnumeratorsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<Order>().ToList();
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
    public void RootElementIsNullPrefetchTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          new Model.Book {Title = new Model.Title {Text = "T0"}, Category = "1"};
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var books = session.Query.All<Model.Book>().AsEnumerable()
            .Concat(EnumerableUtils.One<Model.Book>(null)).Prefetch(b => b.Title);
          var titleField = Domain.Model.Types[typeof (Model.Book)].Fields["Title"];
          var titleType = Domain.Model.Types[typeof (Model.Title)];
          var count = 0;
          foreach (var book in books) {
            count++;
            if (book != null) {
              var titleKey = book.GetReferenceKey(titleField);
              PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
            }
          }
          Assert.AreEqual(2, count);
        }
      }
    }

    [Test]
    public void NestedPrefetchWhenChildElementIsNullTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var book0 = new Model.Book {Title = new Model.Title {Text = "T0"}, Category = "1"};
          var book1 = new Model.Book {Category = "2"};
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var prefetcher = session.Query.All<Model.Book>()
            .Prefetch(b => b.Title.Book);
          var titleField = Domain.Model.Types[typeof (Model.Book)].Fields["Title"];
          var titleType = Domain.Model.Types[typeof (Model.Title)];
          foreach (var book in prefetcher) {
            var titleKey = book.GetReferenceKey(titleField);
            if (titleKey != null)
              PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
          }
        }
      }
    }

    [Test]
    public void NestedPrefetchWhenRootElementIsNullTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          var book = new Model.Book {Title = new Model.Title {Text = "T0"}, Category = "1"};
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var books = session.Query.All<Model.Book>().AsEnumerable().Concat(EnumerableUtils.One<Model.Book>(null))
            .Prefetch(b => b.Title.Book);
          var titleField = Domain.Model.Types[typeof (Model.Book)].Fields["Title"];
          var titleType = Domain.Model.Types[typeof (Model.Title)];
          var count = 0;
          foreach (var book in books) {
            count++;
            if (book!=null) {
              var titleKey = book.GetReferenceKey(titleField);
              if (titleKey != null)
                PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
            }
          }
          Assert.AreEqual(2, count);
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var containers = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(containerKey))
          .Prefetch(oc => oc.RealOffer.Book)
          .Prefetch(oc => oc.IntermediateOffer.RealOffer.BookShop);
        foreach (var key in containers) {
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book0Key, book0Key.TypeInfo, session);
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(bookShop1Key, bookShop1Key.TypeInfo, session);
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var containers = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(containerKey))
          .Prefetch(oc => oc.IntermediateOffer);
        foreach (var key in containers) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(containerKey, containerKey.TypeInfo, session,
            field => PrefetchTestHelper.IsFieldToBeLoadedByDefault(field) || field.Name.StartsWith("IntermediateOffer"));
        }
      }
    }

    private void RemoveAllBooks()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        foreach (var book in session.Query.All<Model.Book>())
          book.Remove();
        tx.Complete();
      }
    }

    private static EntitySetState GetFullyLoadedEntitySet(Session session, Key key,
      FieldInfo employeesField)
    {
      EntitySetState entitySetState;
      Assert.IsTrue(session.Handler.LookupState(key, employeesField, out entitySetState));
      Assert.IsTrue(entitySetState.IsFullyLoaded);
      return entitySetState;
    }
  }
}