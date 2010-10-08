// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.30

using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public sealed class TransactionModesTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Sql);
    }

    [Test]
    public void DefaultTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      short reorderLevel;
      Key productKey;
      using (Domain.OpenSession(sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.All<Product>().First();
        product.ReorderLevel++;
        Session.Current.SaveChanges();
        var dbTransaction = StorageTestHelper.GetNativeTransaction();
        product.ReorderLevel++;
        Session.Current.SaveChanges();
        Assert.AreSame(dbTransaction, StorageTestHelper.GetNativeTransaction());
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
        tx.Complete();
      }

      using (Domain.OpenSession(sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }
    
    [Test]
    public void NotActivatedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      using (Domain.OpenSession(sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        tx.Complete();
      }
    }

    [Test]
    public void RollBackedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      short reorderLevel;
      Key productKey;
      using (Domain.OpenSession(sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
      }

      using (Domain.OpenSession(sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.All<Product>().First();
        reorderLevel = product.ReorderLevel;
        product.ReorderLevel++;
        productKey = product.Key;
      }

      using (Domain.OpenSession(sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = Query.Single<Product>(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }
    
    [Test]
    public void NestedTransactionsWithAutoshortenedOptionTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var sessionConfiguration = new SessionConfiguration();
      using (Domain.OpenSession(sessionConfiguration)) {
        using (var outer = Transaction.Open(TransactionOpenMode.New)) {
          Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var mid = Transaction.Open(TransactionOpenMode.New)) {
            Assert.IsTrue(outer.Transaction.IsActuallyStarted);
            Assert.IsTrue(mid.Transaction.IsActuallyStarted);
            using (var inner = Transaction.Open(TransactionOpenMode.New)) {
              Assert.IsTrue(outer.Transaction.IsActuallyStarted);
              Assert.IsTrue(mid.Transaction.IsActuallyStarted);
              Assert.IsTrue(inner.Transaction.IsActuallyStarted);
            }
          }
        }
      }

      using (Domain.OpenSession(sessionConfiguration)) {
        using (var outer = Transaction.Open(TransactionOpenMode.New)) {
          Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var inner = Transaction.Open(TransactionOpenMode.New)) {
            var lacor = Query.Single<Customer>("LACOR");
            Assert.IsTrue(outer.Transaction.IsActuallyStarted);
            Assert.IsTrue(inner.Transaction.IsActuallyStarted);
          }
        }
      }
    }
  }
}