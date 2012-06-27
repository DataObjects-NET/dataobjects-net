// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Xtensive.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Indexing.Serialization;
using Xtensive.Indexing.Serialization.Binary;
using Xtensive.Testing;

namespace Xtensive.Indexing.Tests.Serialization
{
  [TestFixture]
  public class BinaryValueSerializerTest
  {
    private const int BaseCount = 10000;
    private static readonly Random random = RandomManager.CreateRandom(SeedVariatorType.CallingType);
    private string typeName;

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      int count = BaseCount;
      TestInternal<string>(count);
    }

    [Test]
    public void RegularTest()
    {
      int count = BaseCount / 10;
      TestInternal<bool>(count);
      TestInternal<byte>(count);
      TestInternal<char>(count);
      TestInternal<decimal>(count);
      TestInternal<double>(count);
      TestInternal<Guid>(count);
      TestInternal<short>(count);
      TestInternal<int>(count);
      TestInternal<long>(count);
      TestInternal<sbyte>(count);
      TestInternal<float>(count);
      TestInternal<string>(count);
      TestInternal<ushort>(count);
      TestInternal<uint>(count);
      TestInternal<ulong>(count);
    }

    private void TestInternal<T>(int count)
    {
      typeName = typeof(T).GetShortName();
      var streamXtensive = new MemoryStream();
      var streamSystem = new MemoryStream();
      var xtensiveValueSerializer = ValueSerializer<T>.Default;
      var binarySerializer = LegacyBinarySerializer.Instance;
      Assert.IsNotNull(xtensiveValueSerializer);
      T[] instances = new List<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, count)).ToArray();

      typeName = typeof(T).GetShortName();
      Log.Info("================= {0} serialization test =======================", typeName, count);

      // Serialize
      using (new Measurement(string.Format("{0} Xtensive serialize. ", typeName), count))
      {
        for (int i = 0; i < count; i++)
        {
          xtensiveValueSerializer.Serialize(streamXtensive, instances[i]);
        }
      }
      using (new Measurement(string.Format("{0} System serialize.   ", typeName), count))
      {
        for (int i = 0; i < count; i++)
        {
          binarySerializer.Serialize(streamSystem, instances[i]);
        }
      }

      Log.Info("{0} serialized stream sizes. {1} values.", typeName, count);
      Log.Info("Xtensive stream: {0} bytes", streamXtensive.Length);
      Log.Info("System stream:   {0} bytes", streamSystem.Length);

      // Deserialize
      T[] deserializedInstances = new T[instances.Length];
      streamSystem.Seek(0, SeekOrigin.Begin);
      streamXtensive.Seek(0, SeekOrigin.Begin);
      using (new Measurement(string.Format("{0} Xtensive deserialize. ", typeName), count))
      {
        for (int i = 0; i < count; i++)
        {
          deserializedInstances[i] = xtensiveValueSerializer.Deserialize(streamXtensive);
        }
      }
      for (int i = 0; i < instances.Length; i++)
      {
        Assert.AreEqual(instances[i], deserializedInstances[i]);
      }
      Array.Clear(deserializedInstances, 0, deserializedInstances.Length);
      using (new Measurement(string.Format("{0} System deserialize.   ", typeName), count))
      {
        for (int i = 0; i < count; i++)
        {
          deserializedInstances[i] = (T)binarySerializer.Deserialize(streamSystem);
        }
      }
      for (int i = 0; i < instances.Length; i++)
      {
        Assert.AreEqual(instances[i], deserializedInstances[i]);
      }

      Log.Info("================= {0} serialization test finished =======================\n", typeName);
    }


    [Test]
    public void LongSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(long));
      var valueSerializer = ValueSerializer<long>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "Int64ValueSerializer");
      for (long i = -1000; i < 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        long restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (long i = long.MinValue; i < long.MinValue + 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        long restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (long i = long.MaxValue - 1000; i < long.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        long restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void ULongSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(ulong));
      var valueSerializer = ValueSerializer<ulong>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "UInt64ValueSerializer");
      for (ulong i = 0; i < 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        ulong restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (ulong i = ulong.MinValue; i < ulong.MinValue + 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        ulong restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (ulong i = ulong.MaxValue - 1000; i < ulong.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        ulong restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void ShortSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(short));
      var valueSerializer = ValueSerializer<short>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "Int16ValueSerializer");
      for (short i = short.MinValue; i < short.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        short restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void UShortSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(ushort));
      var valueSerializer = ValueSerializer<ushort>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "UInt16ValueSerializer");
      for (ushort i = ushort.MinValue; i < ushort.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        ushort restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }


    [Test]
    public void IntSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(int));
      var valueSerializer = ValueSerializer<int>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "Int32ValueSerializer");
      for (int i = -1000; i < 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        int restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (int i = int.MinValue; i < int.MinValue + 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        int restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (int i = int.MaxValue - 1000; i < int.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        int restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void UIntSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(uint));
      var valueSerializer = ValueSerializer<uint>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "UInt32ValueSerializer");
      for (uint i = 0; i < 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        uint restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (uint i = uint.MinValue; i < uint.MinValue + 1000; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        uint restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
      for (uint i = uint.MaxValue - 1000; i < uint.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        uint restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void ByteSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(byte));
      var valueSerializer = ValueSerializer<byte>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "ByteValueSerializer");
      for (byte i = byte.MinValue; i < byte.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        byte restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void SByteSerializerTest()
    {
      MemoryStream stream = new MemoryStream(sizeof(sbyte));
      var valueSerializer = ValueSerializer<sbyte>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "SByteValueSerializer");
      for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, i);
        stream.Seek(0, SeekOrigin.Begin);
        sbyte restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(i, restoredValue);
      }
    }

    [Test]
    public void FloatSerializerTest()
    {
      float[] values = new float[] { float.MinValue, 0, float.MaxValue, 1, 1.2354563f, 123.5f / 345 };
      MemoryStream stream = new MemoryStream(sizeof(float));
      var valueSerializer = ValueSerializer<float>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "SingleValueSerializer");
      foreach (float value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        float restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void DoubleSerializerTest()
    {
      double[] values = new double[] { double.MinValue, 0, double.MaxValue, 1, 1.2354563, 123.5 / 345 };
      MemoryStream stream = new MemoryStream(sizeof(double));
      var valueSerializer = ValueSerializer<double>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "DoubleValueSerializer");
      foreach (double value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        double restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void BooleanSerializerTest()
    {
      bool[] values = new bool[] { true, false };
      MemoryStream stream = new MemoryStream(sizeof(bool));
      var valueSerializer = ValueSerializer<bool>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "BooleanValueSerializer");
      foreach (bool value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        bool restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void DecimalSerializerTest()
    {
      decimal[] values = new decimal[] { decimal.MinValue, decimal.MaxValue, decimal.MinusOne, decimal.One, decimal.Zero, -123.456m, 1.000001m };
      MemoryStream stream = new MemoryStream(sizeof(decimal));
      var valueSerializer = ValueSerializer<decimal>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "DecimalValueSerializer");
      foreach (decimal value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        decimal restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void GuidSerializerTest()
    {
      int count = 1000;
      Guid[] values = new Guid[count];
      for (int i = 0; i < count; i++)
        values[i] = Guid.NewGuid();
      MemoryStream stream = new MemoryStream(sizeof(bool));
      var valueSerializer = ValueSerializer<Guid>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "GuidValueSerializer");
      foreach (Guid value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        Guid restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void CharSerializerTest()
    {
      char[] values = new char[] { char.MinValue, char.MaxValue, ' ', '3' };
      MemoryStream stream = new MemoryStream(sizeof(char));
      var valueSerializer = ValueSerializer<char>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "CharValueSerializer");
      foreach (char value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        char restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void StringSerializerTest()
    {
      string[] values = new string[] { string.Empty, null, "", "testString", new string(new char[100000]), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
      MemoryStream stream = new MemoryStream(sizeof(char));
      var valueSerializer = ValueSerializer<string>.Default;
      Assert.IsNotNull(valueSerializer);
      Assert.AreEqual(valueSerializer.Implementation.GetType().Name, "StringValueSerializer");
      foreach (string value in values)
      {
        stream.Seek(0, SeekOrigin.Begin);
        valueSerializer.Serialize(stream, value);
        stream.Seek(0, SeekOrigin.Begin);
        string restoredValue = valueSerializer.Deserialize(stream);
        Assert.AreEqual(value, restoredValue);
      }
    }

    [Test]
    public void UnknowType()
    {
      var valueSerializer = ValueSerializer<BinaryValueSerializerTest>.Default;
      Assert.IsNull(valueSerializer);
    }
  }
}