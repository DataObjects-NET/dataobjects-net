// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DisableSaveChangesTestModel;

namespace Xtensive.Orm.Tests.Storage.DisableSaveChangesTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Killer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Frags { get; private set; }

    [Field, Association(PairTo = "Killer", OnTargetRemove = OnRemoveAction.Clear)]
    private EntitySet<Victim> Victims { get; set; }

    public void Kill(Victim victim)
    {
      if (Victims.Add(victim))
        Frags++;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Victim : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    public bool IsAlive { get { return Killer==null; } }
    
    [Field]
    public Killer Killer {
      get { return GetFieldValue<Killer>("Killer"); }
      private set {
        KillDate = value==null ? (DateTime?) null : Transaction.Current.TimeStamp;
        SetFieldValue("Killer", value);
      }
    }

    [Field]
    public DateTime? KillDate { get; private set; }
 
    public void Resurrect()
    {
      if (IsAlive)
        throw new InvalidOperationException("Is not dead yet");
      Killer = null;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Node : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Node> Children { get; private set; }

    [Field]
    public int Tag { get; set; }
   
    [Field, Association(PairTo = "Children",
      OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
    public Node Parent { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class DisableSaveChangesTest : AutoBuildTest
  {
    private List<Node> allTrees;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Victim).Assembly, typeof (Victim).Namespace);
      return configuration;
    }

    [Test]
    public void DisableAllTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var victim = new Victim();
        var count = session.Query.All<Victim>().Count();
        using (session.DisableSaveChanges()) {
          var anotherVictim = new Victim();
          var newCount = session.Query.All<Victim>().Count();
          Assert.AreEqual(count, newCount);
          session.SaveChanges();
          var newestCount = session.Query.All<Victim>().Count();
          Assert.AreEqual(count + 1, newestCount);
        }
      }
    }

    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void DisableAllCommitTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var victim = new Victim();
        var count = session.Query.All<Victim>().Count();
        session.DisableSaveChanges();

        var anotherVictim = new Victim();
        var newCount = session.Query.All<Victim>().Count();
        Assert.AreEqual(count, newCount);
        t.Complete();
      }
    }
    
    [Test]
    public void SimplePinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var butcher = new Killer();
        var firstVictim = new Victim();
        // "Killers" who have not killed yet should not be considered as killers
        using (session.DisableSaveChanges(butcher)) {
          session.SaveChanges();
          Assert.AreEqual(PersistenceState.New, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, firstVictim.PersistenceState);
          butcher.Kill(firstVictim);
          session.SaveChanges();
          Assert.AreEqual(PersistenceState.New, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Modified, firstVictim.PersistenceState);
        }
        session.SaveChanges();
        using (session.DisableSaveChanges(butcher)) {
          firstVictim.Resurrect();
          var secondVictim = new Victim();
          butcher.Kill(secondVictim);
          session.SaveChanges();
          Assert.AreEqual(PersistenceState.Modified, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, firstVictim.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, secondVictim.PersistenceState);
        }
      }
    }

    [Test]
    public void PinGraphsTest()
    {
      try {
        allTrees = new List<Node>();
        using (var session = Domain.OpenSession())
        using (session.OpenTransaction()) {
          var node = T(T(), T(), T(T()));
          using (session.DisableSaveChanges(node)) {
            AssertNumberOfNodesInDatabaseIs(0);
            foreach (var item in allTrees)
              Assert.AreEqual(PersistenceState.New, item.PersistenceState);
          }
          session.SaveChanges();
          AssertNumberOfNodesInDatabaseIs(allTrees.Count);
          using (session.DisableSaveChanges(node)) {
            foreach (var item in allTrees)
              item.Tag++;
            session.SaveChanges();
            Assert.AreEqual(PersistenceState.Modified, node.PersistenceState);
            foreach (var item in allTrees.Where(item => item!=node))
              Assert.AreEqual(PersistenceState.Synchronized, item.PersistenceState);
          }
          session.SaveChanges();
          var newNode = T(node);
          using (session.DisableSaveChanges(newNode)) {
            AssertNumberOfNodesInDatabaseIs(allTrees.Count - 1);
            Assert.AreEqual(PersistenceState.New, newNode.PersistenceState);
            Assert.AreEqual(PersistenceState.Modified, node.PersistenceState);
            foreach (var item in allTrees.Where(item => item!=newNode && item!=node))
              Assert.AreEqual(PersistenceState.Synchronized, item.PersistenceState);
          }
          session.SaveChanges();
          AssertNumberOfNodesInDatabaseIs(allTrees.Count);
          var cycledNode = T();
          cycledNode.Parent = cycledNode;
          using (session.DisableSaveChanges(cycledNode)) {
            AssertNumberOfNodesInDatabaseIs(allTrees.Count - 1);
          }
          session.SaveChanges();
          var cycledGraph = T(T(T(cycledNode)));
          cycledGraph.Parent = cycledNode;
          using (session.DisableSaveChanges(cycledGraph)) {
            AssertNumberOfNodesInDatabaseIs(allTrees.Count - 3);
            Assert.AreEqual(PersistenceState.Modified, cycledNode.PersistenceState);
          }
          session.SaveChanges();
        }
      }
      finally {
        allTrees = null;
      }
    }

    [Test]
    public void NestedPinTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var victim = new Victim();
        using (session.DisableSaveChanges(victim))
          Assert.IsNull(session.DisableSaveChanges(victim));
      }
    }

    [Test]
    public void PinRemovedEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var victim = new Victim();
        victim.Remove();
        AssertEx.ThrowsInvalidOperationException(() => session.DisableSaveChanges(victim));
      }
    }

    [Test]
    public void CommittingWithPinnedEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        var victim = new Victim();
        using (session.DisableSaveChanges(victim)) {
          transactionScope.Complete();
          AssertEx.ThrowsInvalidOperationException(transactionScope.Dispose);
        }
      }      
    }

    [Test]
    public void OpenNestedTransactionWithPinnedEntityTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var victim = new Victim();
        using (session.DisableSaveChanges(victim))
          AssertEx.ThrowsInvalidOperationException(() => session.OpenTransaction(TransactionOpenMode.New));
      }
    }
    
    private static void AssertNumberOfNodesInDatabaseIs(int expected)
    {
      var actual = Session.Demand().Query.All<Node>().Count();
      Assert.AreEqual(expected, actual);
    }

    private Node T(params Node[] children)
    {
      var result = new Node();
      allTrees.Add(result);
      result.Children.AddRange(children);
      return result;
    }
  }
}