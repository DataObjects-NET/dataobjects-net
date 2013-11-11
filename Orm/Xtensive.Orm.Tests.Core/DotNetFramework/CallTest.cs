// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  public interface IContainer<T>
  {
    T Value { get; set; }
    T GetValue();
    void GenericMethod1<T>();
    void GenericMethod2<T1,T2>();
  }

  public abstract class ContainerBase<T>
  {
    public abstract T Value { get; set; }
    public abstract T GetValue();
    public abstract object BoxedValue { get; set; }
    public abstract void GenericMethod1<T>();
    public abstract void GenericMethod2<T1,T2>();
  }

  public sealed class Container<T>: ContainerBase<T>, IContainer<T>
  {
    private T value;

    T IContainer<T>.Value {
      get { return value; }
      set { this.value = value; }
    }

    public override T Value {
      get { return value; }
      set { this.value = value; }
    }

    public override T GetValue() 
    {
      return value;
    }

    public override object BoxedValue {
      get { return value; }
      set { this.value = (T)value; }
    }

    public void Method()
    {
    }

    public void Method1<T1>()
    {
    }

    public void Method2<T1,T2>()
    {
    }

    public override void GenericMethod1<T1>()
    {
    }

    public override void GenericMethod2<T1, T2>()
    {
    }

    public Container(T value)
    {
      this.value = value;
    }
  }

  public class Caller<T>
  {
    private IContainer<T> container;

    public virtual void GetContainerValue()
    {
      container.GetValue();
    }

    public Caller(IContainer<T> container)
    {
      this.container = container;
    }
  }

  [TestFixture]
  public class CallTest
  {
    public class FastCache<T>
    {
      public static IContainer<T> Value;
    }

    private bool isRegularTestRunning;
    public const int IterationCount = 10000000;

    [Test]
    public void RegularTest()
    {
      isRegularTestRunning = true;
      Test(0.01);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      isRegularTestRunning = false;
      Test(1);
    }    

    [Test]
    [Explicit]
    [Category("Profile")]
    public void ProfileTest()
    {
      isRegularTestRunning = false;
      GVMethod1CallTest<int,int>(1);
      GVMethod1CallTest<string,string>(1);
    }

    public virtual int? DefaultNullableMethod()
    {
      return null;
    }

    public virtual int? NullableMethod(int i)
    {
      var result = new int?(i);
      return result;
    }

    [Test]
    public void NullableResultMethodTest()
    {
      using (new Measurement("Default int?", IterationCount))
        for (int i = 0; i < IterationCount; i++) {
          var result = DefaultNullableMethod();
        }

      using (new Measurement("New instance int?", IterationCount))
        for (int i = 0; i < IterationCount; i++) {
          var result = NullableMethod(i);
        }
    }

    public void Test(double speedFactor)
    {
      MethodCallTest<int>(1*speedFactor);
      GMethod1CallTest<int,int>(1*speedFactor);
      GMethod2CallTest<int,int,int>(1*speedFactor);
      VMethodCallTest<bool>(1*speedFactor);
      VMethodCallTest<int>(1*speedFactor);
      VMethodCallTest<long>(1*speedFactor);
//      VMethodCallTest<double>(1*speedFactor);
//      VMethodCallTest<Guid>(0.5*speedFactor);
//      VMethodCallTest<Pair<Guid, Guid>>(0.2*speedFactor);
      VMethodCallTest<Pair<Pair<Guid, Guid>, Pair<Guid, Guid>>>(0.1*speedFactor);
      VMethodCallTest<string>(0.3*speedFactor);
      GVMethod1CallTest<int,int>(0.5*speedFactor);
      GVMethod1CallTest<string,string>(0.5*speedFactor);
      GVMethod2CallTest<int,int,int>(0.25*speedFactor);
      GVMethod2CallTest<string,string,string>(0.25*speedFactor);
    }

    private void MethodCallTest<T>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      Container<T> c = new Container<T>(o);
      Action a = c.Method;
      // Warmup
      int iterations = 100;
      CallClassMethod<T>(c, iterations);
      a.Invoke();
      CastClassMethod<T>(c, iterations);
      // Real test
      iterations = (int)(IterationCount*speedFactor);
      TestLog.Info("Regular call test:");
      TestLog.Info("  Type: {0}", typeof(T).GetShortName());
      FastCache<T>.Value = null;
      using (Orm.Logging.IndentManager.IncreaseIndent()) {
        Cleanup();
        using (new Measurement("Method              ", MeasurementOptions.Log, iterations))
          CallClassMethod<T>(c, iterations);
        // Cleanup();
        using (new Measurement("Method (by delegate)", MeasurementOptions.Log, iterations))
          CallAction(a, iterations);
        // Cleanup();
        using (new Measurement("Method cast         ", MeasurementOptions.Log, iterations))
          CastClassMethod<T>(c, iterations);
        Cleanup();
      }
    }

    private void GMethod1CallTest<T,T1>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      Container<T> c = new Container<T>(o);
      Action a = c.Method1<T1>;
      // Warmup
      int iterations = 100;
      CallClassGMethod1<T,T1>(c, iterations);
      a.Invoke();
      CastClassGMethod1<T,T1>(c, iterations);
      // Real test
      iterations = (int)(IterationCount*speedFactor);
      TestLog.Info("Regular call test (1 generic argument: {0}):", typeof(T1).GetShortName());
      TestLog.Info("  Type: {0}", typeof(T).GetShortName());
      FastCache<T>.Value = null;
      using (IndentManager.IncreaseIndent()) {
        Cleanup();
        using (new Measurement("Method 1              ", MeasurementOptions.Log, iterations))
          CallClassGMethod1<T,T1>(c, iterations);
        // Cleanup();
        using (new Measurement("Method 1 (by delegate)", MeasurementOptions.Log, iterations))
          CallAction(a, iterations);
        // Cleanup();
        using (new Measurement("Method 1 cast         ", MeasurementOptions.Log, iterations))
          CastClassGMethod1<T,T1>(c, iterations);
        Cleanup();
      }
    }

    private void GMethod2CallTest<T,T1,T2>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      Container<T> c = new Container<T>(o);
      Action a = c.Method2<T1,T2>;
      // Warmup
      int iterations = 100;
      CallClassGMethod2<T,T1,T2>(c, iterations);
      a.Invoke();
      CastClassGMethod2<T,T1,T2>(c, iterations);
      // Real test
      iterations = (int)(IterationCount*speedFactor);
      TestLog.Info("Regular call test (2 generic arguments: {0}, {1}):", typeof(T1).GetShortName(), typeof(T2).GetShortName());
      TestLog.Info("  Type: {0}", typeof(T).GetShortName());
      FastCache<T>.Value = null;
      using (IndentManager.IncreaseIndent()) {
        Cleanup();
        using (new Measurement("Method 2              ", MeasurementOptions.Log, iterations))
          CallClassGMethod2<T,T1,T2>(c, iterations);
        // Cleanup();
        using (new Measurement("Method 2 (by delegate)", MeasurementOptions.Log, iterations))
          CallAction(a, iterations);
        // Cleanup();
        using (new Measurement("Method 2 cast         ", MeasurementOptions.Log, iterations))
          CastClassGMethod2<T,T1,T2>(c, iterations);
        Cleanup();
      }
    }

    private void VMethodCallTest<T>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      ContainerBase<T> c = new Container<T>(o);
      IContainer<T> ic = (IContainer<T>)c;
      // Warmup
      int iterations = 100;
      FastCache<T>.Value = null;
      CallClassVMethod(c, iterations);
      CallClassVMethod_WithBoxing(c, iterations);
      CallInterfaceVMethod(ic, iterations);
      CallInterfaceVMethod_WithoutCaching(ic, iterations);
      CallInterfaceVMethod_WithCast(c, iterations);
      // Real test
      iterations = (int)(IterationCount*speedFactor);
      TestLog.Info("Virtual call test:");
      TestLog.Info("  Type: {0}", typeof(T).GetShortName());
      FastCache<T>.Value = null;
      using (IndentManager.IncreaseIndent()) {
        Cleanup();
        using (new Measurement("Virtual method (typed)          ", MeasurementOptions.Log, iterations))
          CallClassVMethod(c, iterations);
        // Cleanup();
        using (new Measurement("Virtual method (with boxing)    ", MeasurementOptions.Log, iterations))
          CallClassVMethod_WithBoxing(c, iterations);
        Cleanup();
        using (new Measurement("Virtual method cast             ", MeasurementOptions.Log, iterations))
          CastClassVMethod(c, iterations);
        Cleanup();
        using (new Measurement("Interface method                ", MeasurementOptions.Log, iterations))
          CallInterfaceVMethod(ic, iterations);
        Cleanup();
        using (new Measurement("Interface method (worst case)   ", MeasurementOptions.Log, iterations))
          CallInterfaceVMethod_WithoutCaching(ic, iterations);
        Cleanup();
        using (new Measurement("Interface method (with cast)    ", MeasurementOptions.Log, iterations))
          CallInterfaceVMethod_WithCast(c, iterations);
        Cleanup();
        using (new Measurement("Interface method cast           ", MeasurementOptions.Log, iterations))
          CastInterfaceVMethod(ic, iterations);
        Cleanup();
      }
    }

    private void GVMethod1CallTest<T,T1>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      ContainerBase<T> c = new Container<T>(o);
      IContainer<T> ic = (IContainer<T>)c;
      Action a1 = c.GenericMethod1<T1>;
      Action a2 = ic.GenericMethod1<T1>;
      // Warmup
      int iterations = 100;
      CallClassGVMethod1<T,T1>(c, iterations);
      CallInterfaceGVMethod1<T,T1>(ic, iterations);
      a1.Invoke();
      a2.Invoke();
      CastClassGVMethod1<T,T1>(c, iterations);
      CastInterfaceGVMethod1<T,T1>(ic, iterations);
      // Real test
      iterations = (int)(IterationCount*speedFactor);
      TestLog.Info("Virtual generic call test (1 generic argument: {0}):", typeof(T1).GetShortName());
      TestLog.Info("  Type: {0}", typeof(T).GetShortName());
      FastCache<T>.Value = null;
      using (IndentManager.IncreaseIndent()) {
        Cleanup();
        using (new Measurement("Generic method 1 (class)                 ", MeasurementOptions.Log, iterations))
          CallClassGVMethod1<T,T1>(c, iterations);
        // Cleanup();
        using (new Measurement("Generic method 1 (class, by delegate)    ", MeasurementOptions.Log, iterations))
          CallAction(a1, iterations);
        // Cleanup();
        using (new Measurement("Generic method 1 cast (class)            ", MeasurementOptions.Log, iterations))
          CastClassGVMethod1<T,T1>(c, iterations);
        Cleanup();
        using (new Measurement("Generic method 1 (interface)             ", MeasurementOptions.Log, iterations))
          CallInterfaceGVMethod1<T,T1>(ic, iterations);
        // Cleanup();
        using (new Measurement("Generic method 1 (interface, by delegate)", MeasurementOptions.Log, iterations))
          CallAction(a2, iterations);
        // Cleanup();
        using (new Measurement("Generic method 1 cast (interface)        ", MeasurementOptions.Log, iterations))
          CastInterfaceGVMethod1<T,T1>(ic, iterations);
        Cleanup();
      }
    }

    private void GVMethod2CallTest<T,T1,T2>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      ContainerBase<T> c = new Container<T>(o);
      IContainer<T> ic = (IContainer<T>)c;
      Action a1 = c.GenericMethod2<T1,T2>;
      Action a2 = ic.GenericMethod2<T1,T2>;
      // Warmup
      int iterations = 100;
      CallClassGVMethod2<T,T1,T2>(c, iterations);
      CallInterfaceGVMethod2<T,T1,T2>(ic, iterations);
      a1.Invoke();
      a2.Invoke();
      CastClassGVMethod2<T,T1,T2>(c, iterations);
      CastInterfaceGVMethod2<T,T1,T2>(ic, iterations);
      // Real test
      iterations = (int)(IterationCount*speedFactor);
      TestLog.Info("Virtual generic call test (2 generic arguments: {0}, {1}):", typeof(T1).GetShortName(), typeof(T2).GetShortName());
      TestLog.Info("  Type: {0}", typeof(T).GetShortName());
      FastCache<T>.Value = null;
      using (IndentManager.IncreaseIndent()) {
        Cleanup();
        using (new Measurement("Generic method 2 (class)                 ", MeasurementOptions.Log, iterations))
          CallClassGVMethod2<T,T1,T2>(c, iterations);
        // Cleanup();
        using (new Measurement("Generic method 2 (class, by delegate)    ", MeasurementOptions.Log, iterations))
          CallAction(a1, iterations);
        // Cleanup();
        using (new Measurement("Generic method 2 cast (class)            ", MeasurementOptions.Log, iterations))
          CastClassGVMethod2<T,T1,T2>(c, iterations);
        Cleanup();
        using (new Measurement("Generic method 2 (interface)             ", MeasurementOptions.Log, iterations))
          CallInterfaceGVMethod2<T,T1,T2>(ic, iterations);
        // Cleanup();
        using (new Measurement("Generic method 2 (interface, by delegate)", MeasurementOptions.Log, iterations))
          CallAction(a2, iterations);
        // Cleanup();
        using (new Measurement("Generic method 2 cast (interface)        ", MeasurementOptions.Log, iterations))
          CastInterfaceGVMethod2<T,T1,T2>(ic, iterations);
        Cleanup();
      }
    }

    private void CallAction(Action a, int count)
    {
      for (int i = 0; i<count; i+=10) {
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
        a.Invoke();
      }
    }

    private void CallClassMethod<T>(Container<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.Method();
        c.Method();
        c.Method();
        c.Method();
        c.Method();
        c.Method();
        c.Method();
        c.Method();
        c.Method();
        c.Method();
      }
    }

    private void CastClassMethod<T>(Container<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
        a = c.Method;
      }
    }

    private void CallClassGMethod1<T,T1>(Container<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
        c.Method1<T1>();
      }
    }

    private void CastClassGMethod1<T,T1>(Container<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
        a = c.Method1<T1>;
      }
    }

    private void CallClassGMethod2<T,T1,T2>(Container<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
        c.Method2<T1,T2>();
      }
    }

    private void CastClassGMethod2<T,T1,T2>(Container<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
        a = c.Method2<T1,T2>;
      }
    }

    private void CallClassVMethod<T>(ContainerBase<T> c, int count)
    {
      T o;
      for (int i = 0; i<count; i+=10) {
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
      }
    }

    private void CastClassVMethod<T>(ContainerBase<T> c, int count)
    {
      Func<T> f;
      for (int i = 0; i<count; i+=10) {
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
      }
    }

    private void CallClassVMethod_WithBoxing<T>(ContainerBase<T> c, int count)
    {
      T o;
      for (int i = 0; i<count; i+=10) {
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
        o = (T)c.BoxedValue;
      }
    }

    private void CallInterfaceVMethod<T>(IContainer<T> c, int count)
    {
      T o;
      for (int i = 0; i<count; i+=10) {
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
        o = c.Value;
      }
    }

    private void CallInterfaceVMethod_WithoutCaching<T>(IContainer<T> c, int count)
    {
      var caller = new Caller<T>(c);
      for (int i = 0; i<count; i+=10) {
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
        caller.GetContainerValue();
      }
    }

    protected virtual void GetCValue()
    {
      throw new NotImplementedException();
    }

    private void CastInterfaceVMethod<T>(IContainer<T> c, int count)
    {
      Func<T> f;
      for (int i = 0; i<count; i+=10) {
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
        f = c.GetValue;
      }
    }

    private void CallInterfaceVMethod_WithCast<T>(ContainerBase<T> c, int count)
    {
      IContainer<T> ic;
      T o;
      for (int i = 0; i<count; i+=10) {
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
        ic = (IContainer<T>)c;
        o = ic.Value;
      }
    }

    private void CallClassGVMethod1<T,T1>(ContainerBase<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
      }
    }

    private void CastClassGVMethod1<T,T1>(ContainerBase<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
      }
    }

    private void CallInterfaceGVMethod1<T,T1>(IContainer<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
        c.GenericMethod1<T1>();
      }
    }

    private void CastInterfaceGVMethod1<T,T1>(IContainer<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
        a = c.GenericMethod1<T1>;
      }
    }

    private void CallClassGVMethod2<T,T1,T2>(ContainerBase<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
      }
    }

    private void CastClassGVMethod2<T,T1,T2>(ContainerBase<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
        a = c.GenericMethod2<T1, T2>;
      }
    }

    private void CallInterfaceGVMethod2<T,T1,T2>(IContainer<T> c, int count)
    {
      for (int i = 0; i<count; i+=10) {
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
        c.GenericMethod2<T1,T2>();
      }
    }

    private void CastInterfaceGVMethod2<T,T1,T2>(IContainer<T> c, int count)
    {
      Action a;
      for (int i = 0; i<count; i+=10) {
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
        a = c.GenericMethod2<T1,T2>;
      }
    }

    private void Cleanup()
    {
      int baseSleepTime = 100;
      if (isRegularTestRunning)
        baseSleepTime = 1;

      for (int i = 0; i<5; i++) {
        GC.GetTotalMemory(true);
        Thread.Sleep(baseSleepTime);
      }
      Thread.Sleep(5*baseSleepTime);
    }
  }
}