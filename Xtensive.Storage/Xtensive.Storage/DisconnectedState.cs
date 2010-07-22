// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
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
  public sealed class DisconnectedState : IEnumerable<Entity>
  {
    private OperationLog serializedOperations;
    private SerializableEntityState[] serializedRegistry;
    private SerializableEntityState[] serializedGlobalRegistry;
    private VersionSet versions;

    [NonSerialized]
    private Session session;
    [NonSerialized]
    private IDisposable sessionHandlerSubstitutionScope;
    [NonSerialized]
    internal IDisposable transactionReplacementScope;
    [NonSerialized]
    internal IDisposable logIndentScope;
    [NonSerialized]
    private DisconnectedSessionHandler handler;
    [NonSerialized]
    private StateRegistry transactionalState;
    [NonSerialized]
    private StateRegistry state;
    [NonSerialized]
    private StateRegistry originalState;
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
    /// Gets a value indicating whether disconnected state
    /// was attached to a <see cref="Session"/> with already
    /// running transaction.
    /// </summary>
    public bool IsAttachedWhenTransactionWasOpen {
      get {
        return IsAttached && transactionReplacementScope!=null;
      }
    }

    /// <summary>
    /// Gets or sets the merge mode to use when loading state of new entities 
    /// into this <see cref="DisconnectedState"/>.
    /// </summary>
    public MergeMode MergeMode { get; set; }

    /// <summary>
    /// Gets the <see cref="VersionSet"/> storing
    /// original version of entities cached in this
    /// <see cref="DisconnectedState"/>.
    /// </summary>
    public VersionSet Versions {
      get { return versions; }
    }

    /// <summary>
    /// Gets the <see cref="OperationLog"/> storing
    /// information of <legacyBold>already committed</legacyBold> operations.
    /// So operations captured in the active transaction aren't exposed here.
    /// </summary>
    public OperationLog Operations {
      get { return state.Operations; }
    }

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
      
      if (Session.IsDebugEventLoggingEnabled)
        Log.Debug(Strings.LogSessionXDisconnectedStateConnect, Session);
      ConnectInternal();

      return new Disposable(disposing => {
        if (!IsConnected)
          return;
        if (Session.IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXDisconnectedStateDisconnect, Session);
        DisconnectInternal();
      });
    }

    /// <summary>
    /// Applies all the changes to the attached <see cref="Session"/>.
    /// </summary>
    /// <returns>Resulting key mapping.</returns>
    public KeyMapping ApplyChanges()
    {
      EnsureIsAttached();
      return ApplyChanges(Session);
    }

    /// <summary>
    /// Applies all the changes to the specified <see cref="Session"/>.
    /// </summary>
    /// <param name="targetSession">The session to apply the changes to.</param>
    /// <returns>Resulting key mapping.</returns>
    public KeyMapping ApplyChanges(Session targetSession)
    {
      ArgumentValidator.EnsureArgumentNotNull(targetSession, "targetSession");
      EnsureNoTransaction();

      IDisposable disposable = null;
      KeyMapping keyMapping;
      using (OpenDetachRegion())
      using (targetSession.Activate())
      try {
        if (targetSession.IsDebugEventLoggingEnabled) {
          disposable = Log.DebugRegion(Strings.LogSessionXDisconnectedStateApplyChanges, targetSession);
          Log.Debug("{0}", Operations);
        }
        using (VersionValidator.Attach(targetSession, key => Versions[key])) {
          keyMapping = state.Operations.Replay(targetSession);
          state.Commit(true);
        }
        if (targetSession.IsDebugEventLoggingEnabled) {
          Log.Debug(Strings.LogChangesAreSuccessfullyApplied);
          Log.Debug("{0}", keyMapping);
        }
        originalState.Remap(keyMapping);
        state = new StateRegistry(originalState);
        RebuildVersions();
      }
      finally {
        disposable.DisposeSafely();
      }
      // Remapping Entity keys, if necessary
      if (IsAttached && keyMapping.Map.Count!=0)
        session.RemapEntityKeys(keyMapping);
      return keyMapping;
    }

    /// <summary>
    /// Cancels all the changes.
    /// </summary>
    public void CancelChanges()
    {
      EnsureNoTransaction();
      Log.Debug(Strings.LogDisconnectedStateCancelChanges);
      state = new StateRegistry(originalState);
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>A clone of this instance.</returns>
    public DisconnectedState Clone()
    {
      EnsureNoTransaction();
      return Cloner.Default.Clone(this);
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
        RegisterState(entityState.Key, entityState.Tuple, 
          Versions[entityState.Key], mergeMode);
    }

    /// <summary>
    /// Gets the state of the entity cached in <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <returns>Persistence state;
    /// <see langword="null" />, if entity isn't cached.</returns>
    public PersistenceState? GetPersistenceState(Key key)
    {
      return GetPersistenceState(GetState(key));
    }

    /// <summary>
    /// Gets the sequence of all keys and entity persistence states cached by this <see cref="DisconnectedState"/>.
    /// </summary>
    /// <returns>The sequence of all keys and entity persistence states.</returns>
    public IEnumerable<KeyValuePair<Key, PersistenceState>> AllPersistenceStates()
    {
      EnsureIsAttached();
      var allKeys = new HashSet<Key>();
      var currentState = state;
      while (currentState!=null) {
        allKeys.UnionWith(currentState.Keys);
        currentState = currentState.Origin;
      }
      return 
        from key in allKeys
        let entityState = GetState(key)
        let persistenceState = GetPersistenceState(entityState)
        where persistenceState.HasValue
        select new KeyValuePair<Key, PersistenceState>(entityState.Key, persistenceState.GetValueOrDefault());
    }

    /// <summary>
    /// Gets the sequence of all the entities of the specified type cached by <see cref="DisconnectedState"/>,
    /// except removed ones.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>The sequence of all the entities of the specified type.</returns>
    public IEnumerable<TEntity> All<TEntity>()
      where TEntity : class, IEntity
    {
      return this.OfType<TEntity>();
    }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public IEnumerator<Entity> GetEnumerator()
    {
      EnsureIsAttached();
      Session.Persist();
      var all =
        from pair in AllPersistenceStates()
        where pair.Value!=PersistenceState.Removed
        let entity = Query.SingleOrDefault(pair.Key)
        where entity!=null
        select entity;
      return all.GetEnumerator();
    }

    #endregion

    #region Internal \ protected methods

    internal AssociationCache AssociationCache {
      get { return associationCache; }
    }

    internal void OnTransactionOpened()
    {
      DetachOperationCapturer();
      transactionalState = new StateRegistry(transactionalState ?? state);
      AttachOperationCapturer();
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
      DetachOperationCapturer();
      var origin = transactionalState.Origin;
      if (origin!=state) {
        transactionalState = origin;
        AttachOperationCapturer();
      }
      else {
        transactionalState = null;
      }
    }

    internal PersistenceState? GetPersistenceState(DisconnectedEntityState entityState)
    {
      if (entityState==null)
        return null;
      if (!entityState.IsLoaded)
        return null;
      if (entityState.IsRemoved)
        return PersistenceState.Removed;
      var originalEntityState = originalState.Get(entityState.Key);
      if (originalEntityState==null || originalEntityState.IsRemoved || !originalEntityState.IsLoaded)
        return PersistenceState.New;
      var differentialTuple = entityState.Tuple as DifferentialTuple;
      while (differentialTuple!=null) {
        if (differentialTuple.Difference!=null)
          return PersistenceState.Modified;
        differentialTuple = differentialTuple.Origin as DifferentialTuple;
      }
      return PersistenceState.Synchronized;
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
      if (tuple==null)
        return;

      DisconnectedEntityState entityState;
      if (transactionalState!=null)
        entityState = transactionalState.Get(key);
      else
        entityState = state.Get(key);

      if (entityState==null || !entityState.IsLoaded) {
        originalState.Register(key, tuple);
        Versions.Add(key, version, true);
        return;
      }

      var originalEntityState = originalState.Get(key);
      bool isNew = false;
      if (originalEntityState==null || !originalEntityState.IsLoaded)
        isNew = true;

      var existingVersion = isNew 
        ? VersionInfo.Void 
        : GetVersion(originalEntityState.Key.Type, originalEntityState.Tuple);
      var newVersion      = GetVersion(key.Type, tuple);

      if (existingVersion==newVersion)
        originalState.MergeUnavailableFields(key, tuple);
      else if (mergeMode==MergeMode.Strict)
        throw new VersionConflictException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      else if (mergeMode==MergeMode.PreferSource) {
        originalState.Merge(key, tuple);
        Versions.Add(key, version, true);
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

    private void RebuildVersions()
    {
      versions = new VersionSet();
      foreach (var entityState in originalState.EntityStates)
        if (entityState.Tuple!=null) {
          var version = GetVersion(entityState.Key.Type, entityState.Tuple);
          if (!version.IsVoid)
            Versions.Add(entityState.Key, version, true);
        }
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

    private IDisposable OpenDetachRegion()
    {
      var oldSession = Session;
      bool wasConnected = false;
      if (oldSession!=null) {
        if (IsConnected) {
          wasConnected = true;
          DisconnectInternal();
        }
        DetachInternal();
      }
      return new Disposable(disposing => {
        if (oldSession!=null)
          AttachInternal(oldSession);
        if (wasConnected)
          ConnectInternal();
      });
    }

    private void AttachInternal(Session session)
    {
      if (session.IsDebugEventLoggingEnabled)
        logIndentScope = Log.DebugRegion(Strings.LogSessionXDisconnectedStateAttach, Session);
      else
        logIndentScope = null;
      if (session.Transaction!=null) {
        session.Persist();
        session.Invalidate();
      }
      var directSessionAccessor = session.Services.Demand<DirectSessionAccessor>();
      transactionReplacementScope = directSessionAccessor.NullifySessionTransaction();
      handler = new DisconnectedSessionHandler(session.Handler, this);
      sessionHandlerSubstitutionScope = session.Services.Get<DirectSessionAccessor>()
        .ChangeSessionHandler(handler);
      session.DisconnectedState = this;
      this.session = session;
    }

    private void DetachInternal()
    {
      EnsureIsAttached();
      bool transactionWasOpen = transactionReplacementScope!=null;
      try {
        try {
          try {
            if (IsConnected)
              DisconnectInternal();
          }
          finally {
            sessionHandlerSubstitutionScope.DisposeSafely();
          }
        }
        finally {
          transactionReplacementScope.DisposeSafely();
        }
      }
      finally {
        try {
          logIndentScope.DisposeSafely();
        }
        finally {
          var oldSession = session;
          session.DisconnectedState = null;
          sessionHandlerSubstitutionScope = null;
          transactionReplacementScope = null;
          logIndentScope = null;
          session = null;
          handler = null;
          if (transactionWasOpen)
            oldSession.Invalidate();
        }
      }
    }

    private void ConnectInternal()
    {
      IsConnected = true;
    }

    private void DisconnectInternal()
    {
      IsConnected = false;
      handler.CommitChainedTransaction();
    }

    private static VersionInfo GetVersion(TypeInfo type, Tuple tuple)
    {
      if (type.VersionExtractor==null)
        return new VersionInfo();
      var versionTuple = type.VersionExtractor.Apply(TupleTransformType.Tuple, tuple);
      return new VersionInfo(versionTuple);
    }

    private void AttachOperationCapturer()
    {
      operationCapturer = OperationCapturer.Attach(Session, transactionalState.Operations);
    }

    private void DetachOperationCapturer()
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
      versions = new VersionSet();
    }


    // Serialization

    [OnSerializing]
    protected void OnSerializing(StreamingContext context)
    {
      EnsureNoTransaction();
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
    }

    [OnDeserialized]
    protected void OnDeserialized(StreamingContext context)
    {
      var domain = Session.Demand().Domain;

      associationCache = new AssociationCache();

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