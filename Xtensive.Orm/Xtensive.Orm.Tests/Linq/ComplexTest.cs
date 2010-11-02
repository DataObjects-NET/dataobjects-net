// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Parameters;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class ComplexTest : NorthwindDOModelTest
  {
    private static IQueryable<Customer> GetQuery(Session.QueryEndpoint qe, string filter)
    {
      var customers = qe.All<Customer>().Where(cn => cn.CompanyName.StartsWith(filter));
      return customers;
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void CachedQueryTest()
    {
      for (char c = 'A'; c <= 'Z'; c++) {
        string firstChar = c.ToString();
        var builtQuery = GetQuery(Session.Query, firstChar);
        var query = builtQuery
          .Select(customer => customer.ContactName);
        var cachedQuery = Session.Query
          .Execute(qe => GetQuery(Session.Query, firstChar).Select(customer => customer.ContactName));
        var fullQuery = Session.Query.All<Customer>()
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
      var result = Session.Query.All<Product>()
        .Select(p => Session.Query.All<Supplier>());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMutiple1Test()
    {
      var result = Session.Query.All<Supplier>()
        .Select(supplier => 
          Session.Query.All<Product>()
          .Select(product=> Session.Query.All<Product>()
            .Where(p=>p==product)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple2Test()
    {
      var result = Session.Query.All<Supplier>()
        .Select(supplier => 
          Session.Query.All<Product>()
          .Select(product=> Session.Query.All<Product>()
            .Where(p=>p.Supplier==supplier)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple3Test()
    {
      var result = Session.Query.All<Supplier>()
        .Select(supplier => 
          Session.Query.All<Product>()
          .Select(product=> Session.Query.All<Product>()
            .Where(p=>p==product && p.Supplier==supplier)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableFieldTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Supplier>()
        .Select(supplier => Session.Query.All<Product>()
          .Where(p=>p.Supplier == supplier)
          .First()
          .UnitPrice);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableColumnTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Supplier>()
        .Select(supplier => Session.Query.All<Product>()
          .Where(p=>p.Supplier == supplier)
          .Count());
      var expectedResult = Session.Query.All<Supplier>()
        .ToList()
        .Select(supplier => Session.Query.All<Product>()
          .ToList()
          .Where(p=>p.Supplier == supplier)
          .Count());
      Assert.AreEqual(0, expectedResult.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void CorrelatedQueryTest()
    {
      var products = Session.Query.All<Product>();
      var suppliers = Session.Query.All<Supplier>();
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
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result =
        from c in Session.Query.All<Customer>()
        orderby Session.Query.All<Order>().Where(o => o.Customer==c).Count() , c.Id
        select c;
      var expected =
        from c in Session.Query.All<Customer>().ToList()
        orderby Session.Query.All<Order>().ToList().Where(o => o.Customer==c).Count() , c.Id
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
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Order>()
          .Where(o => o.Customer==c)
          .All(o => Session.Query.All<Employee>()
            .Where(e => o.Employee==e
            ).Any(e => e.FirstName.StartsWith("A")))
        select c;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 2);
    }

    [Test]
    public void GroupByWithSelectorSelectManyTest()
    {
      var result = Session.Query.All<Customer>()
        .GroupBy(c => c.Address.Country,
          (country, customers) => customers.Where(k => k.CompanyName.Substring(0, 1)==country.Substring(0, 1)))
        .SelectMany(k => k);
      var expected = Session.Query.All<Customer>()
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
        Session.Query.All<Customer>().ToList();
      }
    }

    [Test]
    public void AsEnumerableSelectDistinctTest()
    {
      var result = Session.Query.All<Order>().ToList().Select(o => o.Employee).Distinct();
      Assert.IsNotNull(result.First());
      Assert.Greater(result.Count(), 2);
    }

    [Test]
    public void ModifiedClosuresTest()
    {
      var result = from order in Session.Query.All<Order>()
                      join customer in Session.Query.All<Customer>() on order.Customer equals customer into oc
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