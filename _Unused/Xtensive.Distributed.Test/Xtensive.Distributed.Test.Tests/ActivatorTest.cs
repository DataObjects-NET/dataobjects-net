// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.17

using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Distributed.Test.Tests
{
  [TestFixture]
  public class ActivatorTest
  {
    // [Test]
    public void Activate()
    {
      string path = "Xtensive.Distributed.Test.TaskActivator.exe";
      string commandString = @". Xtensive.Distributed.Test.Tests Xtensive.Distributed.Test.Tests.ActivatorTest Test";
      ProcessStartInfo processStartInfo = new ProcessStartInfo(path, commandString);
      processStartInfo.UseShellExecute = false;
      processStartInfo.RedirectStandardOutput = true;
      processStartInfo.RedirectStandardError = true;
      Process runProcess = Process.Start(processStartInfo);
      OutputReader reader = new OutputReader(runProcess);
      Thread OutThread = new Thread(reader.OutReader);
      Thread ErrorThread = new Thread(reader.ErrorReader);
      OutThread.Start();
      ErrorThread.Start();
      OutThread.Join();
      ErrorThread.Join();
      runProcess.WaitForExit();
      Assert.AreEqual("TestCompleted", reader.Output);
      Assert.AreEqual("", reader.Error);
    }

    internal class OutputReader
    {
      private readonly Process process;
      private string output;
      private string error;

      public OutputReader(Process process)
      {
        this.process = process;
      }

      public void OutReader()
      {
        output = process.StandardOutput.ReadToEnd();
      }

      public void ErrorReader()
      {
        error = process.StandardError.ReadToEnd();
      }

      public string Output
      {
        get { return output; }
      }

      public string Error
      {
        get { return error; }
      }
    }

    public void Test()
    {
      Console.Write("TestCompleted");
    }
  }
}