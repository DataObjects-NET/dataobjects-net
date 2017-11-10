// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.02

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class NewTupleLogicTest
  {
    public const int IterationCount = 10000000;
    public const int MemoryPressureShift = 4;
    public const int MemoryPressureFactor = 16;
    private bool warmup = false;

    #region Nested types: TestTuple, BoxingTuple, NonBoxingTuple

    public delegate T TupleFieldGetter<T>(TestTuple tuple, out TupleFieldState fieldState);

    public abstract class TestTuple
    {
      public abstract object GetValue(int i, out TupleFieldState fieldState);
      public abstract void SetValue(int i, object value);
      
      public virtual Delegate GetValueGetter(int i)
      {
        return null;
      }

      public virtual Delegate GetValueSetter(int i)
      {
        return null;
      }

      public T GetValue<T>(int i, out TupleFieldState fieldState)
      {
        var func = GetValueGetter(i) as TupleFieldGetter<T>;
        if (func!=null)
          return func.Invoke(this, out fieldState);
        else
          return (T) GetValue(i, out fieldState);
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
      
      public override object GetValue(int i, out TupleFieldState fieldState)
      {
        fieldState = TupleFieldState.Available;
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

      public override object GetValue(int i, out TupleFieldState fieldState)
      {
        if (i!=0)
          throw new ArgumentOutOfRangeException("i");
        fieldState = TupleFieldState.Available;
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
        TupleFieldGetter<long> getter1 = delegate(TestTuple t, out TupleFieldState s) {
          s = TupleFieldState.Available;
          return ((NonBoxingTuple) t).value1;
        };
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
      using (warmup ? new Disposable(delegate { }) :
        TestLog.InfoRegion("With boxing"))
        TestTupleAccess(new BoxingTuple(), count);
      using (warmup ? new Disposable(delegate { }) :
        TestLog.InfoRegion("Without boxing"))
        TestTupleAccess(new NonBoxingTuple(), count);
    }

    private void TestTupleAccess(TestTuple tuple, int count)
    {
      object[] pressure;
      TestHelper.CollectGarbage();
      pressure = new object[count / MemoryPressureFactor];
      using (warmup ? (IDisposable)new Disposable(delegate { }) : 
        new Measurement("Setter", count)) {
        for (long i = 0; i < count; i++) {
          tuple.SetValue<long>(0, i);
//          if (0 == (i & (MemoryPressureFactor-1)))
//            pressure[i >> MemoryPressureShift] = new object();
        }
      }
      pressure = null;
      TestHelper.CollectGarbage();
      pressure = new object[count / MemoryPressureFactor];
      TupleFieldState fieldState;
      using (warmup ? (IDisposable)new Disposable(delegate { }) : 
        new Measurement("Getter", count)) {
        for (long i = 0; i < count; i++) {
          tuple.GetValue<long>(0, out fieldState);
//          if (0 == (i & (MemoryPressureFactor-1)))
//            pressure[i >> MemoryPressureShift] = new object();
        }
      }
    }
  }
}