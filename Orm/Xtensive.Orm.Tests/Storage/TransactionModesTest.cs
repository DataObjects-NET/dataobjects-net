// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.07.30

using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public sealed class TransactionModesTest : ChinookDOModelTest
  {
    public override void SetUp()
    {
      DoNotActivateSharedSession = true;
      base.SetUp();
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesNotSupported(ProviderFeatures.ExclusiveWriterConnection);
    }

    [Test]
    public void DefaultTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      int milliseconds;
      Key trackKey;
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var track = session.Query.All<Track>().First();
        track.Milliseconds++;
        session.SaveChanges();
        Assert.IsTrue(session.Handler.TransactionIsStarted);
        object dbTransaction;
        using (session.Activate()) {
          dbTransaction = StorageTestHelper.GetNativeTransaction();
        }
        track.Milliseconds++;
        session.SaveChanges();
        using (session.Activate()) {
          Assert.AreSame(dbTransaction, StorageTestHelper.GetNativeTransaction());
        }
        track.Milliseconds++;
        milliseconds = track.Milliseconds;
        trackKey = track.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var track = session.Query.Single<Track>(trackKey);
        Assert.AreEqual(milliseconds, track.Milliseconds);
      }
    }
    
    [Test]
    public void NotActivatedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        tx.Complete();
      }
    }

    [Test]
    public void RollBackedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      int milliseconds;
      Key trackKey;
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var product = session.Query.All<Track>().First();
        milliseconds = product.Milliseconds;
        product.Milliseconds++;
        trackKey = product.Key;
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var product = session.Query.Single<Track>(trackKey);
        Assert.AreEqual(milliseconds, product.Milliseconds);
      }
    }
    
    [Test]
    public void NestedTransactionsWithAutoshortenedOptionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var sessionConfiguration = new SessionConfiguration();
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        using (var outer = session.OpenTransaction(TransactionOpenMode.New)) {
          Assert.IsFalse(session.Handler.TransactionIsStarted);
          using (var mid = session.OpenTransaction(TransactionOpenMode.New)) {
            Assert.IsTrue(session.Handler.TransactionIsStarted);
            using (var inner = session.OpenTransaction(TransactionOpenMode.New)) {
              Assert.IsTrue(session.Handler.TransactionIsStarted);
            }
          }
        }
      }

      using (var session = Domain.OpenSession(sessionConfiguration)) {
        using (var outer = session.OpenTransaction(TransactionOpenMode.New)) {
          Assert.IsFalse(session.Handler.TransactionIsStarted);
          using (var inner = session.OpenTransaction(TransactionOpenMode.New)) {
            var lacor = session.Query.All<Customer>().First();
            Assert.IsTrue(session.Handler.TransactionIsStarted);
          }
        }
      }
    }
  }
}