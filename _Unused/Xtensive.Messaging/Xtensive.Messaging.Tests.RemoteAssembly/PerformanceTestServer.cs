// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.16

using System;

namespace Xtensive.Messaging.Tests.RemoteAssembly
{
  public class PerformanceTestServer: MarshalByRefObject
  {
    private Receiver receiver;

    public void StartServer(string serverUrl, TimeSpan statisticsTimeout)
    {
      receiver = new Receiver(serverUrl);
      receiver.AddProcessor(typeof(BaseProcessor));
      receiver.ProcessorContext = new Statistics(statisticsTimeout, "SERVER  :");
      receiver.StartReceive();
    }
  }
}