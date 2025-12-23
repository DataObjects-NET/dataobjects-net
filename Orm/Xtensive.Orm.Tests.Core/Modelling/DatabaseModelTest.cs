// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Core.Modelling.DatabaseModel;

namespace Xtensive.Orm.Tests.Core.Modelling
{
  [TestFixture]
  public class DatabaseModelTest
  {
    private Server srv;
    private DatabaseModel.Security sec1;
    private DatabaseModel.Security sec2;
    private User u1;
    private User u2;
    private Role r1;
    private Role r2;
    private Database db1;
    private Database db2;
    private Schema s1;
    private Schema s2;
    private Table t1;
    private Table t2;

    [OneTimeSetUp]
    public void Setup()
    {
      srv = new Server("srv");
      srv.Actions = new ActionSequence();
      sec1 = new DatabaseModel.Security(srv, "sec1");
      u1 = new User(sec1, "u1") { Password = "1"};
      u1.Password = "u1";
      u2 = new User(sec1, "u2") { Password = "2"};
      u2.Password = "u2";
      r1 = new Role(sec1, "r1");
      r2 = new Role(sec1, "r2");
      _ = new RoleRef(u1, r1);
      _ = new RoleRef(u1, r2);
      _ = new RoleRef(u2, r2);
      // sec2 = new Security(srv, "sec2");
      db1 = new Database(srv, "db1") { Owner = u1 };
      db2 = new Database(srv, "db2") { Owner = u1 };
      s1 = new Schema(db1, "s1");
      t1 = new Table(s1, "t1");
      _ = new PrimaryIndex(t1, "PK_t1");
      _ = new SecondaryIndex(t1, "SI_t1a");
      _ = new SecondaryIndex(t1, "SI_t1b");
      _ = new SecondaryIndex(t1, "SI_t1ab");
      s2 = new Schema(db1, "s2");
      t2 = new Table(s2, "t2");
      _ = new PrimaryIndex(t2, "PK_t2");
      _ = new SecondaryIndex(t2, "SI_t2a");
      _ = new SecondaryIndex(t2, "SI_t2ab");
    }

    [Test]
    [Ignore("TODO: AY check it ASAP")]
    public void SerializationTest()
    {
      var clone = Cloner.CloneViaBinarySerialization(srv);
//      clone.Dump();
      clone.Validate();
    }

    [Test]
    public void CloneTest()
    {
      var clone = Clone(srv);
      clone.Validate();
//      clone.Dump();
    }

    [Test]
    public void BaseComparisonTest()
    {
      var source = new Server("Source");
      var target = srv;
      target.Validate();
       
      TestLog.Info("Source model:");
//      source.Dump();
      TestLog.Info("Target model:");
//      target.Dump();

      var comparer = new Comparer();
      var hints = new HintSet(source, target) {
        new RenameHint("", "")
      };
      Difference diff = comparer.Compare(source, target, hints);
      TestLog.Info($"Difference: \r\n{diff}");

      var actions = new ActionSequence {
        new Upgrader().GetUpgradeSequence(diff, hints, comparer)
      };
      TestLog.Info($"Actions: \r\n{actions}");

      TestLog.Info("Applying actions...");
      actions.Apply(source);

      TestLog.Info("Updated Model 1:");
//      source.Dump();
    }

    [Test]
    public void ComplexComparisonTest()
    {
      srv.Validate();
      TestUpdate((s1, s2, hs) => {
        s1.Databases[1].Remove();
        s2.Databases[0].Remove();
        s2.Security.Users[0].Name = "Renamed";
        hs.Add(new RenameHint(s1.Security.Users[0].Path, s2.Security.Users[0].Path));
      });
    }

    [Test]
    public void ComplexReferenceComparisonTest()
    {
      srv.Validate();
      TestUpdate((s1, s2, hs) => {
        var s1r = s1.Security.Roles[0];
        RemoveReferencesTo(s1r);
        s1r.Remove();
        var s2r = s2.Security.Roles[1];
        RemoveReferencesTo(s2r);
        s2r.Remove();
      });
    }

    private void RemoveReferencesTo(Role role)
    {
      var s = role.Model;
      var roleRefs = new List<RoleRef>();
      foreach (var roleRef in s.Security.Users.SelectMany(u => u.Roles))
        if (roleRef.Value==role)
          roleRefs.Add(roleRef);
      foreach (var roleRef in roleRefs)
        roleRef.Remove();
    }

    [Test]
    public void CreateActionsTest()
    {
      TestLog.Info("Model:");
      srv.Dump();
      TestLog.Info("Actions:");
      TestLog.Info($"{srv.Actions}");

      TestLog.Info("Creating new model...");
      var srvx = new Server("srv");
      TestLog.Info("Applying actions...");
      srv.Actions.Apply(srvx);
      TestLog.Info("Updated model:");
//      srvx.Dump();
    }

    [Test]
    [Ignore("TODO: AY check it ASAP")]
    public void UnnamedNodeTest()
    {
      var rr0 = u1.Roles[0];
      var rr1 = u1.Roles[1];
      Assert.That(rr0.Name, Is.EqualTo("0"));
      Assert.That(rr1.Name, Is.EqualTo("1"));

      rr0.Index = 1;
      Assert.That(rr0.Name, Is.EqualTo("1"));
      Assert.That(rr1.Name, Is.EqualTo("0"));
      Assert.That(rr0, Is.EqualTo(u1.Roles[1]));
      Assert.That(rr1, Is.EqualTo(u1.Roles[0]));
      
      rr0.Index = 0;
      Assert.That(rr0.Name, Is.EqualTo("0"));
      Assert.That(rr1.Name, Is.EqualTo("1"));
      Assert.That(rr0, Is.EqualTo(u1.Roles[0]));
      Assert.That(rr1, Is.EqualTo(u1.Roles[1]));
    }

    [Test]
    public void IndexAndParentTest()
    {
      // srv
      Assert.That(srv.Index, Is.EqualTo(0));
      srv.Index = 0;
      Assert.That(srv.Index, Is.EqualTo(0));
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => srv.Index=1);
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => srv.Index=-1);

      // db1, db2
      Assert.That(db1.Index, Is.EqualTo(0));
      Assert.That(db2.Index, Is.EqualTo(1));
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => db1.Index=-1);
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => db1.Index=2);
      db1.Index = 0;
      Assert.That(db1.Index, Is.EqualTo(0));
      Assert.That(db2.Index, Is.EqualTo(1));
      db1.Index = 1;
      Assert.That(db1.Index, Is.EqualTo(1));
      Assert.That(db2.Index, Is.EqualTo(0));
      db2.Index = 1;
      Assert.That(db1.Index, Is.EqualTo(0));
      Assert.That(db2.Index, Is.EqualTo(1));

      // s1, s2 : index
      Assert.That(s1.Parent, Is.EqualTo(db1));
      Assert.That(s2.Parent, Is.EqualTo(db1));
      s1.Parent = db1;
      Assert.That(s1.Parent, Is.EqualTo(db1));
      s1.Parent = db2;
      Assert.That(s1.Parent, Is.EqualTo(db2));
      Assert.That(s2.Parent, Is.EqualTo(db1));
      Assert.That(s1.Index, Is.EqualTo(0));
      Assert.That(s2.Index, Is.EqualTo(0));
      s2.Parent = db2;
      Assert.That(s1.Parent, Is.EqualTo(db2));
      Assert.That(s2.Parent, Is.EqualTo(db2));
      Assert.That(s1.Index, Is.EqualTo(0));
      Assert.That(s2.Index, Is.EqualTo(1));
      s2.Parent = db1;
      s1.Parent = db1;
      Assert.That(s1.Index, Is.EqualTo(1));
      Assert.That(s2.Index, Is.EqualTo(0));
      s1.Index = 0;
      Assert.That(s1.Index, Is.EqualTo(0));
      Assert.That(s2.Index, Is.EqualTo(1));
      Assert.That(s1.Parent, Is.EqualTo(db1));
      Assert.That(s2.Parent, Is.EqualTo(db1));
    }

    [Test]
    public void ContainerPropertyTest()
    {
      srv.Security.Remove();
      Assert.That(sec1.State, Is.EqualTo(NodeState.Removed));
      Assert.That(srv.Security, Is.EqualTo(null));
      
      sec2 = new DatabaseModel.Security(srv, "sec2");
      Assert.That(srv.Security, Is.EqualTo(sec2));
      AssertEx.Throws<InvalidOperationException>(
        () => sec1 = new DatabaseModel.Security(srv, "sec3"));

      sec2.Remove();
      Assert.That(sec2.State, Is.EqualTo(NodeState.Removed));
      Assert.That(srv.Security, Is.EqualTo(null));

      sec1 = new DatabaseModel.Security(srv, "sec1");
      Assert.That(srv.Security, Is.EqualTo(sec1));
    }

    [Test]
    public void PathTest()
    {
      Assert.That(srv.Path, Is.EqualTo(string.Empty));
      Assert.That(srv.Resolve(string.Empty), Is.EqualTo(srv));

      Assert.That("Databases", Is.EqualTo(srv.Databases.Path));
      Assert.That(srv.Databases, Is.EqualTo(srv.Resolve("Databases")));

      Assert.That("Security", Is.EqualTo(srv.Security.Path));
      Assert.That(srv.Security, Is.EqualTo(srv.Resolve("Security")));

      Assert.That("Security/Users", Is.EqualTo(srv.Security.Users.Path));
      Assert.That(srv.Resolve("Security/Users"), Is.EqualTo(srv.Security.Users));

      Assert.That(db2.Path, Is.EqualTo("Databases/db2"));
      Assert.That(srv.Resolve("Databases/db2"), Is.EqualTo(db2));

      Assert.That(db2.Schemas.Path, Is.EqualTo("Databases/db2/Schemas"));
      Assert.That(srv.Resolve("Databases/db2/Schemas"), Is.EqualTo(db2.Schemas));

      Assert.That(s1.Path, Is.EqualTo("Databases/db1/Schemas/s1"));
      Assert.That(srv.Resolve("Databases/db1/Schemas/s1"), Is.EqualTo(s1));

      Assert.That(s2.Path, Is.EqualTo("Databases/db1/Schemas/s2"));
      Assert.That(srv.Resolve("Databases/db1/Schemas/s2"), Is.EqualTo(s2));
    }

    [Test]
    public void DumpTest()
    {
//      srv.Dump();
    }

    [Test]
    public void ApplyTest()
    {
      TestLog.Info("Model:");
      srv.Dump();
      TestLog.Info("Actions:");
      TestLog.Info($"{srv.Actions}");

      TestLog.Info("Creating new model...");
      var srvx = new Server("srv");
      TestLog.Info("Applying actions...");
      srv.Actions.Apply(srvx);
      TestLog.Info("Updated model:");
//      srvx.Dump();
    }

    [Test]
    public void LockModelTest()
    {
      srv.Lock(true);
      AssertEx.Throws<InstanceIsLockedException>(
        () => new Database(srv, "newDatabase"));
      AssertEx.Throws<InstanceIsLockedException>(
        () => srv.Databases.Clear());
    }

    #region Private methods

    private void TestUpdate(Action<Server, Server, HintSet> update)
    {
      TestUpdate(update, true);
      TestUpdate(update, false);
    }

    private void TestUpdate(Action<Server, Server, HintSet> update, bool useHints)
    {
      var s1 = Clone(srv);
      var s2 = Clone(srv);
      var hints = new HintSet(s1, s2);
      update.Invoke(s1, s2, hints);
      if (!useHints)
        hints = new HintSet(s1, s2);
      TestLog.Info($"Update test ({(useHints ? "with" : "without")} hints)");
//      s1.Dump();
//      s2.Dump();
      s1.Validate();
      s2.Validate();

      // Comparing different models
      TestLog.Info("Comparing models:");
      var comparer = new Comparer();
      var diff = comparer.Compare(s1, s2, hints);
      TestLog.Info($"\r\nDifference:\r\n{diff}");
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, comparer)
      };
      TestLog.Info($"\r\nActions:\r\n{actions}");
    }

    private Server Clone(Server server)
    {
      return (Server) server.Clone(null, server.Name);
      // return (Server) LegacyBinarySerializer.Instance.Clone(server);
    }

    #endregion
  }
}