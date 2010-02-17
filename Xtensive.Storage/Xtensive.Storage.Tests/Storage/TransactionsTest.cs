// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.27

using NUnit.Framework;
using System;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Storage.Tests.Storage.TransactionsTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Hexagon : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Kwanza { get; set; }

    [Field]
    public Hexagon Babuka { get; set; }

    public void IncreaseKwanza()
    {
      Kwanza++;
    }

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
      Require.ProviderIs(StorageProvider.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
      return configuration;
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
    public void TransactionEventsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var session = Session.Open(Domain))
      using (Transaction.Open()) {
        var hexagon = new Hexagon {Kwanza = 1};

        session.TransactionOpened +=
          (sender, args) => Assert.AreEqual(Transaction.Current, args.Transaction);
        session.TransactionCommitting +=
          (sender, args) => Assert.AreEqual(Transaction.Current, args.Transaction);
        session.TransactionRollbacking +=
          (sender, args) => Assert.AreEqual(Transaction.Current, args.Transaction);

        session.TransactionOpening +=
          (sender, args) => Assert.AreNotEqual(Transaction.Current, args.Transaction);
        session.TransactionCommitted +=
          (sender, args) => Assert.AreNotEqual(Transaction.Current, args.Transaction);
        session.TransactionRollbacked +=
          (sender, args) => Assert.AreNotEqual(Transaction.Current, args.Transaction);

        using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {
          hexagon.Kwanza = 2;
          // Rollback
        }

        using (var nestedScope = Transaction.Open(TransactionOpenMode.New)) {
          hexagon.Kwanza = 2;
          nestedScope.Complete();
        }
      }
    }
  }
}