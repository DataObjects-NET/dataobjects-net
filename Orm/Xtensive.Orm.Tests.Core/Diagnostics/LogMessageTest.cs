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
      TestLog.Info("Message for {0} log parameters.",param1);
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.Info(e, "Message for {0} log parameters.", param2);
      }

      TestLog.InfoRegion("Error logs");
      TestLog.Error("Message for {0} log parameters.", param1);
      try{
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e){
        TestLog.Error(e, "Message for {0} log parameters.", param2);
      }

      TestLog.DebugRegion("Debug logs");
      TestLog.Debug("Message for {0} log parameters.", param1);
      try {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.Debug(e, "Message for {0} log parameters.", param2);
      }

      TestLog.InfoRegion("Warning logs");
      TestLog.Warning("Message for {0} log parameters.", param1);
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.Warning(e, "Message for {0} log parameters.", param2);
      }

      TestLog.InfoRegion("Fatal error logs");
      TestLog.FatalError("Message for {0} log parameters.", param1);
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        TestLog.FatalError(e, "Message for {0} log parameters.", param2);
      }

    }
  }
}