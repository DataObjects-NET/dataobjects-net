// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Storage.Disconnected;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Services;

namespace Xtensive.Storage
{
  /// <summary>
  /// Disconnected state.
  /// </summary>
  [Serializable]
  public sealed class DisconnectedState
  {
    private OperationLog serializedOperations;
    private KeyValuePair<string, VersionInfo>[] serializedVersions;
    private SerializableEntityState[] serializedRegistry;
    private SerializableEntityState[] serializedGlobalRegistry;
    
    [NonSerialized]
    private Dictionary<Key, VersionInfo> versionCache;
    [NonSerialized]
    private StateRegistry transactionalState;
    [NonSerialized]
    private StateRegistry state;
    [NonSerialized]
    private StateRegistry originalState;
    [NonSerialized]
    private IDisposable sessionHandlerSubstitutionScope;
    [NonSerialized]
    private DisconnectedSessionHandler handler;
    [NonSerialized]
    private Session session;
    [NonSerialized]
    private OperationCapturer operationCapturer;
    [NonSerialized]
    private AssociationCache associationCache;
    

    /// <summary>
    /// Gets the session this instance attached to.
    /// </summary>
    public Session Session {
      get { return session; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is attached to <see cref="Session"/>.
    /// See <see cref="Attach()"/> method for details.
    /// </summary>
    public bool IsAttached {
      get { return sessionHandlerSubstitutionScope!=null; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is "connected".
    /// See <see cref="Connect"/> method for details.
    /// </summary>
    public bool IsConnected {
      get; private set;
    }

    /// <summary>
    /// Gets a value indicating whether local transaction is open.
    /// </summary>
    public bool IsLocalTransactionOpen {
      get { return transactionalState!=null; }
    }

    /// <summary>
    /// Gets or sets the merge mode to use when loading state of new entities 
    /// into this <see cref="DisconnectedState"/>.
    /// </summary>
    public MergeMode MergeMode { get; set; }

    /// <summary>
    /// Attaches the disconnected state to the current session.
    /// </summary>
    /// <returns>A disposable object that will detach the disconnected state
    /// on its disposal.</returns>
    /// <exception cref="InvalidOperationException">Transaction is running.</exception>
    public IDisposable Attach()
    {
      return Attach(Session.Demand());
    }

    /// <summary>
    /// Attaches the disconnected state to the specified session.
    /// </summary>
    /// <param name="session">The session to attach disconnected state to.</param>
    /// <returns>A disposable object that will detach the disconnected state
    /// on its disposal.</returns>
    /// <exception cref="InvalidOperationException">Transaction is running.</exception>
    public IDisposable Attach(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      EnsureNotAttached();
      if (session.Transaction!=null)
        throw new InvalidOperationException(Strings.ExTransactionIsRunning);
      
      AttachInternal(session);
      return new Disposable<DisconnectedState>(this, (b, _this) => _this.DetachInternal());
    }

    /// <summary>
    /// "Connects" the disconnected state. 
    /// When disconnected state is connected, it is allowed to
    /// forward the queries to the underlying <see cref="Session"/>.
    /// </summary>
    /// <returns>A disposable object that will disconnected the disconnected state
    /// on its disposal.</returns>
    public IDisposable Connect()
    {
      EnsureIsAttached();
      if (IsConnected)
        return null;
      
      IsConnected = true;
      return new Disposable<DisconnectedState>(this, (b, _this) => _this.CloseConnection());
    }

    /// <summary>
    /// Applies all the changes to the attached <see cref="Session"/>.
    /// </summary>
    /// <returns>Resulting key mapping.</returns>
    public KeyMapping ApplyChanges()
    {
      return ApplyChanges(Session);
    }

    /// <summary>
    /// Applies all the changes to the specified <see cref="Session"/>.
    /// </summary>
    /// <param name="targetSession">The session to apply the changes to.</param>
    /// <returns>Resulting key mapping.</returns>
    public KeyMapping ApplyChanges(Session targetSession)
    {
      EnsureIsAttached();
      EnsureNoTransaction();
      var attachedSession = Session;
      DetachInternal();
      try {
        KeyMapping keyMapping;
        using (VersionValidator.Attach(targetSession, GetOriginalVersion)) {
          keyMapping = state.Operations.Replay(targetSession);
          state.Commit(true);
        }
        originalState.Remap(keyMapping);
        state = new StateRegistry(originalState);
        RebuildVersionCache();
        return keyMapping;
      }
      finally {
        AttachInternal(attachedSession);
      }
    }

    /// <summary>
    /// Cancels all the changes.
    /// </summary>
    public void CancelChanges()
    {
      EnsureIsAttached();
      EnsureNoTransaction();
      state = new StateRegistry(originalState);
    }

    /// <summary>
    /// Merges this instance with the specified source <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="source">The source <see cref="DisconnectedState"/>.</param>
    public void Merge(DisconnectedState source)
    {
      Merge(source, MergeMode);
    }

    /// <summary>
    /// Merges this instance with the specified source <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="source">The source <see cref="DisconnectedState"/>.</param>
    /// <param name="mergeMode">The merge mode to use.</param>
    public void Merge(DisconnectedState source, MergeMode mergeMode)
    {
      var sourceStates = source.originalState.EntityStates;
      foreach (var entityState in sourceStates)
        RegisterState(entityState.Key, entityState.Tuple, source.GetOriginalVersion(entityState.Key), mergeMode);
    }

    /// <summary>
    /// Gets the original entity version.
    /// </summary>
    /// <param name="entity">The entity to get the original version for.</param>
    /// <returns>Original version.</returns>
    public VersionInfo GetOriginalVersion(Entity entity)
    {
      if (entity==null)
        return new VersionInfo();
      else
        return GetOriginalVersion(entity.Key);
    }

    /// <summary>
    /// Gets the original entity version for the specified key.
    /// </summary>
    /// <param name="key">The key of the entity to get the original version for.</param>
    /// <returns>Original version.</returns>
    public VersionInfo GetOriginalVersion(Key key)
    {
      VersionInfo originalVersion;
      if (versionCache.TryGetValue(key, out originalVersion))
        return originalVersion;
      return new VersionInfo();
    }

    #region Internal \ protected methods

    internal AssociationCache AssociationCache {
      get { return associationCache; }
    }

    internal void OnTransactionOpened()
    {
      DisposeOperationLogger();
      transactionalState = new StateRegistry(transactionalState ?? state);
      CreateOperationLogger();
    }

    internal void OnTransactionCommited()
    {
      transactionalState.Commit(false);
      OnTransactionClosed();
    }

    internal void OnTransactionRollbacked()
    {
      OnTransactionClosed();
    }

    private void OnTransactionClosed()
    {
      DisposeOperationLogger();
      var origin = transactionalState.Origin;
      if (origin!=state) {
        transactionalState = origin;
        CreateOperationLogger();
      }
      else {
        transactionalState = null;
      }
    }

    internal DisconnectedEntityState GetState(Key key)
    {
      EnsureIsAttached();
      return transactionalState.Get(key);
    }
    
    internal DisconnectedEntityState RegisterState(Key key, Tuple tuple)
    {
      RegisterState(key, tuple, GetVersion(key.TypeRef.Type, tuple), MergeMode);
      return GetState(key);
    }

    /// <exception cref="VersionConflictException">Version check failed.</exception>
    internal void RegisterState(Key key, Tuple tuple, VersionInfo version, MergeMode mergeMode)
    {
      DisconnectedEntityState cachedState;
      if (transactionalState!=null)
        cachedState = transactionalState.Get(key);
      else
        cachedState = state.Get(key);

      if (cachedState==null || !cachedState.IsLoaded) {
        originalState.Register(key, tuple);
        versionCache[key] = version;
        return;
      }

      var targetVersion = GetVersion(cachedState.Key.Type, cachedState.Tuple);
      var sourceVersion = GetVersion(key.Type, tuple);

      if (targetVersion==sourceVersion)
        originalState.MergeUnavailableFields(key, tuple);
      else if (mergeMode==MergeMode.Strict)
        throw new VersionConflictException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      else if (mergeMode==MergeMode.PreferSource) {
        originalState.Merge(key, tuple);
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

      var state = this.state.GetOrCreate(key);
      var setState = state.GetEntitySetState(fieldInfo);
      setState.IsFullyLoaded = isFullyLoaded;
      foreach (var entity in entities)
        if (!setState.Items.ContainsKey(entity))
          setState.Items.Add(entity, entity);
      return transactionalState.Get(key).GetEntitySetState(fieldInfo);
    }

    internal void Persist(EntityState entityState, PersistActionKind persistAction)
    {
      EnsureIsAttached();

      switch (persistAction) {
        case PersistActionKind.Insert:
          transactionalState.Insert(entityState.Key, entityState.Tuple.ToRegular());
          break;
        case PersistActionKind.Update:
          transactionalState.Update(entityState.Key, entityState.DifferentialTuple.Difference.ToRegular());
          break;
        case PersistActionKind.Remove:
          transactionalState.Remove(entityState.Key);
          break;
        default:
          throw new ArgumentOutOfRangeException("persistAction");
      }
    }

    private void RebuildVersionCache()
    {
      versionCache = new Dictionary<Key, VersionInfo>();
      foreach (var entityState in originalState.EntityStates)
        if (entityState.Tuple!=null) {
          var version = GetVersion(entityState.Key.Type, entityState.Tuple);
          if (!version.IsVoid)
            versionCache.Add(entityState.Key, version);
        }
    }

    private void CloseConnection()
    {
      if (!IsConnected)
        return;
      IsConnected = false;
      handler.EndChainedTransaction();
    }

    private void EnsureNotAttached()
    {
      if (IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsAlreadyAttachedToSession);
    }

    private void EnsureIsAttached()
    {
      if (!IsAttached)
        throw new InvalidOperationException(Strings.ExDisconnectedStateIsDetached);
    }

    private void EnsureNoTransaction()
    {
      if (!IsAttached)
        return;
      if (Session.Transaction!=null)
        throw new InvalidOperationException(Strings.ExTransactionIsRunning);
    }

    private void AttachInternal(Session session)
    {
      handler = new DisconnectedSessionHandler(session.Handler, this);
      sessionHandlerSubstitutionScope = session.Services.Get<DirectSessionAccessor>()
        .ChangeSessionHandler(handler);
      session.DisconnectedState = this;
      this.session = session;
    }

    private void DetachInternal()
    {
      EnsureIsAttached();
      try {
        try {
          if (IsConnected)
            CloseConnection();
        }
        finally {
          sessionHandlerSubstitutionScope.DisposeSafely();
        }
      }
      finally {
        session.DisconnectedState = null;
        session = null;
        handler = null;
        sessionHandlerSubstitutionScope = null;
      }
    }

    private static VersionInfo GetVersion(TypeInfo type, Tuple tuple)
    {
      if (type.VersionInfoTupleExtractor==null)
        return new VersionInfo();
      var versionTuple = type.VersionInfoTupleExtractor.Apply(TupleTransformType.Tuple, tuple);
      return new VersionInfo(versionTuple);
    }

    private void CreateOperationLogger()
    {
      operationCapturer = OperationCapturer.Attach(Session, transactionalState.Operations);
    }

    private void DisposeOperationLogger()
    {
      try {
        operationCapturer.DisposeSafely();
      }
      finally {
        operationCapturer = null;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedState()
    {
      associationCache = new AssociationCache();
      originalState = new StateRegistry(associationCache);
      state = new StateRegistry(originalState);
      versionCache = new Dictionary<Key, VersionInfo>();
    }


    // Serialization

    [OnSerializing]
    protected void OnSerializing(StreamingContext context)
    {
      serializedVersions = versionCache.Select(pair => 
        new KeyValuePair<string, VersionInfo>(pair.Key.ToString(true), pair.Value))
        .ToArray();
      serializedOperations = state.Operations;
      serializedRegistry = state.EntityStates
        .Select(entityState => entityState.Serialize()).ToArray();
      serializedGlobalRegistry = originalState.EntityStates
        .Select(entityState => entityState.Serialize()).ToArray();
    }

    [OnSerialized]
    protected void OnSerialized(StreamingContext context)
    {
      serializedRegistry = null;
      serializedGlobalRegistry = null;
      serializedOperations = null;
      serializedVersions = null;
    }

    [OnDeserialized]
    protected void OnDeserialized(StreamingContext context)
    {
      var domain = Session.Demand().Domain;

      associationCache = new AssociationCache();

      versionCache = new Dictionary<Key, VersionInfo>();
      foreach (var pair in serializedVersions)
        versionCache.Add(Key.Parse(domain, pair.Key), pair.Value);

      originalState = new StateRegistry(associationCache);
      foreach (var entityState in serializedGlobalRegistry)
        originalState.AddState(DisconnectedEntityState.Deserialize(entityState, originalState, domain));
      serializedGlobalRegistry = null;

      state = new StateRegistry(originalState) {
        Operations = serializedOperations
      };
      foreach (var entityState in serializedRegistry)
        state.AddState(DisconnectedEntityState.Deserialize(entityState, state, domain));
      serializedRegistry = null;
      serializedOperations = null;
    }
  }
}