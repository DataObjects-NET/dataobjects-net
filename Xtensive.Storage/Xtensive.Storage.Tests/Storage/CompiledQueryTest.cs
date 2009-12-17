// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.25

using System;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Storage
{
  [Serializable]
  public class CompiledQueryTest : NorthwindDOModelTest
  {
    [Test]
    public void CachedSequenceTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.Execute(() => Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice));
    }

    [Test]
    public void ScalarLongTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).LongCount();
    }

    [Test]
    public void CachedScalarLongTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.Execute(() => Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).LongCount());
    }

    [Test]
    public void CachedScalarTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = Query.Execute(() => Query.All<Product>().Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).Count());
    }
  }
}