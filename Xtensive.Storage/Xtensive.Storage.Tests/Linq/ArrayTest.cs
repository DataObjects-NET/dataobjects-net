// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Comparison;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  [Serializable]
  public class ArrayTest : NorthwindDOModelTest
  {
    [Test]
    public void NewIntArrayTest()
    {
      var result = Query<Customer>.All.Select(x => new[] {1, 2});
      foreach (var array in result) {
        Assert.AreEqual(2, array.Length);
        Assert.AreEqual(1, array[0]);
        Assert.AreEqual(2, array[1]);
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewByteArrayTest()
    {
      var result = Query<Customer>.All.Select(x => new byte[] {1, 2});
      var expected = Query<Customer>.All.ToList().Select(x => new byte[] {1, 2});
      var comparer = AdvancedComparer<byte[]>.Default.EqualityComparerImplementation;
      Assert.AreEqual(0, expected.Except(result, comparer).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewStringArrayTest()
    {
      var result = Query<Customer>.All
        .Select(customer => new[] {
          customer.CompanyName,
          customer.ContactTitle
        });
      var expected = Query<Customer>.All
        .ToList()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.ContactTitle
        });
      var comparer = AdvancedComparer<string[]>.Default.EqualityComparerImplementation;
      Assert.AreEqual(0, expected.Except(result, comparer).Count());
      QueryDumper.Dump(result);
    }


    [Test]
    public void NewByteArrayAnonymousTest()
    {
      var result = Query<Product>.All
        .Select(p => new {
          Value = new byte[] {1, 2, 3},
          p.ProductName
        });
      var expected = Query<Product>.All
        .ToList()
        .Select(p => new {
          Value = new byte[] {1, 2, 3},
          p.ProductName
        });
      var list = result.ToList();
      var expectedList = expected.ToList();
      Assert.AreEqual(expectedList.Count, list.Count);
      var comparer = AdvancedComparer<byte[]>.Default;
      for (int i = 0; i < expectedList.Count; i++)
        Assert.IsTrue(comparer.Equals(expectedList[i].Value, list[i].Value));
      QueryDumper.Dump(result);
    }


    [Test]
    public void NewArrayConstantTest()
    {
      var method = MethodInfo.GetCurrentMethod().Name;
      var result =
        from r in
          from p in Query<Product>.All
          select new {
            Value = new byte[] {1, 2, 3},
            Method = method,
            p.ProductName
          }
        orderby r.ProductName
        where r.Method==method
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(method, i.Method);
      var expected =
        from r in
          from p in Query<Product>.All.ToList()
          select new {
            Value = new byte[] {1, 2, 3},
            Method = method,
            p.ProductName
          }
        orderby r.ProductName
        where r.Method==method
        select r;
      var expectedList = expected.ToList();
      Assert.AreEqual(expectedList.Count, list.Count);
      var comparer = AdvancedComparer<byte[]>.Default;
      for (int i = 0; i < expectedList.Count; i++) {
        var expectedValue = expectedList[i];
        var value = list[i];
        Assert.AreEqual(expectedValue.Method, value.Method);
        Assert.IsTrue(comparer.Equals(expectedValue.Value, value.Value));
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void ArrayMemberAccessTest()
    {
      var result = Query<Customer>.All
        .Select(customer => new[] {
          customer.CompanyName,
          customer.ContactTitle
        })
        .Select(a => a[0]);
      var expected = Query<Customer>.All
        .ToList()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.ContactTitle
        })
        .Select(a => a[0]);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    [Ignore("Not supported")]
    public void ArrayAggregateAccessTest()
    {
      var result = Query<Customer>.All
        .Select(x => new byte[] {1, 2})
        .Select(a => a[0])
        .Sum(b => b);
      var expected = Query<Customer>.All
        .ToList()
        .Select(x => new byte[] {1, 2})
        .Select(a => a[0])
        .Sum(b => b);
      Assert.AreEqual(expected, result);
      QueryDumper.Dump(result);
    }

    [Test]
    [Ignore("Not supported")]
    public void ArrayExpressionIndexAccessTest()
    {
      var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
      var result = Query<Category>.All
        .Select(category => bytes[category.Id]);
      var expected = Query<Category>.All
        .ToList()
        .Select(category => bytes[category.Id]);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }
  }
}