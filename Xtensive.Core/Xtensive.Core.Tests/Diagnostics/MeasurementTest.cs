// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core;

namespace Xtensive.Core.Tests.Diagnostics
{
  [TestFixture]
  public class MeasurementTest
  {
    [Test]
    [Ignore("TODO: AY check it ASAP")]
    public void CompoundTest()
    {
      // This part ensures everything below is JITter
      using (Measurement m = new Measurement()) {
        Log.Info("Ignore: TimeSpent = " + m.TimeSpent.TotalMilliseconds.ToString());
        Assert.AreEqual(m.Name, "Unnamed");
        m.OperationCount = 10;
        Assert.AreEqual(m.OperationCount,10);
        Log.Info("Ignore: MemoryAllocated = " + m.MemoryAllocated.ToString());
        Assert.IsTrue(m.MemoryAllocated < 2000000);
        Log.Info("Before array allocation: "+m.ToString());
        object[] a = new object[1000];
        Thread.Sleep(10);
        Log.Info("After  array allocation: "+m.ToString());
        a = null;
      }

      // Actual test
      {
        Measurement m = new Measurement("Named");
        Assert.AreEqual(m.Name, "Named");
        m.OperationCount = 10;
        Assert.AreEqual(m.OperationCount, 10);
        m.OperationCount = 0;
        Assert.IsTrue(m.TimeSpent.TotalMilliseconds < 5);
        Log.Info("Before array allocation: "+m.ToString());
        long before = m.MemoryAllocated;
        byte[] a = new byte[1000000];
        a[0] = 1;
        long diff = m.MemoryAllocated-before;
        Log.Info("Difference: " + diff);
        Assert.IsTrue(diff>900000);
        Assert.IsTrue(diff<1100000);
        Log.Info("After array allocation: "+m.ToString());
        Thread.Sleep(150);
        a = null;
        m.DisposeSafely();
        try {
          m.Complete();
          Assert.Fail("InvalidOperationException expected.");
        }
        catch (InvalidOperationException) {
        }
        Assert.IsTrue(m.TimeSpent.TotalMilliseconds > 100);
        Assert.IsTrue(m.TimeSpent.TotalMilliseconds < 200);
        Assert.IsTrue(m.MemoryAllocated < 1000);
      }
    }
  }
}