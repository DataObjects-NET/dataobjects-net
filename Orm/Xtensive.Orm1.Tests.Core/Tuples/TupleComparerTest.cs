// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.23

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  [TestFixture]
  public class  TupleComparerTest
  {
    private Type[] types1 = new Type[] { typeof(int), typeof(string) };
    private Type[] types2 = new Type[] { typeof(int), typeof(string), typeof(DateTime) };

    private readonly AdvancedComparer<Xtensive.Tuples.Tuple>  advancedComparer = AdvancedComparer<Xtensive.Tuples.Tuple>.Default;
    private readonly IEqualityComparer<Xtensive.Tuples.Tuple> equalityComparer = EqualityComparer<Xtensive.Tuples.Tuple>.Default;
    private readonly IComparer<Xtensive.Tuples.Tuple> comparer = Comparer<Xtensive.Tuples.Tuple>.Default;

    bool ManualEquals(Xtensive.Tuples.Tuple x, Xtensive.Tuples.Tuple y)
    {
      if (y != null) {
        TupleFieldState state;
        TupleFieldState state2;
        if (x == y)
          return true;
        if (x.Descriptor != y.Descriptor)
          return false;
        var flag = x.GetValue<int>(0, out state) == y.GetValue<int>(0, out state2);
        if ((state == state2) && ((state != TupleFieldState.Available) || flag))
          return true;
      }
      return false;
    }

    [Test]
    public void EqualityTest()
    {
      var x = Xtensive.Tuples.Tuple.Create(10);
      var y = Xtensive.Tuples.Tuple.Create(10);
      Assert.IsTrue(ManualEquals(x,y));
      Assert.AreEqual(x,y);
    }

    [Test]
    public void EmptyTupleTest()
    {
      Xtensive.Tuples.Tuple tuple1 = Xtensive.Tuples.Tuple.Create(new Type[] {});
      Xtensive.Tuples.Tuple tuple2 = Xtensive.Tuples.Tuple.Create(new Type[] {});

      CheckComparisons(tuple1, tuple2, 0);
      CheckComparisons(tuple1, tuple1, 0);
    }

    [Test]
    public void DifferentSizeAndTypeTest()
    {
      CheckComparisons(Xtensive.Tuples.Tuple.Create(1), Xtensive.Tuples.Tuple.Create("1"), -1);
      CheckComparisons(Xtensive.Tuples.Tuple.Create(1), Xtensive.Tuples.Tuple.Create(1, "1"), -1);
      CheckComparisons(Xtensive.Tuples.Tuple.Create(0), Xtensive.Tuples.Tuple.Create(1, "1"), -1);
      CheckComparisons(Xtensive.Tuples.Tuple.Create(1), Xtensive.Tuples.Tuple.Create(0, "1"), 1);
    }

    [Test]
    public void DebugTest()
    {
      Xtensive.Tuples.Tuple tuple1 = Xtensive.Tuples.Tuple.Create(types1);
      Xtensive.Tuples.Tuple tuple2 = Xtensive.Tuples.Tuple.Create(types2);

      tuple1.SetValue(1, "test"); // null-"test" null-null-null
      tuple2.SetValue(1, "test"); // null-"test" null-"test"-null
      CheckComparisons(tuple1, tuple2, -1);;
    }

    [Test]
    public void BaseTest()
    {
      Assert.AreEqual("TupleComparer", advancedComparer.Implementation.GetType().Name);

      Xtensive.Tuples.Tuple tuple1 = Xtensive.Tuples.Tuple.Create(types1);
      Xtensive.Tuples.Tuple tuple2 = Xtensive.Tuples.Tuple.Create(types2);

      CheckComparisons(tuple1, tuple1, 0);
      CheckComparisons(tuple1, tuple2, -1);

      // Same length
      tuple1 = Xtensive.Tuples.Tuple.Create(types1);
      tuple2 = Xtensive.Tuples.Tuple.Create(types1);

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
      var tuple1 = Xtensive.Tuples.Tuple.Create(guid1);
      var tuple2 = Xtensive.Tuples.Tuple.Create(guid2);
      var condition = tuple1.Equals(tuple2);
      Assert.IsFalse(condition);
    }

    [Test]
    public void DateTimeTupleEqualsTest()
    {
      var dateTime1 = DateTime.Now;
      var dateTime2 = dateTime1.AddMinutes(1);
      Assert.IsFalse(dateTime1.Equals(dateTime2));
      var tuple1 = Xtensive.Tuples.Tuple.Create(dateTime1);
      var tuple2 = Xtensive.Tuples.Tuple.Create(dateTime2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void TimeSpanTupleEqualsTest()
    {
      var timeSpan1 = TimeSpan.FromMinutes(10);
      var timeSpan2 = TimeSpan.FromMinutes(15);
      Assert.IsFalse(timeSpan1.Equals(timeSpan2));
      var tuple1 = Xtensive.Tuples.Tuple.Create(timeSpan1);
      var tuple2 = Xtensive.Tuples.Tuple.Create(timeSpan2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void IntTupleEqualsTest()
    {
      var int1 = 10;
      var int2 = 15;
      Assert.IsFalse(int1.Equals(int2));
      var tuple1 = Xtensive.Tuples.Tuple.Create(int1);
      var tuple2 = Xtensive.Tuples.Tuple.Create(int2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void CharTupleEqualsTest()
    {
      var char1 = 'a';
      var char2 = 'b';
      Assert.IsFalse(char1.Equals(char2));
      var tuple1 = Xtensive.Tuples.Tuple.Create(char1);
      var tuple2 = Xtensive.Tuples.Tuple.Create(char2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    [Test]
    public void StringTupleEqualsTest()
    {
      var string1 = "a";
      var string2 = "b";
      Assert.IsFalse(string1.Equals(string2));
      var tuple1 = Xtensive.Tuples.Tuple.Create(string1);
      var tuple2 = Xtensive.Tuples.Tuple.Create(string2);
      Assert.IsFalse(tuple1.Equals(tuple2));
    }

    private void CheckComparisons(Xtensive.Tuples.Tuple x, Xtensive.Tuples.Tuple y, int expectedResult)
    {
      expectedResult = Normalize(expectedResult);
      bool boolResult = expectedResult==0;
      Assert.AreEqual(boolResult, x.Equals(y));
      Assert.AreEqual(boolResult, equalityComparer.Equals(x, y));
      Assert.AreEqual(boolResult, advancedComparer.Equals(x, y));

      // Reverse
      expectedResult *= -1;
      Assert.AreEqual(boolResult, equalityComparer.Equals(y, x));
      Assert.AreEqual(boolResult, y.Equals(x));
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