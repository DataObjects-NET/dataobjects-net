// Copyright (C) 2009 Xtensive LLC.
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

  [HierarchyRoot]
  public class Node : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Node> Children { get; set; }
   
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
    public void SimpleTest()
    {
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var butcher = new Killer();
        var firstVictim = new Victim();
        // "Killers" who have not killed yet should not be considered as killers
        using (session.Pin(butcher)) {
          session.Persist();
          Assert.AreEqual(PersistenceState.New, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, firstVictim.PersistenceState);
          butcher.Kill(firstVictim);
          session.Persist();
          Assert.AreEqual(PersistenceState.New, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Modified, firstVictim.PersistenceState);
        }
        session.Persist();
        using (session.Pin(butcher)) {
          firstVictim.Resurrect();
          var secondVictim = new Victim();
          butcher.Kill(secondVictim);
          session.Persist();
          Assert.AreEqual(PersistenceState.Modified, butcher.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, firstVictim.PersistenceState);
          Assert.AreEqual(PersistenceState.Synchronized, secondVictim.PersistenceState);
        }
      }
    }

    [Test]
    public void PinLargeGraphTest()
    {
      try {
        allTrees = new List<Node>();
        using (var session = Session.Open(Domain))
        using (Transaction.Open()) {
          var tree = T(T(), T(), T(T()));
          using (session.Pin(tree)) {
            int numberOfTreesInDatabase = Query<Node>.All.Count();
            Assert.AreEqual(0, numberOfTreesInDatabase);
          }
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
    public void CommitingWithPinnedEntityTest()
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
    
    private Node T(params Node[] children)
    {
      var result = new Node();
      allTrees.Add(result);
      result.Children.AddRange(children);
      return result;
    }
  }
}