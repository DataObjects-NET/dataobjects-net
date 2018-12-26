// Copyright (C) 2018 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2018.10.15

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.RemoveReasonEntityRemovingSessionEventTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class RemoveReasonOnEntityRemovingEventTest : AutoBuildTest
  {
    [Test]
    public void NoReferenceEntityRemovalTest()
    {
      TestEntityRemove(
        session => new ReferencedEntity1().Remove(),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
    }

    [Test]
    public void ZeroToOneRemovalTest()
    {
      Action<Session> removeAction = session =>  new ReferencingEntity1 {Ref = new ReferencedEntity1()}.Remove();

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencingEntity1 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(2));
    }

    [Test]
    public void ZeroToManyRemovalTest()
    {
      Action<Session> removeAction = session =>  new ReferencingEntity2 {Items = {new ReferencedEntity2(), new ReferencedEntity2()}}.Remove();

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencingEntity2 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(5));
    }

    [Test]
    public void OneToOneRemovalTest()
    {
      Action<Session> removeAction = session => new ReferencingEntity3 {Value = new ReferencedEntity3()}.Remove();

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencingEntity3 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(2));
    }

    [Test]
    public void OneToManyRemovalTest1()
    {
      Action<Session> removeAction = session => {
        var reference = new ReferencingEntity4();
        new ReferencedEntity4 {Items = {reference}};
        reference.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencingEntity4 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(2));
    }

    [Test]
    public void OneToManyRemovalTest2()
    {
      Action<Session> removeAction = session => {
        var reference = new ReferencingEntity5();
        new ReferencedEntity5 {Items = {reference}}.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencedEntity5 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      TestEntityRemove(removeAction, handler);
    }

    [Test]
    public void ManyToManyRemovalTest1()
    {
      Action<Session> removeAction = session => {
        var reference1 = new ReferencingEntity6();
        var reference2 = new ReferencingEntity6();
        new ReferencedEntity6 {Items = {reference1, reference2}}.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencedEntity6 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(5));
    }

    [Test]
    public void ManyToManyRemovalTest2()
    {
      Action<Session> removeAction = session => {
        var reference1 = new ReferencedEntity7();
        var reference2 = new ReferencedEntity7();
        new ReferencingEntity7 {Items = {reference1, reference2}}.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencingEntity7 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(5));
    }

    [Test]
    public void DeepCascadeRemovalTest1()
    {
      Action<Session> removeAction = session => {
        var mid1 = new MidNode1 {Items = {new LeafNode1(), new LeafNode1()}};
        var mid2 = new MidNode1 {Items = {new LeafNode1(), new LeafNode1()}};
        var root = new RootNode1();
        root.Items.Add(mid1);
        root.Items.Add(mid2);

        root.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is RootNode1 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(13));
    }

    [Test]
    public void DeepCascadeRemovalTest2()
    {
      Action<Session> removeAction = session => {
        var mid1 = new MidNode2 {Items = {new LeafNode2(), new LeafNode2()}};
        var mid2 = new MidNode2 {Items = {new LeafNode2(), new LeafNode2()}};
        var root = new RootNode2();
        root.Items.Add(mid1);
        root.Items.Add(mid2);

        root.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is RootNode2 ? EntityRemoveReason.User : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(7));
    }

    [Test]
    public void DeepCascadeRemovalTest3()
    {
      Action<Session> removeAction = session => {
        var leaf = new LeafNode3();
        var mid1 = new MidNode3 {Items = {leaf, new LeafNode3()}};
        var mid2 = new MidNode3 {Items = {new LeafNode3(), new LeafNode3()}};
        var root = new RootNode3();
        root.Items.Add(mid1);
        root.Items.Add(mid2);
        session.Extensions.Set(leaf.Key);
        leaf.Remove();
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var removedEntityKey = ((SessionEventAccessor) s).Session.Extensions.Get<Key>();
        var expectedReason = e.Entity is LeafNode3 && e.Entity.Key==removedEntityKey
          ? EntityRemoveReason.User
          : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(7));
    }

    [Test]
    public void EntitySetItemRemovalTest1()
    {
      Action<Session> removeAction = session => {
        var referenced = new ReferencedEntity2();
        var referencing = new ReferencingEntity2 {Items = {new ReferencedEntity2(), referenced}};
        referencing.Items.Remove(referenced);
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencedEntity2 || e.Entity is ReferencingEntity2
          ? EntityRemoveReason.User
          : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(1));
    }

    [Test]
    public void EntitySetItemRemovalTest2()
    {
      Action<Session> removeAction = session => {
        var referencing = new ReferencingEntity4();
        var referenced = new ReferencedEntity4 {Items = {referencing}};
        referenced.Items.Remove(referencing);
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        // no entity should be removed
        throw new Exception();
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(0));
    }

    [Test]
    public void EntitySetItemRemovalTest3()
    {
      Action<Session> removeAction = session => {
        var referencing = new ReferencingEntity5();
        var referenced = new ReferencedEntity5 {Items = {referencing}};
        referenced.Items.Remove(referencing);
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        // no entity should be removed
        throw new Exception();
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(0));
    }

    [Test]
    public void EntitySetItemRemovalTest5()
    {
      Action<Session> removeAction = session => {
        var reference1 = new ReferencingEntity6();
        var reference2 = new ReferencingEntity6();
        new ReferencedEntity6 {Items = {reference1, reference2}}.Items.Remove(reference1);
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencedEntity6 || e.Entity is ReferencingEntity6
          ? EntityRemoveReason.User
          : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(1));
    }

    [Test]
    public void EntitySetItemRemovalTest6()
    {
      Action<Session> removeAction = session => {
        var reference1 = new ReferencingEntity7();
        var reference2 = new ReferencingEntity7();
        new ReferencedEntity7 { Items = { reference1, reference2 } }.Items.Remove(reference1);
      };

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        var expectedReason = e.Entity is ReferencedEntity7 || e.Entity is ReferencingEntity7
          ? EntityRemoveReason.User
          : EntityRemoveReason.Association;
        Assert.That(e.Reason, Is.EqualTo(expectedReason));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(1));
    }

    [Test]
    public void DirectPersistentAccessorTest()
    {
      Action<Session> removeAction = session => session.Services.Get<DirectPersistentAccessor>().Remove(new ReferencingEntity1());

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(1));
    }

    [Test]
    public void SessionRemoveTest()
    {
      Action<Session> removeAction = session => session.Remove(new[] {new ReferencingEntity1()});

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(1));
    }

    [Test]
    public void RemoveLaterTest()
    {
      Action<Session> removeAction = session => new ReferencingEntity1().RemoveLater();

      EventHandler<EntityRemovingEventArgs> handler = (s, e) => {
        Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var affectedEntities = TestEntityRemove(removeAction, handler);
      Assert.That(affectedEntities, Is.EqualTo(1));
    }

    private int TestEntityRemove(
      Action<Session> action,
      EventHandler<EntityRemovingEventArgs> entityRemoving = null)
    {
      var removedEntities = new List<Entity>();
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.SystemEvents.EntityRemoving += (s, e) => {
          removedEntities.Add(e.Entity);
          if (entityRemoving != null)
            entityRemoving(s, e);
        };

        action(session);
      }

      return removedEntities.Distinct().Count();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (ReferencingEntity1).Assembly, typeof (ReferencingEntity1).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }

  namespace RemoveReasonEntityRemovingSessionEventTestModel
  {
    // zero-to-one
    [HierarchyRoot]
    public class ReferencedEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }

    [HierarchyRoot]
    public class ReferencingEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public ReferencedEntity1 Ref { get; set; }
    }

    //zero-to-many
    [HierarchyRoot]
    public class ReferencingEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<ReferencedEntity2> Items { get; set; }
    }

    [HierarchyRoot]
    public class ReferencedEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }

    // one-to-one
    [HierarchyRoot]
    public class ReferencedEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public ReferencingEntity3 Value { get; set; }
    }

    [HierarchyRoot]
    public class ReferencingEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Value")]
      public ReferencedEntity3 Value { get; set; }
    }

    //one-to-many
    [HierarchyRoot]
    public class ReferencedEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntitySet<ReferencingEntity4> Items { get; private set; }
    }

    [HierarchyRoot]
    public class ReferencingEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Items")]
      public ReferencedEntity4 Value { get; set; }
    }

    [HierarchyRoot]
    public class ReferencedEntity5 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Value")]
      public EntitySet<ReferencingEntity5> Items { get; private set; }
    }

    [HierarchyRoot]
    public class ReferencingEntity5 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public ReferencedEntity5 Value { get; set; }
    }


    //many-to-many
    [HierarchyRoot]
    public class ReferencedEntity6 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Items")]
      public EntitySet<ReferencingEntity6> Items { get; private set; }
    }

    [HierarchyRoot]
    public class ReferencingEntity6 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntitySet<ReferencedEntity6> Items { get; set; }
    }

    [HierarchyRoot]
    public class ReferencedEntity7 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntitySet<ReferencingEntity7> Items { get; private set; }
    }

    [HierarchyRoot]
    public class ReferencingEntity7 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Items")]
      public EntitySet<ReferencedEntity7> Items { get; set; }
    }


    //muiti-level cascade
    [HierarchyRoot]
    public class RootNode1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<MidNode1> Items { get; set; }
    }

    [HierarchyRoot]
    public class MidNode1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<LeafNode1> Items { get; set; }
    }

    [HierarchyRoot]
    public class LeafNode1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }


    [HierarchyRoot]
    public class RootNode2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(PairTo = "Parent", OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<MidNode2> Items { get; set; }
    }

    [HierarchyRoot]
    public class MidNode2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public RootNode2 Parent { get; set; }

      [Field]
      [Association(PairTo= "Parent", OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<LeafNode2> Items { get; set; }
    }

    [HierarchyRoot]
    public class LeafNode2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }

      [Field]
      public MidNode2 Parent { get; set; }
    }

    [HierarchyRoot]
    public class RootNode3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<MidNode3> Items { get; set; }
    }

    [HierarchyRoot]
    public class MidNode3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Cascade)]
      public RootNode3 Parent { get; set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<LeafNode3> Items { get; set; }
    }

    [HierarchyRoot]
    public class LeafNode3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }

      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Cascade)]
      public MidNode3 Parent { get; set; }
    }
  }
}
