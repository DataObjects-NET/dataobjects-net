// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Diagnostics;
using Xtensive.Orm.Tests;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.Testing
{
  // Verifies that 2 generated sequences of given type have appropriate share of coincident elements.
  // Checks all system types.
  [TestFixture]
  public class InstanceGeneratorTest
  {
    private const int BaseSequenceLength = 1000000;

    [Test]
    public void RegularTest()
    {
      TestSequence(BaseSequenceLength / 10);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      TestSequence(BaseSequenceLength);
    }

    public void TestSequence(int sequenceLength)
    {
      double baseTolerance = 0.05/(sequenceLength/10000d);
      TestSequence<bool> (sequenceLength, OP(1d/2), baseTolerance*10);
      TestSequence<bool?>(sequenceLength, NP(1d/2), baseTolerance*10);
      TestSequence<byte> (sequenceLength, OP(1d/256), baseTolerance);
      TestSequence<byte?>(sequenceLength, NP(1d/256), baseTolerance);
      TestSequence<char> (sequenceLength, OP(1d/65536), baseTolerance);
      TestSequence<char?>(sequenceLength, NP(1d/65536), baseTolerance);
      TestSequence<DateTime> (sequenceLength, 0, baseTolerance);
      TestSequence<DateTime?>(sequenceLength, 0, baseTolerance);
      TestSequence<double> (sequenceLength, 0, baseTolerance);
      TestSequence<double?>(sequenceLength, 0, baseTolerance);
      TestSequence<Guid> (sequenceLength, 0, baseTolerance);
      TestSequence<Guid?>(sequenceLength, 0, baseTolerance);
      TestSequence<short> (sequenceLength, OP(1d/65536), baseTolerance);
      TestSequence<short?>(sequenceLength, NP(1d/65536), baseTolerance);
      TestSequence<int> (sequenceLength, 0, baseTolerance);
      TestSequence<int?>(sequenceLength, 0, baseTolerance);
      TestSequence<long>(sequenceLength, 0, baseTolerance);
      TestSequence<long?>(sequenceLength, 0, baseTolerance);
      TestSequence<sbyte> (sequenceLength, OP(1d/256), baseTolerance);
      TestSequence<sbyte?>(sequenceLength, NP(1d/256), baseTolerance);
      TestSequence<float> (sequenceLength, 0, baseTolerance);
      TestSequence<float?>(sequenceLength, 0, baseTolerance);
      TestSequence<string>(sequenceLength, 0, baseTolerance);
      TestSequence<ushort> (sequenceLength, OP(1d/65536), baseTolerance);
      TestSequence<ushort?>(sequenceLength, NP(1d/65536), baseTolerance);
      TestSequence<uint>(sequenceLength, 0, baseTolerance);
      TestSequence<uint?>(sequenceLength, 0, baseTolerance);
      TestSequence<ulong>(sequenceLength, 0, baseTolerance);
      TestSequence<ulong?>(sequenceLength, 0, baseTolerance);
      TestSequence<Xtensive.Tuples.Tuple>(sequenceLength/10, 0, baseTolerance*10);
    }

    // Calculates probability for original type (actually does nothing)
    public static double OP(double p)
    {
      return p;
    }

    // Calculates probability for nullable type
    public static double NP(double p)
    {
      return p;
//      double np = 0.01;
//      return np*np + (1-np)*(1-np)*p;
    }

    public void TestSequence<T>(int size, double expectedShare, double shareTolerance)
    {
      TestLog.Info("{0} random sequence, {1} items:", typeof(T).GetShortName(), size);
      using (new LogIndentScope()) {
        TestLog.Info("Expected probability: {0}, tolerance: {1}.", expectedShare, shareTolerance);
        // Testing the same sequence
        Random r1 = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        Random r2 = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        double equalityProbability = GetEqualityProbability<T>(r1, r2, size);
        Assert.AreEqual(1, equalityProbability);
        // Testing different sequences
        r1 = RandomManager.CreateRandom(1, SeedVariatorType.CallingMethod);
        r2 = RandomManager.CreateRandom(2, SeedVariatorType.CallingMethod);
        using (new Measurement("Generation and comparison", size*2))
          equalityProbability = GetEqualityProbability<T>(r1, r2, size);
        TestLog.Info("Actual probability:   {0}.", equalityProbability);
        Assert.AreEqual(expectedShare, equalityProbability, shareTolerance);
      }
    }

    public double GetEqualityProbability<T>(Random r1, Random r2, int size)
    {
      IEnumerator<T> enumerator1 = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(r1, size).GetEnumerator();
      IEnumerator<T> enumerator2 = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(r2, size).GetEnumerator();
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      int equalCount = 0;
      int totalCount = 0;
      while (true)
      {
        bool isMoved1 = enumerator1.MoveNext();
        bool isMoved2 = enumerator2.MoveNext();
        if (isMoved1 != isMoved2)
          return 0;
        if (!isMoved1)
          break;
        totalCount++;
        if (comparer.Compare(enumerator1.Current, enumerator2.Current) == 0)
          equalCount++;
      }
      return (totalCount==0)?1:(double)equalCount/(double)totalCount;
    }
    
  }
}