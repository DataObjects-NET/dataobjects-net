// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.25

using System;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage
{
  [Serializable]
  public class CompiledQueryTest : ChinookDOModelTest
  {
    [Test]
    public void CachedSequenceTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = Session.Query.Execute(qe => qe.All<Track>().Where(p => p.Name==trackName && p.UnitPrice > unitPrice));
    }

    [Test]
    public void CachedSubquerySequenceTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var addresses = Session.Query.All<Customer>()
        .Select(c => c.Address)
        .ToList();
      var expectedItems = Session.Query.All<Customer>()
        .Select(c => new {Customer = c, ProductsCount = c.Invoices.Count})
        .ToDictionary(a => a.Customer.Address);
      foreach (var address in addresses) {
        var result = Session.Query.Execute(
          qe => qe.All<Customer>()
            .Where(c => c.Address==address)
            .Select(
              c => new {
                Customer = c,
                Products = Session.Query.All<Invoice>().Where(p => p.Customer.Address==address)
              })
        ).ToList();
        var expected = expectedItems[address];
        Assert.AreSame(expected.Customer, result.Single().Customer);
        Assert.AreEqual(expected.ProductsCount, result.Single().Products.ToList().Count);
      }
    }

    [Test]
    public void ScalarLongTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = Session.Query.All<Track>().Where(p => p.Name==trackName && p.UnitPrice > unitPrice).LongCount();
    }

    [Test]
    public void CachedScalarLongTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = Session.Query.Execute(qe => qe.All<Track>().Where(p => p.Name==trackName && p.UnitPrice > unitPrice).LongCount());
    }

    [Test]
    public void CachedScalarTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = Session.Query.Execute(qe => qe.All<Track>().Where(p => p.Name==trackName && p.UnitPrice > unitPrice).Count());
    }
  }
}