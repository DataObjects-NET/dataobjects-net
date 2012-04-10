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
using Xtensive.Diagnostics;
using Xtensive.Disposing;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;
using Xtensive.Orm.Disconnected;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Orm.Operations;

using Xtensive.Orm.Services;

namespace Xtensive.Orm
{
  /// <summary>
  /// Disconnected state.
  /// </summary>
  [Serializable]
  public sealed class DisconnectedState : IEnumerable<Entity>,
    IVersionSetProvider
  {
    private OperationLog serializedOperations;
    private SerializableEntityState[] serializedState;
    private SerializableEntityState[] serializedOriginalState;
    private VersionSet versions;
    private VersionsUsageOptions versionsUsageOptions = VersionsUsageOptions.Default;
    private VersionsProviderType versionsProviderType = VersionsProviderType.Default;
    private OperationLogType operationLogType;
    private Func<OperationLog, IEnumerable<IOperation>> operationLogReplayFilter;
    private Func<Key, bool> versionUpdateFilter;

    [NonSerialized]
    private OperationLogType logType;
    [NonSerialized]
    private bool attachedWhenTransactionExisted;
    [NonSerialized]
    private Session session;
    [NonSerialized]
    private IVersionSetProvider versionsProvider;
    [NonSerialized]
    private IDisposable sessionHandlerSubstitutionScope;
    [NonSerialized] 
    private IDisposable logIndentScope;
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
    /// Gets or sets the <see cref="VersionSet"/> storing
    /// original version of entities cached in this
    /// <see cref="DisconnectedState"/>.
    /// </summary>
    public VersionSet Versions {
      get { return versions; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        versions = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Versions"/> provider used by this instance to
    /// refresh <see cref="Versions"/> content during <see cref="ApplyChanges()"/>
    /// method execution.
    /// <see langword="null" /> indicates that provider shouldn't be used.
    /// </summary>
    /// <remarks>
    /// When the value of this property is set, value of
    /// <see cref="VersionsProviderType"/> is automatically set to
    /// <see cref="Orm.VersionsProviderType.Other"/>.
    /// </remarks>
    public IVersionSetProvider VersionsProvider {
      get {
        switch (versionsProviderType) {
        case VersionsProviderType.Session:
          return session;
        case VersionsProviderType.DisconnectedState:
          return this;
        case VersionsProviderType.Other:
          return versionsProvider;
        default: // None
          return null;
        }
      }
      set {
        versionsProvider = value;
        versionsProviderType = VersionsProviderType.Other;
      }
    }

    /// <summary>
    /// Gets or sets the versions provider selection mode.
    /// </summary>
    public VersionsProviderType VersionsProviderType {
      get { return versionsProviderType; }
      set { versionsProviderType = value; }
    }

    /// <summary>
    /// Gets or sets the versions usage options.
    /// </summary>
    public VersionsUsageOptions VersionsUsageOptions {
      get { return versionsUsageOptions; }
      set { versionsUsageOptions = value; }
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
    /// Gets or sets the merge mode to use when loading state of new entities 
    /// into this <see cref="DisconnectedState"/>.
    /// </summary>
    public MergeMode MergeMode { get; set; }

    /// <summary>
    /// Gets or sets the type of the <see cref="OperationLog"/> to use.
    /// </summary>
    public OperationLogType OperationLogType {
      get { return operationLogType; }
      set {
        if (value==OperationLogType.UndoOperationLog)
          throw new ArgumentOutOfRangeException("value");
        EnsureNoTransaction();
        if (state.Operations.Count!=0)
          throw new InvalidOperationException(Strings.ExYouMustEitherApplyOrCancelCachedChangesToChangeThisProperty);
        operationLogType = value;
        state.Operations = new OperationLog(operationLogType);
      }
    }

    /// <summary>
    /// Gets or sets the operation log filter used by <see cref="ApplyChanges()"/>.
    /// Only operations returned by filter are applied; others are left in
    /// <see cref="Operations"/>.
    /// <see langword="null" /> value (default) indicates all the operations are applied.
    /// </summary>
    public Func<OperationLog, IEnumerable<IOperation>> OperationLogReplayFilter {
      get { return operationLogReplayFilter; }
      set { operationLogReplayFilter = value; }
    }

    /// <summary>
    /// Gets or sets the version filter.
    /// This predicate is used after <see cref="ApplyChanges()"/>
    /// to determine which <see cref="Versions"/> must be refreshed.
    /// <see langword="null" /> value (default) indicates all the versions are refreshed.
    /// </summary>
    public Func<Key, bool> VersionUpdateFilter {
      get { return versionUpdateFilter; }
      set { versionUpdateFilter = value; }
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

      IDisposable disposable = null;
      KeyMapping keyMapping;
      EnsureNoTransaction();
      using (OpenDetachRegion())
      using (targetSession.Activate())
        try {
          if (targetSession.IsDebugEventLoggingEnabled) {
            disposable = Log.DebugRegion(Strings.LogSessionXDisconnectedStateApplyChanges, targetSession);
            Log.Debug("{0}", Operations);
          }
          var originalVersions = Versions; // Necessary, because it will be changed later
          var versionValidator = (VersionsUsageOptions & VersionsUsageOptions.Validate)!=0
            ? VersionValidator.Attach(targetSession, key => originalVersions[key])
            : null;
          using (versionValidator)
          using (var tx = Session.Demand().OpenTransaction()) {
            var replayFilter = OperationLogReplayFilter ?? (log => log);
            var operations = state.Operations;
            var operationsToApply = new OperationLog(operationLogType, replayFilter.Invoke(operations));

            keyMapping = operations.Replay(targetSession);
            // Updating internal state.
            // Note that if commit will fail, the state will be inconsistent with DB.
            state.Commit(true);
            originalState.Remap(keyMapping);
            state = new StateRegistry(originalState);
            if (operationsToApply.Count != operations.Count) // Not everything is applied
              state.Operations = new OperationLog(operationLogType, operations.Except(operationsToApply));

            // Updating (refetching / rebuilding) versions
            var currentVersionProvider = VersionsProviderType==VersionsProviderType.Session
              ? targetSession // not session, but targetSession
              : VersionsProvider;
            if (currentVersionProvider!=null && (VersionsUsageOptions & VersionsUsageOptions.Update)!=0) {
              var versionUpdateFilter = VersionUpdateFilter ?? (key => true);
              Versions = currentVersionProvider.CreateVersionSet( AllKeys().Where(versionUpdateFilter) );
            }
            tx.Complete();
          }
          if (targetSession.IsDebugEventLoggingEnabled) {
            Log.Debug(Strings.LogChangesAreSuccessfullyApplied);
            Log.Debug("{0}", keyMapping);
          }
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
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      bool inTransaction = IsAttached && Session.Transaction!=null;

      if (inTransaction)
        Session.Persist(PersistReason.DisconnectedStateMerge);
      try {
        var sourceStates = source.originalState.EntityStates;
        foreach (var entityState in sourceStates)
          RegisterEntityState(
            entityState.Key,
            entityState.Tuple,
            source.Versions[entityState.Key],
            mergeMode);
      }
      finally {
        if (inTransaction)
          Session.Invalidate();
      }
    }

    /// <summary>
    /// Gets the state of the entity cached in <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="key">The key of the entity.</param>
    /// <returns>Persistence state;
    /// <see langword="null" />, if entity isn't cached.</returns>
    public PersistenceState? GetPersistenceState(Key key)
    {
      return GetPersistenceState(GetEntityState(key));
    }

    /// <summary>
    /// Gets the sequence of all keys of entities cached by this <see cref="DisconnectedState"/>.
    /// Removed entity keys aren't included to this sequence.
    /// </summary>
    /// <returns>The sequence of all keys of entities cached by this <see cref="DisconnectedState"/>.</returns>
    public IEnumerable<Key> AllKeys()
    {
      return AllKeys(false);
    }

    /// <summary>
    /// Gets the sequence of all keys of entities cached by this <see cref="DisconnectedState"/>.
    /// </summary>
    /// <param name="includingRemovedEntityKeys">If set to <see langword="true"/> removed entity keys will also be included.</param>
    /// <returns>
    /// The sequence of all keys of entities cached by this <see cref="DisconnectedState"/>.
    /// </returns>
    public IEnumerable<Key> AllKeys(bool includingRemovedEntityKeys)
    {
      var keys = new HashSet<Key>();
      var currentState = GetCurrentState();
      while (currentState!=null) {
        keys.UnionWith(currentState.Keys);
        currentState = currentState.Origin;
      }
      if (!includingRemovedEntityKeys)
        return keys;
      else
        return 
          from key in keys
          let entityState = GetEntityState(key)
          let persistenceState = GetPersistenceState(entityState)
          where persistenceState.HasValue
          select key;
    }

    /// <summary>
    /// Gets the sequence of all keys and entity persistence states cached by this <see cref="DisconnectedState"/>.
    /// </summary>
    /// <returns>The sequence of all keys and entity persistence states.</returns>
    public IEnumerable<KeyValuePair<Key, PersistenceState>> AllPersistenceStates()
    {
      return 
        from key in AllKeys(true)
        let entityState = GetEntityState(key)
        let persistenceState = GetPersistenceState(entityState)
        where persistenceState.HasValue
        select new KeyValuePair<Key, PersistenceState>(key, persistenceState.GetValueOrDefault());
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

    #region IVersionSetProvider members


    /// <summary>
    /// Creates <see cref="VersionSet"/> set containing versions
    /// for specified <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The keys to create version set for.</param>
    /// <returns>
    ///   <see cref="VersionSet"/> containing versions
    /// for specified <paramref name="keys"/>.
    /// </returns>
    public VersionSet CreateVersionSet(IEnumerable<Key> keys)
    {
      var result = new VersionSet();
      foreach (var key in keys) {
        var entityState = GetEntityState(key);
        if (entityState==null)
          continue;
        var tuple = entityState.Tuple;
        if (tuple==null)
          continue;
        var version = GetVersion(entityState.Key.TypeInfo, tuple);
        if (version.IsVoid)
          continue;
        result.Add(entityState.Key, version, true);
      }
      return result;
    }

    #endregion

    #region IEnumerable<...> methods

    
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<Entity> GetEnumerator()
    {
      EnsureIsAttached();
      Session.Persist(PersistReason.DisconnectedStateEnumeration);
      var all =
        from pair in AllPersistenceStates()
        where pair.Value!=PersistenceState.Removed
        let entity = Session.Query.SingleOrDefault(pair.Key)
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
      if (!entityState.IsLoadedOrRemoved)
        return null;
      if (entityState.IsRemoved)
        return PersistenceState.Removed;
      var originalEntityState = originalState.Get(entityState.Key);
      if (originalEntityState==null || originalEntityState.IsRemoved || !originalEntityState.IsLoadedOrRemoved)
        return PersistenceState.New;
      var differentialTuple = entityState.Tuple as DifferentialTuple;
      while (differentialTuple!=null) {
        if (differentialTuple.Difference!=null)
          return PersistenceState.Modified;
        differentialTuple = differentialTuple.Origin as DifferentialTuple;
      }
      return PersistenceState.Synchronized;
    }

    private StateRegistry GetCurrentState()
    {
      return transactionalState ?? state;
    }

    internal DisconnectedEntityState GetEntityState(Key key)
    {
      return GetCurrentState().Get(key);
    }
    
    internal DisconnectedEntityState RegisterEntityState(Key key, Tuple tuple)
    {
      RegisterEntityState(key, tuple, GetVersion(key.TypeReference.Type, tuple), MergeMode);
      return GetEntityState(key);
    }

    /// <exception cref="VersionConflictException">Version check failed.</exception>
    internal void RegisterEntityState(Key key, Tuple tuple, VersionInfo version, MergeMode mergeMode)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      var existingVersion = Versions[key];
      var versionConflict = 
        (VersionsUsageOptions & VersionsUsageOptions.Validate)!=0 
          && (!existingVersion.IsVoid)
            && existingVersion!=version;
      if (versionConflict && mergeMode == MergeMode.Strict) {
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogSessionXVersionValidationFailedKeyYVersionZExpected3,
            Session!=null ? Session.ToString() : "None (DisconnectedState)", 
            key, version, existingVersion);
        throw new VersionConflictException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      }

      if (tuple==null)
        return;
      var entityState = originalState.Get(key); // originalState must be used here, not the current one!

      if (entityState==null || !entityState.IsLoadedOrRemoved)
        originalState.Create(key, tuple, true);
      else if (mergeMode!=MergeMode.PreferOriginal)
        originalState.UpdateOrigin(key, tuple, MergeBehavior.PreferDifference); 
      else // mergeMode==MergeMode.PreferOrigin
        originalState.UpdateOrigin(key, tuple, MergeBehavior.PreferOrigin);
      if ((VersionsUsageOptions & VersionsUsageOptions.Update)!=0 && (existingVersion.IsVoid || mergeMode==MergeMode.PreferNew))
        Versions.Add(key, version, true);
    }

    internal DisconnectedEntitySetState RegisterEntitySetState(Key key, FieldInfo fieldInfo, bool isFullyLoaded, 
      List<Key> entities, List<Pair<Key, Tuple>> auxEntities)
    {
      EnsureIsAttached();

      if (auxEntities!=null)
        foreach (var entity in auxEntities)
          RegisterEntityState(entity.First, entity.Second);

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
        transactionalState.Create(entityState.Key, entityState.Tuple.ToRegular(), false);
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
      if (Session.Transaction != null && Session.Transaction.IsDisconnected)
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
      logIndentScope = session.IsDebugEventLoggingEnabled 
        ? Log.DebugRegion(Strings.LogSessionXDisconnectedStateAttach, Session) 
        : null;

      if (session.Transaction!=null) {
        session.Persist(PersistReason.DisconnectedStateAttach);
        if (!session.Transaction.IsActuallyStarted) {
          session.BeginTransaction(session.Transaction);
          session.EnsureTransactionIsStarted();
        }
        session.Invalidate();
      }

      this.session = session;
      session.DisconnectedState = this;
      handler = new DisconnectedSessionHandler(session.Handler, this);
      var directSessionAccessor = session.Services.Get<DirectSessionAccessor>();
      sessionHandlerSubstitutionScope = directSessionAccessor.ChangeSessionHandler(handler);

      if (session.Transaction != null) {
        OnTransactionOpened();
        attachedWhenTransactionExisted = true;
      }

      if (versionsProvider==this)
        versionsProvider = session;
    }

    private void DetachInternal()
    {
      EnsureIsAttached();
      try
      {
        try {
          if (attachedWhenTransactionExisted) {
            attachedWhenTransactionExisted = false;
            OnTransactionCommited();
          }
          if (IsConnected)
            DisconnectInternal();
        }
        finally {
          sessionHandlerSubstitutionScope.DisposeSafely();
        }
      }
      finally {
        try {
          logIndentScope.DisposeSafely();
        }
        finally {
          var oldSession = session;
          if (versionsProvider==oldSession)
            versionsProvider = this;
          session.DisconnectedState = null;
          sessionHandlerSubstitutionScope = null;
          logIndentScope = null;
          session = null;
          handler = null;
          oldSession.Invalidate();
        }
      }
    }

    private void ConnectInternal()
    {
      IsConnected = true;
      handler.Connect();
    }

    private void DisconnectInternal()
    {
      IsConnected = false;
      handler.Disconnect();
    }

    private static VersionInfo GetVersion(TypeInfo type, Tuple tuple)
    {
      if (type.VersionExtractor==null)
        return VersionInfo.Void;
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
    /// Initializes a new instance of this class.
    /// </summary>
    public DisconnectedState()
    {
      associationCache = new AssociationCache(this);
      originalState = new StateRegistry(this, associationCache);
      state = new StateRegistry(originalState);
      versions = new VersionSet();
      versionsProvider = this;
    }


    // Serialization

    [OnSerializing]
    private void OnSerializing(StreamingContext context)
    {
      EnsureNoTransaction();
      serializedOperations = state.Operations;
      serializedOriginalState = originalState.EntityStates
        .Select(entityState => entityState.ToSerializable()).ToArray();
      serializedState = state.EntityStates
        .Select(entityState => entityState.ToSerializable()).ToArray();
    }

    [OnSerialized]
    private void OnSerialized(StreamingContext context)
    {
      serializedState = null;
      serializedOriginalState = null;
      serializedOperations = null;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      session = Session.Demand();
      var domain = session.Domain;

      versionsProvider = this;
      associationCache = new AssociationCache(this);
      originalState = new StateRegistry(this, associationCache);
      state = new StateRegistry(originalState);
      
      foreach (var entityState in serializedOriginalState)
        originalState.AddState(DisconnectedEntityState.FromSerializable(entityState, originalState, domain));
      foreach (var entityState in serializedState)
        state.AddState(DisconnectedEntityState.FromSerializable(entityState, state, domain));
      state.Operations = serializedOperations;

      serializedOriginalState = null;
      serializedState = null;
      serializedOperations = null;
    }
  }
}