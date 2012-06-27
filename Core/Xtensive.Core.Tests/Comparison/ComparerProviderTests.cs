// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Testing;

namespace Xtensive.Tests.Comparison
{
  [TestFixture]
  public class ComparerProviderTests
  {
    [Test]
    public void PairComparerTest()
    {
      Assert.IsNotNull(AdvancedComparer<Pair<Assembly, string>>.System);
      Assert.IsNotNull(AdvancedComparer<Pair<Assembly, string>>.Default);
    }

    [Test]
    public void SerializationTest()
    {
      AdvancedComparer<short> comparer = AdvancedComparer<short>.Default;
      Assert.IsNotNull(comparer.Compare);
      var deserializedComparer = Cloner.Clone(comparer);
      Assert.IsNotNull(deserializedComparer.Compare);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void GettingDefaultComparerPerformanceTest()
    {
      GettingDefaultComparerLoop(100);
      int count = 1000000;
      using (new Measurement("Getting default comparer", count)) {
        GettingDefaultComparerLoop(count);
      }
    }

    private static void GettingDefaultComparerLoop(int count)
    {
      for (int i = 0; i < count; i++) {
        ComparerProvider.Default.GetComparer<bool>();
        ComparerProvider.Default.GetComparer<byte>();
        ComparerProvider.Default.GetComparer<char>();
        ComparerProvider.Default.GetComparer<short>();
        ComparerProvider.Default.GetComparer<ushort>();
        ComparerProvider.Default.GetComparer<int>();
        ComparerProvider.Default.GetComparer<uint>();
        ComparerProvider.Default.GetComparer<long>();
        ComparerProvider.Default.GetComparer<ulong>();
        ComparerProvider.Default.GetComparer<string>();
      }
    }


    private class TestClass
    {
      private string x;
      private int y;
    }
     

    [Test]
    public void ClassTest()
    {
      AdvancedComparerStruct<TestClass> dd = AdvancedComparerStruct<TestClass>.System;
    }

    [Test]
    public void Int32ComparerTest()
    {
      int o1 = 1;
      int o2 = 2;
      AdvancedComparer<int> comparer = AdvancedComparer<int>.Default;
      Assert.IsNotNull(comparer);
      Assert.AreEqual("Int32Comparer", comparer.Implementation.GetType().Name);
      Assert.Greater(comparer.Compare(o2,o1), 0);
      Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
      Assert.IsTrue(comparer.ValueRangeInfo.HasMaxValue);
      Assert.AreEqual(int.MinValue, comparer.ValueRangeInfo.MinValue);
      Assert.AreEqual(int.MaxValue, comparer.ValueRangeInfo.MaxValue);
      Assert.AreEqual(1, comparer.ValueRangeInfo.DeltaValue);
      Assert.AreEqual(1, comparer.GetNearestValue(0, Direction.Positive));
      Assert.AreEqual(0, comparer.GetNearestValue(1, Direction.Negative));
    }

    [Test]
    public void StringComparerTest()
    {
      string o1 = "1";
      string o2 = "2";
      AdvancedComparer<string> comparer = AdvancedComparer<string>.Default;
      Assert.IsNotNull(comparer);
      Assert.AreEqual("StringComparer", comparer.Implementation.GetType().Name);
      Assert.Greater(comparer.Compare(o2,o1), 0);
      Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
      Assert.IsFalse(comparer.ValueRangeInfo.HasMaxValue);
      Assert.AreEqual(null, comparer.ValueRangeInfo.MinValue);
      AssertEx.ThrowsInvalidOperationException(delegate {string s = comparer.ValueRangeInfo.MaxValue;});

      char z = char.MaxValue;
      char y = unchecked ((char) (char.MaxValue - 1));
      char a = char.MinValue;
            
      string str = "BCD";
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.AreEqual("BCC" + z, str);
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.AreEqual("BCC" + y + z, str);
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.AreEqual("BCC" + y + y + z, str);
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.AreEqual("BCC" + y + z, str);
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.AreEqual("BCC" + z, str);
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.AreEqual("BCD", str);
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.AreEqual("BCD" + a, str);
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.AreEqual("BCD" + a + a, str);
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.AreEqual("BCD" + a, str);
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.AreEqual("BCD", str);
      Assert.IsNull(comparer.GetNearestValue(null, Direction.Negative));
      Assert.IsNull(comparer.GetNearestValue(string.Empty, Direction.Negative));
      Assert.AreEqual(string.Empty, comparer.GetNearestValue(null, Direction.Positive));
      Assert.AreEqual(char.MaxValue.ToString(), comparer.GetNearestValue(char.MaxValue.ToString(), Direction.Positive));
      AssertEx.Throws<ArgumentOutOfRangeException>(delegate { comparer.GetNearestValue(str, Direction.None); });
    }

//    [Test]
//    public void EntireComparerTest()
//    {
//      AdvancedComparer<Entire<int>> comparer = AdvancedComparer<Entire<int>>.Default;
//      Assert.IsNotNull(comparer);
//      Assert.AreEqual("EntireComparer`1", comparer.Implementation.GetType().Name);
//      Assert.AreEqual(comparer.Compare((Entire<int>)Entire<int>.Create(InfinityType.Positive), (Entire<int>)Entire<int>.Create(InfinityType.Positive)), 0);
//      Assert.AreEqual(comparer.Compare((Entire<int>)Entire<int>.Create(InfinityType.Negative), (Entire<int>)Entire<int>.Create(InfinityType.Negative)), 0);
//      Assert.Greater(comparer.Compare((Entire<int>)Entire<int>.Create(InfinityType.Positive), (Entire<int>)Entire<int>.Create(InfinityType.Negative)), 0);
//      Assert.Greater(comparer.Compare((Entire<int>)Entire<int>.Create(InfinityType.Positive), (Entire<int>)Entire<int>.Create(100)), 0);
//      Assert.Less(comparer.Compare((Entire<int>)Entire<int>.Create(InfinityType.Negative), (Entire<int>)Entire<int>.Create(InfinityType.Positive)), 0);
//      Assert.Less(comparer.Compare((Entire<int>)Entire<int>.Create(InfinityType.Negative), (Entire<int>)Entire<int>.Create(100)), 0);
//      Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
//      Assert.IsTrue(comparer.ValueRangeInfo.HasMaxValue);
//      Assert.IsTrue(comparer.ValueRangeInfo.HasDeltaValue);
//      Assert.AreEqual(Entire<int>.MinValue, comparer.ValueRangeInfo.MinValue);
//      Assert.AreEqual(Entire<int>.MaxValue, comparer.ValueRangeInfo.MaxValue);
//      Assert.AreEqual(Entire<int>.Create(1), comparer.ValueRangeInfo.DeltaValue);      
//    }

//    [Test]
//    public void EntireInterfaceComparerTest()
//    {
//      AdvancedComparer<IEntire<int>> comparer = AdvancedComparer<IEntire<int>>.Default;
//      Assert.IsNotNull(comparer);
//      Assert.AreEqual("EntireInterfaceComparer`1", comparer.Implementation.GetType().Name);
//      Assert.AreEqual(comparer.Compare(Entire<int>.Create(InfinityType.Positive), Entire<int>.Create(InfinityType.Positive)), 0);
//      Assert.AreEqual(comparer.Compare(Entire<int>.Create(InfinityType.Negative), Entire<int>.Create(InfinityType.Negative)), 0);
//      Assert.Greater(comparer.Compare(Entire<int>.Create(InfinityType.Positive), Entire<int>.Create(InfinityType.Negative)), 0);
//      Assert.Greater(comparer.Compare(Entire<int>.Create(InfinityType.Positive), Entire<int>.Create(100)), 0);
//      Assert.Less(comparer.Compare(Entire<int>.Create(InfinityType.Negative), Entire<int>.Create(InfinityType.Positive)), 0);
//      Assert.Less(comparer.Compare(Entire<int>.Create(InfinityType.Negative), Entire<int>.Create(100)), 0);
//      Assert.AreNotEqual(comparer.Compare(new Entire<int>(1, Direction.Positive), new Entire<int>(2)), 0);
//      Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
//      Assert.IsTrue(comparer.ValueRangeInfo.HasMaxValue);
//      Assert.IsTrue(comparer.ValueRangeInfo.HasDeltaValue);
//      Assert.AreEqual(Entire<int>.MinValue, comparer.ValueRangeInfo.MinValue);
//      Assert.AreEqual(Entire<int>.MaxValue, comparer.ValueRangeInfo.MaxValue);
//      Assert.AreEqual(Entire<int>.Create(1), comparer.ValueRangeInfo.DeltaValue);
//    }

//    [Test]
//    public void ReversedComparerTest()
//    {
//      {
//        AdvancedComparer<Reversed<int>> comparer = AdvancedComparer<Reversed<int>>.Default;
//        Assert.IsNotNull(comparer);
//        Assert.AreEqual("ReversedComparer`1", comparer.Implementation.GetType().Name);
//        Assert.Less(comparer.Compare(10, 1), 0);
//        Assert.Greater(comparer.Compare(1, 10), 0);
//        Assert.AreEqual(comparer.Compare(10, 10), 0);
//        Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
//        Assert.IsTrue(comparer.ValueRangeInfo.HasMaxValue);
//        Assert.IsTrue(comparer.ValueRangeInfo.HasDeltaValue);
//        Assert.AreEqual(new Reversed<int>(int.MaxValue), new Reversed<int>(int.MaxValue));
//        Assert.AreEqual(new Reversed<int>(int.MaxValue), comparer.ValueRangeInfo.MinValue);
//        Assert.AreEqual(new Reversed<int>(int.MinValue), comparer.ValueRangeInfo.MaxValue);
//        Assert.AreEqual(new Reversed<int>(1), comparer.ValueRangeInfo.DeltaValue);
//      }
//
//      {
//        AdvancedComparer<Reversed<int?>> comparer = AdvancedComparer<Reversed<int?>>.Default;
//        Assert.IsNotNull(comparer);
//        Assert.AreEqual("ReversedComparer`1", comparer.Implementation.GetType().Name);
//        Assert.Less(comparer.Compare(10, 1), 0);
//        Assert.Greater(comparer.Compare(1, 10), 0);
//        Assert.AreEqual(comparer.Compare(10, 10), 0);
//        Assert.Greater(comparer.Compare(1, 10), 0);
//        Assert.Less(comparer.Compare(10, 1), 0);
//        Assert.AreEqual(comparer.Compare(10, 10), 0);
//        Assert.Greater(comparer.Compare(null, 10), 0);
//        Assert.AreEqual(comparer.Compare(null, null), 0);
//        Assert.Less(comparer.Compare(10, null), 0);
//        Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
//        Assert.IsTrue(comparer.ValueRangeInfo.HasMaxValue);
//        Assert.IsTrue(comparer.ValueRangeInfo.HasDeltaValue);
//        Assert.AreEqual(new Reversed<int?>(int.MaxValue), comparer.ValueRangeInfo.MinValue);
//        Assert.AreEqual(new Reversed<int?>(), comparer.ValueRangeInfo.MaxValue);
//        Assert.AreEqual(new Reversed<int?>(1), comparer.ValueRangeInfo.DeltaValue);
//      }
//    }

    [Test]
    public void NullableComparerTest()
    {
      AdvancedComparer<int?> comparer = AdvancedComparer<int?>.Default;
      Assert.IsNotNull(comparer);
      Assert.AreEqual("NullableComparer`1", comparer.Implementation.GetType().Name);
      Assert.Greater(comparer.Compare(10, 1), 0);
      Assert.Less(comparer.Compare(1, 10), 0);
      Assert.AreEqual(comparer.Compare(10, 10), 0);
      Assert.Less(comparer.Compare(1, 10), 0);
      Assert.Greater(comparer.Compare(10, 1), 0);
      Assert.AreEqual(comparer.Compare(10, 10), 0);
      Assert.Less(comparer.Compare(null, 10), 0);
      Assert.AreEqual(comparer.Compare(null, null), 0);
      Assert.Greater(comparer.Compare(10, null), 0);
      Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
      Assert.IsTrue(comparer.ValueRangeInfo.HasMaxValue);
      Assert.IsTrue(comparer.ValueRangeInfo.HasDeltaValue);
      Assert.AreEqual(null, comparer.ValueRangeInfo.MinValue);
      Assert.AreEqual(int.MaxValue, comparer.ValueRangeInfo.MaxValue);
      Assert.AreEqual(1, comparer.ValueRangeInfo.DeltaValue);
    }

    [Test]
    public void EnumComparerTest()
    {
      AdvancedComparer<Direction> comparer = AdvancedComparer<Direction>.System;
      Assert.IsNotNull(comparer);
      Assert.AreEqual("EnumComparer`2", comparer.Implementation.GetType().Name);
      Assert.Greater(comparer.Compare(Direction.Positive, Direction.Negative), 0);
      Assert.Greater(comparer.Compare(Direction.Positive, Direction.None), 0);
      Assert.Less(comparer.Compare(Direction.Negative, Direction.Positive), 0);
      Assert.Less(comparer.Compare(Direction.Negative, Direction.None), 0);
      Assert.AreEqual(comparer.Compare(Direction.Positive, Direction.Positive), 0);
      Assert.AreEqual(comparer.Compare(Direction.None, Direction.None), 0);
      Assert.AreEqual(comparer.Compare(Direction.Negative, Direction.Negative), 0);

      Assert.IsTrue(comparer.Equals(comparer.ValueRangeInfo.MaxValue, Direction.Positive));
      Assert.IsTrue(comparer.Equals(comparer.ValueRangeInfo.MinValue, Direction.Negative));
      Assert.IsFalse(comparer.Equals(Direction.Positive, Direction.Negative));

      Assert.AreEqual(comparer.GetNearestValue(Direction.Negative, Direction.Negative), Direction.Negative);
      Assert.AreEqual(comparer.GetNearestValue(Direction.Negative, Direction.Positive), Direction.None);
      Assert.AreEqual(comparer.GetNearestValue(Direction.Positive, Direction.Negative), Direction.None);
      Assert.AreEqual(comparer.GetNearestValue(Direction.Positive, Direction.Positive), Direction.Positive);
      Assert.AreEqual(comparer.GetNearestValue(Direction.None, Direction.Negative), Direction.Negative);
      Assert.AreEqual(comparer.GetNearestValue(Direction.None, Direction.Positive), Direction.Positive);
      Assert.IsFalse(comparer.ValueRangeInfo.HasDeltaValue);
    }

    [Test]
    public void CustomComparerTest()
    {
      Wrapper<int> o1 = new Wrapper<int>(1);
      Wrapper<int> o2 = new Wrapper<int>(2);
      AdvancedComparer<Wrapper<int>> comparer = AdvancedComparer<Wrapper<int>>.Default;
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "WrapperComparer*");
      Assert.Greater(comparer.Compare(o2,o1), 0);
    }

    [Test]
    public void CustomComparerTest2()
    {
      Wrapper2<int, int> o1 = new Wrapper2<int, int>(0,1);
      Wrapper2<int, int> o2 = new Wrapper2<int, int>(0,2);
      AdvancedComparer<Wrapper2<int, int>> comparer = AdvancedComparer<Wrapper2<int, int>>.Default;
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "Wrapper2Comparer*");
      Assert.Greater(comparer.Compare(o2,o1), 0);
    }

    [Test]
    public void InheritedComparerTest()
    {
      Wrapper2a<int, int> o1 = new Wrapper2a<int, int>(0,1);
      Wrapper2a<int, int> o2 = new Wrapper2a<int, int>(0,2);
      AdvancedComparer<Wrapper2a<int, int>> comparer = AdvancedComparer<Wrapper2a<int, int>>.Default;
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "BaseComparerWrapper*");
      Assert.AreEqual(comparer.Compare(o2,o1), 0);
    }

    [Test]
    public void CastingComparerTest()
    {
      Wrapper1<int> o1 = new Wrapper1<int>(1);
      Wrapper1<int> o2 = new Wrapper1<int>(2);
      AdvancedComparer<Wrapper<int>> comparer = AdvancedComparer<Wrapper1<int>>.Default.Cast<Wrapper<int>>();
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "CastingComparer*");
      Assert.Greater(comparer.Compare(o2,o1), 0);
    }

//    [Test]
//    public void TupleEntireComparerTest()
//    {
//      List<Type> fields = new List<Type>(3);
//      fields.AddRange(new Type[] {typeof(int), typeof(bool), typeof(string)});
//      TupleDescriptor tupleDescriptor = TupleDescriptor.Create(fields);
//      Tuple tuple = Tuple.Create(tupleDescriptor);
//      tuple.SetValue(0, 1);
//      tuple.SetValue(1, true);
//      tuple.SetValue(2, "TupleEntire");
//      IEntire<Tuple> entire1 = Entire<Tuple>.Create(tuple);
//      IEntire<Tuple> entire2 = Entire<Tuple>.Create(tuple);
//      AdvancedComparer<IEntire<Tuple>> comparer = AdvancedComparer<IEntire<Tuple>>.Default;
//      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "EntireInterfaceComparer*");
//
////      entire2.Shift(1);
////      Assert.IsFalse(c.Equals(entire1, entire2));
////      Assert.AreEqual(-1, c.Compare(entire1, entire2));
////
////      entire1.Shift();
////      Assert.IsFalse(c.Equals(entire1, entire2));
////      Assert.AreEqual(1, c.Compare(entire1, entire2));
////
////      entire2.Shift();
////      Assert.IsTrue(c.Equals(entire1, entire2));
////      Assert.AreEqual(0, c.Compare(entire1, entire2));
//    }

    int ToReflect1(Direction d)
    {
      return (int) d;
    }

    Direction ToReflect2(int i)
    {
      return (Direction) i;
    }
  }
}