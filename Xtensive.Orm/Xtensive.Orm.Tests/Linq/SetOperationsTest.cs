// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.06

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [Serializable]
  public class SetOperationsTest : NorthwindDOModelTest
  {
    [Test]
    public void SimpleConcatTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Session.Query.All<Customer>();;
      var result = customers.Where(c => c.Orders.Count <= 1).Concat(Session.Query.All<Customer>().Where(c => c.Orders.Count > 1));
      QueryDumper.Dump(result);
      Assert.AreEqual(customers.Count(), result.Count());
    }

    [Test]
    public void SimpleUnionTest()
    {
      var products = Session.Query.All<Product>();
      var customers = Session.Query.All<Customer>();
      var productFirstChars =
          from p in products
          select p.ProductName.Substring(0, 1);
      var customerFirstChars =
          from c in customers
          select c.CompanyName.Substring(0, 1);
      var uniqueFirstChars = productFirstChars.Union(customerFirstChars);
      QueryDumper.Dump(uniqueFirstChars);
      
    }

    [Test]
    public void IntersectTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var products = Session.Query.All<Product>();
      var customers = Session.Query.All<Customer>();
      var productFirstChars =
          from p in products
          select p.ProductName.Substring(0, 1);
      var customerFirstChars =
          from c in customers
          select c.CompanyName.Substring(0, 1);
      var commonFirstChars = productFirstChars.Intersect(customerFirstChars);
      QueryDumper.Dump(commonFirstChars);
      
    }

    [Test]
    public void SimpleIntersectTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Session.Query.All<Order>()
        .Select(o => o.Employee)
        .Intersect(Session.Query.All<Order>().Select(o => o.Employee));

      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleExceptTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var products = Session.Query.All<Product>();
      var customers = Session.Query.All<Customer>();
      var productFirstChars =
          from p in products
          select p.ProductName.Substring(0, 1);
      var customerFirstChars =
          from c in customers
          select c.CompanyName.Substring(0, 1);
      var productOnlyFirstChars = productFirstChars.Except(customerFirstChars);
      QueryDumper.Dump(productOnlyFirstChars);
    }

    [Test]
    public void ConcatDifferentTest()
    {
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
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
      QueryDumper.Dump(result);
    }

    [Test]
    public void ConcatDifferentTest2()
    {
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = (
               from c in customers
               select new { Name = c.CompanyName, c.Phone }
              ).Concat(
               from e in employees
               select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone }
              );
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionDifferentTest()
    {
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = (
               from c in customers
               select c.Address.Country
              ).Union(
               from e in employees
               select e.Address.Country
              );
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectDifferentTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = (
               from c in customers
               select c.Address.Country
              ).Intersect(
               from e in employees
               select e.Address.Country
              );
      QueryDumper.Dump(result);
    }

    [Test]
    public void ExceptDifferentTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = (
               from c in customers
               select c.Address.Country
              ).Except(
               from e in employees
               select e.Address.Country
              );
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymousTest()
    {
      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => new {c.CompanyName, c.ContactName})
        .Take(10)
        .Union(customers.Select(c => new {c.CompanyName, c.ContactName}));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous2Test()
    {
      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => new { c.CompanyName, c.ContactName, c.Address })
        .Where(c => c.Address.StreetAddress.Length < 10)
        .Select(c => new {c.CompanyName, c.Address.City})
        .Take(10)
        .Union(customers.Select(c => new { c.CompanyName, c.Address.City})).Where(c=>c.CompanyName.Length < 10);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous3Test()
    {
      var customers = Session.Query.All<Customer>();
      var shipper = Session.Query.All<Shipper>();
      var result = customers.Select(c => new { c.CompanyName, c.ContactName, c.Address })
        .Where(c => c.Address.StreetAddress.Length < 15)
        .Select(c => new { Name = c.CompanyName, Address = c.Address.City })
        .Take(10)
        .Union(shipper.Select(s => new { Name = s.CompanyName, Address = s.Phone }))
        .Where(c=>c.Address.Length < 7);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionStructureTest()
    {
      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => c.Address)
        .Where(c => c.StreetAddress.Length > 0)
        .Union(customers.Select(c => c.Address))
        .Where(c => c.Region == "BC");
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectWithoutOneOfSelect()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var actual = from c in Session.Query.All<Customer>()
      from r in (c.Orders)
        .Intersect(c.Orders).Select(o => o.ShippedDate)
      orderby r
      select r;
      var expected = from c in Session.Query.All<Customer>().ToList()
      from r in (c.Orders)
        .Intersect(c.Orders).Select(o => o.ShippedDate)
      orderby r
      select r;
    }
  }
}