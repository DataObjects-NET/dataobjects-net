// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.16

using System;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  [Ignore]
  public class TypedKeyTest : NorthwindDOModelTest
  {
    private class X
    {
      public TypedKey<Order> OrderKey;
      public int SomeInt;
    }


    [Test]
    public void GetEntityTest()
    {
      var xs = Query<Order>.All.Take(10).Select((order, index) =>
        new X {
          OrderKey = order.Key.ToTypedKey<Order>(),
          SomeInt = index
        })
        .ToList();

      var query =
        from o in Query<Order>.All
        from x in Query.Store(xs)
        where o==x.OrderKey.Entity
        select o;

      var expected = 
        from o in Query<Order>.All.AsEnumerable()
        from x in xs
        where o==x.OrderKey.Entity
        select o;

      Assert.AreEqual(0 , expected.Except(query).Count());
    }

    [Test]
    public void GetEntity2Test()
    {
      var xs = Query<Order>.All.Take(10).Select((order, index) =>
        new X {
          OrderKey = order.Key.ToTypedKey<Order>(),
          SomeInt = index
        })
        .ToList();

      var query =
        from o in Query<Order>.All
        from x in xs
        where o==x.OrderKey.Entity
        select o;

      var expected = 
        from o in Query<Order>.All.AsEnumerable()
        from x in xs
        where o==x.OrderKey.Entity
        select o;

      Assert.AreEqual(0 , expected.Except(query).Count());
    }

    [Test]
    public void KeyTest()
    {
      var keys = Query<Order>.All.Take(10).Select(order => order.Key).ToTyped<Order>().ToList();
      var query = Query<Order>.All.Join(keys, order => order.Key, key => key, (order, key) => new {order, key});
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Join(keys, order => order.Key, key => key, (order, key) => new {order, key});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }
  }
}