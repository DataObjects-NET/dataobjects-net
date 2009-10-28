// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Internals;
using System;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Disconnected.Log;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected state.
  /// </summary>
  public sealed class DisconnectedState
  {
    private StateRegistry transactionalRegistry;
    private StateRegistry registry;
    private readonly StateRegistry globalRegistry;
    private IDisposable disposable;
    private DisconnectedSessionHandler handler;
    private Session session;
    private readonly Dictionary<Key, VersionInfo> versionCache;
    private Logger logger;

    internal ModelHelper ModelHelper { get; private set; }

    # region Public API

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
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsAlreadyAttachedToSession);
      if (session.Transaction!=null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsPresent);

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
        return;
      if (session.Transaction!=null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsPresent);

      session = null;
      handler = null;
      disposable.Dispose();
      disposable = null;
    }

    /// <summary>
    /// Prefetches instances to <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="prefetcher">The prefetcher.</param>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    public void Prefetch(Action prefetcher)
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);

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
//      throw new NotSupportedException();
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);
      var tempSession = Session;
      try {
        Detach();
        using (var transactionScope = Transaction.Open(tempSession)) {
          registry.Log.Apply(tempSession);
          transactionScope.Complete();
        }
      }
      finally {
        Attach(tempSession);
      }

//      var itemsToSave = cache.GetChanges()
//        .Where(item => item.State!=PersistenceState.Synchronized)
//        .ToList();
//      if (itemsToSave.Count==0)
//        return;
//      var tempSession = Session;
//      Detach();
//      try {
//        using (var transactionScope = Transaction.Open(tempSession)) {
//          CheckVersions(itemsToSave, tempSession.Handler);
//          foreach (var cachedState in itemsToSave) {
//            var tuple = cachedState.Actual!=null
//              ? new DifferentialTuple(cachedState.Actual, cachedState.Differences)
//              : cachedState.Actual;
//            var entityState = tempSession.UpdateEntityState(cachedState.Key, tuple, true);
//            entityState.PersistenceState = cachedState.State;
//          }
//          transactionScope.Complete();
//        }
//        // TODO: Complete (removes)
//        foreach (var entityState in itemsToSave)
//          entityState.State = PersistenceState.Synchronized;
//      }
//      finally {
//        Attach(tempSession);
//      }
    }

    /// <summary>
    /// Clears all changes.
    /// </summary>
    public void ClearChanges()
    {
      registry = new StateRegistry(globalRegistry);
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

    # endregion
    
    # region Internal API

    internal void OnTransactionStarted()
    {
      transactionalRegistry = new StateRegistry(registry);
      logger = new Logger(Session, transactionalRegistry.Log);
    }

    internal void OnTransactionCommited()
    {
      transactionalRegistry.Commit();
      transactionalRegistry = null;
      logger.DisposeSafely();
      logger = null;
    }

    internal void OnTransactionRollbacked()
    {
      logger.DisposeSafely();
      transactionalRegistry = null;
    }

    internal DisconnectedEntityState GetState(Key key)
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);

      return transactionalRegistry.GetState(key);
    }

    internal DisconnectedEntityState RegisterState(Key key, Tuple tuple)
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);

      // Store object version
      if (key.TypeRef.Type.VersionExtractor!=null && !versionCache.ContainsKey(key)) {
        var versionTuple = key.TypeRef.Type.VersionExtractor.Apply(TupleTransformType.Tuple, tuple);
        versionCache.Add(key, new VersionInfo(versionTuple));
      }

      globalRegistry.Register(key, tuple);
      return GetState(key);
    }

    internal DisconnectedEntitySetState RegisterSetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Key> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);

      if (auxEntities!=null)
        foreach (var entity in auxEntities)
          RegisterState(entity.First, entity.Second);

      var state = registry.GetForUpdate(key);
      var setState = state.GetEntitySetState(fieldInfo);
      setState.IsFullyLoaded = isFullyLoaded;
      foreach (var entity in entities)
        if (!setState.Items.ContainsKey(entity))
          setState.Items.Add(entity, entity);
      return transactionalRegistry.GetState(key).GetEntitySetState(fieldInfo);
    }

    internal void Persist(EntityState entityState, PersistActionKind persistAction)
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);

      switch (persistAction) {
        case PersistActionKind.Insert:
          transactionalRegistry.Insert(entityState.Key, entityState.Tuple.ToRegular());
          break;
        case PersistActionKind.Update:
          transactionalRegistry.Update(entityState.Key, entityState.DifferentialTuple.Difference.ToRegular());
          break;
        case PersistActionKind.Remove:
          transactionalRegistry.Remove(entityState.Key);
          break;
        default:
          throw new ArgumentOutOfRangeException("persistAction");
      }
    }

    # endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedState()
    {
      ModelHelper = new ModelHelper();
      globalRegistry = new StateRegistry(this);
      registry = new StateRegistry(globalRegistry);
      versionCache = new Dictionary<Key, VersionInfo>();
    }
  }
}