// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.24

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.CascadeRemoveTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace CascadeRemoveTestModel
  {
    [HierarchyRoot]
    public class Bank : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field, Association(PairTo = "Bank", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<BankBranch> Branches { get; private set; }
    }

    [HierarchyRoot]
    public class BankBranch : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public Bank Bank { get; set; }

      [Field, Association(PairTo = "Branch", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<BankAccount> Accounts { get; private set; }
    }

    [HierarchyRoot]
    public class BankAccount : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public BankBranch Branch { get; set; }
    }
  }

  [TestFixture]
  public class CascadeRemoveTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Bank).Assembly, typeof (Bank).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var bank = new Bank();
        var branch = new BankBranch();
        var account = new BankAccount();
        bank.Branches.Add(branch);
        branch.Accounts.Add(account);
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var bank = session.Query.All<Bank>().Single();
        bank.Remove();
        tx.Complete();
      }
    }
  }
}