// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.15

using System.Collections;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class ExecuteTest : NorthwindDOModelTest
  {
    [Test]
    public void NonGenericTest()
    {
      var query = Query.All<Order>().Where(o => o.Freight > 5);
      var nonGenericQuery = (IQueryable) query;
      foreach (var item in nonGenericQuery) {
        Assert.IsNotNull(item);
        var order = item as Order;
        Assert.IsNotNull(order);
      }

      query = Query.All<Order>();
      nonGenericQuery = query;
      foreach (var item in nonGenericQuery) {
        Assert.IsNotNull(item);
        var order = item as Order;
        Assert.IsNotNull(order);
      }

      var provider = (IQueryProvider) QueryProvider.Instance;
      var result = provider.Execute(nonGenericQuery.Expression);
      var enumerable = (IEnumerable) result;
      foreach (var item in enumerable) {
        Assert.IsNotNull(item);
        var order = item as Order;
        Assert.IsNotNull(order);
      }

      query = Query.All<Order>().Where(o => o.Freight > 5);
      nonGenericQuery = (IQueryable) query;
      provider = (IQueryProvider) QueryProvider.Instance;
      result = provider.Execute(nonGenericQuery.Expression);
      enumerable = (IEnumerable) result;
      foreach (var item in enumerable) {
        Assert.IsNotNull(item);
        var order = item as Order;
        Assert.IsNotNull(order);
      }
    }
  }
}