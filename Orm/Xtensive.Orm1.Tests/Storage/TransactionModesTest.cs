// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.30

using NUnit.Framework;
using System.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public sealed class TransactionModesTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesNotSupported(ProviderFeatures.ExclusiveWriterConnection);
    }

    [Test]
    public void DefaultTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      short reorderLevel;
      Key productKey;
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var product = session.Query.All<Product>().First();
        product.ReorderLevel++;
        Session.Current.SaveChanges();
        Assert.IsTrue(session.Handler.TransactionIsStarted);
        var dbTransaction = StorageTestHelper.GetNativeTransaction();
        product.ReorderLevel++;
        Session.Current.SaveChanges();
        Assert.AreSame(dbTransaction, StorageTestHelper.GetNativeTransaction());
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
        tx.Complete();
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var product = session.Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
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
      short reorderLevel;
      Key productKey;
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var product = session.Query.All<Product>().First();
        reorderLevel = product.ReorderLevel;
        product.ReorderLevel++;
        productKey = product.Key;
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsFalse(session.Handler.TransactionIsStarted);
        var product = session.Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
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
            var lacor = session.Query.Single<Customer>("LACOR");
            Assert.IsTrue(session.Handler.TransactionIsStarted);
          }
        }
      }
    }
  }
}