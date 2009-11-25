// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.24

using NUnit.Framework;
using Xtensive.Core.Aspects;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Aspects;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.AutoTransactionsTestModel;

namespace Xtensive.Storage.Tests.Storage.AutoTransactionsTestModel
{
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
    public void MainTest()
    {
      using (Session.Open(Domain)) {

        var testObject = new MySessionBound(1);
        testObject.CheckAutoTransactions();
      }
    }
  }

  #region MySessionBound class
  public class MySessionBound : SessionBound
  {
    #region Properties

    public bool PublicProperty
    {
      get { return IsTransactionOpen; }
    }

    protected bool ProtectedProperty
    {
      get { return IsTransactionOpen; }
    }

    internal bool InternalProperty
    {
      get { return IsTransactionOpen; }
    }

    protected internal bool ProtectedInternalProperty
    {
      get { return IsTransactionOpen; }
    }

    public static bool PublicStaticProperty
    {
      get { return IsTransactionOpen; }
    }

    private bool PrivateProperty
    {
      get { return IsTransactionOpen; }
    }

    [Transactional(false)]
    public bool PublicNotTransactionalProperty
    {
        get { return IsTransactionOpen; }
    }

    [Transactional]
    private bool PrivateTransactionalProperty
    {
        get { return IsTransactionOpen; }
    }

    [Infrastructure]
    public bool PublicInfrastructureProperty
    {
        get { return IsTransactionOpen; }
    }

    #endregion 

    #region Methods

    public bool PublicMethod()
    {
      return IsTransactionOpen;
    }

    protected bool ProtectedMethod()
    {
      return IsTransactionOpen;
    }

    internal bool InternalMethod()
    {
      return IsTransactionOpen;
    }

    protected internal bool ProtectedInternalMethod()
    {
      return IsTransactionOpen;
    }

    public static bool PublicStaticMethod()
    {
      return IsTransactionOpen;
    }

    private bool PrivateMethod()
    {
      return IsTransactionOpen;
    }

    [Transactional(false)]
    public bool PublicNotTransactionalMethod()
    {
      return IsTransactionOpen;
    }

    [Transactional(true)]
    private bool PrivateTransactionalMethod()
    {
      return IsTransactionOpen;
    }

    [Infrastructure]
    public bool PublicInfrastructureMethod()
    {
      return IsTransactionOpen;
    }

    #endregion 

    #region Constructors

    private readonly bool isCreatedTransactionally;

    public MySessionBound(int parameter)
    {
      isCreatedTransactionally = IsTransactionOpen;
    }

    internal MySessionBound()
    {
      isCreatedTransactionally = IsTransactionOpen;
    }

    private MySessionBound(string parameter)
    {
      isCreatedTransactionally = IsTransactionOpen;
    }

    [Transactional(false)]
    public MySessionBound(float parameter)
    {
      isCreatedTransactionally = IsTransactionOpen;
    }

    [Transactional]
    private MySessionBound(bool parameter)
    {
      isCreatedTransactionally = IsTransactionOpen;
    }

    [Infrastructure]
    public MySessionBound(char parameter)
    {
      isCreatedTransactionally = IsTransactionOpen;
    }

    #endregion

    [ActivateSession(false), Transactional(false)]
    public void CheckAutoTransactions()
    {
      // Check whether IsTransactionOpen property is OK

      Assert.IsFalse(IsTransactionOpen);
      using (Transaction.Open()) {
        Assert.IsTrue(IsTransactionOpen);
      }
      Assert.IsFalse(IsTransactionOpen);

      // Check properties

      Assert.IsTrue(PublicProperty);
      Assert.IsTrue(PrivateTransactionalProperty);

      Assert.IsFalse(ProtectedProperty);
      Assert.IsFalse(InternalProperty);
      Assert.IsFalse(ProtectedInternalProperty);
      Assert.IsFalse(PrivateProperty);
      Assert.IsFalse(PublicStaticProperty);
      Assert.IsFalse(PublicNotTransactionalProperty);
      Assert.IsFalse(PublicInfrastructureProperty);

      // Check methods

      Assert.IsTrue(PublicMethod());
      Assert.IsTrue(PrivateTransactionalMethod());

      Assert.IsFalse(ProtectedMethod());
      Assert.IsFalse(InternalMethod());
      Assert.IsFalse(ProtectedInternalMethod());
      Assert.IsFalse(PrivateMethod());
      Assert.IsFalse(PublicStaticMethod());
      Assert.IsFalse(PublicNotTransactionalMethod());
      Assert.IsFalse(PublicInfrastructureMethod());

      // Check constructors

      Assert.IsTrue(new MySessionBound(1).isCreatedTransactionally); // public constructor
      Assert.IsTrue(new MySessionBound(true).isCreatedTransactionally); // private transactional constructor
            
      Assert.IsFalse(new MySessionBound().isCreatedTransactionally); // internal constructor
      Assert.IsFalse(new MySessionBound("hello").isCreatedTransactionally); // private constructor
      Assert.IsFalse(new MySessionBound(1.5F).isCreatedTransactionally); // not transactional public constructor
      Assert.IsFalse(new MySessionBound('a').isCreatedTransactionally); // infrastructure constructor
    }

    private static bool IsTransactionOpen
    {
      get
      {
        var transaction = Session.Current.Transaction;
        return 
          transaction!=null && 
          transaction.State==TransactionState.Active;
      }
    }
  }
#endregion
}