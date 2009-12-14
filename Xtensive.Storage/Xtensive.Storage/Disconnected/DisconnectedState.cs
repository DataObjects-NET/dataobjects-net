// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Disconnected
{
  /// <summary>
  /// Disconnected state.
  /// </summary>
  [Serializable]
  public sealed class DisconnectedState
  {
    private OperationSet serializedSet;
    private KeyValuePair<string, VersionInfo>[] serializedVersions;
    private SerializableEntityState[] serializedRegistry;
    private SerializableEntityState[] serializedGlobalRegistry;
    
    [NonSerialized]
    private Dictionary<Key, VersionInfo> versionCache;
    [NonSerialized]
    private StateRegistry transactionalStates;
    [NonSerialized]
    private StateRegistry commitedStates;
    [NonSerialized]
    private StateRegistry originalStates;
    [NonSerialized]
    private IDisposable sessionHandlerSubstitutionScope;
    [NonSerialized]
    private DisconnectedSessionHandler handler;
    [NonSerialized]
    private Session session;
    [NonSerialized]
    private OperationLogger operationLogger;
    [NonSerialized]
    private ModelRequestCache modelRequestCache;
    
    #region Public API

    /// <summary>
    /// Gets the session this instance attached to.
    /// </summary>
    public Session Session { get { return session; } }

    /// <summary>
    /// Gets a value indicating whether this instance is attached to <see cref="Session"/>.
    /// See <see cref="Attach"/> method for details.
    /// </summary>
    public bool IsAttached { get { return sessionHandlerSubstitutionScope!=null; } }

    /// <summary>
    /// Gets a value indicating whether this instance is "connected".
    /// See <see cref="Connect"/> method for details.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Gets or sets the merge mode.
    /// </summary>
    public MergeMode MergeMode { get; set; }

    /// <summary>
    /// Attach to specified session.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>Scope.</returns>
    public IDisposable Attach(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      EnsureIsNotAttached();
      if (session.Transaction!=null)
        throw new InvalidOperationException(Strings.ExActiveTransactionIsPresent);
      
      AttachInternal(session);
      return new Disposable<DisconnectedState>(this, (b, _this) => _this.Detach());
    }

    /// <summary>
    /// Opens connection.
    /// </summary>
    /// <returns>Scope.</returns>
    public IDisposable Connect()
    {
      EnsureIsAttached();
      if (IsConnected)
        return null;
      
      IsConnected = true;
      return new Disposable<DisconnectedState>(this, (b, _this) => _this.CloseConnection());
    }

    /// <summary>
    /// Saves all the changes to storage.
    /// </summary>
    public KeyMapping SaveChanges()
    {
      EnsureIsAttached();
      KeyMapping keyMapping;
      var tempSession = Session;
      var newVersionCache = new Dictionary<Key, VersionInfo>();
      Detach();
      try {
        using (VersionValidator.Attach(tempSession, GetOriginalVerion)) {
          using (var transactionScope = Transaction.Open(tempSession)) {
            keyMapping = commitedStates.OperationSet.Apply(tempSession);
            transactionScope.Complete();
          }
          commitedStates.Commit();
          commitedStates = new StateRegistry(originalStates);
          versionCache = new Dictionary<Key, VersionInfo>();
          foreach (var state in originalStates.States)
            if (state.Tuple!=null) {
              var version = GetVersion(state.Key.Type, state.Tuple);
              if (!version.IsVoid)
                versionCache.Add(state.Key, version);
            }
        }
      }
      finally {
        AttachInternal(tempSession);
      }
      return keyMapping;
    }

    /// <summary>
    /// Clears all changes.
    /// </summary>
    public void ClearChanges()
    {
      if (Session.Transaction!=null)
        throw new InvalidOperationException(Strings.ExTransactionShouldNotBeActive);

      commitedStates = new StateRegistry(originalStates);
    }

    /// <summary>
    /// Merges state with the specified source state.
    /// </summary>
    /// <param name="source">The source state.</param>
    /// <param name="mergeMode">The merge mode.</param>
    public void Merge(DisconnectedState source, MergeMode mergeMode)
    {
      var sourceStates = source.originalStates.States;
      foreach (var state in sourceStates) {
        RegisterState(state.Key, state.Tuple, source.GetOriginalVerion(state.Key), mergeMode);
      }
    }
    
    #endregion
    
    #region Internal API

    internal bool TransactionIsStarted { get { return transactionalStates!=null; } }

    internal ModelRequestCache ModelRequestCache { get { return modelRequestCache; } }
    
    internal VersionInfo GetOriginalVerion(Key key)
    {
      VersionInfo originalVersion;
      if (versionCache.TryGetValue(key, out originalVersion))
        return originalVersion;
      return new VersionInfo();
    }

    internal void OnTransactionStarted()
    {
      DisposeLogger();
      transactionalStates = new StateRegistry(transactionalStates ?? commitedStates);
      CreateLogger();
    }

    internal void OnTransactionCommited()
    {
      transactionalStates.Commit();
      OnTransactionEnded();
    }

    internal void OnTransactionRollbacked()
    {
      OnTransactionEnded();
    }

    internal DisconnectedEntityState GetState(Key key)
    {
      EnsureIsAttached();
      return transactionalStates.GetState(key);
    }
    
    internal DisconnectedEntityState RegisterState(Key key, Tuple tuple)
    {
      RegisterState(key, tuple, GetVersion(key.TypeRef.Type, tuple), MergeMode);
      return GetState(key);
    }

    internal void RegisterState(Key key, Tuple tuple, VersionInfo version, MergeMode mergeMode)
    {
      DisconnectedEntityState cachedState;
      if (transactionalStates!=null)
        cachedState = transactionalStates.GetState(key);
      else
        cachedState = commitedStates.GetState(key);

      if (cachedState==null || !cachedState.IsLoaded) {
        originalStates.Register(key, tuple);
        versionCache[key] = version;
        return;
      }

      var targetVersion = GetVersion(cachedState.Key.Type, cachedState.Tuple);
      var sourceVersion = GetVersion(key.Type, tuple);

      var isVersionEquals =
        (targetVersion.IsVoid && sourceVersion.IsVoid)
          || (!targetVersion.IsVoid && !sourceVersion.IsVoid && targetVersion==sourceVersion);

      if (isVersionEquals)
        originalStates.MergeUnloadedFields(key, tuple);
      else if (mergeMode==MergeMode.Strict)
        throw new InvalidOperationException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      else if (mergeMode==MergeMode.PreferSource) {
        originalStates.Merge(key, tuple);
        versionCache[key] = version;
      }
    }

    internal DisconnectedEntitySetState RegisterSetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Key> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      EnsureIsAttached();

      if (auxEntities!=null)
        foreach (var entity in auxEntities)
          RegisterState(entity.First, entity.Second);

      var state = commitedStates.GetForUpdate(key);
      var setState = state.GetEntitySetState(fieldInfo);
      setState.IsFullyLoaded = isFullyLoaded;
      foreach (var entity in entities)
        if (!setState.Items.ContainsKey(entity))
          setState.Items.Add(entity, entity);
      return transactionalStates.GetState(key).GetEntitySetState(fieldInfo);
    }

    internal void Persist(EntityState entityState, PersistActionKind persistAction)
    {
      EnsureIsAttached();

      switch (persistAction) {
        case PersistActionKind.Insert:
          transactionalStates.Insert(entityState.Key, entityState.Tuple.ToRegular());
          break;
        case PersistActionKind.Update:
          transactionalStates.Update(entityState.Key, entityState.DifferentialTuple.Difference.ToRegular());
          break;
        case PersistActionKind.Remove:
          transactionalStates.Remove(entityState.Key);
          break;
        default:
          throw new ArgumentOutOfRangeException("persistAction");
      }
    }

    #endregion
    
    private void CloseConnection()
    {
      if (!IsConnected)
        return;
      IsConnected = false;
      handler.EndChainedTransaction();
    }

    private void EnsureIsNotAttached()
    {
      if (IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsAlreadyAttachedToSession);
    }

    private void EnsureIsAttached()
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);
    }

    private void AttachInternal(Session session)
    {
      this.session = session;
      handler = new DisconnectedSessionHandler(Session.Handler, this);
      sessionHandlerSubstitutionScope = Session.CoreServices.ChangeSessionHandler(handler);
    }

    private void Detach()
    {
      EnsureIsAttached();
      if (IsConnected)
        CloseConnection();
      session = null;
      handler = null;
      sessionHandlerSubstitutionScope.Dispose();
      sessionHandlerSubstitutionScope = null;
    }

    private static VersionInfo GetVersion(TypeInfo type, Tuple tuple)
    {
      if (type.VersionExtractor==null)
        return new VersionInfo();
      
      var versionTuple = type.VersionExtractor.Apply(TupleTransformType.Tuple, tuple);
      return new VersionInfo(versionTuple);
    }

    private void CreateLogger()
    {
      operationLogger = OperationLogger.Attach(Session, transactionalStates.OperationSet);
    }

    private void DisposeLogger()
    {
      try {
        operationLogger.DisposeSafely();
      }
      finally {
        operationLogger = null;
      }
    }

    private void OnTransactionEnded()
    {
      DisposeLogger();
      var origin = transactionalStates.Origin;
      if (origin!=commitedStates) {
        transactionalStates = origin;
        CreateLogger();
      }
      else {
        transactionalStates = null;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedState()
    {
      modelRequestCache = new ModelRequestCache();
      originalStates = new StateRegistry(modelRequestCache);
      commitedStates = new StateRegistry(originalStates);
      versionCache = new Dictionary<Key, VersionInfo>();
    }


    // Serialization

    [OnSerializing]
    protected void OnSerializing(StreamingContext context)
    {
      serializedVersions = versionCache.Select(pair => 
        new KeyValuePair<string, VersionInfo>(pair.Key.ToString(true), pair.Value))
        .ToArray();
      serializedSet = commitedStates.OperationSet;
      serializedRegistry = commitedStates.States.Select(state => state.Serialize()).ToArray();
      serializedGlobalRegistry = originalStates.States.Select(state => state.Serialize()).ToArray();
    }

    [OnSerialized]
    protected void OnSerialized(StreamingContext context)
    {
      serializedRegistry = null;
      serializedGlobalRegistry = null;
      serializedSet = null;
      serializedVersions = null;
    }

    [OnDeserialized]
    protected void OnDeserialized(StreamingContext context)
    {
      var domain = Session.Demand().Domain;

      modelRequestCache = new ModelRequestCache();

      versionCache = new Dictionary<Key, VersionInfo>();
      foreach (var pair in serializedVersions)
        versionCache.Add(Key.Parse(domain, pair.Key), pair.Value);

      originalStates = new StateRegistry(modelRequestCache);
      foreach (var state in serializedGlobalRegistry)
        originalStates.AddState(DisconnectedEntityState.Deserialize(state, originalStates, domain));
      serializedGlobalRegistry = null;

      commitedStates = new StateRegistry(originalStates);
      commitedStates.OperationSet = serializedSet;
      foreach (var state in serializedRegistry)
        commitedStates.AddState(DisconnectedEntityState.Deserialize(state, commitedStates, domain));
      serializedRegistry = null;
      serializedSet = null;
    }
  }
}