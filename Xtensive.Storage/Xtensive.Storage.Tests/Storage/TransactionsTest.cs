// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.27

using System.Reflection;
using NUnit.Framework;
using System;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Storage.Tests.Storage.TransactionsTestModel
{
  [HierarchyRoot]
  public class Hexagon : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Kwanza { get; set; }

    [Field]
    public Hexagon Babuka { get; set; }

    public void Wobble(int newKanza)
    {
      Kwanza = newKanza;
      throw new InvalidOperationException();
    }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class TransactionsTest : AutoBuildTest
  {    
    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
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
    public void VoidScopesTest()
    {
      using (Session.Open(Domain)) {

        using (var scope = Transaction.Open()) {
          
          Assert.IsFalse(scope.IsVoid);
          Assert.IsNotNull(scope.Transaction);

          using (var scope2 = Transaction.Open()) {
            
            Assert.IsTrue(scope2.IsVoid);
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

    [Test]
    public void NestedTransactionTest()
    {
      using (Session.Open(Domain)) {
        using (Transaction.Open()) {
          var theThing = new Hexagon();
          var outerTransaction = Transaction.Current;
          using (Transaction.Open(TransactionOpenMode.New)) {
            var innerTransaction = Transaction.Current;
            Assert.IsTrue(CheckStateIsActual(theThing.State));
            Assert.AreSame(theThing.State.StateTransaction, outerTransaction);
            theThing.Kwanza = 5;
            Assert.AreSame(theThing.State.StateTransaction, innerTransaction);
            // rollback
          }
          Assert.IsFalse(CheckStateIsActual(theThing.State));
        }
      }
    }

    [Test]
    public void WrongNestedTransactionUsageTest()
    {
      using (Session.Open(Domain)) {
        try {
          using (var outer = Transaction.Open()) {
            using (var inner = Transaction.Open(TransactionOpenMode.New)) {
              AssertEx.ThrowsInvalidOperationException(outer.Dispose);
            }
          }
        }
        finally {
          Assert.IsNull(Session.Current.Transaction);
          Assert.IsNull(GetNativeTransaction());
        }
      }
    }

    private static bool CheckStateIsActual(EntityState state)
    {
      var result = typeof (TransactionalStateContainer<Tuple>).InvokeMember(
        "CheckStateIsActual",
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
        null, state, new object[0]);
      return (bool) result;
    }
  }
}