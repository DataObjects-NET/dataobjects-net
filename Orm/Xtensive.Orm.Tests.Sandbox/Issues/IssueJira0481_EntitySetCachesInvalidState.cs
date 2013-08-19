// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.17

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0481_EntitySetCachesInvalidStateModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0481_EntitySetCachesInvalidStateModel
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

  [TestFixture]
  public class IssueJira0481_EntitySetCachesInvalidState : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Owner).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var owner = new Owner();
        session.SaveChanges();
        using (session.DisableSaveChanges()) {
          var item = new Item();
          owner.Items.Add(item);
          var dummy = owner.Items.ToList();
        }
        session.SaveChanges();
        var items = owner.Items.ToList();
        Assert.That(items.Count, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}