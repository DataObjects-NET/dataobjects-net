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
    [Test]
    public void DefaultTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration();
      short reorderLevel;
      Key productKey;
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = session.Query.All<Product>().First();
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

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
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
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
        var product = session.Query.All<Product>().First();
        reorderLevel = product.ReorderLevel;
        product.ReorderLevel++;
        productKey = product.Key;
      }

      using (var session = Domain.OpenSession(sessionConfiguration))
      using (var tx = session.OpenTransaction()) {
        Assert.IsNull(StorageTestHelper.GetNativeTransaction());
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
          //Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var mid = session.OpenTransaction(TransactionOpenMode.New)) {
            //Assert.IsTrue(outer.Transaction.IsActuallyStarted);
            //Assert.IsTrue(mid.Transaction.IsActuallyStarted);
            using (var inner = session.OpenTransaction(TransactionOpenMode.New)) {
              //Assert.IsTrue(outer.Transaction.IsActuallyStarted);
              //Assert.IsTrue(mid.Transaction.IsActuallyStarted);
              //Assert.IsTrue(inner.Transaction.IsActuallyStarted);
            }
          }
        }
      }

      using (var session = Domain.OpenSession(sessionConfiguration)) {
        using (var outer = session.OpenTransaction(TransactionOpenMode.New)) {
          //Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var inner = session.OpenTransaction(TransactionOpenMode.New)) {
            var lacor = session.Query.Single<Customer>("LACOR");
            //Assert.IsTrue(outer.Transaction.IsActuallyStarted);
            //Assert.IsTrue(inner.Transaction.IsActuallyStarted);
          }
        }
      }
    }
  }
}