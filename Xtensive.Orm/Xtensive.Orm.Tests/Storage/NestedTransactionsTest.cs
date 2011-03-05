// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using NUnit.Framework;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class NestedTransactionsTest : AutoBuildTest
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

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      Domain.OpenSession();
    }

    public override void TestFixtureTearDown()
    {
      Session.Current.DisposeSafely();
    }

    [Test]
    public void UnmodifiedStateIsValidInInnerTransactionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
          // rollback
        }
        AssertStateIsValid(theHexagon, outerTransaction);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsInvalidInOuterTransactionAfterInnerTransactionRolledBackTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
          innerScope.Complete();
        }
        AssertStateIsValid(theHexagon, outerTransaction);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsValidInOuterTransactionAfterInnerTransactionCommitedTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        Transaction innerTransaction;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction())
      using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
        outerScope.Complete();
        AssertEx.ThrowsInvalidOperationException(outerScope.Dispose);
      }
      Assert.IsNull(Session.Current.Transaction);
      Assert.IsNull(StorageTestHelper.GetNativeTransaction());
    }

    private static void AssertStateIsValid(Entity entity, Transaction expectedStateTransaction)
    {
      Assert.IsTrue(CheckStateIsActual(entity));
      Assert.AreSame(expectedStateTransaction, entity.State.Transaction);
    }

    private static void AssertStateIsInvalid(Entity entity)
    {
      Assert.IsFalse(CheckStateIsActual(entity));
    }
    
    private static bool CheckStateIsActual(Entity entity)
    {
      var stateTransaction = entity.State.Transaction;
      return stateTransaction!=null && stateTransaction.AreChangesVisibleTo(Transaction.Current);
    }
  }
}