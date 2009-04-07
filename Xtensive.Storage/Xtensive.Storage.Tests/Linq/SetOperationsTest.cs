// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.06

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Serializable]
  public class SetOperationsTest : NorthwindDOModelTest
  {
    [Test]
    [Ignore]
    public void SimpleConcatTest()
    {
      var customers = Query<Customer>.All;;
      var result = customers.Where(c => c.Orders.Count <= 1).Concat(Query<Customer>.All.Where(c => c.Orders.Count > 1));
      Assert.IsNotNull(result.First());
      Assert.AreEqual(customers.Count(), result.Count());
    }

    [Test]
    [Ignore]
    public void SimpleUnionTest()
    {
      var products = Query<Product>.All;
      var customers = Query<Customer>.All;
      var productFirstChars =
          from p in products
          select p.ProductName[0];
      var customerFirstChars =
          from c in customers
          select c.CompanyName[0];
      var uniqueFirstChars = productFirstChars.Union(customerFirstChars);
      Assert.IsNotNull(uniqueFirstChars);
      
    }

    [Test]
    [Ignore]
    public void SimpleIntersectTest()
    {
      var products = Query<Product>.All;
      var customers = Query<Customer>.All;
      var productFirstChars =
          from p in products
          select p.ProductName[0];
      var customerFirstChars =
          from c in customers
          select c.CompanyName[0];
      var commonFirstChars = productFirstChars.Intersect(customerFirstChars);
      Assert.IsNotNull(commonFirstChars);
      
    }

    [Test]
    [Ignore]
    public void SimpleExceptTest()
    {
      var products = Query<Product>.All;
      var customers = Query<Customer>.All;
      var productFirstChars =
          from p in products
          select p.ProductName[0];
      var customerFirstChars =
          from c in customers
          select c.CompanyName[0];
      var productOnlyFirstChars = productFirstChars.Except(customerFirstChars);
      Assert.IsNotNull(productOnlyFirstChars);
    }

    [Test]
    [Ignore]
    public void ConcatDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = (
               from c in customers
               select c.Phone
              ).Concat(
               from c in customers
               select c.Fax
              ).Concat(
               from e in employees
               select e.HomePhone
              );
      Assert.IsNotNull(result.First());
    }

    [Test]
    [Ignore]
    public void ConcatDifferentTest2()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = (
               from c in customers
               select new { Name = c.CompanyName, c.Phone }
              ).Concat(
               from e in employees
               select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone }
              );

      Assert.IsNotNull(result);
    }

    [Test]
    [Ignore]
    public void UnionDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = (
               from c in customers
               select c.Address.Country
              ).Union(
               from e in employees
               select e.Address.Country
              );
      Assert.IsNotNull(result);
    }

    [Test]
    [Ignore]
    public void IntersectDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = (
               from c in customers
               select c.Address.Country
              ).Intersect(
               from e in employees
               select e.Address.Country
              );

      Assert.IsNotNull(result);
    }

    [Test]
    [Ignore]
    public void ExceptDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = (
               from c in customers
               select c.Address.Country
              ).Except(
               from e in employees
               select e.Address.Country
              );

      Assert.IsNotNull(result);
    }
  }
}