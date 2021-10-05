// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Logging;
using Xtensive.Reflection;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class ValidationTest
  {
    public const int IterationCount = 1000000;

    private class Holder
    {
      public virtual void ThrowNoException()
      {
        return;
      }

      public virtual void ThrowException()
      {
        throw new InvalidOperationException();
      }
    }

    [Test]
    [Explicit]
    [Category("Performance")]
    public void PeformanceTest()
    {
      Test(1);
    }

    [Test]
    public void RegularTest()
    {
      Test(0.01);
    }

    private void Test(double speedFactor)
    {
      RunTryCatchTest(speedFactor);
      RunNullCheckTest(speedFactor);
    }

    private void RunTryCatchTest(double speedFactor)
    {
      int count = (int)(IterationCount * speedFactor);
      // Warmup
      Holder h = new Holder();
      NoTryNoExceptionLoop(h, 10);
      TryNoExceptionLoop(h, 10);
      TryCatchExceptionLoop(h, 10);
      // Real test
      TestLog.Info("Try-catch test:");
      using (IndentManager.IncreaseIndent()) {
        TestHelper.CollectGarbage();
        using (new Measurement("NoTryNoException ", MeasurementOptions.Log, count))
          NoTryNoExceptionLoop(h, count);
        TestHelper.CollectGarbage();
        using (new Measurement("TryNoException   ", MeasurementOptions.Log, count))
          TryNoExceptionLoop(h, count);
        TestHelper.CollectGarbage();
        using (new Measurement("TryCatchException", MeasurementOptions.Log, count))
          TryCatchExceptionLoop(h, count);
        TestHelper.CollectGarbage();
      }
    }

    private void RunNullCheckTest(double speedFactor)
    {
      int count = (int)(IterationCount * speedFactor);
      // Warmup
      Holder[] holders = new Holder[10];
      FillHolders(holders);
      NoCheckLoop(holders);
      NullCheckLoop(holders);
      // Real test
      holders = new Holder[count];
      FillHolders(holders);
      TestLog.Info("Null check test:");
      using (IndentManager.IncreaseIndent()) {
        TestHelper.CollectGarbage();
        using (new Measurement("No check  ", MeasurementOptions.Log, count))
          NoCheckLoop(holders);
        TestHelper.CollectGarbage();
        using (new Measurement("Null check  ", MeasurementOptions.Log, count))
          NullCheckLoop(holders);
        TestHelper.CollectGarbage();
      }
    }

    private void NoTryNoExceptionLoop(Holder h, int count)
    {
      for (int i = 0; i<count; i++) {
        h.ThrowNoException();
      }
    }

    private void TryNoExceptionLoop(Holder h, int count)
    {
      for (int i = 0; i<count; i++) {
        try {
          h.ThrowNoException();
        }
        catch {
        }
      }
    }

    private void TryCatchExceptionLoop(Holder h, int count)
    {
      for (int i = 0; i<count; i++) {
        try {
          h.ThrowException();
        }
        catch (Exception) {
        }
      }
    }

    private void FillHolders(Holder[] holders)
    {
      Holder h = new Holder();
      for (int i = 0; i < holders.Length; i++)
        holders[i] = h;
    }

    private void NoCheckLoop(Holder[] holders)
    {
      for (int i = 0; i < holders.Length; i++) {
        Holder h = holders[i];
        h.ThrowNoException();
      }
    }

    private void NullCheckLoop(Holder[] holders)
    {
      for (int i = 0; i < holders.Length; i++) {
        Holder h = holders[i];
        if (h!=null)
          h.ThrowNoException();
      }
    }
  }
}