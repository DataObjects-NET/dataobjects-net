// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using System.Linq;
using System.Threading;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Tests;
using Xtensive.Reflection;

namespace Xtensive.Orm.Manual.Concurrency.Locking
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Counter : Entity
  {
    [Key, Field]
    public string Name { get; private set; }

    [Field]
    public int Value { get; set; }

    public Counter(Session session, string name)
      : base(session, name)
    {}
  }

  #endregion
  
  [TestFixture]
  public class LockingTest
  {
    public enum LockingMode 
    {
      None,
      EntityLock,
      QueryLock
    }

    private const string LockingTestName = "LockingTest";
    private const int TestTime = 5000;
    private Domain existingDomain;
    private volatile LockingMode lockingMode;
    private volatile IsolationLevel isolationLevel;
    private long threadCount;
    private Key counterKey;


    [Test]
    public void CombinedTest()
    {
      lockingMode = LockingMode.None;
      isolationLevel = IsolationLevel.ReadCommitted;
      Run("Counter isn't isolated.");

      isolationLevel = IsolationLevel.RepeatableRead;
      Run("There are deadlocks or version conflicts, but counter is isolated.");

      lockingMode = LockingMode.EntityLock;
      Run("Still can be a deadlock, since lock happen after read.");

      lockingMode = LockingMode.QueryLock;
      Run("No deadlocks, counter isolation.");
    }

    private void Run(string remark)
    {
      using (var session = GetDomain().OpenSession())
      using (var tx = session.OpenTransaction()) {
        Counter counter;
        if (counterKey==null) {
          counter = new Counter(session, LockingTestName);
          counterKey = counter.Key;
        }
        else
          counter = session.Query.Single<Counter>(counterKey);
        counter.Value = 0;
        tx.Complete();
      }

      Console.WriteLine($"{LockingTestName}, LockingMode={lockingMode}, IsolationLevel = {isolationLevel}");
      Console.WriteLine($"{remark}");
      ThreadPool.QueueUserWorkItem(TestThread, 500);
      ThreadPool.QueueUserWorkItem(TestThread, 333);
      WaitForCompletion();

      using (var session = GetDomain().OpenSession())
      using (var tx = session.OpenTransaction()) {
        var counter = session.Query.Single<Counter>(counterKey);
        Console.WriteLine($"Final counter.Value = {counter.Value}");
        tx.Complete();
      }

      Console.WriteLine();
    }

    private void TestThread(object state)
    {
      int delay = (int) state;
      var startTime = DateTime.UtcNow;
      var threadNumber = Interlocked.Increment(ref threadCount);
      try {
        string threadName = $"Thread {threadNumber}".Indent(2 + (int) (threadNumber - 1) * 5);
        using (var session = GetDomain().OpenSession()) {
          while ((DateTime.UtcNow - startTime).TotalMilliseconds < TestTime) {
            try {
              Console.WriteLine($"{threadName}: beginning of transaction");
              using (var tx = session.OpenTransaction(isolationLevel)) {
                Counter counter;
                if (lockingMode!=LockingMode.QueryLock) {
                  Console.WriteLine($"{threadName}:   reading shared counter");
                  counter = session.Query.Single<Counter>(counterKey);
                  if (lockingMode==LockingMode.EntityLock) {
                    Console.WriteLine($"{threadName}:   locking counter");
                    counter.Lock(LockMode.Exclusive, LockBehavior.Wait);
                    Console.WriteLine($"{threadName}:   counter is locked");
                  }
                }
                else {
                  Console.WriteLine($"{threadName}:   reading & locking shared counter");
                  counter = session.Query.All<Counter>()
                    .Where(c => c.Key==counterKey)
                    .Lock(LockMode.Exclusive, LockBehavior.Wait)
                    .Single();
                }
                Console.WriteLine($"{threadName}:   delay ({delay}ms)");
                Thread.Sleep(delay);
                Console.WriteLine($"{threadName}:   incrementing counter");
                counter.Value++;
                Console.WriteLine($"{threadName}:   counter.Value = {counter.Value}");
                Console.WriteLine($"{threadName}:   committing transaction");
                tx.Complete();
              }
              Console.WriteLine($"{threadName}: transaction is committed");
            }
            catch (Exception e) {
              Console.WriteLine($"{threadName}:   error: {e.GetType().GetShortName()}");
              Console.WriteLine($"{threadName}: transaction is rolled back");
            }
          }
        }
      }
      finally {
        Interlocked.Decrement(ref threadCount);
      }
    }

    private void WaitForCompletion()
    {
      Thread.Sleep(500);
      while (Interlocked.Read(ref threadCount) > 0)
        Thread.Sleep(100);
    }

    private Domain GetDomain()
    {
      if (existingDomain==null) {
        var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
        config.UpgradeMode = DomainUpgradeMode.Recreate;
        config.Sessions.Add(new SessionConfiguration("Default") {
          DefaultIsolationLevel = IsolationLevel.Serializable
        });
        config.Types.Register(typeof (Counter).Assembly, typeof (Counter).Namespace);
        var domain = Domain.Build(config);
        existingDomain = domain;
      }
      return existingDomain;
    }
  }
}