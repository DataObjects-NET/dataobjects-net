// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.24

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Aspects;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.AutoTransactionsTestModel;

namespace Xtensive.Storage.Tests.Storage.AutoTransactionsTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  
  public class AutoTransactionsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MyEntity).Assembly, typeof (MyEntity).Namespace);
      return config;
    }

    [Test]
    public void TransactionsTest()
    {
      using (Session.Open(Domain)) {

        var testObject = new MySessionBound();
        testObject.CheckAutoTransactions();
      }
    }

    [Test]
    public void SessionsTest()
    {
      var session = Session.Open(Domain, false);

      MySessionBound testObject;

      using (session.Activate()) {
        testObject = new MySessionBound();
      }

      testObject.CheckSessionActivating();
    }
  }    

  
  public class MySessionBound : SessionBound
  {

    [ActivateSession(false), Transactional(false)]
    public void CheckSessionActivating()
    {
      TestSession = this.Session;

      Assert(TransactionState.None, SessionState.NotActive);
      using (Session.Activate()) {
        Assert(TransactionState.None, SessionState.Active);
      }
      Assert(TransactionState.None, SessionState.NotActive);

      // Check properties

      CallAllMethods();
    }

    [ActivateSession(true), Transactional(false)]
    public void CheckAutoTransactions()
    {
      // Check whether IsTransactionOpen property is OK

      TestSession = this.Session;

      Assert(TransactionState.None, SessionState.Active);
      using (Transaction.Open()) {
        Assert(TransactionState.Open, SessionState.Active);
      }
      Assert(TransactionState.None, SessionState.Active);

      CallAllMethods();
      CallConstructors();
    }

    [Infrastructure]
    public void CallConstructors()
    {
      new MySessionBound(1); // public constructor
      new MySessionBound(true); // private transactional constructor
            
      new MySessionBound(DateTime.Now); // internal constructor
      new MySessionBound("hello"); // private constructor
      new MySessionBound(1.5F); // not transactional public constructor
      new MySessionBound('a'); // infrastructure constructor
    }

    [Infrastructure]
    public void CallAllMethods()
    {
      PublicProperty = PublicProperty;
      PrivateTransactionalProperty = PrivateTransactionalProperty;

      ProtectedProperty = ProtectedProperty;
      InternalProperty = InternalProperty;
      ProtectedInternalProperty = ProtectedInternalProperty;
      PrivateProperty = PrivateProperty;
      PublicNotTransactionalProperty = PublicNotTransactionalProperty;
      PublicInfrastructureProperty = PublicInfrastructureProperty;

      // Check methods

      PublicMethod();
      PrivateTransactionalMethod();

      ProtectedMethod();
      InternalMethod();
      ProtectedInternalMethod();
      PrivateMethod();
      PublicNotTransactionalMethod();
      PublicInfrastructureMethod();
      PublicNotSessionMethod();
      InternalTransactionalNotSessionMethod();
    }


    #region Test Members

    public int PublicProperty
    {
      get { return Assert(TransactionState.Open, SessionState.Active); }
      set { Assert(TransactionState.Open, SessionState.Active); }
    }

    protected int ProtectedProperty
    {
      get { return Assert(TransactionState.None, SessionState.NotActive); }
      set { Assert(TransactionState.None, SessionState.NotActive); }
    }

    internal int InternalProperty
    {
      get { return Assert(TransactionState.None, SessionState.NotActive); }
      set { Assert(TransactionState.None, SessionState.NotActive); }
    }

    protected internal int ProtectedInternalProperty
    {
      get { return Assert(TransactionState.None, SessionState.NotActive); }
      set { Assert(TransactionState.None, SessionState.NotActive); }
    }

    private int PrivateProperty
    {
      get { return Assert(TransactionState.None, SessionState.NotActive); }
      set { Assert(TransactionState.None, SessionState.NotActive); }
    }

    [Transactional(false)]
    public int PublicNotTransactionalProperty
    {
      get { return Assert(TransactionState.None, SessionState.Active); }
      set { Assert(TransactionState.None, SessionState.Active); }
    }

    [Transactional]
    private int PrivateTransactionalProperty
    {
      get { return Assert(TransactionState.Open, SessionState.Active); }
      set { Assert(TransactionState.Open, SessionState.Active); }
    }

    [Infrastructure]
    public int PublicInfrastructureProperty
    {
      get { return Assert(TransactionState.None, SessionState.NotActive); }
      set { Assert(TransactionState.None, SessionState.NotActive); }
    }


    // Methods

    public void PublicMethod()
    {
      Assert(TransactionState.Open, SessionState.Active);
    }

    protected void ProtectedMethod()
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    internal void InternalMethod()
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    protected internal void ProtectedInternalMethod()
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    private void PrivateMethod()
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    [Transactional(false)]
    public void PublicNotTransactionalMethod()
    {
      Assert(TransactionState.None, SessionState.Active);
    }

    [Transactional(true)]
    private void PrivateTransactionalMethod()
    {
      Assert(TransactionState.Open, SessionState.Active);
    }

    [Infrastructure]
    public void PublicInfrastructureMethod()
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    [ActivateSession(false)]
    public void PublicNotSessionMethod()
    {
      Assert(TransactionState.Open, SessionState.NotActive);
    }

    [ActivateSession(false), Transactional]
    public void InternalTransactionalNotSessionMethod()
    {
      Assert(TransactionState.Open, SessionState.NotActive);
    }


    // Constructors

    public MySessionBound(int parameter)
    {
      Assert(TransactionState.Open, SessionState.NotActive);
    }

    internal MySessionBound(DateTime parameter)
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    private MySessionBound(string parameter)
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    [Transactional(false)]
    public MySessionBound(float parameter)
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    [Transactional]
    private MySessionBound(bool parameter)
    {
      Assert(TransactionState.Open, SessionState.NotActive);
    }

    [Infrastructure]
    public MySessionBound(char parameter)
    {
      Assert(TransactionState.None, SessionState.NotActive);
    }

    public MySessionBound()
    {
    }

    #endregion


    #region Infrastructure

    private static Session TestSession { get; set;}

    public enum TransactionState
    {
      None,
      Open,
      Nested
    }

    public enum SessionState
    {
      Active,
      NotActive
    }

    public static bool CheckSession { get; set; }

    public static int Assert(TransactionState transactionState, SessionState sessionState)
    {
      if (CheckSession)
        NUnit.Framework.Assert.IsTrue(TestSession.IsActive);

      bool isTransactionOpen = TestSession.Transaction!=null &&
        TestSession.Transaction.State==Integrity.Transactions.TransactionState.Active;

      NUnit.Framework.Assert.AreEqual(isTransactionOpen, transactionState==TransactionState.Open);

      return 0;
    }
    #endregion
  }
  
}