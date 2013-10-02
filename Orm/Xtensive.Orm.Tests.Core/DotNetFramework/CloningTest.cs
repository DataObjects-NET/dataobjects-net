// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  public class Cloneable<T>
  {
    public T Value;

    public Cloneable<T> CloneByMC()
    {
      return (Cloneable<T>) MemberwiseClone();
    }

    public Cloneable<T> CloneByC()
    {
      return new Cloneable<T>(this);
    }

    public Cloneable(T value)
    {
      Value = value;
    }

    public Cloneable(Cloneable<T> source)
    {
      Value = source.Value;
    }
  }


  [TestFixture]
  public class CloningTest
  {
    private bool isRegularTestRunning;
    public const int CloneTestArrayLength = 1000000;

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      isRegularTestRunning = false;
      Test(1);
    }

    [Test]
    public void RegularTest()
    {
      isRegularTestRunning = true;
      Test(0.01);
    }

    public void Test(double speedFactor)
    {
      CloneTest<bool>(1*speedFactor);
      CloneTest<int>(1*speedFactor);
      CloneTest<long>(1*speedFactor);
      CloneTest<double>(1*speedFactor);
      CloneTest<Guid>(0.5*speedFactor);
      CloneTest<Pair<Guid, Guid>>(0.2*speedFactor);
      CloneTest<Pair<Pair<Guid, Guid>, Pair<Guid, Guid>>>(0.1*speedFactor);
      CloneTest<string>(0.3*speedFactor);
    }

    private void CloneTest<T>(double speedFactor)
    {
      Random r = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      T o = InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstance(r);
      Cloneable<T> c = new Cloneable<T>(o);
      // Warmup
      Cloneable<T>[] a = new Cloneable<T>[100];
      CloneByMCLoop(c, a);
      CloneByCLoop(c, a);
      // Real test
      a = new Cloneable<T>[(int)(CloneTestArrayLength*speedFactor / 10 * 10)];
      TestLog.Info("Cloning test:");
      TestLog.Info("  Type: {0}, length: {1}", c.GetType().GetShortName(), a.Length);
      using (new LogIndentScope()) {
        Cleanup();
        using (new Measurement("MemberwiseClone   ", MeasurementOptions.Log, a.Length))
          CloneByMCLoop(c, a);
        Cleanup();
        using (new Measurement("CopyingConstructor", MeasurementOptions.Log, a.Length))
          CloneByCLoop(c, a);
        Cleanup();
      }
    }

    private void CloneByMCLoop<T>(Cloneable<T> c, Cloneable<T>[] a)
    {
      int count = a.Length;
      for (int i = 0; i<count; ) {
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
        a[i++] = c.CloneByMC();
      }
    }

    private void CloneByCLoop<T>(Cloneable<T> c, Cloneable<T>[] a)
    {
      int count = a.Length;
      for (int i = 0; i<count; ) {
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
        a[i++] = c.CloneByC();
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