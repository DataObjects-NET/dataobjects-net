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
      Assert.That(AdvancedComparer<Pair<Assembly, string>>.System, Is.Not.Null);
      Assert.That(AdvancedComparer<Pair<Assembly, string>>.Default, Is.Not.Null);
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
      Assert.That(comparer.Compare, Is.Not.Null);
      var deserializedComparer = Cloner.Clone(comparer);
      Assert.That(deserializedComparer.Compare, Is.Not.Null);
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
      Assert.That(comparer, Is.Not.Null);
      Assert.That(comparer.Implementation.GetType().Name, Is.EqualTo(nameof(Int32Comparer)));
      Assert.That(comparer.Compare(o2,o1), Is.GreaterThan(0));
      Assert.That(comparer.ValueRangeInfo.HasMinValue, Is.True);
      Assert.That(comparer.ValueRangeInfo.HasMaxValue, Is.True);
      Assert.That(comparer.ValueRangeInfo.MinValue, Is.EqualTo(int.MinValue));
      Assert.That(comparer.ValueRangeInfo.MaxValue, Is.EqualTo(int.MaxValue));
      Assert.That(comparer.ValueRangeInfo.DeltaValue, Is.EqualTo(1));
      Assert.That(comparer.GetNearestValue(0, Direction.Positive), Is.EqualTo(1));
      Assert.That(comparer.GetNearestValue(1, Direction.Negative), Is.EqualTo(0));
    }

    [Test]
    public void StringComparerTest()
    {
      var o1 = "1";
      var o2 = "2";
      var comparer = AdvancedComparer<string>.Default;
      Assert.That(comparer, Is.Not.Null);
      Assert.That(comparer.Implementation.GetType().Name, Is.EqualTo("StringComparer"));
      Assert.That(comparer.Compare(o2,o1), Is.GreaterThan(0));
      Assert.That(comparer.ValueRangeInfo.HasMinValue, Is.True);
      Assert.That(comparer.ValueRangeInfo.HasMaxValue, Is.False);
      Assert.That(comparer.ValueRangeInfo.MinValue, Is.EqualTo(null));
      AssertEx.ThrowsInvalidOperationException(delegate { var s = comparer.ValueRangeInfo.MaxValue; });

      var z = char.MaxValue;
      var y = unchecked ((char) (char.MaxValue - 1));
      var a = char.MinValue;

      var str = "BCD";
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.That(str, Is.EqualTo("BCC" + z));
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.That(str, Is.EqualTo("BCC" + y + z));
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.That(str, Is.EqualTo("BCC" + y + y + z));
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.That(str, Is.EqualTo("BCC" + y + z));
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.That(str, Is.EqualTo("BCC" + z));
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.That(str, Is.EqualTo("BCD"));
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.That(str, Is.EqualTo("BCD" + a));
      str = comparer.GetNearestValue(str, Direction.Positive);
      Assert.That(str, Is.EqualTo("BCD" + a + a));
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.That(str, Is.EqualTo("BCD" + a));
      str = comparer.GetNearestValue(str, Direction.Negative);
      Assert.That(str, Is.EqualTo("BCD"));
      Assert.That(comparer.GetNearestValue(null, Direction.Negative), Is.Null);
      Assert.That(comparer.GetNearestValue(string.Empty, Direction.Negative), Is.Null);
      Assert.That(comparer.GetNearestValue(null, Direction.Positive), Is.EqualTo(string.Empty));
      Assert.That(comparer.GetNearestValue(char.MaxValue.ToString(), Direction.Positive), Is.EqualTo(char.MaxValue.ToString()));
      AssertEx.Throws<ArgumentOutOfRangeException>(delegate { _ = comparer.GetNearestValue(str, Direction.None); });
    }

    [Test]
    public void NullableComparerTest()
    {
      var comparer = AdvancedComparer<int?>.Default;
      Assert.That(comparer, Is.Not.Null);
      Assert.That(comparer.Implementation.GetType().Name, Is.EqualTo("NullableComparer`1"));
      Assert.That(comparer.Compare(10, 1), Is.GreaterThan(0));
      Assert.That(comparer.Compare(1, 10), Is.LessThan(0));
      Assert.That(0, Is.EqualTo(comparer.Compare(10, 10)));
      Assert.That(comparer.Compare(1, 10), Is.LessThan(0));
      Assert.That(comparer.Compare(10, 1), Is.GreaterThan(0));
      Assert.That(0, Is.EqualTo(comparer.Compare(10, 10)));
      Assert.That(comparer.Compare(null, 10), Is.LessThan(0));
      Assert.That(0, Is.EqualTo(comparer.Compare(null, null)));
      Assert.That(comparer.Compare(10, null), Is.GreaterThan(0));
      Assert.That(comparer.ValueRangeInfo.HasMinValue, Is.True);
      Assert.That(comparer.ValueRangeInfo.HasMaxValue, Is.True);
      Assert.That(comparer.ValueRangeInfo.HasDeltaValue, Is.True);
      Assert.That(comparer.ValueRangeInfo.MinValue, Is.EqualTo(null));
      Assert.That(comparer.ValueRangeInfo.MaxValue, Is.EqualTo(int.MaxValue));
      Assert.That(comparer.ValueRangeInfo.DeltaValue, Is.EqualTo(1));
    }

    [Test]
    public void EnumComparerTest()
    {
      var comparer = AdvancedComparer<Direction>.System;
      Assert.That(comparer, Is.Not.Null);
      Assert.That(comparer.Implementation.GetType().Name, Is.EqualTo("EnumComparer`2"));
      Assert.That(comparer.Compare(Direction.Positive, Direction.Negative), Is.GreaterThan(0));
      Assert.That(comparer.Compare(Direction.Positive, Direction.None), Is.GreaterThan(0));
      Assert.That(comparer.Compare(Direction.Negative, Direction.Positive), Is.LessThan(0));
      Assert.That(comparer.Compare(Direction.Negative, Direction.None), Is.LessThan(0));
      Assert.That(0, Is.EqualTo(comparer.Compare(Direction.Positive, Direction.Positive)));
      Assert.That(0, Is.EqualTo(comparer.Compare(Direction.None, Direction.None)));
      Assert.That(0, Is.EqualTo(comparer.Compare(Direction.Negative, Direction.Negative)));

      Assert.That(comparer.Equals(comparer.ValueRangeInfo.MaxValue, Direction.Positive), Is.True);
      Assert.That(comparer.Equals(comparer.ValueRangeInfo.MinValue, Direction.Negative), Is.True);
      Assert.That(comparer.Equals(Direction.Positive, Direction.Negative), Is.False);

      Assert.That(Direction.Negative, Is.EqualTo(comparer.GetNearestValue(Direction.Negative, Direction.Negative)));
      Assert.That(Direction.None, Is.EqualTo(comparer.GetNearestValue(Direction.Negative, Direction.Positive)));
      Assert.That(Direction.None, Is.EqualTo(comparer.GetNearestValue(Direction.Positive, Direction.Negative)));
      Assert.That(Direction.Positive, Is.EqualTo(comparer.GetNearestValue(Direction.Positive, Direction.Positive)));
      Assert.That(Direction.Negative, Is.EqualTo(comparer.GetNearestValue(Direction.None, Direction.Negative)));
      Assert.That(Direction.Positive, Is.EqualTo(comparer.GetNearestValue(Direction.None, Direction.Positive)));
      Assert.That(comparer.ValueRangeInfo.HasDeltaValue, Is.False);
    }

    [Test]
    public void CustomComparerTest()
    {
      var o1 = new Wrapper<int>(1);
      var o2 = new Wrapper<int>(2);
      var comparer = AdvancedComparer<Wrapper<int>>.Default;
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "WrapperComparer*");
      Assert.That(comparer.Compare(o2,o1), Is.GreaterThan(0));
    }

    [Test]
    public void CustomComparerTest2()
    {
      var o1 = new Wrapper2<int, int>(0,1);
      var o2 = new Wrapper2<int, int>(0,2);
      var comparer = AdvancedComparer<Wrapper2<int, int>>.Default;
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "Wrapper2Comparer*");
      Assert.That(comparer.Compare(o2,o1), Is.GreaterThan(0));
    }

    [Test]
    public void InheritedComparerTest()
    {
      var o1 = new Wrapper2a<int, int>(0,1);
      var o2 = new Wrapper2a<int, int>(0,2);
      var comparer = AdvancedComparer<Wrapper2a<int, int>>.Default;
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "BaseComparerWrapper*");
      Assert.That(0, Is.EqualTo(comparer.Compare(o2,o1)));
    }

    [Test]
    public void CastingComparerTest()
    {
      var o1 = new Wrapper1<int>(1);
      var o2 = new Wrapper1<int>(2);
      var comparer = AdvancedComparer<Wrapper1<int>>.Default.Cast<Wrapper<int>>();
      Assert.That(comparer, Is.Not.Null);
      AssertEx.IsPatternMatch(comparer.Implementation.GetType().Name, "CastingComparer*");
      Assert.That(comparer.Compare(o2,o1), Is.GreaterThan(0));
    }
  }
}