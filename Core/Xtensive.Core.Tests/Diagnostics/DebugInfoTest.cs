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
  public class DebugInfoTest
  {
    [Test]
    public void CompoundTest()
    {
      Log.Info("Is running on build server:   {0}", DebugInfo.IsRunningOnBuildServer);
      Log.Info("Is unit test session running: {0}", DebugInfo.IsUnitTestSessionRunning);
      Assert.IsTrue(DebugInfo.IsUnitTestSessionRunning);
      int count = 1000000;
      bool b = true;
      using (new Measurement("DebugInfo.IsUnitTestSessionRunning", count)) {
        for (int i = 0; i<count; i++)
          b &= DebugInfo.IsUnitTestSessionRunning;
      }
      Assert.IsTrue(b);
    }
  }
}