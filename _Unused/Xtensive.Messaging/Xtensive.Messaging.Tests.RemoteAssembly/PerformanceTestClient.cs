// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.16

using System;
using System.Threading;

namespace Xtensive.Messaging.Tests.RemoteAssembly
{
  public class PerformanceTestClient: MarshalByRefObject
  {
    private Thread thread;
    private string serverUrl;
    private TimeSpan statisticsTimeout;
    private string debugPrefix;

    public void StartClient(string serverUrl, TimeSpan statisticsTimeout, string debugPrefix)
    {
      this.serverUrl = serverUrl;
      this.statisticsTimeout = statisticsTimeout;
      this.debugPrefix = debugPrefix;
      thread = new Thread(SendThread);
      thread.Start();
    }

    private void SendThread()
    {
      Statistics statistics = new Statistics(statisticsTimeout, debugPrefix);
      using (Sender sender = new Sender(serverUrl))
      {
        while (true)
        {
          sender.Ask("QUERY");
          statistics.IncreaseMessageCount();
        }
      }
    }
  }
}