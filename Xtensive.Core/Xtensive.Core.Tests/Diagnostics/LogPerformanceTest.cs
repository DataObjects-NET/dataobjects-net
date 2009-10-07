// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.IoC;

namespace Xtensive.Core.Tests.Diagnostics
{
  [TestFixture]
  public class LogPerformanceTest
  {
    private const int BaseCount = 50000;

    [Test]
    public void NullLogTest()
    {
      var lp = ServiceLocator.GetInstance<ILogProvider>();

      int count = BaseCount;
      if (DebugInfo.IsRunningOnBuildServer)
        count /= 100;
      ILog log = lp.NullLog;
      using (new Measurement("Null log test", count)) {
        for (int i = 0; i<count; i++)
          log.Info("{0}", i);
      }
    }

    [Test]
    public void ConsoleLogTest()
    {
      var lp = ServiceLocator.GetInstance<ILogProvider>();
      int count = BaseCount / 100;
      if (DebugInfo.IsRunningOnBuildServer)
        count /= 100;
      ILog log = lp.ConsoleLog;
      using (new Measurement("Console log test", count)) {
        for (int i = 0; i<count; i++)
          log.Info("{0}", i);
      }
    }

    [Test]
    public void LogCaptureAndIndentTest()
    {
      var lp = ServiceLocator.GetInstance<ILogProvider>();
      
      int count = BaseCount;
      if (DebugInfo.IsRunningOnBuildServer)
        count /= 100;
      ILog log = lp.NullLog;
      using (new Measurement("Log capture and indent test", count)) {
        using (new LogIndentScope())
        using (new LogCaptureScope(lp.NullLog)) {
          for (int i = 0; i<count; i++)
            log.Info("{0}", i);
        }
      }
    }
  }
}