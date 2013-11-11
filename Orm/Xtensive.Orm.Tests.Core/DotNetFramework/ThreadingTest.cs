// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.27

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class ThreadingTest
  {
    public const int IterationCount = 1000000;
    public static bool warmup = true;
    public static Thread[] threads;

    private class Target
    {
      public int ThreadCount;
      public int PassCount;
      public bool Stop;
      public Thread LastAccessor;
      public object ObjectLock = new object();
      public ReaderWriterLockSlim SlimLock = new ReaderWriterLockSlim();

      public void ExecuteLock(object argument)
      {
        using (IndentManager.IncreaseIndent()) {
          bool log = !warmup && (int) argument==0;
          int lockCount = 0;
          int switchCount = 0;
          int passCount = PassCount;
          Thread thread = Thread.CurrentThread;
          using (new Measurement("Regular lock", log ? MeasurementOptions.Log : 0, passCount))
            for (int j = 0; j < passCount; j++) {
              lock (ObjectLock) {
                lockCount++;
                if (LastAccessor!=thread) {
                  LastAccessor = thread;
                  switchCount++;
                }
              }
            }
          if (log)
            TestLog.Info("  Switch rate: {0} ({1:F3}%)", switchCount, switchCount * 1.0 / lockCount);
        }
      }

      public void ExecuteReadLock(object argument)
      {
        using (IndentManager.IncreaseIndent()) {
          bool log = !warmup && (int) argument==0;
          int lockCount = 0;
          int switchCount = 0;
          int passCount = PassCount;
          Thread thread = Thread.CurrentThread;
          using (new Measurement("Read lock   ", log ? MeasurementOptions.Log : 0, passCount))
            for (int j = 0; j < passCount; j++) {
              SlimLock.EnterReadLock();
              try {
                lockCount++;
                if (LastAccessor!=thread) {
                  LastAccessor = thread;
                  switchCount++;
                }
              }
              finally {
                SlimLock.ExitReadLock();
              }
            }
          if (log)
            TestLog.Info("  Switch rate: {0} ({1:F3}%)", switchCount, switchCount * 1.0 / lockCount);
        }
      }

      public void ExecuteWriteLock(object argument)
      {
        using (IndentManager.IncreaseIndent()) {
          bool log = !warmup && (int) argument==0;
          int lockCount = 0;
          int switchCount = 0;
          int passCount = PassCount;
          Thread thread = Thread.CurrentThread;
          using (new Measurement("Write lock  ", log ? MeasurementOptions.Log : 0, passCount))
            for (int j = 0; j < passCount; j++) {
              SlimLock.EnterWriteLock();
              try {
                lockCount++;
                if (LastAccessor!=thread) {
                  LastAccessor = thread;
                  switchCount++;
                }
              }
              finally {
                SlimLock.ExitWriteLock();
              }
            }
          if (log)
            TestLog.Info("  Switch rate: {0} ({1:F3}%)", switchCount, switchCount * 1.0 / lockCount);
        }
      }

      public void ExecuteWaitLock(object argument)
      {
        using (IndentManager.IncreaseIndent()) {
          bool log = !warmup && (int) argument==0;
          int passCount = PassCount;
          Thread thread = Thread.CurrentThread;
          using (new Measurement("Wait lock   ", log ? MeasurementOptions.Log : 0, passCount))
            for (int j = 0; j < passCount; j++) {
              Wait:
              while (LastAccessor==thread) {
                if (Stop)
                  return;
              }
              lock (ObjectLock) {
                if (LastAccessor==thread)
                  goto Wait;
                LastAccessor = thread;
              }
            }
        }
      }

      public void ExecuteSleepLock(object argument)
      {
        using (IndentManager.IncreaseIndent()) {
          bool log = !warmup && (int) argument==0;
          int passCount = PassCount;
          Thread thread = Thread.CurrentThread;
          using (new Measurement("Sleep lock  ", log ? MeasurementOptions.Log : 0, passCount))
            for (int j = 0; j < passCount; j++) {
              Wait:
              while (LastAccessor==thread) {
                if (Stop)
                  return;
              }
              lock (ObjectLock) {
                if (LastAccessor==thread)
                  goto Wait;
                LastAccessor = thread;
              }
              Thread.Sleep(0);
            }
        }
      }

      public void ExecuteInvokeAsync(object argument)
      {
        Action d = delegate {
          return;
        };
        using (IndentManager.IncreaseIndent()) {
          bool log = !warmup && (int) argument==0;
          int passCount = PassCount;
          Thread thread = Thread.CurrentThread;
          using (new Measurement("Execute", log ? MeasurementOptions.Log : 0, passCount))
            for (int j = 0; j < passCount; j++) {
              IAsyncResult r = d.BeginInvoke(null, null);
              d.EndInvoke(r);
            }
        }
      }
    }

    [Test]
    public void RegularTest()
    {
      Test(0.00001);
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PerformanceTest()
    {
      Test(1);
    }


    private void Test(double speedFactor)
    {
      for (int i = 0; i<2; warmup = (++i)==0) {
        double sf = warmup ? speedFactor / 100 : speedFactor;
        LockTest(sf, 1);
        if (!warmup) {
          LockTest(sf, 2);
          LockTest(sf, 4);
          LockTest(sf, 8);
          LockTest(sf, 16);
          LockTest(sf, 256);
          LockTest(sf, 1024);
        }
        InvokeAsyncTest(sf, 1);
        if (!warmup) {
          InvokeAsyncTest(sf, 2);
          InvokeAsyncTest(sf, 4);
          InvokeAsyncTest(sf, 8);
        }
      }
    }

    private void LockTest(double speedFactor, int threadCount)
    {
      threads = new Thread[threadCount];
      Target target = new Target();
      target.ThreadCount = threadCount;
      int passCountBase = (int)(IterationCount * speedFactor);
      if (threadCount>100)
        passCountBase /= 10;

      TestHelper.CollectGarbage();
      using (warmup ? null : TestLog.InfoRegion(string.Format("{0} threads", threadCount))) {
        ThreadedTest(target, passCountBase,     target.ExecuteLock);
        ThreadedTest(target, passCountBase,     target.ExecuteReadLock);
        ThreadedTest(target, passCountBase,     target.ExecuteWriteLock);
        if (threadCount>1) {
          ThreadedTest(target, passCountBase / 200, target.ExecuteWaitLock);
          ThreadedTest(target, passCountBase / 400, target.ExecuteSleepLock);
        }
      }
    }

    private void InvokeAsyncTest(double speedFactor, int threadCount)
    {
      threads = new Thread[threadCount];
      Target target = new Target();
      target.ThreadCount = threadCount;
      int passCountBase = (int)(IterationCount * speedFactor);
      if (threadCount>100)
        passCountBase /= 10;

      TestHelper.CollectGarbage();
      using (warmup ? null : TestLog.InfoRegion(string.Format("{0} threads", threadCount))) {
        ThreadedTest(target, passCountBase/100, target.ExecuteInvokeAsync);
      }
    }

    private static void ThreadedTest(Target target, int passCount, ParameterizedThreadStart method)
    {
      int threadCount = threads.Length;
      target.ThreadCount = threadCount;
      target.PassCount = passCount;
      target.Stop = false;
      for (int i = 0; i < threadCount; i++) {
        Thread t = new Thread(method);
        threads[i] = t;
      }
      for (int i = 0; i < threadCount; i++)
        threads[i].Start(i);
      for (int i = 0; i < threadCount; i++) {
        threads[i].Join();
        target.Stop = true;
      }
      TestHelper.CollectGarbage();
    }
  }
}