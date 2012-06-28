// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.03.03

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Testing;

namespace Xtensive.Indexing.Tests.Index
{
  [TestFixture]
  public class SerializationTest
  {
    private const int maxCount = 10;
    private Random random = RandomManager.CreateRandom();
    private string fileName = String.Format("{0}\\index.bin", TestHelper.TestFolderName);
    private string fileLoction = String.Format("file://localhost/{0}/index.bin", TestHelper.TestFolderName);

    [Test]
    public void ConfigurationSerializeTest()
    {
      ConfigurationSerializeInternal<int>();
      ConfigurationSerializeInternal<string>();
      ConfigurationSerializeInternal<long>();
      ConfigurationSerializeInternal<Guid>();
      ConfigurationSerializeInternal<byte>();
    }

    private void ConfigurationSerializeInternal<T>()
    {
//      IValueSerializer<Stream> serializer = ValueSerializationScope.CurrentSerializer;
//      using (MemoryStream stream = new MemoryStream()) {
//        serializer.Serialize(stream, GetConfiguration<T>());
//        stream.Seek(0, SeekOrigin.Begin);
//        IndexConfiguration<T, T> serialized = (IndexConfiguration<T, T>) serializer.Deserialize(stream);
//        Assert.IsNotNull(serialized);
//      }
    }

    [Test]
    [Explicit]
    public void SerializeTest()
    {
      //StreamPageProvider
      SerializeInternal<short>(GetCount());
      SerializeInternal<int>(GetCount());
      SerializeInternal<ulong>(GetCount());
      SerializeInternal<Guid>(GetCount());
      SerializeInternal<string>(GetCount());
      SerializeInternal<byte>(GetCount());
      //MemoryPageProvider
      MemorySerializeInternal<short>(GetCount());
      MemorySerializeInternal<int>(GetCount());
      MemorySerializeInternal<ulong>(GetCount());
      MemorySerializeInternal<Guid>(GetCount());
      MemorySerializeInternal<string>(GetCount());
      MemorySerializeInternal<byte>(GetCount());
    }

    private void SerializeInternal<T>(int count)
    {
      if (File.Exists(fileName)) {
        File.Delete(fileName);
      }
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);

      foreach (T item in instances) {
        index.Add(item);
      }

      IndexConfiguration<T, T> configuration = GetConfiguration<T>();
      configuration.Location = fileLoction;

      try {
        using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
          using (new Measurement("Serializing index."))
            serializedIndex.Serialize(index);
          IEnumerator<T> serializedEnumerator = serializedIndex.GetEnumerator();
          IEnumerator<T> indexEnumerator = index.GetEnumerator();
          while (indexEnumerator.MoveNext()) {
            Assert.IsTrue(serializedEnumerator.MoveNext());
            Assert.AreEqual(indexEnumerator.Current, serializedEnumerator.Current);
          }
          Assert.AreEqual(index.Count, serializedIndex.Count);
        }
      }
      finally {
        File.Delete(fileName);
      }

      try {
        using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
          serializedIndex.Serialize(new T[] {});
          IEnumerator<T> serializedEnum = serializedIndex.GetEnumerator();
          Assert.IsFalse(serializedEnum.MoveNext());
        }
      }
      finally {
        File.Delete(fileName);
      }
    }

    private void MemorySerializeInternal<T>(int count)
    {
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);

      foreach (T item in instances) {
        index.Add(item);
      }

      IndexConfiguration<T, T> configuration = GetConfiguration<T>();
      using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
        using (new Measurement("Serializing index."))
          serializedIndex.Serialize(index);
        IEnumerator<T> serializedEnumerator = serializedIndex.GetEnumerator();
        IEnumerator<T> indexEnumerator = index.GetEnumerator();
        while (indexEnumerator.MoveNext()) {
          Assert.IsTrue(serializedEnumerator.MoveNext());
          Assert.AreEqual(indexEnumerator.Current, serializedEnumerator.Current);
        }
        Assert.AreEqual(index.Count, serializedIndex.Count);
      }

      using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
        serializedIndex.Serialize(new T[] {});
        IEnumerator<T> serializedEnum = serializedIndex.GetEnumerator();
        Assert.IsFalse(serializedEnum.MoveNext());
      }
    }


    [Test]
    [Explicit]
    public void DeserializeTest()
    {
      DeserializeInternal<short>(GetCount());
      DeserializeInternal<byte>(GetCount());
      DeserializeInternal<long>(GetCount());
      DeserializeInternal<string>(GetCount());
      DeserializeInternal<Guid>(GetCount());
    }

    private void DeserializeInternal<T>(int count)
    {
      if (File.Exists(fileName)) {
        File.Delete(fileName);
      }
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);

      Xtensive.Collections.ISet<T> missingInstances = GetUniqueInstances<T>(count);
      missingInstances.ExceptWith(instances);

      foreach (T item in instances) {
        index.Add(item);
      }

      IndexConfiguration<T, T> configuration = GetConfiguration<T>();
      configuration.Location = fileLoction;

      try {
        using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
          serializedIndex.Serialize(index);
          serializedIndex.CheckIntegrity();
          index.CheckIntegrity();
          foreach (T instance in instances) {
            if (!serializedIndex.Contains(instance)) {
              Log.Error("Index serializing error: {0} does not exist in the index.", instance);
            }
            Assert.IsTrue(serializedIndex.Contains(instance));
          }
        }
        using (new Measurement("Enumerating serialized index."))
        using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
          // Assert.IsNotNull(serializedIndex.DescriptorPage.Provider);
          Assert.IsNotNull(serializedIndex.KeyComparer);
          Assert.IsNotNull(serializedIndex.KeyExtractor);
          IEnumerator<T> serializedEnum = serializedIndex.GetEnumerator();
          IEnumerator<T> indexEnum = index.GetEnumerator();
          while (indexEnum.MoveNext()) {
            Assert.IsTrue(serializedEnum.MoveNext());
            Assert.AreEqual(indexEnum.Current, serializedEnum.Current);
          }
          foreach (T instance in instances) {
            if (!index.Contains(instance)) {
              Log.Error("Original error: {0} does not exist in the index.", instance);
            }
            Assert.IsTrue(index.Contains(instance));
          }
          foreach (T instance in instances) {
            if (!serializedIndex.Contains(instance)) {
              Log.Error("Error: {0}.", instance);
              if (index.ContainsKey(instance)) {
                Log.Error("But original index contains this key.");
              }
            }
            Assert.IsTrue(serializedIndex.Contains(instance));
          }
          foreach (T instance in missingInstances) {
            Assert.IsFalse(serializedIndex.Contains(instance));
          }
        }
      }
      finally {
        if (File.Exists(fileName)) {
          File.Delete(fileName);
        }
      }
    }

    [Test]
    [Explicit]
    public void DebugTest()
    {
      DebugInternal<short>(GetCount());
      DebugInternal<int>(GetCount());
      DebugInternal<ulong>(GetCount());
      DebugInternal<byte>(GetCount());
      DebugInternal<string>(GetCount());
      DebugInternal<Guid>(GetCount());
    }

    private void DebugInternal<T>(int count)
    {
      Index<T, T> index = GetIndex<T>();
      Xtensive.Collections.ISet<T> instances = GetUniqueInstances<T>(count);

      foreach (T item in instances) {
        index.Add(item);
      }

      IndexConfiguration<T, T> configuration = GetConfiguration<T>();
      configuration.Location = fileLoction;
      if (File.Exists(fileName)) {
        File.Delete(fileName);
      }

      try {
        using (Index<T, T> serializedIndex = new Index<T, T>(configuration)) {
          serializedIndex.Serialize(index);
          serializedIndex.CheckIntegrity();
        }
      }
      finally {
        if (File.Exists(fileName)) {
          File.Delete(fileName);
        }
      }
    }


    private SetSlim<T> GetUniqueInstances<T>(int count)
    {
      return new SetSlim<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, count));
    }

    private IndexConfiguration<T, T> GetConfiguration<T>()
    {
      return new IndexConfiguration<T, T>(AdvancedConverter<T, T>.Default.Implementation.Convert, AdvancedComparer<T>.Default)
        {
          PageSize = 4
        };
    }

    private int GetCount()
    {
      return random.Next() % maxCount + 1;
    }

    private Index<T, T> GetIndex<T>()
    {
      return new Index<T, T>(GetConfiguration<T>());
    }
  }
}