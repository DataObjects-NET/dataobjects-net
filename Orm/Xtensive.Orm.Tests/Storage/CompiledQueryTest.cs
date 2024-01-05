// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.04.25

using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

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
      var result = Session.Query.Execute(qe => qe.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice));
    }

    [Test]
    public async Task CachedSequenceAsyncTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = await Session.Query.ExecuteAsync(qe => qe.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice));
    }

    [Test]
    public void CachedSubquerySequenceTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var addresses = Session.Query.All<Customer>()
        .Select(c => c.Address)
        .ToList();
      var expectedItems = Customers
        .Select(c => new { Customer = c, ProductsCount = c.Invoices.Count })
        .ToDictionary(a => a.Customer.Address);
      foreach (var address in addresses) {
        var result = Session.Query.Execute(
          qe => qe.All<Customer>()
            .Where(c => c.Address == address)
            .Select(
              c => new {
                Customer = c,
                Products = Session.Query.All<Invoice>().Where(p => p.Customer.Address == address)
              })
        ).ToList();
        var expected = expectedItems[address];
        Assert.AreSame(expected.Customer, result.Single().Customer);
        Assert.AreEqual(expected.ProductsCount, result.Single().Products.ToList().Count);
      }
    }

    [Test]
    public async Task CachedSubquerySequenceAsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var addresses = Session.Query.All<Customer>()
        .Select(c => c.Address)
        .ToList();
      var expectedItems = Session.Query.All<Customer>()
        .Select(c => new { Customer = c, ProductsCount = c.Invoices.Count })
        .ToDictionary(a => a.Customer.Address);
      foreach (var address in addresses) {
        var result = (await Session.Query.ExecuteAsync(
          qe => qe.All<Customer>()
            .Where(c => c.Address == address)
            .Select(
              c => new {
                Customer = c,
                Products = Session.Query.All<Invoice>().Where(p => p.Customer.Address == address)
              })
        )).ToList();
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
      var result = Session.Query.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice).LongCount();
    }

    [Test]
    public void CachedScalarLongTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = Session.Query.Execute(qe => qe.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice).LongCount());
    }

    [Test]
    public async Task CachedScalarLongAsyncTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = await Session.Query.ExecuteAsync(qe => qe.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice).LongCount());
    }

    [Test]
    public void CachedScalarTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = Session.Query.Execute(qe => qe.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice).Count());
    }

    [Test]
    public async Task CachedScalarAsyncTest()
    {
      var trackName = "Babylon";
      var unitPrice = 0.9m;
      var result = await Session.Query.ExecuteAsync(qe => qe.All<Track>().Where(p => p.Name == trackName && p.UnitPrice > unitPrice).Count());
    }
  }
}
