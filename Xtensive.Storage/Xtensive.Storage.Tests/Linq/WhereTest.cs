// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class WhereTest : NorthwindDOModelTest
  {
    private Key supplier20Key;
    private Key category1Key;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (Domain.OpenSession()) {
        supplier20Key = Query<Supplier>.All.Single(s => s.Id == 20).Key;
        category1Key = Query<Category>.All.Single(c => c.Id == 1).Key;
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
          var supplier = suppliers.Where(s => s.Id == 20 ).First();
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
          var key = Key.Create<Supplier>(Tuple.Create(20));
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
          var supplier20 = supplier20Key.Resolve<Supplier>();
          var suppliers = Query<Supplier>.All;
          var supplier = suppliers.Where(s => s == supplier20).First();
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
          var supplier20 = supplier20Key.Resolve<Supplier>();
          var products = Query<Product>.All;
          var product = products.Where(p => p.Supplier.Key == supplier20.Key).First();
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
          var supplier20 = supplier20Key.Resolve<Supplier>();
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
          var supplier20 = supplier20Key.Resolve<Supplier>();
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
          product = products.Where(p => p.Supplier.CompanyName == "Leka Trading" && p.Category.Key == category1Key && p.Supplier.ContactTitle == "Owner").First();
          Assert.IsNotNull(product);
          Assert.AreEqual("Leka Trading", product.Supplier.CompanyName);
          t.Complete();
        }
      }
    }

    /*
     *         public void TestStringLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Length == 7));
        }

        public void TestStringStartsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith("M")));
        }

        public void TestStringStartsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.StartsWith(c.ContactName)));
        }

        public void TestStringEndsWithLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith("s")));
        }

        public void TestStringEndsWithColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.EndsWith(c.ContactName)));
        }

        public void TestStringContainsLiteral()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains("and")));
        }

        public void TestStringContainsColumn()
        {
            TestQuery(db.Customers.Where(c => c.ContactName.Contains(c.ContactName)));
        }

        public void TestStringConcatImplicit2Args()
        {
            TestQuery(db.Customers.Where(c => c.ContactName + "X" == "X"));
        }

        public void TestStringConcatExplicit2Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X") == "X"));
        }

        public void TestStringConcatExplicit3Args()
        {
            TestQuery(db.Customers.Where(c => string.Concat(c.ContactName, "X", c.Country) == "X"));
        }

        public void TestStringConcatExplicitNArgs()
        {
            TestQuery(db.Customers.Where(c => string.Concat(new string[] { c.ContactName, "X", c.Country }) == "X"));
        }

        public void TestStringIsNullOrEmpty()
        {
            TestQuery(db.Customers.Where(c => string.IsNullOrEmpty(c.City)));
        }

        public void TestStringToUpper()
        {
            TestQuery(db.Customers.Where(c => c.City.ToUpper() == "SEATTLE"));
        }

        public void TestStringToLower()
        {
            TestQuery(db.Customers.Where(c => c.City.ToLower() == "seattle"));
        }

        public void TestStringReplace()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("ea", "ae") == "Saettle"));
        }

        public void TestStringReplaceChars()
        {
            TestQuery(db.Customers.Where(c => c.City.Replace("e", "y") == "Syattly"));
        }

        public void TestStringSubstring()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(0, 4) == "Seat"));
        }

        public void TestStringSubstringNoLength()
        {
            TestQuery(db.Customers.Where(c => c.City.Substring(4) == "tle"));
        }

        public void TestStringRemove()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(1, 2) == "Sttle"));
        }

        public void TestStringRemoveNoCount()
        {
            TestQuery(db.Customers.Where(c => c.City.Remove(4) == "Seat"));
        }

        public void TestStringIndexOf()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf("tt") == 4));
        }

        public void TestStringIndexOfChar()
        {
            TestQuery(db.Customers.Where(c => c.City.IndexOf('t') == 4));
        }

        public void TestStringTrim()
        {
            TestQuery(db.Customers.Where(c => c.City.Trim() == "Seattle"));
        }

        public void TestStringToString()
        {
            // just to prove this is a no op
            TestQuery(db.Customers.Where(c => c.City.ToString() == "Seattle"));
        }

        public void TestDateTimeConstructYMD()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Year, 1, 1)));
        }

        public void TestDateTimeConstructYMDHMS()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate == new DateTime(o.OrderDate.Year, 1, 1, 10, 25, 55)));
        }

        public void TestDateTimeDay()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Day == 5));
        }

        public void TestDateTimeMonth()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Month == 12));
        }

        public void TestDateTimeYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Year == 1997));
        }

        public void TestDateTimeHour()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Hour == 6));
        }

        public void TestDateTimeMinute()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Minute == 32));
        }

        public void TestDateTimeSecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Second == 47));
        }

        public void TestDateTimeMillisecond()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.Millisecond == 200));
        }

        public void TestDateTimeDayOfWeek()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.DayOfWeek == DayOfWeek.Friday));
        }

        public void TestDateTimeDayOfYear()
        {
            TestQuery(db.Orders.Where(o => o.OrderDate.DayOfYear == 360));
        }

        public void TestMathAbs()
        {
            TestQuery(db.Orders.Where(o => Math.Abs(o.OrderID) == 10));
        }

        public void TestMathAcos()
        {
            TestQuery(db.Orders.Where(o => Math.Acos(o.OrderID) == 0));
        }

        public void TestMathAsin()
        {
            TestQuery(db.Orders.Where(o => Math.Asin(o.OrderID) == 0));
        }

        public void TestMathAtan()
        {
            TestQuery(db.Orders.Where(o => Math.Atan(o.OrderID) == 0));
        }

        public void TestMathAtan2()
        {
            TestQuery(db.Orders.Where(o => Math.Atan2(o.OrderID, 3) == 0));
        }

        public void TestMathCos()
        {
            TestQuery(db.Orders.Where(o => Math.Cos(o.OrderID) == 0));
        }

        public void TestMathSin()
        {
            TestQuery(db.Orders.Where(o => Math.Sin(o.OrderID) == 0));
        }

        public void TestMathTan()
        {
            TestQuery(db.Orders.Where(o => Math.Tan(o.OrderID) == 0));
        }

        public void TestMathExp()
        {
            TestQuery(db.Orders.Where(o => Math.Exp(o.OrderID < 1000 ? 1 : 2) == 0));
        }

        public void TestMathLog()
        {
            TestQuery(db.Orders.Where(o => Math.Log(o.OrderID) == 0));
        }

        public void TestMathLog10()
        {
            TestQuery(db.Orders.Where(o => Math.Log10(o.OrderID) == 0));
        }

        public void TestMathSqrt()
        {
            TestQuery(db.Orders.Where(o => Math.Sqrt(o.OrderID) == 0));
        }

        public void TestMathCeiling()
        {
            TestQuery(db.Orders.Where(o => Math.Ceiling((double)o.OrderID) == 0));
        }

        public void TestMathFloor()
        {
            TestQuery(db.Orders.Where(o => Math.Floor((double)o.OrderID) == 0));
        }

        public void TestMathPow()
        {
            TestQuery(db.Orders.Where(o => Math.Pow(o.OrderID < 1000 ? 1 : 2, 3) == 0));
        }

        public void TestMathRoundDefault()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID) == 0));
        }

        public void TestMathRoundToPlace()
        {
            TestQuery(db.Orders.Where(o => Math.Round((decimal)o.OrderID, 2) == 0));
        }

        public void TestMathTruncate()
        {
            TestQuery(db.Orders.Where(o => Math.Truncate((double)o.OrderID) == 0));
        }

        public void TestStringCompareToLT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") < 0));
        }

        public void TestStringCompareToLE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") <= 0));
        }

        public void TestStringCompareToGT()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") > 0));
        }

        public void TestStringCompareToGE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") >= 0));
        }

        public void TestStringCompareToEQ()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") == 0));
        }

        public void TestStringCompareToNE()
        {
            TestQuery(db.Customers.Where(c => c.City.CompareTo("Seattle") != 0));
        }

        public void TestStringCompareLT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") < 0));
        }

        public void TestStringCompareLE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") <= 0));
        }

        public void TestStringCompareGT()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") > 0));
        }

        public void TestStringCompareGE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") >= 0));
        }

        public void TestStringCompareEQ()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") == 0));
        }

        public void TestStringCompareNE()
        {
            TestQuery(db.Customers.Where(c => string.Compare(c.City, "Seattle") != 0));
        }

        public void TestIntCompareTo()
        {
            // prove that x.CompareTo(y) works for types other than string
            TestQuery(db.Orders.Where(o => o.OrderID.CompareTo(1000) == 0));
        }

        public void TestDecimalCompare()
        {
            // prove that type.Compare(x,y) works with decimal
            TestQuery(db.Orders.Where(o => decimal.Compare((decimal)o.OrderID, 0.0m) == 0));
        }

        public void TestDecimalAdd()
        {
            TestQuery(db.Orders.Where(o => decimal.Add(o.OrderID, 0.0m) == 0.0m));
        }

        public void TestDecimalSubtract()
        {
            TestQuery(db.Orders.Where(o => decimal.Subtract(o.OrderID, 0.0m) == 0.0m));
        }

        public void TestDecimalMultiply()
        {
            TestQuery(db.Orders.Where(o => decimal.Multiply(o.OrderID, 1.0m) == 1.0m));
        }

        public void TestDecimalDivide()
        {
            TestQuery(db.Orders.Where(o => decimal.Divide(o.OrderID, 1.0m) == 1.0m));
        }

        public void TestDecimalRemainder()
        {
            TestQuery(db.Orders.Where(o => decimal.Remainder(o.OrderID, 1.0m) == 0.0m));
        }

        public void TestDecimalNegate()
        {
            TestQuery(db.Orders.Where(o => decimal.Negate(o.OrderID) == 1.0m));
        }

        public void TestDecimalCeiling()
        {
            TestQuery(db.Orders.Where(o => decimal.Ceiling(o.OrderID) == 0.0m));
        }

        public void TestDecimalFloor()
        {
            TestQuery(db.Orders.Where(o => decimal.Floor(o.OrderID) == 0.0m));
        }

        public void TestDecimalRoundDefault()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID) == 0m));
        }

        public void TestDecimalRoundPlaces()
        {
            TestQuery(db.Orders.Where(o => decimal.Round(o.OrderID, 2) == 0.00m));
        }

        public void TestDecimalTruncate()
        {
            TestQuery(db.Orders.Where(o => decimal.Truncate(o.OrderID) == 0m));
        }

        public void TestDecimalLT()
        {
            // prove that decimals are treated normally with respect to normal comparison operators
            TestQuery(db.Orders.Where(o => ((decimal)o.OrderID) < 0.0m));
        }

        public void TestIntLessThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 0));
        }

        public void TestIntLessThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID <= 0));
        }

        public void TestIntGreaterThan()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0));
        }

        public void TestIntGreaterThanOrEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >= 0));
        }

        public void TestIntEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID == 0));
        }

        public void TestIntNotEqual()
        {
            TestQuery(db.Orders.Where(o => o.OrderID != 0));
        }

        public void TestIntAdd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID + 0 == 0));
        }

        public void TestIntSubtract()
        {
            TestQuery(db.Orders.Where(o => o.OrderID - 0 == 0));
        }

        public void TestIntMultiply()
        {
            TestQuery(db.Orders.Where(o => o.OrderID * 1 == 1));
        }

        public void TestIntDivide()
        {
            TestQuery(db.Orders.Where(o => o.OrderID / 1 == 1));
        }

        public void TestIntModulo()
        {
            TestQuery(db.Orders.Where(o => o.OrderID % 1 == 0));
        }

        public void TestIntLeftShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID << 1 == 0));
        }

        public void TestIntRightShift()
        {
            TestQuery(db.Orders.Where(o => o.OrderID >> 1 == 0));
        }

        public void TestIntBitwiseAnd()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID & 1) == 0));
        }

        public void TestIntBitwiseOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID | 1) == 1));
        }

        public void TestIntBitwiseExclusiveOr()
        {
            TestQuery(db.Orders.Where(o => (o.OrderID ^ 1) == 1));
        }

        public void TestIntBitwiseNot()
        {
            TestQuery(db.Orders.Where(o => ~o.OrderID == 0));
        }

        public void TestIntNegate()
        {
            TestQuery(db.Orders.Where(o => -o.OrderID == -1));
        }

        public void TestAnd()
        {
            TestQuery(db.Orders.Where(o => o.OrderID > 0 && o.OrderID < 2000));
        }

        public void TestOr()
        {
            TestQuery(db.Orders.Where(o => o.OrderID < 5 || o.OrderID > 10));
        }

        public void TestNot()
        {
            TestQuery(db.Orders.Where(o => !(o.OrderID == 0)));
        }
     * 
     * public void TestEqualNull()
            {
                TestQuery(db.Customers.Where(c => c.City == null));
            }

            public void TestEqualNullReverse()
            {
                TestQuery(db.Customers.Where(c => null == c.City));
            }

            public void TestCoalsce()
            {
                TestQuery(db.Customers.Where(c => (c.City ?? "Seattle") == "Seattle"));
            }

            public void TestCoalesce2()
            {
                TestQuery(db.Customers.Where(c => (c.City ?? c.Country ?? "Seattle") == "Seattle"));
            }

            public void TestConditional()
            {
                TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : 0) == 1000));
            }

            public void TestConditional2()
            {
                TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? 1000 : o.CustomerID == "ABCDE" ? 2000 : 0) == 1000));
            }

            public void TestConditionalTestIsValue()
            {
                TestQuery(db.Orders.Where(o => (((bool)(object)o.OrderID) ? 100 : 200) == 100));
            }

            public void TestConditionalResultsArePredicates()
            {
                TestQuery(db.Orders.Where(o => (o.CustomerID == "ALFKI" ? o.OrderID < 10 : o.OrderID > 10)));
            }
    */

  }
}