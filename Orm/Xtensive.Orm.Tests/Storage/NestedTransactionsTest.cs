// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class NestedTransactionsTest : TransactionsTestBase
  {
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
          AssertStateIsValid(theHexagon);
          Assert.AreEqual(theHexagon.Kwanza, 0);
          AssertStateIsValid(theHexagon);
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
          theHexagon.IncreaseKwanza();
          AssertStateIsValid(theHexagon);
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
        AssertStateIsValid(theHexagon);
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
        AssertStateIsValid(theHexagon);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsValidInOuterTransactionAfterInnerTransactionCommitedTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      using (var outerScope = Session.Demand().OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = Session.Demand().OpenTransaction(TransactionOpenMode.New)) {
          theHexagon.IncreaseKwanza();
          innerScope.Complete();
        }
        AssertStateIsValid(theHexagon);
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
  }
}