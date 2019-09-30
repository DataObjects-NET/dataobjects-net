// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class TargetField
  {
    public TargetField(string name, object value)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException("Parameter 'name' cannot be null or empty.");

      this.Name = name;
      this.Value = value;
    }

    public string Name { get; private set; }

    public object Value { get; private set; }
  }

  [Category("Linq")]
  [TestFixture]
  public class WhereTest : ChinookDOModelTest
  {
    private Key albumOutOfExileKey;
    private Key mediaTypeFirstKey;

    [OneTimeSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          albumOutOfExileKey = session.Query.All<Album>().Single(a => a.Title=="Out Of Exile").Key;
          mediaTypeFirstKey = session.Query.All<MediaType>().First().Key;
        }
      }
    }

    [Test]
    public void IndexerTest()
    {
      var query = Session.Query.All<Invoice>();
      var kvp = new Pair<string, object>("Customer", Session.Query.All<Customer>().First());
      query = query.Where(invoice => invoice[kvp.First]==kvp.Second);
      var r = query.ToList();
    }

    [Test]
    public void IndexerNullTest()
    {
      var query = Session.Query.All<Invoice>();
      var parameters = Session.Query.All<Customer>().Take(1).Select(c => new TargetField("Customer", (Customer)null)).ToList();
      foreach (var item in parameters) {
        var field = item; // This is important to use local variable
        query = query.Where(invoice => invoice[field.Name]==field.Value);
      }
      var result = query.ToList<Invoice>();
      }

    [Test]
    public void ReferenceFieldConditionTest()
    {
      var result = Session.Query.All<Invoice>()
        .Where(invoice => invoice.Customer!=null)
        .ToList();

//      var expected = Session.Query.All<Invoice>()
//        .AsEnumerable()
//        .First(invoice => invoice.Customer!=null);
//      Assert.AreEqual(expected.Customer, result.Customer);
    }

    [Test]
    public void IndexerSimpleFieldTest()
    {
      object freight = Session.Query
        .All<Invoice>()
        .First(invoice => invoice.Commission > 0.2m)
        .Commission;
      var result = Session.Query
        .All<Invoice>()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice["Commission"]==freight)
        .ToList();
      var expected = Session.Query
        .All<Invoice>()
        .AsEnumerable()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice.Commission==(decimal?) freight)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void IndexerStructureTest()
    {
      object address = Session.Query
        .All<Customer>()
        .First(customer => customer.Address.City.Length > 0)
        .Address;
      var result = Session.Query
        .All<Customer>()
        .OrderBy(customer => customer.CustomerId)
        .Where(customer => customer["Address"]==address)
        .ToList();
#pragma warning disable 252,253
      var expected = Session.Query
        .All<Customer>()
        .AsEnumerable()
        .OrderBy(customer => customer.CustomerId)
        .Where(customer => customer.Address==address)
        .ToList();
#pragma warning restore 252,253
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void IndexerEntity1Test()
    {
      object customer = Session.Query
        .All<Invoice>()
        .First(invoice => invoice.Customer!=null)
        .Customer;
      var result = Session.Query
        .All<Invoice>()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice["Customer"]==customer)
        .ToList();
      var expected = Session.Query
        .All<Invoice>()
        .AsEnumerable()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice.Customer==customer)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void EntitySubqueryTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Invoice>()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice.Customer==
          Session.Query.All<Invoice>()
            .OrderBy(invoice2 => invoice2.Customer.CustomerId)
            .First(invoice2 => invoice2.Customer!=null)
            .Customer)
        .ToList();
      var expected = Session.Query.All<Invoice>()
        .AsEnumerable()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice.Customer==
          Session.Query.All<Invoice>()
            .OrderBy(invoice2 => invoice2.Customer.CustomerId)
            .First(invoice2 => invoice2.Customer!=null)
            .Customer)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void EntitySubqueryIndexerTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Invoice>()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice.Customer==
          Session.Query.All<Invoice>()
            .OrderBy(invoice2 => invoice2.Customer.CustomerId)
            .First(invoice2 => invoice2["Customer"]!=null)
            .Customer)
        .ToList();
      var expected = Session.Query.All<Invoice>()
        .AsEnumerable()
        .OrderBy(invoice => invoice.InvoiceId)
        .Where(invoice => invoice.Customer==
          Session.Query.All<Invoice>()
            .OrderBy(invoice2 => invoice2.Customer.CustomerId)
            .First(invoice2 => invoice2.Customer!=null)
            .Customer)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void ParameterTest()
    {
      var album = Session.Query.All<Album>().First();
      var result = Session.Query.All<Track>().Where(p => p.Album==album);
      QueryDumper.Dump(result);
    }

    [Test]
    public void MultipleConditionTest()
    {
      var customerCompanies = Session.Query.All<Customer>().Select(c => c.Company).Where(cn => cn.StartsWith("A") || cn.StartsWith("B")).ToList();
      Assert.That(customerCompanies.Count, Is.EqualTo(2));
      Assert.That(customerCompanies.All(c => c.StartsWith("A") || c.StartsWith("B")));
    }

    [Test]
    public void AnonymousTest()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.Company, first.LastName};
      var result = Session.Query.All<Customer>().Select(c => new {c.Company, c.LastName}).Take(10).Where(x => x==p).ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0].Company, Is.EqualTo(p.Company));
      Assert.That(result[0].LastName, Is.EqualTo(p.LastName));
    }


    [Test]
    public void AnonymousTest2()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.Company, first.LastName};
      var result = Session.Query.All<Customer>().Select(c => new {c.Company, c.LastName}).Take(10).Where(x => p==x).ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0].Company, Is.EqualTo(p.Company));
      Assert.That(result[0].LastName, Is.EqualTo(p.LastName));
    }

    [Test]
    public void AnonymousWithParameterTest()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.Company, first.LastName};
      var result = Session.Query.All<Customer>().Where(c => new {c.Company, c.LastName}==p).ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0].Company, Is.EqualTo(p.Company));
      Assert.That(result[0].LastName, Is.EqualTo(p.LastName));
    }

    [Test]
    public void AnonymousWithParameter2Test()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.Company, first.LastName};
      var k = new {InternalFiled = p};
      var result = Session.Query.All<Customer>().Where(c => new {c.Company, c.LastName}==k.InternalFiled).ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0].Company, Is.EqualTo(p.Company));
      Assert.That(result[0].LastName, Is.EqualTo(p.LastName));
    }


    [Test]
    public void AnonymousWithParameter3Test()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.Company, first.LastName};
      var k = new {InternalFiled = p};
      var result = Session.Query.All<Customer>().Where(c => new {X = new {c.Company, c.LastName}}.X==k.InternalFiled).ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0].Company, Is.EqualTo(p.Company));
      Assert.That(result[0].LastName, Is.EqualTo(p.LastName));
    }

    [Test]
    public void Anonymous2Test2()
    {
      var result = Session.Query.All<Customer>().Where(c => new {c.Company}.Company== "JetBrains s.r.o.").ToList();
      Assert.That(result.Count, Is.EqualTo(1));
      Assert.That(result[0].Company, Is.EqualTo("JetBrains s.r.o."));
    }

    [Test]
    public void Anonymous3Test()
    {
      var expectedResultCount = Session.Query.All<Customer>().Count();
      var result = Session.Query.All<Customer>().Where(c => new {c.FirstName, c.LastName}==new {c.FirstName, c.LastName}).ToList();
      Assert.That(result.Count, Is.EqualTo(expectedResultCount));
    }

    [Test]
    public void Anonymous4Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      Customer first = Session.Query.All<Customer>().First();
      Customer second = Session.Query.All<Customer>().Skip(1).First();
      var p = new {first.Address.PostalCode, first.Address.City};
      var l = new {second.Address.PostalCode, second.Address.City};
      var result = Session.Query.All<Customer>().Where(c => l==p).ToList();
      Assert.That(result, Is.Empty);
    }


    [Test]
    public void ColumnTest()
    {
      var albums = Session.Query.All<Album>();
      var album = albums.Where(a => a.Title=="Plays Metallica By Four Cellos").FirstOrDefault();
      Assert.IsNotNull(album);
      Assert.AreEqual("Plays Metallica By Four Cellos", album.Title);
    }

    [Test]
    public void CalculatedTest()
    {
      var expected = Session.Query.All<Invoice>().ToList().Where(i => i.Total * i.Commission >= 10).ToList();
      var actual = Session.Query.All<Invoice>().ToList().Where(i => i.Total * i.Commission >= 10).ToList();
      Assert.That(expected.Count, Is.GreaterThan(0));
      Assert.AreEqual(expected.Count, actual.Count);
    }

    [Test]
    public void StructureTest()
    {
      var employees = Session.Query.All<Employee>();
      var employee = employees.Where(e => e.Address.State=="AB").FirstOrDefault();
      Assert.IsNotNull(employee);
      Assert.AreEqual("AB", employee.Address.State);
    }

    [Test]
    public void IdTest()
    {
      var albums = Session.Query.All<Album>();
      var album = albums.Where(a => a.AlbumId==albumOutOfExileKey.Value.GetValue<int>(0)).FirstOrDefault();
      Assert.IsNotNull(album);
      Assert.AreEqual("Out Of Exile", album.Title);
    }

    [Test]
    public void KeyTest()
    {
      var albums = Session.Query.All<Album>();
      var key = Key.Create<Album>(Domain, albumOutOfExileKey.Value);
      var album = albums.Where(a => a.Key==key).FirstOrDefault();
      Assert.IsNotNull(album);
      Assert.AreEqual("Out Of Exile", album.Title);
    }

    [Test]
    public void InstanceTest()
    {
      var albumOutOfExile = Session.Query.SingleOrDefault<Album>(albumOutOfExileKey);
      var albums = Session.Query.All<Album>();
      var album = albums.Where(a => a==albumOutOfExile).FirstOrDefault();
      Assert.IsNotNull(album);
      Assert.AreEqual("Out Of Exile", album.Title);
    }

    [Test]
    public void ForeignKeyTest()
    {
      var album = Session.Query.SingleOrDefault<Album>(albumOutOfExileKey);
      var tracks = Session.Query.All<Track>();
      var track = tracks.Where(t => t.Album.Key==album.Key).FirstOrDefault();
      Assert.IsNotNull(track);
      Assert.AreEqual("Out Of Exile", track.Album.Title);
    }

    [Test]
    public void ForeignIDTest()
    {
      var album = Session.Query.SingleOrDefault<Album>(albumOutOfExileKey);
      var products = Session.Query.All<Track>();
      var product = products.Where(t => t.Album.AlbumId==album.AlbumId).FirstOrDefault();
      Assert.IsNotNull(product);
      Assert.AreEqual("Out Of Exile", product.Album.Title);
    }

    [Test]
    public void ForeignInstanceTest()
    {
      var album20 = Session.Query.SingleOrDefault<Album>(albumOutOfExileKey);
      var tracks = Session.Query.All<Track>();
      var track = tracks.Where(t => t.Album==album20).FirstOrDefault();
      Assert.IsNotNull(track);
      Assert.AreEqual("Out Of Exile", track.Album.Title);
    }

    [Test]
    public void ForeignPropertyTest()
    {
      var tracks = Session.Query.All<Track>();
      var trackFormAlbum = tracks.Where(t => t.Album.Title=="Out Of Exile").FirstOrDefault();
      Assert.IsNotNull(trackFormAlbum);
      Assert.AreEqual("Out Of Exile", trackFormAlbum.Album.Title);
      trackFormAlbum =
        tracks.Where(
          t =>
            t.Album.Title=="Out Of Exile" && t.MediaType.Key==mediaTypeFirstKey).FirstOrDefault();
      Assert.IsNotNull(trackFormAlbum);
      Assert.AreEqual("Out Of Exile", trackFormAlbum.Album.Title);
    }

    [Test]
    public void CoalesceTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => (c.Address.State ?? "Australia")=="Australia").FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => (c.Address.State ?? c.Address.PostalCode ?? "Australia")=="Australia").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void ConditionalTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => (i.Customer.LastName=="Ramos" ? 1000 : 0)==1000).FirstOrDefault();
      Assert.IsNotNull(invoice);
      invoice = invoices
        .Where(i => (i.Customer.LastName=="Ramos" ? 1000 : i.Customer.LastName== "Philips" ? 2000 : 0)==1000).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void StringLengthTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Length==7).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringStartsWithLiteralTest()
    {
      var mediaTypes = Session.Query.All<MediaType>();
      var mpegType = mediaTypes.Where(c => c.Name.StartsWith("M")).FirstOrDefault();
      Assert.IsNotNull(mpegType);
    }

    [Test]
    public void StringStartsWithColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.LastName.StartsWith(c.LastName)).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringEndsWithLiteralTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.LastName.EndsWith("s")).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringEndsWithColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.LastName.EndsWith(c.LastName)).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringContainsLiteralTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.LastName.Contains("and")).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringContainsColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.LastName.Contains(c.LastName)).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringConcatImplicitArgsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.LastName + "X"=="X").FirstOrDefault();
      Assert.IsNull(customer);
    }

    [Test]
    public void StringConcatExplicitNArgTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Concat(c.LastName, "X")=="X").FirstOrDefault();
      Assert.IsNull(customer);
      customer = customers.Where(c => string.Concat(c.LastName, "X", c.Address.Country)=="X").FirstOrDefault();
      Assert.IsNull(customer);
    }

    [Test]
    public void StringIsNullOrEmptyTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.IsNullOrEmpty(c.Address.State)).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringToUpperTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.ToUpper()=="ORLANDO").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringToLowerTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.ToLower()=="orlando").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringReplaceTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Replace("land", "earth")=="Oreartho").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringReplaceCharsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Replace('d', 'g')=="Orlango").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringSubstringTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Substring(0, 4)=="Orla").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringSubstringNoLengthTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Substring(4)=="ndo").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringRemoveTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Remove(1, 2)=="Oando").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringRemoveNoCountTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Remove(2)=="Or").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringIndexOfTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support IndexOf()");

      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.IndexOf("land")==2).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringIndexOfCharTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support IndexOf()");

      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.IndexOf('l')==2).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringTrimTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Trim()=="Orlando").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringToStringTest()
    {
      // just to prove this is a no op
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.ToString()=="Orlando").FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void DateTimeConstructYMDTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate >= new DateTime(i.InvoiceDate.Year, 1, 1)).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeConstructYMDHMSTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate >= new DateTime(i.InvoiceDate.Year, 1, 1, 10, 25, 55)).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => ((DateTime?)i.InvoiceDate) < ((DateTime?)new DateTime(2010, 12, 31))).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeDayTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Day==5).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeMonthTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Month==12).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeYearTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Year==2010).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeHourTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Hour==0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeMinuteTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Minute==0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeSecond()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Second==0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeMillisecondTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.Millisecond==0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeDayOfWeekTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.DayOfWeek==DayOfWeek.Friday).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DateTimeDayOfYearTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceDate.DayOfYear==360).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathAbsTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Abs(i.InvoiceId)==10 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathAcosTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Acos()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Acos(Math.Sin(i.InvoiceId))==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathAsinTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Asin()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Asin(Math.Cos(i.InvoiceId))==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathAtanTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Atan()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Atan(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
      invoice = invoices.Where(i => Math.Atan2(i.InvoiceId, 3)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathCosTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Cos(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathSinTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Sin()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Sin(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathTanTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Tan()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Tan(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathExpTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Exp(i.InvoiceId < 1000 ? 1 : 2)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathLogTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Log(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathLog10Test()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Log10(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathSqrtTest()
    {
Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Sqrt()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Sqrt(i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathCeilingTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Ceiling((double) i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathFloorTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Floor((double) i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathPowTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Pow()");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Pow(i.InvoiceId < 1000 ? 1 : 2, 3)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathRoundDefaultTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Round((decimal) i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathRoundToPlaceTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Pow() which is used in Round() translation");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Round((decimal) i.InvoiceId, 2)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void MathTruncateTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => Math.Truncate((double) i.InvoiceId)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void StringLessThanTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.LessThan("Orlando")).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringLessThanOrEqualsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.LessThanOrEqual("Orlando")).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringGreaterThanTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.GreaterThan("Orlando")).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringGreaterThanOrEqualsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.GreaterThanOrEqual("Orlando")).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToLTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Orlando") < 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToLETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Orlando") <= 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToGTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Orlando") > 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToGETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Orlando") >= 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToEQTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Orlando")==0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }


    [Test]
    public void StringCompareToNETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Orlando")!=0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareLTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Orlando") < 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareLETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Orlando") <= 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareGTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Orlando") > 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareGETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Orlando") >= 0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareEQTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Orlando")==0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareNETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Orlando")!=0).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void IntCompareToTest()
    {
      // prove that x.CompareTo(y) works for types other than string
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId.CompareTo(1000)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalCompareTest()
    {
      // prove that type.Compare(x,y) works with decimal
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Compare((decimal) i.InvoiceId, 0.0m)==0 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalAddTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Add(i.InvoiceId, 0.0m)==0.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalSubtractTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Subtract(i.InvoiceId, 0.0m)==0.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalMultiplyTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Multiply(i.InvoiceId, 1.0m)==1.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalDivideTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Divide(i.InvoiceId, 1.0m)==1.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalRemainderTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Remainder(i.InvoiceId, 1.0m)==0.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalNegateTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Negate(i.InvoiceId)==1.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalCeilingTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Ceiling(i.InvoiceId)==0.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalFloorTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Floor(i.InvoiceId)==0.0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalRoundDefaultTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Round(i.InvoiceId)==0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalRoundPlacesTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Pow() which is used in Round() translation");

      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Round(i.InvoiceId, 2)==0.00m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalTruncateTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => decimal.Truncate(i.InvoiceId)==0m || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void DecimalGTTest()
    {
      // prove that decimals are treated normally with respect to normal comparison operators
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => ((decimal) i.InvoiceId) > 0.0m).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void FkCompareTest()
    {
      var employee = Session.Query.All<Employee>().Where(e => e.ReportsToManager.EmployeeId > 20).FirstOrDefault();
      Assert.IsNotNull(employee);
    }

    [Test]
    public void IntLessThanTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId < 0).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntLessThanOrEqualTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId <= 0).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntGreaterThanTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntGreaterThanOrEqualTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId >= 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntEqualTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId==0).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntNotEqualTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId!=0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntAddTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId + 0==0).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntSubtractTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId - 0==0).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntMultiplyTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId * 1==1 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntDivideTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId / 1==1 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntModuloTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId % 1==0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntBitwiseAndTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => (i.InvoiceId & 1)==0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void IntBitwiseOrTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => (i.InvoiceId | 1)==1 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }


    [Test]
    public void IntBitwiseExclusiveOrTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => (i.InvoiceId ^ 1)==1).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntBitwiseNotTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => ~i.InvoiceId==0).FirstOrDefault();
      Assert.IsNull(invoice);
    }

    [Test]
    public void IntNegateTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => -i.InvoiceId==-1 || i.InvoiceId > 0).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void AndTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId > 0 && i.InvoiceId < 4500).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void OrTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => i.InvoiceId < 5 || i.InvoiceId > 10).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void NotTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var invoice = invoices.Where(i => !(i.InvoiceId==0)).FirstOrDefault();
      Assert.IsNotNull(invoice);
    }

    [Test]
    public void EqualsNullTest()
    {
      //  TODO: Check IsNull or Equals(null)
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City!=null).FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => !c.Address.State.Equals(null)).FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => c!=null).FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => !c.Equals(null)).FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => c.Address!=null).FirstOrDefault();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => !c.Address.Equals(null)).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void EqualNullReverseTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => null!=c.Address.City).FirstOrDefault();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void TimeSpanTest()
    {
      var maxProcessingTime = new TimeSpan(5, 0, 0, 0);
      Session.Query.All<Invoice>().Where(i => i.ProcessingTime > maxProcessingTime).ToList();
    }

    [Test]
    public void NonPersistentFieldTest()
    {
      var result = from e in Session.Query.All<Employee>() where e.FullName!=null select e;
      Assert.Throws<QueryTranslationException>(() => result.ToList());
    }

    [Test]
    public void JoinTest()
    {
      var actual = from customer in Session.Query.All<Customer>()
        join invoice in Session.Query.All<Invoice>() on customer equals invoice.Customer
        where invoice.Commission > 30
        orderby new {customer, invoice}
        select new {customer, invoice};
      var list = actual.ToList();
      var expected = from customer in Session.Query.All<Customer>().ToList()
        join invoice in Session.Query.All<Invoice>().ToList() on customer equals invoice.Customer
        where invoice.Commission > 30
        orderby customer.CustomerId , invoice.InvoiceId
        select new {customer, invoice};
      Assert.IsTrue(expected.SequenceEqual(list));
    }

    [Test]
    public void ApplyTest()
    {
      var actual = Session.Query.All<Customer>()
        .Where(customer => customer.Invoices.Any(i => i.Commission > 0.30m));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable() // AG: Just to remeber about it.
        .Where(customer => customer.Invoices.Any(i => i.Commission > 0.30m));
      Assert.IsTrue(expected.SequenceEqual(actual));
    }
  }
}