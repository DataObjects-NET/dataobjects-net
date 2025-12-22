// Copyright (C) a Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    17.06.2008

using System;
using System.Globalization;
using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.Diagnostics
{
  [TestFixture]
  public class LogMessageTest
  {
    [Test]
    public void PerformanceTest()
    {
      string param1 = "'(string, object[])'";
      string param2 = "'(Exception, string, object[])'";

      TestLog.InfoRegion("Information logs");
      TestLog.Info($"Message for {param1} log parameters.");
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.Info($"Message for {param2} log parameters.");
      }

      TestLog.InfoRegion("Error logs");
      TestLog.Error($"Message for {param1} log parameters.");
      try{
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e){
        TestLog.Error($"Message for {param2} log parameters.");
      }

      TestLog.DebugRegion("Debug logs");
      TestLog.Debug($"Message for {param1} log parameters.");
      try {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.Debug($"Message for {param2} log parameters.");
      }

      TestLog.InfoRegion("Warning logs");
      TestLog.Warning($"Message for {param1} log parameters.");
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.Warning($"Message for {param2} log parameters.");
      }

      TestLog.InfoRegion("Fatal error logs");
      TestLog.FatalError($"Message for {param1} log parameters.");
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.FatalError($"Message for {param2} log parameters.");
      }

    }
  }
}