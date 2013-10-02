// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.30

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0243_PrefetchSyntaxSugarModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0243_PrefetchSyntaxSugarModel
{
  [HierarchyRoot]
  public class Owner : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Target Target1 { get; set; }

    [Field]
    public Target Target2 { get; set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear, OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<Item> Items1 { get; private set; }

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear, OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<Item> Items2 { get; private set; }
  }

  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Target RelatedTarget { get; set; }
  }

  [HierarchyRoot]
  public class Target : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Target RelatedTarget { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0243_PrefetchSyntaxSugar : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Owner).Assembly, typeof (Target).Namespace);
      return configuration;
    }

    [Test]
    public void ShortcutTest()
    {
      Key target1Key;
      Key target2Key;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Reset();
        var owner = new Owner {Target1 = new Target(), Target2 = new Target()};
        target1Key = owner.Target1.Key;
        target2Key = owner.Target2.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // The following statement should prefetch Target1 and Target2
        var result = session.Query.All<Owner>()
          .Prefetch(x => new {x.Target1, x.Target2})
          .ToList();
        // Check that Target1 and Target2 are loaded into session.
        Assert.IsTrue(StorageTestHelper.IsFetched(session, target1Key));
        Assert.IsTrue(StorageTestHelper.IsFetched(session, target2Key));
      }
    }

    [Test]
    public void NestedShortcutTest()
    {
      Key target1Key;
      Key target2Key;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Reset();
        var target1 = new Target();
        var target2 = new Target();
        var owner = new Owner();
        owner.Items1.Add(new Item {RelatedTarget = target1});
        owner.Items1.Add(new Item {RelatedTarget = target2});
        target1Key = target1.Key;
        target2Key = target2.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // The following statement should prefetch Target1 and Target2
        var result = session.Query.All<Owner>()
          .Prefetch(x => x.Items1.Prefetch(y => new {y.RelatedTarget}))
          .ToList();
        // Check that Target1 and Target2 are loaded into session.
        Assert.IsTrue(StorageTestHelper.IsFetched(session, target1Key));
        Assert.IsTrue(StorageTestHelper.IsFetched(session, target2Key));
      }
    }

    [Test]
    public void NestingInShortcutTest()
    {
      Key target1Key;
      Key target2Key;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Reset();
        var target1 = new Target();
        var target2 = new Target();
        var owner = new Owner();
        owner.Items1.Add(new Item {RelatedTarget = target1});
        owner.Items1.Add(new Item {RelatedTarget = target2});
        target1Key = target1.Key;
        target2Key = target2.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        // The following statement should prefetch Target1 and Target2
        var result = session.Query.All<Owner>()
          .Prefetch(x => new {v = x.Items1.Prefetch(y => y.RelatedTarget)})
          .ToList();
        // Check that Target1 and Target2 are loaded into session.
        Assert.IsTrue(StorageTestHelper.IsFetched(session, target1Key));
        Assert.IsTrue(StorageTestHelper.IsFetched(session, target2Key));
      }
    }

    private void Reset()
    {
      Query.All<Item>().Remove();
      Query.All<Owner>().Remove();
      Query.All<Target>().Remove();
    }
  }
}