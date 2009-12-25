// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.07.05

using System;
using System.Globalization;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Messaging;

namespace Xtensive.Messaging.Diagnostics
{
  ///<summary>
  /// Provides debug information and tools.
  ///</summary>
  public sealed class DebugInfo : DebugInfoTemplate<DebugInfo>
  {
    private const double ProbabilityScale = 1000000;

    private static TimeSpan statisticsLogPeriod = TimeSpan.FromSeconds(5);
    private static long messageSentCount;
    private static long messageReceivedCount;
    private static long senderCount;
    private static long receiverCount;
    private static long sendErrorCount;
    private static long receiveErrorCount;
    private static int skipSendProbability;
    private static int skipReceiveProbability;
    private static int dropConnectionProbability;
    private static readonly Thread statisticsThread;
    private static readonly long[] lastLoggedCounterValues = new long[16];
    private static DateTime lastLoggedTime = HighResolutionTime.Now.Subtract(TimeSpan.FromSeconds(1));

    ///<summary>
    /// Gets or sets time period than statistics automatically shows in <see cref="Log"/>.
    ///</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> less or equal to <see cref="TimeSpan.Zero"/></exception>
    public static TimeSpan StatisticsLogPeriod
    {
      get
      {
        EnsureOperable();
        return statisticsLogPeriod;
      }
      set
      {
        EnsureOperable();
        ArgumentValidator.EnsureArgumentIsInRange(value, TimeSpan.FromTicks(1), TimeSpan.MaxValue, "value");
        statisticsLogPeriod = value;
      }
    }

    /// <summary>
    /// Gets count of messages sent by <see cref="Sender"/>s.
    /// </summary>
    public static long MessageSentCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref messageSentCount);
      }
    }

    /// <summary>
    /// Gets count of messages received by <see cref="Sender"/>s.
    /// </summary>
    public static long MessageReceivedCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref messageReceivedCount);
      }
    }

    /// <summary>
    /// Gets current count of active <see cref="Sender"/>s.
    /// </summary>
    public static long SenderCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref senderCount);
      }
    }

    /// <summary>
    /// Gets current count of active <see cref="Receiver"/>s.
    /// </summary>
    public static long ReceiverCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref senderCount);
      }
    }

    /// <summary>
    /// Gets count of errors while send messages.
    /// </summary>
    public static long SendErrorCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref sendErrorCount);
      }
    }

    /// <summary>
    /// Gets count of errors while receive messages.
    /// </summary>
    public static long ReceiveErrorCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref receiveErrorCount);
      }
    }

    /// <summary>
    /// Gets or sets probability of skip message while send. Used to emulate message loss. If <paramref name="value"/> is equal to 1, all messages will be lost. Default value is <see langword="0"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is outside of [0,1] interval.</exception>
    public static double SkipSendProbability
    {
      get
      {
        EnsureOperable();
        return skipSendProbability/ProbabilityScale;
      }
      set
      {
        EnsureOperable();
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, 1, "value");
        skipSendProbability = (int) (value*ProbabilityScale);
      }
    }

    /// <summary>
    /// Gets or sets probability of skip message while receive. Used to emulate message loss. If <paramref name="value"/> is equal to 1, all messages will be lost. Default value is <see langword="0"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is outside of [0,1] interval.</exception>
    public static double SkipReceiveProbability
    {
      get
      {
        EnsureOperable();
        return skipReceiveProbability/ProbabilityScale;
      }
      set
      {
        EnsureOperable();
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, 1, "value");
        skipReceiveProbability = (int) (value*ProbabilityScale);
      }
    }

    /// <summary>
    /// Gets or sets probability of drop connection while send. Used to emulate connection broke in real environment. If <paramref name="value"/> is equal to 1, all connection will be dropped. Default value is <see langword="0"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is outside of [0,1] interval.</exception>
    public static double DropConnectionProbability
    {
      get
      {
        EnsureOperable();
        return dropConnectionProbability/ProbabilityScale;
      }
      set
      {
        EnsureOperable();
        ArgumentValidator.EnsureArgumentIsInRange(value, 0, 1, "value");
        dropConnectionProbability = (int) (value*ProbabilityScale);
      }
    }

    // Methods

    /// <summary>
    /// Resets counters and probabilities to default values.
    /// </summary>
    /// <param name="counters">If <see langword="true"/> all counters will be reseted to theirs default values.</param>
    /// <param name="probabilities">If <see langword="true"/> all probabilities will be reseted to theirs default values.</param>
    public static void Reset(bool counters, bool probabilities)
    {
      EnsureOperable();
      lock (SyncRoot)
      {
        if (counters)
        {
          Interlocked.Exchange(ref messageSentCount, 0);
          Interlocked.Exchange(ref messageReceivedCount, 0);
          Interlocked.Exchange(ref senderCount, 0);
          Interlocked.Exchange(ref receiverCount, 0);
          Interlocked.Exchange(ref sendErrorCount, 0);
          Interlocked.Exchange(ref receiveErrorCount, 0);
          for (int i = 0; i < lastLoggedCounterValues.Length; i++)
            lastLoggedCounterValues[i] = 0;
        }
        if (probabilities)
        {
          SkipSendProbability = 0;
          SkipReceiveProbability = 0;
          DropConnectionProbability = 0;
        }
      }
    }

    /// <summary>
    /// Writes current statistics to <see cref="Log"/>.
    /// </summary>
    public static void LogStatistics()
    {
      EnsureOperable();
      if (!Log.IsLogged(LogEventTypes.Debug))
        return;
      lock (SyncRoot)
      {
        DateTime now = HighResolutionTime.Now;
        double timeDelta = now.Subtract(lastLoggedTime).TotalSeconds;
        lastLoggedTime = now;
        if (timeDelta <= 0)
          return;
        Pair<long, long> p1 = CounterValueAndDifference(MessageSentCount, 0);
        Pair<long, long> p2 = CounterValueAndDifference(MessageReceivedCount, 1);
        Log.Debug("Statistics: {0}/{1} messages per second sent/received, total: {2}/{3}",
                  (int) (p1.Second/timeDelta), (int) (p2.Second/timeDelta),
                  p1.First, p2.First);
        Log.Debug("Statistics: {0}/{1} senders/receivers are open", SenderCount, ReceiverCount);
        Log.Debug("Statistics: {0}/{1} send/receive errors", SendErrorCount, ReceiveErrorCount);
      }
    }

    private static Pair<long, long> CounterValueAndDifference(long counterValue, int counterIndex)
    {
      long lastLoggedCounterValue = lastLoggedCounterValues[counterIndex];
      lastLoggedCounterValues[counterIndex] = counterValue;
      return new Pair<long, long>(counterValue, counterValue - lastLoggedCounterValue);
    }

    #region SkipXxxNow methods

    internal static bool SkipSendNow
    {
      get
      {
        if (!IsOperableDefined || !IsOperable || skipSendProbability == 0)
          return false;
        return Random <= SkipSendProbability;
      }
    }

    internal static bool SkipReceiveNow
    {
      get
      {
        if (!IsOperableDefined || !IsOperable || skipReceiveProbability == 0)
          return false;
        return Random <= SkipReceiveProbability;
      }
    }

    internal static bool DropConnectionNow
    {
      get
      {
        if (!IsOperableDefined || !IsOperable || dropConnectionProbability == 0)
          return false;
        return Random <= DropConnectionProbability;
      }
    }

    #endregion

    #region IncreaseXxxCount methods

    internal static void IncreaseMessageSentCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Increment(ref messageSentCount);
    }

    internal static void IncreaseMessageReceivedCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Increment(ref messageReceivedCount);
    }

    internal static void IncreaseSenderCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Increment(ref senderCount);
    }

    internal static void DecreaseSenderCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Decrement(ref senderCount);
    }

    internal static void IncreaseReceiverCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Increment(ref receiverCount);
    }

    internal static void DecreaseReceiverCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Decrement(ref receiverCount);
    }

    internal static void IncreaseSendErrorCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Increment(ref sendErrorCount);
    }

    internal static void IncreaseReceiveErrorCount()
    {
      if (!IsOperable)
        return;
      Interlocked.Increment(ref receiveErrorCount);
    }

    private static void StatisticsThread()
    {
      while (true)
      {
        if (IsOperableDefined)
        {
          if (!IsOperable)
            return;
          LogStatistics();
        }
        Thread.Sleep(statisticsLogPeriod);
      }
    }

    #endregion

    
    // Constructors

    private DebugInfo()
    {
    }

    // Type initializer
    
    static DebugInfo()
    {
      if (Log.IsLogged(LogEventTypes.Debug)) {
        statisticsThread = new Thread(StatisticsThread);
        statisticsThread.Start();
      }
    }
  }
}