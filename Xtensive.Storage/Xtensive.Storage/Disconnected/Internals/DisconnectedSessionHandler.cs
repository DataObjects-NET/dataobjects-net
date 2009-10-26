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
      disconnectedState.OnTransactionStarted();
    }

    /// <inheritdoc/>
    public override void CommitTransaction()
    {
      if (isChainedTransactionStarted)
        ChainedHandler.CommitTransaction();
      isChainedTransactionStarted = false;
      disconnectedState.OnTransactionCommited();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction()
    {
      if (isChainedTransactionStarted)
        ChainedHandler.RollbackTransaction();
      isChainedTransactionStarted = false;
      disconnectedState.OnTransactionRollbacked();
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
      
      var cachedEntityState = disconnectedState.GetState(key);
      if (cachedEntityState!=null && cachedEntityState.IsLoaded) {
        var tuple = cachedEntityState.Tuple!=null ? cachedEntityState.Tuple.Clone() : null;
        entityState = Session.UpdateEntityState(key, tuple, true);
        return true;
      }
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
      if (cachedEntityState==null || cachedEntityState.IsRemoved)
        return Session.UpdateEntityState(key, null, true);
      
      var entityState = Session.UpdateEntityState(cachedEntityState.Key, cachedEntityState.Tuple.Clone(), true);
      
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
      if (isChainedTransactionStarted)
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
      return false;
    }

    internal override EntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
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
    protected internal override EntityState FetchInstance(Key key)
    {
      var cachedState = disconnectedState.GetState(key);
      if (cachedState!=null && cachedState.IsLoaded) {
        var tuple = cachedState.Tuple!=null ? cachedState.Tuple.Clone() : null;
        var entityState = Session.UpdateEntityState(cachedState.Key, tuple, true);
        return cachedState.IsRemoved ? null : entityState;
      }
      if (cachedState!=null && !cachedState.IsLoaded)
        throw new ConnectionRequiredException();
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
        throw new ConnectionRequiredException();

      base.Execute(queryTasks, dirty);
    }

    /// <inheritdoc/>
    public override IEnumerator<Tuple> Execute(Rse.Providers.ExecutableProvider provider)
    {
      if (!isChainedTransactionStarted)
        throw new ConnectionRequiredException();

      return base.Execute(provider);
    }

    /// <inheritdoc/>
    protected internal override IEnumerable<ReferenceInfo> GetReferencesTo(Entity target, AssociationInfo association)
    {
      switch (association.Multiplicity) {
        case Multiplicity.ManyToOne:
        case Multiplicity.ZeroToOne:
        case Multiplicity.ZeroToMany:
        case Multiplicity.ManyToMany:
          Session.Persist(false);
          var list = new List<ReferenceInfo>();
          var state = disconnectedState.GetState(target.Key);
          foreach (var reference in state.GetReferences(association.OwnerField)) {
            var item = FetchInstance(reference.Key);
            list.Add(new ReferenceInfo(item.Entity, target, association));
          }
          return list;
        default:
          return base.GetReferencesTo(target, association);
      }
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