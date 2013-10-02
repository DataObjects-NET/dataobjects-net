// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.01

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class FieldTypeTest
  {
    #region Nested type: ThreadData<T>

    internal static class ThreadData<T>
      where T : class
    {
      private struct SlotData
      {
        public int ManagerThreadId;
        public T Value;

        public SlotData(int managerThreadId, T value)
        {
          ManagerThreadId = managerThreadId;
          Value = value;
        }
      }

      private const int MaxData = 65536;
      private const int MaxDataMask = MaxData-1;
      private static object[] data;
      private static Type slotDataType;

      public static T Get()
      {
        int tid = Thread.CurrentThread.ManagedThreadId;
        object bigSlot = data[tid & MaxDataMask];
        if (bigSlot==null)
          return null;
        if (bigSlot.GetType()!=slotDataType)
          return bigSlot as T;
        else
          return GetFromBigSlot(tid, bigSlot);
      }

      private static T GetFromBigSlot(int tid, object bigSlot)
      {
        var slots = bigSlot as SlotData[];
        foreach (var slot in slots) {
          if (slot.ManagerThreadId==tid)
            return slot.Value;
        }
        return null;
      }

      public static void Set(T value)
      {
        var thread = Thread.CurrentThread;
        int tid = thread.ManagedThreadId;
        var index = tid & MaxDataMask;
        object bigSlot = data[index];
        lock (data) {
          var slots = bigSlot as SlotData[];
          if (slots==null) {
            data[index] = value;
            Thread.MemoryBarrier();
            return;
          }
          var newSlots = new SlotData[slots.Length];
          slots.CopyTo(newSlots, 1);
          newSlots[0] = new SlotData(tid, value);
          data[index] = newSlots;
          Thread.MemoryBarrier();
        }
      }

      public static void Initiialize()
      {
        data = new object[MaxData];
        slotDataType = typeof (SlotData);
      }
    }

    #endregion

    public const int IterationCount = 1000000000;

    public object InstanceField;
    public object VolatileInstanceField;
    public static object StaticField;
    [ThreadStatic]
    public static object ThreadStaticField;

    public class Host
    {
      public static object StaticField;

      public virtual void GetStaticField(int iterationCount)
      {
        object o = null;
        for (int i = 0; i<iterationCount; i++)
          o = StaticField;
      }
    }

    public class Host<T> : Host
    {
      // We're using different StaticField here
      public new static object StaticField;

      public override void GetStaticField(int iterationCount)
      {
        object o = null;
        for (int i = 0; i<iterationCount; i++)
          o = StaticField;
      }
    }

    [Test, Explicit]
    public void CombinedTest()
    {
      var slot = Thread.AllocateDataSlot();
      object o;

      int ic = IterationCount;
      using (new Measurement("Instance field", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++)
          o = InstanceField;
      using (new Measurement("Volatile instance field", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++)
          o = VolatileInstanceField;
      using (new Measurement("Static field", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++)
          o = StaticField;

      Host host = new Host();
      using (new Measurement("Static field of Host", MeasurementOptions.Log, ic))
        host.GetStaticField(ic);

      host = new Host<int>();
      using (new Measurement("Static field of Host<int>", MeasurementOptions.Log, ic))
        host.GetStaticField(ic);

      host = new Host<Array>();
      using (new Measurement("Static field of Host<Array>", MeasurementOptions.Log, ic))
        host.GetStaticField(ic);

      ic /= 100;
      using (new Measurement("ThreadStatic field", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++)
          o = ThreadStaticField;
      
      ThreadData<FieldTypeTest>.Initiialize();
      ThreadData<FieldTypeTest>.Set(this);
      using (new Measurement("Own ThreadData", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++)
          o = ThreadData<FieldTypeTest>.Get();
      byte[] bytes;
      int length = 128;
      using (new Measurement("new byte[128]", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++) {
          bytes = new byte[length];
          bytes[0] = (byte) i;
        }
      ic /= 10;
      using (new Measurement("Thread data slot", MeasurementOptions.Log, ic))
        for (int i = 0; i<ic; i++)
          o = Thread.GetData(slot);
    }
  }
}