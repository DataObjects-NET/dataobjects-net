// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08

using System;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.SessionEventsTestModel;

namespace Xtensive.Storage.Tests.Storage.SessionEventsTestModel
{
  [Serializable]
  [HierarchyRoot]
  public class MegaEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int Value { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  public class SessionEventsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (MegaEntity).Assembly, typeof (MegaEntity).Namespace);
      return config;
    }

    private TransactionEventArgs transactionOpenArgs;
    private TransactionEventArgs transactionCommitingArgs;
    private TransactionEventArgs transactionCommitedArgs;

    private TransactionEventArgs transactionRollbackingArgs;
    private TransactionEventArgs transactionRollbackedArgs;

    private EventArgs persistingArgs;
    private EventArgs persistedArgs;

    private EntityEventArgs entityCreatedArgs;
    private EntityEventArgs entityRemoving;
    private EntityEventArgs entityRemoved;

    private EntityFieldEventArgs entityEntityFieldGettingArgs;
    private EntityFieldValueEventArgs entityEntityFieldArgs;
    private EntityFieldValueEventArgs entityEntityFieldSettingArgs;
    private EntityFieldValueSetEventArgs entityEntityFieldSetArgs;

    private class TestException : Exception { } 
    
    private bool throwExceptionOnCommit;

    private void ClearEvents()
    {
      transactionOpenArgs = null;
      transactionCommitingArgs  = null;
      transactionCommitedArgs  = null;

      transactionRollbackingArgs  = null;
      transactionRollbackedArgs  = null;

      persistingArgs = null;
      persistedArgs = null;

      entityCreatedArgs = null;
      entityRemoving = null;
      entityRemoved = null;

      entityEntityFieldGettingArgs = null;
      entityEntityFieldArgs = null;
      entityEntityFieldSettingArgs = null;
      entityEntityFieldSetArgs = null;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {

        session.TransactionOpening += (sender, e) => transactionOpenArgs = e;

        session.TransactionCommitting += (sender, e) => {
          transactionCommitingArgs = e;
          if (throwExceptionOnCommit)
            throw new TestException();
        };
        session.TransactionCommitted += (sender, e) => transactionCommitedArgs = e;

        session.TransactionRollbacking += (sender, e) => transactionRollbackingArgs = e;
        session.TransactionRollbacked += (sender, e) => transactionRollbackedArgs = e;

        session.Persisting += (sender, e) => persistingArgs = e;
        session.Persisted += (sender, e) => persistedArgs = e;

        session.EntityCreated += (sender, e) => entityCreatedArgs = e;
        session.EntityRemoving += (sender, e) => entityRemoving = e;
        session.EntityRemove += (sender, e) => entityRemoved = e;

        session.EntityFieldValueGetting += (sender, e) => entityEntityFieldGettingArgs = e;
        session.EntityFieldValueGet += (sender, e) => entityEntityFieldArgs = e;
        session.EntityFieldValueSetting += (sender, e) => entityEntityFieldSettingArgs = e;
        session.EntityFieldValueSet += (sender, e) => entityEntityFieldSetArgs = e;

        CommitTransaction();
        RollbackTransaction();
        ErrorOnCommit();
        EditEntity();
      }
    }

    private void ErrorOnCommit()
    {
      ClearEvents();

      throwExceptionOnCommit = true;
      
      var transactionScope = Transaction.Open();
      Assert.IsNotNull(transactionOpenArgs);
      Assert.AreSame(Transaction.Current, transactionOpenArgs.Transaction);

      var megaEntity = new MegaEntity {Value = 1};

      transactionScope.Complete();
      AssertEx.Throws<TestException>(transactionScope.Dispose);

      Assert.IsNotNull(transactionRollbackingArgs);
      Assert.IsNotNull(transactionRollbackedArgs);
      Assert.IsNotNull(transactionCommitingArgs);
      Assert.IsNotNull(persistingArgs);
      Assert.IsNotNull(persistedArgs);
      Assert.IsNull(transactionCommitedArgs);
    }

    private void CommitTransaction()
    {
      ClearEvents();

      using (var transactionScope = Transaction.Open()) {
        Assert.IsNotNull(transactionOpenArgs);
        Assert.AreSame(Transaction.Current, transactionOpenArgs.Transaction);

        var megaEntity = new MegaEntity {Value = 1};
        transactionScope.Complete();
      }
      Assert.IsNull(transactionRollbackingArgs);
      Assert.IsNull(transactionRollbackedArgs);
      Assert.IsNotNull(persistingArgs);
      Assert.IsNotNull(persistedArgs);
      Assert.IsNotNull(transactionCommitingArgs);
      Assert.IsNotNull(transactionCommitedArgs);
    }

    private void RollbackTransaction()
    {
      ClearEvents();

      using (var transactionScope = Transaction.Open()) {
        Assert.IsNotNull(transactionOpenArgs);
        Assert.AreSame(Transaction.Current, transactionOpenArgs.Transaction);

        var megaEntity = new MegaEntity {Value = 1};
      }
      Assert.IsNotNull(transactionRollbackingArgs);
      Assert.IsNotNull(transactionRollbackedArgs);
      Assert.IsNull(persistingArgs);
      Assert.IsNull(persistedArgs);
      Assert.IsNull(transactionCommitingArgs);
      Assert.IsNull(transactionCommitedArgs);
    }

    private void EditEntity()
    {
      using (var transactionScope = Transaction.Open()) {

        ClearEvents();

        var entity = new MegaEntity();
        Assert.IsNotNull(entityCreatedArgs);
        Assert.AreEqual(entity, entityCreatedArgs.Entity);

        ClearEvents();

        entity.Value = 2;

        Assert.IsNotNull(entityEntityFieldSettingArgs);
        Assert.AreEqual(entity, entityEntityFieldSettingArgs.Entity);
        Assert.AreEqual(2, entityEntityFieldSettingArgs.Value);

        Assert.IsNotNull(entityEntityFieldSetArgs);
        Assert.AreEqual(entity, entityEntityFieldSetArgs.Entity);
        Assert.AreEqual(0, entityEntityFieldSetArgs.OldValue);
        Assert.AreEqual(2, entityEntityFieldSetArgs.NewValue);

        ClearEvents();

        int value = entity.Value;

        Assert.IsNull(entityEntityFieldSettingArgs);
        Assert.IsNull(entityEntityFieldSetArgs);

        Assert.IsNotNull(entityEntityFieldGettingArgs);
        Assert.AreEqual(entity, entityEntityFieldGettingArgs.Entity);
        Assert.IsNotNull(entityEntityFieldArgs);
        Assert.AreEqual(entity, entityEntityFieldArgs.Entity);
        Assert.AreEqual(2, entityEntityFieldArgs.Value);

        ClearEvents();

        entity.Remove();
        Assert.IsNotNull(entityRemoving);
        Assert.AreEqual(entity, entityRemoving.Entity);
        Assert.IsNotNull(entityRemoved);
        Assert.AreEqual(entity, entityRemoved.Entity);
      }
    }
  }
}