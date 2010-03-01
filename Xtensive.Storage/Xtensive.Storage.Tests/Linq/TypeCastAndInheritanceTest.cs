// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture, Category("Linq")]
  public class TypeCastAndInheritanceTest : NorthwindDOModelTest
  {
    [Test]
    public void InheritanceCountTest()
    {
      var productCount = Query.All<Product>().Count();
      var discontinuedProductCount = Query.All<DiscontinuedProduct>().Count();
      var activeProductCount = Query.All<ActiveProduct>().Count();
      Assert.IsTrue(productCount > 0);
      Assert.IsTrue(discontinuedProductCount > 0);
      Assert.IsTrue(activeProductCount > 0);
      Assert.AreEqual(productCount, discontinuedProductCount, activeProductCount);
    }

    [Test]
    public void IsSimpleTest()
    {
      var result = Query.All<Product>().Where(p => p is DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSameTypeTest()
    {
#pragma warning disable 183
      var result = Query.All<Product>().Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSubTypeTest()
    {
#pragma warning disable 183
      var result = Query.All<DiscontinuedProduct>().Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void IsIntermediateTest()
    {
      Query.All<Product>()
        .Where(p => p is IntermediateProduct)
        .Select(product => (IntermediateProduct) product)
        .Count();
    }

    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void IsCountTest()
    {
      int productCount = Query.All<Product>().Count();
      int intermediateProductCount = Query.All<IntermediateProduct>().Count();
      int discontinuedProductCount = Query.All<DiscontinuedProduct>().Count();
      int activeProductCount = Query.All<ActiveProduct>().Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Query.All<Product>()
          .Where(p => p is IntermediateProduct)
          .Select(product => (IntermediateProduct) product)
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Query.All<Product>()
          .Where(p => p is DiscontinuedProduct)
          .Select(product => (DiscontinuedProduct) product)
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Query.All<Product>()
          .Where(p => p is ActiveProduct)
          .Select(product => (ActiveProduct) product)
          .Count());

#pragma warning disable 183
      Assert.AreEqual(
        productCount,
        Query.All<Product>()
          .Where(p => p is Product)
          .Count());
#pragma warning restore 183
    }

    [Test]
    public void OfTypeSimpleTest()
    {
      var result = Query.All<Product>().OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSameTypeTest()
    {
      var result = Query.All<Product>().OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSubTypeTest()
    {
      var result = Query.All<DiscontinuedProduct>().OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeIntermediateTest()
    {
      Query.All<Product>()
        .OfType<IntermediateProduct>()
        .Count();
    }

    [Test]
    public void OfTypeWithFieldAccessTest()
    {
      var result = Query.All<Product>()
        .OfType<ActiveProduct>()
        .Select(ip=>ip.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeCountTest()
    {
      int productCount = Query.All<Product>().Count();
      int intermediateProductCount = Query.All<IntermediateProduct>().Count();
      int discontinuedProductCount = Query.All<DiscontinuedProduct>().Count();
      int activeProductCount = Query.All<ActiveProduct>().Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Query.All<Product>()
          .OfType<IntermediateProduct>()
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Query.All<Product>()
          .OfType<DiscontinuedProduct>()
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Query.All<Product>()
          .OfType<ActiveProduct>()
          .Count());

      Assert.AreEqual(
        productCount,
        Query.All<Product>()
          .OfType<Product>()
          .Count());
    }

    [Test]
    public void CastSimpleTest()
    {
      var discontinuedProducts = Query.All<DiscontinuedProduct>().Cast<Product>();
      AssertEx.Throws<TranslationException>(() => QueryDumper.Dump(discontinuedProducts));
    }


    [Test]
    [ExpectedException(typeof (TranslationException))]
    public void CastCountTest()
    {
      var discontinuedProductCount1 = Query.All<DiscontinuedProduct>().Count();
      var discontinuedProductCount2 = Query.All<DiscontinuedProduct>().Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(discontinuedProductCount1, discontinuedProductCount2);

      var activeProductCount1 = Query.All<ActiveProduct>().Count();
      var activeProductCount2 = Query.All<ActiveProduct>().Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(activeProductCount1, activeProductCount2);

      var productCount1 = Query.All<Product>().Count();
      var productCount2 = Query.All<Product>().Cast<Product>().Count();
      Assert.AreEqual(productCount1, productCount2);
    }

    [Test]
    public void OfTypeGetFieldTest()
    {
      var result = Query.All<Product>()
        .OfType<DiscontinuedProduct>()
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void IsGetParentFieldTest()
    {
      var result = Query.All<Product>()
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.ProductName);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void IsGetChildFieldTest()
    {
      var result = Query.All<Product>()
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void CastToBaseTest()
    {
      var result = Query.All<DiscontinuedProduct>()
        .Select(x => (Product) x);
      QueryDumper.Dump(result);
    }


    [Test]
    public void IsBoolResultTest()
    {
      var result = Query.All<Product>()
        .Select(x => x is DiscontinuedProduct
          ? x
          : null);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousCastTest()
    {
      var result = Query.All<Product>()
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
      var result = Query.All<Product>()
        .Select(x =>
          new
          {
            DiscontinuedProduct = x as DiscontinuedProduct,
            ActiveProduct = x as ActiveProduct
          });
      QueryDumper.Dump(result);
    }


    [Test]
    [ExpectedException(typeof(TranslationException))]
    public void ComplexIsCastTest()
    {
      var result = Query.All<Product>()
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
      var result = Query.All<Product>()
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
      var result = Query.All<Product>()
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
      var result = Query.All<Product>()
        .Select(product => product as DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsDowncastWithFieldSelectTest()
    {
      var result = Query.All<Product>()
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
      var result = Query.All<DiscontinuedProduct>()
        .Select(discontinuedProduct => discontinuedProduct as Product);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastWithFieldSelectTest()
    {
      var result = Query.All<DiscontinuedProduct>()
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
      var result = Query.All<DiscontinuedProduct>()
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
      var result = Query.All<DiscontinuedProduct>()
        .Select(discontinuedProduct => discontinuedProduct as Product)
        .Select(product => product as ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsSimpleTest()
    {
      var result = Query.All<OrderDetails>()
        .Select(orderDetails => orderDetails.Product as ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsSimpleTest()
    {
      var result = Query.All<OrderDetails>()
        .Where(orderDetails => orderDetails.Product is ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeSimpleTest()
    {
      var result = Query.All<OrderDetails>()
        .Select(orderDetails => orderDetails.Product)
        .OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeWithFieldTest()
    {
      var result = Query.All<OrderDetails>()
        .Select(orderDetails => orderDetails.Product)
        .OfType<DiscontinuedProduct>()
        .Select(dp=>dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsAnonymousTest()
    {
      var result = Query.All<OrderDetails>()
        .Select(orderDetails => new {Product = orderDetails.Product as ActiveProduct});
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsAnonymousTest()
    {
      var result = Query.All<OrderDetails>()
        .Where(orderDetails => orderDetails.Product is ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeAnonymousTest()
    {
      var result = Query.All<OrderDetails>()
        .Select(orderDetails => new{orderDetails.Product})
        .Select(p=>p.Product)
        .OfType<ActiveProduct>();
      QueryDumper.Dump(result);
    }
    
    [Test]
    public void ReferenceOfTypeAnonymousWithFieldAccessTest()
    {
      var result = Query.All<OrderDetails>()
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
      var customer = Query.All<Customer>().OrderBy(c => c.CompanyName).First();
      var result = Query.All<Customer>()
        .Where(c => c==customer)
        .Select(c => c.CompanyName.Length + value)
        .First();
      Assert.AreEqual(customer.CompanyName.Length, result);
    }
  }
}