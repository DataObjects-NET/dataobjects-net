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
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

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
  public class WhereTest : NorthwindDOModelTest
  {
    private Key supplierLekaKey;
    private Key categoryFirstKey;

    [OneTimeSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          supplierLekaKey = session.Query.All<Supplier>().Single(s => s.CompanyName=="Leka Trading").Key;
          categoryFirstKey = session.Query.All<Category>().First().Key;
        }
      }
    }

    [Test]
    public void IndexerTest()
    {
      var query = Session.Query.All<Order>();
      var kvp = new Pair<string, object>("Customer", Session.Query.All<Customer>().First());
      query = query.Where(order => order[kvp.First]==kvp.Second);
      var r = query.ToList();
    }

    [Test]
    public void IndexerNullTest()
    {
      var query = Session.Query.All<Order>();
      var parameters = Session.Query.All<Customer>().Take(1).Select(c => new TargetField("Customer", (Customer)null)).ToList();
      foreach (var item in parameters) {
        var field = item; // This is important to use local variable
        query = query.Where(order => order[field.Name]==field.Value);
      }
      var result = query.ToList<Order>();
      }

    [Test]
    public void ReferenceFieldConditionTest()
    {
      var result = Session.Query.All<Order>()
        .Where(order => order.Customer!=null)
        .ToList();

//      var expected = Session.Query.All<Order>()
//        .AsEnumerable()
//        .First(order => order.Customer!=null);
//      Assert.AreEqual(expected.Customer, result.Customer);
    }

    [Test]
    public void IndexerSimpleFieldTest()
    {
      object freight = Session.Query
        .All<Order>()
        .First(order => order.Freight > 0)
        .Freight;
      var result = Session.Query
        .All<Order>()
        .OrderBy(order => order.Id)
        .Where(order => order["Freight"]==freight)
        .ToList();
      var expected = Session.Query
        .All<Order>()
        .AsEnumerable()
        .OrderBy(order => order.Id)
        .Where(order => order.Freight==(decimal?) freight)
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
        .OrderBy(customer => customer.Id)
        .Where(customer => customer["Address"]==address)
        .ToList();
#pragma warning disable 252,253
      var expected = Session.Query
        .All<Customer>()
        .AsEnumerable()
        .OrderBy(customer => customer.Id)
        .Where(customer => customer.Address==address)
        .ToList();
#pragma warning restore 252,253
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void IndexerEntity1Test()
    {
      object customer = Session.Query
        .All<Order>()
        .First(order => order.Customer!=null)
        .Customer;
      var result = Session.Query
        .All<Order>()
        .OrderBy(order => order.Id)
        .Where(order => order["Customer"]==customer)
        .ToList();
      var expected = Session.Query
        .All<Order>()
        .AsEnumerable()
        .OrderBy(order => order.Id)
        .Where(order => order.Customer==customer)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void EntitySubqueryTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Order>()
        .OrderBy(order => order.Id)
        .Where(order => order.Customer==
          Session.Query.All<Order>()
            .OrderBy(order2 => order2.Customer.Id)
            .First(order2 => order2.Customer!=null)
            .Customer)
        .ToList();
      var expected = Session.Query.All<Order>()
        .AsEnumerable()
        .OrderBy(order => order.Id)
        .Where(order => order.Customer==
          Session.Query.All<Order>()
            .OrderBy(order2 => order2.Customer.Id)
            .First(order2 => order2.Customer!=null)
            .Customer)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void EntitySubqueryIndexerTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var result = Session.Query.All<Order>()
        .OrderBy(order => order.Id)
        .Where(order => order.Customer==
          Session.Query.All<Order>()
            .OrderBy(order2 => order2.Customer.Id)
            .First(order2 => order2["Customer"]!=null)
            .Customer)
        .ToList();
      var expected = Session.Query.All<Order>()
        .AsEnumerable()
        .OrderBy(order => order.Id)
        .Where(order => order.Customer==
          Session.Query.All<Order>()
            .OrderBy(order2 => order2.Customer.Id)
            .First(order2 => order2.Customer!=null)
            .Customer)
        .ToList();
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void ParameterTest()
    {
      var category = Session.Query.All<Category>().First();
      var result = Session.Query.All<Product>().Where(p => p.Category==category);
      QueryDumper.Dump(result);
    }

    [Test]
    public void MultipleConditionTest()
    {
      var customers = Session.Query.All<Customer>().Select(c => c.CompanyName).Where(cn => cn.StartsWith("A") || cn.StartsWith("B"));
      var list = customers.ToList();
    }

    [Test]
    public void AnonymousTest()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.CompanyName, first.ContactName};
      var result = Session.Query.All<Customer>().Select(c => new {c.CompanyName, c.ContactName}).Take(10).Where(x => x==p);
      var list = result.ToList();
    }


    [Test]
    public void AnonymousTest2()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.CompanyName, first.ContactName};
      var result = Session.Query.All<Customer>().Select(c => new {c.CompanyName, c.ContactName}).Take(10).Where(x => p==x);
      var list = result.ToList();
    }

    [Test]
    public void AnonymousWithParameterTest()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.CompanyName, first.ContactName};
      var result = Session.Query.All<Customer>().Where(c => new {c.CompanyName, c.ContactName}==p);
      var list = result.ToList();
    }

    [Test]
    public void AnonymousWithParameter2Test()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.CompanyName, first.ContactName};
      var k = new {InternalFiled = p};
      var result = Session.Query.All<Customer>().Where(c => new {c.CompanyName, c.ContactName}==k.InternalFiled);
      QueryDumper.Dump(result);
    }


    [Test]
    public void AnonymousWithParameter3Test()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.CompanyName, first.ContactName};
      var k = new {InternalFiled = p};
      var result = Session.Query.All<Customer>().Where(c => new {X = new {c.CompanyName, c.ContactName}}.X==k.InternalFiled);
      QueryDumper.Dump(result);
    }

    [Test]
    public void Anonymous2Test2()
    {
      Customer first = Session.Query.All<Customer>().First();
      var p = new {first.CompanyName, first.ContactName};
      var result = Session.Query.All<Customer>().Where(c => new {c.CompanyName}.CompanyName=="CompanyName");
      var list = result.ToList();
    }

    [Test]
    public void Anonymous3Test()
    {
      Customer first = Session.Query.All<Customer>().First();
      var result = Session.Query.All<Customer>().Where(c => new {c.CompanyName, c.ContactName}==new {c.CompanyName, c.ContactName});
      var list = result.ToList();
    }

    [Test]
    public void Anonymous4Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      Customer first = Session.Query.All<Customer>().First();
      Customer second = Session.Query.All<Customer>().Skip(1).First();
      var p = new {first.CompanyName, first.ContactName};
      var l = new {second.CompanyName, second.ContactName};
      var result = Session.Query.All<Customer>().Where(c => l==p);
      var list = result.ToList();
    }


    [Test]
    public void ColumnTest()
    {
      var suppliers = Session.Query.All<Supplier>();
      var supplier = suppliers.Where(s => s.CompanyName=="Tokyo Traders").First();
      Assert.IsNotNull(supplier);
      Assert.AreEqual("Tokyo Traders", supplier.CompanyName);
    }

    [Test]
    public void CalculatedTest()
    {
      var expected = Session.Query.All<Product>().ToList().Where(p => p.UnitPrice * p.UnitsInStock >= 100).ToList();
      var actual = Session.Query.All<Product>().Where(p => p.UnitPrice * p.UnitsInStock >= 100).ToList();
      Assert.AreEqual(expected.Count, actual.Count);
    }

    [Test]
    public void StructureTest()
    {
      var suppliers = Session.Query.All<Supplier>();
      var supplier = suppliers.Where(s => s.Address.Region=="Victoria").First();
      Assert.IsNotNull(supplier);
      Assert.AreEqual("Victoria", supplier.Address.Region);
    }

    [Test]
    public void IdTest()
    {
      var suppliers = Session.Query.All<Supplier>();
      var supplier = suppliers.Where(s => s.Id==supplierLekaKey.Value.GetValue<int>(0)).First();
      Assert.IsNotNull(supplier);
      Assert.AreEqual("Leka Trading", supplier.CompanyName);
    }

    [Test]
    public void KeyTest()
    {
      var suppliers = Session.Query.All<Supplier>();
      var key = Key.Create<Supplier>(Domain, supplierLekaKey.Value);
      var supplier = suppliers.Where(s => s.Key==key).First();
      Assert.IsNotNull(supplier);
      Assert.AreEqual("Leka Trading", supplier.CompanyName);
    }

    [Test]
    public void InstanceTest()
    {
      var supplierLeka = Session.Query.SingleOrDefault<Supplier>(supplierLekaKey);
      var suppliers = Session.Query.All<Supplier>();
      var supplier = suppliers.Where(s => s==supplierLeka).First();
      Assert.IsNotNull(supplier);
      Assert.AreEqual("Leka Trading", supplier.CompanyName);
    }

    [Test]
    public void ForeignKeyTest()
    {
      var supplierLeka = Session.Query.SingleOrDefault<Supplier>(supplierLekaKey);
      var products = Session.Query.All<Product>();
      var product = products.Where(p => p.Supplier.Key==supplierLeka.Key).First();
      Assert.IsNotNull(product);
      Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
    }

    [Test]
    public void ForeignIDTest()
    {
      var supplier20 = Session.Query.SingleOrDefault<Supplier>(supplierLekaKey);
      var products = Session.Query.All<Product>();
      var product = products.Where(p => p.Supplier.Id==supplier20.Id).First();
      Assert.IsNotNull(product);
      Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
    }

    [Test]
    public void ForeignInstanceTest()
    {
      var supplier20 = Session.Query.SingleOrDefault<Supplier>(supplierLekaKey);
      var products = Session.Query.All<Product>();
      var product = products.Where(p => p.Supplier==supplier20).First();
      Assert.IsNotNull(product);
      Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
    }

    [Test]
    public void ForeignPropertyTest()
    {
      var products = Session.Query.All<Product>();
      var product = products.Where(p => p.Supplier.CompanyName=="Leka Trading").First();
      Assert.IsNotNull(product);
      Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
      product =
        products.Where(
          p =>
            p.Supplier.CompanyName=="Leka Trading" && p.Category.Key==categoryFirstKey &&
              p.Supplier.ContactTitle=="Owner").First();
      Assert.IsNotNull(product);
      Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
    }

    [Test]
    public void CoalesceTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => (c.Address.City ?? "Seattle")=="Seattle").First();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => (c.Address.City ?? c.Address.Country ?? "Seattle")=="Seattle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void ConditionalTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => (o.Customer.Id=="ALFKI" ? 1000 : 0)==1000).First();
      Assert.IsNotNull(order);
      order =
        orders.Where(o => (o.Customer.Id=="ALFKI" ? 1000 : o.Customer.Id=="ABCDE" ? 2000 : 0)==1000).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void StringLengthTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Length==7).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringStartsWithLiteralTest()
    {
      var customers = Session.Query.All<Category>();
      var customer = customers.Where(c => c.CategoryName.StartsWith("M")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringStartsWithColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.ContactName.StartsWith(c.ContactName)).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringEndsWithLiteralTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.ContactName.EndsWith("s")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringEndsWithColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.ContactName.EndsWith(c.ContactName)).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringContainsLiteralTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.ContactName.Contains("and")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringContainsColumnTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.ContactName.Contains(c.ContactName)).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringConcatImplicitArgsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.ContactName + "X"=="X").FirstOrDefault();
      Assert.IsNull(customer);
    }

    [Test]
    public void StringConcatExplicitNArgTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Concat(c.ContactName, "X")=="X").FirstOrDefault();
      Assert.IsNull(customer);
      customer = customers.Where(c => string.Concat(c.ContactName, "X", c.Address.Country)=="X").FirstOrDefault();
      Assert.IsNull(customer);
    }

    [Test]
    public void StringIsNullOrEmptyTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.IsNullOrEmpty(c.Address.Region)).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringToUpperTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.ToUpper()=="SEATTLE").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringToLowerTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.ToLower()=="seattle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringReplaceTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Replace("ea", "ae")=="Saettle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringReplaceCharsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Replace("e", "y")=="Syattly").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringSubstringTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Substring(0, 4)=="Seat").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringSubstringNoLengthTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Substring(4)=="tle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringRemoveTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Remove(1, 2)=="Sttle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringRemoveNoCountTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Remove(3)=="Sea").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringIndexOfTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support IndexOf()");

      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.IndexOf("tt")==3).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringIndexOfCharTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support IndexOf()");

      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.IndexOf('t')==3).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringTrimTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.Trim()=="Seattle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringToStringTest()
    {
      // just to prove this is a no op
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.ToString()=="Seattle").First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void DateTimeConstructYMDTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate >= new DateTime(o.OrderDate.Value.Year, 1, 1)).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeConstructYMDHMSTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate >= new DateTime(o.OrderDate.Value.Year, 1, 1, 10, 25, 55)).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => ((DateTime?)o.OrderDate) < ((DateTime?)new DateTime(2010, 12, 31))).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeDayTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Day==5).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeMonthTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Month==12).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeYearTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Year==1997).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeHourTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Hour==0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeMinuteTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Minute==0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeSecond()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Second==0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeMillisecondTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.Millisecond==0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeDayOfWeekTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ScalarSubqueries);
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.DayOfWeek==DayOfWeek.Friday).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DateTimeDayOfYearTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.OrderDate.Value.DayOfYear==360).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathAbsTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Abs(o.Id)==10 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathAcosTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Acos()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Acos(Math.Sin(o.Id))==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathAsinTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Asin()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Asin(Math.Cos(o.Id))==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathAtanTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Atan()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Atan(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
      order = orders.Where(o => Math.Atan2(o.Id, 3)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathCosTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Cos(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathSinTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Sin()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Sin(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathTanTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Tan()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Tan(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathExpTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Exp(o.Id < 1000 ? 1 : 2)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathLogTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Log(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathLog10Test()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Log10(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathSqrtTest()
    {
Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Sqrt()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Sqrt(o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathCeilingTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Ceiling((double) o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathFloorTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Floor((double) o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathPowTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Pow()");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Pow(o.Id < 1000 ? 1 : 2, 3)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathRoundDefaultTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Round((decimal) o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathRoundToPlaceTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Pow() which is used in Round() translation");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Round((decimal) o.Id, 2)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void MathTruncateTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => Math.Truncate((double) o.Id)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void StringLessThanTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.LessThan("Seattle")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringLessThanOrEqualsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.LessThanOrEqual("Seattle")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringGreaterThanTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.GreaterThan("Seattle")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringGreaterThanOrEqualsTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.GreaterThanOrEqual("Seattle")).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToLTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") < 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToLETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") <= 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToGTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") > 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToGETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") >= 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareToEQTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Seattle")==0).First();
      Assert.IsNotNull(customer);
    }


    [Test]
    public void StringCompareToNETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City.CompareTo("Seattle")!=0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareLTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") < 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareLETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") <= 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareGTTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") > 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareGETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") >= 0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareEQTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle")==0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void StringCompareNETest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle")!=0).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void IntCompareToTest()
    {
      // prove that x.CompareTo(y) works for types other than string
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id.CompareTo(1000)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalCompareTest()
    {
      // prove that type.Compare(x,y) works with decimal
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Compare((decimal) o.Id, 0.0m)==0 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalAddTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Add(o.Id, 0.0m)==0.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalSubtractTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Subtract(o.Id, 0.0m)==0.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalMultiplyTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Multiply(o.Id, 1.0m)==1.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalDivideTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Divide(o.Id, 1.0m)==1.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalRemainderTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Remainder(o.Id, 1.0m)==0.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalNegateTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Negate(o.Id)==1.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalCeilingTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Ceiling(o.Id)==0.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalFloorTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Floor(o.Id)==0.0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalRoundDefaultTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Round(o.Id)==0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalRoundPlacesTest()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite, "sqlite does not support Pow() which is used in Round() translation");

      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Round(o.Id, 2)==0.00m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalTruncateTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => decimal.Truncate(o.Id)==0m || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void DecimalGTTest()
    {
      // prove that decimals are treated normally with respect to normal comparison operators
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => ((decimal) o.Id) > 0.0m).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void FkCompareTest()
    {
      var employee = Session.Query.All<Employee>().Where(e => e.ReportsTo.Id > 20).First();
      Assert.IsNotNull(employee);
    }

    [Test]
    public void IntLessThanTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id < 0).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntLessThanOrEqualTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id <= 0).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntGreaterThanTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntGreaterThanOrEqualTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id >= 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntEqualTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id==0).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntNotEqualTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id!=0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntAddTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id + 0==0).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntSubtractTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id - 0==0).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntMultiplyTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id * 1==1 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntDivideTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id / 1==1 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntModuloTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id % 1==0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntBitwiseAndTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => (o.Id & 1)==0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void IntBitwiseOrTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => (o.Id | 1)==1 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }


    [Test]
    public void IntBitwiseExclusiveOrTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => (o.Id ^ 1)==1).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntBitwiseNotTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => ~o.Id==0).FirstOrDefault();
      Assert.IsNull(order);
    }

    [Test]
    public void IntNegateTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => -o.Id==-1 || o.Id > 0).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void AndTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id > 0 && o.Id < 2000).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void OrTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => o.Id < 5 || o.Id > 10).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void NotTest()
    {
      var orders = Session.Query.All<Order>();
      var order = orders.Where(o => !(o.Id==0)).First();
      Assert.IsNotNull(order);
    }

    [Test]
    public void EqualsNullTest()
    {
      //  TODO: Check IsNull or Equals(null)
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => c.Address.City!=null).First();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => !c.Address.Region.Equals(null)).First();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => c!=null).First();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => !c.Equals(null)).First();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => c.Address!=null).First();
      Assert.IsNotNull(customer);
      customer = customers.Where(c => !c.Address.Equals(null)).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void EqualNullReverseTest()
    {
      var customers = Session.Query.All<Customer>();
      var customer = customers.Where(c => null!=c.Address.City).First();
      Assert.IsNotNull(customer);
    }

    [Test]
    public void TimeSpanTest()
    {
      var maxProcessingTime = new TimeSpan(5, 0, 0, 0);
      Session.Query.All<Order>().Where(o => o.ProcessingTime > maxProcessingTime).ToList();
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
        join order in Session.Query.All<Order>() on customer equals order.Customer
        where order.Freight > 30
        orderby new {customer, order}
        select new {customer, order};
      var list = actual.ToList();
      var expected = from customer in Session.Query.All<Customer>().ToList()
        join order in Session.Query.All<Order>().ToList() on customer equals order.Customer
        where order.Freight > 30
        orderby customer.Id , order.Id
        select new {customer, order};
      Assert.IsTrue(expected.SequenceEqual(list));
    }

    [Test]
    public void ApplyTest()
    {
      var actual = Session.Query.All<Customer>()
        .Where(customer => customer.Orders.Any(o => o.Freight > 30));
      var expected = Session.Query.All<Customer>()
        .AsEnumerable() // AG: Just to remeber about it.
        .Where(customer => customer.Orders.Any(o => o.Freight > 30));
      Assert.IsTrue(expected.SequenceEqual(actual));
    }
  }
}