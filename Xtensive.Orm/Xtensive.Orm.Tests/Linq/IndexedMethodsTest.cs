// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.14

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class IndexedMethodsTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
    }
    
    [Test]
    public void SelectIndexedTest()
    {
      var result = Session.Query.All<Order>().OrderBy(order=>order.Id).Select((order, index) => new {order, index}).ToList();
      var expected = Session.Query.All<Order>().AsEnumerable().OrderBy(order=>order.Id).Select((order, index) => new {order, index}).ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void SelectIndexedWhereTest()
    {
      var result = Query.All<Customer>()
        .Select((c, i) => new {Customer = c, Index = i})
        .Where(a => a.Customer.Id == "SPLIR")
        .ToList();
      Assert.AreEqual(1, result.Count);
      Assert.Greater(result[0].Index, 1);
    }

    [Test]
    public void SelectManyIndexedTest()
    {
      var count = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>()
        .OrderBy(customer=>customer.Id)
        .SelectMany((customer, index) => customer.Orders.OrderBy(order=>order.Id).Select(order=>new {index, order.Freight}));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .OrderBy(customer=>customer.Id)
        .SelectMany((customer, index) => customer.Orders.OrderBy(order=>order.Id).Select(order=>new {index, order.Freight}));
      Assert.IsTrue(expected.SequenceEqual(result));
    }

//    public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector);
    [Test]
    public void SelectManyIndexedResultSelectorTest()
    {
      var result = Session.Query.All<Customer>()
        .OrderBy(customer=>customer.Id)
        .SelectMany((customer, index) => customer.Orders.OrderBy(order=>order.Id).Select(order=>new {index, order.Freight}), (customer, takenOrders)=>new {customer, takenOrders});
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .OrderBy(customer=>customer.Id)
        .SelectMany((customer, index) => customer.Orders.OrderBy(order=>order.Id).Select(order=>new {index, order.Freight}), (customer, takenOrders)=>new {customer, takenOrders});
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void WhereIndexedTest()
    {
      var avgFreight = Session.Query.All<Order>().Select(order => order.Freight).Average();
      var result = Session.Query.All<Order>().OrderBy(order=>order.Id).Where((order, index) =>index > 10 || order.Freight > avgFreight);
      var expected = Session.Query.All<Order>().AsEnumerable().OrderBy(order=>order.Id).Where((order, index) =>index > 10 || order.Freight > avgFreight);
      Assert.IsTrue(expected.SequenceEqual(result));
    }


//    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate);


  }
}