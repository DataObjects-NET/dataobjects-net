// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.28

using System;
using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Messaging.Tests
{
  public class MemoryLeakDetector
  {
    private Action test;
    private int iterations;

    public MemoryLeakDetector(Action test)
      : this(test, 100)
    {
    }

    public MemoryLeakDetector(Action test, int iterations)
    {
      ArgumentValidator.EnsureArgumentNotNull(test, "test");
      this.test = test;
      this.iterations = iterations;
    }

    public bool Test()
    {
      int errorCount = 0;
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
      long previousMemoryValue = 0;
      for (int i = 0; i<iterations; i++) {
        test();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        long memory = GC.GetTotalMemory(false);
        if (i>1 && memory!=previousMemoryValue)
          errorCount++;
        previousMemoryValue = memory;
        if (i%(iterations/10)==0)
          Console.WriteLine("{0}: {1}", i, memory);
      }
      Assert.IsTrue(errorCount/iterations<0.02); // Allows 2% of mistmatches
      return true;
    }
  }
}