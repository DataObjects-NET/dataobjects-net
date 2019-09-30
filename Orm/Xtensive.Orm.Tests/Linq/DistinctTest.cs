// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

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
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
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
      var expected = Session.Query.All<Customer>()
        .ToList()
        .OrderBy(c => c.CompanyName)
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      Assert.IsTrue(expected.SequenceEqual(result));
      QueryDumper.Dump(result);
    }

    [Test]
    public void DefaultTest()
    {
      var result = Session.Query.All<Customer>().Distinct();
      var expected = Session.Query.All<Customer>().ToList().Distinct();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void ScalarTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => c.Address.City)
        .Distinct();
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      Assert.IsTrue(expected.SequenceEqual(result));
      QueryDumper.Dump(result);
    }

    [Test]
    public void CountTest()
    {
      var result = Session.Query.All<Customer>()
        .Distinct()
        .Count();
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Invoice>()
        .ToList()
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
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(c => c.FirstName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }
  }
}