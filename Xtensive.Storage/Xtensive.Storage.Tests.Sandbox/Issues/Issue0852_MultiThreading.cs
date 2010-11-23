// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2010.10.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Issues.Issue0852_MultiThreading_Model1
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

namespace Xtensive.Storage.Tests.Issues.Issue0852_MultiThreading_Model2
{
  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0852_MultiThreading : AutoBuildTest
  {
    private const int threadCount = 1;
    private const int entityCount = 20;

    private static Key[] keys = new Key[entityCount];

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Issue0852_MultiThreading_Model1.BaseEntity).Assembly, typeof (Issue0852_MultiThreading_Model1.BaseEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var tx = Transaction.Open()) {
          for (int i = 0; i < entityCount; ++i) {
            var entity = new Issue0852_MultiThreading_Model1.UniqueTextEntity {Text = "Test" + i};
            keys[i] = entity.Key;
          }
          tx.Complete();
        }
      }

      var threads = new Thread[threadCount];

      for (int i = 0; i < threadCount; i++) {
        var thread = new Thread(ThreadFunction);
        threads[i] = thread;
        thread.Start();
      }

      Thread.Sleep(20 * 1000);

      Console.WriteLine("Aborting the threads...");
      for (int i = 0; i < threadCount; i++) {
        var thread = threads[i];
        thread.Abort();
        thread.Join();
      }
      Console.WriteLine("Done.");


      RebuildDomain();
    }

    private void ThreadFunction()
    {
      string entityText = null;

      using (Session.Open(Domain)) {
        var rnd = new Random();
        while (true) {
          using (var tx = Transaction.Open()) {
            foreach (var key in keys) {
              var entity = Query.SingleOrDefault<Issue0852_MultiThreading_Model1.UniqueTextEntity>(key);
            }
            tx.Complete();
          }
        }
      }
    }
  }
}