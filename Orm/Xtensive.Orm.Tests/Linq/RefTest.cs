// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.16

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  [Ignore]
  public class RefTest : NorthwindDOModelTest
  {
    private class X
    {
      public Ref<Order> OrderRef;
      public int SomeInt;
    }

    [Test]
    public void GetEntityTest()
    {
      var xs = Session.Query.All<Order>().OrderBy(o => o.Id).Take(10).Select((order, index) =>
        new X {
          OrderRef = (Ref<Order>) order.Key,
          SomeInt = index
        })
        .ToList();

      var query =
        from o in Session.Query.All<Order>()
        from x in Session.Query.Store(xs)
        where o==x.OrderRef.Value
        select o;

      var expected = 
        from o in Session.Query.All<Order>().AsEnumerable()
        from x in xs
        where o==x.OrderRef.Value
        select o;

      Assert.AreEqual(0 , expected.Except(query).Count());
    }

    [Test]
    public void GetEntity2Test()
    {
      var xs = Session.Query.All<Order>().OrderBy(o => o.Id).Take(10).Select((order, index) =>
        new X {
          OrderRef = (Ref<Order>) order.Key,
          SomeInt = index
        })
        .ToList();

      var query =
        from o in Session.Query.All<Order>()
        from x in xs
        where o==x.OrderRef.Value
        select o;

      var expected = 
        from o in Session.Query.All<Order>().AsEnumerable()
        from x in xs
        where o==x.OrderRef.Value
        select o;

      Assert.AreEqual(0 , expected.Except(query).Count());
    }

    [Test]
    public void KeyTest()
    {
      var refs = Session.Query.All<Order>().Take(10).Select(order => (Ref<Order>) order).ToList();
      var query = Session.Query.All<Order>()
        .Join(refs, order => order.Key, @ref => @ref.Key, (order, key) => new {order, key});
      QueryDumper.Dump(query);
      var expectedQuery = Session.Query.All<Order>().AsEnumerable()
        .Join(refs, order => order.Key, @ref => @ref.Key, (order, key) => new {order, key});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }
  }
}