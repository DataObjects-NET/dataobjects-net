// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.Issue0696.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Issues.Issue0696.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Master : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "Master")]
    public EntitySet<Detail> Details { get; private set; }

    [Field]
    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Detail : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public Master Master { get; set; }

    [Field]
    public string Name { get; set; }

    public override string ToString()
    {
      return Name;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class SyncEntry
  {
    public AssociationInfo Association;
    public Entity End1;
    public Entity End2;
    public Direction LastDirection;
    public int Depth;

    public bool CheckDirection(Direction currentDirection, out int depth)
    {
      depth = Depth;
      if (currentDirection==Direction.None)
        return false;
      switch (LastDirection) {
      case Direction.None:
        if (currentDirection!=Direction.Positive)
          Assert.Fail("Positive direction expected.");
        LastDirection = currentDirection;
        Depth++;
        return false;
      case Direction.Positive:
        if (currentDirection==Direction.Positive) {
          Depth++;
          return false;
        }
        LastDirection = currentDirection;
        Depth--;
        depth = Depth;
        return Depth==0;
      case Direction.Negative:
        if (currentDirection!=Direction.Negative)
          Assert.Fail("Negative direction expected.");
        Depth--;
        depth = Depth;
        if (Depth<0)
          Assert.Fail("Depth is negative.");
        return Depth == 0;
      default:
        throw new ArgumentOutOfRangeException("currentDirection");
      }
    }


    public SyncEntry(AssociationInfo association, Entity end1, Entity end2)
    {
      Association = association;
      End1 = end1;
      End2 = end2;
    }
  }

  [TestFixture]
  public class Issue0696_EntitySetItemAddBug : AutoBuildTest
  {
    private const string VersionFieldName = "Version";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Master).Assembly, typeof(Master).Namespace);
      return configuration;
    }

    [Test]
    public void BugTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var m = new Master();

        session.SaveChanges();
        session.Events.EntitySetItemAdd += (sender, e) => {
          if (e.EntitySet ==m.Details && e.Item is Detail) {
            // This call leads to EntitySet content refresh, and thus
            // discarding of just added item.
            var count = m.Details.Count;
          }
        };

        var d1 = new Detail();
        var d2 = new Detail();
        d1.Master = m;
        d2.Master = m;
        Assert.IsTrue(m.Details.Contains(d2));
        Assert.IsTrue(m.Details.Contains(d1));
      }
    }

    [Test]
    public void EventSequenceTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        AttachSyncEventCheckers(session);
        var m1 = new Master { Name = "m1" };
        var m2 = new Master { Name = "m2" };
        var d1 = new Detail { Name = "d1" };
        var d2 = new Detail { Name = "d2" };
        var d3 = new Detail { Name = "d3" };
        d1.Master = m1;
        d2.Master = m1;
        d1.Master = m2;
        m1.Details.Add(d2);

        EventHandler<EntitySetItemEventArgs> h1 =  (s, e) => {
          isNewChain = true;
          m2.Details.Add(d3);
        };
        session.Events.EntitySetItemRemove += h1;
        m2.Details.Add(d2);
      }
    }

    private void AttachSyncEventCheckers(Session session)
    {
      session.Events.EntityFieldValueSetting += CreateEntityFieldSettingHandler("Field value setting", Direction.Positive);
      session.Events.EntityFieldValueSet     += CreateEntityFieldSetHandler("Field value set", Direction.Negative);
      session.Events.EntitySetItemAdding     += CreateEntitySetItemHandler("Item adding", Direction.Positive);
      session.Events.EntitySetItemAdd        += CreateEntitySetItemHandler("Item added", Direction.Negative);
      session.Events.EntitySetItemRemoving   += CreateEntitySetItemHandler("Item removing", Direction.Positive);
      session.Events.EntitySetItemRemove     += CreateEntitySetItemHandler("Item removed", Direction.Negative);
    }

    private EventHandler<EntityFieldValueSetEventArgs> CreateEntityFieldSetHandler(string description, Direction direction)
    {
      return (sender, e) => {
        var association = e.Field.Associations.LastOrDefault();
        if (association == null)
          return;
        CheckDirection(association, e.Entity, (Entity)e.NewValue, direction, 
          "{0}, Field = {1}, Entity = {2}, Value = {3}".FormatWith(description,
            e.Field.UnderlyingProperty.GetShortName(true), e.Entity, e.NewValue));
      };
    }

    private EventHandler<EntityFieldValueEventArgs> CreateEntityFieldSettingHandler(string description, Direction direction)
    {
      return (sender, e) => {
        var association = e.Field.Associations.LastOrDefault();
        if (association==null)
          return;
        CheckDirection(association, e.Entity, (Entity)e.Value, direction, 
          "{0}, Field = {1}, Entity = {2}, Value = {3}".FormatWith(description,
            e.Field.UnderlyingProperty.GetShortName(true), e.Entity, e.Value));
      };
    }

    private EventHandler<EntitySetItemEventArgs> CreateEntitySetItemHandler(string description, Direction direction)
    {
      return (sender, e) => {
        if (e.Field.Associations.LastOrDefault() == null)
          return;
        CheckDirection(e.EntitySet.Field.GetAssociation(e.Item.TypeInfo), e.Entity, e.Item, direction,
          "{0}, Field = {1}, Owner = {2}, Item = {3}".FormatWith(description, 
            e.EntitySet.Field.UnderlyingProperty.GetShortName(true), e.Entity, e.Item));
      };
    }

    private Stack<SyncEntry> stack = new Stack<SyncEntry>();
    private bool isNewChain;

    private void CheckDirection(AssociationInfo association, Entity owner, Entity item, Direction direction, string message)
    {
      var entry = (stack.Count>0) ? stack.Peek() : null;
      if (entry==null || isNewChain) {
        isNewChain = false;
        entry = new SyncEntry(association, owner, item);
        stack.Push(entry);
      }
      int depth;
      if (entry.CheckDirection(direction, out depth))
        stack.Pop();
      Log.Info("{0}", message.Indent(depth * 2));
    }
  }
}