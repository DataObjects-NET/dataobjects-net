// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.15

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class QueryMethodTests : NorthwindDOModelTest
  {
    [Test]
    public void SingleParameterTest()
    {
      var key = Query.All<Customer>().First().Key;
      var query = Query.All<Customer>().Where(c => c==Query.Single(key));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.Single(key));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void SingleSubqueryNonGenericTest()
    {
      var query = Query.All<Customer>().Where(c => c==Query.Single(Query.All<Customer>().FirstOrDefault().Key));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.Single(Query.All<Customer>().FirstOrDefault().Key));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleSubqueryKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Query.All<Customer>().Where(c => c==Query.Single<Customer>(Query.All<Customer>().FirstOrDefault().Key));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.Single<Customer>(Query.All<Customer>().FirstOrDefault().Key));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void SingleSubqueryTupleTest()
    {
      var query = Query.All<Customer>().Where(c => c==Query.Single<Customer>(Query.All<Customer>().FirstOrDefault().Id));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.Single<Customer>(Query.All<Customer>().FirstOrDefault().Id));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleOrDefaultParameterTest()
    {
      var key = Query.All<Customer>().First().Key;
      var query = Query.All<Customer>().Where(c => c==Query.SingleOrDefault(key));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.SingleOrDefault(key));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void SingleOrDefaultSubqueryNonGenericTest()
    {
      var query = Query.All<Customer>().Where(c => c==Query.SingleOrDefault(Query.All<Customer>().FirstOrDefault().Key));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.SingleOrDefault(Query.All<Customer>().FirstOrDefault().Key));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleOrDefaultSubqueryKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Query.All<Customer>().Where(c => c==Query.SingleOrDefault<Customer>(Query.All<Customer>().FirstOrDefault().Key));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.SingleOrDefault<Customer>(Query.All<Customer>().FirstOrDefault().Key));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void SingleOrDefaultSubqueryTupleTest()
    {
      var query = Query.All<Customer>().Where(c => c==Query.SingleOrDefault<Customer>(Query.All<Customer>().FirstOrDefault().Id));
      var expected = Query.All<Customer>().AsEnumerable().Where(c => c==Query.SingleOrDefault<Customer>(Query.All<Customer>().FirstOrDefault().Id));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void Store1Test()
    {
      var localCustomers = Query.All<Customer>().Take(10).ToList();
      var query = Query.All<Customer>().Join(Query.Store(localCustomers), customer => customer, localCustomer => localCustomer, (customer, localCustomer) => new {customer, localCustomer});
      var expected = Query.All<Customer>().AsEnumerable().Join(Query.Store(localCustomers), customer => customer, localCustomer => localCustomer, (customer, localCustomer) => new {customer, localCustomer});
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void Store2Test()
    {
      var query = Query.All<Customer>().Join(Query.Store(Query.All<Customer>().Take(10)), customer => customer, localCustomer => localCustomer, (customer, localCustomer) => new {customer, localCustomer});
      var expected = Query.All<Customer>().AsEnumerable().Join(Query.Store(Query.All<Customer>().Take(10)), customer => customer, localCustomer => localCustomer, (customer, localCustomer) => new {customer, localCustomer});
      Assert.AreEqual(0, expected.Except(query).Count());
    }
  }
}