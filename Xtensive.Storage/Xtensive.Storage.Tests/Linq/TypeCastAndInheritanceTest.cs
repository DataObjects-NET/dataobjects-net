// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.10

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class TypeCastAndInheritanceTest : NorthwindDOModelTest
  {
    [Test]
    public void InheritanceCountTest()
    {
      var productCount = Query<Product>.All.Count();
      var discontinuedProductCount = Query<DiscontinuedProduct>.All.Count();
      var activeProductCount = Query<ActiveProduct>.All.Count();
      Assert.IsTrue(productCount > 0);
      Assert.IsTrue(discontinuedProductCount > 0);
      Assert.IsTrue(activeProductCount > 0);
      Assert.AreEqual(productCount, discontinuedProductCount, activeProductCount);
    }

    [Test]
    public void IsSimpleTest()
    {
      var result = Query<Product>.All.Where(p => p is DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsCountTest()
    {
      Assert.AreEqual(
        Query<Product>.All
          .Where(p => p is DiscontinuedProduct)
          .Select(product => (DiscontinuedProduct) product)
          .Count(),
        Query<DiscontinuedProduct>.All.Count());
      Assert.AreEqual(
        Query<Product>.All
          .Where(p => p is ActiveProduct)
          .Select(product => (ActiveProduct) product)
          .Count(),
        Query<ActiveProduct>.All.Count());
      Assert.AreEqual(
        Query<Product>.All
          .Where(p => p is Product)
          .Count(),
        Query<Product>.All.Count());
    }

    [Test]
    public void OfTypeSimpleTest()
    {
      var result = Query<Product>.All.OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeCountTest()
    {
      var productCount1 = Query<Product>.All.Count();
      var productCount2 = Query<Product>.All.OfType<Product>().Count();
      Assert.AreEqual(productCount1, productCount2);

      var discontinuedProductCount1 = Query<DiscontinuedProduct>.All.Count();
      var discontinuedProductCount2 = Query<DiscontinuedProduct>.All.OfType<DiscontinuedProduct>().Count();
      Assert.AreEqual(discontinuedProductCount1, discontinuedProductCount2);

      var activeProductCount1 = Query<ActiveProduct>.All.Count();
      var activeProductCount2 = Query<ActiveProduct>.All.OfType<ActiveProduct>().Count();
      Assert.AreEqual(activeProductCount1, activeProductCount2);
    }

    [Test]
    public void CastSimpleTest()
    {
      var discontinuedProducts = Query<DiscontinuedProduct>.All.Cast<Product>();
      QueryDumper.Dump(discontinuedProducts);
    }


    [Test]
    public void CastCountTest()
    {
      var productCount1 = Query<Product>.All.Count();
      var productCount2 = Query<Product>.All.Cast<Product>().Count();
      Assert.AreEqual(productCount1, productCount2);

      var discontinuedProductCount1 = Query<DiscontinuedProduct>.All.Count();
      var discontinuedProductCount2 = Query<DiscontinuedProduct>.All.Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(discontinuedProductCount1, discontinuedProductCount2);

      var activeProductCount1 = Query<ActiveProduct>.All.Count();
      var activeProductCount2 = Query<ActiveProduct>.All.Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(activeProductCount1, activeProductCount2);
    }
  }
}