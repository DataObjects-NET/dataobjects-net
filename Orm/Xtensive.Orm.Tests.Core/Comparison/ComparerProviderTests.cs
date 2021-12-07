// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Comparison
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
      SerializationTest<int[]>();
      SerializationTest<Guid>();
      SerializationTest<bool>();
      SerializationTest<byte>();
      SerializationTest<sbyte>();
      SerializationTest<short>();
      SerializationTest<int>();
      SerializationTest<long>();
      SerializationTest<ushort>();
      SerializationTest<uint>();
      SerializationTest<ulong>();
      SerializationTest<float>();
      SerializationTest<double>();
      SerializationTest<decimal>();
      SerializationTest<Direction>();
      SerializationTest<IEnumerable<int>>();
      SerializationTest<char>();
      SerializationTest<string>();
      SerializationTest<short?>();
      SerializationTest<Pair<int>>();
      SerializationTest<Pair<int, long>>();
      SerializationTest<Xtensive.Tuples.Tuple>();
    }

    private void SerializationTest<T>()
    {
      var comparer = AdvancedComparer<T>.Default;
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
      var count = 1000000;
      using (new Measurement("Getting default comparer", count)) {
        GettingDefaultComparerLoop(count);
      }
    }

    private static void GettingDefaultComparerLoop(int count)
    {
      for (var i = 0; i < count; i++) {
        _ = ComparerProvider.Default.GetComparer<bool>();
        _ = ComparerProvider.Default.GetComparer<byte>();
        _ = ComparerProvider.Default.GetComparer<char>();
        _ = ComparerProvider.Default.GetComparer<short>();
        _ = ComparerProvider.Default.GetComparer<ushort>();
        _ = ComparerProvider.Default.GetComparer<int>();
        _ = ComparerProvider.Default.GetComparer<uint>();
        _ = ComparerProvider.Default.GetComparer<long>();
        _ = ComparerProvider.Default.GetComparer<ulong>();
        _ = ComparerProvider.Default.GetComparer<string>();
      }
    }


    private class TestClass
    {
#pragma warning disable IDE0044, IDE0051, CS0169 // Add readonly modifier + Remove unused private members
      private string x;
      private int y;
#pragma warning restore IDE0044, IDE0051, CS0169 // Add readonly modifier + Remove unused private members
    }

    [Test]
    public void ClassTest()
    {
      _ = AdvancedComparerStruct<TestClass>.System;
    }

    [Test]
    public void Int32ComparerTest()
    {
      var o1 = 1;
      var o2 = 2;
      var comparer = AdvancedComparer<int>.Default;
      Assert.IsNotNull(comparer);
      Assert.AreEqual(nameof(Int32Comparer), comparer.Implementation.GetType().Name);
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
      var o1 = "1";
      var o2 = "2";
      var comparer = AdvancedComparer<string>.Default;
      Assert.IsNotNull(comparer);
      Assert.AreEqual("StringComparer", comparer.Implementation.GetType().Name);
      Assert.Greater(comparer.Compare(o2,o1), 0);
      Assert.IsTrue(comparer.ValueRangeInfo.HasMinValue);
      Assert.IsFalse(comparer.ValueRangeInfo.HasMaxValue);
      Assert.AreEqual(null, comparer.ValueRangeInfo.MinValue);
      AssertEx.ThrowsInvalidOperationException(delegate { var s = comparer.ValueRangeInfo.MaxValue; });

      var z = char.MaxValue;
      var y = unchecked ((char) (char.MaxValue - 1));
      var a = char.MinValue;

      var str = "BCD";
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
      AssertEx.Throws<ArgumentOutOfRangeException>(delegate { _ = comparer.GetNearestValue(str, Direction.None); });
    }

    [Test]
    public void NullableComparerTest()
    {
      var comparer = AdvancedComparer<int?>.Default;
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
      var comparer = AdvancedComparer<Direction>.System;
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
      var o1 = new Wrapper<int>(1);
      var o2 = new Wrapper<int>(2);
      var comparer = AdvancedComparer<Wrapper<int>>.Default;
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "WrapperComparer*");
      Assert.Greater(comparer.Compare(o2,o1), 0);
    }

    [Test]
    public void CustomComparerTest2()
    {
      var o1 = new Wrapper2<int, int>(0,1);
      var o2 = new Wrapper2<int, int>(0,2);
      var comparer = AdvancedComparer<Wrapper2<int, int>>.Default;
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "Wrapper2Comparer*");
      Assert.Greater(comparer.Compare(o2,o1), 0);
    }

    [Test]
    public void InheritedComparerTest()
    {
      var o1 = new Wrapper2a<int, int>(0,1);
      var o2 = new Wrapper2a<int, int>(0,2);
      var comparer = AdvancedComparer<Wrapper2a<int, int>>.Default;
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "BaseComparerWrapper*");
      Assert.AreEqual(comparer.Compare(o2,o1), 0);
    }

    [Test]
    public void CastingComparerTest()
    {
      var o1 = new Wrapper1<int>(1);
      var o2 = new Wrapper1<int>(2);
      var comparer = AdvancedComparer<Wrapper1<int>>.Default.Cast<Wrapper<int>>();
      Assert.IsNotNull(comparer);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "CastingComparer*");
      Assert.Greater(comparer.Compare(o2,o1), 0);
    }
  }
}