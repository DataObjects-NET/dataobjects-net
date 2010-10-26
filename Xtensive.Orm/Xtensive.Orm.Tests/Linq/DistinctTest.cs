// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class DistinctTest : NorthwindDOModelTest
  {
    [Test]
    public void BlobTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Category>().Select(c => c.Picture).Distinct();
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
        .OrderBy(c => c.Id)
        .Select(c => c.Address.City)
        .Distinct()
        .ToList();
      var expected = Session.Query.All<Customer>()
        .ToList()
        .OrderBy(c => c.Id)
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
        .Count(c => c.Id=="ALFKI");
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Distinct()
        .Count(c => c.Id=="ALFKI");
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var result = Session.Query.All<Order>()
        .Distinct()
        .Sum(o => o.Id);
      var expected = Session.Query.All<Order>()
        .ToList()
        .Distinct()
        .Sum(o => o.Id);
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumTest()
    {
      var result = Session.Query.All<Order>()
        .Select(o => o.Id)
        .Distinct()
        .Sum();
      var expected = Session.Query.All<Order>()
        .ToList()
        .Select(o => o.Id)
        .Distinct()
        .Sum();
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void TakeTest()
    {
      // NOTE: Distinct must be forced to apply after top has been computed
      var result = Session.Query.All<Order>()
        .OrderBy(o => o.Id)
        .Take(5)
        .Distinct().OrderBy(o => o.Id)
        .ToList();
      var expected = Session.Query.All<Order>()
        .ToList()
        .OrderBy(o => o.Id)
        .Take(5)
        .Distinct().OrderBy(o => o.Id);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeTakeTest()
    {
      var result = Session.Query.All<Order>()
        .OrderBy(o => o.Id)
        .Take(2)
        .Take(1);
      var expected = Session.Query.All<Order>()
        .ToList()
        .OrderBy(o => o.Id)
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
      var result = Session.Query.All<Order>()
        .Distinct().OrderBy(o => o.Id)
        .Take(5);
      var expected = Session.Query.All<Order>()
        .ToList()
        .Distinct().OrderBy(o => o.Id)
        .Take(5);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeCountTest()
    {
      var result = Session.Query.All<Order>()
        .Distinct()
        .Take(5)
        .Count();
      var expected = Session.Query.All<Order>()
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
      var result = Session.Query.All<Order>()
        .Take(5)
        .Distinct()
        .Count();
      var expected = Session.Query.All<Order>()
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
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var result = Session.Query.All<Order>()
        .OrderBy(o => o.OrderDate)
        .Skip(5)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      var expected = Session.Query.All<Order>()
        .ToList()
        .OrderBy(o => o.OrderDate)
        .Skip(5)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void DistinctSkipTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var result = Session.Query.All<Customer>()
        .Distinct()
        .OrderBy(c => c.ContactName)
        .Skip(5);
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Distinct()
        .OrderBy(c => c.ContactName)
        .Skip(5);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void SkipTakeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var result = Session.Query.All<Order>()
        .OrderBy(o => o.OrderDate)
        .Skip(5)
        .Take(10)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      var expected = Session.Query.All<Order>()
        .ToList()
        .OrderBy(o => o.OrderDate)
        .Skip(5)
        .Take(10)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeSkipTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var result = Session.Query.All<Order>()
        .OrderBy(o => o.OrderDate)
        .Take(10)
        .Skip(5)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      var expected = Session.Query.All<Order>()
        .ToList()
        .OrderBy(o => o.OrderDate)
        .Take(10)
        .Skip(5)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void DistinctSkipTakeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var result = Session.Query.All<Customer>()
        .Select(c => c.ContactName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(c => c.ContactName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }
  }
}