// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
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
    public void IsSameTypeTest()
    {
#pragma warning disable 183
      var result = Query<Product>.All.Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSubTypeTest()
    {
#pragma warning disable 183
      var result = Query<DiscontinuedProduct>.All.Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IsIntermediateTest()
    {
      Query<Product>.All
        .Where(p => p is IntermediateProduct)
        .Select(product => (IntermediateProduct) product)
        .Count();
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IsCountTest()
    {
      int productCount = Query<Product>.All.Count();
      int intermediateProductCount = Query<IntermediateProduct>.All.Count();
      int discontinuedProductCount = Query<DiscontinuedProduct>.All.Count();
      int activeProductCount = Query<ActiveProduct>.All.Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Query<Product>.All
          .Where(p => p is IntermediateProduct)
          .Select(product => (IntermediateProduct) product)
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Query<Product>.All
          .Where(p => p is DiscontinuedProduct)
          .Select(product => (DiscontinuedProduct) product)
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Query<Product>.All
          .Where(p => p is ActiveProduct)
          .Select(product => (ActiveProduct) product)
          .Count());

#pragma warning disable 183
      Assert.AreEqual(
        productCount,
        Query<Product>.All
          .Where(p => p is Product)
          .Count());
#pragma warning restore 183
    }

    [Test]
    public void OfTypeSimpleTest()
    {
      var result = Query<Product>.All.OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSameTypeTest()
    {
      var result = Query<Product>.All.OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSubTypeTest()
    {
      var result = Query<DiscontinuedProduct>.All.OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeIntermediateTest()
    {
      Query<Product>.All
        .OfType<IntermediateProduct>()
        .Count();
    }

    [Test]
    public void OfTypeWithFieldAccessTest()
    {
      var result = Query<Product>.All
        .OfType<ActiveProduct>()
        .Select(ip=>ip.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeCountTest()
    {
      int productCount = Query<Product>.All.Count();
      int intermediateProductCount = Query<IntermediateProduct>.All.Count();
      int discontinuedProductCount = Query<DiscontinuedProduct>.All.Count();
      int activeProductCount = Query<ActiveProduct>.All.Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Query<Product>.All
          .OfType<IntermediateProduct>()
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Query<Product>.All
          .OfType<DiscontinuedProduct>()
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Query<Product>.All
          .OfType<ActiveProduct>()
          .Count());

      Assert.AreEqual(
        productCount,
        Query<Product>.All
          .OfType<Product>()
          .Count());
    }

    [Test]
    public void CastSimpleTest()
    {
      var discontinuedProducts = Query<DiscontinuedProduct>.All.Cast<Product>();
      AssertEx.ThrowsNotSupportedException(() => QueryDumper.Dump(discontinuedProducts));
    }


    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void CastCountTest()
    {
      var discontinuedProductCount1 = Query<DiscontinuedProduct>.All.Count();
      var discontinuedProductCount2 = Query<DiscontinuedProduct>.All.Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(discontinuedProductCount1, discontinuedProductCount2);

      var activeProductCount1 = Query<ActiveProduct>.All.Count();
      var activeProductCount2 = Query<ActiveProduct>.All.Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(activeProductCount1, activeProductCount2);

      var productCount1 = Query<Product>.All.Count();
      var productCount2 = Query<Product>.All.Cast<Product>().Count();
      Assert.AreEqual(productCount1, productCount2);
    }

    [Test]
    public void OfTypeGetFieldTest()
    {
      var result = Query<Product>.All
        .OfType<DiscontinuedProduct>()
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IsGetParentFieldTest()
    {
      var result = Query<Product>.All
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.ProductName);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IsGetChildFieldTest()
    {
      var result = Query<Product>.All
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void CastToBaseTest()
    {
      var result = Query<DiscontinuedProduct>.All
        .Select(x => (Product) x);
      QueryDumper.Dump(result);
    }


    [Test]
    public void IsBoolResultTest()
    {
      var result = Query<Product>.All
        .Select(x => x is DiscontinuedProduct
          ? x
          : null);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousCastTest()
    {
      var result = Query<Product>.All
        .Select(x =>
          new
          {
            DiscontinuedProduct = x as DiscontinuedProduct,
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void TwoChildrenCastTest()
    {
      var result = Query<Product>.All
        .Select(x =>
          new
          {
            DiscontinuedProduct = x as DiscontinuedProduct,
            ActiveProduct = x as ActiveProduct
          });
      QueryDumper.Dump(result);
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void ComplexIsCastTest()
    {
      var result = Query<Product>.All
        .Select(x =>
          new
          {
            DiscontinuedProduct = x is DiscontinuedProduct
              ? (DiscontinuedProduct) x
              : null,
            ActiveProduct = x is ActiveProduct
              ? (ActiveProduct) x
              : null
          })
        .Select(x =>
          new
          {
            AQ = x.ActiveProduct==null
              ? "NULL"
              : x.ActiveProduct.QuantityPerUnit,
            DQ = x.DiscontinuedProduct==null
              ? "NULL"
              : x.DiscontinuedProduct.QuantityPerUnit
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexAsCastTest()
    {
      var result = Query<Product>.All
        .Select(product =>
          new
          {
            DiscontinuedProduct = product as DiscontinuedProduct,
            ActiveProduct = product as ActiveProduct})
        .Select(anonymousArgument =>
          new
          {
            AQ = anonymousArgument.ActiveProduct == null
              ? "NULL"
              : anonymousArgument.ActiveProduct.QuantityPerUnit,
            DQ = anonymousArgument.DiscontinuedProduct == null
              ? "NULL"
              : anonymousArgument.DiscontinuedProduct.QuantityPerUnit
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexAsCast2Test()
    {
      var result = Query<Product>.All
        .Select(product =>
          new
          {
            DiscontinuedProduct = product,
            ActiveProduct = product})
        .Select(anonymousArgument =>
          new
          {
            AQ = anonymousArgument.ActiveProduct as ActiveProduct,
            DQ = anonymousArgument.DiscontinuedProduct as DiscontinuedProduct
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsDowncastTest()
    {
      var result = Query<Product>.All
        .Select(product => product as DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsDowncastWithFieldSelectTest()
    {
      var result = Query<Product>.All
        .Select(product => product as DiscontinuedProduct)
        .Select(discontinuedProduct => 
          discontinuedProduct == null 
          ? "NULL" 
          : discontinuedProduct.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastTest()
    {
      var result = Query<DiscontinuedProduct>.All
        .Select(discontinuedProduct => discontinuedProduct as Product);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastWithFieldSelectTest()
    {
      var result = Query<DiscontinuedProduct>.All
        .Select(discontinuedProduct => discontinuedProduct as Product)
        .Select(product => 
          product == null 
          ? "NULL" 
          : product.ProductName);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastWithConditionalTest()
    {
      var result = Query<DiscontinuedProduct>.All
        .Select(discontinuedProduct => discontinuedProduct as Product)
        .Select(product => 
          product == null 
          ? null 
          : product);
      QueryDumper.Dump(result);
    }

    [Test]
    public void WrongCastTest()
    {
      var result = Query<DiscontinuedProduct>.All
        .Select(discontinuedProduct => discontinuedProduct as Product)
        .Select(product => product as ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsSimpleTest()
    {
      var result = Query<OrderDetails>.All
        .Select(orderDetails => orderDetails.Product as ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsSimpleTest()
    {
      var result = Query<OrderDetails>.All
        .Where(orderDetails => orderDetails.Product is ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeSimpleTest()
    {
      var result = Query<OrderDetails>.All
        .Select(orderDetails => orderDetails.Product)
        .OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeWithFieldTest()
    {
      var result = Query<OrderDetails>.All
        .Select(orderDetails => orderDetails.Product)
        .OfType<DiscontinuedProduct>()
        .Select(dp=>dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsAnonymousTest()
    {
      var result = Query<OrderDetails>.All
        .Select(orderDetails => new {Product = orderDetails.Product as ActiveProduct});
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsAnonymousTest()
    {
      var result = Query<OrderDetails>.All
        .Where(orderDetails => orderDetails.Product is ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeAnonymousTest()
    {
      var result = Query<OrderDetails>.All
        .Select(orderDetails => new{orderDetails.Product})
        .Select(p=>p.Product)
        .OfType<ActiveProduct>();
      QueryDumper.Dump(result);
    }
    
    [Test]
    public void ReferenceOfTypeAnonymousWithFieldAccessTest()
    {
      var result = Query<OrderDetails>.All
        .Select(orderDetails => new{orderDetails.Product})
        .Select(p=>p.Product)
        .OfType<ActiveProduct>()
        .Select(ap=>ap.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ImplicitNumericTest()
    {
      long value = 0;
      var customer = Query<Customer>.All.OrderBy(c => c.CompanyName).First();
      var result = Query<Customer>.All
        .Where(c => c==customer)
        .Select(c => c.CompanyName.Length + value)
        .First();
      Assert.AreEqual(customer.CompanyName.Length, result);
    }
  }
}