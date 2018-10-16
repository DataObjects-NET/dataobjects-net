using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.RemoveReasonEntityRemovingSessionEventTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class RemoveReasonEntityRemovingSessionEventTest : AutoBuildTest
  {
    [Test]
    public void UserTest()
    {
      EntityRemovingAction(
        session => new EntityTest1().Remove(),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
    }

    [Test]
    public void UserAssociationTestOwner()
    {
      EventHandler<EntityRemovingEventArgs> entityRemoving = (s, e) => {
        if (e.Entity is EntityTest1)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntityTest2)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var entities = EntityRemovingAction(
        session => new EntityTest2() { Value = new EntityTest1() }.Remove(), 
        entityRemoving);

      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public void UserAssociationEntitySetTestOwner()
    {
      Action<Session> action = session => {
        var entity = new EntityTest3();
        entity.Items.Add(new EntityTest1());
        entity.Remove();
      };

      EventHandler<EntityRemovingEventArgs> entityRemoving = (s, e) => {
        if (e.Entity is EntityTest1)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntitySetItem<EntityTest3, EntityTest1>)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntityTest3)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var entities = EntityRemovingAction(action, entityRemoving);
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(3));
    }

    [Test]
    public void UserNoAssociationTest()
    {
      EventHandler<EntityRemovingEventArgs> entityRemoving = (s, e) => {
        if (e.Entity is EntityTest4)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var entities = EntityRemovingAction(
        session => new EntityTest4() { Value = new EntityTest1() }.Remove()
        , entityRemoving);

      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(1));
    }

    [Test]
    public void PairedAssociationTestOwner()
    {
      Action<Session> action = session => {
        var entity = new EntityTest5() {Value = new EntityTest6()};
        entity.Value.Remove();
      };

      EventHandler<EntityRemovingEventArgs> entityRemoving = (s, e) => {
        if (e.Entity is EntityTest6)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
        if (e.Entity is EntityTest5)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));

      };

      var entities = EntityRemovingAction(action, entityRemoving);
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public void ManyToManyAssociationTestOwner()
    {
      Action<Session> action = session => {
        var entity = new EntityTest7();
        entity.Items.Add(new EntityTest8());
        entity.Items.First().Remove();
      };

      EventHandler<EntityRemovingEventArgs> entityRemoving = (s, e) => {
        if (e.Entity is EntityTest7)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntitySetItem<EntityTest7, EntityTest8>)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntityTest8)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
      };

      var entities = EntityRemovingAction(action, entityRemoving);
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(3));
    }

    [Test]
    public void DeepCascadeRemoveTest()
    {
      Action<Session> action = session => {
        var entity = new EntityTest9();

        entity.Items.Add(new EntityTest10());
        entity.Items.Add(new EntityTest10());

        entity.Items.ElementAt(0).Items.Add(new EntityTest11());
        entity.Items.ElementAt(0).Items.Add(new EntityTest11());
        entity.Items.ElementAt(1).Items.Add(new EntityTest11());
        entity.Items.ElementAt(1).Items.Add(new EntityTest11());

        entity.Remove();
      };

      EventHandler<EntityRemovingEventArgs> entityRemoving = (s, e) => {
        if (e.Entity is EntityTest9)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));

        var baseType = e.Entity.GetType().BaseType;
        if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition()==typeof(EntitySetItem<,>))
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntityTest10)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
        if (e.Entity is EntityTest11)
          Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
      };

      var entities = EntityRemovingAction(action, entityRemoving);
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(5));
    }

    [Test]
    public void DirectPersistentAccessorTest()
    {
      var entities = EntityRemovingAction(
        session => session.Services.Get<DirectPersistentAccessor>().Remove(new EntityTest1()),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(1));
    }

    [Test]
    public void EntitySetItemsRemoveTest()
    {
      Action<Session> action = session => {
        var entity = new EntityTest3();
        entity.Items.Add(new EntityTest1());
        entity.Items.Remove(entity.Items.First());
      };

      var entities = EntityRemovingAction(action, (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(1));
    }

    [Test]
    public void RemoveLaterTest()
    {
      EntityRemovingAction(
        session => new EntityTest1().RemoveLater(),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
    }

    [Test]
    public void SessionRemoveTest()
    {
      EntityRemovingAction(
        session => session.Remove(new[] {new EntityTest1()}),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
    }

    [Test]
    public void SessionRemoveLaterTest()
    {
      EntityRemovingAction(
        session => session.Remove(new[] { new EntityTest1() }),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
    }

    private Entity[] EntityRemovingAction(
      Action<Session> action,
      EventHandler<EntityRemovingEventArgs> entityRemoving = null)
    {
      var removedEntities = new List<Entity>();
      using (var domain = this.BuildDomain(this.BuildConfiguration()))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        session.SystemEvents.EntityRemoving += (s, e) => {
          removedEntities.Add(e.Entity);
          if (entityRemoving!=null)
            entityRemoving(s, e);
        };

        action(session);
        transaction.Complete();
      }

      if (entityRemoving!=null)
        Assert.IsNotEmpty(removedEntities);
      else
        Assert.IsEmpty(removedEntities);

      return removedEntities.ToArray();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(this.GetType().Assembly, this.GetType().Namespace + ".RemoveReasonEntityRemovingSessionEventTestModel");
      return config;
    }
  }

  namespace RemoveReasonEntityRemovingSessionEventTestModel
  {
    [HierarchyRoot]
    public class EntityTest1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Value { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntityTest1 Value { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<EntityTest1> Items { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntityTest1 Value { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest5 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntityTest6 Value { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest6 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Value")]
      public EntityTest5 Value { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest7 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public EntitySet<EntityTest8> Items { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest8 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Items")]
      public EntitySet<EntityTest7> Items { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest9 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<EntityTest10> Items { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest10 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
      public EntitySet<EntityTest11> Items { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest11 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Items { get; set; }
    }
  }
}
