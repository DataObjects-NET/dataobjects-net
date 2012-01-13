// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.11.18

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0202_OnRemoveActionOverriding_Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0202_OnRemoveActionOverriding_Model
{
  public interface IBaseEntity : IEntity
  {
    [Field(Nullable = false)]
    Linked Entity { get; set; }
  }

  [HierarchyRoot]
  public class Linked2 : Entity, IBaseEntity
  {
    public Linked2(Guid id)
      : base(id)
    {
    }

    [Field, Key]
    public Guid Id { get; private set; }

    [Association(OnTargetRemove = OnRemoveAction.Cascade)]
    public Linked Entity { get; set; }
  }

  [HierarchyRoot]
  public class Linked : Entity
  {
    public Linked(Guid id)
      : base(id)
    {
    }

    [Field, Key]
    public Guid Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [Ignore]
  public class IssueJira0202_OnRemoveActionOverriding : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Linked).Assembly, typeof (Linked).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var l = new Linked(Guid.NewGuid());
          var l2 = new Linked2(Guid.NewGuid()) {Entity = l};

          l.Remove();
          // Rollback
        }
      }
    }
  }
}