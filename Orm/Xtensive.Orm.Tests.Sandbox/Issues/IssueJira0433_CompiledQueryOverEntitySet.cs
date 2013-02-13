// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.02.13

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0433_CompiledQueryOverEntitySetModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0433_CompiledQueryOverEntitySetModel
  {
    [HierarchyRoot]
    public class Owner : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity
    {
      [Key, Field]
      public long Id { get; private set; }
    }
  }

  public class IssueJira0433_CompiledQueryOverEntitySet : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx =session.OpenTransaction()) {
        var owner1 = new Owner();
        var owner2 = new Owner();
        owner1.Items.Add(new Item());
        owner1.Items.Add(new Item());

        Assert.That(HasItems(session, owner1));
        Assert.That(HasItems(session, owner2), Is.False);
      }
    }

    public bool HasItems(Session session, Owner owner)
    {
      return session.Query.Execute(q => owner.Items.Any());
    }
  }
}