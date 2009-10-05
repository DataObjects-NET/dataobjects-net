// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.23

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Comparison;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Tests.Tuples
{
  [TestFixture]
  public class  TupleComparerTest
  {
    private Type[] types1 = new Type[] { typeof(int), typeof(string) };
    private Type[] types2 = new Type[] { typeof(int), typeof(string), typeof(DateTime) };

    private readonly AdvancedComparer<Tuple>  advancedComparer = AdvancedComparer<Tuple>.Default;
    private readonly IEqualityComparer<Tuple> equalityComparer = EqualityComparer<Tuple>.Default;
    private readonly IComparer<Tuple> comparer = Comparer<Tuple>.Default;

    [Test]
    public void EmptyTupleTest()
    {
      Tuple tuple1 = Tuple.Create(new Type[] {});
      Tuple tuple2 = Tuple.Create(new Type[] {});

      CheckComparisons(tuple1, tuple2, 0);
      CheckComparisons(tuple1, tuple1, 0);
    }

    [Test]
    public void ComparisonResultsTest()
    {
      Assert.AreEqual(2,  advancedComparer.Compare(Tuple.Create(1, 2),    Tuple.Create(1)));
      Assert.AreEqual(-2, advancedComparer.Compare(Tuple.Create(1, 2, 3), Tuple.Create(1, 3, 5)));
      Assert.AreEqual(-1, advancedComparer.Compare(Tuple.Create(1),       Tuple.Create(2)));
      Assert.AreEqual(-1, advancedComparer.Compare(Tuple.Create(1),       Tuple.Create(5)));

      Assert.AreEqual(2,  comparer.Compare(Tuple.Create(1, 2), Tuple.Create(1)));
      Assert.AreEqual(-2, comparer.Compare(Tuple.Create(1, 2, 3), Tuple.Create(1, 3, 5)));
      Assert.AreEqual(-1, comparer.Compare(Tuple.Create(1), Tuple.Create(2)));
      Assert.AreEqual(-1, comparer.Compare(Tuple.Create(1), Tuple.Create(5)));
    }

    [Test]
    public void DifferentSizeAndTypeTest()
    {
      CheckComparisons(Tuple.Create(1), Tuple.Create("1"), -1);
      CheckComparisons(Tuple.Create(1), Tuple.Create(1, "1"), -1);
      CheckComparisons(Tuple.Create(0), Tuple.Create(1, "1"), -1);
      CheckComparisons(Tuple.Create(1), Tuple.Create(0, "1"), 1);
    }

    [Test]
    public void DebugTest()
    {
      Tuple tuple1 = Tuple.Create(types1);
      Tuple tuple2 = Tuple.Create(types2);

      tuple1.SetValue(1, "test"); // null-"test" null-null-null
      tuple2.SetValue(1, "test"); // null-"test" null-"test"-null
      CheckComparisons(tuple1, tuple2, -1);;
    }

    [Test]
    public void BaseTest()
    {
      Assert.AreEqual("TupleComparer", advancedComparer.Implementation.GetType().Name);

      Tuple tuple1 = Tuple.Create(types1);
      Tuple tuple2 = Tuple.Create(types2);

      CheckComparisons(tuple1, tuple1, 0);
      CheckComparisons(tuple1, tuple2, -1);

      // Same length
      tuple1 = Tuple.Create(types1);
      tuple2 = Tuple.Create(types1);

      CheckComparisons(tuple1, tuple2, 0);

      tuple1.SetValue(1, "test"); // null-"test" null-null
      CheckComparisons(tuple1, tuple2, 1);

      tuple2.SetValue(1, "test"); // null-"test" null-"test"
      CheckComparisons(tuple1, tuple2, 0);

      tuple2.SetValue(1, null); // null-"test" null-null
      CheckComparisons(tuple1, tuple2, 1);

      tuple1.SetValue(0, 123); // 123-"test" null-null
      CheckComparisons(tuple1, tuple2, 1);

      tuple2.SetValue(0, 123); // 123-"test" 123-null
      CheckComparisons(tuple1, tuple2, 1);

      tuple2.SetValue(1, "test"); 
      tuple1.SetValue(1, null); // 123-null 123-"test"
      CheckComparisons(tuple1, tuple2, -1);
    }

    [Test]
    public void GuidTupleEqualsTest()
    {
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      Assert.IsFalse(guid1.Equals(guid2));
      var tuple1 = Tuple.Create(guid1);
      var tuple2 = Tuple.Create(guid2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void DateTimeTupleEqualsTest()
    {
      var dateTime1 = DateTime.Now;
      var dateTime2 = dateTime1.AddMinutes(1);
      Assert.IsFalse(dateTime1.Equals(dateTime2));
      var tuple1 = Tuple.Create(dateTime1);
      var tuple2 = Tuple.Create(dateTime2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void TimeSpanTupleEqualsTest()
    {
      var timeSpan1 = TimeSpan.FromMinutes(10);
      var timeSpan2 = TimeSpan.FromMinutes(15);
      Assert.IsFalse(timeSpan1.Equals(timeSpan2));
      var tuple1 = Tuple.Create(timeSpan1);
      var tuple2 = Tuple.Create(timeSpan2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void IntTupleEqualsTest()
    {
      var int1 = 10;
      var int2 = 15;
      Assert.IsFalse(int1.Equals(int2));
      var tuple1 = Tuple.Create(int1);
      var tuple2 = Tuple.Create(int2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void CharTupleEqualsTest()
    {
      var char1 = 'a';
      var char2 = 'b';
      Assert.IsFalse(char1.Equals(char2));
      var tuple1 = Tuple.Create(char1);
      var tuple2 = Tuple.Create(char2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void StringTupleEqualsTest()
    {
      var string1 = "a";
      var string2 = "b";
      Assert.IsFalse(string1.Equals(string2));
      var tuple1 = Tuple.Create(string1);
      var tuple2 = Tuple.Create(string2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    private void CheckComparisons(Tuple x, Tuple y, int expectedResult)
    {
      expectedResult = Normalize(expectedResult);
      bool boolResult = expectedResult==0;
      Assert.AreEqual(expectedResult, Normalize(comparer.Compare(x, y)));
      Assert.AreEqual(expectedResult, Normalize(x.CompareTo(y)));
      Assert.AreEqual(boolResult, x.Equals(y));
      Assert.AreEqual(boolResult, equalityComparer.Equals(x, y));
      Assert.AreEqual(expectedResult, Normalize(advancedComparer.Compare(x, y)));
      Assert.AreEqual(boolResult, advancedComparer.Equals(x, y));

      // Reverse
      expectedResult *= -1;
      Assert.AreEqual(expectedResult, Normalize(comparer.Compare(y, x)));
      Assert.AreEqual(expectedResult, Normalize(y.CompareTo(x)));
      Assert.AreEqual(boolResult, equalityComparer.Equals(y, x));
      Assert.AreEqual(boolResult, y.Equals(x));
      Assert.AreEqual(expectedResult, Normalize(advancedComparer.Compare(y, x)));
      Assert.AreEqual(boolResult, advancedComparer.Equals(y, x));
    }

    private static int Normalize(int i)
    {
      if (i<0)
        return -1;
      else if (i>0)
        return 1;
      else
        return 0;
    }
  }
}