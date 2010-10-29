// Copyright (C) a Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    17.06.2008

using System;
using System.Globalization;
using NUnit.Framework;

namespace Xtensive.Tests.Diagnostics
{
  [TestFixture]
  public class LogMessageTest
  {
    [Test]
    public void PerformanceTest()
    {
      string param1 = "'(string, object[])'";
      string param2 = "'(Exception, string, object[])'";

      Log.InfoRegion("Information logs");
      Log.Info("Message for {0} log parameters.",param1);
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        Log.Info(e, "Message for {0} log parameters.", param2);
      }

      Log.InfoRegion("Error logs");
      Log.Error("Message for {0} log parameters.", param1);
      try{
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e){
        Log.Error(e, "Message for {0} log parameters.", param2);
      }

      Log.DebugRegion("Debug logs");
      Log.Debug("Message for {0} log parameters.", param1);
      try {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        Log.Debug(e, "Message for {0} log parameters.", param2);
      }

      Log.InfoRegion("Warning logs");
      Log.Warning("Message for {0} log parameters.", param1);
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        Log.Warning(e, "Message for {0} log parameters.", param2);
      }

      Log.InfoRegion("Fatal error logs");
      Log.FatalError("Message for {0} log parameters.", param1);
      try
      {
        throw new InvalidOperationException("Exception text.");
      }
      catch (Exception e)
      {
        Log.FatalError(e, "Message for {0} log parameters.", param2);
      }

    }
  }
}