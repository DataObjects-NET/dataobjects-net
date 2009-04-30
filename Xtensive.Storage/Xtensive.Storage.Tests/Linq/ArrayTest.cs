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

    [Test]
    public void ArrayMemberAccessTest()
    {
      var result = Query<Customer>.All
        .Select(customer => new[] {customer.CompanyName, customer.ContactTitle})
        .Select(a => a[0]);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ArrayAggregateAccessTest()
    {
      var result = Query<Customer>.All
        .Select(x => new byte[] {1, 2})
        .Select(a => a[0])
        .Sum(b => b);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ArrayExpressionIndexAccessTest()
    {
      var bytes = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
      var result = Query<Category>.All
        .Select(category => bytes[category.Id]);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ArrayExpressionIndexAccess2Test()
    {
      var result = Query<Category>.All
        .Select(x => new{Category = x, Array = new byte[] {1, 2, 3, 4, 5, 6, 7, 8}} )
        .Select(a => a.Array[a.Category.Id]);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexTest()
    {
      var result = Query<Product>.All
        .Where(p => p.Category.Id==1 || p.Category.Id==2)
        .Select(p => new {
          Product = p,
          CategoryNames = new[] {
            Query<Category>.All.Where(c => c.Id==1).First().CategoryName,
            Query<Category>.All.Where(c => c.Id==2).First().CategoryName,
          },
          CategoryIds = new[] {
            Query<Category>.All.Where(c => c.Id==1).First().Id,
            Query<Category>.All.Where(c => c.Id==2).First().Id,
          },
          Categories = new[] {
            Query<Category>.All.Where(c => c.Id==1).First(),
            Query<Category>.All.Where(c => c.Id==2).First(),
          },
        })
        .Select(at => new {
          at.Product,
          FirstCategoryName = at.CategoryNames[at.CategoryIds[1]],
          SecondCategoryName = at.CategoryNames[at.CategoryIds[2]],
          ProductCategory = at.Categories[at.Product.Category.Id],
        }
        );
      QueryDumper.Dump(result);
    }
  }
}