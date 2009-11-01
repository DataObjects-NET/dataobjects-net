//// Copyright (C) 2007 Xtensive LLC.
//// All rights reserved.
//// For conditions of distribution and use, see license.
//// Created by: Alexey Kochetov
//// Created:    2007.09.17
//
//using System;
//using NUnit.Framework;
//using Xtensive.Integrity.Transactions;
//using Xtensive.Storage.Configuration;
//
//namespace Xtensive.Storage.Tests.Transactions
//{
//  [TestFixture]
//  public class TransactionScopeTest
//  {
//    private StorageScope scope;
//    private SessionScope sessionScope;
//
//    [TestFixtureSetUp]
//    public void Init()
//    {
//      DomainConfiguration storageConfiguration = new DomainConfiguration();
//      SessionConfiguration sessionConfiguration = new SessionConfiguration();
//      scope = new StorageScope(storageConfiguration);
//      sessionScope = new SessionScope(sessionConfiguration);
//    }
//
//    [TestFixtureTearDown]
//    public void Dispose()
//    {
//      sessionScope.Dispose();
//      scope.Dispose();
//    }
//
//#if DEBUG
//    [Test]
//    [ExpectedException(typeof (ApplicationException))]
//    public void RollbackTest()
//    {
//      using (new TransactionScope(SessionScope.Current.Session)) {
//        throw new ApplicationException();
//      }
//    }
//
//    [Ignore("Not implemented")]
//    [Test]
//    public void CommitTest()
//    {
//      using (TransactionScope transactionScope = new TransactionScope(SessionScope.Current.Session)) {
//        transactionScope.Complete();
//        Assert.AreEqual(TransactionState.Active, transactionScope.Transaction.State);
//      }
//    }
//
//    [Test]
//    [ExpectedException(typeof (ApplicationException))]
//    public void RollbackNestedScopeTest()
//    {
//      Transaction tran = null;
//      try {
//        using (TransactionScope transactionScope = new TransactionScope(SessionScope.Current.Session)) {
//          tran = transactionScope.Transaction;
//          try {
//            using (TransactionScope nestedTransactionScope = new TransactionScope(SessionScope.Current.Session, TransactionScopeOptions.ExistingTransactionRequired)) {
//              Assert.AreEqual(tran, nestedTransactionScope.Transaction);
//              throw new ApplicationException();
//            }
//          }
//          finally {
//            Assert.AreEqual(TransactionState.Active, tran.State);
//          }
//        }
//      }
//      finally {
//        Assert.AreNotEqual(null, tran);
//        Assert.AreEqual(TransactionState.RolledBack, tran.State);
//      }
//    }
//
//    [Test]
//    [ExpectedException(typeof (ApplicationException))]
//    public void RollbackOuterScopeTest()
//    {
//      Transaction tran = null;
//      try {
//        using (TransactionScope transactionScope = new TransactionScope(SessionScope.Current.Session)) {
//          tran = transactionScope.Transaction;
//          try {
//            using (TransactionScope nestedTransactionScope = new TransactionScope(SessionScope.Current.Session, TransactionScopeOptions.ExistingTransactionRequired)) {
//              Assert.AreEqual(tran, nestedTransactionScope.Transaction);
//              nestedTransactionScope.Complete();
//            }
//          }
//          finally {
//            Assert.AreEqual(TransactionState.Active, tran.State);
//          }
//          throw new ApplicationException();
//        }
//      }
//      finally {
//        Assert.AreNotEqual(null, tran);
//        Assert.AreEqual(TransactionState.RolledBack, tran.State);
//      }
//    }
//
//
//    [Test]
//    [ExpectedException(typeof (ApplicationException))]
//    public void RollbackNestedTransactionTest()
//    {
//      Transaction tran = null;
//      try {
//        using (TransactionScope transactionScope = new TransactionScope(SessionScope.Current.Session)) {
//          tran = transactionScope.Transaction;
//          try {
//            using (
//              TransactionScope nestedTransactionScope =
//                  new TransactionScope(SessionScope.Current.Session, TransactionScopeOptions.NewTransactionRequired)) {
//              Assert.AreNotEqual(tran, nestedTransactionScope.Transaction);
//              throw new ApplicationException();
//            }
//          }
//          finally {
//            Assert.AreEqual(TransactionState.Active, tran.State);
//          }
//        }
//      }
//      finally {
//        Assert.AreNotEqual(null, tran);
//        Assert.AreEqual(TransactionState.RolledBack, tran.State);
//      }
//    }
//
//    [Test]
//    [ExpectedException(typeof (ApplicationException))]
//    public void RollbackOuterTransactionTest()
//    {
//      Transaction tran = null;
//      try {
//        using (TransactionScope transactionScope = new TransactionScope(SessionScope.Current.Session)) {
//          tran = transactionScope.Transaction;
//          try {
//            using (
//              TransactionScope nestedTransactionScope =
//                new TransactionScope(SessionScope.Current.Session, TransactionScopeOptions.NewTransactionRequired)) {
//              Assert.AreNotEqual(tran, nestedTransactionScope.Transaction);
//              nestedTransactionScope.Complete();
//            }
//          }
//          finally {
//            Assert.AreEqual(TransactionState.Active, tran.State);
//          }
//          throw new ApplicationException();
//        }
//      }
//      finally {
//        Assert.AreNotEqual(null, tran);
//        Assert.AreEqual(TransactionState.RolledBack, tran.State);
//      }
//    }
//
//    [Test]
//    [ExpectedException(typeof (InvalidOperationException))]
//    public void InvalidOuterTransactionTest()
//    {
//      using (TransactionScope transactionScope = new TransactionScope(SessionScope.Current.Session)) {
//        transactionScope.Complete();  
//        using (TransactionScope nestedTransactionScope = new TransactionScope(SessionScope.Current.Session, TransactionScopeOptions.NewTransactionRequired)) {
//          nestedTransactionScope.Complete();
//        }
//      }
//    }
//  
//#endif
//    
//
//
//  }
//}
