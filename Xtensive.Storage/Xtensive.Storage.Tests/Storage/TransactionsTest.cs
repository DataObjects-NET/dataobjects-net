// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.27

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.TranscationsTest
{
  public class TransactionsTest : AutoBuildTest
  {    
    [HierarchyRoot]
    public class Hexagon : Entity
    {
      [Field, Key]
      public int ID { get; private set; }

      [Field]
      public int Kwanza { get; set;}

      [Field]
      public Hexagon Babuka { get; set;}

      public void Wobble(int newKanza)
      {
        Kwanza = newKanza;
        throw new InvalidOperationException();
      }
    }

    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.TranscationsTest");
      return config;
    }

    [Test]
    public void RollbackCreationTest()
    {
      using (Session.Open(Domain)) {
        Hexagon hexagon;
        using (Transaction.Open()) {
          hexagon = new Hexagon();
        }
        using (Transaction.Open()) {
          Assert.IsTrue(hexagon.IsRemoved);
        }
        AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 15; });
      }
    }

    [Test]
    public void RollbackRemovingTest()
    {
      using (Session.Open(Domain)) {
        Hexagon hexagon;
        using (var t = Transaction.Open()) {
          hexagon = new Hexagon {Kwanza = 36};
          t.Complete();
        }
        using (Transaction.Open()) {
          hexagon.Remove();
          AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 20; });
          // rolling back removal
        }
        using (Transaction.Open()) {
          hexagon.Kwanza = 14;
          Assert.AreEqual(14, hexagon.Kwanza);
        }
      }
    }

    [Test]
    public void HollowScopesTest()
    {
      using (Session.Open(Domain)) {

        using (var scope = Transaction.Open()) {
          
          Assert.IsFalse(scope.IsHollow);
          Assert.IsNotNull(scope.Transaction);

          using (var scope2 = Transaction.Open()) {
            
            Assert.IsTrue(scope2.IsHollow);
            Assert.IsNull(scope2.Transaction);

            using (var scope3 = Transaction.Open()) {
              Assert.IsTrue(ReferenceEquals(scope2, scope3));
            }
          }
          scope.Complete();
        }
      }
    }

    [Test]
    public void RollbackModifyingTest()
    {
      using (Session.Open(Domain)) {
        Hexagon hexagon;

        using (var t = Transaction.Open()) {
          hexagon = new Hexagon {Kwanza = 3};
          t.Complete();
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }

        using (Transaction.Open()) {
          hexagon.Kwanza = 11;
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }
        using (Transaction.Open()) {
          hexagon.Babuka = new Hexagon();
        }
        using (Transaction.Open()) {
          Assert.IsNull(hexagon.Babuka);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }
        using (Transaction.Open()) {
          hexagon.Kwanza = 12;
          Session.Current.Persist();
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }
        using (Transaction.Open()) {
          try {
            hexagon.Wobble(18);
          }
          catch (InvalidOperationException) {
          }
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
        }
      } 
    }
  }
}