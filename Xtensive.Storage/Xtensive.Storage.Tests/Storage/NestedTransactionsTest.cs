// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Storage.Tests.Storage
{
  public class NestedTransactionsTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      Session.Open(Domain);
    }

    public override void TestFixtureTearDown()
    {
      Session.Current.DisposeSafely();
    }

    [Test]
    public void UnmodifiedStateIsValidInInnerTransactionTest()
    {
      using (var outerScope = Transaction.Open()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
          var innerTransaction = Transaction.Current;
          AssertStateIsValid(theHexagon, outerTransaction);
          Assert.AreEqual(theHexagon.Kwanza, 0);
          AssertStateIsValid(theHexagon, outerTransaction);
          // rollback
        }
      }
    }

    [Test]
    public void ModifiedStateIsValidInInnerTransactionTest()
    {
      using (var outerScope = Transaction.Open()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
          var innerTransaction = Transaction.Current;
          theHexagon.IncreaseKwanza();
          AssertStateIsValid(theHexagon, innerTransaction);
          Assert.AreEqual(theHexagon.Kwanza, 1);
          // rollback
        }
      }
    }

    [Test]
    public void UnmodifiedStateIsValidInOuterTransactionAfterInnerTransactionRolledBackTest()
    {
      using (var outerScope = Transaction.Open()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
          // rollback
        }
        AssertStateIsValid(theHexagon, outerTransaction);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsInvalidInOuterTransactionAfterInnerTransactionRolledBackTest()
    {
      using (var outerScope = Transaction.Open()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
          theHexagon.IncreaseKwanza();
          // rollback
        }
        AssertStateIsInvalid(theHexagon);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void UnmodifiedStateIsValidInOuterTransactionAfterInnerTransactionCommitedTest()
    {
      using (var outerScope = Transaction.Open()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
          innerScope.Complete();
        }
        AssertStateIsValid(theHexagon, outerTransaction);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsValidInOuterTransactionAfterInnerTransactionCommitedTest()
    {
      using (var outerScope = Transaction.Open()) {
        var outerTransaction = Transaction.Current;
        Transaction innerTransaction;
        var theHexagon = new Hexagon();
        using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
          innerTransaction = Transaction.Current;
          theHexagon.IncreaseKwanza();
          innerScope.Complete();
        }
        AssertStateIsValid(theHexagon, innerTransaction);
        Assert.AreEqual(theHexagon.Kwanza, 1);
      }
    }

    [Test]
    public void WrongNestedTransactionUsageTest()
    {
      try {
        using (var outerScope = Transaction.Open()) {
          using (var innerScope = Transaction.Open(TransactionOpenMode.New)) {
            outerScope.Complete();
            AssertEx.ThrowsInvalidOperationException(outerScope.Dispose);
          }
        }
      }
      finally {
        Assert.IsNull(Session.Current.Transaction);
        Assert.IsNull(GetNativeTransaction());
      }
    }

    private static void AssertStateIsValid(Entity entity, Transaction expectedStateTransaction)
    {
      Assert.IsTrue(CheckStateIsActual(entity));
      Assert.AreSame(expectedStateTransaction, entity.State.StateTransaction);
    }

    private static void AssertStateIsInvalid(Entity entity)
    {
      Assert.IsFalse(CheckStateIsActual(entity));
    }
    
    private static bool CheckStateIsActual(Entity entity)
    {
      var stateTransaction = entity.State.StateTransaction;
      return stateTransaction!=null && stateTransaction.AreChangesVisibleTo(Transaction.Current);
    }
  }
}