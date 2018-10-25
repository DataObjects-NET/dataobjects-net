// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.03.29

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.SchemaSharing.Requests.Model
{
  namespace Part1
  {
    [HierarchyRoot]
    public class TestEntity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part2
  {
    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part3
  {
    [HierarchyRoot]
    public class TestEntity3 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  namespace Part4
  {
    [HierarchyRoot]
    public class TestEntity4 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    private int initialCountOfEntities = 0;

    public override bool CanUpgradeFrom(string oldVersion)
    {
      return true;
    }

    public override void OnPrepare()
    {
      if (UpgradeContext.NodeConfiguration.UpgradeMode==DomainUpgradeMode.Recreate)
        initialCountOfEntities = 0;
      else
        initialCountOfEntities = 1;
    }

    public override void OnUpgrade()
    {
      base.OnUpgrade();
      // we have a storageNode so we have an access to tables;
      var session = Session.Current;

      Select(session);
      var createdKeys = Insert(session);
      Update(session, createdKeys);
      Delete(session, createdKeys);
      Select(session);
    }

    public override void OnStage()
    {
      base.OnStage();
      //we have a storage node so we have an access to tables
      var session = Session.Current;

      Select(session);
      var createdKeys = Insert(session);
      Update(session, createdKeys);
      Delete(session, createdKeys);
      Select(session);
    }

    private void Select(Session session)
    {
      Assert.That(session.Query.All<Part1.TestEntity1>().AsEnumerable().Count(), Is.EqualTo(initialCountOfEntities));
      Assert.That(session.Query.All<Part2.TestEntity2>().AsEnumerable().Count(), Is.EqualTo(initialCountOfEntities));
      Assert.That(session.Query.All<Part3.TestEntity3>().AsEnumerable().Count(), Is.EqualTo(initialCountOfEntities));
      Assert.That(session.Query.All<Part4.TestEntity4>().AsEnumerable().Count(), Is.EqualTo(initialCountOfEntities));

      if (initialCountOfEntities==0)
        return;
      Assert.That(session.Query.All<Part1.TestEntity1>().AsEnumerable().First().Text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));
      Assert.That(session.Query.All<Part2.TestEntity2>().AsEnumerable().First().Text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));
      Assert.That(session.Query.All<Part3.TestEntity3>().AsEnumerable().First().Text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));
      Assert.That(session.Query.All<Part4.TestEntity4>().AsEnumerable().First().Text, Is.EqualTo(UpgradeContext.NodeConfiguration.NodeId));
    }

    private Key[] Insert(Session session)
    {
      var text = string.Format("{0}_during_upgrade", UpgradeContext.NodeConfiguration.NodeId);
      var a = new Part1.TestEntity1 {Text = text};
      var b = new Part2.TestEntity2 {Text = text};
      var c = new Part3.TestEntity3 {Text = text};
      var d = new Part4.TestEntity4 {Text = text};

      session.SaveChanges();

      Assert.That(session.Query.All<Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      a = session.Query.All<Part1.TestEntity1>().Where(e => e.Text==text).FirstOrDefault();
      Assert.That(a, Is.Not.Null);

      Assert.That(session.Query.All<Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      b = session.Query.All<Part2.TestEntity2>().Where(e => e.Text==text).FirstOrDefault();
      Assert.That(b, Is.Not.Null);

      Assert.That(session.Query.All<Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      c = session.Query.All<Part3.TestEntity3>().Where(e => e.Text==text).FirstOrDefault();
      Assert.That(c, Is.Not.Null);

      Assert.That(session.Query.All<Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      d = session.Query.All<Part4.TestEntity4>().Where(e => e.Text==text).FirstOrDefault();
      Assert.That(d, Is.Not.Null);

      return new Key[] {a.Key, b.Key, c.Key, d.Key};
    }

    private void Update(Session session, Key[] createdKeys)
    {
      var updatedText = string.Format("{0}_during_upgrade_updated", UpgradeContext.NodeConfiguration.NodeId);

      var a = session.Query.All<Part1.TestEntity1>().Where(e => e.Key==createdKeys[0]).First();
      a.Text = updatedText;
      var b = session.Query.All<Part2.TestEntity2>().Where(e => e.Key==createdKeys[1]).First();
      b.Text = updatedText;
      var c = session.Query.All<Part3.TestEntity3>().Where(e => e.Key==createdKeys[2]).First();
      c.Text = updatedText;
      var d = session.Query.All<Part4.TestEntity4>().Where(e => e.Key==createdKeys[3]).First();
      d.Text = updatedText;

      session.SaveChanges();

      Assert.That(session.Query.All<Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      a = session.Query.All<Part1.TestEntity1>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(a, Is.Not.Null);

      Assert.That(session.Query.All<Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      b = session.Query.All<Part2.TestEntity2>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(b, Is.Not.Null);

      Assert.That(session.Query.All<Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      c = session.Query.All<Part3.TestEntity3>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(c, Is.Not.Null);

      Assert.That(session.Query.All<Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities + 1));
      d = session.Query.All<Part4.TestEntity4>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(d, Is.Not.Null);
    }

    private void Delete(Session session, Key[] createdKeys)
    {
      var a = session.Query.All<Part1.TestEntity1>().Where(e => e.Key==createdKeys[0]).First();
      a.Remove();
      var b = session.Query.All<Part2.TestEntity2>().Where(e => e.Key==createdKeys[1]).First();
      b.Remove();
      var c = session.Query.All<Part3.TestEntity3>().Where(e => e.Key==createdKeys[2]).First();
      c.Remove();
      var d = session.Query.All<Part4.TestEntity4>().Where(e => e.Key==createdKeys[3]).First();
      d.Remove();

      session.SaveChanges();

      var updatedText = string.Format("{0}_during_upgrade_updated", UpgradeContext.NodeConfiguration.NodeId);
      Assert.That(session.Query.All<Part1.TestEntity1>().Count(), Is.EqualTo(initialCountOfEntities));
      a = session.Query.All<Part1.TestEntity1>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(a, Is.Null);

      Assert.That(session.Query.All<Part2.TestEntity2>().Count(), Is.EqualTo(initialCountOfEntities));
      b = session.Query.All<Part2.TestEntity2>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(b, Is.Null);

      Assert.That(session.Query.All<Part3.TestEntity3>().Count(), Is.EqualTo(initialCountOfEntities));
      c = session.Query.All<Part3.TestEntity3>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(c, Is.Null);

      Assert.That(session.Query.All<Part4.TestEntity4>().Count(), Is.EqualTo(initialCountOfEntities));
      d = session.Query.All<Part4.TestEntity4>().Where(e => e.Text==updatedText).FirstOrDefault();
      Assert.That(d, Is.Null);
    }
  }
}
