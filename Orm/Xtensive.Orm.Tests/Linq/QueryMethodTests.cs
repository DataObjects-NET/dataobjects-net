// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.15

using System.Linq;
using NUnit.Framework;
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
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c==Session.Query.Single(Session.Query.All<Customer>().FirstOrDefault().Key));
      Assert.Throws<QueryTranslationException>(() => Assert.AreEqual(0, expected.Except(query).Count()));
    }

    [Test]
    public void SingleSubqueryKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.Single<Customer>(Session.Query.All<Customer>().FirstOrDefault().Key));
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c==Session.Query.Single<Customer>(Session.Query.All<Customer>().FirstOrDefault().Key));

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleSubqueryTupleTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c == Session.Query.Single<Customer>(Session.Query.All<Customer>().FirstOrDefault().CustomerId));
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c == Session.Query.Single<Customer>(Session.Query.All<Customer>().FirstOrDefault().CustomerId));
      Assert.Throws<QueryTranslationException>(() => Assert.AreEqual(0, expected.Except(query).Count()));
    }

    [Test]
    public void SingleOrDefaultParameterTest()
    {
      var key = Session.Query.All<Customer>().First().Key;
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault(key));
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c==Session.Query.SingleOrDefault(key));

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleOrDefaultSubqueryNonGenericTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault(Session.Query.All<Customer>().FirstOrDefault().Key));
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c==Session.Query.SingleOrDefault(Session.Query.All<Customer>().FirstOrDefault().Key));
      Assert.Throws<QueryTranslationException>(() => Assert.AreEqual(0, expected.Except(query).Count()));
    }

    [Test]
    public void SingleOrDefaultSubqueryKeyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault<Customer>(Session.Query.All<Customer>().FirstOrDefault().Key));
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c==Session.Query.SingleOrDefault<Customer>(Session.Query.All<Customer>().FirstOrDefault().Key));

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SingleOrDefaultSubqueryTupleTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c==Session.Query.SingleOrDefault<Customer>(Session.Query.All<Customer>().FirstOrDefault().CustomerId));
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Where(c => c==Session.Query.SingleOrDefault<Customer>(Session.Query.All<Customer>().FirstOrDefault().CustomerId));
      Assert.Throws<QueryTranslationException>(() => Assert.AreEqual(0, expected.Except(query).Count()));
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
    public void TraceTest()
    {
      Session.Events.DbCommandExecuting += EventsOnDbCommandExecuting;
      var subquery = CreateQuery1();
      var query = CreateQuery2()
        .Where(c => c==Session.Query.Single<Customer>(subquery.FirstOrDefault().Key));

      Assert.IsNotEmpty(query.ToArray());
    }

    private void EventsOnDbCommandExecuting(object sender, DbCommandEventArgs e)
    {
      var data = e.TraceData;
      if (data == null) {
        return;
      }
      var comment = $"-- {data.CallerMemberName} at {data.CallerFilePath}, line {data.CallerLineNumber}\n";

      var command = e.Command;
      command.CommandText = comment + command.CommandText;
    }

    private IQueryable<Customer> CreateQuery1() => Session.Query.All<Customer>().Trace();

    private IQueryable<Customer> CreateQuery2() => Session.Query.All<Customer>().Trace();
  }
}