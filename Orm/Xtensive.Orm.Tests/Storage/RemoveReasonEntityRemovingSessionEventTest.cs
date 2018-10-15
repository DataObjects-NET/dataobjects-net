using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Tests.Storage
{
  public class RemoveReasonEntityRemovingSessionEventTest : AutoBuildTest
  {
    [Test]
    public void UserTest()
    {
      EntityRemovingAction(
        (d, s, t) => new EntityTest1().Remove(),
        (s, e) => Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User)));
    }

    [Test]
    public void UserAssociationTestOwner()
    {
      var entities = EntityRemovingAction(
        (d, s, t) => new EntityTest2() {Value = new EntityTest1()}.Remove(),
        (s, e) => {
          if (e.Entity is EntityTest1)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
          if (e.Entity is EntityTest2)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));

        });
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public void UserAssociationEntitySetTestOwner()
    {
      var entities = EntityRemovingAction(
        (d, s, t) => {
          var entity = new EntityTest3();
          entity.Value.Add(new EntityTest1());
          entity.Remove();
        },
        (s, e) => {
          if (e.Entity is EntityTest1)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
          if (e.Entity is EntitySetItem<EntityTest3, EntityTest1>)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
          if (e.Entity is EntityTest3)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));

        });
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(3));
    }

    [Test]
    public void UserNoAssociationTest()
    {
      var entities = EntityRemovingAction(
        (d, s, t) => new EntityTest4() { Value = new EntityTest1() }.Remove(),
        (s, e) => {
          if (e.Entity is EntityTest4)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));

        });
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(1));
    }

    [Test]
    public void PairedAssociationTestOwner()
    {
      var entities = EntityRemovingAction(
        (d, s, t) => {
          var entity = new EntityTest5() {Value = new EntityTest6()};
          entity.Value.Remove();
        },
        (s, e) => {
          if (e.Entity is EntityTest6)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
          if (e.Entity is EntityTest5)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));

        });
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public void ManyToManyAssociationTestOwner()
    {
      var entities = EntityRemovingAction(
        (d, s, t) => {
          var entity = new EntityTest7();
          entity.Value.Add(new EntityTest8());
          entity.Value.First().Remove();
        },
        (s, e) => {
          if (e.Entity is EntityTest7)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
          if (e.Entity is EntitySetItem<EntityTest7, EntityTest8>)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.Association));
          if (e.Entity is EntityTest8)
            Assert.That(e.Reason, Is.EqualTo(EntityRemoveReason.User));
        });
      Assert.That(entities.Select(x => x.GetType()).Distinct().Count(), Is.EqualTo(3));
    }

    private Entity[] EntityRemovingAction(
      Action<Domain, Session, TransactionScope> action,
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
        action(domain, session, transaction);
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
      this.GetType().GetNestedTypes().ForEach(config.Types.Register);
      return config;
    }

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
      public EntitySet<EntityTest1> Value { get; set; }
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
      public EntitySet<EntityTest8> Value { get; set; }
    }

    [HierarchyRoot]
    public class EntityTest8 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Value")]
      public EntitySet<EntityTest7> Value { get; set; }
    }
  }
}
