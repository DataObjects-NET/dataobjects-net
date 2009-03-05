// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class ComplexTest : NorthwindDOModelTest
  {
    [Test]
    public void CorrelatedAggregateTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var suppliers = Query<Supplier>.All;
          var result = from p in products
                       select new { Product = p, MaxID = suppliers.Where(s => s == p.Supplier).Max(s => s.Id) };
          var list = result.ToList();
          Assert.Greater(list.Count , 0);
          t.Complete();
        }
      }
    }

    [Test]
    public void CorrelatedFirstTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var orderDetails = Query<OrderDetails>.All;
          var result = from p in products
                       select new { Product = p, MaxOrder = orderDetails.OrderByDescending(od => od.UnitPrice * od.Quantity).First(od => od.Product == p).Order };
          var list = result.ToList();
          Assert.Greater(list.Count , 0);
          t.Complete();
        }
      }
    }


    [Test]
    public void CorrelatedQueryTest() 
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var suppliers = Query<Supplier>.All;
          var result = from p in products
                       select new { Product = p,  Suppliers = suppliers.Where(s => s.Id == p.Supplier.Id).Select(s => s.CompanyName) };
          var list = result.ToList();
          Assert.Greater(list.Count, 0);
          foreach (var p in list) {
            foreach (var companyName in p.Suppliers) {
              Assert.IsNotNull(companyName);
            }
          }
          t.Complete();
        }
      }
    }

    [Test]
    public void CorrelatedFilterTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result = Query<Customer>.All.Where(c => Query<Order>.All.Count(o => o.Customer == c) > 5);
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }
    
    [Test]
    public void CorrelatedOrderByTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var result =
          from c in Query<Customer>.All
          orderby Query<Order>.All.Where(o => o.Customer == c).Count()
          select c;
        var list = result.ToList();
        Assert.Greater(list.Count, 0);
        t.Complete();
      }
    }
  }
}