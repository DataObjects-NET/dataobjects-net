// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.16

using System;
using System.Threading;

namespace Xtensive.Messaging.Tests.RemoteAssembly
{
  public class Statistics
  {
    private int messageCount;
    private readonly TimeSpan statisticsTimeout;
    private readonly DateTime startTime;
    private readonly Thread thread;
    private readonly string prefix;
    private DateTime lastTime = DateTime.Now;

    public int MessageCount
    {
      get { return messageCount; }
    }

    public void IncreaseMessageCount()
    {
      Interlocked.Increment(ref messageCount);
    }

    private void StatisticsThread()
    {
      int lastMessageCount = messageCount;
      while (true) {
        int currentMessageCount = messageCount;
        DateTime currentTime = DateTime.Now;
        double totalSeconds = (DateTime.Now - startTime).TotalSeconds;
        double currentTurnSeconds = (currentTime - lastTime).TotalSeconds;
        Log.Info("{0} {1} msg/sec, {2} average msg/sec, {3} msgs, {4} seconds", prefix,
                  (currentMessageCount - lastMessageCount) / currentTurnSeconds,
                  currentMessageCount/totalSeconds, currentMessageCount, totalSeconds);
        lastMessageCount = currentMessageCount;
        lastTime = currentTime;
        Thread.Sleep(statisticsTimeout);
      }
    }

    public Statistics(TimeSpan statisticsTimeout, string prefix)
    {
      this.prefix = prefix;
      this.statisticsTimeout = statisticsTimeout;
      startTime = DateTime.Now;
      thread = new Thread(StatisticsThread);
      thread.Start();
    }
  }
}