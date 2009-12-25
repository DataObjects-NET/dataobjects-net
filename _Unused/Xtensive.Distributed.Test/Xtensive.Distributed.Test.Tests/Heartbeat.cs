// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.09.20

using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Distributed.Test.Tests
{
  [TestFixture]
  public class Heartbeat
  {
    public const string ServerUrl = "tcp://127.0.0.1:37091/Server";
    private readonly string targetFolder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "Target");

    [Test]
    public void Simple()
    {
      AgentInfo[] result;
      Server server = new Server(ServerUrl);
      using (new Agent(ServerUrl, targetFolder)) {
        Thread.Sleep(TimeSpan.FromSeconds(10));
      }
      result = server.AgentInfos;
      server.Dispose();
      server.Dispose();
      Console.WriteLine(result);
    }

    [Test]
    public void TestHeartbeats()
    {
      const int agentCount = 20;
      AgentInfo[] result;
      using (Server server = new Server(ServerUrl)) {
        Agent[] agents = new Agent[agentCount];
        for (int i = 0; i < agentCount; i++) {
          agents[i] = new Agent(ServerUrl,targetFolder);
        }
        Thread.Sleep(TimeSpan.FromSeconds(10));
        foreach (Agent agent in agents) {
          agent.Dispose();
        }

        result = server.AgentInfos;
      }
      Assert.AreEqual(agentCount, result.Length);
    }
  }
}