// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.02


using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  [Serializable]
  public class OptimizerTest : NorthwindDOModelTest
  {
    [Test]
    public void MainTest()
    {
      var query =
        (from customer in Query<Customer>.All
        join order in Query<Order>.All on customer equals order.Customer into orderJoins
        from orderJoin in orderJoins.DefaultIfEmpty()
        select new {customer.Address, Order = orderJoin})
          .GroupBy(x => x.Address.City)
          .Select(g => new {City = g.Key, MaxFreight = g.Max(o => o.Order.Freight)});
      foreach (var queryable in query) {
        var city = queryable.City;
        var maxFreight = queryable.MaxFreight;
      }
    }

    [Test]
    public void Main2Test()
    {
      var query =
        from grouping in
          from customerOrderJoin in
            from customer in Query<Customer>.All
            join order in Query<Order>.All
              on customer equals order.Customer into orderJoins
            from orderJoin in orderJoins.DefaultIfEmpty()
            select new {customer.Address, Order = orderJoin}
          group customerOrderJoin by customerOrderJoin.Address.City
        select new {City = grouping.Key
          , MaxFreight = grouping.Max(o => o.Order.Freight)
          , MinFreight = grouping.Min(o => o.Order.Freight)
          , AverageFreight = grouping.Average(o => o.Order.Freight)
        };

      foreach (var queryable in query) {
        var city = queryable.City;
        var maxFreight = queryable.MaxFreight;
      }
    }
  }
}