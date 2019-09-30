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
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Core;
using System.Linq.Dynamic;

namespace Xtensive.Orm.Tests.Linq.Interfaces
{
  [TestFixture, Category("Linq")]
  public sealed class SimpleTest : ChinookDOModelTest
  {
    public class DTO
    {
      public decimal? Commission;
    }

    [Test]
    public void QueryTest()
    {
      var expected = Session.Query.All<Invoice>().Count();
      var result = Session.Query.All<IHasCommission>().ToList();
      Assert.AreEqual(expected, result.Count);
      Assert.IsTrue(result.All(i => i!=null));
    }

    [Test]
    public void QueryUnknownTypeDynamicTest()
    {
      var type = typeof (Invoice);
      var queryable = Session.Query.All(type);
      var result = new List<DTO>();
      var anonyms = queryable.Select("new(Commission as Commission)");
      foreach (dynamic anonym in anonyms)
        result.Add(new DTO() {Commission = anonym.Commission});
      Assert.Greater(result.Count, 0);
    }

    [Test]
    public void QueryOfUnknownTypeCastTest()
    {
      var type = typeof (Invoice);
      var queryable = Session.Query.All(type);
      var result = queryable.Cast<IHasCommission>()
        .Select(i => new DTO() {Commission = i.Commission})
        .ToList();
    }

    [Test]
    public void ComplexQueryOfUnknownTypeTest()
    {
      var type = typeof (Invoice);
      var queryable = Session.Query.All(type);
      var result = queryable.Cast<IHasCommission>()
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
      var actual = Session.Query.All<IHasCommission>().ToList();
      var expected = Session.Query.All<Invoice>().ToList();
      Assert.AreEqual(0, expected.Except(actual.Cast<Invoice>()).Count());
    }
  }
}