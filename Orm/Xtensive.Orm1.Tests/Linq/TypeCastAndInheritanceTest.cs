// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture, Category("Linq")]
  public class TypeCastAndInheritanceTest : NorthwindDOModelTest
  {
    [Test]
    public void InheritanceCountTest()
    {
      var productCount = Session.Query.All<Product>().Count();
      var discontinuedProductCount = Session.Query.All<DiscontinuedProduct>().Count();
      var activeProductCount = Session.Query.All<ActiveProduct>().Count();
      Assert.IsTrue(productCount > 0);
      Assert.IsTrue(discontinuedProductCount > 0);
      Assert.IsTrue(activeProductCount > 0);
      Assert.AreEqual(productCount, discontinuedProductCount, activeProductCount);
    }

    [Test]
    public void IsSimpleTest()
    {
      var result = Session.Query.All<Product>().Where(p => p is DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSameTypeTest()
    {
#pragma warning disable 183
      var result = Session.Query.All<Product>().Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSubTypeTest()
    {
#pragma warning disable 183
      var result = Session.Query.All<DiscontinuedProduct>().Where(p => p is Product);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void IsIntermediateTest()
    {
      Session.Query.All<Product>()
        .Where(p => p is IntermediateProduct)
        .Select(product => (IntermediateProduct) product)
        .Count();
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void IsCountTest()
    {
      int productCount = Session.Query.All<Product>().Count();
      int intermediateProductCount = Session.Query.All<IntermediateProduct>().Count();
      int discontinuedProductCount = Session.Query.All<DiscontinuedProduct>().Count();
      int activeProductCount = Session.Query.All<ActiveProduct>().Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Session.Query.All<Product>()
          .Where(p => p is IntermediateProduct)
          .Select(product => (IntermediateProduct) product)
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Session.Query.All<Product>()
          .Where(p => p is DiscontinuedProduct)
          .Select(product => (DiscontinuedProduct) product)
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Session.Query.All<Product>()
          .Where(p => p is ActiveProduct)
          .Select(product => (ActiveProduct) product)
          .Count());

#pragma warning disable 183
      Assert.AreEqual(
        productCount,
        Session.Query.All<Product>()
          .Where(p => p is Product)
          .Count());
#pragma warning restore 183
    }

    [Test]
    public void OfTypeSimpleTest()
    {
      var result = Session.Query.All<Product>().OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSameTypeTest()
    {
      var result = Session.Query.All<Product>().OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSubTypeTest()
    {
      var result = Session.Query.All<DiscontinuedProduct>().OfType<Product>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeIntermediateTest()
    {
      Session.Query.All<Product>()
        .OfType<IntermediateProduct>()
        .Count();
    }

    [Test]
    public void OfTypeWithFieldAccessTest()
    {
      var result = Session.Query.All<Product>()
        .OfType<ActiveProduct>()
        .Select(ip=>ip.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeCountTest()
    {
      int productCount = Session.Query.All<Product>().Count();
      int intermediateProductCount = Session.Query.All<IntermediateProduct>().Count();
      int discontinuedProductCount = Session.Query.All<DiscontinuedProduct>().Count();
      int activeProductCount = Session.Query.All<ActiveProduct>().Count();

      Assert.Greater(productCount, 0);
      Assert.Greater(intermediateProductCount, 0);
      Assert.Greater(discontinuedProductCount, 0);
      Assert.Greater(activeProductCount, 0);

      Assert.AreEqual(
        productCount,
        intermediateProductCount);

      Assert.AreEqual(
        intermediateProductCount,
        Session.Query.All<Product>()
          .OfType<IntermediateProduct>()
          .Count());

      Assert.AreEqual(
        discontinuedProductCount,
        Session.Query.All<Product>()
          .OfType<DiscontinuedProduct>()
          .Count());

      Assert.AreEqual(
        activeProductCount,
        Session.Query.All<Product>()
          .OfType<ActiveProduct>()
          .Count());

      Assert.AreEqual(
        productCount,
        Session.Query.All<Product>()
          .OfType<Product>()
          .Count());
    }

    [Test]
    public void CastSimpleTest()
    {
      var discontinuedProducts = Session.Query.All<DiscontinuedProduct>().Cast<Product>();
      var list = discontinuedProducts.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void CastCountTest()
    {
      var discontinuedProductCount1 = Session.Query.All<DiscontinuedProduct>().Count();
      var discontinuedProductCount2 = Session.Query.All<DiscontinuedProduct>().Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(discontinuedProductCount1, discontinuedProductCount2);

      var activeProductCount1 = Session.Query.All<ActiveProduct>().Count();
      var activeProductCount2 = Session.Query.All<ActiveProduct>().Cast<Product>().Where(product => product!=null).Count();
      Assert.AreEqual(activeProductCount1, activeProductCount2);

      var productCount1 = Session.Query.All<Product>().Count();
      var productCount2 = Session.Query.All<Product>().Cast<Product>().Count();
      Assert.AreEqual(productCount1, productCount2);
    }

    [Test]
    public void OfTypeGetFieldTest()
    {
      var result = Session.Query.All<Product>()
        .OfType<DiscontinuedProduct>()
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void IsGetParentFieldTest()
    {
      var result = Session.Query.All<Product>()
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.ProductName);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void IsGetChildFieldTest()
    {
      var result = Session.Query.All<Product>()
        .Where(p => p is DiscontinuedProduct)
        .Select(x => (DiscontinuedProduct) x)
        .Select(dp => dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void CastToBaseTest()
    {
      var result = Session.Query.All<DiscontinuedProduct>()
        .Select(x => (Product) x);
      QueryDumper.Dump(result);
    }


    [Test]
    public void IsBoolResultTest()
    {
      var result = Session.Query.All<Product>()
        .Select(x => x is DiscontinuedProduct
          ? x
          : null);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousCastTest()
    {
      var result = Session.Query.All<Product>()
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
      var result = Session.Query.All<Product>()
        .Select(x =>
          new
          {
            DiscontinuedProduct = x as DiscontinuedProduct,
            ActiveProduct = x as ActiveProduct
          });
      QueryDumper.Dump(result);
    }


    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void ComplexIsCastTest()
    {
      var result = Session.Query.All<Product>()
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
      var result = Session.Query.All<Product>()
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
      var result = Session.Query.All<Product>()
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
      var result = Session.Query.All<Product>()
        .Select(product => product as DiscontinuedProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsDowncastWithFieldSelectTest()
    {
      var result = Session.Query.All<Product>()
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
      var result = Session.Query.All<DiscontinuedProduct>()
        .Select(discontinuedProduct => discontinuedProduct as Product);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastWithFieldSelectTest()
    {
      var result = Session.Query.All<DiscontinuedProduct>()
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
      var result = Session.Query.All<DiscontinuedProduct>()
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
      var result = Session.Query.All<DiscontinuedProduct>()
        .Select(discontinuedProduct => discontinuedProduct as Product)
        .Select(product => product as ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsSimpleTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Select(orderDetails => orderDetails.Product as ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsSimpleTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Where(orderDetails => orderDetails.Product is ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeSimpleTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Select(orderDetails => orderDetails.Product)
        .OfType<DiscontinuedProduct>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeWithFieldTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Select(orderDetails => orderDetails.Product)
        .OfType<DiscontinuedProduct>()
        .Select(dp=>dp.QuantityPerUnit);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsAnonymousTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Select(orderDetails => new {Product = orderDetails.Product as ActiveProduct});
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsAnonymousTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Where(orderDetails => orderDetails.Product is ActiveProduct);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeAnonymousTest()
    {
      var result = Session.Query.All<OrderDetails>()
        .Select(orderDetails => new{orderDetails.Product})
        .Select(p=>p.Product)
        .OfType<ActiveProduct>();
      QueryDumper.Dump(result);
    }
    
    [Test]
    public void ReferenceOfTypeAnonymousWithFieldAccessTest()
    {
      var result = Session.Query.All<OrderDetails>()
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
      var customer = Session.Query.All<Customer>().OrderBy(c => c.CompanyName).First();
      var result = Session.Query.All<Customer>()
        .Where(c => c==customer)
        .Select(c => c.CompanyName.Length + value)
        .First();
      Assert.AreEqual(customer.CompanyName.Length, result);
    }
  }
}