// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.23

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Diagnostics;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using MethodInfo=System.Reflection.MethodInfo;

namespace Xtensive.Orm.Tests.Core.Tuples
{
  public abstract class TupleBehaviorTestBase
  {
    protected static readonly Type[] fieldTypes = 
      new Type[]{
        typeof (int),
        typeof (byte),
        typeof (short),
        typeof (string),
        typeof (DateTime),
        typeof (decimal),
        typeof (float),
      };

    private const int MaxFieldCount = 100;

    protected abstract Xtensive.Tuples.Tuple CreateTestTuple(TupleDescriptor descriptor);

    protected virtual Xtensive.Tuples.Tuple CreateTestTuple(Xtensive.Tuples.Tuple source)
    {
      return source;
    }

    public void Test()
    {
      IList<Type> types = new List<Type>();
      for (int i = 0; i < 4; i++)
        types.Add(typeof (short));

      TupleDescriptor descriptor = TupleDescriptor.Create(types);
      DummyTuple dummyTuple = new DummyTuple(descriptor);
      ITuple tuple = CreateTestTuple(descriptor);
      PopulateData(types, dummyTuple, tuple);
      AssertAreSame(dummyTuple, tuple);
    }

    protected static void PopulateData(IList<Type> types, ITuple sourceTuple, ITuple tuple)
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      MethodInfo setValueMethodGeneric =
        typeof(TupleBehaviorTestBase).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Static);
      for (int fieldIndex = 0; fieldIndex < types.Count / 2; fieldIndex++) {
        Type type = types[fieldIndex];
        MethodInfo setValueMethod = setValueMethodGeneric.MakeGenericMethod(type);
        setValueMethod.Invoke(null, new object[] { sourceTuple, tuple, fieldIndex, random });
      }
    }

    protected static void PopulateData(IList<Type> types, Xtensive.Tuples.Tuple tuple)
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      MethodInfo setValueMethodGeneric =
        typeof(TupleBehaviorTestBase).GetMethod("InternalSetValue", BindingFlags.NonPublic | BindingFlags.Static);
      for (int fieldIndex = 0; fieldIndex < tuple.Count; fieldIndex++) {
        Type type = types[fieldIndex % types.Count];
        MethodInfo setValueMethod = setValueMethodGeneric.MakeGenericMethod(type);
        setValueMethod.Invoke(null, new object[] { tuple, fieldIndex, random });
      }
    }

    public void BehaviorTest()
    {
      List<Type> fields = new List<Type>(3);
      fields.AddRange(new Type[] {typeof(int), typeof(bool), typeof(string)});

      TupleDescriptor d = TupleDescriptor.Create(fields);
      TestTuple(CreateTestTuple(d));
      TestTuple(new DummyTuple(d));
    }

    private static void TestTuple(Xtensive.Tuples.Tuple tuple)
    {
      Assert.IsFalse(tuple.GetFieldState(0).IsAvailable());

      try {
        tuple.GetFieldState(0).IsNull();
        throw new AssertionException("Value is not available. No one knows if it is null or not.");
      } catch (InvalidOperationException) {
      }

      tuple.SetValue(0, 1);
      Assert.IsTrue(tuple.GetFieldState(0).IsAvailable());
      Assert.IsFalse(tuple.GetFieldState(0).IsNull());
      Assert.IsTrue(tuple.GetFieldState(0).HasValue());
      Assert.AreEqual(1, tuple.GetValue(0));
      Assert.AreEqual(1, tuple.GetValue<int>(0));
      Assert.AreEqual(new int?(1), tuple.GetValue<int?>(0));

      tuple.SetValue(0, null);
      Assert.IsTrue(tuple.GetFieldState(0).IsAvailable());
      Assert.IsTrue(tuple.GetFieldState(0).IsNull());
      Assert.IsFalse(tuple.GetFieldState(0).HasValue());
      Assert.AreEqual(null, tuple.GetValue(0));
      Assert.AreEqual(null, tuple.GetValue<int?>(0));

      tuple.SetValue<int?>(0, null);
      Assert.IsTrue(tuple.GetFieldState(0).IsAvailable());
      Assert.IsTrue(tuple.GetFieldState(0).IsNull());
      Assert.IsFalse(tuple.GetFieldState(0).HasValue());
      Assert.AreEqual(null, tuple.GetValue(0));
      Assert.AreEqual(null, tuple.GetValue<int?>(0));
      Assert.AreEqual(null, tuple.GetValueOrDefault(0));
      Assert.AreEqual(null, tuple.GetValueOrDefault<int?>(0));

      try {
        tuple.GetValue(1);
        throw new AssertionException("Value should not be available.");
      } catch (InvalidOperationException) {
      }

      try {
        tuple.GetValue<string>(2);
        throw new AssertionException("Value should not be available.");
      } catch (InvalidOperationException) {
      }

      try {
        tuple.GetValue<byte>(0);
        throw new AssertionException("Null reference or Invalid cast exception should be thrown.");
      } 
      catch (NullReferenceException) {}
      catch (InvalidCastException) {}

      Assert.IsTrue(tuple.Equals(tuple));
    }

    public void EmptyFieldsTest()
    {
      List<Type> fields = new List<Type>();
      TupleDescriptor descriptor = TupleDescriptor.Create(fields);
      DummyTuple dummyTuple = new DummyTuple(descriptor);
      ITuple tuple = CreateTestTuple(descriptor);
      Assert.AreEqual(0, tuple.Count);
    }

    public void RandomTest()
    {
      const int IterationCount = 10;
      int iteration = 0;
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
        
      MethodInfo setValueMethodGeneric =
            typeof(TupleBehaviorTestBase).GetMethod("SetValue", BindingFlags.NonPublic | BindingFlags.Static);
      IList<TupleDescriptor> descriptorList = new List<TupleDescriptor>();

      while (iteration++ < IterationCount) {
        int fieldCount = random.Next(0, MaxFieldCount);
        List<Type> fields = new List<Type>(fieldCount);
        for (int i = 0; i < fieldCount; i++)
          fields.Add(fieldTypes[random.Next(0, fieldTypes.Length - 1)]);
        TupleDescriptor descriptor = TupleDescriptor.Create(fields);
        descriptorList.Add(descriptor);
      }

      foreach (TupleDescriptor descriptor in descriptorList) {
        DummyTuple dummyTuple = new DummyTuple(descriptor);
        ITuple tuple = CreateTestTuple(descriptor);
        for (int fieldIndex = 0; fieldIndex < tuple.Count / 2; fieldIndex++) {
          Type type = descriptor[fieldIndex];
          MethodInfo setValueMethod = setValueMethodGeneric.MakeGenericMethod(type);
          setValueMethod.Invoke(null, new object[] { dummyTuple, tuple, fieldIndex, random });
        }
        AssertAreSame(dummyTuple, tuple);  
      }
      
    }

    protected void AssertAreSame(ITuple dummyTuple, ITuple tuple)
    {
      for (int i = 0; i < dummyTuple.Count; i++) {
        bool available = dummyTuple.GetFieldState(i).IsAvailable();
        try {
          Assert.AreEqual(available, tuple.GetFieldState(i).IsAvailable());
        }
        catch (AssertionException)
        {
          Console.Out.WriteLine(string.Format("Tuple type: {0}", tuple.GetType().Name));
          Console.Out.WriteLine(string.Format("Field Index: {0}", i));
          Console.Out.WriteLine();
        }
        
      }
      Assert.AreEqual(dummyTuple.GetHashCode(), tuple.GetHashCode());
      Assert.IsTrue(dummyTuple.Equals(tuple));
      Assert.IsTrue(tuple.Equals(dummyTuple));
    }

    protected void AssertAreSame(ITuple source, ITuple target, int startIndex, int targetStartIndex, int count)
    {
      for (int i = 0; i < count; i++) {
        bool available = source.GetFieldState(i + startIndex).IsAvailable();
        try {
          Assert.AreEqual(available, target.GetFieldState(i + targetStartIndex).IsAvailable());
          Assert.AreEqual(source.GetValue(i + startIndex), target.GetValue(i + targetStartIndex));
        }
        catch (AssertionException) {
          Console.Out.WriteLine(string.Format("Tuple type: {0}", target.GetType().Name));
          Console.Out.WriteLine(string.Format("Field Index: {0}", i));
          Console.Out.WriteLine();
        }
      }
    }

    private static void SetValue<T>(ITuple dummyTuple, ITuple tuple, int fieldIndex, Random random)
    {
      IInstanceGenerator<T> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>();
      T instance = generator.GetInstance(random);
      dummyTuple.SetValue(fieldIndex, instance);
      tuple.SetValue(fieldIndex, instance);
    }

    private static void InternalSetValue<T>(Xtensive.Tuples.Tuple tuple, int fieldIndex, Random random)
    {
      IInstanceGenerator<T> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>();
      T instance = generator.GetInstance(random);
      tuple.SetValue(fieldIndex, instance);
    }
  }
}