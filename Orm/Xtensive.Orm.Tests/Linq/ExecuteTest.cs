// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.15

using System.Collections;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class ExecuteTest : ChinookDOModelTest
  {
    [Test]
    public void NonGenericTest()
    {
      var query = Session.Query.All<Invoice>().Where(i => i.Commission > 0.3m);
      Assert.That(query, Is.Not.Empty);
      var nonGenericQuery = (IQueryable) query;
      foreach (var item in nonGenericQuery) {
        Assert.IsNotNull(item);
        var invoice = item as Invoice;
        Assert.IsNotNull(invoice);
      }

      query = Session.Query.All<Invoice>();
      nonGenericQuery = query;
      foreach (var item in nonGenericQuery) {
        Assert.IsNotNull(item);
        var invoice = item as Invoice;
        Assert.IsNotNull(invoice);
      }

      var provider = query.Provider;
      var result = provider.Execute(nonGenericQuery.Expression);
      var enumerable = (IEnumerable) result;
      foreach (var item in enumerable) {
        Assert.IsNotNull(item);
        var invoice = item as Invoice;
        Assert.IsNotNull(invoice);
      }

      query = Session.Query.All<Invoice>().Where(i => i.Commission > 0.3m);
      Assert.That(query, Is.Not.Empty);
      nonGenericQuery = (IQueryable) query;
      result = provider.Execute(nonGenericQuery.Expression);
      enumerable = (IEnumerable) result;
      foreach (var item in enumerable) {
        Assert.IsNotNull(item);
        var invoice = item as Invoice;
        Assert.IsNotNull(invoice);
      }
    }
  }
}