// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.01

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Core;
using System.Linq.Dynamic;

namespace Xtensive.Orm.Tests.Linq.Interfaces
{
  [TestFixture, Category("Linq")]
  public sealed class SimpleTest : NorthwindDOModelTest
  {
    public class DTO
    {
      public decimal? Freight;
    }

    [Test]
    public void QueryTest()
    {
      var result = Session.Query.All<IHasFreight>();
      var list = result.ToList();
      Assert.AreEqual(830, list.Count);
      Assert.IsTrue(list.All(i => i != null));
    }

#if NET40 
    [Test]
    public void QueryUnknownTypeDynamicTest()
    {
      var type = typeof (Order);
      var queryable = Session.Query.All(type);
      var result = new List<DTO>();
      var anonyms = queryable.Select("new(Freight as Freight)");
      foreach (dynamic anonym in anonyms)
        result.Add(new DTO() {Freight = anonym.Freight});
      Assert.Greater(result.Count, 0);
    }
#endif

    [Test]
    public void QueryOfUnknownTypeCastTest()
    {
      var type = typeof(Order);
      var queryable = Session.Query.All(type);
      var result = queryable.Cast<IHasFreight>()
        .Select(i => new DTO() {Freight = i.Freight})
        .ToList();
    }

    [Test]
    public void ComplexQueryOfUnknownTypeTest()
    {
      var type = typeof(Order);
      var queryable = Session.Query.All(type);
      var result = queryable.Cast<IHasFreight>()
        .Select(i => new {
          i, 
          c1 = queryable.Count(),
          c2 = Session.Query.All(type).Count()
        })
        .ToList();

      int expectedCount = result.Count;
      Assert.Greater(expectedCount, 0);
      foreach (var item in result) {
        Assert.AreEqual(expectedCount, item.c1);
        Assert.AreEqual(expectedCount, item.c2);
      }
    }

    [Test]
    public void QueryByInterfaceTest()
    {
      var actual = Session.Query.All<IHasFreight>().ToList();
      var expected = Session.Query.All<Order>().ToList();
      Assert.AreEqual(0, expected.Except(actual.Cast<Order>()).Count());
    }
  }
}