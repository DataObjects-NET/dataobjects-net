using System;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Messaging.Tests
{
  [TestFixture]
  public class ProcessorsTest
  {
    private const string RemoteEndPoint = "tcp://127.0.0.1:3333/";
    private const string LocalEndPoint = "tcp://127.0.0.1:7777/";


    [Test]
    [ExpectedException(typeof (InvalidOperationException), ExpectedMessage = "Unknown message")]
    public void TestAutomaticDefault()
    {
      using (Receiver receiver = new Receiver(RemoteEndPoint)) {
        receiver.AddProcessor(typeof (BaseProcessor), true);
        receiver.StartReceive();
        using (Sender sender = new Sender(RemoteEndPoint)) {
          object response = sender.Ask("");
        }
      }
    }

    private const int AsyncMessageCount = 40;
    private bool[] asyncResultArray = new bool[AsyncMessageCount];

    [Test]
    public void ReplySenderAsync()
    {
      using (Receiver localReceiver = new Receiver(LocalEndPoint)) {
        localReceiver.MessageReceived += LocalReceiverMessageReceived;
        localReceiver.StartReceive();
        using (Receiver receiver = new Receiver(RemoteEndPoint)) {
          receiver.AddProcessor(typeof (AsyncBaseProcessor), true);
          receiver.StartReceive();
          using (Sender sender = new Sender(RemoteEndPoint)) {
            sender.ResponseReceiver = localReceiver;
            for (int i = 0; i<AsyncMessageCount; i++) {
              if (i%2==0)
                sender.Send(i);
              else
                sender.Send(i.ToString());
            }
          }
          Thread.Sleep(1000);
          for (int i = 0; i < AsyncMessageCount; i++) {
            Assert.IsTrue(asyncResultArray[i]);
          }

        }
      }
    }

    private void LocalReceiverMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      int index = 0;
      if (e.Message is string) {
        index = int.Parse((string)e.Message);
        Assert.IsTrue(index%2==1);
      }
      else if (e.Message is int) {
        index = (int)e.Message;
        Assert.IsTrue(index%2==0);
      }
      else {
        Assert.Fail("Wrong message received");        
      }
      asyncResultArray[index] = true;
    }


    [Test]
    public void TestAutomaticSend()
    {
      using (Receiver receiver = new Receiver(RemoteEndPoint)) {
        receiver.AddProcessor(typeof (BaseProcessor),true);
        receiver.StartReceive();
        using (Sender sender = new Sender(RemoteEndPoint)) {
          ConfirmResponseMessage response = (ConfirmResponseMessage)sender.Ask(new AppendFileQueryMessage());
        }
      }
    }

    [Test]
    [ExpectedException(typeof (TimeoutException))]
    public void TestManualNoDefault()
    {
      using (Receiver receiver = new Receiver(RemoteEndPoint)) {
        receiver.AddProcessor(typeof (AppendFileProcessor));
        receiver.StartReceive();
        using (Sender sender = new Sender(RemoteEndPoint)) {
          sender.ResponseTimeout = TimeSpan.FromSeconds(1);
          object response = sender.Ask("");
        }
      }
    }

    [Test]
    public void TestManualSend()
    {
      using (Receiver receiver = new Receiver(RemoteEndPoint)) {
        receiver.AddProcessor(typeof (AppendFileProcessor));
        receiver.StartReceive();
        using (Sender sender = new Sender(RemoteEndPoint)) {
          ConfirmResponseMessage response = (ConfirmResponseMessage)sender.Ask(new AppendFileQueryMessage());
        }
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Unknown message")]
    public void TestManualDefault()
    {
      using (Receiver receiver = new Receiver(RemoteEndPoint)) {
        receiver.AddProcessor(typeof (DefaultProcessor));
        receiver.StartReceive();
        using (Sender sender = new Sender(RemoteEndPoint)) {
          object response = sender.Ask(new AppendFileQueryMessage());
        }
      }
    }


    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Processor type should implement IMessageProcessor interface.")]
    public void TestBaseProcessorType()
    {
      using (Receiver receiver = new Receiver(RemoteEndPoint)) {
        receiver.AddProcessor(typeof (Exception));
      }
    }
  }
}