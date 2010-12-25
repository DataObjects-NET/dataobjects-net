// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Linq;
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
      var customers = Query.All<Customer>().Where(cn => cn.CompanyName.StartsWith(filter));
      return customers;
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void CachedQueryTest()
    {
      for (char c = 'A'; c <= 'Z'; c++) {
        string firstChar = c.ToString();
        var builtQuery = GetQuery(firstChar);
        var query = builtQuery
          .Select(customer => customer.ContactName);
        var cachedQuery = Query
          .Execute(() => GetQuery(firstChar).Select(customer => customer.ContactName));
        var fullQuery = Query.All<Customer>()
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
      var result = Query.All<Product>()
        .Select(p => Query.All<Supplier>());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMutiple1Test()
    {
      var result = Query.All<Supplier>()
        .Select(supplier => 
          Query.All<Product>()
          .Select(product=> Query.All<Product>()
            .Where(p=>p==product)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple2Test()
    {
      var result = Query.All<Supplier>()
        .Select(supplier => 
          Query.All<Product>()
          .Select(product=> Query.All<Product>()
            .Where(p=>p.Supplier==supplier)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple3Test()
    {
      var result = Query.All<Supplier>()
        .Select(supplier => 
          Query.All<Product>()
          .Select(product=> Query.All<Product>()
            .Where(p=>p==product && p.Supplier==supplier)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableFieldTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Query.All<Supplier>()
        .Select(supplier => Query.All<Product>()
          .Where(p=>p.Supplier == supplier)
          .First()
          .UnitPrice);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableColumnTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Query.All<Supplier>()
        .Select(supplier => Query.All<Product>()
          .Where(p=>p.Supplier == supplier)
          .Count());
      var expectedResult = Query.All<Supplier>()
        .ToList()
        .Select(supplier => Query.All<Product>()
          .ToList()
          .Where(p=>p.Supplier == supplier)
          .Count());
      Assert.AreEqual(0, expectedResult.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void CorrelatedQueryTest()
    {
      var products = Query.All<Product>();
      var suppliers = Query.All<Supplier>();
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Query.All<Customer>()
        orderby Query.All<Order>().Where(o => o.Customer==c).Count() , c.Id
        select c;
      var expected =
        from c in Query.All<Customer>().ToList()
        orderby Query.All<Order>().ToList().Where(o => o.Customer==c).Count() , c.Id
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
      Require.ProviderIsNot(StorageProvider.Oracle);
      var result =
        from c in Query.All<Customer>()
        where Query.All<Order>()
          .Where(o => o.Customer==c)
          .All(o => Query.All<Employee>()
            .Where(e => o.Employee==e
            ).Any(e => e.FirstName.StartsWith("A")))
        select c;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 2);
    }

    [Test]
    public void GroupByWithSelectorSelectManyTest()
    {
      var result = Query.All<Customer>()
        .GroupBy(c => c.Address.Country,
          (country, customers) => customers.Where(k => k.CompanyName.Substring(0, 1)==country.Substring(0, 1)))
        .SelectMany(k => k);
      var expected = Query.All<Customer>()
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
        Query.All<Customer>().ToList();
      }
    }

    [Test]
    public void AsEnumerableSelectDistinctTest()
    {
      var result = Query.All<Order>().ToList().Select(o => o.Employee).Distinct();
      Assert.IsNotNull(result.First());
      Assert.Greater(result.Count(), 2);
    }

    [Test]
    public void ModifiedClosuresTest()
    {
      var result = from order in Query.All<Order>()
                      join customer in Query.All<Customer>() on order.Customer equals customer into oc
                      from joinedCustomer in oc.DefaultIfEmpty()
                      select new {
                        CustomerId = joinedCustomer.Id, 
                        joinedCustomer.CompanyName, 
                        joinedCustomer.Address.Country
                      };
      var t = result.ToList();
      string searchTerms = "U A";
      var searchCriteria = searchTerms.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
      foreach (var search in searchCriteria) {
        var searchTerm = search;
        result = result.Where(p => p.Country.Contains(searchTerm));
      }
      var ids = (from c in result select c.CustomerId).ToArray();
    }
  }
}