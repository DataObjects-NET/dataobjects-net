// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using AggregateException = System.AggregateException;

namespace Xtensive.Orm.Tests.Storage.Prefetch
{
  [TestFixture]
  public sealed class PrefetchTest : ChinookDOModelTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.NamingConvention.NamespacePolicy = NamespacePolicy.AsIs;
      config.Types.Register(typeof(Model.Offer).Assembly, typeof(Model.Offer).Namespace);
      return config;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      //var recreateConfig = configuration.Clone();
      var domain = Domain.Build(configuration);
      DataBaseFiller.Fill(domain);
      return domain;
    }

    [Test]
    public void PrefetchGraphTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>()
          .Prefetch(i => i.Customer.CompanyName)
          .Prefetch(i => i.Customer.Address)
          .Prefetch(i => i.InvoiceLines
            .Prefetch(id => id.Track)
            .Prefetch(id => id.Track.Album)
            .Prefetch(id => id.Track.Bytes));
        var result = invoices.ToList();
        foreach (var invoice in result) {
          Console.WriteLine($"Invoice: {invoice.InvoiceId}, Customer CompanyName: {invoice.Customer.CompanyName}, City {invoice.Customer.Address.City}, Items count: {invoice.InvoiceLines.Count}");
          foreach (var orderDetail in invoice.InvoiceLines) {
            Console.WriteLine($"\tTrack: {orderDetail.Track.Name}, Album: {orderDetail.Track.Album.Title}, Track Length: {orderDetail.Track.Bytes.Length}");
          }
        }
        t.Complete();
      }
    }

    [Test]
    public async Task PrefetchGraphAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>()
          .Prefetch(i => i.Customer.CompanyName)
          .Prefetch(i => i.Customer.Address)
          .Prefetch(i => i.InvoiceLines
            .Prefetch(id => id.Track)
            .Prefetch(id => id.Track.Album)
            .Prefetch(id => id.Track.Bytes)).AsAsync();
        var result = (await invoices).ToList();
        foreach (var invoice in result) {
          Console.Out.WriteLine($"Invoice: {invoice.InvoiceId}, Customer CompanyName: {invoice.Customer.CompanyName}, City {invoice.Customer.Address.City}, Items count: {invoice.InvoiceLines.Count}");
          foreach (var orderDetail in invoice.InvoiceLines) {
            Console.WriteLine($"\tTrack: {orderDetail.Track.Name}, Album: {orderDetail.Track.Album.Title}, Track Length: {orderDetail.Track.Bytes.Length}");
          }
        }
        t.Complete();
      }
    }

    [Test]
    public void PrefetchGraphPerformanceTest()
    {
      using (var mx = new Measurement("Query graph of orders without prefetch."))
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>();
        foreach (var invoice in invoices) {
          var id = invoice.InvoiceId;
          var name = invoice.Customer.CompanyName;
          var city = invoice.Customer.Address.City;
          var count = invoice.InvoiceLines.Count;
          foreach (var orderDetail in invoice.InvoiceLines) {
            var trackName = orderDetail.Track.Name;
            var albumTitle = orderDetail.Track.Album.Title;
            var trackBytes = orderDetail.Track.Bytes;
          }
        }
        mx.Complete();
        t.Complete();
        Console.Out.WriteLine(mx.ToString());
      }

      using (var mx = new Measurement("Query graph of orders with prefetch."))
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>()
          .Prefetch(i => i.Customer.CompanyName)
          .Prefetch(i => i.Customer.Address)
          .Prefetch(i => i.InvoiceLines
            .Prefetch(id => id.Track)
            .Prefetch(id => id.Track.Album)
            .Prefetch(id => id.Track.Bytes));
        foreach (var invoice in invoices) {
          var id = invoice.InvoiceId;
          var name = invoice.Customer.CompanyName;
          var city = invoice.Customer.Address.City;
          var count = invoice.InvoiceLines.Count;
          foreach (var orderDetail in invoice.InvoiceLines) {
            var trackName = orderDetail.Track.Name;
            var albumTitle = orderDetail.Track.Album.Title;
            var trackBytes = orderDetail.Track.Bytes;
          }
        }
        mx.Complete();
        t.Complete();
        Console.Out.WriteLine(mx.ToString());
      }
    }

    [Test]
    public async Task PrefetchGraphPerformanceAsyncTest()
    {
      using (var mx = new Measurement("Query graph of orders without prefetch."))
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>();
        foreach (var invoice in invoices) {
          var id = invoice.InvoiceId;
          var name = invoice.Customer.CompanyName;
          var city = invoice.Customer.Address.City;
          var count = invoice.InvoiceLines.Count;
          foreach (var orderDetail in invoice.InvoiceLines) {
            var trackName = orderDetail.Track.Name;
            var albumTitle = orderDetail.Track.Album.Title;
            var trackBytes = orderDetail.Track.Bytes;
          }
        }
        mx.Complete();
        t.Complete();
        Console.Out.WriteLine(mx.ToString());
      }

      using (var mx = new Measurement("Query graph of orders with prefetch."))
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var invoices = session.Query.All<Invoice>()
          .Prefetch(i => i.Customer.CompanyName)
          .Prefetch(i => i.Customer.Address)
          .Prefetch(i => i.InvoiceLines
            .Prefetch(id => id.Track)
            .Prefetch(id => id.Track.Album)
            .Prefetch(id => id.Track.Bytes)).AsAsync();
        foreach (var invoice in await invoices) {
          var id = invoice.InvoiceId;
          var name = invoice.Customer.CompanyName;
          var city = invoice.Customer.Address.City;
          var count = invoice.InvoiceLines.Count;
          foreach (var orderDetail in invoice.InvoiceLines) {
            var trackName = orderDetail.Track.Name;
            var albumTitle = orderDetail.Track.Album.Title;
            var trackBytes = orderDetail.Track.Bytes;
          }
        }
        mx.Complete();
        t.Complete();
        Console.Out.WriteLine(mx.ToString());
      }
    }

    [Test]
    public void EnumerableOfNonEntityTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Invoice>().Select(i => i.Key).ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.Many<Invoice>(keys)
          .Prefetch(o => o.DesignatedEmployee);
        var orderType = Domain.Model.Types[typeof(Invoice)];
        var employeeField = orderType.Fields[nameof(Invoice.DesignatedEmployee)];
        var employeeType = Domain.Model.Types[typeof(Employee)];

        foreach (var invoice in invoices) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(invoice.Key, invoice.Key.TypeInfo, session, fieldSelector);
          var invoiceState = session.EntityStateCache[invoice.Key, true];
          var employeeKey = Key.Create(Domain, WellKnown.DefaultNodeId, Domain.Model.Types[typeof(Employee)],
            TypeReferenceAccuracy.ExactType, employeeField.Associations.Last()
              .ExtractForeignKey(invoiceState.Type, invoiceState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session, fieldSelector);
        }
      }

      static bool fieldSelector(FieldInfo field)
      {
        return field.IsPrimaryKey || field.IsSystem || !field.IsLazyLoad && !field.IsEntitySet;
      }
    }

    [Test]
    public async Task EnumerableOfNonEntityAsyncTest()
    {
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Invoice>().Select(i => i.Key).ToList();
        Assert.Greater(keys.Count, 0);
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoices = session.Query.Many<Invoice>(keys)
          .Prefetch(o => o.DesignatedEmployee).AsAsync();
        var invoiceType = Domain.Model.Types[typeof(Invoice)];
        var employeeField = invoiceType.Fields[nameof(Invoice.DesignatedEmployee)];
        var employeeType = Domain.Model.Types[typeof(Employee)];

        foreach (var invoice in await invoices) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(invoice.Key, invoice.Key.TypeInfo, session, fieldSelector);
          var invoiceState = session.EntityStateCache[invoice.Key, true];
          var employeeKey = Key.Create(Domain, WellKnown.DefaultNodeId, Domain.Model.Types[typeof(Employee)],
            TypeReferenceAccuracy.ExactType, employeeField.Associations.Last()
              .ExtractForeignKey(invoiceState.Type, invoiceState.Tuple));
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(employeeKey, employeeType, session, fieldSelector);
        }
      }

      static bool fieldSelector(FieldInfo field)
      {
        return field.IsPrimaryKey || field.IsSystem || !field.IsLazyLoad && !field.IsEntitySet;
      }
    }

    [Test]
    public void EnumerableOfEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetcher = session.Query.All<Invoice>()
          .Prefetch(o => o.ProcessingTime)
          .Prefetch(o => o.InvoiceLines);
        var invoiceLineField = Domain.Model.Types[typeof(Invoice)].Fields[nameof(Invoice.InvoiceLines)];
        foreach (var invoice in prefetcher) {
          Assert.IsTrue(session.Handler.LookupState(invoice.Key, invoiceLineField, out var entitySetState));
          Assert.IsTrue(entitySetState.IsFullyLoaded);
        }
      }
    }

    [Test]
    public async Task EnumerableOfEntityAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var prefetcher = session.Query.All<Invoice>()
          .Prefetch(o => o.ProcessingTime)
          .Prefetch(o => o.InvoiceLines).AsAsync();
        var invoiceLineField = Domain.Model.Types[typeof(Invoice)].Fields[nameof(Invoice.InvoiceLines)];
        foreach (var invoice in await prefetcher) {
          Assert.IsTrue(session.Handler.LookupState(invoice.Key, invoiceLineField, out var entitySetState));
          Assert.IsTrue(entitySetState.IsFullyLoaded);
        }
      }
    }

    [Test]
    public void PreservingOriginalOrderOfElementsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().ToList();
        var actual = expected.Prefetch(i => i.ProcessingTime).Prefetch(i => i.InvoiceLines).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public async Task PreservingOriginalOrderOfElementsAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().ToList();
        var actual = (await expected.Prefetch(i => i.ProcessingTime).Prefetch(i => i.InvoiceLines).AsAsync()).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PrefetchManyNotFullBatchTest()
    {
      //.Prefetch(t => t.Invoices.Prefetch(e => e.InvoiceLines))

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoicesField = Domain.Model.Types[typeof (Track)].Fields["Playlists"];
        var invoiceLinesField = Domain.Model.Types[typeof (Playlist)].Fields["Tracks"];
        var tracks = session.Query.All<Track>().Take(50)
          .Prefetch(t => t.Playlists.Prefetch(il => il.Tracks));
        int count1 = 0, count2 = 0;
        foreach (var track in tracks) {
          count1++;
          var entitySetState = GetFullyLoadedEntitySet(session, track.Key, invoicesField);
          foreach (var invoice in entitySetState) {
            count2++;
            _ = GetFullyLoadedEntitySet(session, invoice, invoiceLinesField);
          }
        }
        Console.WriteLine(count1);
        Console.WriteLine(count2);
        Assert.AreEqual(2, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public async Task PrefetchManyNotFullBatchAsyncTest()
    {
      //.Prefetch(t => t.Invoices.Prefetch(e => e.InvoiceLines))

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoicesField = Domain.Model.Types[typeof (Track)].Fields["Playlists"];
        var invoiceLinesField = Domain.Model.Types[typeof (Playlist)].Fields["Tracks"];
        var tracks = session.Query.All<Track>().Take(50)
          .Prefetch(t => t.Playlists.Prefetch(il => il.Tracks)).AsAsync();
        int count1 = 0, count2 = 0;
        foreach (var track in await tracks) {
          count1++;
          var entitySetState = GetFullyLoadedEntitySet(session, track.Key, invoicesField);
          foreach (var invoice in entitySetState) {
            count2++;
            _ = GetFullyLoadedEntitySet(session, invoice, invoiceLinesField);
          }
        }
        Console.WriteLine(count1);
        Console.WriteLine(count2);
        Assert.AreEqual(2, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchManySeveralBatchesTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var linesField = Domain.Model.Types[typeof(Invoice)].Fields[nameof(Invoice.InvoiceLines)];
        var trackField = Domain.Model.Types[typeof(InvoiceLine)].Fields[nameof(InvoiceLine.Track)];
        var invoices = session.Query.All<Invoice>()
          .Take(90)
          .Prefetch(o => o.InvoiceLines.Prefetch(od => od.Track));
        int count1 = 0, count2 = 0;
        foreach (var invoice in invoices) {
          count1++;
          var entitySetState = GetFullyLoadedEntitySet(session, invoice.Key, linesField);
          foreach (var line in entitySetState){
            count2++;
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(line, line.TypeInfo, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
            PrefetchTestHelper.AssertReferencedEntityIsLoaded(line, session, trackField);
          }
        }
        Console.WriteLine(count1);
        Console.WriteLine(count2);
        Assert.AreEqual(11, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public async Task PrefetchManySeveralBatchesAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var linesField = Domain.Model.Types[typeof(Invoice)].Fields[nameof(Invoice.InvoiceLines)];
        var trackField = Domain.Model.Types[typeof(InvoiceLine)].Fields[nameof(InvoiceLine.Track)];
        var invoices = session.Query.All<Invoice>()
          .Take(90)
          .Prefetch(o => o.InvoiceLines.Prefetch(od => od.Track)).AsAsync();
        int count1 = 0, count2 = 0;
        foreach (var invoice in await invoices) {
          count1++;
          var entitySetState = GetFullyLoadedEntitySet(session, invoice.Key, linesField);
          foreach (var line in entitySetState) {
            count2++;
            PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(line, line.TypeInfo, session,
              PrefetchTestHelper.IsFieldToBeLoadedByDefault);
            PrefetchTestHelper.AssertReferencedEntityIsLoaded(line, session, trackField);
          }
        }
        Console.WriteLine(count1);
        Console.WriteLine(count2);
        Assert.AreEqual(11, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PrefetchSingleTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.PostgreSql | StorageProvider.MySql);
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Invoice>().Select(o => o.Key).ToList();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoiceType = Domain.Model.Types[typeof(Invoice)];
        var employeeType = Domain.Model.Types[typeof(Employee)];
        var employeeField = Domain.Model.Types[typeof(Invoice)].Fields[nameof(Invoice.DesignatedEmployee)];
        var invoicesField = Domain.Model.Types[typeof(Employee)].Fields[nameof(Employee.Invoices)];
        var invoices = session.Query.Many<Invoice>(keys)
          .Prefetch(o => o.DesignatedEmployee.Invoices);
        var count = 0;
        foreach (var invoice in invoices) {
          Assert.AreEqual(keys[count], invoice.Key);
          count++;
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(invoice.Key, invoiceType, session,
            field => PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(employeeField) || (field.Parent != null && field.Parent.Equals(employeeField)));
          var state = session.EntityStateCache[invoice.Key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(
            state.Entity.GetFieldValue<Employee>(employeeField).Key,
            employeeType, session, field =>
              PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(invoicesField));
        }
        Assert.AreEqual(keys.Count, count);
        Assert.AreEqual(12, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public async Task PrefetchSingleAsyncTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird | StorageProvider.PostgreSql | StorageProvider.MySql);
      List<Key> keys;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        keys = session.Query.All<Invoice>().Select(o => o.Key).ToList();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var invoiceType = Domain.Model.Types[typeof(Invoice)];
        var employeeType = Domain.Model.Types[typeof(Employee)];
        var employeeField = Domain.Model.Types[typeof(Invoice)].Fields[nameof(Invoice.DesignatedEmployee)];
        var invoicesField = Domain.Model.Types[typeof(Employee)].Fields[nameof(Employee.Invoices)];
        var invoices = session.Query.Many<Invoice>(keys)
          .Prefetch(o => o.DesignatedEmployee.Invoices).AsAsync();
        var count = 0;
        foreach (var invoice in await invoices) {
          Assert.AreEqual(keys[count], invoice.Key);
          count++;
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(invoice.Key, invoiceType, session,
            field => PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(employeeField) || (field.Parent != null && field.Parent.Equals(employeeField)));
          var state = session.EntityStateCache[invoice.Key, true];
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(
            state.Entity.GetFieldValue<Employee>(employeeField).Key,
            employeeType, session, field =>
              PrefetchHelper.IsFieldToBeLoadedByDefault(field) || field.Equals(invoicesField));
        }
        Assert.AreEqual(keys.Count, count);
        Assert.AreEqual(12, session.Handler.PrefetchTaskExecutionCount);
      }
    }

    [Test]
    public void PreservingOrderInPrefetchManyNotFullBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Customer>()
          .ToList();
        var actual = expected
          .Prefetch(c => c.Invoices.Prefetch(e => e.InvoiceLines))
          .ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public async Task PreservingOrderInPrefetchManyNotFullBatchAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Customer>()
          .ToList();
        var actual = (await expected
          .Prefetch(c => c.Invoices.Prefetch(e => e.InvoiceLines)).AsAsync())
          .ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreserveOrderingInPrefetchManySeveralBatchesTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().ToList();
        var actual = expected
          .Prefetch(i => i.InvoiceLines.Prefetch(il => il.Track))
          .ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public async Task PreserveOrderingInPrefetchManySeveralBatchesAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().ToList();
        var actual = (await expected
          .Prefetch(i => i.InvoiceLines.Prefetch(il => il.Track)).AsAsync())
          .ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleNotFullBatchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().Take(53).ToList();
        var actual = expected.Prefetch(i => i.DesignatedEmployee.Invoices).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public async Task PreservingOrderInPrefetchSingleNotFullBatchAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().Take(53).ToList();
        var actual = (await expected.Prefetch(i => i.DesignatedEmployee.Invoices).AsAsync()).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void PreservingOrderInPrefetchSingleSeveralBatchesTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().ToList();
        var actual = expected.Prefetch(i => i.DesignatedEmployee.Invoices).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public async Task PreservingOrderInPrefetchSingleSeveralBatchesAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var expected = session.Query.All<Invoice>().ToList();
        var actual = (await expected.Prefetch(i => i.DesignatedEmployee.Invoices).AsAsync()).ToList();
        Assert.AreEqual(expected.Count, actual.Count);
        Assert.IsTrue(expected.SequenceEqual(actual));
      }
    }

    [Test]
    public void ArgumentsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var a = session.Query.All<Track>()
          .Prefetch(t => new {t.TrackId, t.Album})
          .ToList();
        var b = session.Query.All<Track>()
          .Prefetch(t => t.Album.Title)
          .ToList();
        AssertEx.Throws<KeyNotFoundException>(() => session.Query.All<Track>()
          .Prefetch(t => t.PersistenceState)
          .ToList());
        var d = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(Key.Create<Model.OfferContainer>(Domain, 1)))
          .Prefetch(oc => oc.IntermediateOffer.AnotherContainer.RealOffer.Book)
          .ToList();
      }
    }

    [Test]
    public async Task ArgumentsAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var a = (await session.Query.All<Track>()
          .Prefetch(t => new { t.TrackId, t.Album }).AsAsync())
          .ToList();
        var b = (await session.Query.All<Track>()
          .Prefetch(t => t.Album.Title).AsAsync())
          .ToList();
        _ = Assert.ThrowsAsync<KeyNotFoundException>(async () => (await session.Query.All<Track>()
          .Prefetch(t => t.PersistenceState).AsAsync())
          .ToList());
        var d = (await session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(Key.Create<Model.OfferContainer>(Domain, 1)))
          .Prefetch(oc => oc.IntermediateOffer.AnotherContainer.RealOffer.Book).AsAsync())
          .ToList();
      }
    }

    [Test]
    public void SimultaneouslyUsageOfMultipleEnumeratorsTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<Invoice>().ToList();
        var prefetcher = source.Prefetch(i => i.InvoiceLines);
        using (var enumerator0 = prefetcher.GetEnumerator()) {
          _ = enumerator0.MoveNext();
          _ = enumerator0.MoveNext();
          _ = enumerator0.MoveNext();
          Assert.IsTrue(source.SequenceEqual(prefetcher));
          var index = 3;
          while (enumerator0.MoveNext()) {
            Assert.AreSame(source[index++], enumerator0.Current);
          }
          Assert.AreEqual(source.Count, index);
        }
      }
    }

    [Test]
    public async Task SimultaneouslyUsageOfMultipleEnumeratorsAsyncTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var source = session.Query.All<Invoice>().ToList();
        var prefetcher = await source.Prefetch(i => i.InvoiceLines).AsAsync();
        using (var enumerator0 = prefetcher.GetEnumerator()) {
          _ = enumerator0.MoveNext();
          _ = enumerator0.MoveNext();
          _ = enumerator0.MoveNext();
          Assert.IsTrue(source.SequenceEqual(prefetcher));
          var index = 3;
          while (enumerator0.MoveNext()) {
            Assert.AreSame(source[index++], enumerator0.Current);
          }
          Assert.AreEqual(source.Count, index);
        }
      }
    }

    [Test]
    public void RootElementIsNullPrefetchTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          _ = new Model.Book { Title = new Model.Title { Text = "T0" }, Category = "1" };
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var books = session.Query.All<Model.Book>().AsEnumerable()
            .Concat(EnumerableUtils.One<Model.Book>(null)).Prefetch(b => b.Title);
          var titleField = Domain.Model.Types[typeof(Model.Book)].Fields[nameof(Model.Book.Title)];
          var titleType = Domain.Model.Types[typeof(Model.Title)];
          var count = 0;
          foreach (var book in books) {
            count++;
            if (book != null) {
              var titleKey = book.GetReferenceKey(titleField);
              PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
            }
          }
          Assert.AreEqual(2, count);
        }
      }
    }

    [Test]
    public async Task RootElementIsNullPrefetchAsyncTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          _ = new Model.Book { Title = new Model.Title { Text = "T0" }, Category = "1" };
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var books = session.Query.All<Model.Book>().AsEnumerable()
            .Concat(EnumerableUtils.One<Model.Book>(null)).Prefetch(b => b.Title).AsAsync();
          var titleField = Domain.Model.Types[typeof(Model.Book)].Fields[nameof(Model.Book.Title)];
          var titleType = Domain.Model.Types[typeof(Model.Title)];
          var count = 0;
          foreach (var book in await books) {
            count++;
            if (book != null) {
              var titleKey = book.GetReferenceKey(titleField);
              PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
            }
          }
          Assert.AreEqual(2, count);
        }
      }
    }

    [Test]
    public void NestedPrefetchWhenChildElementIsNullTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          _ = new Model.Book { Title = new Model.Title { Text = "T0" }, Category = "1" };
          _ = new Model.Book { Category = "2" };
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var prefetcher = session.Query.All<Model.Book>()
            .Prefetch(b => b.Title.Book);
          var titleField = Domain.Model.Types[typeof(Model.Book)].Fields[nameof(Model.Book.Title)];
          var titleType = Domain.Model.Types[typeof(Model.Title)];
          foreach (var book in prefetcher) {
            var titleKey = book.GetReferenceKey(titleField);
            if (titleKey != null) {
              PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
            }
          }
        }
      }
    }

    [Test]
    public async Task NestedPrefetchWhenChildElementIsNullAsyncTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          _ = new Model.Book { Title = new Model.Title { Text = "T0" }, Category = "1" };
          _ = new Model.Book { Category = "2" };
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var prefetcher = session.Query.All<Model.Book>()
            .Prefetch(b => b.Title.Book).AsAsync();
          var titleField = Domain.Model.Types[typeof(Model.Book)].Fields[nameof(Model.Book.Title)];
          var titleType = Domain.Model.Types[typeof(Model.Title)];
          foreach (var book in await prefetcher) {
            var titleKey = book.GetReferenceKey(titleField);
            if (titleKey != null) {
              PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
            }
          }
        }
      }
    }

    [Test]
    public void NestedPrefetchWhenRootElementIsNullTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          _ = new Model.Book { Title = new Model.Title { Text = "T0" }, Category = "1" };
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var books = session.Query.All<Model.Book>().AsEnumerable().Concat(EnumerableUtils.One<Model.Book>(null))
            .Prefetch(b => b.Title.Book);
          var titleField = Domain.Model.Types[typeof (Model.Book)].Fields["Title"];
          var titleType = Domain.Model.Types[typeof (Model.Title)];
          var count = 0;
          foreach (var book in books) {
            count++;
            if (book != null) {
              var titleKey = book.GetReferenceKey(titleField);
              if (titleKey != null) {
                PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
              }
            }
          }
          Assert.AreEqual(2, count);
        }
      }
    }

    [Test]
    public async Task NestedPrefetchWhenRootElementIsNullAsyncTest()
    {
      RemoveAllBooks();
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
          _ = new Model.Book { Title = new Model.Title { Text = "T0" }, Category = "1" };
          tx.Complete();
        }
        using (var tx = session.OpenTransaction()) {
          var books = session.Query.All<Model.Book>().AsEnumerable().Concat(EnumerableUtils.One<Model.Book>(null))
            .Prefetch(b => b.Title.Book).AsAsync();
          var titleField = Domain.Model.Types[typeof(Model.Book)].Fields[nameof(Model.Book.Title)];
          var titleType = Domain.Model.Types[typeof(Model.Title)];
          var count = 0;
          foreach (var book in await books) {
            count++;
            if (book != null) {
              var titleKey = book.GetReferenceKey(titleField);
              if (titleKey != null) {
                PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(titleKey, titleType, session);
              }
            }
          }
          Assert.AreEqual(2, count);
        }
      }
    }

    [Test]
    public void StructureFieldsPrefetchTest()
    {
      PrefetchTestHelper.CreateOfferContainer(Domain, out var containerKey, out var book0Key,
        out var bookShop0Key, out var book1Key, out var bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var containers = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(containerKey))
          .Prefetch(oc => oc.RealOffer.Book)
          .Prefetch(oc => oc.IntermediateOffer.RealOffer.BookShop);
        foreach (var key in containers) {
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book0Key, book0Key.TypeInfo, session);
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(bookShop1Key, bookShop1Key.TypeInfo, session);
        }
      }
    }

    [Test]
    public async Task StructureFieldsPrefetchAsyncTest()
    {
      PrefetchTestHelper.CreateOfferContainer(Domain, out var containerKey, out var book0Key,
        out var bookShop0Key, out var book1Key, out var bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var containers = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(containerKey))
          .Prefetch(oc => oc.RealOffer.Book)
          .Prefetch(oc => oc.IntermediateOffer.RealOffer.BookShop).AsAsync();
        foreach (var key in await containers) {
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(book0Key, book0Key.TypeInfo, session);
          PrefetchTestHelper.AssertOnlyDefaultColumnsAreLoaded(bookShop1Key, bookShop1Key.TypeInfo, session);
        }
      }
    }

    [Test]
    public void StructurePrefetchTest()
    {
      PrefetchTestHelper.CreateOfferContainer(Domain, out var containerKey, out var book0Key,
        out var bookShop0Key, out var book1Key, out var bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var containers = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(containerKey))
          .Prefetch(oc => oc.IntermediateOffer);
        foreach (var key in containers) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(containerKey, containerKey.TypeInfo, session,
            field => PrefetchTestHelper.IsFieldToBeLoadedByDefault(field) || field.Name.StartsWith("IntermediateOffer"));
        }
      }
    }

    [Test]
    public async Task StructurePefetchAsyncTest()
    {
      PrefetchTestHelper.CreateOfferContainer(Domain, out var containerKey, out var book0Key,
        out var bookShop0Key, out var book1Key, out var bookShop1Key);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var containers = session.Query.Many<Model.OfferContainer>(EnumerableUtils.One(containerKey))
          .Prefetch(oc => oc.IntermediateOffer).AsAsync();
        foreach (var key in await containers) {
          PrefetchTestHelper.AssertOnlySpecifiedColumnsAreLoaded(containerKey, containerKey.TypeInfo, session,
            field => PrefetchTestHelper.IsFieldToBeLoadedByDefault(field) || field.Name.StartsWith("IntermediateOffer"));
        }
      }
    }

    private void RemoveAllBooks()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        foreach (var book in session.Query.All<Model.Book>()) {
          book.Remove();
        }
        tx.Complete();
      }
    }

    private static EntitySetState GetFullyLoadedEntitySet(Session session, Key key,
      FieldInfo employeesField)
    {
      Assert.IsTrue(session.Handler.LookupState(key, employeesField, out var entitySetState));
      Assert.IsTrue(entitySetState.IsFullyLoaded);
      return entitySetState;
    }
  }
}