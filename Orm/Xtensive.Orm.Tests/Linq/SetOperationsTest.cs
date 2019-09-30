// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.06

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [Serializable]
  public class SetOperationsTest : ChinookDOModelTest
  {
    [Test]
    public void SimpleConcatTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var customers = Session.Query.All<Customer>();;
      var result = customers.Where(c => c.Invoices.Count <= 1).Concat(Session.Query.All<Customer>().Where(c => c.Invoices.Count > 1));
      QueryDumper.Dump(result);
      Assert.AreEqual(customers.Count(), result.Count());
    }

    [Test]
    public void SimpleUnionTest()
    {
      var products = Session.Query.All<Track>();
      var customers = Session.Query.All<Customer>();
      var productFirstChars =
          from p in products
          select p.Name.Substring(0, 1);
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
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var products = Session.Query.All<Track>();
      var customers = Session.Query.All<Customer>();
      var productFirstChars =
          from p in products
          select p.Name.Substring(0, 1);
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
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var query = Session.Query.All<Invoice>()
        .Select(o => o.DesignatedEmployee)
        .Intersect(Session.Query.All<Invoice>().Select(o => o.DesignatedEmployee));

      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleExceptTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
      var products = Session.Query.All<Track>();
      var customers = Session.Query.All<Customer>();
      var productFirstChars =
          from p in products
          select p.Name.Substring(0, 1);
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
               select e.Email
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
               select new { Name = e.FirstName + " " + e.LastName, Phone = e.Phone }
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
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
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
      Require.ProviderIsNot(StorageProvider.Firebird);
      Require.ProviderIsNot(StorageProvider.MySql);
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
      // SQLite does not support paging operations inside set operations
      Require.ProviderIsNot(StorageProvider.Sqlite);

      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => new {Company = c.CompanyName, c.LastName})
        .Take(10)
        .Union(customers.Select(c => new {Company = c.CompanyName, c.LastName}));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous2Test()
    {
      // SQLite does not support paging operations inside set operations
      Require.ProviderIsNot(StorageProvider.Sqlite);

      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => new { Company = c.CompanyName, c.LastName, c.Address })
        .Where(c => c.Address.StreetAddress.Length < 10)
        .Select(c => new {c.Company, c.Address.City})
        .Take(10)
        .Union(customers.Select(c => new { Company = c.CompanyName, c.Address.City})).Where(c=>c.Company.Length < 10);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous3Test()
    {
      // SQLite does not support paging operations inside set operations
      Require.ProviderIsNot(StorageProvider.Sqlite);

      var customers = Session.Query.All<Customer>();
      var shipper = Session.Query.All<Employee>();
      var result = customers.Select(c => new { c.FirstName, c.LastName, c.Address })
        .Where(c => c.Address.StreetAddress.Length < 15)
        .Select(c => new { Name = c.FirstName, Address = c.Address.City })
        .Take(10)
        .Union(shipper.Select(s => new { Name = s.FirstName, Address = s.Phone }))
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
        .Where(c => c.State=="BC");
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectWithoutOneOfSelect()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Apply);
      var actual = from c in Session.Query.All<Customer>()
      from r in (c.Invoices)
        .Intersect(c.Invoices).Select(o => o.PaymentDate)
      orderby r
      select r;
      var expected = from c in Session.Query.All<Customer>().ToList()
      from r in (c.Invoices)
        .Intersect(c.Invoices).Select(o => o.PaymentDate)
      orderby r
      select r;
      Assert.That(expected.Except(actual), Is.Empty);
    }
  }
}