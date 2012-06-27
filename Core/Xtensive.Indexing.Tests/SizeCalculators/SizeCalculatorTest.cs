// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Indexing.SizeCalculators;
using Xtensive.Tuples;
using Xtensive.Diagnostics;
using Xtensive.Testing;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Tests.SizeCalculators
{
  [TestFixture]
  public class SizeCalculatorTest
  {
    internal class TestClass : ISizeCalculatorAware
    {
      private readonly int size;

      public int GetSize(ISizeCalculatorProvider provider)
      {
        return size;
      }

      public TestClass(int size)
      {
        this.size = size;
      }
    }

    internal struct TestStruct
    {
      public string Name;
      public object Value;
      public int    Age;
      public bool?  HasChildren;
    }

    [Test]
    public void PrimitiveTypesTest()
    {
      PrimitiveTypeTest<bool>("BooleanSizeCalculator", sizeof(bool));
      PrimitiveTypeTest<byte>("ByteSizeCalculator", sizeof(byte));
      PrimitiveTypeTest<char>("CharSizeCalculator", sizeof(char));
      PrimitiveTypeTest<double>("DoubleSizeCalculator", sizeof(double));
      PrimitiveTypeTest<float>("SingleSizeCalculator", sizeof(float));
      PrimitiveTypeTest<short>("Int16SizeCalculator", sizeof(short));
      PrimitiveTypeTest<int>("Int32SizeCalculator", sizeof(int));
      PrimitiveTypeTest<long>("Int64SizeCalculator", sizeof(long));
      PrimitiveTypeTest<ushort>("UInt16SizeCalculator", sizeof(ushort));
      PrimitiveTypeTest<uint>("UInt32SizeCalculator", sizeof(uint));
      PrimitiveTypeTest<ulong>("UInt64SizeCalculator", sizeof(ulong));
      PrimitiveTypeTest<decimal>("DecimalSizeCalculator", sizeof(decimal));
      PrimitiveTypeTest<Guid>("GuidSizeCalculator", 16);
    }

    private static void PrimitiveTypeTest<T>(string expectedCalculator, int fieldSize)
    {
      SizeCalculator<T> calculator = SizeCalculator<T>.Default;
      Assert.IsNotNull(calculator);
      Assert.AreEqual(expectedCalculator, calculator.Implementation.GetType().Name);
      Assert.AreEqual(fieldSize, calculator.GetDefaultSize());
      Assert.AreEqual(fieldSize, calculator.GetValueSize(default(T)));
    }

    [Test]
    public void ObjectTest()
    {
      SizeCalculator<object> calculator = SizeCalculator<object>.Default;
      Assert.IsNotNull(calculator);
      Assert.AreEqual("ObjectSizeCalculator`1", calculator.Implementation.GetType().Name);

      int minSize = calculator.GetDefaultSize();
      int size1 = calculator.GetValueSize(null);
      int size2 = calculator.GetValueSize(new object());
      Log.Info("Default size: {0}", minSize);
      Log.Info("Value size 1: {0}", size1);
      Log.Info("Value size 2: {0}", size2);
      Assert.AreEqual(minSize, RuntimeInfo.PointerSize * 4);
      Assert.AreEqual(size1, RuntimeInfo.PointerSize);
      Assert.AreEqual(size2, minSize);
    }

    [Test]
    public void StructTest()
    {
      SizeCalculator<TestStruct> calculator = SizeCalculator<TestStruct>.Default;
      Assert.IsNotNull(calculator);
      Assert.AreEqual("ObjectSizeCalculator`1", calculator.Implementation.GetType().Name);

      int minSize = calculator.GetDefaultSize();
      int size = calculator.GetValueSize(new TestStruct());
      Log.Info("Default size: {0}", minSize);
      Log.Info("Value size:   {0}", size);
      Log.Info("Bool size:   {0}", sizeof(bool));
      Assert.AreEqual(SizeCalculatorBase<int>.GetPackedStructSize(RuntimeInfo.PointerSize * 2 + sizeof(Int32) + sizeof(bool) + sizeof(bool)), minSize);
      Assert.AreEqual(minSize, size);
    }

    [Test]
    public void BoxTest()
    {
      ISizeCalculatorBase calculator = SizeCalculatorProvider.Default.GetSizeCalculatorByInstance(1);
      Assert.IsNotNull(calculator);
      Assert.AreEqual("BoxSizeCalculator`1", calculator.GetType().Name);

      int minSize = calculator.GetDefaultSize();
      int size1 = calculator.GetInstanceSize(null);
      int size2 = calculator.GetInstanceSize(1);
      Log.Info("Default size: {0}", minSize);
      Log.Info("Value size 1: {0}", size1);
      Log.Info("Value size 2: {0}", size2);
      Assert.AreEqual(RuntimeInfo.PointerSize * 3 + sizeof(Int32), minSize);
      Assert.AreEqual(RuntimeInfo.PointerSize, size1);
      Assert.AreEqual(minSize, size2);
    }

    [Test]
    public void PairTest()
    {
      SizeCalculator<Pair<TestStruct,object>> calculator = SizeCalculator<Pair<TestStruct,object>>.Default;
      Assert.IsNotNull(calculator);
      Assert.AreEqual("PairSizeCalculator`2", calculator.Implementation.GetType().Name);

      int minSize = calculator.GetDefaultSize();
      int size = calculator.GetValueSize(new Pair<TestStruct,object>(new TestStruct(), new TestStruct()));
      Log.Info("Default size: {0}", minSize);
      Log.Info("Value size:   {0}", size);
      Assert.AreEqual(SizeCalculatorBase<int>.GetPackedStructSize(RuntimeInfo.PointerSize * 3 + sizeof(Int32) + sizeof(bool) + sizeof(bool)), minSize);
      Assert.IsTrue(size > minSize*2);
    }

    [Test]
    public void SizeCalculatorAwareTest()
    {
      int rndSize = RandomManager.CreateRandom(SeedVariatorType.CallingMethod).Next(1000);
      TestClass testClass = new TestClass(rndSize);

      SizeCalculator<TestClass> calculator = SizeCalculator<TestClass>.Default;
      Assert.IsNotNull(calculator);
      Assert.AreEqual("SizeCalculatorAwareInterfaceSizeCalculator`1", calculator.Implementation.GetType().Name);

      int minSize = calculator.GetDefaultSize();
      int size = calculator.GetValueSize(testClass);
      Log.Info("Default size: {0}", minSize);
      Log.Info("Value size:   {0}", size);
      Assert.AreEqual(rndSize, size);
    }

    [Test]
    public void TupleTest()
    {
      Xtensive.Tuples.Tuple tuple = Xtensive.Tuples.Tuple.Create(new Type[] {typeof(int), typeof(string), typeof(bool), typeof(byte)});
      string testString = "Test string. Sample...";
      tuple.SetValue(0, 0x11111111);
      tuple.SetValue(1, testString);
      tuple.SetValue(2, true);
      tuple.SetValue(3, (byte)0x33);

      SizeCalculator<Xtensive.Tuples.Tuple> calculator = SizeCalculator<Xtensive.Tuples.Tuple>.Default;
      Assert.IsNotNull(calculator);
      Assert.AreEqual("TupleSizeCalculator", calculator.Implementation.GetType().Name);

      int minSize = calculator.GetDefaultSize();
      int size = calculator.GetValueSize(tuple);
      Log.Info("Default size: {0}", minSize);
      Log.Info("Value size:   {0}", size);
      Assert.IsTrue(size > minSize + testString.Length*2);
    }
  }
}