// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Arithmetic;
using Xtensive.Diagnostics;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Arithmetics
{
  [TestFixture]
  public class AritmeticsTest
  {
    private readonly Random random = RandomManager.CreateRandom();

    [Test]
    public void Test()
    {
      const int count = 100;
      Assert.IsNull(Arithmetic<string>.Default);
      Arithmetic<long>   longArithmetic   = Arithmetic<long>.Default;
      Arithmetic<double> doubleArithmetic = Arithmetic<double>.Default;
      Arithmetic<int>    intArithmetic    = Arithmetic<int>.Default;
      Assert.IsNotNull(longArithmetic);
      Assert.IsNotNull(doubleArithmetic);
      Assert.IsNotNull(intArithmetic);
      Assert.AreEqual(longArithmetic.Implementation.GetType().Name,   "Int64Arithmetic");
      Assert.AreEqual(intArithmetic.Implementation.GetType().Name, "Int32Arithmetic");
      Assert.AreEqual(doubleArithmetic.Implementation.GetType().Name, "DoubleArithmetic");
      checked {
        IEnumerable<long> longValues = InstanceGeneratorProvider.Default.GetInstanceGenerator<long>().GetInstances(random, count);
        foreach (long longValue in longValues) {
          AssertEx.ResultsAreEqual<long>(() => (long) (longValue/12.34d), () => longArithmetic.Divide(longValue, 12.34d));
          AssertEx.ResultsAreEqual<long>(() => (long) (longValue*12.34d), () => longArithmetic.Multiply(longValue, 12.34d));
          AssertEx.ResultsAreEqual<long>(() => longValue + longValue, () => longArithmetic.Add(longValue, longValue));
          AssertEx.ResultsAreEqual<long>(() => longValue - longValue, () => longArithmetic.Subtract(longValue, longValue));
          AssertEx.ResultsAreEqual<long>(() => -longValue, () => longArithmetic.Negation(longValue));
        }
        IEnumerable<int> intValues = InstanceGeneratorProvider.Default.GetInstanceGenerator<int>().GetInstances(random, count);
        foreach (int intValue in intValues) {
          AssertEx.ResultsAreEqual<int>(() => (int) (intValue/12.34d), () => intArithmetic.Divide(intValue, 12.34d));
          AssertEx.ResultsAreEqual<int>(() => (int) (intValue*12.34d), () => intArithmetic.Multiply(intValue, 12.34d));
          AssertEx.ResultsAreEqual<int>(() => intValue + intValue, () => intArithmetic.Add(intValue, intValue));
          AssertEx.ResultsAreEqual<int>(() => intValue - intValue, () => intArithmetic.Subtract(intValue, intValue));
          AssertEx.ResultsAreEqual<int>(() => -intValue, () => intArithmetic.Negation(intValue));
        }
        IEnumerable<double> doubleValues = InstanceGeneratorProvider.Default.GetInstanceGenerator<double>().GetInstances(random, count);
        foreach (double doubleValue in doubleValues) {
          AssertEx.ResultsAreEqual<double>(() => doubleValue/12.34d, () => doubleArithmetic.Divide(doubleValue, 12.34d));
          AssertEx.ResultsAreEqual<double>(() => doubleValue*12.34d, () => doubleArithmetic.Multiply(doubleValue, 12.34d));
          AssertEx.ResultsAreEqual<double>(() => doubleValue + doubleValue, () => doubleArithmetic.Add(doubleValue, doubleValue));
          AssertEx.ResultsAreEqual<double>(() => doubleValue - doubleValue, () => doubleArithmetic.Subtract(doubleValue, doubleValue));
          AssertEx.ResultsAreEqual<double>(() => -doubleValue, () => doubleArithmetic.Negation(doubleValue));
        }
      }
    }


    [Test]
    [Explicit]
    public void PerfTest()
    {
      int count = 500*1000*1000;
      PerfTestInternal<int>(count);
      PerfTestInternal<uint>(count);
    }

    [Test]
    public void RulesTest()
    {
      Arithmetic<int> defaultArithmetic = Arithmetic<int>.Default;
      Arithmetic<int> arithmetic1 =
        defaultArithmetic.ApplyRules(new ArithmeticRules(NullBehavior.ThreatNullAsNull, OverflowBehavior.AllowOverflow));
      Arithmetic<int> arithmetic2 =
        arithmetic1.ApplyRules(new ArithmeticRules(NullBehavior.ThreatNullAsZero, OverflowBehavior.DenyOverflow));
      Arithmetic<int> arithmetic3 =
        defaultArithmetic.ApplyRules(new ArithmeticRules(NullBehavior.ThreatNullAsNull, OverflowBehavior.AllowOverflow));
      Assert.AreEqual(arithmetic1, arithmetic3);
      Assert.AreNotEqual(arithmetic1, arithmetic2);
    }

    private void PerfTestInternal<T>(int count)
    {
      // Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<T> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>();
      T instance1 = generator.GetInstance(random);
      T instance2 = generator.GetInstance(random);
      Arithmetic<T> arithmetic = Arithmetic<T>.Default;
      using(new Measurement(typeof(T).Name, count)) {
        for(int i=0;i<count;i++)
          arithmetic.Add(instance1, instance2);
      }
    }

  }
}