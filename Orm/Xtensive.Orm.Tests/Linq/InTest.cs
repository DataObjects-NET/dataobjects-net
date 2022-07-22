// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class InTest : ChinookDOModelTest
  {
    [Test]
    public void StringContainsTest()
    {
      var list = new List<string> {"Michelle", "Jack"};
      var query1 = from c in Session.Query.All<Customer>()
                   where !c.FirstName.In(list)
                   select c.Invoices;
      var query2 = from c in Session.Query.All<Customer>()
                   where !c.FirstName.In("Michelle", "Jack")
                   select c.Invoices;
      var expected1 = from c in Customers
                      where !list.Contains(c.FirstName)
                      select c.Invoices;
      var expected2 = from c in Customers
                      where !c.FirstName.In(list)
                      select c.Invoices;
      Assert.That(query1, Is.Not.Empty);
      Assert.That(query2, Is.Not.Empty);
      Assert.AreEqual(0, expected1.Except(query1).Count());
      Assert.AreEqual(0, expected2.Except(query1).Count());
      Assert.AreEqual(0, expected1.Except(query2).Count());
      Assert.AreEqual(0, expected2.Except(query2).Count());
      QueryDumper.Dump(query1);
      QueryDumper.Dump(query2);
    }

    [Test]
    public async Task StringContainsAsyncTest()
    {
      var list = new List<string> { "Michelle", "Jack" };
      var query1 = (await (from c in Session.Query.All<Customer>()
                   where !c.FirstName.In(list)
                   select c.Invoices).ExecuteAsync()).ToList();
      var query2 = (await (from c in Session.Query.All<Customer>()
                   where !c.FirstName.In("Michelle", "Jack")
                   select c.Invoices).ExecuteAsync()).ToList();
      var expected1 = from c in Customers
                      where !list.Contains(c.FirstName)
                      select c.Invoices;
      var expected2 = from c in Customers
                      where !c.FirstName.In(list)
                      select c.Invoices;
      Assert.That(query1, Is.Not.Empty);
      Assert.That(query2, Is.Not.Empty);
      Assert.AreEqual(0, expected1.Except(query1).Count());
      Assert.AreEqual(0, expected2.Except(query1).Count());
      Assert.AreEqual(0, expected1.Except(query2).Count());
      Assert.AreEqual(0, expected2.Except(query2).Count());
      QueryDumper.Dump(query1);
      QueryDumper.Dump(query2);
    }

    [Test]
    public void MartinTest()
    {
      _ = Session.Query.All<Customer>()
        .LeftJoin(Session.Query.All<Invoice>(), c => c, i => i.Customer, (c, i) => new { Customer = c, Invoice = i })
        .GroupBy(i => new { i.Customer.FirstName, i.Customer.LastName })
        .Select(g => new { Key = g.Key, Count = g.Count(j => j.Invoice != null) })
        .ToList();
    }

    [Test]
    public void LongSequenceIntTest()
    {
      // Wrong JOIN mapping for temptable version of .In
      var list1 = new List<decimal?> {0.05m, 0.18m, 0.41m};
      var list2 = new List<decimal?>();
      for (var i = 0; i < 100; i++) 
        list2.AddRange(list1);
      var query1 = from invoice in Session.Query.All<Invoice>()
                   where (invoice.Commission).In(list1)
                   select invoice;
      var query2 = from invoice in Session.Query.All<Invoice>()
                   where (invoice.Commission).In(list2)
                   select invoice;
      var expected = from invoice in Invoices
                     where (invoice.Commission).In(list1)
                     select invoice;
      Assert.That(query1, Is.Not.Empty);
      Assert.That(query2, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query1).Count());
      Assert.AreEqual(0, expected.Except(query2).Count());
    }

    [Test]
    public async Task LongSequenceIntAsyncTest()
    {
      // Wrong JOIN mapping for temptable version of .In
      var list1 = new List<decimal?> { 0.05m, 0.18m, 0.41m };
      var list2 = new List<decimal?>();
      for (var i = 0; i < 100; i++)
        list2.AddRange(list1);
      var query1 = (await (from invoice in Session.Query.All<Invoice>()
                   where (invoice.Commission).In(list1)
                   select invoice).ExecuteAsync()).ToList();
      var query2 = (await (from invoice in Session.Query.All<Invoice>()
                   where (invoice.Commission).In(list2)
                   select invoice).ExecuteAsync()).ToList();
      var expected = from invoice in Invoices
                     where (invoice.Commission).In(list1)
                     select invoice;
      Assert.That(query1, Is.Not.Empty);
      Assert.That(query2, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query1).Count());
      Assert.AreEqual(0, expected.Except(query2).Count());
    }

    [Test]
    public void IntAndDecimalContains1Test()
    {
      // casts int to decimal
      var list = new List<int> {5, 18, 41};
      var query = from invoice in Session.Query.All<Invoice>()
                  where !((int) invoice.Commission).In(list)
                  select invoice;
      var expected = from invoice in Invoices
                     where !((int) invoice.Commission).In(list)
                     select invoice;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task IntAndDecimalContains1AsyncTest()
    {
      // casts int to decimal
      var list = new List<int> { 5, 18, 41 };
      var query = (await (from invoice in Session.Query.All<Invoice>()
                  where !((int) invoice.Commission).In(list)
                  select invoice).ExecuteAsync()).ToList();
      var expected = from invoice in Invoices
                     where !((int) invoice.Commission).In(list)
                     select invoice;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleIntContainsTest()
    {
      var list = new List<int> {276192, 349492, 232463};
      var query = from track in Session.Query.All<Track>()
                  where track.Milliseconds.In(list)
                  select track;
      var expected = from track in Tracks
                     where track.Milliseconds.In(list)
                     select track;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task SimpleIntContainsAsyncTest()
    {
      var list = new List<int> { 276192, 349492, 232463 };
      var query = (await (from track in Session.Query.All<Track>()
                  where track.Milliseconds.In(list)
                  select track).ExecuteAsync()).ToList();
      var expected = from track in Tracks
                     where track.Milliseconds.In(list)
                     select track;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ReuseIntContainsTest()
    {
      var list = new List<int> {276192, 349492};
      var tracks = GetTracks(list);

      var tracksExist = false;
      foreach (var track in tracks) {
        Assert.IsTrue(track.Milliseconds.In(list));
        tracksExist = true;
      }
      Assert.That(tracksExist, Is.True);

      list = new List<int> {232463};
      tracks = GetTracks(list);
      tracksExist = false;
      foreach (var invoice in tracks) {
        Assert.IsTrue(invoice.Milliseconds.In(list));
        tracksExist = true;
      }
      Assert.That(tracksExist, Is.True);
    }

    [Test]
    public void IntAndDecimalContains2Test()
    {
      // casts decimal to int
      var list = new List<decimal> {7, 22, 46};
      var query = from invoice in Session.Query.All<Invoice>()
                  where !((decimal) invoice.InvoiceId).In(list)
                  select invoice;
      var expected = from invoice in Invoices
                     where !((decimal) invoice.InvoiceId).In(list)
                     select invoice;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task IntAndDecimalContains2AsyncTest()
    {
      // casts decimal to int
      var list = new List<decimal> { 7, 22, 46 };
      var query = (await (from invoice in Session.Query.All<Invoice>()
                  where !((decimal) invoice.InvoiceId).In(list)
                  select invoice).ExecuteAsync()).ToList();
      var expected = from invoice in Invoices
                     where !((decimal) invoice.InvoiceId).In(list)
                     select invoice;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void EntityContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5).ToList();
      var query = from c in Session.Query.All<Customer>()
                  where !c.In(list)
                  select c.Invoices;
      var expected = from c in Customers
                     where !c.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task EntityContainsAsyncTest()
    {
      var list = Session.Query.All<Customer>().Take(5).ToList();
      var query = (await (from c in Session.Query.All<Customer>()
                  where !c.In(list)
                  select c.Invoices).ExecuteAsync()).ToList();
      var expected = from c in Customers
                     where !c.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void StructContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5).Select(c => c.Address).ToList();
      var query = from c in Session.Query.All<Customer>()
                  where !c.Address.In(list)
                  select c.Invoices;
      var expected = from c in Customers
                     where !c.Address.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task StructContainsAsyncTest()
    {
      var list = Session.Query.All<Customer>().Take(5).Select(c => c.Address).ToList();
      var query = (await (from c in Session.Query.All<Customer>()
                  where !c.Address.In(list)
                  select c.Invoices).ExecuteAsync()).ToList();
      var expected = from c in Customers
                     where !c.Address.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void StructSimpleContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5).Select(c => c.Address).ToList();
      var query = Session.Query.All<Customer>().Select(c => c.Address).Where(c => c.In(list));
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task StructSimpleContainsAsyncTest()
    {
      var list = (await Session.Query.All<Customer>().Take(5).Select(c => c.Address).ExecuteAsync()).ToList();
      var query = Session.Query.All<Customer>().Select(c => c.Address).Where(c => c.In(list));
      QueryDumper.Dump(query);
    }

    [Test]
    public void AnonimousContainsTest()
    {
      var list = new[] {new {FirstName = "Michelle"}, new {FirstName = "Jack"}};
      var query = Session.Query.All<Customer>().Where(c => new {c.FirstName}.In(list)).Select(c => c.Invoices);
      var expected = Customers.Where(c => new {c.FirstName}.In(list)).Select(c => c.Invoices);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task AnonimousContainsAsyncTest()
    {
      var list = new[] { new { FirstName = "Michelle" }, new { FirstName = "Jack" } };
      var query = (await Session.Query.All<Customer>()
        .Where(c => new { c.FirstName }.In(list))
        .Select(c => c.Invoices)
        .ExecuteAsync()).ToList();
      var expected = Customers.Where(c => new { c.FirstName }.In(list)).Select(c => c.Invoices);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void AnonimousContains2Test()
    {
      var list = new[] {new {Id1 = "Michelle", Id2 = "Michelle"}, new {Id1 = "Jack", Id2 = "Jack"}};
      var query = Session.Query.All<Customer>().Where(c => new {Id1 = c.FirstName, Id2 = c.FirstName}.In(list)).Select(c => c.Invoices);
      var expected = Customers.Where(c => new {Id1 = c.FirstName, Id2 = c.FirstName}.In(list)).Select(c => c.Invoices);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task AnonimousContains2AsyncTest()
    {
      var list = new[] { new { Id1 = "Michelle", Id2 = "Michelle" }, new { Id1 = "Jack", Id2 = "Jack" } };
      var query = (await Session.Query.All<Customer>()
        .Where(c => new { Id1 = c.FirstName, Id2 = c.FirstName }.In(list))
        .Select(c => c.Invoices)
        .ExecuteAsync()).ToList();
      var expected = Customers.Where(c => new { Id1 = c.FirstName, Id2 = c.FirstName }.In(list)).Select(c => c.Invoices);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableDoubleContainsTest()
    {
      var list = Session.Query.All<Invoice>()
        .Select(o => o.Commission)
        .Distinct()
        .Take(10);
      var query = from invoice in Session.Query.All<Invoice>()
                  where !invoice.Commission.In(list)
                  select invoice;
      var expected = from invoice in Invoices
                     where !invoice.Commission.In(list)
                     select invoice;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task QueryableDoubleContainsAsyncTest()
    {
      var list = Session.Query.All<Invoice>()
        .Select(o => o.Commission)
        .Distinct()
        .Take(10);
      var query = (await (from invoice in Session.Query.All<Invoice>()
                  where !invoice.Commission.In(list)
                  select invoice).ExecuteAsync()).ToList();
      var expected = from invoice in Invoices
                     where !invoice.Commission.In(list)
                     select invoice;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableEntityContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(5);
      var query = from c in Session.Query.All<Customer>()
                  where !c.In(list)
                  select c.Invoices;
      var expected = from c in Customers
                     where !c.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task QueryableEntityContainsAsyncTest()
    {
      var list = Session.Query.All<Customer>().Take(5);
      var query = (await (from c in Session.Query.All<Customer>()
                  where !c.In(list)
                  select c.Invoices).ExecuteAsync()).ToList();
      var expected = from c in Customers
                     where !c.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableStructContainsTest()
    {
      var list = Session.Query.All<Customer>()
        .Take(5)
        .Select(c => c.Address);
      var query = from c in Session.Query.All<Customer>()
                  where !c.Address.In(list)
                  select c.Invoices;
      var expected = from c in Customers
                     where !c.Address.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task QueryableStructContainsAsyncTest()
    {
      var list = Session.Query.All<Customer>()
        .Take(5)
        .Select(c => c.Address);
      var query = (await (from c in Session.Query.All<Customer>()
                  where !c.Address.In(list)
                  select c.Invoices).ExecuteAsync()).ToList();
      var expected = from c in Customers
                     where !c.Address.In(list)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void QueryableAnonimousContainsTest()
    {
      var list = Session.Query.All<Customer>().Take(10).Select(c => new {c.FirstName});
      var query = Session.Query.All<Customer>().Where(c => new {c.FirstName}.In(list)).Select(c => c.Invoices);
      var expected = Customers.Where(c => new {c.FirstName}.In(list)).Select(c => c.Invoices);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task QueryableAnonimousContainsAsyncTest()
    {
      var list = Session.Query.All<Customer>().Take(10).Select(c => new { c.FirstName });
      var query = (await Session.Query.All<Customer>()
        .Where(c => new { c.FirstName }.In(list))
        .Select(c => c.Invoices)
        .ExecuteAsync()).ToList();
      var expected = Customers.Where(c => new { c.FirstName }.In(list)).Select(c => c.Invoices);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    private IEnumerable<Track> GetTracks(IEnumerable<int> ids)
    {
      return Session.Query.Execute(qe =>
        from track in qe.All<Track>()
        where track.Milliseconds.In(ids)
        select track);
    }

    [Test]
    public void ComplexCondition1Test()
    {
      var includeAlgorithm = IncludeAlgorithm.TemporaryTable;
      var list = new List<int> {276192, 349492, 232463};
      var query = from track in Session.Query.All<Track>()
                  where track.Milliseconds.In(includeAlgorithm, list)
                  select track;
      var expected = from track in Tracks
                     where track.Milliseconds.In(includeAlgorithm, list)
                     select track;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task ComplexCondition1AsyncTest()
    {
      var includeAlgorithm = IncludeAlgorithm.TemporaryTable;
      var list = new List<int> { 276192, 349492, 232463 };
      var query = (await (from track in Session.Query.All<Track>()
                  where track.Milliseconds.In(includeAlgorithm, list)
                  select track).ExecuteAsync()).ToList();
      var expected = from track in Tracks
                     where track.Milliseconds.In(includeAlgorithm, list)
                     select track;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ComplexCondition2Test()
    {
      var includeAlgorithm = IncludeAlgorithm.TemporaryTable;
      var query = from track in Session.Query.All<Track>()
                  where track.Milliseconds.In(includeAlgorithm, 276192, 349492, 232463)
                  select track;
      var expected = from track in Tracks
                     where track.Milliseconds.In(includeAlgorithm, 276192, 349492, 232463)
                     select track;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public async Task ComplexCondition2AsyncTest()
    {
      var includeAlgorithm = IncludeAlgorithm.TemporaryTable;
      var query = (await (from track in Session.Query.All<Track>()
                  where track.Milliseconds.In(includeAlgorithm, 276192, 349492, 232463)
                  select track).ExecuteAsync()).ToList();
      var expected = from track in Tracks
                     where track.Milliseconds.In(includeAlgorithm, 276192, 349492, 232463)
                     select track;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void CompiledInTest()
    {
      var result1 = GetCustomers("Michelle", "Jack")
        .Select(customer => customer.FirstName)
        .ToList();
      Assert.AreEqual(2, result1.Count);
      Assert.IsTrue(result1.Contains("Michelle"));
      Assert.IsTrue(result1.Contains("Jack"));

      var result2 = GetCustomers("Leonie")
        .Select(customer => customer.FirstName)
        .ToList();
      Assert.AreEqual(1, result2.Count);
      Assert.AreEqual("Leonie", result2[0]);
    }

    private IEnumerable<Customer> GetCustomers(params string[] customerNames)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().Where(customer => customer.FirstName.In(customerNames)));
    }
  }
}
