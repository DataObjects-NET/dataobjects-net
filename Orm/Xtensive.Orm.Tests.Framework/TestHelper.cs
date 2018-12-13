// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Threading;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Test helper class.
  /// </summary>
  public static class TestHelper
  {
    /// <summary>
    /// Ensures full garbage collection.
    /// </summary>
    public static void CollectGarbage()
    {
      CollectGarbage(TestInfo.IsPerformanceTestRunning);
    }

    /// <summary>
    /// Ensures full garbage collection.
    /// </summary>
    /// <param name="preferFullRatherThanFast">Full rather then fast collection should be performed.</param>
    public static void CollectGarbage(bool preferFullRatherThanFast)
    {
      int baseSleepTime = 1;
      if (preferFullRatherThanFast)
        baseSleepTime = 100;

      for (int i = 0; i<5; i++) {
        Thread.Sleep(baseSleepTime);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        GC.WaitForPendingFinalizers();
      }
    }

    public static System.Configuration.Configuration GetConfigurationForAssembly(this Type typeFromAssembly)
    {
      return typeFromAssembly.Assembly.GetAssemblyConfiguration();
    }

    public static System.Configuration.Configuration GetConfigurationForAssembly(this object instanceOfTypeFromAssembly)
    {
      return instanceOfTypeFromAssembly.GetType().Assembly.GetAssemblyConfiguration();
    }
  }
}