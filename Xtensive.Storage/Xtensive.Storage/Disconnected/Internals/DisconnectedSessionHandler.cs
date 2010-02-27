// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.01

using System;
using System.Collections.Generic;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Internals.Prefetch;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using System.Linq;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected session handler.
  /// </summary>
  public sealed class DisconnectedSessionHandler : ChainingSessionHandler
  {
    private readonly DisconnectedState disconnectedState;
    
    #region Transactions

    /// <inheritdoc/>
    public override bool TransactionIsStarted { get { return disconnectedState.IsLocalTransactionOpen; } }

    /// <inheritdoc/>
    public override void BeginTransaction(IsolationLevel isolationLevel)
    {
      disconnectedState.OnTransactionOpened();
    }

    /// <inheritdoc/>
    public override void MakeSavepoint(string name)
    {
      disconnectedState.OnTransactionOpened();
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(string name)
    {
      disconnectedState.OnTransactionRollbacked();
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(string name)
    {
      disconnectedState.OnTransactionCommited();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      if (chainedHandler.TransactionIsStarted)
        chainedHandler.CommitTransaction();
      disconnectedState.OnTransactionCommited();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      if (chainedHandler.TransactionIsStarted)
        chainedHandler.RollbackTransaction();
      disconnectedState.OnTransactionRollbacked();
    }

    public void BeginChainedTransaction()
    {
      if (!chainedHandler.TransactionIsStarted) {
        if (!disconnectedState.IsConnected)
          throw new ConnectionRequiredException();
        chainedHandler.BeginTransaction(Session.Configuration.DefaultIsolationLevel);
      }
    }

    // We assume that chained transactions are always readonly, so there is no rollback.

    public void EndChainedTransaction()
    {
      if (chainedHandler.TransactionIsStarted)
        chainedHandler.CommitTransaction();
    }


    #endregion

    internal override bool TryGetEntityState(Key key, out EntityState entityState)
    {
      if (Session.EntityStateCache.TryGetItem(key, true, out entityState))
        return true;
      
      var cachedEntityState = disconnectedState.GetState(key);
      if (cachedEntityState!=null && cachedEntityState.IsLoaded) {
        var tuple = cachedEntityState.Tuple!=null ? cachedEntityState.Tuple.Clone() : null;
        entityState = Session.UpdateEntityState(key, tuple, true);
        return true;
      }
      entityState = null;
      return false;
    }

    internal override EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      var cachedEntityState = tuple==null 
        ? disconnectedState.GetState(key) 
        : disconnectedState.RegisterState(key, tuple);
     
      if (cachedEntityState!=null) {
        if (!cachedEntityState.Key.HasExactType)
          cachedEntityState.Key.TypeRef = key.TypeRef;
      }

      // tuple==null && key is not cached || cached entity state is removed
      if (cachedEntityState==null || cachedEntityState.IsRemoved || cachedEntityState.Tuple == null)
        return Session.UpdateEntityState(key, null, true);
      
      var entityState = Session.UpdateEntityState(cachedEntityState.Key, cachedEntityState.Tuple.Clone(), true);
      
      // Fetch version roots
      if (entityState.Type.HasVersionRoots) {
        BeginChainedTransaction();
        var entity = entityState.Entity as IHasVersionRoots;
        if (entity!=null)
          entity.GetVersionRoots().ToList();
      }
      return entityState;
    }

    internal override bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      if (chainedHandler.TransactionIsStarted)
        return base.TryGetEntitySetState(key, fieldInfo, out entitySetState);

      if (base.TryGetEntitySetState(key, fieldInfo, out entitySetState))
        return true;
      var cachedState = disconnectedState.GetState(key);
      if (cachedState!=null) {
        var setState = cachedState.GetEntitySetState(fieldInfo);
        entitySetState = UpdateEntitySetState(key, fieldInfo, setState.Items.Keys.ToList(), setState.IsFullyLoaded);
        return true;
      }
      entitySetState = null;
      return !disconnectedState.IsConnected;
    }

    internal override EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      var cachedOwner = disconnectedState.GetState(key);
      if (cachedOwner==null || cachedOwner.IsRemoved)
        return null;

      // Merge with disconnected state cache
      var cachedState = disconnectedState.RegisterSetState(key, fieldInfo, isFullyLoaded, entityKeys, auxEntities);
      
      // Update session cache
      return UpdateEntitySetState(key, fieldInfo, cachedState.Items.Keys, cachedState.IsFullyLoaded);
    }

    /// <inheritdoc/>
    public override EntityState FetchInstance(Key key)
    {
      var cachedState = disconnectedState.GetState(key);
      
      // If state is cached return it
      if (cachedState!=null && cachedState.IsLoaded) {
        var tuple = cachedState.Tuple!=null ? cachedState.Tuple.Clone() : null;
        var entityState = Session.UpdateEntityState(cachedState.Key, tuple, true);
        return cachedState.IsRemoved ? null : entityState;
      }

      // If state isn't cached try cache get it form storage
      if ((cachedState!=null && !cachedState.IsLoaded) || disconnectedState.IsConnected) {
        BeginChainedTransaction();
        var type = key.TypeRef.Type;
        Prefetch(key, type, PrefetchHelper.CreateDescriptorsForFieldsLoadedByDefault(type));
        ExecutePrefetchTasks(true);
        EntityState result;
        return TryGetEntityState(key, out result) ? result : null;
      }

      // If state unknown return null
      return null;
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      registry.GetItems(PersistenceState.New)
        .Apply(item => disconnectedState.Persist(item, PersistActionKind.Insert));
      registry.GetItems(PersistenceState.Modified)
        .Apply(item => disconnectedState.Persist(item, PersistActionKind.Update));
      registry.GetItems(PersistenceState.Removed)
        .Apply(item => disconnectedState.Persist(item, PersistActionKind.Remove));
    }

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      BeginChainedTransaction();
      base.ExecuteQueryTasks(queryTasks, allowPartialExecution);
    }

    /// <inheritdoc/>
    public override Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      BeginChainedTransaction();
      return base.CreateEnumerationContext();
    }

    /// <inheritdoc/>
    public override IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ManyToOne:
        case Multiplicity.ZeroToOne:
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          Session.Persist();
          var list = new List<ReferenceInfo>();
          var state = disconnectedState.GetState(target.Key);
          foreach (var reference in state.GetReferences(association.OwnerField)) {
            var item = FetchInstance(reference.Key);
            list.Add(new ReferenceInfo(item.Entity, target, association));
          }
          return list;
        case Multiplicity.OneToOne:
        case Multiplicity.OneToMany:
          var key = target.GetReferenceKey(association.Reversed.OwnerField);
          if (key!=null)
            return EnumerableUtils.One(new ReferenceInfo(FetchInstance(key).Entity, target, association));
          break;
      }
      throw new ArgumentException("association.Multiplicity");
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedSessionHandler(SessionHandler chainedHandler, DisconnectedState disconnectedState)
      : base(chainedHandler)
    {
      this.disconnectedState = disconnectedState;
    }
  }
}