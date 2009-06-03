// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2009.01.12

using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SelectTest : NorthwindDOModelTest
  {
    [Test]
    public void AnoimousEntityTest()
    {
      var result = Query<Category>.All
        .Select(category => new{Category = category} )
        .Select(a => a.Category);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnoimousEntityKeyTest()
    {
      var result = Query<Category>.All
        .Select(category => new{Category = category} )
        .Select(a => a.Category.Key);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnoimousEntityFieldTest()
    {
      var result = Query<Category>.All
        .Select(category => new{Category = category} )
        .Select(a => a.Category.CategoryName);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnoimousEntityKeyFieldTest()
    {
      var result = Query<Category>.All
        .Select(category => new{Category = category} )
        .Select(a => a.Category.Id);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void OutOfHierarchy()
    {
      Assert.Greater(Query<Person>.All.Count(), 0);
    }

    [Test]
    public void SimpleSelectTest()
    {
      var result = Query<Order>.All;
      QueryDumper.Dump(result);
    }

    [Test]
    public void SimpleConstantTest()
    {
      var products = Query<Product>.All;
      var result =
        from p in products
        select 0;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(0, i);
    }

    [Test]
    public void AnonymousColumn()
    {
      var products = Query<Product>.All.Select(p => new {p.ProductName}.ProductName);
      var list = products.ToList();
    }

    [Test]
    public void AnonymousWithCalculatedColumnsTest()
    {
      var result = Query<Customer>.All.Select(c =>
        new {
          n1 = c.ContactTitle.Length + c.ContactName.Length,
          n2 = c.ContactName.Length + c.ContactTitle.Length
        });
      result = result.Where(i => i.n1 > 10);
      result.ToList();
    }

    [Test]
    public void AnonymousParameterColumn()
    {
      var param = new {ProductName = "name"};
      var products = Query<Product>.All.Select(p => param.ProductName);
      QueryDumper.Dump(products);
    }


    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void NewPairTest()
    {
      var method = MethodInfo.GetCurrentMethod().Name;
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {
            Value = new Pair<string>(p.ProductName, method),
            Method = method,
            p.ProductName
          }
        orderby r.ProductName
        where r.Method==method
        select r;
      var list = result.ToList();
      foreach (var i in list)
        Assert.AreEqual(method, i.Method);
    }

    [Test]
    public void ConstantTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
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
      var products = Query<Product>.All;
      var result = from p in products
      select (string) null;
      var list = result.ToList();
      foreach (var s in list)
        Assert.AreEqual(null, s);
    }

    [Test]
    public void LocalTest()
    {
      int x = 10;
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
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
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select p.ProductName
        where r!=null
        select r;
      var list = result.ToList();
      foreach (var s in list)
        Assert.IsNotNull(s);
    }

    [Test]
    public void CalculatedColumnTest()
    {
      var products = Query<Product>.All;
      var result = from r in
        from p in products
        select p.UnitsInStock * p.UnitPrice
      where r > 0
      select r;
      var list = result.ToList();
      var checkList = products.AsEnumerable().Select(p => p.UnitsInStock * p.UnitPrice).ToList();
      list.SequenceEqual(checkList);
    }

    [Test]
    public void KeyTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select p.Key
        where r!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      foreach (var k in list) {
        Assert.IsNotNull(k);
        var p = k.Resolve<Product>();
        Assert.IsNotNull(p);
      }
    }

    [Test]
    public void AnonymousTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select new {p.ProductName, p.UnitPrice, p.UnitsInStock};
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnonymousEmptyTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select new {};
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void AnonymousCalculatedTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {p.ProductName, TotalPriceInStock = p.UnitPrice * p.UnitsInStock}
        where r.TotalPriceInStock > 0
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void JoinedEntityColumnTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select p.Supplier.CompanyName;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void JoinedEntityTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select p.Supplier
        where r.CompanyName!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void StructureColumnTest()
    {
      var products = Query<Product>.All;
      var result = from p in products
      select p.Supplier.Address;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void StructureTest()
    {
      var products = Query<Product>.All;
      var result = from a in (
        from p in products
        select p.Supplier.Address)
      where a.Region!=null
      select a.StreetAddress;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void EntitySetTest()
    {
      var suppliers = Query<Supplier>.All;
      var result = from s in suppliers
      select s.Products;
      var list = result.ToList();
    }

    [Test]
    public void AnonymousWithEntityTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {p.ProductName, Product = p}
        where r.Product!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void AnonymousNestedTest()
    {
      var products = Query<Product>.All;
      var result =
        from r in
          from p in products
          select new {p, Desc = new {p.ProductName, p.UnitPrice}}
        where r.Desc.ProductName!=null
        select r;
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void NestedQueryTest()
    {
      var products = Query<Product>.All;
      var result = from pd in
        from p in products
        select new {ProductKey = p.Key, p.ProductName, TotalPrice = p.UnitPrice * p.UnitsInStock}
      where pd.TotalPrice > 100
      select new {PKey = pd.ProductKey, pd.ProductName, Total = pd.TotalPrice};

      var list = result.ToList();
    }

    [Test]
    public void NestedQueryWithStructuresTest()
    {
      var products = Query<Product>.All;
      var result =
        from a in
          from pd in
            from p in products
            select new {ProductKey = p.Key, SupplierAddress = p.Supplier.Address}
          select new {PKey = pd.ProductKey, pd.SupplierAddress, SupplierCity = pd.SupplierAddress.City}
        select new {a.PKey, a.SupplierAddress, a.SupplierCity};
      var list = result.ToList();
    }


    [Test]
    public void NestedQueryWithEntitiesTest()
    {
      var products = Query<Product>.All;
      var result = from pd in
        from p in products
        select new {ProductKey = p.Key, Product = p}
      select new {PKey = pd.ProductKey, pd.Product};

      var list = result.ToList();
    }

    [Test]
    public void NestedQueryWithAnonymousTest()
    {
      var products = Query<Product>.All;
      var result = from pd in
        from p in products
        select new {ProductKey = p.Key, Product = new {Entity = new {p}, Name = p.ProductName}}
      select new {PKey = pd.ProductKey, pd.Product.Name, A = pd, AProduct = pd.Product, AEntity = pd.Product.Entity};

      var list = result.ToList();
    }

    [Test]
    public void SelectEnumTest()
    {
      var result = from o in Query<Order>.All select o.OrderDate.Value.DayOfWeek;
      result.ToList();
    }

    [Test]
    public void SelectAnonymousEnumTest()
    {
      var result = from o in Query<Order>.All select new {o.OrderDate.Value.DayOfWeek};
      result.ToList();
    }

    [Test]
    public void SelectEnumFieldTest()
    {
      var result = from p in Query<ActiveProduct>.All select p.ProductType;
      foreach (var p in result)
        Assert.AreEqual(p, ProductType.Active);
    }

    [Test]
    public void SelectAnonymousEnumFieldTest()
    {
      var result = from p in Query<ActiveProduct>.All select new {p.ProductType};
      foreach (var p in result)
        Assert.AreEqual(p.ProductType, ProductType.Active);
    }

    [Test]
    public void SelectCharTest()
    {
      var result = from c in Query<Customer>.All select c.CompanyName[0];
      var list = result.ToList();
    }

    [Test]
    public void SelectByteArrayLengthTest()
    {
      var categories = Query<Category>.All;
      var result = from c in categories select c.Picture.Length;
      var list = result.ToList();
    }

    [Test]
    public void SelectEqualsTest()
    {
      var customers = Query<Customer>.All;
      var result = from c in customers select c.CompanyName.Equals("lalala");
      var list = result.ToList();
    }

    [Test]
    public void DoubleSelectEntitySet1Test()
    {
      IQueryable<EntitySet<Order>> query = Query<Customer>.All.Select(c => c.Orders).Select(c => c);
      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void DoubleSelectEntitySet2Test()
    {
      IQueryable<EntitySet<Order>> query = Query<Customer>.All.Select(c => c).Select(c => c.Orders);
      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void DoubleSelectEntitySet3Test()
    {
      var query = Query<Customer>.All.Select(c => c.Orders.Select(o => o));
//          var query = Query<Customer>.All.Select(c => c.Orders);

      foreach (var order in query)
        QueryDumper.Dump(order);
    }

    [Test]
    public void NestedAnonymousTest()
    {
      var result = Query<Customer>.All
        .Select(c => new {c})
        .Select(a1 => new {a1})
        .Select(a2 => a2.a1.c.CompanyName);
      QueryDumper.Dump(result);
    }

    [Test]
    public void EntityWithLazyLoadFieldTest()
    {
      var category = Query<Category>.All.Where(c => c.Picture!=null).First();
      int columnIndex = Domain.Model.Types[typeof (Category)].Fields["Picture"].MappingInfo.Offset;
      Assert.IsFalse(category.State.Tuple.IsAvailable(columnIndex));
    }

    [Test]
    public void AnonymousSelectTest()
    {
      var result = Query<Order>.All
        .Select(o => new {o.OrderDate, o.Freight})
        .Select(g => g.OrderDate);
      QueryDumper.Dump(result);
    }

    [Test]
    public void SelectJustOuterParameterTest()
    {
      var result = Query<Customer>.All.Select(c => Query<Supplier>.All.Select(s => c));
      foreach (var i in result)
        i.ToList();
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void NonPersistentFieldTest()
    {
      var result = from e in Query<Employee>.All select e.FullName;
      result.ToList();
    }

    [Test]
    public void SelectBigMulTest()
    {
      var result =
        from order in Query<Order>.All
        select Math.BigMul(order.Id, order.Employee.Id);
      result.ToList();
    }

    [Test]
    public void SelectSignTest()
    {
      var result =
        from order in Query<Order>.All
        where order.Id > 0 && order.Id < 50
        let values = new
          {
            Byte = (sbyte) order.Id,
            Short = (short) order.Id,
            Int = order.Id,
            Long = (long) order.Id,
            Decimal = (decimal) order.Id,
            Float = (float) order.Id,
            Double = (double) order.Id,
          }
        select new
          {
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
        Query<Customer>.All
          .Select(c => new
            {
              String = c.Id,
              Char0 = c.Id[0],
              Char1 = c.Id[1],
              Char2 = c.Id[2],
              Char3 = c.Id[3],
              Char4 = c.Id[4],
            })
          .OrderBy(item => item.String)
          .ToArray();
      var expected =
        Query<Customer>.All
          .AsEnumerable()
          .Select(c => new
            {
              String = c.Id,
              Char0 = c.Id[0],
              Char1 = c.Id[1],
              Char2 = c.Id[2],
              Char3 = c.Id[3],
              Char4 = c.Id[4],
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
      var result =
        Query<Customer>.All
          .Select(c => new
            {
              String = c.Id,
              IndexOfChar = c.Id.IndexOf('A'),
              IndexOfCharStart = c.Id.IndexOf('A', 1),
              IndexOfCharStartCount = c.Id.IndexOf('A', 1, 1),
              IndexOfString = c.Id.IndexOf("A"),
              IndexOfStringStart = c.Id.IndexOf("A", 1),
              IndexOfStringStartCount = c.Id.IndexOf("A", 1, 1)
            })
          .OrderBy(item => item.String)
          .ToArray();
      var expected =
        Query<Customer>.All
          .AsEnumerable()
          .Select(c => new
            {
              String = c.Id,
              IndexOfChar = c.Id.IndexOf('A'),
              IndexOfCharStart = c.Id.IndexOf('A', 1),
              IndexOfCharStartCount = c.Id.IndexOf('A', 1, 1),
              IndexOfString = c.Id.IndexOf("A"),
              IndexOfStringStart = c.Id.IndexOf("A", 1),
              IndexOfStringStartCount = c.Id.IndexOf("A", 1, 1)
            })
          .OrderBy(item => item.String)
          .ToArray();
      Assert.AreEqual(expected.Length, result.Length);
      for (int i = 0; i < expected.Length; i++) {
        Assert.AreEqual(expected[0].String, result[0].String);
        Assert.AreEqual(expected[0].IndexOfChar, result[0].IndexOfChar);
        Assert.AreEqual(expected[0].IndexOfCharStart, result[0].IndexOfCharStart);
        Assert.AreEqual(expected[0].IndexOfCharStartCount, result[0].IndexOfCharStartCount);
        Assert.AreEqual(expected[0].IndexOfString, result[0].IndexOfString);
        Assert.AreEqual(expected[0].IndexOfStringStart, result[0].IndexOfStringStart);
        Assert.AreEqual(expected[0].IndexOfStringStartCount, result[0].IndexOfStringStartCount);
      }
    }

    [Test]
    public void SelectStringContainsTest()
    {
      var result =
        Query<Customer>.All
          .Where(c => c.Id.Contains('C'))
          .OrderBy(c => c.Id)
          .ToArray();
      var expected =
        Query<Customer>.All
          .AsEnumerable()
          .Where(c => c.Id.Contains('C'))
          .OrderBy(c => c.Id)
          .ToArray();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void SelectDateTimeTimeSpanTest()
    {
      var dateTime = new DateTime(2001, 1, 1, 1, 1, 1);
      var timeSpan = new TimeSpan(1, 1, 1, 1);

      var result = Query<Customer>.All
        .Select(c => new
          {
            CustomerId = c.Id,
            DateTime = dateTime,
            TimeSpan = timeSpan
          }
        )
        .Select(k => new
          {
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
            TimeSpanTicks = k.TimeSpan.Ticks,
            TimeSpanDuration = k.TimeSpan.Duration(),
          }
        )
        .First();

      var expected =
        new
          {
            DateTime = dateTime,
            DateTimeDate = dateTime.Date,
            DateTimeTime = dateTime.TimeOfDay,
            DateTimeYear = dateTime.Year,
            DateTimeMonth = dateTime.Month,
            DateTimeDay = dateTime.Day,
            DateTimeHour = dateTime.Hour,
            DateTimeMinute = dateTime.Minute,
            DateTimeSecond = dateTime.Second,
            DateTimeDayOfYear = dateTime.DayOfYear,
            DateTimeDayOfWeek = dateTime.DayOfWeek,
            TimeSpan = timeSpan,
            TimeSpanDays = timeSpan.Days,
            TimeSpanHours = timeSpan.Hours,
            TimeSpanMinutes = timeSpan.Minutes,
            TimeSpanSeconds = timeSpan.Seconds,
            TimeSpanTotalDays = timeSpan.TotalDays,
            TimeSpanTotalHours = timeSpan.TotalHours,
            TimeSpanTotalMinutes = timeSpan.TotalMinutes,
            TimeSpanTotalSeconds = timeSpan.TotalSeconds,
            TimeSpanTicks = timeSpan.Ticks,
            TimeSpanDuration = timeSpan.Duration(),
          };

      Assert.AreEqual(expected.DateTime, result.DateTime);
      Assert.AreEqual(expected.DateTimeDate, result.DateTimeDate);
      Assert.AreEqual(expected.DateTimeTime, result.DateTimeTime);
      Assert.AreEqual(expected.DateTimeYear, result.DateTimeYear);
      Assert.AreEqual(expected.DateTimeMonth, result.DateTimeMonth);
      Assert.AreEqual(expected.DateTimeDay, result.DateTimeDay);
      Assert.AreEqual(expected.DateTimeHour, result.DateTimeHour);
      Assert.AreEqual(expected.DateTimeMinute, result.DateTimeMinute);
      Assert.AreEqual(expected.DateTimeSecond, result.DateTimeSecond);
      Assert.AreEqual(expected.DateTimeDayOfYear, result.DateTimeDayOfYear);
      Assert.AreEqual(expected.DateTimeDayOfWeek, result.DateTimeDayOfWeek);
      Assert.AreEqual(expected.TimeSpan, result.TimeSpan);
      Assert.AreEqual(expected.TimeSpanDays, result.TimeSpanDays);
      Assert.AreEqual(expected.TimeSpanHours, result.TimeSpanHours);
      Assert.AreEqual(expected.TimeSpanMinutes, result.TimeSpanMinutes);
      Assert.AreEqual(expected.TimeSpanSeconds, result.TimeSpanSeconds);
      Assert.AreEqual(expected.TimeSpanTotalDays, result.TimeSpanTotalDays);
      Assert.AreEqual(expected.TimeSpanTotalHours, result.TimeSpanTotalHours);
      Assert.AreEqual(expected.TimeSpanTotalMinutes, result.TimeSpanTotalMinutes);
      Assert.AreEqual(expected.TimeSpanTotalSeconds, result.TimeSpanTotalSeconds);
      Assert.AreEqual(expected.TimeSpanTicks, result.TimeSpanTicks);
      Assert.AreEqual(expected.TimeSpanDuration, result.TimeSpanDuration);
    }
  }
}
