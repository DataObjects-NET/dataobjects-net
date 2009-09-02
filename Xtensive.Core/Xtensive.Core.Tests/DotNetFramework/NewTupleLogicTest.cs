// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.02

using System;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;

namespace Xtensive.Core.Tests.DotNetFramework
{
  [TestFixture]
  public class NewTupleLogicTest
  {
    public const int IterationCount = 10000000;
    public const int MemoryPressureShift = 4;
    public const int MemoryPressureFactor = 16;
    private bool warmup = false;

    #region Nested types: TestTuple, BoxingTuple, NonBoxingTuple

    public abstract class TestTuple
    {
      public abstract object GetValue(int i);
      public abstract void SetValue(int i, object value);
      
      public virtual Delegate GetValueGetter(int i)
      {
        return null;
      }

      public virtual Delegate GetValueSetter(int i)
      {
        return null;
      }

      public T GetValue<T>(int i)
      {
        var func = GetValueGetter(i) as Func<TestTuple, T>;
        if (func!=null)
          return func.Invoke(this);
        else
          return (T) GetValue(i);
      }

      public void SetValue<T>(int i, T value)
      {
        var action = GetValueSetter(i) as Action<TestTuple, T>;
        if (action!=null)
          action.Invoke(this, value);
        else
          SetValue(i, (object) value);
      }
    }

    public class BoxingTuple : TestTuple
    {
      protected long[] values = new long[1];
      
      public override object GetValue(int i)
      {
        return values[i];
      }
      
      public override void SetValue(int i, object value)
      {
        values[i] = (long) value;
      }
    }

    public class NonBoxingTuple : TestTuple
    {
      private static Delegate[] getters;
      private static Delegate[] setters;
      protected long value1;

      public override object GetValue(int i)
      {
        if (i!=0)
          throw new ArgumentOutOfRangeException("i");
        return value1;
      }
      
      public override void SetValue(int i, object value)
      {
        if (i!=0)
          throw new ArgumentOutOfRangeException("i");
        value1 = (long) value;
      }

      public override Delegate GetValueGetter(int i)
      {
        return getters[i];
      }

      public override Delegate GetValueSetter(int i)
      {
        return setters[i];
      }

      static NonBoxingTuple()
      {
        getters = new Delegate[1];
        Func<TestTuple, long> getter1 = t => ((NonBoxingTuple) t).value1;
        getters[0] = getter1;

        setters = new Delegate[1];
        Action<TestTuple, long> setter1 = (t,v) => ((NonBoxingTuple) t).value1 = v;
        setters[0] = setter1;
      }
    }

    #endregion

    [Test]
    public void RegularTest()
    {
      warmup = true;
      Test((int) (0.01 * IterationCount));
      warmup = false;
      Test((int) (0.01 * IterationCount));
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      warmup = true;
      Test((int) (0.1 * IterationCount));
      warmup = false;
      Test((int) (1.0 * IterationCount));
    }

    private void Test(int count)
    {
      TestTupleAccess(new BoxingTuple(), count);
      TestTupleAccess(new NonBoxingTuple(), count);
    }

    private void TestTupleAccess(TestTuple tuple, int count)
    {
      object[] pressure;
      TestHelper.CollectGarbage();
      pressure = new object[count / MemoryPressureFactor];
      using (warmup ? (IDisposable)new Disposing.Disposable(delegate { }) : 
        new Measurement("Setter, with boxing", count)) {
        for (long i = 0; i < count; i++) {
          tuple.SetValue<long>(0, i);
          if (0 == (i & (MemoryPressureFactor-1)))
            pressure[i >> MemoryPressureShift] = new object();
        }
      }
      pressure = null;
      TestHelper.CollectGarbage();
      pressure = new object[count / MemoryPressureFactor];
      using (warmup ? (IDisposable)new Disposing.Disposable(delegate { }) : 
        new Measurement("Getter, with boxing", count)) {
        for (long i = 0; i < count; i++) {
          tuple.GetValue<long>(0);
          if (0 == (i & (MemoryPressureFactor-1)))
            pressure[i >> MemoryPressureShift] = new object();
        }
      }
    }
  }
}