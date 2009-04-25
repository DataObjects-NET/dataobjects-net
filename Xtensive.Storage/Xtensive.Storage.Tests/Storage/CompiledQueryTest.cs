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
    public void CompileSequence0Test()
    {
      var compiled = CompiledQuery.Compile(() => Query<Product>.All.Where(p => p.ProductName != null));
      var result = compiled();
    }

    [Test]
    public void CompileSequence1Test()
    {
      var compiled = CompiledQuery.Compile((string productName) => Query<Product>.All.Where(p => p.ProductName == productName));
      var result = compiled("Chai");
    }

    [Test]
    public void CompileSequence2Test()
    {
      var compiled = CompiledQuery.Compile((string productName, decimal unitPrice) => Query<Product>.All.Where(p => p.ProductName == productName && p.UnitPrice > unitPrice));
      var result = compiled("Chai", 10);
    }

    [Test]
    public void CompileScalar0Test()
    {
      var compiled = CompiledQuery.Compile(() => Query<Product>.All.Where(p => p.ProductName != null).Count());
      var result = compiled();
    }

    [Test]
    public void CompileScalar1Test()
    {
      var compiled = CompiledQuery.Compile((string productName) => Query<Product>.All.Where(p => p.ProductName == productName).Count());
      var result = compiled("Chai");
    }

    [Test]
    public void CompileScalar2Test()
    {
      var compiled = CompiledQuery.Compile((string productName, decimal unitPrice) => Query<Product>.All.Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).Count());
      var result = compiled("Chai", 10);
    }

    [Test]
    public void CachedSequenceTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = CompiledQuery.Execute(() => Query<Product>.All.Where(p => p.ProductName == productName && p.UnitPrice > unitPrice));
    }

    [Test]
    public void CachedScalarTest()
    {
      var productName = "Chai";
      var unitPrice = 10;
      var result = CompiledQuery.Execute(() => Query<Product>.All.Where(p => p.ProductName == productName && p.UnitPrice > unitPrice).Count());
    }
  }
}