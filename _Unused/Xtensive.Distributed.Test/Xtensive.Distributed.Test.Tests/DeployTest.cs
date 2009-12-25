// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.26

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Xtensive.Distributed.Test.Tests.RemoteAssembly;

namespace Xtensive.Distributed.Test.Tests
{
  [TestFixture]
  public class DeployTest
  {
    public const string ServerUrl = "tcp://127.0.0.1:37091/Server";

    private bool errorStringReaded;
    private bool outStringReaded;
    private readonly string targetFolder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "Target");


    [Test]
    public void ConsoleReadTest()
    {
      if (Directory.Exists(targetFolder))
        Directory.Delete(targetFolder, true);
      Directory.CreateDirectory(targetFolder);
      using (new Server(ServerUrl)) {
        using (new Agent(ServerUrl, targetFolder)) {
          Thread.Sleep(TimeSpan.FromSeconds(3));
          Client client = new Client(ServerUrl);
          AgentInfo[] availableAgents = client.Agents;
          Assert.Greater(availableAgents.Length, 0);
          Task<ConsoleTest> task = client.CreateTask<ConsoleTest>();
          task.FileManager.Upload("Xtensive.Distributed.Test.Tests.RemoteAssembly.dll", "");
          task.ConsoleRead += TaskConsoleReadEvent;
          task.Start();
          Thread.Sleep(TimeSpan.FromSeconds(15));
          task.Kill();
          // task.ConsoleRead -= TaskConsoleReadEvent;
        }
      }
      Directory.Delete(targetFolder, true);
      Assert.IsTrue(errorStringReaded);
      Assert.IsTrue(outStringReaded);
    }

    private void TaskConsoleReadEvent(object sender, ConsoleReadEventArgs e)
    {
      if (e.Message == ConsoleTest.ConsoleErrorString && e.IsError)
        errorStringReaded = true;
      if (e.Message == ConsoleTest.ConsoleOutputString && !e.IsError)
        outStringReaded = true;
    }
  }
}