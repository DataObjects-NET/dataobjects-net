// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.10.02

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Messaging.Diagnostics;

namespace Xtensive.Messaging.Tests
{
  [TestFixture]
  public class PerformanceTests
  {
    private const string remoteEndPoint = "tcp://127.0.0.1:3333/";
    private const string localEndPoint = "tcp://127.0.0.1:4444/";
    private static EventWaitHandle collectionTestWaitHandle = new AutoResetEvent(false);
    private static bool collectionTestOk = false;
    private static object askPerformanceSingleThreadRequest = new byte[100];
    private static object askPerformanceSingleThreadReply = new byte[250];
    private static object askPerformanceRequestMessage = new TestQueryMessage("1234567890");
    private static object askPerformanceReply = new TestQueryMessage("1234567890123456789012345");
    private static bool stopMultyThread = false;
    private static int multyThreadMessagesReceived;
    private static bool stopSerializedMessageThread = false;
    private static int serializedMessageThreadMessagesReceived;
    private static ISerializedMessage askPerformanceRequest;
    // private static ISerializedMessage AskPerofomanceReply;

    [Test]
    [Explicit, Category("Performance")]
    public void TestMemory()
    {
      DebugInfo.Reset(true, true);
      MemoryLeakDetector detector = new MemoryLeakDetector(delegate {
                                                             Receiver receiver = new Receiver(remoteEndPoint);
                                                             Sender sender = new Sender(remoteEndPoint);
                                                             receiver.MessageReceived += MemoryTestMessageReceived;
                                                             receiver.StartReceive();
                                                             object reply = sender.Ask(new byte[1000]);
                                                             Assert.IsNotNull(reply);
                                                             sender = null;
                                                             receiver = null;
                                                           }, 1000);
      bool result;
      try {
        result = detector.Test();
      }
      finally {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
      }
      DebugInfo.LogStatistics();
      Assert.IsTrue(result);
      Assert.AreEqual(0, DebugInfo.SenderCount);
      Assert.AreEqual(0, DebugInfo.ReceiverCount);
      Console.WriteLine("Connect new socket in pool: {0}", Providers.Tcp.Diagnostics.DebugInfo.GetSocketFromPoolCount);
      Console.WriteLine("Connection count: {0}", Providers.Tcp.Diagnostics.DebugInfo.ConnectionCount);
    }


    private void MemoryTestMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      // Console.WriteLine("Message received. Sending reply.");
      e.Sender.Send(new byte[2000]);
    }

    [Test]
    [Explicit, Category("Performance")]
    public void TestConnectionBroken()
    {
      int messageCount = 10;
      using (Receiver spareReceiver = new Receiver(localEndPoint)) {
        spareReceiver.StartReceive();
        using (Receiver remoteReceiver = new Receiver(remoteEndPoint)) {
          remoteReceiver.MessageReceived += TestConnectionBrokenMessageReceived;
          remoteReceiver.StartReceive();
          using (Sender sender = new Sender(remoteEndPoint)) {
            sender.ResponseReceiver = spareReceiver;
            for (int i = 0; i < messageCount; i++) {
              Console.WriteLine("----------- NEW ITERATION----------------");
              object reply = sender.Ask(i);
              Console.WriteLine("Reply received");
              Assert.IsNotNull(reply);
            }
          }
        }
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void AskPerofomanceSingleThread()
    {
      int messageCount = 20000;
      using (Receiver receiver = new Receiver(remoteEndPoint))
      using (Sender sender = new Sender(remoteEndPoint)) {
        receiver.MessageReceived += askPerofomanceMessageReceived;
        receiver.StartReceive();
        DateTime startTime = DateTime.Now;
        for (int i = 0; i < messageCount; i++) {
          object reply = sender.Ask(askPerformanceSingleThreadRequest);
          Assert.IsNotNull(reply);
        }
        Console.WriteLine("Messages/sec: {0}", messageCount/(DateTime.Now - startTime).TotalSeconds);
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void AskPerofomanceMultiThread()
    {
      stopMultyThread = false;
      int runSeconds = 15;
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += AskPerofomanceMessageReceivedMultyThread;
        receiver.StartReceive();
        Thread[] threads = new Thread[8];
        for (int i = 0; i < threads.Length; i++) {
          threads[i] = new Thread(PerfomanceMultyThreadProcess);
          threads[i].IsBackground = true;
          threads[i].Start();
        }
        Thread.Sleep(1000*runSeconds);
        stopMultyThread = true;
        for (int i = 0; i < threads.Length; i++)
          threads[i].Join();
        Console.WriteLine("Multithread mesages/sec: {0}", multyThreadMessagesReceived/runSeconds);
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void SendTest()
    {
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += SendTestMessageReceived;
        receiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint)) {
          sender.Send(new SlowMessageCollection(TimeSpan.FromSeconds(3), 10)); // Emulates "hard work" on remote side
          collectionTestWaitHandle.WaitOne(40000, true);
          Assert.IsTrue(collectionTestOk);
        }
      }
    }

    [Test]
    [Explicit, Category("Performance")]
    public void SerializedMessageAskPeroformance()
    {
      stopSerializedMessageThread = false;
      int runSeconds = 15;
      // Prepare messages
      Sender tempSender = new Sender(remoteEndPoint);
      askPerformanceRequest = tempSender.Prepare(new TestQueryMessage("1234567890"));
      // AskPerofomanceReply = tempSender.Prepare(new TestQueryMessage("1234567890123456789012345"));
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += SerializedMessagePeroformanceMessageReceived;
        receiver.StartReceive();
        Thread[] threads = new Thread[8];
        for (int i = 0; i < threads.Length; i++) {
          threads[i] = new Thread(SerializedMessagePerformanceProcess);
          threads[i].IsBackground = true;
          threads[i].Start();
        }
        Thread.Sleep(1000*runSeconds);
        stopSerializedMessageThread = true;
        for (int i = 0; i < threads.Length; i++)
          threads[i].Join();
        Console.WriteLine("Multithread mesages/sec: {0}", serializedMessageThreadMessagesReceived/runSeconds);
      }
    }


    private void SerializedMessagePeroformanceMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Interlocked.Increment(ref serializedMessageThreadMessagesReceived);
      // e.Sender.Send(AskPerofomanceReply);
      e.Sender.Send("ok");
    }

    private void SerializedMessagePerformanceProcess()
    {
      using (Sender sender = new Sender(remoteEndPoint)) {
        while (!stopSerializedMessageThread) {
          object reply = sender.Ask(askPerformanceRequest);
          Assert.IsNotNull(reply);
        }
      }
    }

    private void SendTestMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine("Message received");
      IMessageCollection collection = (IMessageCollection)e.Message;
      foreach (object o in collection) {
        Console.WriteLine(o.ToString());
      }
      collectionTestOk = true;
      collectionTestWaitHandle.Set();
    }

    private void TestConnectionBrokenMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine("Message received");
      //      Thread.Sleep(300);
      //      long currentCount = Interlocked.Increment(ref connectionBrokenTestCount);
      //      if (currentCount%3==0)
      //        // Drop every 3rd connection
      //        //Providers.Tcp.DebugInfo.DropAllPolledConnections();
      //        Thread.Sleep(300);
      e.Sender.Send("REPLY");
    }

    private void askPerofomanceMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      e.Sender.Send(askPerformanceSingleThreadReply);
    }

    private void AskPerofomanceMessageReceivedMultyThread(object sender, MessageReceivedEventArgs e)
    {
      Interlocked.Increment(ref multyThreadMessagesReceived);
      e.Sender.Send(askPerformanceReply);
    }

    private void PerfomanceMultyThreadProcess()
    {
      using (Sender sender = new Sender(remoteEndPoint))
        while (!stopMultyThread) {
          object reply = sender.Ask(askPerformanceRequestMessage);
          Assert.IsNotNull(reply);
        }
    }
  }
}