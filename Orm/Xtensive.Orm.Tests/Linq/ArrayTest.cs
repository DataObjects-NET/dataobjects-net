// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Comparison;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  [Serializable]
  public class ArrayTest : ChinookDOModelTest
  {
    [Test]
    public void NewIntArrayTest()
    {
      var result = Session.Query.All<Customer>().Select(x => new[] {1, 2});
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
      var result = Session.Query.All<Customer>().Select(x => new byte[] {1, 2});
      var expected = Session.Query.All<Customer>().ToList().Select(x => new byte[] {1, 2});
      var comparer = AdvancedComparer<byte[]>.Default.EqualityComparerImplementation;
      Assert.AreEqual(0, expected.Except(result, comparer).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewStringArrayTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.LastName
        });
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.LastName
        });
      var comparer = AdvancedComparer<string[]>.Default.EqualityComparerImplementation;
      Assert.AreEqual(0, expected.Except(result, comparer).Count());
      QueryDumper.Dump(result);
    }


    [Test]
    public void NewByteArrayAnonymousTest()
    {
      var result = Session.Query.All<Track>()
        .Select(p => new {
          Value = new byte[] {1, 2, 3},
          p.Name
        });
      var expected = Session.Query.All<Track>()
        .ToList()
        .Select(p => new {
          Value = new byte[] {1, 2, 3},
          p.Name
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
          from p in Session.Query.All<Track>()
          select new {
            Value = new byte[] {1, 2, 3},
            Method = method,
            p.Name
          }
        orderby r.Name
        where r.Method==method
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(method, i.Method);
      var expected =
        from r in
          from p in Session.Query.All<Track>().ToList()
          select new {
            Value = new byte[] {1, 2, 3},
            Method = method,
            p.Name
          }
        orderby r.Name
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
      var result = Session.Query.All<Customer>()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.LastName
        })
        .Select(a => a[0]);
      var expected = Session.Query.All<Customer>()
        .ToList()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.LastName
        })
        .Select(a => a[0]);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    [Ignore("Not supported")]
    public void ArrayAggregateAccessTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(x => new byte[] {1, 2})
        .Select(a => a[0])
        .Sum(b => b);
      var expected = Session.Query.All<Customer>()
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
      var result = Session.Query.All<Employee>()
        .Select(e => bytes[e.EmployeeId]);
      var expected = Session.Query.All<Employee>()
        .ToList()
        .Select(e => bytes[e.EmployeeId]);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }
  }
}