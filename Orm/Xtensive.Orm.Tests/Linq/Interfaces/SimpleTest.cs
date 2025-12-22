// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
      var expected = Invoices.Count();
      var result = Session.Query.All<IHasCommission>().ToList();
      Assert.That(result.Count, Is.EqualTo(expected));
      Assert.That(result.All(i => i!=null), Is.True);
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
      Assert.That(result.Count, Is.GreaterThan(0));
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
      Assert.That(expectedCount, Is.GreaterThan(0));
      foreach (var item in result) {
        Assert.That(item.c1, Is.EqualTo(expectedCount));
        Assert.That(item.c2, Is.EqualTo(expectedCount));
      }
    }

    [Test]
    public void QueryByInterfaceTest()
    {
      var actual = Session.Query.All<IHasCommission>().ToList();
      var expected = Invoices;
      Assert.That(expected.Except(actual.Cast<Invoice>()).Count(), Is.EqualTo(0));
    }
  }
}