// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.12

using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Orm.Providers;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SelectTest : ChinookDOModelTest
  {
    public class InstanceTestClass
    {
      public string Current
      {
        get { return ""; }
      }
    }

    public static class StaticTestClass
    {
      public static InstanceTestClass Instance
      {
        get { return new InstanceTestClass(); }
      }
    }

    public class Context
    {
      public IQueryable<Invoice> Invoices
      {
        get { return Session.Demand().Query.All<Invoice>(); }
      }

      public IQueryable<Customer> Customers
      {
        get { return Session.Demand().Query.All<Customer>(); }
      }
    }

    [Test]
    public void IndexerSimpleFieldTest()
    {
      var result = Session.Query
        .All<Customer>()
        .OrderBy(customer => customer.CustomerId)
        .Select(customer => customer["Phone"])
        .AsEnumerable();
      var expected = Session.Query
        .All<Customer>()
        .AsEnumerable()
        .OrderBy(customer => customer.CustomerId)
        .Select(customer => customer["Phone"]);
      Assert.IsTrue(expected.SequenceEqual(result));

      var qr = Session.Query.All<Customer>();

      var filter = new Dictionary<string, object> {{"Phone", "Test 718"}};

      foreach (var item in filter) {
          var pair = item; // This is important to use local variable
          qr = qr.Where(customer => customer[pair.Key]==pair.Value);
      }

       var list = qr.ToList();
    }

    [Test]
    public void IndexerEntityTest()
    {
      var result = Session.Query
        .All<Invoice>()
        .OrderBy(invoice => invoice.InvoiceId)
        .Select(invoice => invoice["Customer"])
        .AsEnumerable();
      var expected = Session.Query
        .All<Invoice>()
        .AsEnumerable()
        .OrderBy(invoice => invoice.InvoiceId)
        .Select(invoice => invoice["Customer"]);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void IndexerStructureTest()
    {
      var result = Session.Query
        .All<Customer>()
        .OrderBy(customer => customer.CustomerId)
        .Select(customer => customer["Address"])
        .AsEnumerable();
      var expected = Session.Query
        .All<Customer>()
        .AsEnumerable()
        .OrderBy(customer => customer.CustomerId)
        .Select(customer => customer["Address"]);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void IndexerError1Test()
    {
      Assert.Throws<QueryTranslationException>(() => {
        var result = Session.Query
          .All<Customer>()
          .Select(customer => customer["Ph1one"])
          .ToList();
      });
    }

    [Test]
    public void IndexerError2Test()
    {
      Assert.Throws<QueryTranslationException>(() => {
        var result = Session.Query
          .All<Invoice>()
          .Where(invoice => invoice["Commission"]==invoice["Id"])
          .ToList();
      });
    }


    [Test]
    public void StaticPropertyTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var customers = Session.Query.All<Customer>()
        .Where(customer =>
          customer
            .Invoices
            .Select(invoices => invoices.BillingAddress.State)
            .FirstOrDefault()==StaticTestClass.Instance.Current);
      QueryDumper.Dump(customers);
    }

    [Test]
    public void SelectEmployeeTest()
    {
      var result = Session.Query.All<Employee>();
      var list = result.ToList();
    }

    [Test]
    public void SelectForeignKeyTest()
    {
      var result = Session.Query.All<Track>().Select(t => t.Album.AlbumId);
      var list = result.ToList();
    }

    [Test]
    public void SelectForeignFieldTest()
    {
      var result = Session.Query.All<Track>().Select(t => t.Album.Title);
      var list = result.ToList();
    }

    [Test]
    public void SelectUsingContextTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe | StorageProvider.Oracle);
      var expectedCount = Session.Query.All<Invoice>().Count();
      var context = new Context();
      var actualCount = context.Invoices.Count();
      var list = context.Invoices.ToList();
      Assert.AreEqual(expectedCount, actualCount);
      Assert.AreEqual(expectedCount, list.Count);

      var result = context.Customers.Where(c => context.Invoices.Count(i => i.Customer==c) > 5);
      Assert.Greater(result.ToList().Count, 0);
    }

    [Test]
    public void NullJoinTest()
    {
      Session.Query.All<Track>().First().Album = null; // Set one region reference to NULL
      Session.Current.SaveChanges();

      var tracks = Session.Query.All<Track>();

      var result = tracks.Select(t => t.Album.AlbumId);

      var expectedCount = tracks.ToList().Count();
      var actualCount = result.Count();
      Assert.AreEqual(expectedCount, actualCount);
    }

    [Test]
    public void AnonymousEntityTest()
    {
      var result = Session.Query.All<Album>()
        .Select(album => new {Album = album})
        .Select(a => a.Album);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousEntityKeyTest()
    {
      var result = Session.Query.All<Album>()
        .Select(album => new {Album = album})
        .Select(a => a.Album.Key);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousEntityFieldTest()
    {
      var result = Session.Query.All<Album>()
        .Select(album => new {Album = album})
        .Select(a => a.Album.Title);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousEntityKeyFieldTest()
    {
      var result = Session.Query.All<Album>()
        .Select(album => new {Album = album})
        .Select(a => a.Album.AlbumId);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OutOfHierarchy()
    {
     Assert.Throws<QueryTranslationException>( () => { Assert.Greater(Session.Query.All<Person>().Count(), 0); });
    }

    [Test]
    public void SimpleSelectTest()
    {
      var result = Session.Query.All<Invoice>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void SimpleConstantTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from t in tracks
        select 0;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(0, i);
    }

    [Test]
    public void AnonymousColumn()
    {
      var tracks = Session.Query.All<Track>().Select(t => new {t.Name}.Name);
      var list = tracks.ToList();
    }

    [Test]
    public void AnonymousWithCalculatedColumnsTest()
    {
      var result = Session.Query.All<Customer>().Select(c =>
        new {
          n1 = c.FirstName.Length + c.LastName.Length,
          n2 = c.LastName.Length + c.FirstName.Length
        });
      result = result.Where(i => i.n1 > 10);
      result.ToList();
    }

    [Test]
    public void AnonymousParameterColumn()
    {
      var param = new {ProductName = "name"};
      var tracks = Session.Query.All<Track>().Select(t => param.ProductName);
      QueryDumper.Dump(tracks);
    }


    [Test]
    public void NewPairTest()
    {
      var method = MethodInfo.GetCurrentMethod().Name;
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select new {
            Value = new Pair<string>(t.Name, method),
            Method = method,
            t.Name
          }
        orderby r.Name
        where r.Method==method
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(method, i.Method);
    }

    [Test]
    public void ConstantTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select 0
        where r==0
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(0, i);
    }

    [Test]
    public void ConstantNullStringTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from t in tracks
      select (string) null;
      var list = result.ToList();
      foreach (var s in list)
        Assert.AreEqual(null, s);
    }

    [Test]
    public void LocalTest()
    {
      int x = 10;
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select x
        where r==x
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(10, i);
      x = 20;
      list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(20, i);
    }

    [Test]
    public void ColumnTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select t.Name
        where r!=null
        select r;
      var list = result.ToList();
      foreach (var s in list)
        Assert.IsNotNull(s);
    }

    [Test]
    public void CalculatedColumnTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from r in
        from t in tracks
        select t.UnitPrice * t.UnitPrice
      where r > 0
      select r;
      var list = result.ToList();
      var checkList = tracks.ToList().Select(t => t.UnitPrice * t.UnitPrice).ToList();
      list.SequenceEqual(checkList);
    }

    [Test]
    public void KeyTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = tracks
        .Select(t => t.Key)
        .Where(r => r!=null);
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var k in list) {
        Assert.IsNotNull(k);
        var t = Session.Query.SingleOrDefault<Track>(k);
        Assert.IsNotNull(t);
      }
    }

    [Test]
    public void KeySimpleTest()
    {
      var result = Session.Query
        .All<Track>()
        .Select(t => t.Key);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from t in tracks
      select new {t.Name, t.UnitPrice};
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnonymousEmptyTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from t in tracks
      select new {};
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnonymousCalculatedTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select new {t.Name, TotalPriceInStock = t.UnitPrice * t.UnitPrice}
        where r.TotalPriceInStock > 0
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void JoinedEntityColumnTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from t in tracks
      select t.Album.Title;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void JoinedEntityTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select t.Album
        where r.Title!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void StructureColumnTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var result = from i in invoices
      select i.Customer.Address;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void StructureTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var result = from a in (
        from i in invoices
        select i.Customer.Address)
      where a.City!=null
      select a.StreetAddress;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void EntitySetTest()
    {
      var suppliers = Session.Query.All<Customer>();
      var result = from s in suppliers
      select s.Invoices;
      var list = result.ToList();
    }

    [Test]
    public void AnonymousWithEntityTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select new {t.Name, Track = t}
        where r.Track!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void AnonymousNestedTest()
    {
      var tracks = Session.Query.All<Track>();
      var result =
        from r in
          from t in tracks
          select new {t, Desc = new {t.Name, t.UnitPrice}}
        where r.Desc.Name!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void NestedQueryTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from pd in
        from t in tracks
        select new {ProductKey = t.Key, t.Name, TotalPrice = t.UnitPrice * t.UnitPrice}
      where pd.TotalPrice > 100
      select new {PKey = pd.ProductKey, pd.Name, Total = pd.TotalPrice};

      var list = result.ToList();
    }

    [Test]
    public void NestedQueryWithStructuresTest()
    {
      var invoices = Session.Query.All<Invoice>();
      var result =
        from a in
          from id in
            from i in invoices
            select new {ProductKey = i.Key, SupplierAddress = i.Customer.Address}
          select new {PKey = id.ProductKey, id.SupplierAddress, SupplierCity = id.SupplierAddress.City}
        select new {a.PKey, a.SupplierAddress, a.SupplierCity};
      var list = result.ToList();
    }


    [Test]
    public void NestedQueryWithEntitiesTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from pd in
        from t in tracks
        select new {ProductKey = t.Key, Product = t}
      select new {PKey = pd.ProductKey, pd.Product};

      var list = result.ToList();
    }

    [Test]
    public void NestedQueryWithAnonymousTest()
    {
      var tracks = Session.Query.All<Track>();
      var result = from pd in
        from t in tracks
        select new {ProductKey = t.Key, Product = new {Entity = new {t}, Name = t.Name}}
      select new {PKey = pd.ProductKey, pd.Product.Name, A = pd, AProduct = pd.Product, AEntity = pd.Product.Entity};

      var list = result.ToList();
    }

    [Test]
    public void SelectEnumTest()
    {
      var result = from i in Session.Query.All<Invoice>() select i.InvoiceDate.DayOfWeek;
      result.ToList();
    }

    [Test]
    public void SelectAnonymousEnumTest()
    {
      var result = from i in Session.Query.All<Invoice>() select new {i.InvoiceDate.DayOfWeek};
      result.ToList();
    }

    [Test]
    public void SelectEnumFieldTest()
    {
      var result = from t in Session.Query.All<AudioTrack>() select t.MediaFormat;
      foreach (var t in result)
        Assert.AreEqual(t, MediaFormat.Audio);
    }

    [Test]
    public void SelectAnonymousEnumFieldTest()
    {
      var result = from t in Session.Query.All<AudioTrack>() select new {t.MediaFormat};
      foreach (var t in result)
        Assert.AreEqual(t.MediaFormat, MediaFormat.Audio);
    }

    [Test]
    public void SelectCharTest()
    {
      var result = from c in Session.Query.All<Customer>() select c.LastName[0];
      var list = result.ToList();
    }

    [Test]
    public void SelectByteArrayLengthTest()
    {
      var categories = Session.Query.All<Track>();
      var result = from c in categories select c.Bytes.Length;
      var list = result.ToList();
    }

    [Test]
    public void SelectEqualsTest()
    {
      var customers = Session.Query.All<Customer>();
      var result = from c in customers select c.CompanyName.Equals("lalala");
      var list = result.ToList();
    }

    [Test]
    public void DoubleSelectEntitySet1Test()
    {
      IQueryable<EntitySet<Invoice>> query = Session.Query.All<Customer>().Select(c => c.Invoices).Select(c => c);
      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void DoubleSelectEntitySet2Test()
    {
      IQueryable<EntitySet<Invoice>> query = Session.Query.All<Customer>().Select(c => c).Select(c => c.Invoices);
      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void DoubleSelectEntitySet3Test()
    {
      var query = Session.Query.All<Customer>().Select(c => c.Invoices.Select(o => o));
//          var query = Session.Query.All<Customer>().Select(c => c.Orders);

      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void NestedAnonymousTest()
    {
      var result = Session.Query.All<Customer>()
        .Select(c => new {c})
        .Select(a1 => new {a1})
        .Select(a2 => a2.a1.c.CompanyName);
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityWithLazyLoadFieldTest()
    {
      var category = Session.Query.All<Track>().Where(c => c.Bytes!=null).First();
      int columnIndex = Domain.Model.Types[typeof (Track)].Fields["Bytes"].MappingInfo.Offset;
      Assert.IsFalse(category.State.Tuple.GetFieldState(columnIndex).IsAvailable());
    }

    [Test]
    public void AnonymousSelectTest()
    {
      var result = Session.Query.All<Invoice>()
        .Select(i => new {i.InvoiceDate, i.Commission})
        .Select(g => g.InvoiceDate);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectJustOuterParameterTest()
    {
      var result = Session.Query.All<Customer>().Select(c => Session.Query.All<Invoice>().Select(s => c));
      foreach (var i in result)
        i.ToList();
    }

    [Test]
    public void NonPersistentFieldTest()
    {
      var result = from e in Session.Query.All<Employee>() select e.FullName;
      Assert.Throws<QueryTranslationException>(() => result.ToList());
    }

    [Test]
    public void SelectBigMulTest()
    {
      var result =
        from invoice in Session.Query.All<Invoice>()
        select Math.BigMul(invoice.InvoiceId, invoice.DesignatedEmployee.EmployeeId);
      result.ToList();
    }

    [Test]
    public void SelectSignTest()
    {
      var result =
        from invoice in Session.Query.All<Invoice>()
        where invoice.InvoiceId > 0 && invoice.InvoiceId < 50
        let values = new {
          Byte = (sbyte) invoice.InvoiceId,
          Short = (short) invoice.InvoiceId,
          Int = invoice.InvoiceId,
          Long = (long) invoice.InvoiceId,
          Decimal = (decimal) invoice.InvoiceId,
          Float = (float) invoice.InvoiceId,
          Double = (double) invoice.InvoiceId,
        }
        select new {
          ByteSign = Math.Sign(values.Byte),
          ShortSign = Math.Sign(values.Short),
          IntSign = Math.Sign(values.Int),
          LongSign = Math.Sign(values.Long),
          DecimalSign = Math.Sign(values.Decimal),
          FloatSign = Math.Sign(values.Float),
          DoubleSign = Math.Sign(values.Double)
        };
      foreach (var item in result) {
        Assert.AreEqual(1, item.ByteSign);
        Assert.AreEqual(1, item.ShortSign);
        Assert.AreEqual(1, item.IntSign);
        Assert.AreEqual(1, item.LongSign);
        Assert.AreEqual(1, item.DecimalSign);
        Assert.AreEqual(1, item.FloatSign);
        Assert.AreEqual(1, item.DoubleSign);
      }
    }

    [Test]
    public void SelectStringIndexerTest()
    {
      var result =
        Session.Query.All<Customer>()
          .Select(c => new {
            String = c.CustomerId,
            Char0 = c.Email[0],
            Char1 = c.Email[1],
            Char2 = c.Email[2],
            Char3 = c.Email[3],
            Char4 = c.Email[4],
          })
          .ToArray()
          .OrderBy(item => item.String)
          .ToArray();
      var expected =
        Session.Query.All<Customer>()
          .ToArray()
          .Select(c => new {
            String = c.CustomerId,
            Char0 = c.Email[0],
            Char1 = c.Email[1],
            Char2 = c.Email[2],
            Char3 = c.Email[3],
            Char4 = c.Email[4],
          })
          .OrderBy(item => item.String)
          .ToArray();
      Assert.AreEqual(expected.Length, result.Length);
      for (int i = 0; i < expected.Length; i++) {
        Assert.AreEqual(expected[0].String, result[0].String);
        Assert.AreEqual(expected[0].Char0, result[0].Char0);
        Assert.AreEqual(expected[0].Char1, result[0].Char1);
        Assert.AreEqual(expected[0].Char2, result[0].Char2);
        Assert.AreEqual(expected[0].Char3, result[0].Char3);
        Assert.AreEqual(expected[0].Char4, result[0].Char4);
      }
    }

    [Test]
    public void SelectIndexOfTest()
    {
      char _char = 'A';
      var result =
        Session.Query.All<Customer>()
          .Select(c => new {
            String = c.FirstName,
            IndexOfChar = c.FirstName.IndexOf(_char),
            IndexOfCharStart = c.FirstName.IndexOf(_char, 1),
            IndexOfCharStartCount = c.FirstName.IndexOf(_char, 1, 1),
            IndexOfString = c.FirstName.IndexOf(_char.ToString()),
            IndexOfStringStart = c.FirstName.IndexOf(_char.ToString(), 1),
            IndexOfStringStartCount = c.FirstName.IndexOf(_char.ToString(), 1, 1)
          })
          .ToArray()
          .OrderBy(item => item.String)
          .ToArray();
      var expected =
        Session.Query.All<Customer>()
          .ToArray()
          .Select(c => new {
            String = c.FirstName,
            IndexOfChar = c.FirstName.IndexOf(_char),
            IndexOfCharStart = c.FirstName.IndexOf(_char, 1),
            IndexOfCharStartCount = c.FirstName.IndexOf(_char, 1, 1),
            IndexOfString = c.FirstName.IndexOf(_char.ToString()),
            IndexOfStringStart = c.FirstName.IndexOf(_char.ToString(), 1),
            IndexOfStringStartCount = c.FirstName.IndexOf(_char.ToString(), 1, 1)
          })
          .OrderBy(item => item.String)
          .ToArray();
      Assert.AreEqual(expected.Length, result.Length);
      for (int i = 0; i < expected.Length; i++) {
        Assert.AreEqual(expected[i].String, result[i].String);
        Assert.AreEqual(expected[i].IndexOfChar, result[i].IndexOfChar);
        Assert.AreEqual(expected[i].IndexOfCharStart, result[i].IndexOfCharStart);
        Assert.AreEqual(expected[i].IndexOfCharStartCount, result[i].IndexOfCharStartCount);
        Assert.AreEqual(expected[i].IndexOfString, result[i].IndexOfString);
        Assert.AreEqual(expected[i].IndexOfStringStart, result[i].IndexOfStringStart);
        Assert.AreEqual(expected[i].IndexOfStringStartCount, result[i].IndexOfStringStartCount);
      }
    }

    [Test]
    public void SelectStringContainsTest()
    {
      var result =
        Session.Query.All<Customer>()
          .Where(c => c.FirstName.Contains('ç'))
          .OrderBy(c => c.FirstName)
          .ToArray();
      var expected =
        Session.Query.All<Customer>()
          .ToList()
          .Where(c => c.FirstName.Contains('ç'))
          .OrderBy(c => c.FirstName)
          .ToArray();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void SelectDateTimeTimeSpanTest()
    {
      var dateTime = new DateTime(2001, 1, 1, 1, 1, 1);
      var timeSpan = new TimeSpan(1, 1, 1, 1);

      var result = Session.Query.All<Customer>()
        .Select(c => new {
          CustomerId = c.CustomerId,
          DateTime = dateTime,
          TimeSpan = timeSpan
        })
        .Select(k => new {
          DateTime = k.DateTime,
          DateTimeDate = k.DateTime.Date,
          DateTimeTime = k.DateTime.TimeOfDay,
          DateTimeYear = k.DateTime.Year,
          DateTimeMonth = k.DateTime.Month,
          DateTimeDay = k.DateTime.Day,
          DateTimeHour = k.DateTime.Hour,
          DateTimeMinute = k.DateTime.Minute,
          DateTimeSecond = k.DateTime.Second,
          DateTimeDayOfYear = k.DateTime.DayOfYear,
          DateTimeDayOfWeek = k.DateTime.DayOfWeek,
          TimeSpan = k.TimeSpan,
          TimeSpanDays = k.TimeSpan.Days,
          TimeSpanHours = k.TimeSpan.Hours,
          TimeSpanMinutes = k.TimeSpan.Minutes,
          TimeSpanSeconds = k.TimeSpan.Seconds,
          TimeSpanTotalDays = k.TimeSpan.TotalDays,
          TimeSpanTotalHours = k.TimeSpan.TotalHours,
          TimeSpanTotalMinutes = k.TimeSpan.TotalMinutes,
          TimeSpanTotalSeconds = k.TimeSpan.TotalSeconds,
          TimeSpanTotalMilliSeconds = k.TimeSpan.TotalMilliseconds,
          TimeSpanTicks = k.TimeSpan.Ticks,
          TimeSpanDuration = k.TimeSpan.Duration(),
          TimeSpanFromDays = TimeSpan.FromDays(k.TimeSpan.TotalDays),
          TimeSpanFromHours = TimeSpan.FromHours(k.TimeSpan.TotalHours),
          TimeSpanFromMinutes = TimeSpan.FromMinutes(k.TimeSpan.TotalMinutes),
          TimeSpanFromSeconds = TimeSpan.FromSeconds(k.TimeSpan.TotalSeconds),
          TimeSpanFromMilliseconds = TimeSpan.FromMilliseconds(k.TimeSpan.TotalMilliseconds),
        })
        .First();
      Assert.AreEqual(dateTime, result.DateTime);
      Assert.AreEqual(dateTime.Date, result.DateTimeDate);
      Assert.AreEqual(dateTime.TimeOfDay, result.DateTimeTime);
      Assert.AreEqual(dateTime.Year, result.DateTimeYear);
      Assert.AreEqual(dateTime.Month, result.DateTimeMonth);
      Assert.AreEqual(dateTime.Day, result.DateTimeDay);
      Assert.AreEqual(dateTime.Hour, result.DateTimeHour);
      Assert.AreEqual(dateTime.Minute, result.DateTimeMinute);
      Assert.AreEqual(dateTime.Second, result.DateTimeSecond);
      Assert.AreEqual(dateTime.DayOfYear, result.DateTimeDayOfYear);
      Assert.AreEqual(dateTime.DayOfWeek, result.DateTimeDayOfWeek);
      Assert.AreEqual(timeSpan, result.TimeSpan);
      Assert.AreEqual(timeSpan.Days, result.TimeSpanDays);
      Assert.AreEqual(timeSpan.Hours, result.TimeSpanHours);
      Assert.AreEqual(timeSpan.Minutes, result.TimeSpanMinutes);
      Assert.AreEqual(timeSpan.Seconds, result.TimeSpanSeconds);
      Assert.IsTrue(Math.Abs(timeSpan.TotalDays - result.TimeSpanTotalDays) < 0.1);
      Assert.IsTrue(Math.Abs(timeSpan.TotalHours - result.TimeSpanTotalHours) < 0.1);
      Assert.IsTrue(Math.Abs(timeSpan.TotalMinutes - result.TimeSpanTotalMinutes) < 0.1);
      Assert.IsTrue(Math.Abs(timeSpan.TotalSeconds - result.TimeSpanTotalSeconds) < 0.1);
      Assert.IsTrue(Math.Abs(timeSpan.TotalMilliseconds - result.TimeSpanTotalMilliSeconds) < 0.1);
      Assert.AreEqual(timeSpan.Ticks, result.TimeSpanTicks);
      Assert.AreEqual(timeSpan.Duration(), result.TimeSpanDuration);
      Assert.AreEqual(timeSpan, result.TimeSpanFromDays);
      Assert.AreEqual(timeSpan, result.TimeSpanFromHours);
      Assert.AreEqual(timeSpan, result.TimeSpanFromMinutes);
      Assert.AreEqual(timeSpan, result.TimeSpanFromSeconds);
      Assert.AreEqual(timeSpan, result.TimeSpanFromMilliseconds);
    }

    [Test]
    public void SelectSubstringTest()
    {
      var result = Session.Query.All<Customer>().Select(c => new {
        String = c.Email,
        FromTwo = c.Email.Substring(2),
        FromThreeTakeOne = c.Email.Substring(3, 1),
      }).ToArray();
      foreach (var item in result) {
        Assert.AreEqual(item.String.Substring(2), item.FromTwo);
        Assert.AreEqual(item.String.Substring(3, 1), item.FromThreeTakeOne);
      }
    }

    [Test]
    public void ExternalPropertyCall()
    {
      Require.AllFeaturesSupported(ProviderFeatures.TemporaryTables);
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>().Select(c => Customers.Single(c2 => c2==c)).ToList();
    }

    [Test]
    public void ExternalMethodCall()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>()
        .Select(c => GetCustomers().Single(c2 => c2==c));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .Select(c => GetCustomers().AsEnumerable().Single(c2 => c2==c));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void ExternalMethodWithCorrectParams1Call()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var query = Session.Query.All<Customer>()
        .Select(c => GetCustomers(1).Single(c2 => c2==c));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .Select(c => GetCustomers(1).AsEnumerable().Single(c2 => c2==c));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void ExternalMethodWithCorrectParams2Call()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      int count = 1;
      var query = Session.Query.All<Customer>()
        .Select(c => GetCustomers(count).Single(c2 => c2==c));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable()
        .Select(c => GetCustomers(count).AsEnumerable().Single(c2 => c2==c));
      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void ExternalMethodWithIncorrectParamsCall()
    {
      Assert.Throws<QueryTranslationException>(() => {
        var query = Session.Query.All<Customer>().Select(c => GetCustomers(c.Invoices.Count()).Single(c2 => c2==c)).ToList();
      });
    }

    public IQueryable<Customer> GetCustomers()
    {
      return Session.Query.All<Customer>();
    }

    public IQueryable<Customer> GetCustomers(int count)
    {
      return Session.Query.All<Customer>();
    }
  }
}