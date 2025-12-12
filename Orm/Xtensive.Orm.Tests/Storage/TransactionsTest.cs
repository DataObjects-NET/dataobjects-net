// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.27

using NUnit.Framework;
using System;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage.TransactionsTestModel
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

namespace Xtensive.Orm.Tests.Storage
{
  public class TransactionsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
      return configuration;
    }

    [Test]
    public void RollbackCreationTest()
    {
      using (var session = Domain.OpenSession()) {
        Hexagon hexagon;
        using (session.OpenTransaction()) {
          hexagon = new Hexagon();
        }
        using (session.OpenTransaction()) {
          Assert.That(hexagon.IsRemoved, Is.True);
        }
        AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 15; });
      }
    }

    [Test]
    public void RollbackRemovingTest()
    {
      using (var session = Domain.OpenSession()) {
        Hexagon hexagon;
        using (var t = session.OpenTransaction()) {
          hexagon = new Hexagon {Kwanza = 36};
          t.Complete();
        }
        using (session.OpenTransaction()) {
          hexagon.Remove();
          AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 20; });
          // rolling back removal
        }
        using (session.OpenTransaction()) {
          hexagon.Kwanza = 14;
          Assert.That(hexagon.Kwanza, Is.EqualTo(14));
        }
      }
    }

    [Test]
    public void VoidScopesTest()
    {
      using (var session = Domain.OpenSession()) {

        using (var scope = session.OpenTransaction()) {

          Assert.That(scope.IsVoid, Is.False);
          Assert.That(scope.Transaction, Is.Not.Null);

          using (var scope2 = session.OpenTransaction()) {

            Assert.That(scope2.IsVoid, Is.True);
            Assert.That(scope2.Transaction, Is.Null);

            using (var scope3 = session.OpenTransaction()) {
              Assert.That(ReferenceEquals(scope2, scope3), Is.True);
            }
          }
          scope.Complete();
        }
      }
    }

    [Test]
    public void RollbackModifyingTest()
    {
      using (var session = Domain.OpenSession()) {
        Hexagon hexagon;

        using (var t = session.OpenTransaction()) {
          hexagon = new Hexagon {Kwanza = 3};
          t.Complete();
        }
        using (session.OpenTransaction()) {
          Assert.That(hexagon.Kwanza, Is.EqualTo(3));
          Assert.That(hexagon.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        }

        using (session.OpenTransaction()) {
          hexagon.Kwanza = 11;
        }
        using (session.OpenTransaction()) {
          Assert.That(hexagon.Kwanza, Is.EqualTo(3));
          Assert.That(hexagon.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        }
        using (session.OpenTransaction()) {
          hexagon.Babuka = new Hexagon();
        }
        using (session.OpenTransaction()) {
          Assert.That(hexagon.Babuka, Is.Null);
          Assert.That(hexagon.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        }
        using (session.OpenTransaction()) {
          hexagon.Kwanza = 12;
          Session.Current.SaveChanges();
        }
        using (session.OpenTransaction()) {
          Assert.That(hexagon.Kwanza, Is.EqualTo(3));
          Assert.That(hexagon.PersistenceState, Is.EqualTo(PersistenceState.Synchronized));
        }
        using (session.OpenTransaction()) {
          try {
            hexagon.Wobble(18);
          }
          catch (InvalidOperationException) {
          }
        }
        using (session.OpenTransaction()) {
          Assert.That(hexagon.Kwanza, Is.EqualTo(3));
        }
      }
    }

    [Test]
    public void TransactionEventsTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var hexagon = new Hexagon {Kwanza = 1};

        session.Events.TransactionOpened +=
          (sender, args) => Assert.That(args.Transaction, Is.EqualTo(Transaction.Current));
        session.Events.TransactionCommitting +=
          (sender, args) => Assert.That(args.Transaction, Is.EqualTo(Transaction.Current));
        session.Events.TransactionRollbacking +=
          (sender, args) => Assert.That(args.Transaction, Is.EqualTo(Transaction.Current));

        session.Events.TransactionOpening +=
          (sender, args) => Assert.That(args.Transaction, Is.Not.EqualTo(Transaction.Current));
        session.Events.TransactionCommitted +=
          (sender, args) => Assert.That(args.Transaction, Is.Not.EqualTo(Transaction.Current));
        session.Events.TransactionRollbacked +=
          (sender, args) => Assert.That(args.Transaction, Is.Not.EqualTo(Transaction.Current));

        using (var nestedScope = session.OpenTransaction(TransactionOpenMode.New)) {
          hexagon.Kwanza = 2;
          // Rollback
        }

        using (var nestedScope = session.OpenTransaction(TransactionOpenMode.New)) {
          hexagon.Kwanza = 2;
          nestedScope.Complete();
        }
      }
    }
  }
}