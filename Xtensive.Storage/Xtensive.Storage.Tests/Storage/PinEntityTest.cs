// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.PinEntityTestModel;

namespace Xtensive.Storage.Tests.Storage.PinEntityTestModel
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

namespace Xtensive.Storage.Tests.Storage
{
  public class PinEntityTest : AutoBuildTest
  {
    private List<Node> allTrees;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Victim).Assembly, typeof (Victim).Namespace);
      return configuration;
    }
    
    [Test]
    public void SimplePinTest()
    {
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var butcher = new Killer();
        var firstVictim = new Victim();
        // "Killers" who have not killed yet should not be considered as killers
        using (session.Pin(butcher)) {
          session.SaveChanges();
          Assert.AreEqual(PersistenceState.New, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, firstVictim.PersistenceState);
          butcher.Kill(firstVictim);
          session.SaveChanges();
          Assert.AreEqual(PersistenceState.New, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Modified, firstVictim.PersistenceState);
        }
        session.SaveChanges();
        using (session.Pin(butcher)) {
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
        using (var session = Session.Open(Domain))
        using (Transaction.Open()) {
          var node = T(T(), T(), T(T()));
          using (session.Pin(node)) {
            AssertNumberOfNodesInDatabaseIs(0);
            foreach (var item in allTrees)
              Assert.AreEqual(PersistenceState.New, item.PersistenceState);
          }
          session.SaveChanges();
          AssertNumberOfNodesInDatabaseIs(allTrees.Count);
          using (session.Pin(node)) {
            foreach (var item in allTrees)
              item.Tag++;
            session.SaveChanges();
            Assert.AreEqual(PersistenceState.Modified, node.PersistenceState);
            foreach (var item in allTrees.Where(item => item!=node))
              Assert.AreEqual(PersistenceState.Synchronized, item.PersistenceState);
          }
          session.SaveChanges();
          var newNode = T(node);
          using (session.Pin(newNode)) {
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
          using (session.Pin(cycledNode)) {
            AssertNumberOfNodesInDatabaseIs(allTrees.Count - 1);
          }
          session.SaveChanges();
          var cycledGraph = T(T(T(cycledNode)));
          cycledGraph.Parent = cycledNode;
          using (session.Pin(cycledGraph)) {
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
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var victim = new Victim();
        using (session.Pin(victim))
          Assert.IsNull(session.Pin(victim));
      }
    }

    [Test]
    public void PinRemovedEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var victim = new Victim();
        victim.Remove();
        AssertEx.ThrowsInvalidOperationException(() => session.Pin(victim));
      }
    }

    [Test]
    public void CommittingWithPinnedEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (var transactionScope = Transaction.Open()) {
        var victim = new Victim();
        using (session.Pin(victim)) {
          transactionScope.Complete();
          AssertEx.ThrowsInvalidOperationException(transactionScope.Dispose);
        }
      }      
    }

    [Test]
    public void OpenNestedTransactionWithPinnedEntityTest()
    {
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var victim = new Victim();
        using (session.Pin(victim))
          AssertEx.ThrowsInvalidOperationException(() => Transaction.Open(TransactionOpenMode.New));
      }
    }
    
    private void AssertNumberOfNodesInDatabaseIs(int expected)
    {
      var actual = Query.All<Node>().Count();
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