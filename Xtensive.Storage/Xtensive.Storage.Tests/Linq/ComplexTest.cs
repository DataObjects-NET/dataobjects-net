// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.25

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
    [Test]
    public void CorrelatedQueryTest()
    {
      var products = Query<Product>.All;
      var suppliers = Query<Supplier>.All;
      var result = from p in products
      select new {Product = p, Suppliers = suppliers.Where(s => s.Id==p.Supplier.Id).Select(s => s.CompanyName)};
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
        orderby Query<Order>.All.Where(o => o.Customer==c).Count()
        select c;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void NestedCorrelationTest()
    {
      var result =
        from c in Query<Customer>.All
        where Query<Order>.All.Where(o => o.Customer==c)
          .All(o => Query<Employee>.All.Where(e => o.Employee==e).Any(e => e.FirstName.StartsWith("A")))
        select c;
      var list = result.ToList();
      Assert.AreEqual(list.Count, 2);
    }

    [Test]
    public void GroupByWithSelectorSelectManyTest()
    {
      var result = Query<Customer>.All
        .GroupBy(c => c.Address.Country,
          (country, customers) => customers.Where(k => k.CompanyName.StartsWith(country.Substring(0, 1))))
        .SelectMany(k => k);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ParameterScopeTest()
    {
      using (new ParameterScope()) {
        Query<Customer>.All.ToList();
      }
    }

    [Test]
    public void AsEnumerableSelectDistinctTest()
    {
      var result = Query<Order>.All.AsEnumerable().Select(o => o.Employee).Distinct();
      Assert.IsNotNull(result.First());
      Assert.Greater(result.Count(), 2);
    }
  }
}