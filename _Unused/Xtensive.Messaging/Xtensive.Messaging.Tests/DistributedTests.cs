// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.16

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Distributed.Test;
using Xtensive.Messaging.Tests.RemoteAssembly;

namespace Xtensive.Messaging.Tests
{
  [TestFixture]
  public class DistributedTests
  {
    private Server server;
    private string testServerUrl = "tcp://127.0.0.1:44430/Server";
    private List<Agent> testAgents = new List<Agent>();
    private List<PerformanceTestClient> clients = new List<PerformanceTestClient>();


    [TestFixtureSetUp]
    public void Init()
    {
      server = new Server(testServerUrl);
      for (int i = 1; i < 6; i++) {
        string agentPath = Environment.GetEnvironmentVariable(@"TEMP") + @"\agent" + i;
        if (Directory.Exists(agentPath)) {
          Directory.Delete(agentPath, true);
        }
        Directory.CreateDirectory(agentPath);
        Agent agent = new Agent(testServerUrl, agentPath);
        testAgents.Add(agent);
      }
    }

    [TestFixtureTearDown]
    public void Dispose()
    {
      foreach (Agent agent in testAgents) {
        agent.Dispose();
      }
      server.Dispose();
    }

    [Test]
    [Explicit, Category("Performance")]
    public void PerformanceTest()
    {
      Thread.Sleep(3000);
      TimeSpan statisiticsTimeout = TimeSpan.FromSeconds(1);
      Client client = new Client(testServerUrl);
      List<Task<PerformanceTestClient>> tasks = new List<Task<PerformanceTestClient>>();
      AgentInfo[] agents = client.Agents;
      Assert.Greater(agents.Length, 1);
      // Uploading tasks
      Task<PerformanceTestServer> serverTask = client.CreateTask<PerformanceTestServer>(agents[0].Url);
      serverTask.FileManager.Upload(".", "");
      // string messageServerUrl = "tcp://" + new UrlInfo(serverTask.Url).Host + ":8117/";
      string messageServerUrl = "tcp://127.0.0.1:8117/";
      for (int i = 1; i < agents.Length; i++) {
        Task<PerformanceTestClient> agentTask = client.CreateTask<PerformanceTestClient>(agents[i].Url);
        agentTask.FileManager.Upload(".", "");
        clients.Add(agentTask.Start());
        tasks.Add(agentTask);
      }
      // Start tasks
      PerformanceTestServer messageServer = serverTask.Start();
      messageServer.StartServer(messageServerUrl, statisiticsTimeout);
      Thread.Sleep(10000);
      int taskNumber = 1;
      foreach (PerformanceTestClient remoteClient in clients) {
        remoteClient.StartClient(messageServerUrl, statisiticsTimeout, "CLIENT_" + taskNumber++ + ": ");
      }
      Thread.Sleep(30000);
      //Stop tasks
      foreach (Task<PerformanceTestClient> task in tasks) {
        task.Kill();
      }
      serverTask.Kill();
    }
  }
}