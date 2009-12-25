using System;
using System.Threading;

namespace Xtensive.Messaging.Tests.Client
{
  class Program
  {
    private static long messageCount;
    private static long previousMessageCount;
    private static long droppedCount;
    private static bool stopThreads;
    private static Thread statisticsTread = new Thread(Statistics);
    private static Thread[] workerTreads;
    private static int threadsCount = 5;

    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("Please provide URL of server.");
        return;
      }
      if (args.Length > 1)
        int.TryParse(args[1], out threadsCount);
      Console.WriteLine("Press any key to stop ask server.");
      statisticsTread.Start();
      workerTreads = new Thread[threadsCount];
      for (int i=0; i< threadsCount; i++){
        workerTreads[i] = new Thread(Worker);
        workerTreads[i].Start(args[0]);
      }
      Console.ReadKey();
      stopThreads = true;
      foreach (Thread thread in workerTreads)
        thread.Join();
      statisticsTread.Join();
    }

    static void Worker(object url)
    {
      using (Sender sender = new Sender((string)url)) {
        sender.ResponseTimeout = TimeSpan.FromSeconds(10);
        while (!stopThreads)
        {
          try {
            if (sender.Ask(new byte[100]) == null)
              Interlocked.Increment(ref droppedCount);
            else
              Interlocked.Increment(ref messageCount);
          }
          catch (Exception ex) {
            Console.WriteLine(ex);
            stopThreads = true;
            Thread.Sleep(1200); // To let statistics gather information.
            Console.WriteLine("Application aborted. Press any key to exit.");
          }
        }
      }
    }

    static void Statistics()
    {
      int seconds = 0;
      while (!stopThreads)
      {
        Thread.Sleep(1000);
        long messages = Interlocked.Read(ref messageCount);
        Console.WriteLine("Seconds: {0}, messages: {1}, dropped: {2}, msg/sec: {3}", seconds++, messages, Interlocked.Read(ref droppedCount), messages - previousMessageCount);
        previousMessageCount = messages;
      }
    }

  }
}
