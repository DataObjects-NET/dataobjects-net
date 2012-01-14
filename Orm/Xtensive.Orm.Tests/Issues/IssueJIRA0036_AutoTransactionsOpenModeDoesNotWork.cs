// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.03.14

using System;
using System.ComponentModel;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Sandbox.Issues.IssueJIRA0036_AutoTransactionsOpenModeDoesNotWork_Model;

namespace Xtensive.Orm.Tests.Sandbox.Issues
{
  namespace IssueJIRA0036_AutoTransactionsOpenModeDoesNotWork_Model
  {
    [HierarchyRoot]
    public class Firm : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      [Association(PairTo = "Firm", OnTargetRemove = OnRemoveAction.Cascade, OnOwnerRemove = OnRemoveAction.None)]
      public Contractor Contractor { get; private set; }

      public Firm()
        : this(new Contractor())
      {}

      public Firm(Contractor contractor)
      {
        Contractor = contractor;
      }
    }

    [HierarchyRoot]
    public class Contractor : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public Firm Firm { get; private set; }
    }

  }

  [Serializable]
  public class IssueJIRA0036_AutoTransactionsOpenModeDoesNotWork : AutoBuildTest
  {
    private Session session;
    private SessionScope scope;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Firm).Assembly, typeof (Firm).Namespace);
      config.Sessions.Default.Options = SessionOptions.ClientProfile | SessionOptions.ReadRemovedObjects;
      return config;
    }

    [SetUp]
    public void SetUp()
    {
      session = Domain.OpenSession();
      scope = session.Activate();
    }

    [TearDown]
    public void TearDown()
    {
      scope.Dispose();
      session.Dispose();
    }

    [Test]
    public void MainTest()
    {
      CreateContractors();
      session.SaveChanges();
    }

    [Transactional]
    public static void CreateContractors()
    {
      var contractor = new Contractor();
      var firm = new Firm(contractor);
      var f = new Firm(); //тут происходит ошибка
    }
  }
}