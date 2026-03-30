// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Tuples;
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
      var types = Enumerable.Range(0, 4).Select(_ => typeof(short)).ToArray();
      var d = TupleDescriptor.Create(types);
      var dummyTuple = new DummyTuple(d);
      var tuple = CreateTestTuple(d);
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
      var types = new Type[] {typeof(int), typeof(bool), typeof(string)};
      var d = TupleDescriptor.Create(types);
      TestTuple(CreateTestTuple(d));
      TestTuple(new DummyTuple(d));
    }

    private static void TestTuple(Xtensive.Tuples.Tuple tuple)
    {
      Assert.That(tuple.GetFieldState(0).IsAvailable(), Is.False);

      try {
        tuple.GetFieldState(0).IsNull();
        throw new AssertionException("Value is not available. No one knows if it is null or not.");
      } catch (InvalidOperationException) {
      }

      tuple.SetValue(0, 1);
      Assert.That(tuple.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(tuple.GetFieldState(0).IsNull(), Is.False);
      Assert.That(tuple.GetFieldState(0).HasValue(), Is.True);
      Assert.That(tuple.GetValue(0), Is.EqualTo(1));
      Assert.That(tuple.GetValue<int>(0), Is.EqualTo(1));
      Assert.That(tuple.GetValue<int?>(0), Is.EqualTo(new int?(1)));

      tuple.SetValue(0, null);
      Assert.That(tuple.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(tuple.GetFieldState(0).IsNull(), Is.True);
      Assert.That(tuple.GetFieldState(0).HasValue(), Is.False);
      Assert.That(tuple.GetValue(0), Is.EqualTo(null));
      Assert.That(tuple.GetValue<int?>(0), Is.EqualTo(null));

      tuple.SetValue<int?>(0, null);
      Assert.That(tuple.GetFieldState(0).IsAvailable(), Is.True);
      Assert.That(tuple.GetFieldState(0).IsNull(), Is.True);
      Assert.That(tuple.GetFieldState(0).HasValue(), Is.False);
      Assert.That(tuple.GetValue(0), Is.EqualTo(null));
      Assert.That(tuple.GetValue<int?>(0), Is.EqualTo(null));
      Assert.That(tuple.GetValueOrDefault(0), Is.EqualTo(null));
      Assert.That(tuple.GetValueOrDefault<int?>(0), Is.EqualTo(null));

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

      Assert.That(tuple.Equals(tuple), Is.True);
    }

    public void EmptyFieldsTest()
    {
      var d = TupleDescriptor.Create(Array.Empty<Type>());
      var dummyTuple = new DummyTuple(d);
      var tuple = CreateTestTuple(d);
      Assert.That(tuple.Count, Is.EqualTo(0));
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
        var fieldCount = random.Next(0, MaxFieldCount);
        var types = new List<Type>(fieldCount);
        for (int i = 0; i < fieldCount; i++)
          types.Add(fieldTypes[random.Next(0, fieldTypes.Length - 1)]);
        var d = TupleDescriptor.Create(types.ToArray());
        descriptorList.Add(d);
      }

      foreach (TupleDescriptor descriptor in descriptorList) {
        var dummyTuple = new DummyTuple(descriptor);
        var tuple = CreateTestTuple(descriptor);
        for (int fieldIndex = 0; fieldIndex < tuple.Count / 2; fieldIndex++) {
          var type = descriptor[fieldIndex];
          var setValueMethod = setValueMethodGeneric.MakeGenericMethod(type);
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
          Assert.That(tuple.GetFieldState(i).IsAvailable(), Is.EqualTo(available));
        }
        catch (AssertionException)
        {
          Console.Out.WriteLine($"Tuple type: {tuple.GetType().Name}");
          Console.Out.WriteLine($"Field Index: {i}");
          Console.Out.WriteLine();
        }
        
      }
      Assert.That(tuple.GetHashCode(), Is.EqualTo(dummyTuple.GetHashCode()));
      Assert.That(dummyTuple.Equals(tuple), Is.True);
      Assert.That(tuple.Equals(dummyTuple), Is.True);
    }

    protected void AssertAreSame(ITuple source, ITuple target, int startIndex, int targetStartIndex, int count)
    {
      for (int i = 0; i < count; i++) {
        bool available = source.GetFieldState(i + startIndex).IsAvailable();
        try {
          Assert.That(target.GetFieldState(i + targetStartIndex).IsAvailable(), Is.EqualTo(available));
          Assert.That(target.GetValue(i + targetStartIndex), Is.EqualTo(source.GetValue(i + startIndex)));
        }
        catch (AssertionException) {
          Console.Out.WriteLine($"Tuple type: {target.GetType().Name}");
          Console.Out.WriteLine($"Field Index: {i}");
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
