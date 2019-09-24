// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class ComplexTest : ChinookDOModelTest
  {
    private static IQueryable<Customer> GetQuery(QueryEndpoint qe, string filter)
    {
      var customers = qe.All<Customer>().Where(cn => cn.Company.StartsWith(filter));
      return customers;
    }

    [Test]
    public void CachedQueryTest()
    {
      Assert.Throws<QueryTranslationException>(() => {
        for (char c = 'A'; c <= 'Z'; c++) {
          string firstChar = c.ToString();
          var builtQuery = GetQuery(Session.Query, firstChar);
          var query = builtQuery
            .Select(customer => customer.Email);
          var cachedQuery = Session.Query
            .Execute(qe => GetQuery(Session.Query, firstChar).Select(customer => customer.Email));
          var fullQuery = Session.Query.All<Customer>()
            .Where(cn => cn.Company.StartsWith(firstChar))
            .Select(customer => customer.Company);
          Assert.IsTrue(query.ToList().SequenceEqual(fullQuery.ToList()));
          var cachedQueryList = cachedQuery.ToList();
          var fullQueryList = fullQuery.ToList();
          var condition = cachedQueryList.SequenceEqual(fullQueryList);
          Assert.IsTrue(condition);
        }
      });
    }

    [Test]
    public void SubquerySimpleTest()
    {
      var result = Session.Query.All<Track>()
        .Select(p => Session.Query.All<Album>());
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMutiple1Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(customer =>
          Session.Query.All<Invoice>()
            .Select(invoice => Session.Query.All<Invoice>()
              .Where(i => i==invoice)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple2Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(custumer =>
          Session.Query.All<Invoice>()
            .Select(i => Session.Query.All<Invoice>()
              .Where(y => y.Customer==custumer)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryMultiple3Test()
    {
      var result = Session.Query.All<Customer>()
        .Select(customer =>
          Session.Query.All<Invoice>()
            .Select(i => Session.Query.All<Invoice>()
              .Where(y => y==i && y.Customer==customer)));
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableFieldTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result = Session.Query.All<Invoice>()
        .Select(invoice => Session.Query.All<InvoiceLine>()
          .Where(p=>p.Invoice == invoice)
          .First()
          .UnitPrice);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SubqueryCalculableColumnTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Invoice>()
        .Select(invoice => Session.Query.All<InvoiceLine>()
          .Where(p=>p.Invoice==invoice)
          .Count());
      var expectedResult = Session.Query.All<Invoice>()
        .ToList()
        .Select(invoice => Session.Query.All<InvoiceLine>()
          .ToList()
          .Where(p=>p.Invoice==invoice)
          .Count());
      Assert.AreEqual(0, expectedResult.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void CorrelatedQueryTest()
    {
      var invoiceLines = Session.Query.All<InvoiceLine>();
      var invoices = Session.Query.All<Invoice>();
      var result = from l in invoiceLines
      select new {
        InvoiceLine = l,
        Addresses = invoices
          .Where(s => s.InvoiceId==l.Invoice.InvoiceId)
          .Select(s => s.BillingAddress.StreetAddress)
      };
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var p in list)
        foreach (var address in p.Addresses)
          Assert.IsNotNull(address);
    }

    [Test]
    public void CorrelatedOrderByTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var result =
        from c in Session.Query.All<Customer>()
        orderby Session.Query.All<Invoice>().Where(o => o.Customer==c).Count() , c.CustomerId
        select c;
      var expected =
        from c in Session.Query.All<Customer>().ToList()
        orderby Session.Query.All<Invoice>().ToList().Where(o => o.Customer==c).Count() , c.CustomerId
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
        from c in Session.Query.All<Customer>()
        where Session.Query.All<Invoice>()
          .Where(o => o.Customer==c)
          .All(o => Session.Query.All<Employee>()
              .Where(e => o.DesignatedEmployee==e)
              .Any(e => e.FirstName.StartsWith("A")))
        select c;
      var list = result.ToList();
      Assert.AreEqual(1, list.Count);
    }

    [Test]
    public void GroupByWithSelectorSelectManyTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .GroupBy(c => c.Track.Name,
          (trackName, invoiceLines) => invoiceLines.Where(k => k.Invoice.Customer.FirstName.Substring(0, 1)==trackName.Substring(0, 1)))
        .SelectMany(k => k);
      var expected = Session.Query.All<InvoiceLine>()
        .ToList()
        .GroupBy(c => c.Track.Name,
          (trackName, invoiceLines) => invoiceLines.Where(k => k.Invoice.Customer.FirstName.Substring(0, 1)==trackName.Substring(0, 1)))
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
      var result = Session.Query.All<Invoice>().ToList().Select(o => o.Customer).Distinct();
      Assert.IsNotNull(result.First());
      Assert.GreaterOrEqual(result.Count(), 59);
    }

    [Test]
    public void ModifiedClosuresTest()
    {
      var result = from order in Session.Query.All<Invoice>()
                      join customer in Session.Query.All<Customer>() on order.Customer equals customer into oc
                      from joinedCustomer in oc.DefaultIfEmpty()
                      select new {
                        joinedCustomer.CustomerId, 
                        joinedCustomer.Company,
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