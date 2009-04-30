// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.30

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
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
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewByteArrayTest()
    {
      var result = Query<Customer>.All.Select(x => new byte[] {1, 2});
      QueryDumper.Dump(result);
    }

    [Test]
    public void NewStringArrayTest()
    {
      var result = Query<Customer>.All.Select(customer => new[] {customer.CompanyName, customer.ContactTitle});
      QueryDumper.Dump(result);
    }


    [Test]
    public void NewByteArrayAnonimousTest()
    {
      var products = Query<Product>.All;
      var k = 123;
      var result = products.Select(p => new {
        Value = new byte[] {1, 2, 3},
        p.ProductName
      });
      QueryDumper.Dump(result);
    }


    [Test]
    public void NewArrayConstantTest()
    {
      var method = MethodInfo.GetCurrentMethod().Name;
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
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
      QueryDumper.Dump(result);
    }
  }
}