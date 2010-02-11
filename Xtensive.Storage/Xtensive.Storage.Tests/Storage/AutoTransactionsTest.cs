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
        MySessionBound.TestSession = Session.Demand();
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
      testObject.CheckSessionActivation();
    }
  }    

  
  public class MySessionBound : SessionBound
  {

    [ActivateSession(false), Transactional(false)]
    public void CheckSessionActivation()
    {
      TestSession = Session;

      CheckState(TransactionState.None, SessionState.NotActive);
      using (Session.Activate()) {
        CheckState(TransactionState.None, SessionState.Active);
      }
      CheckState(TransactionState.None, SessionState.NotActive);

      CallAllMethods();
    }

    [ActivateSession(true), Transactional(false)]
    public void CheckAutoTransactions()
    {
      TestSession = Session;

      CheckState(TransactionState.None, SessionState.Active);
      using (Transaction.Open()) {
        CheckState(TransactionState.Open, SessionState.Active);
      }
      CheckState(TransactionState.None, SessionState.Active);

      CallAllMethods();
      CallConstructors();
    }

    [Infrastructure]
    public void CallConstructors()
    {
      new MySessionBound(); // Nothing must happen
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
      get { return CheckState(TransactionState.Open, SessionState.Active); }
      set { CheckState(TransactionState.Open, SessionState.Active); }
    }

    protected int ProtectedProperty
    {
      get { return CheckState(TransactionState.None, SessionState.NotActive); }
      set { CheckState(TransactionState.None, SessionState.NotActive); }
    }

    internal int InternalProperty
    {
      get { return CheckState(TransactionState.None, SessionState.NotActive); }
      set { CheckState(TransactionState.None, SessionState.NotActive); }
    }

    protected internal int ProtectedInternalProperty
    {
      get { return CheckState(TransactionState.None, SessionState.NotActive); }
      set { CheckState(TransactionState.None, SessionState.NotActive); }
    }

    private int PrivateProperty
    {
      get { return CheckState(TransactionState.None, SessionState.NotActive); }
      set { CheckState(TransactionState.None, SessionState.NotActive); }
    }

    [Transactional(false)]
    public int PublicNotTransactionalProperty
    {
      get { return CheckState(TransactionState.None, SessionState.Active); }
      set { CheckState(TransactionState.None, SessionState.Active); }
    }

    [Transactional]
    private int PrivateTransactionalProperty
    {
      get { return CheckState(TransactionState.Open, SessionState.Active); }
      set { CheckState(TransactionState.Open, SessionState.Active); }
    }

    [Infrastructure]
    public int PublicInfrastructureProperty
    {
      get { return CheckState(TransactionState.None, SessionState.NotActive); }
      set { CheckState(TransactionState.None, SessionState.NotActive); }
    }


    // Methods

    public void PublicMethod()
    {
      CheckState(TransactionState.Open, SessionState.Active);
    }

    protected void ProtectedMethod()
    {
      CheckState(TransactionState.None, SessionState.NotActive);
    }

    internal void InternalMethod()
    {
      CheckState(TransactionState.None, SessionState.NotActive);
    }

    protected internal void ProtectedInternalMethod()
    {
      CheckState(TransactionState.None, SessionState.NotActive);
    }

    private void PrivateMethod()
    {
      CheckState(TransactionState.None, SessionState.NotActive);
    }

    [Transactional(false)]
    public void PublicNotTransactionalMethod()
    {
      CheckState(TransactionState.None, SessionState.Active);
    }

    [Transactional(true)]
    private void PrivateTransactionalMethod()
    {
      CheckState(TransactionState.Open, SessionState.Active);
    }

    [Infrastructure]
    public void PublicInfrastructureMethod()
    {
      CheckState(TransactionState.None, SessionState.NotActive);
    }

    [ActivateSession(false)]
    public void PublicNotSessionMethod()
    {
      CheckState(TransactionState.Open, SessionState.NotActive);
    }

    [ActivateSession(false), Transactional]
    public void InternalTransactionalNotSessionMethod()
    {
      CheckState(TransactionState.Open, SessionState.NotActive);
    }


    // Constructors

    public MySessionBound()
    {
      CheckState(TransactionState.None, SessionState.NotActive);
    }

    #endregion

    #region Infrastructure

    internal static Session TestSession { get; set;}

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

    public static int CheckState(TransactionState transactionState, SessionState sessionState)
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