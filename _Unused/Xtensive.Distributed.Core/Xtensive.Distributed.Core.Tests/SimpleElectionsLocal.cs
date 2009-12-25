// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.10

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Distributed.Core;

namespace Xtensive.Distributed.Core.Tests
{
  [TestFixture]
  public class SimpleElectionsLocal
  {
    [Test]
    public void ElectionsLocal()
    {
      int participantCount = 4;
      int workerThreadsCount = 0; // number of worker threads

      var participants = new List<NetworkEntity>();
      for (int i = 0; i < participantCount; i++) {
        int portNumber = 3300 + i;
        var urls = new Dictionary<string, string>();
        urls["SimpleElections"] = "tcp://127.0.0.1:" + portNumber + "/";
        participants.Add(new NetworkEntity("A" + i, urls));
      }
      ElectionGroup egroup = new ElectionGroup("FirstGroup", participants);
      var contexts   = new List<ElectionContext>();
      var algorithms = new List<SimpleElectionAlgorithm>();
      for (int i = 0; i < participantCount; i++) {
        contexts.Add(new ElectionContext(egroup, participants[i]));
        algorithms.Add(new SimpleElectionAlgorithm(contexts[i]));
      }

      var works = new Dictionary<int, Thread>();
      for (int i=0; i<workerThreadsCount; i++) {
        works[i] = new Thread(HardWork);
        works[i].Start();
      }

      // Read act and master from etalon host, then read from others, 
      // and again from etalon. If etalon values hadn't changed
      // all other valid masters and acts should be as etalon has.
      Random rand = new Random();
      for (int c = 0; c < 50; c++ )
      {
        Thread.Sleep(100);

        int standard = rand.Next(participantCount);
        ElectionResult res = contexts[standard].Result;
        if (res == null)
          continue;
        ElectionAct actBefore = res.Act;
        NetworkEntity masterBefore = res.Master;
        if (!res.IsActual)
          continue;

        var currentActs    = new List<ElectionAct>();
        var currentMasters = new List<NetworkEntity>();
        for (int i = 0; i < participantCount; i++) {
          res = contexts[i].Result;
          if (res == null)
            continue;
          currentActs.Add(res.Act);
          currentMasters.Add(res.Master);
          if (!res.IsActual)
            continue;
        }

        res = contexts[standard].Result;
        if (res == null)
          continue;
        ElectionAct actAfter = res.Act;
        NetworkEntity masterAfter = res.Master;
        if (!res.IsActual)
          continue;

        if ((actBefore == actAfter) && (masterBefore == masterAfter)) {
          for (int i=0; i<currentActs.Count; i++) {
            if (currentActs[i]!=actBefore)
              throw new ApplicationException("Acts are inconsistent!");
          }
          for (int i = 0; i < currentMasters.Count; i++) {
            if (currentMasters[i]!=masterBefore)
              throw new ApplicationException("Masters are inconsistent!");
          }
        }
      }

      for (int i = 0; i < participantCount; i++)
        algorithms[i].Dispose();
      for (int i = 0; i < workerThreadsCount; i++)
        works[i].Abort();
    }
    
    private void HardWork()
    {
      int i = 0;
      while (true) {
        i++;
      }
    }
  }
}