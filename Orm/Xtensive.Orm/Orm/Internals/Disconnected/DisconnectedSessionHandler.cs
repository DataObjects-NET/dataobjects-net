// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.01

using System;
using System.Collections.Generic;
using System.Transactions;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using System.Linq;

namespace Xtensive.Orm.Disconnected
{
  /// <summary>
  /// Disconnected session handler.
  /// </summary>
  public sealed class DisconnectedSessionHandler : ChainingSessionHandler
  {
    private readonly DisconnectedState disconnectedState;
    private readonly Stack<Transaction> virtualTransactions = new Stack<Transaction>();
    
    #region Transactions

    /// <inheritdoc/>
    public override bool TransactionIsStarted {
      get { return disconnectedState.IsLocalTransactionOpen; }
    }

    internal void Connect()
    {
      if (virtualTransactions.Count > 0) {
        var transaction = virtualTransactions.Peek();
        base.BeginTransaction(transaction);
      }
    }

    internal void Disconnect()
    {
      if (virtualTransactions.Count > 0) {
        var transaction = virtualTransactions.Peek();
        base.CommitTransaction(transaction);
      }
    }

    /// <inheritdoc/>
    public override void BeginTransaction(Transaction transaction)
    {
      disconnectedState.OnTransactionOpened();
      if (transaction.IsAutomatic) 
        return;

      if (disconnectedState.IsConnected)
        base.BeginTransaction(transaction);
      else
        virtualTransactions.Push(transaction);
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Transaction transaction)
    {
      disconnectedState.OnTransactionOpened();
      if (transaction.IsAutomatic)
        return;

      if (disconnectedState.IsConnected)
        base.CreateSavepoint(transaction);
      else
        virtualTransactions.Push(transaction);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      disconnectedState.OnTransactionRollbacked();
      if (transaction.IsAutomatic)
        return;

      if (disconnectedState.IsConnected)
        base.RollbackToSavepoint(transaction);
      else
        virtualTransactions.Pop();
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      disconnectedState.OnTransactionCommited();
      if (transaction.IsAutomatic)
        return;

      if (disconnectedState.IsConnected)
        base.ReleaseSavepoint(transaction);
      else
        virtualTransactions.Pop();
    }

    /// <inheritdoc/>
    public override void CommitTransaction(Transaction transaction)
    {
//      if (ChainedHandler.TransactionIsStarted && !disconnectedState.IsAttachedWhenTransactionWasOpen)
//        ChainedHandler.CommitTransaction(transaction);
//      disconnectedState.OnTransactionCommited();
      if (!transaction.IsAutomatic) {
        if (disconnectedState.IsConnected)
          base.CommitTransaction(transaction);
        else
          virtualTransactions.Pop();
      }
      disconnectedState.OnTransactionCommited();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
//      if (ChainedHandler.TransactionIsStarted && !disconnectedState.IsAttachedWhenTransactionWasOpen)
//        ChainedHandler.RollbackTransaction(transaction);
//      disconnectedState.OnTransactionRollbacked();
      if (!transaction.IsAutomatic) {
        if (disconnectedState.IsConnected)
          base.RollbackTransaction(transaction);
        else
          virtualTransactions.Pop();
      }
      disconnectedState.OnTransactionRollbacked();
    }

   /* public void BeginChainedTransaction()
    {
      if (ChainedHandler.TransactionIsStarted)
        return;
      if (!disconnectedState.IsConnected)
        throw new ConnectionRequiredException();
      ChainedHandler.BeginTransaction(Session.Configuration.DefaultIsolationLevel);
    }

    public void CommitChainedTransaction()
    {
      // We assume that chained transactions are always readonly, so there is no rollback.
      if (ChainedHandler.TransactionIsStarted && !disconnectedState.IsAttachedWhenTransactionWasOpen)
        ChainedHandler.CommitTransaction();
    }*/


    #endregion

    internal override bool LookupState(Key key, out EntityState entityState)
    {
      if (Session.LookupStateInCache(key, out entityState))
        return true;
      
      var cachedEntityState = disconnectedState.GetEntityState(key);
      if (cachedEntityState!=null && cachedEntityState.IsLoadedOrRemoved) {
        var tuple = cachedEntityState.Tuple!=null ? cachedEntityState.Tuple.Clone() : null;
        entityState = Session.UpdateStateInCache(key, tuple);
        return true;
      }
      entityState = null;
      return false;
    }

    internal override bool LookupState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      if (Session.LookupStateInCache(key, fieldInfo, out entitySetState))
        return true;
      
      var cachedState = disconnectedState.GetEntityState(key);
      if (cachedState!=null) {
        var setState = cachedState.GetEntitySetState(fieldInfo);
        entitySetState = Session.UpdateStateInCache(key, fieldInfo, setState.Items.Keys, setState.IsFullyLoaded);
        return true;
      }
      entitySetState = null;
      return false;
    }

    internal override EntityState UpdateState(Key key, Tuple tuple)
    {
      var cachedEntityState = tuple==null 
        ? disconnectedState.GetEntityState(key) 
        : disconnectedState.RegisterEntityState(key, tuple);
     
      if (cachedEntityState!=null) {
        if (!cachedEntityState.Key.HasExactType)
          cachedEntityState.Key.TypeReference = key.TypeReference;
      }

      if (cachedEntityState==null || cachedEntityState.IsRemoved || cachedEntityState.Tuple == null)
        return Session.UpdateStateInCache(key, null);

      var entityState = Session.UpdateStateInCache(cachedEntityState.Key, cachedEntityState.Tuple.Clone());
      
      // Fetch version roots
      if (entityState.Type.HasVersionRoots) {
//        BeginChainedTransaction();
        var entity = entityState.Entity as IHasVersionRoots;
        if (entity != null) {
          if (!disconnectedState.IsConnected)
            throw new ConnectionRequiredException();
          entity.GetVersionRoots().ToList();
        }
      }
      return entityState;
    }

    internal override EntitySetState UpdateState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      var cachedOwner = disconnectedState.GetEntityState(key);
      if (cachedOwner==null || cachedOwner.IsRemoved)
        return null;

      // Merge with disconnected state cache
      var cachedState = disconnectedState.RegisterEntitySetState(key, fieldInfo, isFullyLoaded, entityKeys, auxEntities);
      
      // Update session cache
      return Session.UpdateStateInCache(key, fieldInfo, cachedState.Items.Keys, cachedState.IsFullyLoaded);
    }

    /// <inheritdoc/>
    public override EntityState FetchEntityState(Key key)
    {
      var cachedState = disconnectedState.GetEntityState(key);
      
      // If state is cached, let's return it
      if (cachedState!=null && cachedState.IsLoadedOrRemoved) {
        Tuple tuple = null;
        if (!cachedState.IsRemoved && cachedState.Tuple!=null) 
          tuple = cachedState.Tuple.Clone();
        var entityState = Session.UpdateStateInCache(cachedState.Key, tuple, true);
        return cachedState.IsRemoved ? null : entityState;
      }

      // If state isn't cached, let's try to to get it from storage
      if ((cachedState!=null && !cachedState.IsLoadedOrRemoved) || disconnectedState.IsConnected) {
        if (!disconnectedState.IsConnected)
          throw new ConnectionRequiredException();
//        BeginChainedTransaction();
        var type = key.TypeReference.Type;
        Prefetch(key, type, PrefetchHelper.CreateDescriptorsForFieldsLoadedByDefault(type));
        ExecutePrefetchTasks(true);
        EntityState result;
        return LookupState(key, out result) ? result : null;
      }

      // If state unknown return null
      return null;
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      registry.GetItems(PersistenceState.New)
        .ForEach(item => disconnectedState.Persist(item, PersistActionKind.Insert));
      registry.GetItems(PersistenceState.Modified)
        .ForEach(item => disconnectedState.Persist(item, PersistActionKind.Update));
      registry.GetItems(PersistenceState.Removed)
        .ForEach(item => disconnectedState.Persist(item, PersistActionKind.Remove));
    }

    /// <inheritdoc/>
    public override StrongReferenceContainer ExecutePrefetchTasks(bool skipPersist)
    {
      Session.ExecuteDelayedQueries(skipPersist); // Important!
      return base.ExecutePrefetchTasks(skipPersist);
    }


    /// <inheritdoc/>
    public override IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ManyToOne:
        case Multiplicity.ZeroToOne:
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          Session.Persist(PersistReason.DisconnectedStateReferenceCacheLookup);
          var list = new List<ReferenceInfo>();
          var state = disconnectedState.GetEntityState(target.Key);
          if (state!=null) {
            foreach (var reference in state.GetReferences(association.OwnerField)) {
              var item = FetchEntityState(reference.Key);
              if (item!=null) // Can be already removed (TODO: check this)
                list.Add(new ReferenceInfo(item.Entity, target, association));
            }
          }
          return list;
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany:
          var key = target.GetReferenceKey(association.Reversed.OwnerField);
          if (key!=null)
            return EnumerableUtils.One(new ReferenceInfo(FetchEntityState(key).Entity, target, association));
          return EnumerableUtils<ReferenceInfo>.Empty;
        default:
          throw new ArgumentOutOfRangeException("association.Multiplicity");
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public DisconnectedSessionHandler(SessionHandler chainedHandler, DisconnectedState disconnectedState)
      : base(chainedHandler)
    {
      this.disconnectedState = disconnectedState;
    }
  }
}