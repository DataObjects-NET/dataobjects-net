using System;

namespace Xtensive.Messaging.Tests.Console
{
  class Program
  {
    private const string remoteEndPoint = "tcp://localhost:3333/";
    private const string localEndPoint = "tcp://localhost:4444/";

    static void Main(string[] args)
    {
      MessagingTest mt = new MessagingTest();
      // mt.TestConnectionBroken();
      System.Console.WriteLine("Press any key to exit.");
      System.Console.ReadKey();
      /*
      System.Console.WriteLine("Begin Test");
      System.Console.ReadKey();
      Receiver receiver = new Receiver(remoteEndPoint);
      Sender sender = new Sender(remoteEndPoint);
      receiver = null;
      sender = null;
      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();
      System.Console.WriteLine("Press any key to exit.");
      System.Console.ReadKey();
       */
    }
  }
}
