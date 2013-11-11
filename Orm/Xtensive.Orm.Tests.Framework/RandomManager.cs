// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.09

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Creates random-seeded <see cref="Random"/>
  /// generators when running tests on build server;
  /// otherwise it creates the generator
  /// </summary>
  public static class RandomManager
  {
    private static bool isInitialized;
    private static int  globalSeedFactor = 0;

    public static Random CreateRandom()
    {
      return CreateRandom(0, SeedVariatorType.None);
    }

    public static Random CreateRandom(int seedFactor)
    {
      return CreateRandom(seedFactor, SeedVariatorType.None);
    }

    public static Random CreateRandom(SeedVariatorType seedVariatorType)
    {
      return CreateRandom(0, seedVariatorType);
    }

    public static Random CreateRandom(int seedFactor, SeedVariatorType seedVariatorType)
    {
      EnsureInitialized();
      MethodBase caller = null;
      int seed = unchecked (seedFactor + globalSeedFactor + GetSeedVariatorFactor(seedVariatorType, ref caller));
      if (DebugInfo.IsRunningOnBuildServer) {
        TestLog.Info("RandomManager: Created Random has seed {0}.", seed);
#if DEBUG
        if (caller==null)
          caller = GetCallingMethod();
        TestLog.Info("RandomManager: Caller: {0}.{1}.", caller.DeclaringType.Name, caller.Name);
#endif
      }
      return new Random(seed);
    }

    private static int GetSeedVariatorFactor(SeedVariatorType seedVariatorType, ref MethodBase caller)
    {
      if (seedVariatorType==SeedVariatorType.None)
        return 0;
      int seedFactor = 0;
      if ((seedVariatorType & SeedVariatorType.CallingAssembly)!=0)
        seedFactor += Path.GetFileName(Assembly.GetCallingAssembly().Location).GetHashCode();
      caller = null;
      if ((seedVariatorType & SeedVariatorType.CallingMethod)!=0) {
        if (caller==null)
          caller = GetCallingMethod();
        seedFactor += caller.Name.GetHashCode();
      }
      if ((seedVariatorType & SeedVariatorType.CallingMethod)!=0 ||
          (seedVariatorType & SeedVariatorType.CallingType)!=0) {
        if (caller==null)
          caller = GetCallingMethod();
        seedFactor += caller.DeclaringType.FullName.GetHashCode();
      }
      if ((seedVariatorType & SeedVariatorType.Day)!=0)
        seedFactor += DateTime.Today.DayOfYear;
      return seedFactor;
    }

    private static MethodBase GetCallingMethod()
    {
      StackTrace stackTrace = new StackTrace();
      for (int i = 1;; i++) {
        MethodBase caller = stackTrace.GetFrame(i).GetMethod();
        if (caller.DeclaringType!=typeof (RandomManager))
          return caller;
      }
    }

    private static void EnsureInitialized()
    {
      if (isInitialized)
        return;
      if (!DebugInfo.IsUnitTestSessionRunning || DebugInfo.IsRunningOnBuildServer)
        globalSeedFactor = unchecked ((int)DateTime.Now.Ticks);
      isInitialized = true;
    }
  }
}