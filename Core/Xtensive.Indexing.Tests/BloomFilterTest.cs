// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.10

using System;
using System.IO;
using NUnit.Framework;
using Xtensive.Indexing.BloomFilter;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class BloomFilterTest
  {
//    private Func<int, int> binaryMapping = delegate(int value) { return value%2; };
//    private Func<int, int> directMapping = delegate(int value) { return value; };
//    private Func<int, int> constantMapping = delegate(int value) { return 3; };

    [Test]
    public void AddClearSerializeDeserialize()
    {
      IBloomFilter<int>[] filters =
        new IBloomFilter<int>[] {new StreamBloomFilter<int>(new MemoryStream(), 1, 2), new MemoryBloomFilter<int>(1, 2)};
      foreach (IBloomFilter<int> filter in filters) {
        Assert.AreEqual(0, filter.FillFactor);
        Assert.IsFalse(filter.HasValue(123));
        filter.AddValue(123);
        Assert.IsTrue(filter.HasValue(123));
        Assert.AreEqual(1, filter.FillFactor);

        // Serialization
        MemoryStream serializedStream = new MemoryStream();
        filter.Serialize(serializedStream);
        serializedStream.Seek(0, SeekOrigin.Begin);
        IBloomFilter<int> streamFilter = new StreamBloomFilter<int>(serializedStream);
        Assert.AreEqual(streamFilter.FillFactor, filter.FillFactor);
        Assert.AreEqual(streamFilter.HasValue(123), filter.HasValue(123));
        Assert.AreEqual(streamFilter.HasValue(1230), filter.HasValue(1230));

        filter.Clear();
        Assert.AreEqual(0, filter.FillFactor);
      }
    }

    [Test]
    public void MemorySerialization()
    {
      Random random = new Random();
      int[] value = new int[10];
      for (int i = 0; i < 10; i++) {
        IBloomFilter<int> sourceFilter = new MemoryBloomFilter<int>(100, 2);
        for (int j = 0; j < value.Length; j++) {
          value[j] = random.Next();
          sourceFilter.AddValue(value[j]);
        }
        for (int j = 0; j < value.Length; j++) {
          Assert.IsTrue(sourceFilter.HasValue(value[j]));
        }
        MemoryStream serializedStream = new MemoryStream();
        sourceFilter.Serialize(serializedStream);
        serializedStream.Seek(0, SeekOrigin.Begin);
        IBloomFilter<int> destinationFilter = new MemoryBloomFilter<int>(serializedStream);
        for (int j = 0; j < value.Length; j++) {
          Assert.IsTrue(destinationFilter.HasValue(value[j]));
        }
      }
    }

    [Test]
    public void StreamSerialization()
    {
      Random random = new Random();
      int[] values = new int[10];
      for (int i = 0; i < 10; i++) {
        IBloomFilter<int> sourceFilter = new MemoryBloomFilter<int>(100, 2);
        for (int j = 0; j < values.Length; j++) {
          values[j] = random.Next();
          sourceFilter.AddValue(values[j]);
        }
        for (int j = 0; j < values.Length; j++) {
          Assert.IsTrue(sourceFilter.HasValue(values[j]));
        }
        MemoryStream serializedStream = new MemoryStream();
        sourceFilter.Serialize(serializedStream);
        serializedStream.Seek(0, SeekOrigin.Begin);
        IBloomFilter<int> destinationFilter = new StreamBloomFilter<int>(serializedStream);
        for (int j = 0; j < values.Length; j++) {
          Assert.IsTrue(destinationFilter.HasValue(values[j]));
        }
      }
    }

    [Test]
    public void MultipleHashFunctionsTest()
    {
      IBloomFilter<int>[] filters =
        new IBloomFilter<int>[]
          {
            new StreamBloomFilter<int>(new MemoryStream(), 100, 4),
            new MemoryBloomFilter<int>(100, 4)
          };
      foreach (IBloomFilter<int> filter in filters) {
        Assert.AreEqual(0, filter.FillFactor);
        for (int i = 10; i < 100; i += 10) {
          if (filter.HasValue(i))
            Log.Info("{0}", i);
          Assert.IsFalse(filter.HasValue(i));
          filter.AddValue(i);
          Assert.IsTrue(filter.HasValue(i));
        }
      }
    }

    [Test]
    public void GuidTest()
    {
      int length = 1024*8;
      int count = 1024;
      IBloomFilter<Guid> filter = new MemoryBloomFilter<Guid>(length, BloomFilter<int>.GetOptimalHashCount(length/count));
      for (int i = 0; i < count; i++) {
        Guid value = Guid.NewGuid();
        filter.AddValue(value);
        Assert.IsTrue(filter.HasValue(value));
      }
      Log.Info("Length/Count/FillFactor {0} / {1} / {2}", length, count, filter.FillFactor);
      double epsilon = 0.1;
      Assert.IsTrue(Math.Abs(filter.FillFactor - 0.5f)<epsilon);
    }
  }
}