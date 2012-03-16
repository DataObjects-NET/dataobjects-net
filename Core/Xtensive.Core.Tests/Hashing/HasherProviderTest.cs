// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Diagnostics;
using Xtensive.Hashing;
using Xtensive.Testing;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Tests.Hashing
{
  [TestFixture]
  public class HasherProviderTest
  {
    private readonly Random random = RandomManager.CreateRandom();
    private int hashCode;
    private bool getHashCodeExecuted;
    private readonly int repeats = 1000;

    private enum TestUInt64Enum:ulong
    {
      Value1,
      Value2,
      Value3
    }

    private enum TestShortEnum : short
    {
      Value1,
      Value2,
      Value3,
      Value4
    }


    [Test]
    public void EnumTest()
    {
      Hasher<TestUInt64Enum> hasher = Hasher<TestUInt64Enum>.Default;
      Assert.IsNotNull(hasher);
      Assert.AreEqual(hasher.Implementation.GetType().Name, "EnumHasher`2");
      hasher.GetHash(TestUInt64Enum.Value1);
      hasher.GetHash(TestUInt64Enum.Value2);
      hasher.GetHash(TestUInt64Enum.Value3);
    }


    [Test]
    [Explicit]
    [Category("Debug")]
    public void DebugTest()
    {
      SingleHashPerformanceTest<TestUInt64Enum>(repeats);
    }

    [Test]
    public void MethodsEquivalenceTest()
    {
      MethodsEquivalenceTest<byte>();
      MethodsEquivalenceTest<sbyte>();
      MethodsEquivalenceTest<int>();
      MethodsEquivalenceTest<uint>();
      MethodsEquivalenceTest<short>();
      MethodsEquivalenceTest<ushort>();
      MethodsEquivalenceTest<long>();
      MethodsEquivalenceTest<ulong>();
      MethodsEquivalenceTest<float>();
      MethodsEquivalenceTest<double>();
      MethodsEquivalenceTest<bool>();
      MethodsEquivalenceTest<decimal>();
      MethodsEquivalenceTest<string>();
      MethodsEquivalenceTest<Guid>();
      MethodsEquivalenceTest<DateTime>();
      MethodsEquivalenceTest<Xtensive.Tuples.Tuple>();
      MethodsEquivalenceTest<Pair<string, double>>();
      MethodsEquivalenceTest<Triplet<float, int?, string>>();
      MethodsEquivalenceTest<KeyValuePair<int, string>>();
      MethodsEquivalenceTest<int?>();
      MethodsEquivalenceTest<TestUInt64Enum>();
      MethodsEquivalenceTest<TestShortEnum>();
      MethodsEquivalenceTest<int[]>();
//      MethodsEquivalenceTest<Entire<ulong>>();
    }

    [Test]
    public void SpecialCasesTest()
    {
      SpecialCasesTest<Triplet<float, int?, string>>();
      SpecialCasesTest<KeyValuePair<int, string>>();
      SpecialCasesTest<Xtensive.Tuples.Tuple>();
      SpecialCasesTest<Triplet<Xtensive.Tuples.Tuple, Xtensive.Tuples.Tuple, string>>();
      SpecialCasesTest<KeyValuePair<int, Xtensive.Tuples.Tuple>>();
      SpecialCasesTest<int?>();
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceMultiHash()
    {
      int repeats = 1000000;
      int count = 10;
      MultiHashPerformanceTest<byte>(repeats, count);
      MultiHashPerformanceTest<sbyte>(repeats, count);
      MultiHashPerformanceTest<int>(repeats, count);
      MultiHashPerformanceTest<uint>(repeats, count);
      MultiHashPerformanceTest<short>(repeats, count);
      MultiHashPerformanceTest<ushort>(repeats, count);
      MultiHashPerformanceTest<long>(repeats, count);
      MultiHashPerformanceTest<ulong>(repeats, count);
      MultiHashPerformanceTest<float>(repeats, count);
      MultiHashPerformanceTest<double>(repeats, count);
      MultiHashPerformanceTest<bool>(repeats, count);
      MultiHashPerformanceTest<decimal>(repeats, count);
      MultiHashPerformanceTest<string>(repeats, count);
      MultiHashPerformanceTest<Guid>(repeats, count);
      MultiHashPerformanceTest<DateTime>(repeats, count);
      MultiHashPerformanceTest<Xtensive.Tuples.Tuple>(repeats, count);
      MultiHashPerformanceTest<Pair<int, string>>(repeats, count);
      MultiHashPerformanceTest<Guid?>(repeats, count);
      MultiHashPerformanceTest<int?>(repeats, count);      
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceSingleHash()
    {
      int repeats = 1000000;
      SingleHashPerformanceTest<byte>(repeats);
      SingleHashPerformanceTest<sbyte>(repeats);
      SingleHashPerformanceTest<int>(repeats);
      SingleHashPerformanceTest<uint>(repeats);
      SingleHashPerformanceTest<short>(repeats);
      SingleHashPerformanceTest<ushort>(repeats);
      SingleHashPerformanceTest<long>(repeats);
      SingleHashPerformanceTest<ulong>(repeats);
      SingleHashPerformanceTest<float>(repeats);
      SingleHashPerformanceTest<double>(repeats);
      SingleHashPerformanceTest<bool>(repeats);
      SingleHashPerformanceTest<decimal>(repeats);
      SingleHashPerformanceTest<string>(repeats);
      SingleHashPerformanceTest<Guid>(repeats);
      SingleHashPerformanceTest<DateTime>(repeats);
      SingleHashPerformanceTest<Xtensive.Tuples.Tuple>(repeats);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceDebug()
    {
      int repeats = 100000;
      SingleHashPerformanceTest<Guid>(repeats);
    }

    private void SingleHashPerformanceTest<T>(int repeats)
    {
      T[] instances = new List<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, repeats)).ToArray();
      Hasher<T> hasher = Hasher<T>.Default;
      using (new Measurement(string.Format("{0} GetHashes[1]", typeof (T).Name), repeats)) {
        for (int i = 0; i < repeats; i++) {
          hasher.GetHashes(instances[i], 1);
        }
      }
      using (new Measurement(string.Format("{0} GetHash()", typeof (T).Name), repeats)) {
        for (int i = 0; i < repeats; i++) {
          hasher.GetHash(instances[i]);
        }
      }
      using (new Measurement(string.Format("{0} standart", typeof (T).Name), repeats)) {
        for (int i = 0; i < repeats; i++) {
          instances[i].GetHashCode();
        }
      }
    }

    private void MultiHashPerformanceTest<T>(int repeats, int count)
    {
      Log.Info("-----------------------------------");
      T[] instances = new List<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, repeats)).ToArray();
      Hasher<T> hasher = Hasher<T>.Default;
      using (new Measurement(string.Format("{0} Xtensive hasher", typeof (T).Name), repeats)) {
        for (int i = 0; i < repeats; i++) {
          hasher.GetHashes(instances[i], count);
        }
      }
      using (new Measurement(string.Format("{0} standard hasher", typeof(T).Name), repeats))
      {
        for (int i = 0; i < repeats; i++) {
          for (int y = 0; y < count; y++)
            instances[i].GetHashCode();
        }
      }
    }

    private void SpecialCasesTest<T>()
    {
      Hasher<T> hasher = Hasher<T>.Default;
      List<T> instances = new List<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, repeats));
      foreach (T instance in instances) {
        long singleHash = HasherProvider.Default.GetHasherByInstance(instance).GetInstanceHash(instance);
        long singleHash1 = hasher.GetHash(instance);
        Assert.AreEqual(singleHash, singleHash1);
      }
    }

    /// <summary>
    /// Whether mthods GetHashe(), GetHashs() and GetDecadeOfHashes() give same results.
    /// </summary>
    /// <typeparam name="T">Type of hashing object.</typeparam>
    private void MethodsEquivalenceTest<T>()
    {
      Hasher<T> hasher = Hasher<T>.Default;
      List<T> instances = new List<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, repeats));
      int hashesCount = 10;
      foreach (T instance in instances) {
        long[] arrayOfHashes = hasher.GetHashes(instance, hashesCount);
        long singleHash = hasher.GetHash(instance);
        Assert.AreEqual(hashesCount, arrayOfHashes.Length);
        Assert.AreEqual(singleHash, arrayOfHashes[0]);
      }
    }

    [Test]
    public void IEnumerableTest()
    {
      List<int> data = new List<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
      int count = 10;
      Hasher<List<int>> hasher = Hasher<List<int>>.Default;
//      Assert.AreEqual("EnumerableInterfaceHasher`2", hasher.GetType().Name);
      long[] hashes = hasher.GetHashes(data, count);
      Assert.AreEqual(hashes.Length, count);
    }

    [Test]
    public void IEnumerableNullTest()
    {
      List<int> data = new List<int>(new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9});
      int count = 10;
      Hasher<List<int>> hasher = Hasher<List<int>>.Default;
//      Assert.AreEqual("EnumerableInterfaceHasher`2", hasher.GetType().Name);
      long[] hashes = hasher.GetHashes(null, count);
      Assert.AreEqual(hashes.Length, count);
    }

    [Test]
    public void DefaultObjectProviderTest()
    {
      Hasher<HasherProviderTest> hasher = Hasher<HasherProviderTest>.Default;
//      Assert.AreEqual("BaseHasherWrapper`2", hasher.GetType().Name);
      for (int i = 0; i < 100; i++) {
        getHashCodeExecuted = false;
        hashCode = RandomManager.CreateRandom().Next();
        hasher.GetHash(this);
        Assert.IsTrue(getHashCodeExecuted);
      }
    }

    [Test]
    public void DefaultStructureProviderTest()
    {
      Hasher<TestStruct> hasher = Hasher<TestStruct>.Default;
//      Assert.AreEqual("ValueTypeHasher`1", hasher.GetType().Name);
      TestStruct testStruct = new TestStruct();
      for (int i = 0; i < 100; i++) {
        testStruct.Hash = RandomManager.CreateRandom().Next();
        testStruct.GetHashCodeExecuted = false;
        hasher.GetHash(testStruct);
        Assert.IsTrue(testStruct.GetHashCodeExecuted);
      }
    }

    [Test]
    public void CollisionTest()
    {
      CheckCollisions<short>(1000, 32);
    }

    private void CheckCollisions<T>(int repeats, int count)
    {
      Hasher<T> hasher = Hasher<T>.Default;
      Dictionary<long, int> hashesDictionary = new Dictionary<long, int>();
      foreach (T instance in InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, repeats))
      {
        long[] hashes = hasher.GetHashes(instance, count);
        for(int i=0;i<count;i++) {
          int currentCount = 0;
          hashesDictionary.TryGetValue(hashes[i], out currentCount);
          currentCount++;
          hashesDictionary[hashes[i]] = currentCount;
        }
      }
      Dictionary<int,int> collisions = new Dictionary<int, int>();
      foreach (KeyValuePair<long, int> pair in hashesDictionary) {
        int currentCount = 0;
        collisions.TryGetValue(pair.Value, out currentCount);
        currentCount++;
        collisions[pair.Value] = currentCount;
      }
      foreach (KeyValuePair<int, int> collision in collisions) {
        Log.Info("Collision rank/count {0}/{1}", collision.Key, collision.Value);
      }
    }

    public override int GetHashCode()
    {
      getHashCodeExecuted = true;
      return hashCode;
    }
  }

  internal struct TestStruct
  {
    private static bool getHashCodeExecuted;
    private int hash;

    public bool GetHashCodeExecuted
    {
      get { return getHashCodeExecuted; }
      set { getHashCodeExecuted = value; }
    }

    public int Hash
    {
      set { hash = value; }
      get { return hash; }
    }

    public override int GetHashCode()
    {
      GetHashCodeExecuted = true;
      return hash;
    }
  }
}