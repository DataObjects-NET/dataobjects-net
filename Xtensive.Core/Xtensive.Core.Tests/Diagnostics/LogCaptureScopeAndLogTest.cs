// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.04

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.IoC;

namespace Xtensive.Core.Tests.Diagnostics
{
  [TestFixture]
  public class LogCaptureScopeAndLogTest
  {
    [Test]
    public void LogSourcesTest()
    {
      var lp = ServiceLocator.GetInstance<ILogProvider>();

      if (DebugInfo.IsRunningOnBuildServer)
        return; // Can't use Console.SetOut(...) there
      Log.Info("Starting...");
      StringBuilder output = new StringBuilder();
      TextWriter oldOutput = Console.Out;
      Console.SetOut(new StringWriter(output));
      string marker = "#Marker#";
      try {
        // Test 1
        Log.Info("Writing to console: " + marker);
        string outputString = output.ToString();
        oldOutput.WriteLine("Output:\r\n{0}\r\nEnd.", outputString);
        int i = 0;
        Assert.IsTrue((i = outputString.IndexOf(marker,i))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))<0);
       
        // Test 2
        output.Length = 0;
        using (new LogCaptureScope(lp.ConsoleLog)) {
          using (new LogCaptureScope(lp.NullLog)) {
            using (new LogCaptureScope(lp.ConsoleLog)) {
              Log.Info("Writing to console: " + marker);
            }
          }
        }
        outputString = output.ToString();
        oldOutput.WriteLine("Output:\r\n{0}\r\nEnd.", outputString);
        i = 0;
        Assert.IsTrue((i = outputString.IndexOf(marker,i))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))<0);
       
        // Test 3
        output.Length = 0;
        using (new LogCaptureScope(lp.NullLog)) {
          Log.Info("Writing to null: "+marker);
        }
        outputString = output.ToString();
        oldOutput.WriteLine("Output:\r\n{0}\r\nEnd.", outputString);
        i = 0;
        Assert.IsTrue((i = outputString.IndexOf(marker,i))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))<0);

        // Test 4
        output.Length = 0;
        using (new LogCaptureScope(lp.ConsoleLog)) {
          Log.Info("Writing to console: "+marker);
        }
        outputString = output.ToString();
        oldOutput.WriteLine("Output:\r\n{0}\r\nEnd.", outputString);
        i = 0;
        Assert.IsTrue((i = outputString.IndexOf(marker,i))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))<0);
      }
      finally {
        Console.SetOut(oldOutput);
      }
      Log.Info("Completed.");
    }
  }
}