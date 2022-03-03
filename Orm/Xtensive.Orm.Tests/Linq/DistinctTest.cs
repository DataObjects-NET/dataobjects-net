// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class DistinctTest : ChinookDOModelTest
  {
    [Test]
    public void BlobTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Track>().Select(c => c.Bytes).Distinct();
      var list = result.ToList();
    }

    [Test]
    public void OrderBy2Test()
    {
      var result = Session.Query.All<Customer>()
        .OrderBy(c => c.CompanyName)
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      var expected = Customers
        .OrderBy(c => c.CompanyName)
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird | StorageProvider.Sqlite | StorageProvider.Oracle)) {
        var storage = result.AsEnumerable().Where(c => !c.StartsWith('S'));
        var local = expected.Where(c => !c.StartsWith('S'));
        Assert.IsTrue(local.SequenceEqual(storage));
        var storageHashset = result.AsEnumerable().Where(c => c.StartsWith('S')).ToHashSet();
        local = expected.Where(c => c.StartsWith('S'));
        var count = 0;
        foreach (var item in local) {
          Assert.That(storageHashset.Contains(item));
          count++;
        }
        Assert.That(storageHashset.Count, Is.EqualTo(count));
      }
      else {
        Assert.IsTrue(expected.SequenceEqual(result));
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void DefaultTest()
    {
      var result = Session.Query.All<Customer>().Distinct();
      var expected = Customers.Distinct();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void ScalarTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.Address.City)
        .Distinct();
      var expected = Customers
        .Select(c => c.Address.City)
        .Distinct();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void OrderByTest()
    {
      var result = Session.Query.All<Customer>()
        .OrderBy(c => c.CustomerId)
        .Select(c => c.Address.City)
        .Distinct()
        .ToList();
      var expected = Customers
        .OrderBy(c => c.CustomerId)
        .Select(c => c.Address.City)
        .Distinct();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }


    [Test]
    public void DistinctOrderByTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      var expected = Customers
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);

      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird | StorageProvider.Sqlite | StorageProvider.Oracle)) {
        var storage = result.AsEnumerable().Where(c => !c.StartsWith('S'));
        var local = expected.Where(c => !c.StartsWith('S'));
        Assert.IsTrue(local.SequenceEqual(storage));
        var storageHashset = result.AsEnumerable().Where(c => c.StartsWith('S')).ToHashSet();
        local = expected.Where(c => c.StartsWith('S'));
        var count = 0;
        foreach (var item in local) {
          Assert.That(storageHashset.Contains(item));
          count++;
        }
        Assert.That(storageHashset.Count, Is.EqualTo(count));
      }
      else {
        Assert.IsTrue(expected.SequenceEqual(result));
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void CountTest()
    {
      var result = Session.Query.All<Customer>()
        .Distinct()
        .Count();
      var expected = Customers
        .Distinct()
        .Count();
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void SelectDistinctCountTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.Address.City)
        .Distinct()
        .Count();
      var expected = Customers
        .Select(c => c.Address.City)
        .Distinct()
        .Count();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void NestedSelectDistinctCountTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.Address)
        .Select(a => a.City)
        .Distinct()
        .Count();
      var expected = Customers
        .Select(c => c.Address)
        .Select(a => a.City)
        .Distinct()
        .Count();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void CountPredicateTest()
    {
      var result = Session.Query.All<Customer>()
        .Distinct()
        .Count(c => c.FirstName=="Leonie");
      var expected = Customers
        .Distinct()
        .Count(c => c.FirstName=="Leonie");
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var result = Session.Query.All<Invoice>()
        .Distinct()
        .Sum(o => o.InvoiceId);
      var expected = Invoices
        .Distinct()
        .Sum(o => o.InvoiceId);
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumTest()
    {
      var result = Session.Query.All<Invoice>()
        .Select(o => o.InvoiceId)
        .Distinct()
        .Sum();
      var expected = Invoices
        .Select(o => o.InvoiceId)
        .Distinct()
        .Sum();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void TakeTest()
    {
      // NOTE: Distinct must be forced to apply after top has been computed
      var result = Session.Query.All<Invoice>()
        .OrderBy(o => o.InvoiceId)
        .Take(5)
        .Distinct()
        .OrderBy(o => o.InvoiceId)
        .ToList();
      var expected = Invoices
        .OrderBy(o => o.InvoiceId)
        .Take(5)
        .Distinct()
        .OrderBy(o => o.InvoiceId);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeTakeTest()
    {
      var result = Session.Query.All<Invoice>()
        .OrderBy(o => o.InvoiceId)
        .Take(2)
        .Take(1);
      var expected = Invoices
        .OrderBy(o => o.InvoiceId)
        .Take(2)
        .Take(1);
      var list = result.ToList();
      Assert.IsTrue(expected.SequenceEqual(list));
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void DistinctTakeTest1()
    {
      var result = Session.Query.All<Customer>().Select(c => c.Key).Distinct().Take(5);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void DistinctTakeTest()
    {
      // NOTE: Top must be forced to apply after distinct has been computed
      var result = Session.Query.All<Invoice>()
        .Distinct()
        .OrderBy(o => o.InvoiceId)
        .Take(5);
      var expected = Invoices
        .Distinct()
        .OrderBy(o => o.InvoiceId)
        .Take(5);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeCountTest()
    {
      var result = Session.Query.All<Invoice>()
        .Distinct()
        .Take(5)
        .Count();
      var expected = Invoices
        .Distinct()
        .Take(5)
        .Count();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void TakeDistinctCountTest()
    {
      var result = Session.Query.All<Invoice>()
        .Take(5)
        .Distinct()
        .Count();
      var expected = Invoices
        .Take(5)
        .Distinct()
        .Count();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SkipTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var result = Session.Query.All<Invoice>()
        .OrderBy(o => o.InvoiceDate)
        .Skip(5)
        .Select(o => o.InvoiceDate)
        .Distinct().OrderBy(d => d);
      var expected = Invoices
        .OrderBy(o => o.InvoiceDate)
        .Skip(5)
        .Select(o => o.InvoiceDate)
        .Distinct().OrderBy(d => d);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void DistinctSkipTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var result = Session.Query.All<Customer>()
        .Distinct()
        .OrderBy(c => c.LastName)
        .Skip(5);
      var expected = Customers
        .Distinct()
        .OrderBy(c => c.LastName)
        .Skip(5);

      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SkipTakeTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var result = Session.Query.All<Invoice>()
        .OrderBy(o => o.InvoiceDate)
        .Skip(5)
        .Take(10)
        .Select(o => o.InvoiceDate)
        .Distinct()
        .OrderBy(d => d);
      var expected = Invoices
        .OrderBy(o => o.InvoiceDate)
        .Skip(5)
        .Take(10)
        .Select(o => o.InvoiceDate)
        .Distinct()
        .OrderBy(d => d);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeSkipTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var result = Session.Query.All<Invoice>()
        .OrderBy(o => o.InvoiceDate)
        .Take(10)
        .Skip(5)
        .Select(o => o.InvoiceDate)
        .Distinct()
        .OrderBy(d => d);
      var expected = Invoices
        .OrderBy(o => o.InvoiceDate)
        .Take(10)
        .Skip(5)
        .Select(o => o.InvoiceDate)
        .Distinct()
        .OrderBy(d => d);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void DistinctSkipTakeTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var result = Session.Query.All<Customer>()
        .Select(c => c.FirstName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      var expected = Customers
        .Select(c => c.FirstName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void DistinctByBoolExpression()
    {
      var result = Session.Query.All<Invoice>().Select(c => c.Status == (InvoiceStatus) 1)
        .Distinct()
        .ToArray();

      CollectionAssert.AreEquivalent(new[] {false, true}, result);
    }

    [Test]
    public void DistinctByBoolExpressionComplex()
    {
      var result = Session.Query.All<Invoice>()
        .Select(c => c.Status == (InvoiceStatus) 1 || c.Status == (InvoiceStatus) 2)
        .Distinct()
        .ToArray();

      CollectionAssert.AreEquivalent(new[] {false, true}, result);
    }
  }
}