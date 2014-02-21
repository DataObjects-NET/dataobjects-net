// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.04.17

using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  public abstract class RootObject
  {
    
  }

  public class InheritedObject1 : RootObject
  {

  }

  public class InheritedObject2 : InheritedObject1
  {

  }

  public sealed class InheritedObject3 : InheritedObject2
  {
    public object Reference;
  }

  public sealed class SlimObject
  {
    public object Reference;
  }

  public sealed class FinalizableSlimObject
  {
    public object Reference;

    ~FinalizableSlimObject()
    {
    }
  }

  public struct SlimStruct
  {
    public object Reference1;
    public object Reference2;
    public object Reference3;
  }


  [TestFixture]
  public class AllocationTest
  {
    public const int IterationCount = 10000000;
    public const int MbSize = 1024*1024;
    public readonly static int ClassSize  = IntPtr.Size * 3;
    public readonly static int StructSize = IntPtr.Size * 3;

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      Test(1);
      Test(0.1);
      Test(0.01);
      Test(0.001);
    }

    [Test]
    public void RegularTest()
    {
      Test(0.01);
      Test(0.001);
    }

    private void Test(double speedFactor)
    {
      TestLog.Info("Class size: {0} bytes", ClassSize);
      TestClassAllocation(speedFactor);
      TestStructAllocation(speedFactor);
    }

    private void TestClassAllocation(double speedFactor)
    {
      // Warmup
      int iterations = 100;
      AllocateClass_SlimObject(iterations);
      AllocateClass_SlimObject_ByFormatterServices(iterations);
      AllocateClass_FinalizableSlimObject(iterations);
      AllocateClass_InheritedObject3(iterations);
      AllocateClass<SlimObject>(iterations);
      AllocateClass<FinalizableSlimObject>(iterations);
      AllocateClassArray_SlimObject(new SlimObject[iterations]);
      AllocateClassArray_InheritedObject3(new InheritedObject3[iterations]);
      AllocateClassArray_FinalizableSlimObject(new FinalizableSlimObject[iterations]);
      AllocateClassArray(new SlimObject[iterations]);
      AllocateClassArray(new FinalizableSlimObject[iterations]);
      // Real test
      iterations = (int)(IterationCount*speedFactor) / 10 * 10;
      using (TestLog.InfoRegion("Object allocation")) {
        TestHelper.CollectGarbage();
        using (TestLog.InfoRegion("Allocation to nothing")) {
          using (new Measurement("SlimObject", MeasurementOptions.Log, iterations))
            AllocateClass_SlimObject(iterations);
          TestHelper.CollectGarbage();
          using (new Measurement("SlimObject (using FormatterServices)", MeasurementOptions.Log, iterations))
            AllocateClass_SlimObject_ByFormatterServices(iterations);
          TestHelper.CollectGarbage();
          using (new Measurement("FinalizableSlimObject", MeasurementOptions.Log, iterations))
            AllocateClass_FinalizableSlimObject(iterations);
          TestHelper.CollectGarbage();
          using (new Measurement("SlimObject, generic", MeasurementOptions.Log, iterations))
            AllocateClass<SlimObject>(iterations);
          TestHelper.CollectGarbage();
          using (new Measurement("FinalizableSlimObject, generic", MeasurementOptions.Log, iterations))
            AllocateClass<FinalizableSlimObject>(iterations);
          TestHelper.CollectGarbage();
          using (new Measurement("InheritedObject3", MeasurementOptions.Log, iterations))
            AllocateClass_InheritedObject3(iterations);
          TestHelper.CollectGarbage();
        }
        using (TestLog.InfoRegion("Allocation to array")) {
          using (new Measurement("SlimObject", MeasurementOptions.Log, iterations))
            AllocateClassArray_SlimObject(new SlimObject[iterations]);
          TestHelper.CollectGarbage();
          using (new Measurement("FinalizableSlimObject", MeasurementOptions.Log, iterations))
            AllocateClassArray_FinalizableSlimObject(new FinalizableSlimObject[iterations]);
          TestHelper.CollectGarbage();
          using (new Measurement("SlimObject, generic", MeasurementOptions.Log, iterations))
            AllocateClassArray(new SlimObject[iterations]);
          TestHelper.CollectGarbage();
          using (new Measurement("FinalizableSlimObject, generic", MeasurementOptions.Log, iterations))
            AllocateClassArray(new FinalizableSlimObject[iterations]);
          TestHelper.CollectGarbage();
          using (new Measurement("InheritedObject3", MeasurementOptions.Log, iterations))
            AllocateClassArray_InheritedObject3(new InheritedObject3[iterations]);
          TestHelper.CollectGarbage();
        }
      }
    }

    private void TestStructAllocation(double speedFactor)
    {
      // Warmup
      int iterations = 100;
      AllocateStructArray_SlimStruct(iterations);
      AllocateStructArray<SlimStruct>(iterations);
      AllocateBoxedStructArray_SlimStruct(new object[iterations]);
      AllocateBoxedStructArray<SlimStruct>(new object[iterations]);
      // Real test
      iterations = (int)(IterationCount*speedFactor) / 10 * 10;
      using (TestLog.InfoRegion("Struct allocation")) {
        TestHelper.CollectGarbage();
        using (TestLog.InfoRegion("Allocation of array")) {
          using (new Measurement("SlimStruct", MeasurementOptions.Log, iterations))
            AllocateStructArray_SlimStruct(iterations);
          TestHelper.CollectGarbage();
          using (new Measurement("SlimStruct, generic", MeasurementOptions.Log, iterations))
            AllocateStructArray<SlimStruct>(iterations);
          TestHelper.CollectGarbage();
        }
        using (TestLog.InfoRegion("Allocation of array with boxing")) {
          using (new Measurement("SlimStruct", MeasurementOptions.Log, iterations))
            AllocateBoxedStructArray_SlimStruct(new object[iterations]);
          TestHelper.CollectGarbage();
          using (new Measurement("SlimStruct, generic", MeasurementOptions.Log, iterations))
            AllocateBoxedStructArray<SlimStruct>(new object[iterations]);
          TestHelper.CollectGarbage();
        }
      }
    }

    private void AllocateClass_SlimObject(int iterationCount)
    {
      for (int i = 0; i<iterationCount; i+=10) {
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
        new SlimObject();
      }
    }

    private void AllocateClass_InheritedObject3(int iterationCount)
    {
      for (int i = 0; i < iterationCount; i += 10)
      {
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
        new InheritedObject3();
      }
    }

    private void AllocateClass_SlimObject_ByFormatterServices(int iterationCount)
    {
      var type = typeof(SlimObject);
      for (int i = 0; i<iterationCount; i+=10) {
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
        FormatterServices.GetUninitializedObject(type);
      }
    }

    private void AllocateClass_FinalizableSlimObject(int iterationCount)
    {
      for (int i = 0; i<iterationCount; i+=10) {
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
        new FinalizableSlimObject();
      }
    }

    private void AllocateClass<T>(int iterationCount)
      where T: class, new()
    {
      for (int i = 0; i<iterationCount; i+=10) {
        new T();
        new T();
        new T();
        new T();
        new T();
        new T();
        new T();
        new T();
        new T();
        new T();
      }
    }

    private void AllocateClassArray_SlimObject(SlimObject[] array)
    {
      int iterationCount = array.Length;
      for (int i = 0; i<iterationCount; ) {
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
        array[i++] = new SlimObject();
      }
    }

    private void AllocateClassArray_InheritedObject3(InheritedObject3[] array)
    {
      int iterationCount = array.Length;
      for (int i = 0; i<iterationCount; ) {
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        array[i++] = new InheritedObject3();
        
      }
    }

    private void AllocateClassArray_FinalizableSlimObject(FinalizableSlimObject[] array)
    {
      int iterationCount = array.Length;
      for (int i = 0; i<iterationCount; ) {
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
        array[i++] = new FinalizableSlimObject();
      }
    }

    private void AllocateClassArray<T>(T[] array)
      where T: class, new()
    {
      int iterationCount = array.Length;
      for (int i = 0; i<iterationCount; ) {
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
      }
    }

    private void AllocateStructArray_SlimStruct(int iterationCount)
    {
      object o = new SlimStruct[iterationCount];
    }

    private void AllocateStructArray<T>(int iterationCount)
      where T: struct
    {
      object o = new T[iterationCount];
    }

    private void AllocateBoxedStructArray_SlimStruct(object[] array)
    {
      int iterationCount = array.Length;
      for (int i = 0; i<iterationCount; ) {
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
        array[i++] = new SlimStruct();
      }
    }

    private void AllocateBoxedStructArray<T>(object[] array)
      where T: struct
    {
      int iterationCount = array.Length;
      for (int i = 0; i<iterationCount; ) {
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
        array[i++] = new T();
      }
    }
  }
}