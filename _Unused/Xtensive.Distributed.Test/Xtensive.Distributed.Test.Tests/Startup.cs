// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.16

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Distributed.Test.Tests
{
  // [TestFixture]
  public class Startup
  {
    private const string ServerUrl = "tcp://alexgx:46551/Server";

    // [Test]
    [Explicit]
    public void StartServer()
    {
      Server server = new Server(ServerUrl);
      Thread.Sleep(Timeout.Infinite);
      server.Dispose();
    }

    // [Test]
    [Explicit]
    public void StartAgent()
    {
      string localPath = Environment.GetEnvironmentVariable("TEMP") + @"\Agent";
      if (Directory.Exists(localPath))
      {
        Directory.Delete(localPath);
      }
      Directory.CreateDirectory(localPath);
      Agent agent = new Agent(ServerUrl, localPath);
      Thread.Sleep(Timeout.Infinite);
      agent.Dispose();
    }
  }
}