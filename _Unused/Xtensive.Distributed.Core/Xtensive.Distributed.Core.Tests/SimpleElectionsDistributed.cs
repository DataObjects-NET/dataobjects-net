// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.17

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Distributed.Core.RemoteAssembly;
using Xtensive.Distributed.Test;
using Xtensive.Distributed.Core;

namespace Xtensive.Distributed.Core.Tests
{
  [TestFixture]
  public class SimpleElectionsDistributed
  {
    private Server server;
    private readonly string testServerUrl = "tcp://127.0.0.1:44430/Server";
    private readonly List<Agent> testAgents = new List<Agent>();
    private readonly int participantCount = 4;
    private readonly List<ElectionsParticipant> clients = new List<ElectionsParticipant>();


    [TestFixtureSetUp]
    public void Init()
    {
      server = new Server(testServerUrl);
      for (int i = 0; i < participantCount; i++) {
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
    }

    [Test]
    [Explicit, Category("Performance")]
    public void StartElections()
    {
      Thread.Sleep(3000);
      Client client = new Client(testServerUrl);
      var tasks = new List<Task<ElectionsParticipant>>();
      AgentInfo[] agents = client.Agents;
      Assert.Greater(agents.Length, 1);
      // Uploading tasks
      for (int i = 0; i < agents.Length; i++) {
        Task<ElectionsParticipant> agentTask = client.CreateTask<ElectionsParticipant>(agents[i].Url);
        agentTask.FileManager.Upload(".", "");
        clients.Add(agentTask.Start());
        tasks.Add(agentTask);
      }

      // Define election group
      var participants = new List<NetworkEntity>();
      for (int i = 0; i < participantCount; i++) {
        int portNumber = 3300 + i;
        Dictionary<string, string> urls = new Dictionary<string, string>();
        urls["SimpleElections"] = "tcp://127.0.0.1:" + portNumber.ToString() + "/";
        participants.Add(new NetworkEntity("A" + i.ToString(), urls));
      }
      ElectionGroup egroup = new ElectionGroup("FirstGroup", participants);

      // Start tasks
      for (int i = 0; i < participantCount; i++) {
        ElectionsParticipant remoteClient = clients[i];
        remoteClient.StartElections(egroup, participants[i]);
      }

      // Poll participants

      // Read act and master from etalon host, then read from others, 
      // and again from etalon. If etalon values hadn't changed
      // all other valid masters and acts should be as etalon has.
      Random rand = new Random();
      for (int c = 0; c < 1000000; c++) {
        Thread.Sleep(100);

        int standard = rand.Next(participantCount);
        ElectionResult res = clients[standard].CurrentResult;
        if (res==null)
          continue;
        ElectionAct actBefore = res.Act;
        NetworkEntity masterBefore = res.Master;

        List<ElectionAct> currentActs = new List<ElectionAct>();
        List<NetworkEntity> currentMasters = new List<NetworkEntity>();
        for (int i = 0; i < participantCount; i++) {
          res = clients[i].CurrentResult;
          if (res==null)
            continue;
          currentActs.Add(res.Act);
          currentMasters.Add(res.Master);
        }

        res = clients[standard].CurrentResult;
        if (res==null)
          continue;
        ElectionAct actAfter = res.Act;
        NetworkEntity masterAfter = res.Master;

        if ((actBefore==actAfter) && (masterBefore==masterAfter)) {
          for (int i = 0; i < currentActs.Count; i++) {
            if (currentActs[i]!=actBefore) {
              throw new ApplicationException("Acts are inconsistent!");
            }
          }
          for (int i = 0; i < currentMasters.Count; i++) {
            if (currentMasters[i]!=masterBefore) {
              throw new ApplicationException("Masters are inconsistent!");
            }
          }
        }
      }

      // Stop tasks
      foreach (Task<ElectionsParticipant> task in tasks)
        task.Kill();
      server.Dispose();
      foreach (Agent a in testAgents)
        a.Dispose();
    }
  }
}