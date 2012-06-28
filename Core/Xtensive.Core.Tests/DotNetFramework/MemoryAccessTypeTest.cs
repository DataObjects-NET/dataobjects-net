// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.04.17

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Diagnostics;
using Xtensive.Testing;

namespace Xtensive.Tests.DotNetFramework
{
  [TestFixture]
  public class MemoryAccessTypeTest
  {
    public const int IterationCount = 10000000;
    public const int MbSize      = 1024*1024;
    public const int RamSizeMin  = MbSize / 4;
    public const int RamSizeMaxN = 16 * MbSize;
    public const int RamSizeMaxP = 256 * MbSize;
    public readonly static int ItemSize = IntPtr.Size * 2;

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
      // Warmup
      TestAccess(100, 100, OrderedSequence, true);
      TestAccess(100, 100, RandomSequence, true);
      // Actual tests
      Log.Info("Item size: {0} bytes", ItemSize);
      int ramSizeMax = TestInfo.IsPerformanceTestRunning ? RamSizeMaxP : RamSizeMaxN;
      using (Log.InfoRegion("Sequential access"))
        for (int ramSize = RamSizeMin; ramSize<=ramSizeMax; ramSize *= 2)
          TestAccess(ramSize / ItemSize, (int) (IterationCount * speedFactor), OrderedSequence, false);
      using (Log.InfoRegion("Random access"))
        for (int ramSize = RamSizeMin; ramSize<=ramSizeMax; ramSize *= 2)
          TestAccess(ramSize / ItemSize, (int) (IterationCount * speedFactor), RandomSequence, false);
    }

    private void TestAccess(int size, int count, Func<int, IEnumerable<int>> generator, bool warmup)
    {
      size = size / 10 * 10;
      Pair<int>[] pairs = new Pair<int>[size];
      // Filling the data
      IEnumerable<int> nexts = generator.Invoke(size);
      int i = 0;
      foreach (int next in nexts)
        pairs[i++] = new Pair<int>(i, next);
      // Test
      Pair<int> current = new Pair<int>();
      TestHelper.CollectGarbage();
      using (warmup ? (IDisposable)new Core.Disposable(delegate { }) : 
        new Measurement(
          string.Format("{0,6:F2} MB", (double)size * ItemSize / MbSize), 
          MeasurementOptions.Log, count)) {
        for (i = 0; i < count; i+=10) {
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
          current = pairs[current.Second];
        }
      }
      pairs = null;
      TestHelper.CollectGarbage();
    }

    private IEnumerable<int> OrderedSequence(int count)
    {
      for (int i = 0; i<count; i++)
        yield return i;
    }

    private IEnumerable<int> RandomSequence(int count)
    {
      int[] ints = new int[count];
      // Filling
      for (int i = 0; i<count; i++)
        ints[i] = i;
      // Mixing
      Random r = RandomManager.CreateRandom(count, SeedVariatorType.None);
      for (int i = 0; i<count; i++) {
        int j = r.Next(count);
        int t = ints[i];
        ints[i] = ints[j];
        ints[j] = t;
      }
      // Returning
      for (int i = 0; i<count; i++)
        yield return ints[i];
    }
  }
}