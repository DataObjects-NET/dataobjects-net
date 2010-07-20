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
using Xtensive.Core.Collections;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Core;
using System.Linq.Dynamic;

namespace Xtensive.Storage.Tests.Linq.Interfaces
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
      var result = Query.All<IHasFreight>();
      var list = result.ToList();
      Assert.AreEqual(830, list.Count);
      Assert.IsTrue(list.All(i => i != null));
    }

    [Test]
    public void QueryUnknownTypeDynamicTest()
    {
      var type = typeof (Order);
      var queryable = Query.All(type);
      var result = new List<DTO>();
      foreach (dynamic anonym in queryable.Select("new(Freight as Freight)"))
        result.Add(new DTO() {Freight = anonym.Freight});
      Assert.Greater(result.Count, 0);
    }

    [Test]
    public void QueryUnknownTypeCastTest()
    {
      var type = typeof(Order);
      var queryable = Query.All(type);
      var result = queryable.Cast<IHasFreight>()
        .Select(i => new DTO() {Freight = i.Freight})
        .ToList();
    }

    [Test]
    public void QueryByInterfaceTest()
    {
      var actual = Query.All<IHasFreight>().ToList();
      var expected = Query.All<Order>().ToList();
      Assert.AreEqual(0, expected.Except(actual.Cast<Order>()).Count());
    }
  }
}