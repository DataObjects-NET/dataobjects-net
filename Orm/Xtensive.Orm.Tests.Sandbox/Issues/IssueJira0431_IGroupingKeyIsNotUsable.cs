// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.01.31

using System;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0431_IGroupingKeyIsNotUsableModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0431_IGroupingKeyIsNotUsableModel
  {
    public interface IItemBase : IEntity
    {
    }

    public interface IItemVersionBase : IEntity
    {
      [Field]
      IItemBase ItemUntyped { get; set; }
    }

    public class ItemItemVersionsEntitySet : EntitySet<IItemVersionBase>
    {
      protected ItemItemVersionsEntitySet(Entity owner, FieldInfo field)
        : base(owner, field)
      {
      }

      protected ItemItemVersionsEntitySet(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }
    }

    public abstract class ItemBase : Entity, IItemBase
    {
      [Key, Field]
      public Guid Id { get; private set; }
    }

    public abstract class ItemVersionBase : Entity, IItemVersionBase
    {
      [Key, Field]
      public Guid Id { get; private set; }

      [Field]
      public IItemBase ItemUntyped { get; set; }
    }

    public abstract class Item<TOwner, TChild> : ItemBase
    {
      [Field, Association(PairTo = "ItemUntyped", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public ItemItemVersionsEntitySet ItemVersionsUntyped { get; private set; }
    }

    public abstract class ItemVersion<TOwner, TChild> : ItemVersionBase
    {
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class Line : Item<Line, LineVersion>
    {
      //Some fields
    }

    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class LineVersion : ItemVersion<Line, LineVersion>
    {
      //Some fields
    }
  }

  [TestFixture]
  public class IssueJira0431_IGroupingKeyIsNotUsable : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Line).Assembly, typeof (Line).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var line = new Line();
        line.ItemVersionsUntyped.Add(new LineVersion());
        line.ItemVersionsUntyped.Add(new LineVersion());
        new LineVersion();
      }
    }

    [Test]
    public void CountLinqTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query =
          from e in session.Query.All<Line>()
          group e by e
          into g
          select new {
            g.Key,
            Count = g.Key.ItemVersionsUntyped.Count(),
          };
        var result = query.ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Count, Is.EqualTo(2));
        tx.Complete();
      }
    }

    [Test]
    public void CountEntitySetTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query =
          from e in session.Query.All<Line>()
          group e by e
          into g
          select new {
            g.Key,
            g.Key.ItemVersionsUntyped.Count,
          };
        var result = query.ToList();
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Count, Is.EqualTo(2));
        tx.Complete();
      }
    }
  }
}