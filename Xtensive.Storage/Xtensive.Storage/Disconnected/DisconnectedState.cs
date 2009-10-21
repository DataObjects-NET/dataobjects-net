// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.09.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using TupleExtensions=Xtensive.Storage.Internals.TupleExtensions;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// The disconnected state.
  /// </summary>
  [Serializable]
  public sealed class DisconnectedState
  {
    private IDisposable disposable;
    private DisconnectedSessionHandler handler;
    private Session session;
    private readonly Dictionary<Key, VersionInfo> versionCache = new Dictionary<Key, VersionInfo>();
    private DisconnectedStateRegistry transactionalCache = new DisconnectedStateRegistry();
    private readonly DisconnectedStateRegistry cache = new DisconnectedStateRegistry();
    
    /// <summary>
    /// Gets the session this instance attached to.
    /// </summary>
    public Session Session { get { return session; } }

    /// <summary>
    /// Gets a value indicating whether this instance is attached to session.
    /// </summary>
    public bool IsAttached { get { return disposable!=null; } }

    /// <summary>
    /// Attach to specified session.
    /// </summary>
    /// <param name="session">The session.</param>
    public void Attach(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      if (IsAttached)
        throw new InvalidOperationException("DisconnectedState is already attached to session.");
      if (session.Transaction!=null)
        throw new InvalidOperationException("Can't attach DisconnectedState to session with active transaction.");

      this.session = session;
      handler = new DisconnectedSessionHandler(Session.Handler, this);
      disposable = Session.CoreServices.ChangeSessionHandler(handler);
    }

    /// <summary>
    /// Detaches this instance.
    /// </summary>
    public void Detach()
    {
      if (!IsAttached)
        throw new InvalidOperationException("DisconnectedState is already detached.");
      if (session.Transaction!=null)
        throw new InvalidOperationException("Can't detach DisconnectedState from session with active transaction.");

      session = null;
      handler = null;
      disposable.Dispose();
      disposable = null;
    }

    /// <summary>
    /// Prefetches instances to <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="prefetcher">The prefetcher.</param>
    public void Prefetch(Action prefetcher)
    {
      if (!IsAttached)
        throw new InvalidOperationException("DisconnectedState is detached.");

      try {
        handler.BeginChainedTransaction();
        if (prefetcher!=null)
          prefetcher.Invoke();
        handler.CommitChainedTransaction();
      }
      catch {
        handler.RollbackChainedTransaction();
        throw;
      }
    }

    /// <summary>
    /// Saves all the changes to storage.
    /// </summary>
    public void SaveChanges()
    {
      if (!IsAttached)
        throw new InvalidOperationException("DisconnectedState is detached.");
      
      var itemsToSave = cache.ChangedEntityStates.ToList();
      if (itemsToSave.Count==0)
        return;
      var tepmSession = Session;
      Detach();
      try {
        using (var transactionScope = Transaction.Open(tepmSession)) {
          CheckVersions(itemsToSave, tepmSession.Handler);
          foreach (var cachedState in itemsToSave) {
            var tuple = cachedState.DifferenceTuple!=null
              ? new DifferentialTuple(cachedState.OriginalTuple, cachedState.DifferenceTuple)
              : cachedState.OriginalTuple;
            var entityState = tepmSession.UpdateEntityState(cachedState.Key, tuple, true);
            entityState.PersistenceState = cachedState.PersistenceState;
          }
          transactionScope.Complete();
        }
        foreach (var entityState in itemsToSave)
          entityState.PersistenceState = PersistenceState.Synchronized;
      }
      finally {
        Attach(tepmSession);
      }
    }

    private void CheckVersions(IEnumerable<DisconnectedEntityState> items, SessionHandler handler)
    {
      var versionsToCheck = new Dictionary<Key, VersionInfo>();
      foreach (var item in items)
        if (versionCache.ContainsKey(item.Key))
          versionsToCheck.Add(item.Key, versionCache[item.Key]);
      var actualVersions = handler.GetActualVersions(versionsToCheck.Keys);
      foreach (var versionPair in versionsToCheck)
        if (!actualVersions.ContainsKey(versionPair.Key) 
          || actualVersions[versionPair.Key] != versionPair.Value)
          throw new InvalidOperationException("Version conflict.");
    }
    

    internal DisconnectedEntityState GetEntityState(Key key)
    {
      // Try get from transactional cache
      var cachedState = transactionalCache.GetEntityState(key);
      if (cachedState!=null)
        return cachedState;
      
      // Try get from cache
      cachedState = cache.GetEntityState(key);
      if (cachedState==null)
        return null;
      var entitySets = cache.GetEntitySetStates(cachedState.Key);
      var references = cache.GetReferences(cachedState.Key);
      cachedState = transactionalCache.AddEntityState(cachedState, entitySets, references);
      return cachedState;
    }
    
    internal DisconnectedEntitySetState GetEntitySetState(Key ownerKey, string fieldName)
    {
      // Try get from transactional cache
      var cachedState = transactionalCache.GetEntitySetState(ownerKey, fieldName);
      if (cachedState!=null)
        return cachedState;
      
      // Try get from cache
      cachedState = cache.GetEntitySetState(ownerKey, fieldName);
      if (cachedState==null)
        return null;
      return transactionalCache.RegisterEntitySetState(cachedState);
    }

    internal IEnumerable<DisconnectedEntityState> GetReferenceTo(Key key, string fieldName)
    {
      var states = new List<DisconnectedEntityState>();
      var cachedRefs = transactionalCache.GetReferencesTo(key, fieldName);
      if (cachedRefs!=null)
        foreach (var targetKey in cachedRefs)
          states.Add(GetEntityState(targetKey));
      return states;
    }

    
    internal DisconnectedEntityState RegisterEntityState(Key key, Tuple tuple)
    {
      // Store object version
      if (key.TypeRef.Type.VersionExtractor!=null && !versionCache.ContainsKey(key)) {
        var versionTuple = key.TypeRef.Type.VersionExtractor.Apply(TupleTransformType.Tuple, tuple);
        versionCache.Add(key, new VersionInfo(versionTuple));
      }
      
      var cachedState = cache.RegisterEntityState(key, tuple);
      if (cachedState.PersistenceState == PersistenceState.Removed)
        return cachedState;
      cachedState = transactionalCache.RegisterEntityState(key, cachedState.OriginalTuple);
      return cachedState;
    }

    internal DisconnectedEntitySetState RegisterEntitySetState(Key key, string fieldInfo, bool isFullyLoaded, 
      List<Key> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      if (auxEntities!=null)
        foreach (var entity in auxEntities)
          RegisterEntityState(entity.First, entity.Second);

      cache.RegisterEntitySetState(key, fieldInfo, entities, isFullyLoaded);
      return transactionalCache.RegisterEntitySetState(key, fieldInfo, entities, isFullyLoaded);
    }


    internal void Persist(EntityState entityState, PersistActionKind persistAction)
    {
      switch (persistAction) {
        case PersistActionKind.Insert:
          transactionalCache.Insert(entityState.Key, entityState.Tuple.ToRegular());
          break;
        case PersistActionKind.Update:
          transactionalCache.Update(entityState.Key, entityState.DifferentialTuple.Difference.ToRegular());
          break;
        case PersistActionKind.Remove:
          transactionalCache.Remove(entityState.Key);
          break;
        default:
          throw new ArgumentOutOfRangeException("persistAction");
      }
    }

    internal void BeginTransaction()
    {
      transactionalCache = new DisconnectedStateRegistry();
    }

    internal void CommitTransaction()
    {
      foreach (var entityState in transactionalCache.ChangedEntityStates) {
        switch (entityState.PersistenceState) {
          case PersistenceState.New:
            cache.Insert(entityState.Key, entityState.OriginalTuple.Clone());
            break;
          case PersistenceState.Modified:
            cache.Update(entityState.Key, entityState.DifferenceTuple.Clone());
            break;
          case PersistenceState.Removed:
            cache.Remove(entityState.Key);
            break;
          default:
            throw new ArgumentOutOfRangeException("entityState.PersistenceState");
        }
      }
      transactionalCache = null;
    }

    internal void RollbackTransaction()
    {
      transactionalCache = null;
    }
  }
}