// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class WhereTest : NorthwindDOModelTest
  {
    private Key supplierLekaKey;
    private Key categoryFirstKey;

    [TestFixtureSetUp]
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession()) {
        supplierLekaKey = Query<Supplier>.All.Single(s => s.CompanyName == "Leka Trading").Key;
        categoryFirstKey = Query<Category>.All.First().Key;
      }
    }

    [Test]
    public void ColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Query<Supplier>.All;
          var supplier = suppliers.Where(s => s.CompanyName == "Tokyo Traders").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Tokyo Traders", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void CalculatedTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var list = products.Where(p => p.UnitPrice * p.UnitsInStock >= 100).ToList();
          Assert.AreEqual(67, list.Count);
          t.Complete();
        }
      }
    }

    [Test]
    public void StructureTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Query<Supplier>.All;
          var supplier = suppliers.Where(s => s.Address.Region == "Victoria").First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Victoria", supplier.Address.Region);
          t.Complete();
        }
      }
    }

    [Test]
    public void IdTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Query<Supplier>.All;
          var supplier = suppliers.Where(s => s.Id == supplierLekaKey.Value.GetValue<int>(0)).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Leka Trading", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void KeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var suppliers = Query<Supplier>.All;
          var key = Key.Create<Supplier>(supplierLekaKey.Value);
          var supplier = suppliers.Where(s => s.Key == key).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Leka Trading", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void InstanceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplierLeka = supplierLekaKey.Resolve<Supplier>();
          var suppliers = Query<Supplier>.All;
          var supplier = suppliers.Where(s => s == supplierLeka).First();
          Assert.IsNotNull(supplier);
          Assert.AreEqual("Leka Trading", supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignKeyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplierLeka = supplierLekaKey.Resolve<Supplier>();
          var products = Query<Product>.All;
          var product = products.Where(p => p.Supplier.Key == supplierLeka.Key).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Singaporean Hokkien Fried Mee", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignIDTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplier20 = supplierLekaKey.Resolve<Supplier>();
          var products = Query<Product>.All;
          var product = products.Where(p => p.Supplier.Id == supplier20.Id).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Singaporean Hokkien Fried Mee", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignInstanceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var supplier20 = supplierLekaKey.Resolve<Supplier>();
          var products = Query<Product>.All;
          var product = products.Where(p => p.Supplier == supplier20).First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Singaporean Hokkien Fried Mee", product.ProductName);
          t.Complete();
        }
      }
    }

    [Test]
    public void ForeignPropertyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var products = Query<Product>.All;
          var product = products.Where(p => p.Supplier.CompanyName == "Leka Trading").First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
          product =
            products.Where(
              p =>
                p.Supplier.CompanyName == "Leka Trading" && p.Category.Key == categoryFirstKey &&
                  p.Supplier.ContactTitle == "Owner").First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
          t.Complete();
        }
      }
    }

    [Test]
    public void CoalesceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => (c.Address.City ?? "Seattle") == "Seattle").First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => (c.Address.City ?? c.Address.Country ?? "Seattle") == "Seattle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void ConditionalTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => (o.Customer.Id == "ALFKI" ? 1000 : 0) == 1000).First();
          Assert.IsNotNull(order);
          order =
            orders.Where(o => (o.Customer.Id == "ALFKI" ? 1000 : o.Customer.Id == "ABCDE" ? 2000 : 0) == 1000).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringLengthTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Length == 7).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringStartsWithLiteralTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName.StartsWith("M")).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringStartsWithColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName.StartsWith(c.ContactName)).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringEndsWithLiteralTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName.EndsWith("s")).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringEndsWithColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName.EndsWith(c.ContactName)).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringContainsLiteralTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName.Contains("and")).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringContainsColumnTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName.Contains(c.ContactName)).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringConcatImplicitArgsTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.ContactName + "X" == "X").FirstOrDefault();
          Assert.IsNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringConcatExplicitNArgTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer =
            customers.Where(c => string.Concat(new string[] {c.ContactName, "X", c.Address.Country}) == "X").First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => string.Concat(c.ContactName, "X") == "X").First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => string.Concat(c.ContactName, "X", c.Address.Country) == "X").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringIsNullOrEmptyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.IsNullOrEmpty(c.Address.Region)).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringToUpperTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.ToUpper() == "SEATTLE").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringToLowerTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.ToLower() == "seattle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringReplaceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Replace("ea", "ae") == "Saettle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringReplaceCharsTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Replace("e", "y") == "Syattly").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringSubstringTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Substring(0, 4) == "Seat").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringSubstringNoLengthTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Substring(4) == "tle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringRemoveTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Remove(1, 2) == "Sttle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringRemoveNoCountTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Remove(4) == "Seat").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringIndexOfTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.IndexOf("tt") == 3).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringIndexOfCharTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.IndexOf('t') == 3).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringTrimTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.Trim() == "Seattle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringToStringTest()
    {
      // just to prove this is a no op
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.ToString() == "Seattle").First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeConstructYMDTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Value.Year, 1, 1)).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeConstructYMDHMSTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Value.Year, 1, 1, 10, 25, 55)).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeDayTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Day == 5).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeMonthTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Month == 12).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeYearTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Year == 1997).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeHourTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Hour == 6).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeMinuteTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Minute == 32).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeSecond()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Second == 47).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeMillisecondTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.Millisecond == 200).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeDayOfWeekTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.DayOfWeek == DayOfWeek.Friday).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DateTimeDayOfYearTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.OrderDate.Value.DayOfYear == 360).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathAbsTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Abs(o.Id) == 10 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathAcosTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Acos(Math.Sin(o.Id)) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathAsinTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Asin(Math.Cos(o.Id)) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathAtanTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Atan(o.Id) == 0).First();
          Assert.IsNotNull(order);
          order = orders.Where(o => Math.Atan2(o.Id, 3) == 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathCosTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Cos(o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathSinTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Sin(o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathTanTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Tan(o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathExpTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Exp(o.Id < 1000 ? 1 : 2) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathLogTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Log(o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathLog10Test()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Log10(o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathSqrtTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Sqrt(o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathCeilingTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Ceiling((double)o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathFloorTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Floor((double)o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathPowTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Pow(o.Id < 1000 ? 1 : 2, 3) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathRoundDefaultTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Round((decimal)o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathRoundToPlaceTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Round((decimal)o.Id, 2) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void MathTruncateTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => Math.Truncate((double)o.Id) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareToLTTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") < 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareToLETest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") <= 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareToGTTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") > 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareToGETest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") >= 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareToEQTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") == 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareToNETest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City.CompareTo("Seattle") != 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareLTTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") < 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareLETest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") <= 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareGTTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") > 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareGETest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") >= 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareEQTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") == 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void StringCompareNETest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => string.Compare(c.Address.City, "Seattle") != 0).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntCompareToTest()
    {
      // prove that x.CompareTo(y) works for types other than string
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id.CompareTo(1000) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalCompareTest()
    {
      // prove that type.Compare(x,y) works with decimal
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Compare((decimal)o.Id, 0.0m) == 0 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalAddTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Add(o.Id, 0.0m) == 0.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalSubtractTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Subtract(o.Id, 0.0m) == 0.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalMultiplyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Multiply(o.Id, 1.0m) == 1.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalDivideTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Divide(o.Id, 1.0m) == 1.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalRemainderTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Remainder(o.Id, 1.0m) == 0.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalNegateTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Negate(o.Id) == 1.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalCeilingTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Ceiling(o.Id) == 0.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalFloorTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Floor(o.Id) == 0.0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalRoundDefaultTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Round(o.Id) == 0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalRoundPlacesTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Round(o.Id, 2) == 0.00m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalTruncateTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => decimal.Truncate(o.Id) == 0m || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void DecimalGTTest()
    {
      // prove that decimals are treated normally with respect to normal comparison operators
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => ((decimal) o.Id) > 0.0m).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntLessThanTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id < 0).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntLessThanOrEqualTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id <= 0).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntGreaterThanTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntGreaterThanOrEqualTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id >= 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntEqualTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id == 0).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntNotEqualTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id != 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntAddTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id + 0 == 0).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntSubtractTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id - 0 == 0).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntMultiplyTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id * 1 == 1 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntDivideTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id / 1 == 1 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntModuloTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id % 1 == 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntBitwiseAndTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => (o.Id & 1) == 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntBitwiseOrTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => (o.Id | 1) == 1 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntBitwiseExclusiveOrTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => (o.Id ^ 1) == 1).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntBitwiseNotTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => ~o.Id == 0).FirstOrDefault();
          Assert.IsNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void IntNegateTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => -o.Id == -1 || o.Id > 0).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void AndTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id > 0 && o.Id < 2000).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void OrTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => o.Id < 5 || o.Id > 10).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void NotTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<Order>.All;
          var order = orders.Where(o => !(o.Id == 0)).First();
          Assert.IsNotNull(order);
          t.Complete();
        }
      }
    }

    [Test]
    public void EqualsNullTest()
    {
      //  TODO: Check IsNull or Equals(null)
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => c.Address.City != null).First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => !c.Address.Region.Equals(null)).First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => c != null).First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => !c.Equals(null)).First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => c.Address != null).First();
          Assert.IsNotNull(customer);
          customer = customers.Where(c => !c.Address.Equals(null)).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }

    [Test]
    public void EqualNullReverseTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var customers = Query<Customer>.All;
          var customer = customers.Where(c => null != c.Address.City).First();
          Assert.IsNotNull(customer);
          t.Complete();
        }
      }
    }
  }
}
