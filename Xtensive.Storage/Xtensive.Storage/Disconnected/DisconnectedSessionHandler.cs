// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.01

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
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
    private bool isChainedTransactionStarted;

    # region Transaction

    /// <inheritdoc/>
    public override void BeginTransaction()
    {
      disconnectedState.BeginTransaction();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      if (isChainedTransactionStarted)
        ChainedHandler.CommitTransaction();
      isChainedTransactionStarted = false;
      disconnectedState.CommitTransaction();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      if (isChainedTransactionStarted)
        ChainedHandler.RollbackTransaction();
      isChainedTransactionStarted = false;
      disconnectedState.RollbackTransaction();
    }

    public void BeginChainedTransaction()
    {
      if (!isChainedTransactionStarted) {
        ChainedHandler.BeginTransaction();
        isChainedTransactionStarted = true;
      }
    }

    public void CommitChainedTransaction()
    {
      if (isChainedTransactionStarted) {
        isChainedTransactionStarted = false;
        ChainedHandler.CommitTransaction();
      }
    }

    public void RollbackChainedTransaction()
    {
      if (isChainedTransactionStarted) {
        isChainedTransactionStarted = false;
        ChainedHandler.RollbackTransaction();
      }
    }

    # endregion

    internal override bool TryGetEntityState(Key key, out EntityState entityState)
    {
      if (Session.EntityStateCache.TryGetItem(key, true, out entityState))
        return true;
      
      var cachedEntityState = disconnectedState.GetEntityState(key);
      if (cachedEntityState!=null) {
        entityState = Session.UpdateEntityState(key, cachedEntityState.OriginalTuple, true);
        return true;
      }
      return false;
    }

    internal override EntityState RegisterEntityState(Key key, Tuple tuple)
    {
      var cachedEntityState = tuple==null 
        ? disconnectedState.GetEntityState(key) 
        : disconnectedState.RegisterEntityState(key, tuple);
     
      // tuple==null && key is not cached || cached entity state is removed
      if (cachedEntityState==null || cachedEntityState.PersistenceState==PersistenceState.Removed)
        return Session.UpdateEntityState(key, null, true);

      var entityState = Session.UpdateEntityState(key, cachedEntityState.OriginalTuple, true);
      
      // Fetch version roots
      if (isChainedTransactionStarted && entityState.Type.HasVersionRoots) {
        var entity = entityState.Entity as IHasVersionRoots;
        if (entity!=null)
          entity.GetVersionRoots().ToList();
      }
      return entityState;
    }

    internal override bool TryGetEntitySetState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      if (base.TryGetEntitySetState(key, fieldInfo, out entitySetState))
        return true;
      var cachedState = disconnectedState.GetEntitySetState(key, fieldInfo.Name);
      if (cachedState!=null) {
        entitySetState = UpdateEntitySetState(key, fieldInfo, cachedState.Items.ToList(), true);
        return true;
      }
      entitySetState = null;
      return false;
    }

    internal override EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      var cachedOwner = disconnectedState.GetEntityState(key);
      if (cachedOwner==null || cachedOwner.PersistenceState == PersistenceState.Removed)
        return null;

      // Merge with disconnected state cache
      var cachedState = disconnectedState
        .RegisterEntitySetState(key, fieldInfo.Name, isFullyLoaded, entityKeys, auxEntities);
      
      // Update session cache
      return UpdateEntitySetState(key, fieldInfo, cachedState.Items, cachedState.IsFullyLoaded);
    }

    /// <inheritdoc/>
    protected internal override EntityState FetchInstance(Key key)
    {
      var cachedState = disconnectedState.GetEntityState(key);
      if (cachedState!=null) {
        var entityState = Session.UpdateEntityState(cachedState.Key, cachedState.OriginalTuple, true);
        return cachedState.PersistenceState==PersistenceState.Removed ? null : entityState;
      }
      if (isChainedTransactionStarted)
        return base.FetchInstance(key);
      return null;
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool dirtyFlush)
    {
      registry.GetItems(PersistenceState.New)
        .Apply(item => disconnectedState.Persist(item, PersistActionKind.Insert));
      registry.GetItems(PersistenceState.Modified)
        .Apply(item => disconnectedState.Persist(item, PersistActionKind.Update));
      registry.GetItems(PersistenceState.Removed)
        .Apply(item => disconnectedState.Persist(item, PersistActionKind.Remove));
    }

    /// <inheritdoc/>
    public override void Execute(IList<QueryTask> queryTasks, bool dirty)
    {
      if (!isChainedTransactionStarted)
        throw new InvalidOperationException();

      base.Execute(queryTasks, dirty);
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> Execute(Rse.Providers.ExecutableProvider provider)
    {
      if (!isChainedTransactionStarted)
        throw new InvalidOperationException();

      return base.Execute(provider);
    }

    protected internal override IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      var key = target.Key;
      string fieldName = null;

      switch (association.Multiplicity) {
        case Multiplicity.ManyToOne:
          fieldName = association.Reversed.OwnerField.Name;
          break;
        case Multiplicity.ZeroToOne:
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          fieldName = association.OwnerField.Name;
          break;
        default:
          return base.GetReferencesTo(target, association);
      }

      Session.Persist(false);
      var list = new List<ReferenceInfo>();
      foreach (var state in disconnectedState.GetReferenceTo(target.Key, association.OwnerField.Name)) {
        var item = Session.UpdateEntityState(state.Key, state.OriginalTuple, true);
        list.Add(new ReferenceInfo(item.Entity, target, association));
      }
      return list;
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