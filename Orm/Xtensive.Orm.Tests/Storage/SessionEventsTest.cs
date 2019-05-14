// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.SessionEventsTestModel;

namespace Xtensive.Orm.Tests.Storage.SessionEventsTestModel
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

namespace Xtensive.Orm.Tests.Storage
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

    private EventArgs changesCancelingArgs;
    private EventArgs changesCanceledArgs;

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
      using (var session = Domain.OpenSession()) {

        session.Events.TransactionOpening += (sender, e) => transactionOpenArgs = e;

        session.Events.TransactionCommitting += (sender, e) => {
          transactionCommitingArgs = e;
          if (throwExceptionOnCommit)
            throw new TestException();
        };
        session.Events.TransactionCommitted += (sender, e) => transactionCommitedArgs = e;

        session.Events.TransactionRollbacking += (sender, e) => transactionRollbackingArgs = e;
        session.Events.TransactionRollbacked += (sender, e) => transactionRollbackedArgs = e;

        session.Events.Persisting += (sender, e) => persistingArgs = e;
        session.Events.Persisted += (sender, e) => persistedArgs = e;

        session.Events.ChangesCanceling += (sender, e) => changesCancelingArgs = e;
        session.Events.ChangesCanceled += (sender, e) => changesCanceledArgs = e;

        session.Events.EntityCreated += (sender, e) => entityCreatedArgs = e;
        session.Events.EntityRemoving += (sender, e) => entityRemoving = e;
        session.Events.EntityRemove += (sender, e) => entityRemoved = e;

        session.Events.EntityFieldValueGetting += (sender, e) => entityEntityFieldGettingArgs = e;
        session.Events.EntityFieldValueGet += (sender, e) => entityEntityFieldArgs = e;
        session.Events.EntityFieldValueSetting += (sender, e) => entityEntityFieldSettingArgs = e;
        session.Events.EntityFieldValueSet += (sender, e) => entityEntityFieldSetArgs = e;

        CommitTransaction();
        RollbackTransaction();
        ErrorOnCommit();
        EditEntity();
        CancelChanges();
      }
    }

    private void ErrorOnCommit()
    {
      ClearEvents();

      throwExceptionOnCommit = true;
      
      var transactionScope = Session.Demand().OpenTransaction();
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
      Assert.IsNull(changesCancelingArgs);
      Assert.IsNull(changesCanceledArgs);
      Assert.IsNull(transactionCommitedArgs);
    }

    private void CommitTransaction()
    {
      ClearEvents();

      using (var transactionScope = Session.Demand().OpenTransaction()) {
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
      Assert.IsNull(changesCancelingArgs);
      Assert.IsNull(changesCanceledArgs);
    }

    private void RollbackTransaction()
    {
      ClearEvents();

      using (var transactionScope = Session.Demand().OpenTransaction()) {
        Assert.IsNotNull(transactionOpenArgs);
        Assert.AreSame(Transaction.Current, transactionOpenArgs.Transaction);

        var megaEntity = new MegaEntity {Value = 1};
      }
      Assert.IsNotNull(transactionRollbackingArgs);
      Assert.IsNotNull(transactionRollbackedArgs);
      Assert.IsNull(persistingArgs);
      Assert.IsNull(persistedArgs);
      Assert.IsNull(changesCancelingArgs);
      Assert.IsNull(changesCanceledArgs);
      Assert.IsNull(transactionCommitingArgs);
      Assert.IsNull(transactionCommitedArgs);
    }

    private void EditEntity()
    {
      using (var transactionScope = Session.Demand().OpenTransaction()) {

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

    private void CancelChanges()
    {
      var session = Session.Demand();
      using (var transactionScope = session.OpenTransaction()) {
        ClearEvents();

        var megaEntity = new MegaEntity {Value = 1};

        session.CancelChanges();
      }

      Assert.IsNotNull(transactionRollbackingArgs);
      Assert.IsNotNull(transactionRollbackedArgs);
      Assert.IsNotNull(changesCancelingArgs);
      Assert.IsNotNull(changesCanceledArgs);
      Assert.IsNull(persistingArgs);
      Assert.IsNull(persistedArgs);
      Assert.IsNull(transactionCommitingArgs);
      Assert.IsNull(transactionCommitedArgs);
    }
  }
}