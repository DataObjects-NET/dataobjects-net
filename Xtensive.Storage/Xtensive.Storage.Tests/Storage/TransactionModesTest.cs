// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.30

using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public sealed class TransactionModesTest : NorthwindDOModelTest
  {
    protected override void CheckRequirements()
    {
      EnsureProtocolIs(StorageProtocol.Sql);
    }

    [Test]
    public void DefaultTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions};
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.All.First();
        product.ReorderLevel++;
        Session.Current.Persist();
        var dbTransaction = GetNativeTransaction();
        product.ReorderLevel++;
        Session.Current.Persist();
        Assert.AreSame(dbTransaction, GetNativeTransaction());
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
        tx.Complete();
      }

      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.Single(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }
    
    [Test]
    public void NotActivatedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions};
      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(GetNativeTransaction());
        tx.Complete();
      }
    }

    [Test]
    public void RollBackedTransactionTest()
    {
      var sessionConfiguration = new SessionConfiguration {Options = SessionOptions.AutoShortenTransactions};
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(GetNativeTransaction());
      }

      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.All.First();
        reorderLevel = product.ReorderLevel;
        product.ReorderLevel++;
        productKey = product.Key;
      }

      using (Session.Open(Domain, sessionConfiguration))
      using (var tx = Transaction.Open()) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.Single(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }
    
    [Test]
    public void AutoTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoShortenTransactions
      };
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.All.Where(p => p.Id > 0).First();
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
      }

      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.Single(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }

    [Test]
    public void AmbientTransactionsTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoShortenTransactions | SessionOptions.AmbientTransactions
      };
      short reorderLevel;
      Key productKey;
      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.All.Where(p => p.Id > 0).First();
        product.ReorderLevel++;
        reorderLevel = product.ReorderLevel;
        productKey = product.Key;
        Assert.IsNotNull(GetNativeTransaction());
        Session.Current.CommitAmbientTransaction();
      }

      using (Session.Open(Domain, sessionConfiguration)) {
        Assert.IsNull(GetNativeTransaction());
        var product = Query<Product>.Single(productKey);
        Assert.AreEqual(reorderLevel, product.ReorderLevel);
      }
    }

    [Test]
    public void NestedTransactionsWithAmbientOptionTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AmbientTransactions
      };

      using (var session = Session.Open(Domain, sessionConfiguration))
        try {
          var anton = Query<Customer>.Single("ANTON");
          var firstTransaction = Transaction.Current;
          using (Transaction.Open(TransactionOpenMode.Auto)) {
            var lacor = Query<Customer>.Single("LACOR");
            Assert.AreSame(firstTransaction, Transaction.Current);
          }
          using (Transaction.Open(TransactionOpenMode.New)) {
            var bergs = Query<Customer>.Single("BERGS");
            Assert.AreNotSame(firstTransaction, Transaction.Current);
          }
        }
        finally {
          session.RollbackAmbientTransaction();
        }

      using (var session = Session.Open(Domain, sessionConfiguration))
        try {
          Transaction outerTransaction;
          using (Transaction.Open(TransactionOpenMode.New)) {
            Assert.IsTrue(Transaction.Current.IsNested);
            outerTransaction = Transaction.Current.Outer;
          }
          Assert.AreSame(outerTransaction, Transaction.Current);
        }
        finally {
          session.RollbackAmbientTransaction();
        }
    }

    [Test]
    public void NestedTransactionsWithAutoshortenedOptionTest()
    {
      var sessionConfiguration = new SessionConfiguration {
        Options = SessionOptions.AutoShortenTransactions
      };
      using (Session.Open(Domain, sessionConfiguration)) {
        using (var outer = Transaction.Open(TransactionOpenMode.New)) {
          Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var mid = Transaction.Open(TransactionOpenMode.New)) {
            Assert.IsFalse(outer.Transaction.IsActuallyStarted);
            Assert.IsFalse(mid.Transaction.IsActuallyStarted);
            using (var inner = Transaction.Open(TransactionOpenMode.New)) {
              Assert.IsFalse(outer.Transaction.IsActuallyStarted);
              Assert.IsFalse(mid.Transaction.IsActuallyStarted);
              Assert.IsFalse(inner.Transaction.IsActuallyStarted);
            }
          }
        }
      }

      using (Session.Open(Domain, sessionConfiguration)) {
        using (var outer = Transaction.Open(TransactionOpenMode.New)) {
          Assert.IsFalse(outer.Transaction.IsActuallyStarted);
          using (var inner = Transaction.Open(TransactionOpenMode.New)) {
            var lacor = Query<Customer>.Single("LACOR");
            Assert.IsTrue(outer.Transaction.IsActuallyStarted);
            Assert.IsTrue(inner.Transaction.IsActuallyStarted);
          }
        }
      }
    }
  }
}