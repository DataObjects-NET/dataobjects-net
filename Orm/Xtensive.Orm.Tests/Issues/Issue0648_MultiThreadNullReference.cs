// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0648_MultiThreadNullReference_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0648_MultiThreadNullReference_Model
  {
    [HierarchyRoot]
    public class Simple : Entity
    {
      [Key, Field]
      public int Id { get; private set; }
    }
  }

  [Serializable]
  public class Issue0648_MultiThreadNullReference : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Simple).Assembly, typeof (Simple).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      const int threadCount = 10;

      var completionEvents = new ManualResetEvent[threadCount];

      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          new Simple();
          new Simple();
          new Simple();
          new Simple();
          new Simple();

          for (int i = 0; i < threadCount; i++) {
            var completionEvent = completionEvents[i] = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(state => {
              using (var session2 = Domain.OpenSession())
              using (var t = session2.OpenTransaction()) {
                var count = session2.Query.All<Simple>().Count();
              }
              completionEvent.Set();
            });
          }

          // Committing transaction
          transactionScope.Complete();
        }
      }

      WaitHandle.WaitAll(completionEvents);

      foreach (var item in completionEvents)
        item.Close();
    }
  }
}