// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.07.07

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Tests.Issues.Issue0271_Model;


namespace Xtensive.Storage.Tests.Issues.Issue0271_Model
{
    [HierarchyRoot]
    public class Address : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    public class User : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public Address Address { get; set; }

      [Field, Association("User", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Deny)]
      public Account Account { get; set; }
    }

    [HierarchyRoot]
    public class Account : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public User User { get; set; }
    }
}
namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0271_EntityNotInserted : AutoBuildTest
  {

    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Issue0271_Model.Address).Namespace);
      return config;
    }

    [Test]
    public void EntityNotInsertedTest()
    {
      using (Session.Open(Domain))
      using (TransactionScope t = Transaction.Open()) {
        var a = new Address();
        var u = new User();
        var ac = new Account();
        u.Address = a;
        u.Account = ac;

        t.Complete();
      }
    }
  }
}