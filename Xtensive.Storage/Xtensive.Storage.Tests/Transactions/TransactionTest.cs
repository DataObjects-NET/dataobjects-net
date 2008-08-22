//// Copyright (C) 2007 Xtensive LLC.
//// All rights reserved.
//// For conditions of distribution and use, see license.
//// Created by: Alexey Kochetov
//// Created:    2007.09.20
//
//using System;
//using System.Data;
//using NUnit.Framework;
//using Xtensive.Integrity.Transactions;
//using Xtensive.Storage.Configuration;
//
//namespace Xtensive.Storage.Tests.Transactions
//{
//  [TestFixture]
//  public class TransactionTest
//  {
//    private Domain domain;
//    private SessionScope sessionScope;
//
//    [TestFixtureSetUp]
//    public void Init()
//    {
//      DomainConfiguration domainConfiguration = new DomainConfiguration();     
//
//      domain = new Domain(domainConfiguration);
//      sessionScope = domain.OpenSession();
//    }
//
//    [TestFixtureTearDown]
//    public void Dispose()
//    {
//      sessionScope.Dispose();      
//    }
//
//    [Ignore("Not implemented")]
//    [Test]
//    public void Test()
//    {
//      using (Session.Current.ActivateTransaction()) {
//        
//
//
//      }
//
//      using (Transaction transaction= new Transaction(SessionScope.Current.Session, IsolationLevel.ReadCommitted)) {
//        Assert.AreEqual(TransactionState.Active, transaction.State);
//        transaction.Commit();
//        Assert.AreEqual(TransactionState.Committed, transaction.State);
//      }
//    }
//
//    [Ignore("Not implemented")]
//    [Test]
//    [ExpectedException(typeof(InvalidOperationException ))]
//    public void DoubleCommitTest()
//    {
//      using (Transaction transaction = new Transaction(SessionScope.Current.Session, IsolationLevel.ReadCommitted)){
//        Assert.AreEqual(TransactionState.Active, transaction.State);
//        transaction.Commit();
//        Assert.AreEqual(TransactionState.Committed, transaction.State);
//        transaction.Commit();
//      }
//    }
//
//    [Ignore("Not implemented")]
//    [Test]
//    public void CommitRollbackTest()
//    {
//      using (Transaction transaction = new Transaction(SessionScope.Current.Session, IsolationLevel.ReadCommitted)){
//        Assert.AreEqual(TransactionState.Active, transaction.State);
//        transaction.Commit();
//        Assert.AreEqual(TransactionState.Committed, transaction.State);
//        transaction.Rollback();
//        Assert.AreEqual(TransactionState.Committed, transaction.State);
//      }
//    }
//
//    [Test]
//    public void NestedTest()
//    {
//      using (Transaction transaction = new Transaction(SessionScope.Current.Session, IsolationLevel.ReadCommitted)) {
//        using (Transaction nestedTransaction = new Transaction(transaction)) {
//          nestedTransaction.Rollback();
//        }
//        Assert.AreEqual(TransactionState.Active, transaction.State & TransactionState.Active);
//      }
//    }
//
//    [Ignore("Not implemented")]
//    [Test]
//    [ExpectedException(typeof(InvalidOperationException))]
//    public void OuterCompleteTest()
//    {
//      using (Transaction transaction = Session.Current.ActivateTransaction()){
//        transaction.Commit();
//        using (Transaction nestedTransaction = new Transaction(SessionScope.Current.Session)){
//          nestedTransaction.Rollback();
//        }
//      }
//    }
//  }
//}
