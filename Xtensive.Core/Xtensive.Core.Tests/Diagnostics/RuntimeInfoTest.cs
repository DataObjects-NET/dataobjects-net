// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.03

using System;
using NUnit.Framework;
using Xtensive.Diagnostics;

namespace Xtensive.Tests.Diagnostics
{
  [TestFixture]
  public class RuntimeInfoTest
  {
    private sealed class Empty {
      public Empty Pointer1;
      public Empty Pointer2;
    }

    private static int GetDefaultStructLayoutPack()
    {
      int measureCount = 128;
      int maxPack = 128;
      int allocationToPackFactor = 4;
      Empty[] objects = new Empty[16384];
      int[] packCounts = new int[maxPack];
      int bestPack = 0;
      int bestPackCount = 0;
      for (int i = 0; i<measureCount; i++) {
//        for (int j = 0; j<objects.Length; j++)
//          objects[j] = null;
        long mem = GetFreeMemory();
        for (int j = 0; j<objects.Length; j++)
          objects[j] = new Empty();
        try {
          long memDiff = GetFreeMemory() - mem;
          int pack = (int)(memDiff / (allocationToPackFactor*objects.Length));
          if (pack<1 || pack>maxPack)
            continue;
          packCounts[pack]++;
          if (bestPackCount<packCounts[pack]) {
            bestPack = pack;
            bestPackCount = packCounts[pack];
          }
        }
        catch (ArithmeticException) {
        }
      }
      if (bestPack<1 || bestPack>maxPack)
        bestPack = 4;
      return bestPack;
    }

    private static long GetFreeMemory()
    {
//      GC.Collect();
//      GC.WaitForPendingFinalizers();
//      GC.Collect();
      return GC.GetTotalMemory(false);
    }

    [Test, Explicit]
    public void CompoundTest()
    {
      Log.Info("PointerSize:             {0}", RuntimeInfo.PointerSize);
      Log.Info("DefaultStructLayoutPack: {0}", RuntimeInfo.DefaultStructLayoutPack);
      Assert.AreEqual(RuntimeInfo.PointerSize, IntPtr.Size);
      for (int i = 0; i<100; i++)
        Assert.AreEqual(RuntimeInfo.DefaultStructLayoutPack, GetDefaultStructLayoutPack());
    }
  }
}