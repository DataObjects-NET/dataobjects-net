// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.17

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Messaging.Diagnostics;

namespace Xtensive.Messaging.Tests
{
  [TestFixture]
  public class FailProbabilityTest
  {
    private const string remoteEndPoint = "tcp://127.0.0.1:3333/";
    private const int MessageToSendCount = 1000;
    private bool[] messageReceivedFlags;
    private bool duplicateMessageReceived;

    [Test]
    public void SentFailFull()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipSendProbability = 1;
      Assert.AreEqual(0, MessageArrival(false));
    }

    [Test]
    public void SentFailNone()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipSendProbability = 0;
      Assert.AreEqual(MessageToSendCount, MessageArrival(false));
    }

    [Test]
    public void SentFailHalf()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipSendProbability = 0.5;
      Console.WriteLine("Message% arrival: {0}%", 100*MessageArrival(false)/MessageToSendCount);
    }


    [Test]
    public void ReceiveFailFull()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipReceiveProbability = 1;
      Assert.AreEqual(0, MessageArrival(false));
    }

    [Test]
    public void ReceiveFailNone()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipReceiveProbability = 0;
      Assert.AreEqual(MessageToSendCount, MessageArrival(false));
    }

    [Test]
    public void ReceiveFailHalf()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipReceiveProbability = 0.5;
      Console.WriteLine("Message% arrival: {0}%", 100*MessageArrival(false)/MessageToSendCount);
    }

    [Test]
    public void SentReceiveFailHalf()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.SkipReceiveProbability = 0.5;
      DebugInfo.SkipSendProbability = 0.5;
      Console.WriteLine("Message% arrival: {0}%", 100*MessageArrival(false)/MessageToSendCount);
    }


    [Test]
    public void DropConnectionFull()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.DropConnectionProbability = 1;
      int arrivalCount = MessageArrival(true);
      DebugInfo.LogStatistics();
      Assert.AreEqual(0, arrivalCount);
    }

    [Test]
    public void DropConnectionNone()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.DropConnectionProbability = 0;
      Assert.AreEqual(MessageToSendCount, MessageArrival(true));
    }

    [Test]
    public void DropConnectionHalf()
    {
      DebugInfo.Reset(true, true);
      DebugInfo.DropConnectionProbability = 0.5;
      Console.WriteLine("Message% arrival: {0}%", 100*MessageArrival(true)/MessageToSendCount);
    }

    private int MessageArrival(bool tryCatchBlock)
    {
      Providers.Tcp.Diagnostics.DebugInfo.Reset();
      duplicateMessageReceived = false;
      messageReceivedFlags = new bool[MessageToSendCount];
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += MessageArrivalReceived;
        receiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint)) {
          for (int i = 0; i < MessageToSendCount; i++) {
            if (tryCatchBlock) {
              try {
                sender.Send(i);
              }
              catch {
              }
            }
            else {
              sender.Send(i);
            }
          }
          Thread.Sleep(1000); // Waiting for all messages arrive
        }
      }
      Console.WriteLine("Connect new socket in pool: {0}", Providers.Tcp.Diagnostics.DebugInfo.GetSocketFromPoolCount);
      Console.WriteLine("Connection count: {0}", Providers.Tcp.Diagnostics.DebugInfo.ConnectionCount);
      int receiveCount = 0;
      for (int i = 0; i < MessageToSendCount; i++)
        if (messageReceivedFlags[i])
          receiveCount++;
      Assert.IsFalse(duplicateMessageReceived);
      return receiveCount;
    }

    private void MessageArrivalReceived(object sender, MessageReceivedEventArgs e)
    {
      lock (messageReceivedFlags) {
        int messageId = (int)e.Message;
        if (messageReceivedFlags[messageId])
          duplicateMessageReceived = true;
        messageReceivedFlags[messageId] = true;
      }
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      DebugInfo.Reset(true, true);
    }
  }
}