// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2010.01.15

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class QueryMethodTests : ChinookDOModelTest
  {
    [Test]
    public void SingleParameterTest()
    {
      var key = Session.Query.All<Customer>().First().Key;
      var query = Session.Query.All<Customer>().Where(c => c==Session.Query.Single(key));
      var expected = Session.Query.All<Customer>().AsEnumerable().Where(c => c==Session.Query.Single(key));

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleSubqueryNonGenericTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.Single(Session.Query.All<Customer>().FirstOrDefault().Key));
      _ = Assert.Throws<QueryTranslationException>(() => query.Run());
    }

    [Test]
    public void SingleSubqueryKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.Single<Customer>(Session.Query.All<Customer>().FirstOrDefault().Key));
      var expected = GetExpectedCustomerAsSequence();

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleSubqueryTupleTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c == Session.Query.Single<Customer>(Session.Query.All<Customer>().FirstOrDefault().CustomerId));
      _ = Assert.Throws<QueryTranslationException>(() => query.Run());
    }

    [Test]
    public void SingleOrDefaultParameterTest()
    {
      var key = Session.Query.All<Customer>().First().Key;
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault(key));
      var expected = Customers.Where(c => c.Key == key);

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleOrDefaultSubqueryNonGenericTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault(Session.Query.All<Customer>().FirstOrDefault().Key));
      _ = Assert.Throws<QueryTranslationException>(() => query.Run());
    }

    [Test]
    public void SingleOrDefaultSubqueryKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault<Customer>(Session.Query.All<Customer>().FirstOrDefault().Key));
      var expected = GetExpectedCustomerAsSequence();

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleOrDefaultSubqueryTupleTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault<Customer>(Session.Query.All<Customer>().FirstOrDefault().CustomerId));
      _ = Assert.Throws<QueryTranslationException>(() => query.Run());
    }

    [Test]
    public void Store1Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var localCustomers = Session.Query.All<Customer>().Take(10).ToList();
      var query = Session.Query.All<Customer>()
        .Join(
          Session.Query.Store(localCustomers),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new {customer, localCustomer});
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Join(
          Session.Query.Store(localCustomers),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new {customer, localCustomer});

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public async Task Store1AsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var localCustomers = Session.Query.All<Customer>().Take(10).ToList();
      var query = (await Session.Query.All<Customer>()
        .Join(
          Session.Query.Store(localCustomers),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new { customer, localCustomer }).ExecuteAsync()).ToList();
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Join(
          Session.Query.Store(localCustomers),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new { customer, localCustomer });

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void Store2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var query = Session.Query.All<Customer>()
        .Join(
          Session.Query.Store(Session.Query.All<Customer>().Take(10)),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new {customer, localCustomer});
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Join(
          Session.Query.Store(Session.Query.All<Customer>().Take(10)),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new {customer, localCustomer});

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public async Task Store2AsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var query = (await Session.Query.All<Customer>()
        .Join(
          Session.Query.Store(Session.Query.All<Customer>().Take(10)),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new { customer, localCustomer }).ExecuteAsync()).ToList();
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Join(
          Session.Query.Store(Session.Query.All<Customer>().Take(10)),
          customer => customer,
          localCustomer => localCustomer,
          (customer, localCustomer) => new { customer, localCustomer });

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    private IEnumerable<Customer> GetExpectedCustomerAsSequence()
    {
      if (StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird)) {
        return EnumerableUtils.One(Session.Query.All<Customer>().OrderBy(c => c.CustomerId).AsEnumerable().First());
      }
      return EnumerableUtils.One(Customers.First());
    }
  }
}