// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Collections.Generic;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class DistinctTest : NorthwindDOModelTest
  {
    [Test]
    public void BlobTest()
    {
      var result = Query<Category>.All.Select(c => c.Picture).Distinct();
      var list = result.ToList();
    }

    [Test]
    public void OrderBy2Test()
    {
      var result = Query<Customer>.All
        .OrderBy(c => c.CompanyName)
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      var expected = Query<Customer>.All
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
      var result = Query<Customer>.All.Distinct();
      var expected = Query<Customer>.All.ToList().Distinct();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void ScalarTest()
    {
      var result = Query<Customer>.All
        .Select(c => c.Address.City)
        .Distinct();
      var expected = Query<Customer>.All
        .ToList()
        .Select(c => c.Address.City)
        .Distinct();
      Assert.AreEqual(0, expected.Except(result).Count());
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void OrderByTest()
    {
      var result = Query<Customer>.All
        .OrderBy(c => c.Id)
        .Select(c => c.Address.City)
        .Distinct();
      var expected = Query<Customer>.All
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
      var result = Query<Customer>.All
        .Select(c => c.Address.City)
        .Distinct()
        .OrderBy(c => c);
      var expected = Query<Customer>.All
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
      var result = Query<Customer>.All
        .Distinct()
        .Count();
      var expected = Query<Customer>.All
        .ToList()
        .Distinct()
        .Count();
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void SelectDistinctCountTest()
    {
      var result = Query<Customer>.All
        .Select(c => c.Address.City)
        .Distinct()
        .Count();
      var expected = Query<Customer>.All
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
      var result = Query<Customer>.All
        .Select(c => c.Address)
        .Select(a => a.City)
        .Distinct()
        .Count();
      var expected = Query<Customer>.All
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
      var result = Query<Customer>.All
        .Distinct()
        .Count(c => c.Id=="ALFKI");
      var expected = Query<Customer>.All
        .ToList()
        .Distinct()
        .Count(c => c.Id=="ALFKI");
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumWithArgTest()
    {
      var result = Query<Order>.All
        .Distinct()
        .Sum(o => o.Id);
      var expected = Query<Order>.All
        .ToList()
        .Distinct()
        .Sum(o => o.Id);
      Assert.AreEqual(expected, result);
      Assert.Greater(result, 0);
    }

    [Test]
    public void SumTest()
    {
      var result = Query<Order>.All
        .Select(o => o.Id)
        .Distinct()
        .Sum();
      var expected = Query<Order>.All
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
      var result = Query<Order>.All
        .OrderBy(o => o.Id)
        .Take(5)
        .Distinct().OrderBy(o => o.Id)
        .ToList();
      var expected = Query<Order>.All
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
      var result = Query<Order>.All
        .OrderBy(o => o.Id)
        .Take(2)
        .Take(1);
      var expected = Query<Order>.All
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
      var result = Query<Customer>.All.Select(c => c.Key).Distinct().Take(5);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void DistinctTakeTest()
    {
      // NOTE: Top must be forced to apply after distinct has been computed
      var result = Query<Order>.All
        .Distinct().OrderBy(o => o.Id)
        .Take(5);
      var expected = Query<Order>.All
        .ToList()
        .Distinct().OrderBy(o => o.Id)
        .Take(5);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void TakeCountTest()
    {
      var result = Query<Order>.All
        .Distinct()
        .Take(5)
        .Count();
      var expected = Query<Order>.All
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
      var result = Query<Order>.All
        .Take(5)
        .Distinct()
        .Count();
      var expected = Query<Order>.All
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
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .Skip(5)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      var expected = Query<Order>.All
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
      var result = Query<Customer>.All
        .Distinct()
        .OrderBy(c => c.ContactName)
        .Skip(5);
      var expected = Query<Customer>.All
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
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .Skip(5)
        .Take(10)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      var expected = Query<Order>.All
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
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .Take(10)
        .Skip(5)
        .Select(o => o.OrderDate)
        .Distinct().OrderBy(d => d);
      var expected = Query<Order>.All
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
      var result = Query<Customer>.All
        .Select(c => c.ContactName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      var expected = Query<Customer>.All
        .ToList()
        .Select(c => c.ContactName)
        .Distinct()
        .OrderBy(c => c)
        .Skip(5)
        .Take(10);
      Assert.IsTrue(expected.SequenceEqual(result));
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void ReuseTakeTest()
    {
      var result1 = GetCustomers(1).Count();
      Assert.AreEqual(1, result1);
      var result2 = GetCustomers(2).Count();
      Assert.AreEqual(2, result2);
    }

    private IEnumerable<Customer> GetCustomers(int amount)
    {
      return Query.Execute(() => Query<Customer>.All.Take(amount));
    }
  }
}