// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.23

using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Orm.Tests;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Performance
{
  public sealed class InvariantStringComparer: IComparer<string>
  {
    public int Compare(string x, string y)
    {
      return CultureInfo.InvariantCulture.CompareInfo.Compare(x, y, CompareOptions.None);
    }
  }

  [TestFixture]
  public class ComparerPerformanceTest
  {
    public const int TwoValueTestIterations = 1000000;
    public const int ArrayTestShortLength = 1000;
    public const int ArrayTestShortIterations = 100;
    public const int ArrayTestLongLength = 100000;
    public const int ArrayTestLongIterations = 1;

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      Test(0.5);
    }

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
//      StructTest<Direction>(0.5);
//      StructTest<double>(1);
      ClassTest<Xtensive.Tuples.Tuple>(0.2);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void RegularTest()
    {
      Test(0.001);
    }

    [Test]
    public void ShortTest()
    {
      StructTest<Direction>(0.5);
      StructTest<double>(1);
      ClassTest<Tuple>(0.2);
    }

    public void Test(double speedFactor)
    {
      StructTest<bool>(1 * speedFactor);
      StructTest<byte>(1 * speedFactor);
      StructTest<sbyte>(1 * speedFactor);
      StructTest<char>(1 * speedFactor);
      StructTest<short>(1 * speedFactor);
      StructTest<ushort>(1 * speedFactor);
      StructTest<int>(1 * speedFactor);
      StructTest<uint>(1 * speedFactor);
      StructTest<long>(1 * speedFactor);
      StructTest<ulong>(1 * speedFactor);
      StructTest<float>(1 * speedFactor);
      StructTest<double>(1 * speedFactor);
      StructTest<DateTime>(1 * speedFactor);
      StructTest<Guid>(0.5 * speedFactor);
      StructTest<Direction>(0.5 * speedFactor);
      ClassTest<string>(0.2 * speedFactor);
      ClassTest<Xtensive.Tuples.Tuple>(0.2 * speedFactor);
    }

    public void ClassTest<T>(double speedFactor)
      where T: class
    {
      InnerTest<T>(speedFactor);
    }

    public void StructTest<T>(double speedFactor)
      where T: struct
    {
      InnerTest<T>(speedFactor);
      InnerTest<T?>(speedFactor);
    }

    public void InnerTest<T>(double speedFactor)
    {
      TestLog.Info("Type {0}:", typeof(T).GetShortName());
      using (IndentManager.IncreaseIndent()) {
        TwoValuesTest<T>((int)(TwoValueTestIterations*speedFactor));
        ArrayTest<T>((int)(ArrayTestShortLength*speedFactor), ArrayTestShortIterations, 0);
        ArrayTest<T>((int)(ArrayTestShortLength*speedFactor), ArrayTestShortIterations, 0.5);
        ArrayTest<T>((int)(ArrayTestShortLength*speedFactor), ArrayTestShortIterations, 1);
        ArrayTest<T>((int)(ArrayTestLongLength*speedFactor),  ArrayTestLongIterations,  0);
        ArrayTest<T>((int)(ArrayTestLongLength*speedFactor),  ArrayTestLongIterations,  0.5);
        ArrayTest<T>((int)(ArrayTestLongLength*speedFactor),  ArrayTestLongIterations,  1);
      }
    }

    public void TwoValuesTest<T>(int count)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<T> g =  InstanceGeneratorProvider.Default.GetInstanceGenerator<T>();
      IEnumerator<T> sequence = InstanceGenerationUtils<T>.GetInstances(g, r, 1).GetEnumerator();
      sequence.MoveNext();
      T o1  = sequence.Current;
      sequence.MoveNext();
      T o1c = sequence.Current;
      T o2  = g.GetInstance(r);
      AdvancedComparerStruct<T> c1 = AdvancedComparer<T>.System;
      AdvancedComparerStruct<T> c2 = AdvancedComparer<T>.Default;
      if (!TestInfo.IsProfileTestRunning)
        SimpleComparisonLoop(c1, o1, o2, 1000);
      SimpleComparisonLoop(c2, o1, o2, 1000);
      TestLog.Info("Values comparison:");
      TestLog.Info("  Type: {0}, instances: {1} x 2, {2}", typeof(T).GetShortName(), o1, o2);
      using (IndentManager.IncreaseIndent()) {
        TestHelper.CollectGarbage();
        if (!TestInfo.IsProfileTestRunning) {
          using (new Measurement("Default  comparer (equal)    ", MeasurementOptions.Log, count))
            SimpleComparisonLoop(c1, o1, o1c, count);
          TestHelper.CollectGarbage();
          using (new Measurement("Default  comparer (different)", MeasurementOptions.Log, count))
            SimpleComparisonLoop(c1, o1, o2, count);
          TestHelper.CollectGarbage();
        }
        using (new Measurement("Xtensive comparer (equal)    ", MeasurementOptions.Log, count))
          SimpleComparisonLoop(c2, o1, o1c, count);
        using (new Measurement("Xtensive comparer (different)", MeasurementOptions.Log, count))
          SimpleComparisonLoop(c2, o1, o2, count);
        TestHelper.CollectGarbage();
      }
    }

    public void ArrayTest<T>(int length, int count, double equalityProbability)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<T> g =  InstanceGeneratorProvider.Default.GetInstanceGenerator<T>();
      IEnumerator<T> sequence = InstanceGenerationUtils<T>.GetInstances(g, r, equalityProbability).GetEnumerator();
      sequence.MoveNext();
      T[] array = new T[length];
      for (int i = 0; i<length; i++, sequence.MoveNext())
        array[i] = sequence.Current;
      AdvancedComparerStruct<T> c1 = AdvancedComparer<T>.System;
      AdvancedComparerStruct<T> c2 = AdvancedComparer<T>.Default;
      if (!TestInfo.IsProfileTestRunning)
        ArrayComparisonLoop(c1, array, 1);
      ArrayComparisonLoop(c2, array, 1);
      TestLog.Info("Array comparison (equality probability = {0}):", (int)(equalityProbability*100));
      TestLog.Info("  Type: {0}, array length: {1}", typeof(T).GetShortName(), length);
      using (IndentManager.IncreaseIndent()) {
        TestHelper.CollectGarbage();
        if (!TestInfo.IsProfileTestRunning) {
          using (new Measurement("Default  comparer", MeasurementOptions.Log, (length - 1) * count))
            ArrayComparisonLoop(c1, array, count);
          TestHelper.CollectGarbage();
        }
        using (new Measurement("Xtensive comparer", MeasurementOptions.Log, (length-1)*count))
          ArrayComparisonLoop(c2, array, count);
        TestHelper.CollectGarbage();
      }
    }

    private static void SimpleComparisonLoop<T>(AdvancedComparerStruct<T> c, T o1, T o2, int count)
    {
      for (int i = 0; i<count; i++)
        c.Equals(o1, o2);
    }

    private static void ArrayComparisonLoop<T>(AdvancedComparerStruct<T> c, T[] array, int count)
    {
      int length = array.Length;
      for (int j = 0; j<count; j++)
        for (int i = 0; i<length-1;)
          c.Equals(array[i], array[++i]);
    }
  }
}
