using System;
using System.Threading;

namespace Xtensive.Messaging.Tests.Server
{
  class Program
  {
    private static long messageCount;
    private static long previousMessageCount;
    private static bool stopStatistics;
    private static Thread statisticsTread = new Thread(Statistics);
    private static Receiver receiver;
    static void Main(string[] args)
    {
      if (args.Length < 1) {
        Console.WriteLine("Please provide URL for listener in command line.");
        return;
      }
      Console.WriteLine("Press any key to stop listener.");
      using (receiver = new Receiver(args[0])) {
        receiver.MessageReceived += MessageReceived;
        statisticsTread.Start();
        Console.ReadKey();
        stopStatistics = true;
        statisticsTread.Join();
      }
    }

    static void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
      Interlocked.Increment(ref messageCount);
      if (e.Sender!=null)
        e.Sender.Send(new byte[250]);
    }

    static void Statistics()
    {
      int seconds = 0;
      while (!stopStatistics) {
        Thread.Sleep(1000);
        long messages = Interlocked.Read(ref messageCount);
        Console.WriteLine("Seconds: {0}, connections: {1} messages: {2}, msg/sec: {3}", seconds++, receiver.ConnectionCount, messages, messages - previousMessageCount);
        previousMessageCount = messages;
      }
      
    }

  }
}
