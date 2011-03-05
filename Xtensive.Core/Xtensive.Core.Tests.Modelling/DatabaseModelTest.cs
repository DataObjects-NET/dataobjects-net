// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Tests.Modelling.DatabaseModel;
using Xtensive.Modelling;
using Xtensive.Serialization.Binary;
using Xtensive.Testing;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Comparison;
using System.Linq;

namespace Xtensive.Core.Tests.Modelling
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

    [TestFixtureSetUp]
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
      new RoleRef(u1, r1);
      new RoleRef(u1, r2);
      new RoleRef(u2, r2);
      // sec2 = new Security(srv, "sec2");
      db1 = new Database(srv, "db1") { Owner = u1 };
      db2 = new Database(srv, "db2") { Owner = u1 };
      s1 = new Schema(db1, "s1");
      t1 = new Table(s1, "t1");
      new PrimaryIndex(t1, "PK_t1");
      new SecondaryIndex(t1, "SI_t1a");
      new SecondaryIndex(t1, "SI_t1b");
      new SecondaryIndex(t1, "SI_t1ab");
      s2 = new Schema(db1, "s2");
      t2 = new Table(s2, "t2");
      new PrimaryIndex(t2, "PK_t2");
      new SecondaryIndex(t2, "SI_t2a");
      new SecondaryIndex(t2, "SI_t2ab");
    }

    [Test]
    [Ignore("TODO: AY check it ASAP")]
    public void SerializationTest()
    {
      var clone = (Server) LegacyBinarySerializer.Instance.Clone(srv);
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
       
      Log.Info("Source model:");
//      source.Dump();
      Log.Info("Target model:");
//      target.Dump();

      var comparer = new Comparer();
      var hints = new HintSet(source, target) {
        new RenameHint("", "")
      };
      Difference diff = comparer.Compare(source, target, hints);
      Log.Info("Difference: \r\n{0}", diff);

      var actions = new ActionSequence {
        new Upgrader().GetUpgradeSequence(diff, hints, comparer)
      };
      Log.Info("Actions: \r\n{0}", actions);

      Log.Info("Applying actions...");
      actions.Apply(source);

      Log.Info("Updated Model 1:");
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
      Log.Info("Model:");
      srv.Dump();
      Log.Info("Actions:");
      Log.Info("{0}", srv.Actions);

      Log.Info("Creating new model...");
      var srvx = new Server("srv");
      Log.Info("Applying actions...");
      srv.Actions.Apply(srvx);
      Log.Info("Updated model:");
//      srvx.Dump();
    }

    [Test]
    [Ignore("TODO: AY check it ASAP")]
    public void UnnamedNodeTest()
    {
      var rr0 = u1.Roles[0];
      var rr1 = u1.Roles[1];
      Assert.AreEqual(rr0.Name, "0");
      Assert.AreEqual(rr1.Name, "1");

      rr0.Index = 1;
      Assert.AreEqual(rr0.Name, "1");
      Assert.AreEqual(rr1.Name, "0");
      Assert.AreEqual(rr0, u1.Roles[1]);
      Assert.AreEqual(rr1, u1.Roles[0]);
      
      rr0.Index = 0;
      Assert.AreEqual(rr0.Name, "0");
      Assert.AreEqual(rr1.Name, "1");
      Assert.AreEqual(rr0, u1.Roles[0]);
      Assert.AreEqual(rr1, u1.Roles[1]);
    }

    [Test]
    public void IndexAndParentTest()
    {
      // srv
      Assert.AreEqual(srv.Index, 0);
      srv.Index = 0;
      Assert.AreEqual(srv.Index, 0);
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => srv.Index=1);
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => srv.Index=-1);

      // db1, db2
      Assert.AreEqual(db1.Index, 0);
      Assert.AreEqual(db2.Index, 1);
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => db1.Index=-1);
      AssertEx.Throws<ArgumentOutOfRangeException>(
        () => db1.Index=2);
      db1.Index = 0;
      Assert.AreEqual(db1.Index, 0);
      Assert.AreEqual(db2.Index, 1);
      db1.Index = 1;
      Assert.AreEqual(db1.Index, 1);
      Assert.AreEqual(db2.Index, 0);
      db2.Index = 1;
      Assert.AreEqual(db1.Index, 0);
      Assert.AreEqual(db2.Index, 1);

      // s1, s2 : index
      Assert.AreEqual(s1.Parent, db1);
      Assert.AreEqual(s2.Parent, db1);
      s1.Parent = db1;
      Assert.AreEqual(s1.Parent, db1);
      s1.Parent = db2;
      Assert.AreEqual(s1.Parent, db2);
      Assert.AreEqual(s2.Parent, db1);
      Assert.AreEqual(s1.Index, 0);
      Assert.AreEqual(s2.Index, 0);
      s2.Parent = db2;
      Assert.AreEqual(s1.Parent, db2);
      Assert.AreEqual(s2.Parent, db2);
      Assert.AreEqual(s1.Index, 0);
      Assert.AreEqual(s2.Index, 1);
      s2.Parent = db1;
      s1.Parent = db1;
      Assert.AreEqual(s1.Index, 1);
      Assert.AreEqual(s2.Index, 0);
      s1.Index = 0;
      Assert.AreEqual(s1.Index, 0);
      Assert.AreEqual(s2.Index, 1);
      Assert.AreEqual(s1.Parent, db1);
      Assert.AreEqual(s2.Parent, db1);
    }

    [Test]
    public void ContainerPropertyTest()
    {
      srv.Security.Remove();
      Assert.AreEqual(sec1.State, NodeState.Removed);
      Assert.AreEqual(srv.Security, null);
      
      sec2 = new DatabaseModel.Security(srv, "sec2");
      Assert.AreEqual(srv.Security, sec2);
      AssertEx.Throws<InvalidOperationException>(
        () => sec1 = new DatabaseModel.Security(srv, "sec3"));

      sec2.Remove();
      Assert.AreEqual(sec2.State, NodeState.Removed);
      Assert.AreEqual(srv.Security, null);

      sec1 = new DatabaseModel.Security(srv, "sec1");
      Assert.AreEqual(srv.Security, sec1);
    }

    [Test]
    public void PathTest()
    {
      Assert.AreEqual(srv.Path, string.Empty);
      Assert.AreEqual(srv, srv.Resolve(string.Empty));

      Assert.AreEqual(srv.Databases.Path, "Databases");
      Assert.AreEqual(srv.Databases, srv.Resolve("Databases"));

      Assert.AreEqual(srv.Security.Path, "Security");
      Assert.AreEqual(srv.Security, srv.Resolve("Security"));

      Assert.AreEqual(srv.Security.Users.Path, "Security/Users");
      Assert.AreEqual(srv.Security.Users, srv.Resolve("Security/Users"));

      Assert.AreEqual(db2.Path, "Databases/db2");
      Assert.AreEqual(db2, srv.Resolve("Databases/db2"));

      Assert.AreEqual(db2.Schemas.Path, "Databases/db2/Schemas");
      Assert.AreEqual(db2.Schemas, srv.Resolve("Databases/db2/Schemas"));
      
      Assert.AreEqual(s1.Path, "Databases/db1/Schemas/s1");
      Assert.AreEqual(s1, srv.Resolve("Databases/db1/Schemas/s1"));

      Assert.AreEqual(s2.Path, "Databases/db1/Schemas/s2");
      Assert.AreEqual(s2, srv.Resolve("Databases/db1/Schemas/s2"));
    }

    [Test]
    public void DumpTest()
    {
//      srv.Dump();
    }

    [Test]
    public void ApplyTest()
    {
      Log.Info("Model:");
      srv.Dump();
      Log.Info("Actions:");
      Log.Info("{0}", srv.Actions);

      Log.Info("Creating new model...");
      var srvx = new Server("srv");
      Log.Info("Applying actions...");
      srv.Actions.Apply(srvx);
      Log.Info("Updated model:");
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
      Log.Info("Update test ({0} hints)", useHints ? "with" : "without");
//      s1.Dump();
//      s2.Dump();
      s1.Validate();
      s2.Validate();

      // Comparing different models
      Log.Info("Comparing models:");
      var comparer = new Comparer();
      var diff = comparer.Compare(s1, s2, hints);
      Log.Info("\r\nDifference:\r\n{0}", diff);
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, comparer)
      };
      Log.Info("\r\nActions:\r\n{0}", actions);
    }

    private Server Clone(Server server)
    {
      return (Server) server.Clone(null, server.Name);
      // return (Server) LegacyBinarySerializer.Instance.Clone(server);
    }

    #endregion
  }
}