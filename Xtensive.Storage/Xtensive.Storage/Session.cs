// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Atomicity;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Providers;
using Xtensive.Storage.ReferentialIntegrity;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Session implementation.
  /// </summary>
  public partial class Session : DomainBound,
    IContext<SessionScope>,
    IResource
  {
    private bool isPersisting;
    private volatile bool isDisposed;
    private readonly Set<object> consumers = new Set<object>();
    private readonly object _lock = new object();
    private ServiceProvider serviceProvider;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to <see cref="Dispose"/>.
    /// </summary>
    public EventHandler OnDisposing;

    /// <summary>
    /// Occurs when <see cref="Session"/> is about to <see cref="Persist"/>.
    /// </summary>
    public EventHandler OnPersisting;

    /// <summary>
    /// Occurs when <see cref="Session"/> persisted.
    /// </summary>
    public EventHandler OnPersist;

    /// <summary>
    /// Occurs when <see cref="Entity"/> created.
    /// </summary>
    public EventHandler<EntityEventArgs> OnCreateEntity;

    /// <summary>
    /// Occurs when <see cref="Entity"/> is about to remove.
    /// </summary>
    public EventHandler<EntityEventArgs> OnRemovingEntity;

    /// <summary>
    /// Occurs when <see cref="Entity"/> removed.
    /// </summary>
    public EventHandler<EntityEventArgs> OnRemoveEntity;

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

    internal SessionHandler Handler { get; private set; }
    
    internal HandlerAccessor Handlers { get; private set; }

    internal CoreServiceAccessor CoreServices { get; private set; }

    internal SyncManager PairSyncManager { get; private set; }

    internal ReferenceManager ReferenceManager { get; private set; }

    private void NotifyDisposing()
    {
      if (OnDisposing!=null)
        OnDisposing(this, EventArgs.Empty);
    }

    private void NotifyPersisting()
    {
      if (OnPersisting!=null)
        OnPersisting(this, EventArgs.Empty);
    }

    private void NotifyPersist()
    {
      if (OnPersist!=null)
        OnPersist(this, EventArgs.Empty);
    }

    internal void NotifyCreateEntity(Entity entity)
    {
      if (OnCreateEntity!=null)
        OnCreateEntity(this, new EntityEventArgs(entity));
    }

    internal void NotifyRemovingEntity(Entity entity)
    {
      if (OnRemovingEntity!=null)
        OnRemovingEntity(this, new EntityEventArgs(entity));
    }

    internal void NotifyRemoveEntity(Entity entity)
    {
      if (OnRemoveEntity!=null)
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
    /// This method should be called to ensure that all delayed
    /// updates are flushed to the storage. 
    /// Note, that this method is called automatically when it's necessary,
    /// e.g. before beginning\committing\rolling back a transaction, performing a
    /// query and so further. So generally you should not worry
    /// about calling this method.
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

        Handler.Persist();

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
        throw Exceptions.ContextRequired<Session,SessionScope>();
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
      ReferenceManager = new ReferenceManager(this);
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
