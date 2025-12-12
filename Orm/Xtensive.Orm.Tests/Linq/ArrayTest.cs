// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
        Assert.That(array.Length, Is.EqualTo(2));
        Assert.That(array[0], Is.EqualTo(1));
        Assert.That(array[1], Is.EqualTo(2));
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewByteArrayTest()
    {
      var result = Session.Query.All<Customer>().Select(x => new byte[] {1, 2});
      var expected = Customers.Select(x => new byte[] {1, 2});
      var comparer = AdvancedComparer<byte[]>.Default.EqualityComparerImplementation;
      Assert.That(expected.Except(result, comparer).Count(), Is.EqualTo(0));
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
      var expected = Customers
        .ToList()
        .Select(customer => new[] {
          customer.CompanyName,
          customer.LastName
        });
      var comparer = AdvancedComparer<string[]>.Default.EqualityComparerImplementation;
      Assert.That(expected.Except(result, comparer).Count(), Is.EqualTo(0));
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
      var expected = Tracks
        .Select(p => new {
          Value = new byte[] {1, 2, 3},
          p.Name
        });
      var list = result.ToList();
      var expectedList = expected.ToList();
      Assert.That(list.Count, Is.EqualTo(expectedList.Count));
      var comparer = AdvancedComparer<byte[]>.Default;
      for (int i = 0; i < expectedList.Count; i++)
        Assert.That(comparer.Equals(expectedList[i].Value, list[i].Value), Is.True);
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
        Assert.That(i.Method, Is.EqualTo(method));
      var expected =
        from r in
          from p in Tracks
          select new {
            Value = new byte[] {1, 2, 3},
            Method = method,
            p.Name
          }
        orderby r.Name
        where r.Method==method
        select r;
      var expectedList = expected.ToList();
      Assert.That(list.Count, Is.EqualTo(expectedList.Count));
      var comparer = AdvancedComparer<byte[]>.Default;
      for (int i = 0; i < expectedList.Count; i++) {
        var expectedValue = expectedList[i];
        var value = list[i];
        Assert.That(value.Method, Is.EqualTo(expectedValue.Method));
        Assert.That(comparer.Equals(expectedValue.Value, value.Value), Is.True);
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
      var expected = Customers
        .Select(customer => new[] {
          customer.CompanyName,
          customer.LastName
        })
        .Select(a => a[0]);
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
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
      var expected = Customers
        .Select(x => new byte[] {1, 2})
        .Select(a => a[0])
        .Sum(b => b);
      Assert.That(result, Is.EqualTo(expected));
      QueryDumper.Dump(result);
    }

    [Test]
    [Ignore("Not supported")]
    public void ArrayExpressionIndexAccessTest()
    {
      var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
      var result = Session.Query.All<Employee>()
        .Select(e => bytes[e.EmployeeId]);
      var expected = Employees
        .Select(e => bytes[e.EmployeeId]);
      Assert.That(expected.Except(result).Count(), Is.EqualTo(0));
      QueryDumper.Dump(result);
    }
  }
}