// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Sorting;
using Xtensive.Integrity.Atomicity;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Providers;
using Xtensive.Storage.ReferentialIntegrity;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Data context, which all persistent objects are bound to.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each session has its own connection to database and set of materialized persistent instates.
  /// It contains identity map and tracks changes in bound persistent classes.
  /// </para>
  /// <para>
  /// <c>Session</c> implements <see cref="IContext"/> interface, it means that each <c>Session</c>
  /// can be either active or not active in particular thread (see <see cref="IsActive"/> property).
  /// Each thread can contain only one active session, it can be a accessed via 
  /// <see cref="Current">Session.Current</see> property or <see cref="Demand">Session.Demand()</see> method.
  /// </para>
  /// <para>
  /// Session can be open and activated by <see cref="Domain.OpenSession()">Domain.OpenSession()</see> method. 
  /// Existing session can be activated by <see cref="Activate"/> method.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code source="..\..\Xtensive.Storage\Xtensive.Storage.Manual\DomainAndSessionSample.cs" region="Session sample"></code>
  /// </example>
  /// <seealso cref="Domain"/>
  /// <seealso cref="SessionBound" />
  public partial class Session : DomainBound,
    IContext<SessionScope>,IResource
  {
    private bool isPersisting;
    private volatile bool isDisposed;
    private readonly Set<object> consumers = new Set<object>();
    private readonly object _lock = new object();
    private ServiceProvider serviceProvider;

    /// <summary>
    /// Gets the configuration of the <see cref="Session"/>.
    /// </summary>
    public SessionConfiguration Configuration { get; private set; }

    /// <summary>
    /// Gets the name of the <see cref="Session"/>
    /// (useful mainly for debugging purposes - e.g. it is used in logs).
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Indicates whether debug event logging is enabled.
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </summary>
    public bool IsDebugEventLoggingEnabled { get; private set; }

    #region Private \ internal members

    internal SessionHandler Handler { get; set; }
    
    internal HandlerAccessor Handlers { get; private set; }

    internal CoreServiceAccessor CoreServices { get; private set; }

    internal SyncManager PairSyncManager { get; private set; }

    internal RemovalProcessor RemovalProcessor { get; private set; }

    private void NotifyDisposing()
    {
      if (!SystemLogicOnly && OnDisposing!=null)
        OnDisposing(this, EventArgs.Empty);
    }

    private void NotifyPersisting()
    {
      if (!SystemLogicOnly && OnPersisting!=null)
        OnPersisting(this, EventArgs.Empty);
    }

    private void NotifyPersist()
    {
      if (!SystemLogicOnly && OnPersist!=null)
        OnPersist(this, EventArgs.Empty);
    }

    internal void NotifyCreateEntity(Entity entity)
    {
      if (!SystemLogicOnly && OnCreateEntity!=null)
        OnCreateEntity(this, new EntityEventArgs(entity));
    }

    internal void NotifyRemovingEntity(Entity entity)
    {
      if (!SystemLogicOnly && OnRemovingEntity!=null)
        OnRemovingEntity(this, new EntityEventArgs(entity));
    }

    internal void NotifyRemoveEntity(Entity entity)
    {
      if (!SystemLogicOnly && OnRemoveEntity!=null)
        OnRemoveEntity(this, new EntityEventArgs(entity));
    }

    #endregion

    /// <summary>
    /// Gets the session service provider.
    /// </summary>
    public ServiceProvider Services {
      get {
        if (serviceProvider==null)
          serviceProvider = new ServiceProvider(this);
        return serviceProvider;
      }
    }

    /// <summary>
    /// Persists all modified instances immediately.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. 
    /// </para>
    /// <para>
    /// Note, that this method is called automatically when it's necessary,
    /// e.g. before beginning, committing and rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Session is already disposed.</exception>
    public void Persist()
    {
      if (isPersisting)
        return;
      isPersisting = true;
      try {
        EnsureNotDisposed();

        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Persisting...", this);
        NotifyPersisting();

        Handler.Persist(GetStatesToPersist());

        if (IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Persisted.", this);
        NotifyPersist();

        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.New))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.Modified))
          item.PersistenceState = PersistenceState.Synchronized;
        EntityStateRegistry.Clear();
      }
      finally {
        isPersisting = false;
      }
    }

    private IEnumerable<EntityStateAction> GetStatesToPersist()
    {
      bool foreignKeysEnabled = (Domain.Configuration.ForeignKeyMode & ForeignKeyMode.Reference) > 0;

      // Insert
      IEnumerable<EntityState> insertEntities = EntityStateRegistry.GetItems(PersistenceState.New).Where(entityState=>!entityState.IsRemoved);
      if (foreignKeysEnabled)
        foreach (var statePair in InsertInAccordanceWithForeignKeys(insertEntities))
          yield return statePair;
      else
        foreach (EntityState data in insertEntities) {
          yield return new EntityStateAction(data, PersistAction.Insert);
          data.GetDifferentialTuple().Merge();
        }

      // Update
      foreach (EntityState data in EntityStateRegistry.GetItems(PersistenceState.Modified)) {
        if (data.IsRemoved)
          continue;
        yield return new EntityStateAction(data, PersistAction.Update);
        data.GetDifferentialTuple().Merge();
      }

      // Delete
      foreach (EntityState data in EntityStateRegistry.GetItems(PersistenceState.Removed))
        yield return new EntityStateAction(data, PersistAction.Remove);
    }

    private static IEnumerable<EntityStateAction> InsertInAccordanceWithForeignKeys(
      IEnumerable<EntityState> entityStates)
    {
      // Topological sorting
      List<Triplet<EntityState, FieldInfo, Entity>> loopReferences;
      List<EntityState> sortedEntities;
      List<EntityState> unreferencedEntities;
      SortAndRemoveLoopEdges(entityStates, out sortedEntities, out unreferencedEntities, out loopReferences);

      // Insert 
      sortedEntities.Reverse();
      sortedEntities.AddRange(unreferencedEntities);

      foreach (EntityState data in sortedEntities)
        yield return new EntityStateAction(data, PersistAction.Insert);

      // Restore loop links
      foreach (var restoreData in loopReferences) {
        Persistent.GetAccessor<Entity>(restoreData.Second).SetValue(restoreData.First.Entity, restoreData.Second, restoreData.Third);
        yield return new EntityStateAction(restoreData.First, PersistAction.Update);
      }

      // Merge
      foreach (EntityState data in sortedEntities)
        data.GetDifferentialTuple().Merge();
    }

    private static void SortAndRemoveLoopEdges(IEnumerable<EntityState> entityStates,
      out List<EntityState> sortResult, out List<EntityState> unreferencedData,
      out List<Triplet<EntityState, FieldInfo, Entity>> keysToRestore)
    {
      var sortData = new Dictionary<Key, Node<EntityState, AssociationInfo>>();
      unreferencedData = new List<EntityState>();
      foreach (EntityState data in entityStates) {
        if (data.Type.GetTargetAssociations().Count==0 && data.Type.GetOwnerAssociations().Count==0)
          unreferencedData.Add(data);
        else
          sortData.Add(data.Key, new Node<EntityState, AssociationInfo>(data));
      }

      // Add connections
      foreach (var data in sortData) {
        EntityState processingEntityState = data.Value.Item;
        foreach (var association in processingEntityState.Type.GetOwnerAssociations().Where(associationInfo => associationInfo.OwnerField.IsEntity)) {
          Key foreignKey = processingEntityState.Entity.GetReferenceKey(association.OwnerField);
          Node<EntityState, AssociationInfo> destination;
          if (foreignKey!=null && !foreignKey.Equals(data.Value.Item.Key) && sortData.TryGetValue(foreignKey, out destination))
            data.Value.AddConnection(destination, true, association);
        }
      }

      // Sort
      List<NodeConnection<EntityState, AssociationInfo>> removedEdges;
      sortResult = TopologicalSorter.Sort(sortData.Values, out removedEdges);

      // Remove loop links
      keysToRestore = new List<Triplet<EntityState, FieldInfo, Entity>>();
      foreach (var edge in removedEdges) {
        AssociationInfo associationInfo = edge.ConnectionItem;
        keysToRestore.Add(new Triplet<EntityState, FieldInfo, Entity>(edge.Source.Item, associationInfo.OwnerField, edge.Destination.Item.Entity));
        Persistent.GetAccessor<Entity>(associationInfo.OwnerField).SetValue(edge.Source.Item.Entity, associationInfo.OwnerField, null);
      }
    }

    #region IResource methods

    /// <inheritdoc/>
    void IResource.AddConsumer(object consumer)
    {
      consumers.Add(consumer);
    }

    /// <inheritdoc/>
    void IResource.RemoveConsumer(object consumer)
    {
      consumers.Remove(consumer);
      if (!(this as IResource).HasConsumers)
        Dispose();
    }

    /// <inheritdoc/>
    bool IResource.HasConsumers
    {
      get { return consumers.Count > 0; }
    }

    #endregion

    #region IContext<...> methods

    /// <summary>
    /// Gets the current active <see cref="Session"/> instance.
    /// </summary>
    public static Session Current {
      [DebuggerStepThrough]
      get { return SessionScope.CurrentSession; }
    }

    /// <summary>
    /// Gets the current <see cref="Session"/>, 
    /// or throws <see cref="InvalidOperationException"/>, 
    /// if active <see cref="Session"/> is not found.
    /// </summary>
    /// <returns>Current session.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Session.Current"/> <see cref="Session"/> is <see langword="null" />.</exception>
    public static Session Demand()
    {
      var currentSession = Current;
      if (currentSession==null)
        throw new InvalidOperationException(Strings.ExActiveSessionIsRequiredForThisOperation);
      return currentSession;
    }

    /// <inheritdoc/>
    public SessionScope Activate()
    {
      if (IsActive)
        return null;
      return new SessionScope(this);
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public bool IsActive {
      get { return Current==this; }
    }

    #endregion

    #region EnsureXxx methods

    /// <summary>
    /// Ensures the object is not disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object is already disposed.</exception>
    protected void EnsureNotDisposed()
    {
      if (isDisposed)
        throw new ObjectDisposedException(Strings.ExSessionIsAlreadyDisposed);
    }

    #endregion

    #region OpenSession methods

    /// <summary>
    /// Opens and activates new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>
    /// New <see cref="SessionConsumptionScope"/> object.
    /// </returns>
    /// <remarks>
    /// Session will be closed when returned <see cref="SessionConsumptionScope"/> is disposed.
    /// </remarks>
    /// <sample><code>
    /// using (Session.Open(domain)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// {
    /// </code></sample>
    /// <seealso cref="Session"/>
    public static SessionConsumptionScope Open(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return Open(domain, domain.Configuration.Sessions.Default);
    }

    /// <summary>
    /// Opens and activates new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="type">The type of session.</param>
    /// <returns>
    /// New <see cref="SessionConsumptionScope"/> object.
    /// </returns>
    /// <remarks>
    /// Session will be closed when returned <see cref="SessionConsumptionScope"/> is disposed.
    /// </remarks>
    /// <sample><code>
    /// using (Session.Open(domain, sessionType)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// {
    /// </code></sample>
    public static SessionConsumptionScope Open(Domain domain, SessionType type)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");

      switch (type) {
      case SessionType.User:
        return Open(domain, domain.Configuration.Sessions.Default);
      case SessionType.System:
        return Open(domain, domain.Configuration.Sessions.System);
      case SessionType.KeyGenerator:
        return Open(domain, domain.Configuration.Sessions.KeyGenerator);
      case SessionType.Service:
        return Open(domain, domain.Configuration.Sessions.Service);
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }

    /// <summary>
    /// Opens and activates new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="SessionConsumptionScope"/> object.
    /// </returns>
    /// <remarks>
    /// Session will be closed when returned <see cref="SessionConsumptionScope"/> is disposed.
    /// </remarks>
    /// <sample><code>
    /// using (Session.Open(domain, sessionConfiguration)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// {
    /// </code></sample>
    /// <seealso cref="Session"/>
    public static SessionConsumptionScope Open(Domain domain, SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      return domain.OpenSession(configuration);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Name;
    }


    // Constructors

    internal Session(Domain domain, SessionConfiguration configuration)
      : base(domain)
    {
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      // Both Domain and Configuration are valid references here;
      // Configuration is already locked
      Configuration = configuration;
      Name = configuration.Name;
      // Handlers
      Handlers = domain.Handlers;
      Handler = Handlers.HandlerFactory.CreateHandler<SessionHandler>();
      Handler.Session = this;
      Handler.DefaultIsolationLevel = configuration.DefaultIsolationLevel;
      Handler.Initialize();
      // Caches, registry
      switch (configuration.CacheType) {
      case SessionCacheType.Infinite:
        EntityStateCache = new InfiniteCache<Key, EntityState>(configuration.CacheSize, i => i.Key);
        break;
      default:
        EntityStateCache = new LruCache<Key, EntityState>(configuration.CacheSize, i => i.Key,
          new WeakCache<Key, EntityState>(false, i => i.Key));
        break;
      }
      EntityStateRegistry = new EntityStateRegistry(this);
      // Etc...
      AtomicityContext = new AtomicityContext(this, AtomicityContextOptions.Undoable);
      CoreServices = new CoreServiceAccessor(this);
      PairSyncManager = new SyncManager(this);
      RemovalProcessor = new RemovalProcessor(this);
      EntityEvents = new EntityEventManager();
    }

    #region Dispose pattern

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~Session()
    {
      Dispose(false);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (isDisposed)
        return;
      lock (_lock) {
        if (isDisposed)
          return;
        try {
          if (IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Disposing.", this);
          NotifyDisposing();
          Handler.DisposeSafely();
        }
        finally {
          isDisposed = true;
        }
      }
    }

    #endregion
  }
}
