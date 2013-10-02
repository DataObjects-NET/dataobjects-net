// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.04

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Diagnostics;

namespace Xtensive.Orm.Tests.Core.Diagnostics
{
  [TestFixture]
  public class LogCaptureScopeAndLogTest
  {
    [Test]
    public void LogSourcesTest()
    {
      if (DebugInfo.IsRunningOnBuildServer)
        return; // Can't use Console.SetOut(...) there
      TestLog.Info("Starting...");
      StringBuilder output = new StringBuilder();
      TextWriter oldOutput = Console.Out;
      Console.SetOut(new StringWriter(output));
      string marker = "#Marker#";
      try {
        // Test 1
        TestLog.Info("Writing to console: " + marker);
        string outputString = output.ToString();
        oldOutput.WriteLine("Output:\r\n{0}\r\nEnd.", outputString);
        int i = 0;
        Assert.IsTrue((i = outputString.IndexOf(marker,i))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))<0);
       
        // Test 2
        output.Length = 0;
        using (new LogCaptureScope(LogProvider.ConsoleLog)) {
          using (new LogCaptureScope(LogProvider.NullLog)) {
            using (new LogCaptureScope(LogProvider.ConsoleLog)) {
              TestLog.Info("Writing to console: " + marker);
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
        using (new LogCaptureScope(LogProvider.NullLog)) {
          TestLog.Info("Writing to null: "+marker);
        }
        outputString = output.ToString();
        oldOutput.WriteLine("Output:\r\n{0}\r\nEnd.", outputString);
        i = 0;
        Assert.IsTrue((i = outputString.IndexOf(marker,i))>=0);
        Assert.IsTrue((i = outputString.IndexOf(marker,i+1))<0);

        // Test 4
        output.Length = 0;
        using (new LogCaptureScope(LogProvider.ConsoleLog)) {
          TestLog.Info("Writing to console: "+marker);
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
      TestLog.Info("Completed.");
    }
  }
}