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