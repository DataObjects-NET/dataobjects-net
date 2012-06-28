using System;
using System.Threading;
using NUnit.Framework;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class LockerTest
  {
    public class ThreadSafeWrapper<T>
    {
      private T value;
      private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
      private string indent = "  ";
      private Random random = new Random();

      // Already thread-safe
      public T Value {
        get {
          T tmpValue = default(T);
          string threadName = Thread.CurrentThread.Name;
          Indexing.Log.Info("{0}{1} waits for read lock...", indent, threadName);
          _lock.ExecuteReader(delegate {
            string oldIndent = indent;
            indent = oldIndent + "  ";
            tmpValue = value;
            Indexing.Log.Info("{0}{1} reads {2}.", indent, threadName, tmpValue);
            int toSleep = random.Next(5);
            Thread.Sleep(toSleep);
            toSleep = random.Next(5);
            Indexing.Log.Info("{0}{1} suspends for {2}...", indent, threadName, toSleep);
            _lock.ExecuteSuspender(delegate { Thread.Sleep(toSleep); });
            Indexing.Log.Info("{0}{1} continues.", indent, threadName);
            if (indent.Length>=2)
              indent = indent.Substring(0, indent.Length-2);
            Indexing.Log.Info("{0}{1} has released read lock.", indent, threadName, tmpValue);
          });
          return tmpValue;
        }
        set {
          string threadName = Thread.CurrentThread.Name;
          Indexing.Log.Info("{0}{1} waits for write lock...", indent, threadName);
          Thread.Sleep(10);
          _lock.ExecuteWriter(delegate
          {
            if (indent.IndexOf('*')>=0) {
              failed = true;
              throw new ApplicationException("float writer lock rule is violated.");
            }
            string oldIndent = indent;
            indent = oldIndent + "* ";
            this.value = value;
            Indexing.Log.Info("{0}{1} writes {2}.", indent, threadName, value);
            int toSleep = random.Next(20);
            Thread.Sleep(toSleep);
            toSleep = random.Next(20);
            indent = oldIndent + "  ";
            Indexing.Log.Info("{0}{1} suspends for {2}...", indent, threadName, toSleep);
            _lock.ExecuteSuspender(delegate { Thread.Sleep(toSleep); });
            Indexing.Log.Info("{0}{1} continues.", indent, threadName);
            if (indent.Length>=2)
              indent = indent.Substring(0, indent.Length-2);
            Indexing.Log.Info("{0}{1} has released write lock.", indent, threadName);
          });
        }
      }
    }


    public class ThreadSafeWrapper1<T>
    {
      private T value = default(T);
      private string indent = "  ";
      private Random random = new Random();

      // Alreary thread-safe
      public T Value
      {
        get
        {
          T tmpValue = default(T);
          string threadName = Thread.CurrentThread.Name;
          Indexing.Log.Info("{0}{1} waits for read lock...", indent, threadName);
          Locker.Execute(this, delegate
          {
            string oldIndent = indent;
            indent = oldIndent + "  ";
            tmpValue = value;
            Indexing.Log.Info("{0}{1} reads {2}.", indent, threadName, tmpValue);
            int toSleep = random.Next(5);
            Thread.Sleep(toSleep);
            toSleep = random.Next(5);
            Indexing.Log.Info("{0}{1} suspends for {2}...", indent, threadName, toSleep);
            Monitor.Wait(this, toSleep); 
            Indexing.Log.Info("{0}{1} continues.", indent, threadName);
            if (indent.Length >= 2)
              indent = indent.Substring(0, indent.Length - 2);
            Indexing.Log.Info("{0}{1} has released read lock.", indent, threadName, tmpValue);
          });
          return tmpValue;
        }
        set
        {
          string threadName = Thread.CurrentThread.Name;
          Indexing.Log.Info("{0}{1} waits for write lock...", indent, threadName);
          Thread.Sleep(10);
          Locker.Execute(this, delegate
          {
            if (indent.IndexOf('*') >= 0) {
              failed = true;
              throw new ApplicationException("float writer lock rule is violated.");
            }
            string oldIndent = indent;
            indent = oldIndent + "* ";
            this.value = value;
            Indexing.Log.Info("{0}{1} writes {2}.", indent, threadName, value);
            int toSleep = random.Next(20);
            Thread.Sleep(toSleep);
            toSleep = random.Next(20);
            indent = oldIndent + "  ";
            Indexing.Log.Info("{0}{1} suspends for {2}...", indent, threadName, toSleep);
            Indexing.Log.Info("{0}{1} continues.", indent, threadName);
            if (indent.Length >= 2)
              indent = indent.Substring(0, indent.Length - 2);
            Indexing.Log.Info("{0}{1} has released write lock.", indent, threadName);
          });
        }
      }
    }

    static ThreadSafeWrapper<int> safeInt = new ThreadSafeWrapper<int>();
    static bool running = false;
    static bool failed  = false;

    [Test]
    public void ThreadSafeWrapperTest()
    {
      running = true;
      int numThreads = 2; // No more then 25 (A..Z)
      Thread[] t = new Thread[numThreads];
      for (int i = 0; i < numThreads; i++) {
        t[i] = new Thread((ThreadStart)delegate {
          string threadName = Thread.CurrentThread.Name;
          Random random = new Random(threadName.GetHashCode());
          while (running) {
            int tmp;
            if (random.Next(2)==0) {
              tmp = random.Next(1000);
              Indexing.Log.Info("{0}: Value = {1}; // Trying...", threadName, tmp);
              safeInt.Value = tmp;
              Indexing.Log.Info("{0}: Value = {1}; // Done.", threadName, tmp);
            }
            else {
              Indexing.Log.Info("{0}: get Value; // Trying...", threadName);
              tmp = safeInt.Value;
              Indexing.Log.Info("{0}: get Value; // Done ({1}).", threadName, tmp);
            }
          }
        });
        t[i].Name = "Thread " + new string(Convert.ToChar(i + 65), 1);
        t[i].Start();
      }
      Thread.Sleep(3000);
      running = false;

      for (int i = 0; i < numThreads; i++)
        t[i].Join();

      Assert.IsFalse(failed);
    }

    static ThreadSafeWrapper1<int> safeInt1 = new ThreadSafeWrapper1<int>();
    [Test]
    public void ThreadSafeWrapper1Test()
    {
      running = true;
      int numThreads = 2; // No more then 25 (A..Z)
      Thread[] t = new Thread[numThreads];
      for (int i = 0; i < numThreads; i++) {
        t[i] = new Thread((ThreadStart)delegate
        {
          string threadName = Thread.CurrentThread.Name;
          Random random = new Random(threadName.GetHashCode());
          while (running) {
            int tmp;
            if (random.Next(2) == 0) {
              tmp = random.Next(1000);
              Indexing.Log.Info("{0}: Value = {1}; // Trying...", threadName, tmp);
              safeInt1.Value = tmp;
              Indexing.Log.Info("{0}: Value = {1}; // Done.", threadName, tmp);
            }
            else {
              Indexing.Log.Info("{0}: get Value; // Trying...", threadName);
              tmp = safeInt1.Value;
              Indexing.Log.Info("{0}: get Value; // Done ({1}).", threadName, tmp);
            }
          }
        });
        t[i].Name = "Thread " + new string(Convert.ToChar(i + 65), 1);
        t[i].Start();
      }
      Thread.Sleep(3000);
      running = false;

      for (int i = 0; i < numThreads; i++)
        t[i].Join();

      Assert.IsFalse(failed);
    }
  }
}
