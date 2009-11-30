// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.23

using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;
using Xtensive.Storage.Internals;
using System;
using Xtensive.Storage.Operations;
using Xtensive.Storage.Resources;
using Xtensive.Core.Disposing;
using System.Linq;

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
    private StateRegistry transactionalRegistry;
    [NonSerialized]
    private StateRegistry registry;
    [NonSerialized]
    private StateRegistry globalRegistry;
    [NonSerialized]
    private IDisposable disposable;
    [NonSerialized]
    private DisconnectedSessionHandler handler;
    [NonSerialized]
    private Session session;
    [NonSerialized]
    private Logger logger;
    [NonSerialized]
    private readonly ModelHelper modelHelper = new ModelHelper();
    
    internal bool IsConnected { get; set; }

    #region Public API

    /// <summary>
    /// Gets the session this instance attached to.
    /// </summary>
    public Session Session { get { return session; } }

    /// <summary>
    /// Gets a value indicating whether this instance is attached to session.
    /// </summary>
    public bool IsAttached { get { return disposable!=null; } }

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
            keyMapping = registry.OperationSet.Apply(tempSession);
            transactionScope.Complete();
          }
          registry.Commit();
          registry = new StateRegistry(globalRegistry);
          versionCache = new Dictionary<Key, VersionInfo>();
          foreach (var state in globalRegistry.States)
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
      if (Transaction.Current!=null)
        throw new InvalidOperationException(Strings.ExTransactionShouldNotBeActive);

      registry = new StateRegistry(globalRegistry);
    }

    /// <summary>
    /// Merges state with the specified source state.
    /// </summary>
    /// <param name="source">The source state.</param>
    /// <param name="mergeMode">The merge mode.</param>
    public void Merge(DisconnectedState source, MergeMode mergeMode)
    {
      var sourceStates = source.globalRegistry.States;
      foreach (var state in sourceStates) {
        RegisterState(state.Key, state.Tuple, source.GetOriginalVerion(state.Key), mergeMode);
      }
    }
    
    #endregion
    
    #region Internal API

    internal ModelHelper ModelHelper { get { return modelHelper; } }
    
    internal VersionInfo GetOriginalVerion(Key key)
    {
      VersionInfo originalVersion;
      if (versionCache.TryGetValue(key, out originalVersion))
        return originalVersion;
      return new VersionInfo();
    }

    internal void OnTransactionStarted()
    {
      transactionalRegistry = new StateRegistry(registry);
      logger = new Logger(Session, transactionalRegistry.OperationSet);
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
      EnsureIsAttached();
      return transactionalRegistry.GetState(key);
    }
    
    internal DisconnectedEntityState RegisterState(Key key, Tuple tuple)
    {
      RegisterState(key, tuple, GetVersion(key.TypeRef.Type, tuple), MergeMode);
      return GetState(key);
    }

    internal void RegisterState(Key key, Tuple tuple, VersionInfo version, MergeMode mergeMode)
    {
      DisconnectedEntityState cachedState;
      if (transactionalRegistry!=null)
        cachedState = transactionalRegistry.GetState(key);
      else
        cachedState = registry.GetState(key);

      if (cachedState==null || !cachedState.IsLoaded) {
        globalRegistry.Register(key, tuple);
        versionCache[key] = version;
        return;
      }

      var targetVersion = GetVersion(cachedState.Key.Type, cachedState.Tuple);
      var sourceVersion = GetVersion(key.Type, tuple);

      var isVersionEquals =
        (targetVersion.IsVoid && sourceVersion.IsVoid)
          || (!targetVersion.IsVoid && !sourceVersion.IsVoid && targetVersion==sourceVersion);

      if (isVersionEquals)
        globalRegistry.MergeUnloadedFields(key, tuple);
      else if (mergeMode==MergeMode.Restrict)
        throw new InvalidOperationException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      else if (mergeMode==MergeMode.PreferSource) {
        globalRegistry.Merge(key, tuple);
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
      EnsureIsAttached();

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

    #endregion
    
    internal IOperationSet GetOperationLog()
    {
      return registry.OperationSet;
    }

    private void CloseConnection()
    {
      if (!IsConnected)
        return;
      IsConnected = false;
      handler.CommitChainedTransaction();
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
      disposable = Session.CoreServices.ChangeSessionHandler(handler);
    }

    private void Detach()
    {
      EnsureIsAttached();
      if (IsConnected)
        CloseConnection();
      session = null;
      handler = null;
      disposable.Dispose();
      disposable = null;
    }

    private static VersionInfo GetVersion(TypeInfo type, Tuple tuple)
    {
      if (type.VersionExtractor==null)
        return new VersionInfo();
      
      var versionTuple = type.VersionExtractor.Apply(TupleTransformType.Tuple, tuple);
      return new VersionInfo(versionTuple);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DisconnectedState()
    {
      modelHelper = new ModelHelper();
      globalRegistry = new StateRegistry(this);
      registry = new StateRegistry(globalRegistry);
      versionCache = new Dictionary<Key, VersionInfo>();
    }


    // Serialization

    [OnSerializing]
    protected void OnSerializing(StreamingContext context)
    {
      serializedVersions = versionCache.Select(pair => 
        new KeyValuePair<string, VersionInfo>(pair.Key.ToString(true), pair.Value))
        .ToArray();
      serializedSet = registry.OperationSet;
      serializedRegistry = registry.States.Select(state => state.Serialize()).ToArray();
      serializedGlobalRegistry = globalRegistry.States.Select(state => state.Serialize()).ToArray();
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

      versionCache = new Dictionary<Key, VersionInfo>();
      foreach (var pair in serializedVersions)
        versionCache.Add(Key.Parse(domain, pair.Key), pair.Value);

      globalRegistry = new StateRegistry(this);
      foreach (var state in serializedGlobalRegistry)
        globalRegistry.AddState(DisconnectedEntityState.Deserialize(state, globalRegistry, domain));
      serializedGlobalRegistry = null;

      registry = new StateRegistry(globalRegistry);
      registry.OperationSet = serializedSet;
      foreach (var state in serializedRegistry)
        registry.AddState(DisconnectedEntityState.Deserialize(state, registry, domain));
      serializedRegistry = null;
      serializedSet = null;
    }
  }
}