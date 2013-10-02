// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.04.17

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xtensive.Core;

namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Provides various runtime related information.
  /// </summary>
  public static class RuntimeInfo
  {
    private sealed class TwoPointers {
      public TwoPointers Pointer1;
      public TwoPointers Pointer2;
    }

    private static int pointerSize;
    private static int defaultStructLayoutPack;
    private static int minimalObjectSize;

    /// <summary>
    /// Gets the size of the pointer (reference field) in bytes.
    /// </summary>
    public static int PointerSize {
      [DebuggerStepThrough]
      get {
        return pointerSize;
      }
    }

    /// <summary>
    /// Gets default struct or class field alignment in bytes.
    /// See <see cref="StructLayoutAttribute.Pack">StructLayoutAttribute.Pack</see> for further information.
    /// </summary>
    public static int DefaultStructLayoutPack {
      [DebuggerStepThrough]
      get {
        return defaultStructLayoutPack;
      }
    }

    /// <summary>
    /// Gets the minimal size of any object in bytes.
    /// </summary>
    public static int MinimalObjectSize {
      [DebuggerStepThrough]
      get {
        return minimalObjectSize;
      }
    }

    private static int GetDefaultStructLayoutPack()
    {
      int measureCount = 128;
      int maxPack = 128;
      int allocationToPackFactor = 4;
      TwoPointers[] objects = new TwoPointers[16384];
      int[] packCounts = new int[maxPack];
      int bestPack = 0;
      int bestPackCount = 0;
      for (int i = 0; i<measureCount; i++) {
//        for (int j = 0; j<objects.Length; j++)
//          objects[j] = null;
        long mem = GetFreeMemory();
        for (int j = 0; j<objects.Length; j++)
          objects[j] = new TwoPointers();
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
      if (bestPack<1 || bestPack>maxPack) {
        CoreLog.Warning("Suspicious RuntimeInfo.DefaultStructLayoutPack value is detected: {0}", bestPack);
        bestPack = 4;
      }
      return bestPack;
    }

    private static long GetFreeMemory()
    {
//      GC.Collect();
//      GC.WaitForPendingFinalizers();
//      GC.Collect();
      return GC.GetTotalMemory(false);
    }


    // Type initializer

    static RuntimeInfo()
    {
      pointerSize = IntPtr.Size;
      minimalObjectSize = pointerSize * 3;
      defaultStructLayoutPack = GetDefaultStructLayoutPack();
    }
  }
}