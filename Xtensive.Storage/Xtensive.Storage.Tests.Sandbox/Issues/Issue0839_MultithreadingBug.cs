// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2010.10.14

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0839_MultithreadingBug_Model;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Issues.Issue0839_MultithreadingBug_Model
{
  [Serializable]
  [HierarchyRoot]
  [Index("NextRun")]
  public class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime NextRun { get; set; }
  }

  [Serializable]
  [Index("Text", Unique = true)]
  public class UniqueTextEntity : BaseEntity
  {
    [Field]
    public string Text { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0839_MultithreadingBug : AutoBuildTest
  {
    private const int threadCount = 2;
    private const int entityCount = 20;
    private const int readCount = 20;

    private static object exceptionLock = new object();
    private static int exceptionCount = 0;
    private static Key[] keys = new Key[entityCount];

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BaseEntity).Assembly, typeof(BaseEntity).Namespace);
      return config;
    }

    [Test]
    [Ignore]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var tx = Transaction.Open()) {
          for (int i = 0; i < entityCount; ++i) {
            var entity = new UniqueTextEntity { Text = "Test" + i };
            keys[i] = entity.Key;
          }
          tx.Complete();
        }
      }

      var threads = new Thread[threadCount];

      Console.WriteLine("Starting {0} threads...", threadCount);
      for (int i = 0; i<threadCount; i++) {
        var thread = new Thread(ThreadFunction);
        threads[i] = thread;
        thread.Start();
      }
      Console.WriteLine("Done.");

      Thread.Sleep(20 * 1000);

      Console.WriteLine("Aborting the threads...");
      for (int i = 0; i<threadCount; i++) {
        var thread = threads[i];
        thread.Abort();
        thread.Join();
      }
      Console.WriteLine("Done.");

      Assert.AreEqual(0, exceptionCount);
    }

    private void ThreadFunction()
    {
      string entityText = null;

      using (Session.Open(Domain)) {
        var rnd = new Random();
        while (true) {
          try {
            using (var tx = Transaction.Open()) {
              // Prefetching the whole set of entities
              foreach (var key in keys) {
                var entity = Query.SingleOrDefault<UniqueTextEntity>(key);
              }
              tx.Complete();
            }
          }
          catch (Exception error) {
            var currentError = error;
            bool notImportantException = false;
            while (currentError!=null) {
              if (currentError is ThreadAbortException)
                return;
              notImportantException =
                currentError is ReprocessableException ||
                currentError is OperationTimeoutException ||
                currentError is UniqueConstraintViolationException ||
                (currentError is StorageException && currentError.Message.Contains("This SqlTransaction has completed"));
              if (notImportantException)
                break;
              currentError = currentError.InnerException;
            }
            if (notImportantException)
              continue;

            lock (exceptionLock) {
              ++exceptionCount;
              Console.WriteLine("Exception #{0}:", exceptionCount);
              Console.WriteLine("{0}", error.ToString().Indent(2));
              Console.WriteLine();
            }
          }
        }
      }
    }
  }
}