// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.07

using System;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.Serialization;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Messaging.Diagnostics;

namespace Xtensive.Messaging.Tests
{
  [TestFixture]
  public class MessagingTest
  {
    private const string remoteEndPoint = "tcp://127.0.0.1:3333/";
    private const string localEndPoint = "tcp://127.0.0.1:4444/";


    private static EventWaitHandle sendTestWaitHandle = new AutoResetEvent(false);
    private static bool sendTestOk = false;

    [TestFixtureSetUp]
    public void SetupTest()
    {
      // Init plugin manager to avoid it to affect on perfomance tests
      Receiver receiver = new Receiver(remoteEndPoint);
      receiver.StartReceive();
      receiver.Dispose();
    }

    [Test]
    public void TestDispose()
    {
      DebugInfo.Reset(true, true);
      Receiver receiver = new Receiver(remoteEndPoint);
      Sender sender = new Sender(remoteEndPoint);
      receiver.MessageReceived += DisposeTestMessageReceived;
      receiver.StartReceive();
      try {
        sender.Ask("dddd");
      }
      catch {
        ;
      }
      // object reply = sender.Ask(new byte[1000]);
      // Assert.IsNotNull(reply);
      //sender.Dispose();
      sender = null;
      receiver = null;
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
      DebugInfo.LogStatistics();
      Assert.AreEqual(0, DebugInfo.SenderCount);
      Assert.AreEqual(0, DebugInfo.ReceiverCount);
      Console.WriteLine("Connect new socket in pool: {0}", Providers.Tcp.Diagnostics.DebugInfo.GetSocketFromPoolCount);
      Console.WriteLine("Connection count: {0}", Providers.Tcp.Diagnostics.DebugInfo.ConnectionCount);
    }

    private void DisposeTestMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      // Console.WriteLine("Message received. Sending reply.");
      e.Sender.Send(new byte[2000]);
    }

    [Test]
    public void MultipleStartReceive()
    {
      using (Receiver receiver = new Receiver(remoteEndPoint))
      {
        receiver.StartReceive();
        receiver.StartReceive();
        receiver.StartReceive();
        receiver.StopReceive();
        receiver.StopReceive();
        receiver.StopReceive();
        receiver.StartReceive();
        receiver.StartReceive();
      }
    }

    

    [Test]
    public void SendTest()
    {
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += SendTestMessageReceived;
        receiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint)) {
          sender.Send(new byte[100]);
          sendTestWaitHandle.WaitOne(1000, true);
          Assert.IsTrue(sendTestOk);
        }
      }
    }

    private void SendTestMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine("Message received");
      sendTestOk = true;
      sendTestWaitHandle.Set();
    }


    [Test]
    public void AskTest()
    {
      using (Receiver receiver = new Receiver(remoteEndPoint))
      using (Sender sender = new Sender(remoteEndPoint)) {
        receiver.MessageReceived += AskTestMessageReceived;
        receiver.StartReceive();
        object reply = sender.Ask(new byte[1000]);
        Assert.IsNotNull(reply);
      }
    }

    private void AskTestMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine("Message received. Sending reply.");
      e.Sender.Send(new byte[2000]);
    }


    private const int MessageArrivalCount = 1000;
    private readonly bool[] MessageReceivedMarkers = new bool[MessageArrivalCount];

    [Test]
    public void MessageArrival()
    {
      DebugInfo.Reset(true, false);
      Providers.Tcp.Diagnostics.DebugInfo.Reset();
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += MessageArrivalReceived;
        receiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint)) {
          for (int i = 0; i<MessageArrivalCount; i++)
            sender.Send(i);
          Thread.Sleep(5000); // Waiting for all messages arrive
        }
      }
      Console.WriteLine("Connect new socket in pool: {0}", Providers.Tcp.Diagnostics.DebugInfo.GetSocketFromPoolCount);
      Console.WriteLine("Connection count: {0}", Providers.Tcp.Diagnostics.DebugInfo.ConnectionCount);
      int receiveCount = 0;
      for (int i = 0; i<MessageArrivalCount; i++)
        if (MessageReceivedMarkers[i])
          receiveCount++;
      Assert.AreEqual(MessageArrivalCount, receiveCount);
    }

    private void MessageArrivalReceived(object sender, MessageReceivedEventArgs e)
    {
      lock (MessageReceivedMarkers) {
        int messageId = (int)e.Message;
        if (MessageReceivedMarkers[messageId])
          Console.WriteLine("ERROR: Message already received");
        Assert.IsFalse(MessageReceivedMarkers[messageId], "Message already received");
        MessageReceivedMarkers[messageId] = true;
      }
    }


    private static bool stopSendRetryThread;
    private static long sendRetryMessagesArrived;
    // This test temporary removed because Sender.Send is not guartee delivery
    // [Test]
    public void SendRetryTest()
    {
      int messageCount = 10000;
      DebugInfo.Reset(true, false);
      Providers.Tcp.Diagnostics.DebugInfo.Reset();
      Thread resetConnectionsThread = new Thread(CloseConnectionsInPool);
      resetConnectionsThread.Start();
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        receiver.MessageReceived += SendRetryMessageReceived;
        receiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint))
          for (int i = 0; i<messageCount; i++)
            sender.Send(i);
        Thread.Sleep(1000); // Waiting for messages to arrive
      }
      stopSendRetryThread = true;
      resetConnectionsThread.Join();
      Console.WriteLine("Sended: {0}", messageCount);
      Console.WriteLine("Received: {0}", sendRetryMessagesArrived);
      Console.WriteLine("Counters-----------");
      Console.WriteLine("GetSocketFromPoolCount: {0}", Providers.Tcp.Diagnostics.DebugInfo.GetSocketFromPoolCount);
      Console.WriteLine("Socket sended: {0}", Providers.Tcp.Diagnostics.DebugInfo.SendCount);
      Console.WriteLine("ConnectionCount: {0}", Providers.Tcp.Diagnostics.DebugInfo.ConnectionCount);
      Assert.IsTrue(sendRetryMessagesArrived==messageCount);
    }

    private void SendRetryMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Interlocked.Increment(ref sendRetryMessagesArrived);
    }

    public static void CloseConnectionsInPool()
    {
      while (!stopSendRetryThread) {
        try {
          Providers.Tcp.Diagnostics.DebugInfo.DropAllPolledConnections();
        }
        catch (Exception) {
        }
        Thread.Sleep(100);
      }
    }


    [Test]
    [ExpectedException(typeof (MessagingException))]
    public void TestProtocol()
    {
      Receiver receiver = new Receiver("udp://localhost:5555/");
      receiver.StartReceive();
    }

    [Test]
    public void TestMultipleIps()
    {
      // Computer msu have several IPs
      using (Receiver receiver = new Receiver("tcp://localhost:5555/")) {
        receiver.StartReceive();
        using (Sender sender = new Sender("tcp://localhost:5555/"))
        {
          sender.Send("TEST");
        }
        Thread.Sleep(3000);

      }
    }

    [Test]
    public void TestProperties()
    {
      using (Receiver receiver = new Receiver(remoteEndPoint)) {
        Assert.AreEqual(receiver.ReceiverInfo.Url, remoteEndPoint);
        Assert.IsNotNull(receiver.SyncRoot);
        Assert.AreEqual(receiver.ReceiverUrl, remoteEndPoint);
        using (Sender sender = new Sender(remoteEndPoint, LegacyBinarySerializer.Instance)) {
          Assert.IsNotNull(sender.ReceiverUrl);
          Assert.IsNotNull(sender.ReceiverUrl);
          sender.ResponseReceiver = new Receiver(localEndPoint);
          Assert.AreEqual(sender.ResponseReceiver.ReceiverUrl, localEndPoint);
          sender.ResponseTimeout = TimeSpan.FromMinutes(10);
        }
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidProgramException))]
    public void TestErrorReply()
    {
      using (Receiver remoteReceiver = new Receiver(remoteEndPoint)) {
        remoteReceiver.MessageReceived += ErrorReplyMessageReceived;
        remoteReceiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint)) {
          object reply = sender.Ask("Reply");
        }
      }
    }

    private void ErrorReplyMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine("Received. Sending error message.");
      ErrorMessage errorMessage = new ErrorMessage(new InvalidProgramException("test reply exception"));
      try {
        e.Sender.Send(errorMessage);
      }
      catch (Exception ex) {
        Console.WriteLine(ex);
      }
    }


    [Test]
    [ExpectedException(typeof (ArgumentNullException))]
    public void TestException()
    {
      using (Receiver remoteReceiver = new Receiver(remoteEndPoint)) {
        remoteReceiver.MessageReceived += ExceptionMessageReceived;
        remoteReceiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint)) {
          object reply = sender.Ask("Reply");
        }
      }
    }

    private void ExceptionMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Console.WriteLine("Received. Throwing exception.");
      throw new ArgumentNullException("sender");
    }
  }
}