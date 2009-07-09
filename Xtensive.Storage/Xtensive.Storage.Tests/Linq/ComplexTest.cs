// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class ComplexTest : NorthwindDOModelTest
  {
    private static IQueryable<Customer> GetQuery(string filter)
    {
      var customers = Query<Customer>.All.Where(cn => cn.CompanyName.StartsWith(filter));
      return customers;
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void CachedQueryTest()
    {
      for (char c = 'A'; c <= 'Z'; c++) {
        string firstChar = c.ToString();
        var builtQuery = GetQuery(firstChar);
        var query = builtQuery
          .Select(customer => customer.ContactName);
        var cachedQuery = CachedQuery
          .Execute(() => GetQuery(firstChar).Select(customer => customer.ContactName));
        var fullQuery = Query<Customer>.All
          .Where(cn => cn.CompanyName.StartsWith(firstChar))
          .Select(customer => customer.ContactName);
        Assert.IsTrue(query.ToList().SequenceEqual(fullQuery.ToList()));
        var cachedQueryList = cachedQuery.ToList();
        var fullQueryList = fullQuery.ToList();
        var condition = cachedQueryList.SequenceEqual(fullQueryList);
        Assert.IsTrue(condition);
      }
    }


    [Test]
    public void SubquerySimpleTest()
    {
      var result = Query<Product>.All
        .Select(p => Query<Supplier>.All);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMutiple1Test()
    {
      var result = Query<Supplier>.All
        .Select(supplier => 
          Query<Product>.All
          .Select(product=> Query<Product>.All
            .Where(p=>p==product)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple2Test()
    {
      var result = Query<Supplier>.All
        .Select(supplier => 
          Query<Product>.All
          .Select(product=> Query<Product>.All
            .Where(p=>p.Supplier==supplier)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple3Test()
    {
      var result = Query<Supplier>.All
        .Select(supplier => 
          Query<Product>.All
          .Select(product=> Query<Product>.All
            .Where(p=>p==product && p.Supplier==supplier)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableFieldTest()
    {
      var result = Query<Supplier>.All
        .Select(supplier => Query<Product>.All
          .Where(p=>p.Supplier == supplier)
          .First()
          .UnitPrice);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableColumnTest()
    {
      var result = Query<Supplier>.All
        .Select(supplier => Query<Product>.All
          .Where(p=>p.Supplier == supplier)
          .Count());
      var expectedResult = Query<Supplier>.All
        .ToList()
        .Select(supplier => Query<Product>.All
          .ToList()
          .Where(p=>p.Supplier == supplier)
          .Count());
      Assert.AreEqual(0, expectedResult.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void CorrelatedQueryTest()
    {
      var products = Query<Product>.All;
      var suppliers = Query<Supplier>.All;
      var result = from p in products
      select new {
        Product = p,
        Suppliers = suppliers
          .Where(s => s.Id==p.Supplier.Id)
          .Select(s => s.CompanyName)
      };
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var p in list)
        foreach (var companyName in p.Suppliers)
          Assert.IsNotNull(companyName);
    }

    [Test]
    public void CorrelatedOrderByTest()
    {
      var result =
        from c in Query<Customer>.All
        orderby Query<Order>.All.Where(o => o.Customer==c).Count() , c.Id
        select c;
      var expected =
        from c in Query<Customer>.All.ToList()
        orderby Query<Order>.All.ToList().Where(o => o.Customer==c).Count() , c.Id
        select c;
      var resultList = result.ToList();
      var expectedList = expected.ToList();
      Assert.AreEqual(resultList.Count, expectedList.Count);
      for (int i = 0; i < resultList.Count; i++)
        Assert.AreEqual(resultList[i], expectedList[i]);
    }

    [Test]
    public void NestedCorrelationTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All
          .Where(o => o.Customer==c)
          .All(o => Query<Employee>.All
            .Where(e => o.Employee==e
            ).Any(e => e.FirstName.StartsWith("A")))
        select c;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 2);
    }

    [Test]
    public void GroupByWithSelectorSelectManyTest()
    {
      var result = Query<Customer>.All
        .GroupBy(c => c.Address.Country,
          (country, customers) => customers.Where(k => k.CompanyName.Substring(0, 1)==country.Substring(0, 1)))
        .SelectMany(k => k);
      var expected = Query<Customer>.All
        .ToList()
        .GroupBy(c => c.Address.Country,
          (country, customers) => customers.Where(k => k.CompanyName.Substring(0, 1)==country.Substring(0, 1)))
        .SelectMany(k => k);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void ParameterScopeTest()
    {
      using (new ParameterContext().Activate()) {
        Query<Customer>.All.ToList();
      }
    }

    [Test]
    public void AsEnumerableSelectDistinctTest()
    {
      var result = Query<Order>.All.ToList().Select(o => o.Employee).Distinct();
      Assert.IsNotNull(result.First());
      Assert.Greater(result.Count(), 2);
    }

    [Test]
    public void ModifiedClosuresTest()
    {
      EnsureIs(StorageProtocols.Index);
      var customers = from o in Query<Order>.All
                      join c in Query<Customer>.All on o.Customer equals c into oc
                      from x in oc.DefaultIfEmpty()
                      select new { CustomerId = x.Id, CompanyName = x.CompanyName, Country = x.Address.Country };

      string searchTerms = "U A";
      var searchCriteria = searchTerms.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var search in searchCriteria) {
        var searchTerm = search;
        customers = customers.Where(p => p.Country.Contains(searchTerm));
      }
      var ids = (from c in customers select c.CustomerId).ToArray();
    }
  }
}