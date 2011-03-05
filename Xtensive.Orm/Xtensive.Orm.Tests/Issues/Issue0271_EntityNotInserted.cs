// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.07.07

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0271_Model;


namespace Xtensive.Orm.Tests.Issues.Issue0271_Model
{
    [Serializable]
    [HierarchyRoot]
    public class Address : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [Serializable]
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

    [Serializable]
    [HierarchyRoot]
    public class Account : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public User User { get; set; }
    }
}
namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0271_EntityNotInserted : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof (Issue0271_Model.Address).Namespace);
      return config;
    }

    [Test]
    public void EntityNotInsertedTest()
    {
      using (var session = Domain.OpenSession())
      using (TransactionScope t = session.OpenTransaction()) {
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