// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

using System;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Messaging.Tests
{
  [TestFixture]
  public class SerializedMessageTest
  {
    private const string remoteEndPoint = "tcp://127.0.0.1:3333/";
    private static EventWaitHandle sendTestWaitHandle = new AutoResetEvent(false);
    private static bool sendTestOk = false;


    private void SendTestMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Assert.IsTrue(e.Message is TestQueryMessage);
      Console.WriteLine("Message received");
      sendTestOk = true;
      sendTestWaitHandle.Set();
    }

    [Test]
    public void Send()
    {
      using (Receiver receiver = new Receiver(remoteEndPoint))
      {
        receiver.MessageReceived += SendTestMessageReceived;
        receiver.StartReceive();
        using (Sender sender = new Sender(remoteEndPoint))
        {
          TestQueryMessage message = new TestQueryMessage(new byte[100]);
          ISerializedMessage serializedMessage = sender.Prepare(message);
          sender.Send(serializedMessage);
          sendTestWaitHandle.WaitOne(1000, true);
          sender.Send(serializedMessage);
          sendTestWaitHandle.WaitOne(1000, true);
          Assert.IsTrue(sendTestOk);
        }
      }
    }



 
  }
}