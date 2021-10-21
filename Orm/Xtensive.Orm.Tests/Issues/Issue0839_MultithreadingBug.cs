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
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0839_MultithreadingBug_Model;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.Issue0839_MultithreadingBug_Model
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
    [Field(Length = 100)]
    public string Text { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0839_MultithreadingBug : AutoBuildTest
  {
    private static bool stopThread = false;
    private const int threadCount = 2;
    private const int entityCount = 20;
    private const int readCount = 20;

    private static object exceptionLock = new object();
    private static int exceptionCount;
    private static Key[] keys = new Key[entityCount];

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BaseEntity).Assembly, typeof(BaseEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var tx = session.OpenTransaction()) {
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

      Thread.Sleep(TimeSpan.FromSeconds(5));

      stopThread = true;

      Console.WriteLine("Aborting the threads...");
      for (int i = 0; i<threadCount; i++) {
        var thread = threads[i];
        thread.Join();
      }
      Console.WriteLine("Done.");

      Assert.AreEqual(0, exceptionCount);
    }

    private void ThreadFunction()
    {
      using (var session = Domain.OpenSession()) {
        var rnd = new Random();
        while (!stopThread) {
          try {
            using (var tx = session.OpenTransaction()) {
              // Prefetching the whole set of entities
              foreach (var key in keys) {
                var entity = session.Query.SingleOrDefault<UniqueTextEntity>(key);
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