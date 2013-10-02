// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.11

using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests.Core.Diagnostics
{
  [TestFixture]
  public class LogPerformanceTest
  {
    private const int BaseCount = 50000;

    [Test]
    public void NullLogTest()
    {
      int count = BaseCount;
      if (DebugInfo.IsRunningOnBuildServer)
        count /= 100;
      ILog log = LogProvider.NullLog;
      using (new Measurement("Null log test", count)) {
        for (int i = 0; i<count; i++)
          log.Info("{0}", i);
      }
    }

    [Test]
    public void ConsoleLogTest()
    {
      int count = BaseCount / 100;
      if (DebugInfo.IsRunningOnBuildServer)
        count /= 100;
      ILog log = LogProvider.ConsoleLog;
      using (new Measurement("Console log test", count)) {
        for (int i = 0; i<count; i++)
          log.Info("{0}", i);
      }
    }

    [Test]
    public void LogCaptureAndIndentTest()
    {
      int count = BaseCount;
      if (DebugInfo.IsRunningOnBuildServer)
        count /= 100;
      ILog log = LogProvider.NullLog;
      using (new Measurement("Log capture and indent test", count)) {
        using (new LogIndentScope())
        using (new LogCaptureScope(LogProvider.NullLog)) {
          for (int i = 0; i<count; i++)
            log.Info("{0}", i);
        }
      }
    }
  }
}