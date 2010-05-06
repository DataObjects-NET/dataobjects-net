// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.24

using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0648_MultiThreadNullReference_Model;

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          new Simple();
          new Simple();
          new Simple();
          new Simple();
          new Simple();

          for (int i = 0; i < 10; i++) {
            ThreadPool.QueueUserWorkItem(state => {
                                             using (var session = Session.Open(Domain))
                                             using (var t = Transaction.Open()) {
                                               var count = Query.All<Simple>().Count();
                                             }
                                           });
          }

          // Committing transaction
          transactionScope.Complete();
        }
      }
    }
  }
}