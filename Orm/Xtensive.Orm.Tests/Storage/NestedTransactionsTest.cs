// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Data.SqlClient;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class NestedTransactionsTest : TransactionsTestBase
  {
    private StorageProviderInfo storageProviderInfo;
    private Session globalSession;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      globalSession = Domain.OpenSession();
      storageProviderInfo = StorageProviderInfo.Instance;
    }

    protected override void CheckRequirements()
      => Require.AllFeaturesSupported(ProviderFeatures.Savepoints);

    public override void TestFixtureTearDown() => globalSession.DisposeSafely();

    [Test]
    public void UnmodifiedStateIsValidInInnerTransactionTest()
    {
      using (var outerScope = globalSession.OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
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
      using (var outerScope = globalSession.OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
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
      using (var outerScope = globalSession.OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
          // rollback
        }
        AssertStateIsValid(theHexagon);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsInvalidInOuterTransactionAfterInnerTransactionRolledBackTest()
    {
      using (var outerScope = globalSession.OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
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
      using (var outerScope = globalSession.OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
          innerScope.Complete();
        }
        AssertStateIsValid(theHexagon);
        Assert.AreEqual(theHexagon.Kwanza, 0);
      }
    }

    [Test]
    public void ModifiedStateIsValidInOuterTransactionAfterInnerTransactionCommitedTest()
    {
      using (var outerScope = globalSession.OpenTransaction()) {
        var outerTransaction = Transaction.Current;
        var theHexagon = new Hexagon();
        using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
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
      using (var outerScope = globalSession.OpenTransaction())
      using (var innerScope = globalSession.OpenTransaction(TransactionOpenMode.New)) {
        outerScope.Complete();
        AssertEx.ThrowsInvalidOperationException(outerScope.Dispose);
      }
      Assert.IsNull(Session.Current.Transaction);
      Assert.IsNull(StorageTestHelper.GetNativeTransaction());
    }

    [Test]
    public void RollbackNestedTransactionWithActiveEnumeratorTest()
    {
      var session = Session.Demand();
      using (var outerTx = session.OpenTransaction()) {
        _ = new Hexagon();
        _ = new Hexagon();
        _ = new Hexagon();

        IEnumerator<int> enumerator = null;
        var innerTx = session.OpenTransaction(TransactionOpenMode.New);

        enumerator = session.Query.All<Hexagon>()
          .Select(item => item.Id).AsEnumerable().GetEnumerator();
        _ = enumerator.MoveNext();

        if (storageProviderInfo.CheckProviderIs(StorageProvider.SqlServer)) {
          _ = Assert.Throws<StorageException>(() => innerTx.Dispose());
        }
        else {
          Assert.DoesNotThrow(() => innerTx.Dispose());
        }
      }
    }

    [Test]
    public void RollbackNestedTransactionWithActiveEnumeratorAndThenCompleteOutermostTest()
    {
      var session = Session.Demand();
      var outerTx = session.OpenTransaction();
      _ = new Hexagon();
      _ = new Hexagon();
      _ = new Hexagon();

      IEnumerator<int> enumerator = null;
      var innerTx = session.OpenTransaction(TransactionOpenMode.New);

      enumerator = session.Query.All<Hexagon>()
        .Select(item => item.Id).AsEnumerable().GetEnumerator();
      _ = enumerator.MoveNext();

      if (storageProviderInfo.CheckProviderIs(StorageProvider.SqlServer)) {
        _ = Assert.Throws<StorageException>(() => innerTx.Dispose());
      }
      else {
        Assert.DoesNotThrow(() => innerTx.Dispose());
      }
      outerTx.Complete();
      if (storageProviderInfo.CheckProviderIs(StorageProvider.SqlServer)) {
        _ = Assert.Throws<StorageException>(() => outerTx.Dispose());
      }
      else {
        Assert.DoesNotThrow(() => outerTx.Dispose());
      }
    }

    [Test]
    public void CommitNestedTransactionWithActiveEnumeratorAndRollbackOutermostTest()
    {
      var session = Session.Demand();
      using (var outerTx = session.OpenTransaction()) {
        _ = new Hexagon();
        _ = new Hexagon();
        _ = new Hexagon();

        IEnumerator<int> enumerator = null;
        var innerTx = session.OpenTransaction(TransactionOpenMode.New);

        enumerator = session.Query.All<Hexagon>()
          .Select(item => item.Id).AsEnumerable().GetEnumerator();
        _ = enumerator.MoveNext();

        innerTx.Complete();
        innerTx.Dispose();
      }
    }

    [Test]
    public void CommitNestedTransactionWithActiveEnumeratorAndCommitOutermostTest()
    {
      var session = Session.Demand();
      var outerTx = session.OpenTransaction();
      _ = new Hexagon();
      _ = new Hexagon();
      _ = new Hexagon();

      IEnumerator<int> enumerator = null;
      var innerTx = session.OpenTransaction(TransactionOpenMode.New);

      enumerator = session.Query.All<Hexagon>()
        .Select(item => item.Id).AsEnumerable().GetEnumerator();
      _ = enumerator.MoveNext();

      innerTx.Complete();
      innerTx.Dispose();

      outerTx.Complete();
      if (storageProviderInfo.CheckProviderIs(StorageProvider.SqlServer)) {
        var exception = Assert.Throws<StorageException>(() => outerTx.Dispose());
        Assert.That(exception.InnerException, Is.InstanceOf<Microsoft.Data.SqlClient.SqlException>());
      }
      else {
        Assert.DoesNotThrow(() => outerTx.Dispose());
      }
    }
  }
}