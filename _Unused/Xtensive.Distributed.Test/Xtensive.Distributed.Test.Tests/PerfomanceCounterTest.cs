// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.12

using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Distributed.Test.Tests
{
  [TestFixture]
  public class PerfomanceCounterTest
  {
    [Test]
    [Explicit]
    public void Test()
    {
      PerformanceCounter processorCounter = new PerformanceCounter("processor", "% Processor Time", "_total", true);
      PerformanceCounter memoryCounter = new PerformanceCounter("memory", "Available MBytes", "", true);
      for (int i = 0; i < 30; i++) {
        Console.WriteLine("{0} {1}Mb", processorCounter.NextValue(), memoryCounter.NextValue());
        Thread.Sleep(1000);
      }
    }
  }
}