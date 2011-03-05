// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.10.28

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Issues.Issue0722_NestedTrancationsAndMemoryProvider_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0722_NestedTrancationsAndMemoryProvider_Model
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0722_NestedTrancationsAndMemoryProvider : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = new DomainConfiguration("memory://localhost/DO40-Tests");
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      config.Sessions.Add(new SessionConfiguration(WellKnown.Sessions.Default, SessionOptions.LegacyProfile));
      return config;
    }

    [Test]
    public void CommitTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        using (var tx = session.OpenTransaction(TransactionOpenMode.New)) {
          new MyEntity();
          tx.Complete();
        }
        ts.Complete();
      }
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void RollbackTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        using (var tx = session.OpenTransaction(TransactionOpenMode.New)) {
          new MyEntity();
          // tx.Complete();
        }
        ts.Complete();
      }
    }

    [Test]
    public void DisconnectedStateTest()
    {
      using (var session = Domain.OpenSession()) {
        var ds = new DisconnectedState();
        using (ds.Attach(session)) {
          new MyEntity();
          ds.ApplyChanges();
        }
      }
    }
  }
}