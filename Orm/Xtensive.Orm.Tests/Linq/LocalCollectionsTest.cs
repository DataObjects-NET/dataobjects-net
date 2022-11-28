// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.09.07

using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.Linq.LocalCollectionsTest_Model;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using System.Threading.Tasks;

namespace Xtensive.Orm.Tests.Linq.LocalCollectionsTest_Model
{
  public class Node
  {
    public string Name { get; set; }

    public Node Parent { get; set; }

    public Node(string name)
    {
      Name = name;
    }
  }

  public class Poco<T>
  {
    public T Value { get; set; }

    public bool Equals(Poco<T> other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Value, Value);
    }

    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Poco<T>))
        return false;
      return Equals((Poco<T>) obj);
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }
  }
  
  public class Poco<T1, T2>
  {
    public T1 Value1 { get; set; }
    public T2 Value2 { get; set; }

    public bool Equals(Poco<T1, T2> other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Value1, Value1) && Equals(other.Value2, Value2);
    }

    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Poco<T1, T2>))
        return false;
      return Equals((Poco<T1, T2>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return (Value1.GetHashCode() * 397) ^ Value2.GetHashCode();
      }
    }

    public Poco(T1 Value1, T2 Value2)
    {
      this.Value1 = Value1;
      this.Value2 = Value2;
    }

    public Poco()
    {

    }
  }

  public class Poco<T1, T2, T3>
  {
    public T1 Value1 { get; set; }
    public T2 Value2 { get; set; }
    public T3 Value3 { get; set; }

    public Poco(T1 Value1, T2 Value2, T3 Value3)
    {
      this.Value1 = Value1;
      this.Value2 = Value2;
      this.Value3 = Value3;
    }

    public bool Equals(Poco<T1, T2, T3> other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Value1, Value1) && Equals(other.Value2, Value2) && Equals(other.Value3, Value3);
    }

    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Poco<T1, T2, T3>))
        return false;
      return Equals((Poco<T1, T2, T3>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        int result = Value1.GetHashCode();
        result = (result * 397) ^ Value2.GetHashCode();
        result = (result * 397) ^ Value3.GetHashCode();
        return result;
      }
    }

    public Poco()
    {

    }
  }
}

namespace Xtensive.Orm.Tests.Linq
{
  public class LocalCollectionsTest : ChinookDOModelTest
  {
    protected override Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Node).Assembly, typeof (Node).Namespace);
      return config;
    }

    [Test]
    public void JoinWithLazyLoadFieldTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      // Track.Bytes is LazyLoad field.
      var tracks = Session.Query.All<Track>().Take(10).ToList();
      var result =
        (from il1 in Session.Query.All<InvoiceLine>().Select(il => il.Track)
        join il2 in tracks on il1 equals il2
        select new {il1, il2})
          .Take(10);
      var list = result.ToList();
      Assert.That(list, Is.Not.Empty);
    }

    [Test]
    public void ListContainsTest()
    {
      var list = new List<string>(){"Michelle", "Jack"};
      var query = from c in Session.Query.All<Customer>()
           where !list.Contains(c.FirstName)
           select c.Invoices;
      var expected = from c in Customers
           where !list.Contains(c.FirstName)
           select c.Invoices;
      Assert.That(list, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ReuseContainsTest()
    {
      var list = new List<int> {382066, 264829};
      var tracks = GetTracks(list);
      foreach (var track in tracks)
        Assert.IsTrue(list.Contains(track.Milliseconds));

      list = new List<int> {217573};
      tracks = GetTracks(list);
      foreach (var track in tracks)
        Assert.IsTrue(list.Contains(track.Milliseconds));

      IEnumerable<Track> GetTracks(IEnumerable<int> ms)
      {
        return Session.Query.Execute(qe =>
          from track in qe.All<Track>()
          where ms.Contains(track.Milliseconds)
          select track);
      }
    }

    [Test]
    public void PairTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var pairs = Session.Query.All<Customer>()
        .Select(customer => new Pair<string, int>(customer.LastName, (int)customer.Invoices.Count))
        .ToList();
      var query = Session.Query.All<Customer>()
        .Join(pairs, customer => customer.LastName, pair => pair.First, (customer, pair) => new {customer, pair.Second});
      var expected = Customers
        .Join(pairs, customer => customer.LastName, pair => pair.First, (customer, pair) => new {customer, pair.Second});
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Pair2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var pairs = Session.Query.All<Customer>()
        .Select(customer => new Pair<string, int>(customer.LastName, (int)customer.Invoices.Count))
        .ToList();
      var query = Session.Query.All<Customer>()
        .Join(pairs, customer => customer.LastName, pair => pair.First, (customer, pair) => pair.Second);
      var expected = Session.Query.All<Customer>().AsEnumerable()
        .Join(pairs, customer => customer.LastName, pair => pair.First, (customer, pair) => pair.Second);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco1Test()
    {
      _ = Assert.Throws<QueryTranslationException>(() => {
        var pocos = Customers
          .Select(customer => new Poco<string>() { Value = customer.LastName })
          .ToList();
        var query = Session.Query.All<Customer>()
          .Join(pocos, customer => customer.LastName, poco => poco.Value, (customer, poco) => poco);
        var expected = Customers
          .Join(pocos, customer => customer.LastName, poco => poco.Value, (customer, poco) => poco);
        Assert.That(query, Is.Not.Empty);
        Assert.AreEqual(0, expected.Except(query).Count());
        QueryDumper.Dump(query);
      });
    }

    [Test]
    public void Poco2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var pocos = Session.Query.All<Customer>()
        .Select(customer => new Poco<string, string>(){Value1 = customer.LastName, Value2 = customer.LastName})
        .ToList();
      var query = Session.Query.All<Customer>()
        .Join(pocos, customer => customer.LastName, poco => poco.Value1, (customer, poco) => poco.Value1);
      var expected = Customers
        .Join(pocos, customer => customer.LastName, poco => poco.Value1, (customer, poco) => poco.Value1);
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco3Test()
    {
      var query = Session.Query.All<Customer>()
        .Select(customer => new Poco<string, string>{Value1 = customer.LastName, Value2 = customer.LastName})
        .Select(poco=>new {poco.Value1, poco.Value2});
      var expected = Customers
        .Select(customer => new Poco<string, string>{Value1 = customer.LastName, Value2 = customer.LastName})
        .Select(poco=>new {poco.Value1, poco.Value2});
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco4Test()
    {
      var query = Session.Query.All<Customer>()
        .Select(customer => new Poco<string, string>(customer.LastName, customer.LastName))
        .Select(poco=>new {poco.Value1, poco.Value2});
      var expected = Customers
        .Select(customer => new Poco<string, string>{Value1 = customer.LastName, Value2 = customer.LastName})
        .Select(poco=>new {poco.Value1, poco.Value2});
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ArrayContainsTest()
    {
      var list = new[] {"Michelle", "Jack"};
      var query = from c in Session.Query.All<Customer>()
                  where !list.Contains(c.FirstName)
                  select c.Invoices;
      var expected = from c in Customers
                     where !list.Contains(c.FirstName)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void IListContainsTest()
    {
      var list = (IList<string>) new List<string> {"Michelle", "Jack"};
      var query = from c in Session.Query.All<Customer>()
                  where !list.Contains(c.FirstName)
                  select c.Invoices;
      var expected = from c in Customers
                     where !list.Contains(c.FirstName)
                     select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ListNewContainsTest()
    {
      var query = from c in Session.Query.All<Customer>()
           where !new List<string> {"Michelle", "Jack"}.Contains(c.FirstName)
           select c.Invoices;
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
    }

    [Test]
    public void TypeLoop1Test()
    {
      var nodes = new Node[10];
      var query = Session.Query.All<Invoice>()
        .Join(nodes, invoice => invoice.Customer.Address.City, node => node.Name, (invoice, node) => new {invoice, node});
      _ = Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(query));
    }

    [Test]
    public void ContainsTest()
    {
      var localInvoiceCommissions = Session.Query.All<Invoice>().Select(invoice => invoice.Commission).Take(5).ToList();
      var query = Session.Query.All<Invoice>()
        .Where(invoice => localInvoiceCommissions.Contains(invoice.Commission));
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices
        .Where(invoice => localInvoiceCommissions.Contains(invoice.Commission));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AnyTest()
    {
      var localInvoiceCommissions = Session.Query.All<Invoice>().Select(invoice => invoice.Commission).Take(5).ToList();
      var query = Session.Query.All<Invoice>()
        .Where(invoice => localInvoiceCommissions.Any(commission => commission==invoice.Commission));
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices
        .Where(invoice => localInvoiceCommissions.Any(commission => commission==invoice.Commission));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AllTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localInvoiceCommissions = Session.Query.All<Invoice>().Select(invoice => invoice.Commission).Take(5).ToList();
      var query = Session.Query.All<Invoice>()
        .Where(invoice => localInvoiceCommissions.All(commission => commission!=invoice.Commission));
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices
        .Where(invoice => localInvoiceCommissions.All(commission => commission!=invoice.Commission));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void KeyTest()
    {
      _ = Assert.Throws<QueryTranslationException>(() => {
        var keys = Session.Query.All<Invoice>().Take(10).Select(invoice => invoice.Key).ToList();
        var query = Session.Query.All<Invoice>()
          .Join(keys, invoice => invoice.Key, key => key, (invoice, key) => new {invoice, key});
        Assert.That(query, Is.Not.Empty);
        QueryDumper.Dump(query);
        var expectedQuery = Invoices
          .Join(keys, invoice => invoice.Key, key => key, (invoice, key) => new {invoice, key});
        Assert.AreEqual(0, expectedQuery.Except(query).Count());
      });
    }

    [Test]
    public void JoinEntityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localInvoices = Session.Query.All<Invoice>().Take(5).ToList();
      var query = Session.Query.All<Invoice>().Join(
        localInvoices,
        invoice => invoice,
        localInvoice => localInvoice,
        (invoice, localInvoice) => new {invoice, localInvoice});
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices.Join(
        localInvoices,
        invoice => invoice,
        localInvoice => localInvoice,
        (invoice, localInvoice) => new {invoice, localInvoice});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityFieldTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.MySql);
      var localInvoices = Session.Query.All<Invoice>().Take(5).ToList();
      var query = Session.Query.All<Invoice>().Join(
        localInvoices,
        invoice => invoice.Commission,
        localInvoice => localInvoice.Commission,
        (invoice, localInvoice) => new {invoice, localInvoice});
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices.Join(
        localInvoices,
        invoice => invoice.Commission,
        localInvoice => localInvoice.Commission,
        (invoice, localInvoice) => new {invoice, localInvoice});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityField2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localCommissions = Session.Query.All<Invoice>().Take(5).Select(invoice => invoice.Commission).ToList();
      var query = Session.Query.All<Invoice>().Join(
        localCommissions,
        invoice => invoice.Commission,
        commission => commission,
        (invoice, commission) => new {invoice, commission});
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices.Join(
        localCommissions,
        invoice => invoice.Commission,
        commission => commission,
        (invoice, commission) => new {invoice, commission});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityField2MaterializeTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localCommissions = Session.Query.All<Invoice>().Take(5).Select(invoice => invoice.Commission).ToList();
      var query = Session.Query.All<Invoice>().Join(
        localCommissions,
        invoice => invoice.Commission,
        commission => commission,
        (invoice, commission) => new {invoice, commission}).Select(x => x.commission);
      Assert.That(query, Is.Not.Empty);
      QueryDumper.Dump(query);
      var expectedQuery = Invoices.Join(
        localCommissions,
        invoice => invoice.Commission,
        commission => commission,
        (invoice, commission) => new {invoice, commission}).Select(x => x.commission);
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }


    [Test]
    public void SimpleConcatTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Session.Query.All<Customer>();
      var result = customers.Where(c => c.Invoices.Count <= 1).Concat(Session.Query.All<Customer>().ToList().Where(c => c.Invoices.Count > 1));
      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
      Assert.AreEqual(customers.Count(), result.Count());
    }

    [Test]
    public void SimpleUnionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var tracks = Session.Query.All<Track>();
      var customers = Session.Query.All<Customer>();
      var tracksFirstChars = tracks.Select(t => t.Name.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.FirstName.Substring(0, 1)).ToList();
      var uniqueFirstChars = tracksFirstChars.Union(customerFirstChars);
      Assert.That(uniqueFirstChars, Is.Not.Empty);
      QueryDumper.Dump(uniqueFirstChars);
    }

    [Test]
    public void IntersectTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.MySql);
      var tracks = Session.Query.All<Track>();
      var customers = Session.Query.All<Customer>();
      var trackFirstChars = tracks.Select(t => t.Name.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.FirstName.Substring(0, 1)).ToList();
      var commonFirstChars = trackFirstChars.Intersect(customerFirstChars);
      Assert.That(commonFirstChars, Is.Not.Empty);
      QueryDumper.Dump(commonFirstChars);
    }

    [Test]
    public void SimpleIntersectTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.MySql);
      var query = Session.Query.All<Invoice>()
        .Select(i => i.DesignatedEmployee.BirthDate)
        .Intersect(Session.Query.All<Invoice>().ToList().Select(i => i.DesignatedEmployee.BirthDate));

      var expected = Invoices
        .Select(i => i.DesignatedEmployee.BirthDate)
        .Intersect(Session.Query.All<Invoice>().ToList().Select(i => i.DesignatedEmployee.BirthDate));

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SimpleIntersectEntityTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.MySql);
      var query = Session.Query.All<Invoice>()
        .Select(i => i.DesignatedEmployee)
        .Intersect(Session.Query.All<Invoice>().ToList().Select(i => i.DesignatedEmployee));

      var expected = Invoices
        .Select(i => i.DesignatedEmployee)
        .Intersect(Session.Query.All<Invoice>().ToList().Select(i => i.DesignatedEmployee));

      Assert.That(query, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SimpleExceptTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.MySql);
      var tracks = Session.Query.All<Track>();
      var customers = Session.Query.All<Customer>();
      var trackFirstChars = tracks.Select(t => t.Name.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.FirstName.Substring(0, 1)).ToList();
      var trackOnlyFirstChars = trackFirstChars.Except(customerFirstChars);

      Assert.That(trackOnlyFirstChars, Is.Not.Empty);
      QueryDumper.Dump(trackOnlyFirstChars);
    }

    [Test]
    public void ConcatDifferentTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = customers
        .Select(c => c.Phone)
        .Concat(customers.ToList().Select(c => c.Fax))
        .Concat(employees.ToList().Select(e => e.Phone));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ConcatDifferentTest2()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = customers
        .Select(c => new {Name = c.FirstName, c.Phone})
        .Concat(employees.ToList().Select(e => new {Name = e.FirstName + " " + e.LastName, e.Phone}));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionDifferentTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var employees = Session.Query.All<Employee>();
      var result = employees
        .Select(c => c.EmployeeId)
        .Union(employees.ToList()
          .Select(e => e.EmployeeId));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionCollationsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = customers
        .Select(c => c.Address.Country)
        .Union(employees.ToList().Select(e => e.Address.Country));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectDifferentTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.MySql);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = customers
        .Select(c => c.Address.Country)
        .Intersect(employees.ToList().Select(e => e.Address.Country));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ExceptDifferentTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      Require.ProviderIsNot(StorageProvider.MySql);
      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = customers
        .Select(c => c.Address.Country)
        .Except(employees.ToList().Select(e => e.Address.Country));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymousTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var customers = Session.Query.All<Invoice>();
      var result = customers.Select(c => new {c.Commission, c.InvoiceDate})
        .Union(customers.ToList().Select(c => new {Commission = c.Commission+1, c.InvoiceDate}));
      var expected = customers.AsEnumerable().Select(c => new {c.Commission, c.InvoiceDate})
        .Union(customers.ToList().Select(c => new {Commission = c.Commission+1, c.InvoiceDate}));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void UnionAnonymousCollationsTest()
    {
      // SQLite does not support paging operations inside set operations
      Require.ProviderIsNot(StorageProvider.Sqlite);

      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => new {c.FirstName, c.LastName})
        .Take(10)
        .Union(customers.ToList().Select(c => new {c.FirstName, c.LastName}));

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous2Test()
    {
      // SQLite does not support paging operations inside set operations
      Require.ProviderIsNot(StorageProvider.Sqlite);

      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var customers = Session.Query.All<Customer>();
      var result = customers.Select(c => new {c.FirstName, c.LastName, c.Address})
        .Where(c => c.Address.StreetAddress.Length < 10)
        .Select(c => new {c.FirstName, c.Address.City})
        .Take(10)
        .Union(customers.ToList().Select(c => new {c.FirstName, c.Address.City})).Where(c => c.FirstName.Length < 5);

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous3Test()
    {
      // SQLite does not support paging operations inside set operations
      Require.ProviderIsNot(StorageProvider.Sqlite);

      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);

      var customers = Session.Query.All<Customer>();
      var employees = Session.Query.All<Employee>();
      var result = customers.Select(c => new {c.FirstName, c.LastName, c.Address})
        .Where(c => c.Address.StreetAddress.Length < 15)
        .Select(c => new {Name = c.FirstName, Address = c.Address.City})
        .Take(10)
        .Union(employees.ToList().Select(e => new {Name = e.FirstName, Address = e.LastName}))
        .Where(c => c.Address.Length < 7);
      QueryDumper.Dump(result);
    }

    [Test]
    public void Grouping1Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localItems = GetLocalItems(10);
      var queryable = Session.Query.Store(localItems);
      var result = queryable
        .GroupBy(keySelector => keySelector.Value3.Substring(0, 1), 
          (key, grouping) => new {key, Value1 = grouping.Select(p => p.Value1)})
        .OrderBy(grouping => grouping.key);
      var expected = localItems
        .GroupBy(keySelector => keySelector.Value3.Substring(0, 1),
        (key, grouping) => new {key, Value1 = grouping.Select(p => p.Value1)})
        .OrderBy(grouping => grouping.key);
      var expectedList = expected.ToList();
      var resultList = result.ToList();

      Assert.That(resultList, Is.Not.Empty);
      Assert.AreEqual(resultList.Count, expectedList.Count);

      for (var i = 0; i < resultList.Count; i++) {
        Console.WriteLine(string.Format("Key (expected/result): {0} / {1}", expectedList[i].key, resultList[i].key));
        foreach (var expectedValue in expectedList[i].Value1)
          Console.WriteLine(string.Format("Expected Value: {0}", expectedValue));
        foreach (var resultValue in resultList[i].Value1)
          Console.WriteLine(string.Format("Result Value: {0}", resultValue));
        Assert.AreEqual(resultList[i].key, expectedList[i].key);
        var isCorrect = expectedList[i].Value1.Except(resultList[i].Value1).Count()==0;
        Assert.IsTrue(isCorrect); 
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void Grouping2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localItems = GetLocalItems(10);
      var queryable = Session.Query.Store(localItems);
      var result = queryable
        .GroupBy(keySelector => keySelector.Value3[0], (key, grouping) => new {key, Value1 = grouping.Select(p => p.Value1)})
        .OrderBy(grouping => grouping.key);
      var expected = localItems
        .GroupBy(keySelector => keySelector.Value3[0], (key, grouping) => new {key, Value1 = grouping.Select(p => p.Value1)})
        .OrderBy(grouping => grouping.key);
      var expectedList = expected.ToList();
      var resultList = result.ToList();

      Assert.That(resultList, Is.Not.Empty);
      Assert.AreEqual(resultList.Count, expectedList.Count);

      for (var i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(resultList[i].key, expectedList[i].key); 
        Assert.AreEqual(0, expectedList[i].Value1.Except(resultList[i].Value1).Count()); 
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void Subquery1Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localItems = GetLocalItems(10);
      var queryable = Session.Query.Store(localItems);
      var result = queryable.Select(poco => Session.Query.All<Invoice>()
          .Where(invoice => invoice.Commission > poco.Value2)).AsEnumerable()
        .Cast<IEnumerable<Invoice>>();
      var expected = localItems.Select(poco => Session.Query.All<Invoice>().AsEnumerable()
        .Where(invoice => invoice.Commission > poco.Value2));
      var expectedList = expected.ToList();
      var resultList = result.ToList();

      Assert.That(resultList, Is.Not.Empty);
      Assert.AreEqual(resultList.Count, expectedList.Count);

      for (var i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(0, expectedList[i].Except(resultList[i]).Count()); 
      }
      QueryDumper.Dump(result);
    }



    [Test]
    public void Subquery2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localItems = GetLocalItems(10);
      var queryable = Session.Query.Store(localItems);
      var result = queryable
        .Select(poco=> queryable.Where(poco2=>poco2.Value2 > poco.Value2).Select(p=>p.Value3)).AsEnumerable().Cast<IEnumerable<string>>();
      var expected = localItems
        .Select(poco=> localItems.Where(poco2=>poco2.Value2 > poco.Value2).Select(p=>p.Value3));
      var expectedList = expected.ToList();
      var resultList = result.ToList();

      Assert.That(resultList, Is.Not.Empty);
      Assert.AreEqual(resultList.Count, expectedList.Count);

      for (var i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(0, expectedList[i].Except(resultList[i]).Count()); 
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void Aggregate1Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      var localItems = GetLocalItems(100);
      var queryable = Session.Query.Store(localItems);
      var result = queryable.Average(selector => selector.Value1);
      var expected = localItems.Average(selector => selector.Value1);
      Assert.AreEqual(result, expected);
    }

    [Test]
    public void Aggregate2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var localItems = GetLocalItems(100);
      var queryable = Session.Query.Store(localItems);
      var result = Session.Query.All<Invoice>()
        .Where(invoice => invoice.Commission > queryable.Max(poco=>poco.Value2));
      var expected = Invoices
        .Where(invoice => invoice.Commission > localItems.Max(poco=>poco.Value2));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public async Task Aggregate2AsyncTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var localItems = GetLocalItems(100);
      var queryable = Session.Query.Store(localItems);
      var result = (await Session.Query.All<Invoice>()
        .Where(invoice => invoice.Commission > queryable.Max(poco => poco.Value2)).ExecuteAsync()).ToList();
      var expected = Invoices
        .Where(invoice => invoice.Commission > localItems.Max(poco => poco.Value2));

      Assert.That(result, Is.Not.Empty);
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void ClosureCacheTest()
    {
      _ = Assert.Throws<QueryTranslationException>( () => {
        var localItems = GetLocalItems(100);
        var queryable = Session.Query.Store(localItems);
        var result = Session.Query.Execute(qe => qe.All<Invoice>().Where(invoice => invoice.Commission > queryable.Max(poco => poco.Value2)));
        Assert.That(result, Is.Not.Empty);
        QueryDumper.Dump(result);
      });
    }

    [Test]
    [Ignore("Very long")]
    public void VeryLongTest()
    {
      var localItems = GetLocalItems(1000000);
      var result = Session.Query.All<Invoice>().Join(
        localItems,
        invoice => invoice.Commission,
        localItem => localItem.Value1,
        (invoice, item) => new {invoice.DesignatedEmployee, item.Value2});

      Assert.That(result, Is.Not.Empty);
      QueryDumper.Dump(result);
    }

    private IEnumerable<Poco<int, decimal, string>> GetLocalItems(int count)
    {
      return Enumerable
        .Range(0, count)
        .Select(i => new Poco<int, decimal, string> {
            Value1 = i,
            Value2 = (decimal)i / 100, 
            Value3 = Guid.NewGuid().ToString()
          }
        )
        .ToList();
    }
  }
}