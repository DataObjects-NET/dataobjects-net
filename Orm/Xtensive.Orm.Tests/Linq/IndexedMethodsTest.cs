// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.14

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class IndexedMethodsTest : ChinookDOModelTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
    }

    [Test]
    public void SelectIndexedTest()
    {
      var result = Session.Query.All<Invoice>().OrderBy(i => i.InvoiceId).Select((invoice, index) => new {invoice, index}).ToList();
      var expected = Session.Query.All<Invoice>().AsEnumerable().OrderBy(i => i.InvoiceId).Select((invoice, index) => new {invoice, index}).ToList();
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void SelectIndexedWhereTest()
    {
      var result = Session.Query.All<Customer>()
        .Select((c, i) => new {Customer = c, Index = i})
        .Where(a => a.Customer.FirstName=="Michelle")
        .ToList();
      Assert.AreEqual(1, result.Count);
      Assert.Greater(result[0].Index, 1);
    }

    [Test]
    public void SelectManyIndexedTest()
    {
      var count = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>()
        .OrderBy(customer=>customer.LastName)
        .SelectMany((customer, index) => customer.Invoices.OrderBy(i=>i.InvoiceId).Select(invoice=>new {index, invoice.Commission}));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .OrderBy(customer=>customer.LastName)
        .SelectMany((customer, index) => customer.Invoices.OrderBy(i=>i.InvoiceId).Select(invoice=>new {index, invoice.Commission}));
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

//    public static IQueryable<TResult> SelectMany<TSource, TCollection, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector, Expression<Func<TSource, TCollection, TResult>> resultSelector);
    [Test]
    public void SelectManyIndexedResultSelectorTest()
    {
      var result = Session.Query.All<Customer>()
        .OrderBy(customer => customer.LastName)
        .SelectMany(
          (customer, index) => customer.Invoices.OrderBy(i => i.InvoiceId).Select(invoice => new {index, invoice.Commission}),
          (customer, takenInvoices) => new {customer, takenInvoices});
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .OrderBy(customer => customer.LastName)
        .SelectMany(
          (customer, index) => customer.Invoices.OrderBy(i => i.InvoiceId).Select(invoice => new {index, invoice.Commission}),
          (customer, takenInvoices) => new {customer, takenInvoices });
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void WhereIndexedTest()
    {
      var avgCommission = Session.Query.All<Invoice>().Select(i => i.Commission).Average();
      var result = Session.Query.All<Invoice>().OrderBy(i => i.InvoiceId)
        .Where((invoice, index) => index > 10 || invoice.Commission > avgCommission);
      var expected = Session.Query.All<Invoice>().AsEnumerable().OrderBy(i => i.InvoiceId)
        .Where((invoice, index) => index > 10 || invoice.Commission > avgCommission);
      Assert.That(result, Is.Not.Empty);
      Assert.IsTrue(expected.SequenceEqual(result));
    }


    //    public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int, bool>> predicate);


  }
}