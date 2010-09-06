// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.24

using System;
using NUnit.Framework;
using Xtensive.Core.Aspects;
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
      testObject.CheckSessionActivation();
    }

    [Test]
    public void NonTransactionalSessionBoundTest()
    {
      using (Session.Open(Domain))
      {
        var sb = new NotTransactionalSessionBound();
        sb.Method1();
      }
    }
  }

  [TransactionalType(TransactionalBehavior.Suppress, ActivateSession = true, AttributeReplace = true)]
  public class NotTransactionalSessionBound : SessionBound
  {
//    [Transactional(TransactionalBehavior.Suppress)]
    public void Method1()
    {
      Console.Out.WriteLine("Blah...");
      Assert.IsNull(Session.Transaction);
    }

    public string Name { get; private set; }
  }

  [TransactionalType(TransactionalBehavior.Open, AttributeReplace = true)]
  public class MySessionBound : SessionBound
  {

    [Transactional(Mode = TransactionalBehavior.Suppress, ActivateSession = false)]
    public void CheckSessionActivation()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
      using (Session.Activate()) {
        CheckState(this, TransactionState.None, SessionState.Active);
      }
      CheckState(this, TransactionState.None, SessionState.NotActive);

      CallAllMethods();
    }

    [NonTransactional]
    [SuppressActivation(typeof(Session))]
    public void CheckAutoTransactions()
    {
      CheckState(this, TransactionState.None, SessionState.Active);
      using (Transaction.Open()) {
        CheckState(this, TransactionState.Open, SessionState.Active);
      }
      CheckState(this, TransactionState.None, SessionState.Active);

      CallAllMethods();

      PrivateStaticMethod();
      PublicStaticMethod();
      PrivateTransactionalStaticMethod();
      PublicNonTransactionalStaticMethod();

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
      get { return CheckState(this, TransactionState.Open, SessionState.Active); }
      set { CheckState(this, TransactionState.Open, SessionState.Active); }
    }

    protected int ProtectedProperty
    {
      get { return CheckState(this, TransactionState.None, SessionState.NotActive); }
      set { CheckState(this, TransactionState.None, SessionState.NotActive); }
    }

    internal int InternalProperty
    {
      get { return CheckState(this, TransactionState.None, SessionState.NotActive); }
      set { CheckState(this, TransactionState.None, SessionState.NotActive); }
    }

    protected internal int ProtectedInternalProperty
    {
      get { return CheckState(this, TransactionState.None, SessionState.NotActive); }
      set { CheckState(this, TransactionState.None, SessionState.NotActive); }
    }

    private int PrivateProperty
    {
      get { return CheckState(this, TransactionState.None, SessionState.NotActive); }
      set { CheckState(this, TransactionState.None, SessionState.NotActive); }
    }

    [NonTransactional]
    public int PublicNotTransactionalProperty
    {
      get { return CheckState(this, TransactionState.None, SessionState.Active); }
      set { CheckState(this,TransactionState.None, SessionState.Active); }
    }

    [Transactional]
    private int PrivateTransactionalProperty
    {
      get { return CheckState(this ,TransactionState.Open, SessionState.Active); }
      set { CheckState(this, TransactionState.Open, SessionState.Active); }
    }

    [Infrastructure]
    public int PublicInfrastructureProperty
    {
      get { return CheckState(this, TransactionState.None, SessionState.NotActive); }
      set { CheckState(this, TransactionState.None, SessionState.NotActive); }
    }


    // Methods

    public void PublicMethod()
    {
      CheckState(this, TransactionState.Open, SessionState.Active);
    }

    protected void ProtectedMethod()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
    }

    internal void InternalMethod()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
    }

    protected internal void ProtectedInternalMethod()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
    }

    private void PrivateMethod()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
    }

    [NonTransactional]
    public void PublicNotTransactionalMethod()
    {
      CheckState(this, TransactionState.None, SessionState.Active);
    }

    [Transactional]
    private void PrivateTransactionalMethod()
    {
      CheckState(this, TransactionState.Open, SessionState.Active);
    }

    [Infrastructure]
    public void PublicInfrastructureMethod()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
    }

    [Transactional(ActivateSession = false)]
    public void PublicNotSessionMethod()
    {
      CheckState(this, TransactionState.Open, SessionState.NotActive);
    }

    [Transactional(ActivateSession = false)]
    internal void InternalTransactionalNotSessionMethod()
    {
      CheckState(this, TransactionState.Open, SessionState.NotActive);
    }

    public static void PublicStaticMethod()
    {
      CheckState(null, TransactionState.None, SessionState.Active);
    }

    [NonTransactional]
    public static void PublicNonTransactionalStaticMethod()
    {
      CheckState(null, TransactionState.None, SessionState.Active);
    }

    private static void PrivateStaticMethod()
    {
      CheckState(null, TransactionState.None, SessionState.Active);
    }

    [Transactional]
    private static void PrivateTransactionalStaticMethod()
    {
      CheckState(null, TransactionState.Open, SessionState.Active);
    }


    // Constructors

    public MySessionBound()
    {
      CheckState(this, TransactionState.None, SessionState.NotActive);
    }

    #endregion

    #region Infrastructure

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

    [Infrastructure]
    public static int CheckState(ISessionBound sessionBound, TransactionState transactionState, SessionState sessionState)
    {
      var session = sessionBound == null
        ? Session.Current
        : sessionBound.Session;
      if (CheckSession) {
        Assert.IsNotNull(session);
        Assert.IsTrue(session.IsActive);
      }

      bool isTransactionOpen = session.Transaction!=null 
        && session.Transaction.State==Integrity.Transactions.TransactionState.Active;

      Assert.AreEqual(transactionState==TransactionState.Open, isTransactionOpen);

      return 0;
    }
    #endregion
  }
}