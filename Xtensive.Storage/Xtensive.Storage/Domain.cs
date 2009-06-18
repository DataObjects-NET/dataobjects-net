// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using Microsoft.Practices.Unity;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage
{
  /// <summary>
  /// An access point to a single storage.
  /// </summary>
  /// <sample>
  /// <code source="..\Xtensive.Storage.Manual\DomainAndSessionSample.cs" region="Domain sample"></code>
  /// </sample>
  public sealed class Domain : CriticalFinalizerObject,
    IDisposableContainer
  {
    /// <summary>
    /// Occurs on <see cref="Session"/> opening.
    /// </summary>
    public EventHandler<SessionEventArgs> OnOpenSession;

    /// <summary>
    /// Occurs when <see cref="Domain"/> is about to <see cref="Dispose"/>.
    /// </summary>
    public EventHandler OnDisposing;

    /// <summary>
    /// Gets the <see cref="Domain"/> of the current <see cref="Session"/>. 
    /// </summary>
    /// <seealso cref="Session.Current"/>
    /// <seealso cref="Demand"/>
    public static Domain Current {
      get {
        var session = Session.Current;
        return session!=null ? session.Domain : null;
      }
    }    

    /// <summary>
    /// Gets the <see cref="Domain"/> of the current <see cref="Session"/>, or throws <see cref="InvalidOperationException"/>, 
    /// if active <see cref="Session"/> is not found.
    /// </summary>
    /// <returns>Current domain.</returns>
    /// <exception cref="InvalidOperationException">Current session is <see langword="null" />.</exception>
    /// <seealso cref="Session.Demand">Session.Current property</seealso>
    public static Domain Demand()
    {
      var session = Session.Demand();
      return session.Domain;
    }    
    
    /// <summary>
    /// Gets the domain configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }
    
    /// <summary>
    /// Gets the <see cref="RecordSetParser"/> instance.
    /// </summary>
    internal RecordSetParser RecordSetParser { get; private set; }
    
    /// <summary>
    /// Gets the disposing state of the domain.
    /// </summary>
    public DisposingState DisposingState { get; private set; }

    /// <summary>
    /// Gets the domain model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    /// <summary>
    /// Gets the storage schema.
    /// </summary>
    public StorageInfo Schema { get; set; }

    /// <summary>
    /// Gets the extracted storage schema.
    /// </summary>
    public StorageInfo ExtractedSchema { get; set; }

    /// <summary>
    /// Gets the handler factory.
    /// </summary>
    public HandlerFactory HandlerFactory  { 
      [DebuggerStepThrough]
      get { return Handlers.HandlerFactory; }
    }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { 
      [DebuggerStepThrough]
      get { return Handlers.NameBuilder; }
    }

    /// <summary>
    /// Gets the domain-level temporary data.
    /// </summary>
    public GlobalTemporaryData TemporaryData { get; private set; }

    /// <summary>
    /// Gets the service container.
    /// </summary>
    public UnityContainer ServiceContainer { get; private set; }

    /// <summary>
    /// Indicates whether debug event logging is enabled.
    /// </summary>
    /// <remarks>
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </remarks>
    public bool IsDebugEventLoggingEnabled { get; private set; }

    /// <summary>
    /// Gets the information about provider's capabilities.
    /// </summary>
    public ProviderInfo StorageProviderInfo
    {
      get { return Handler.ProviderInfo;}
    }

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    #region Private \ internal members

    internal HandlerAccessor Handlers { get; private set; }

    internal Registry<GeneratorInfo, KeyGenerator> KeyGenerators { get; private set; }

    internal ICache<Key, Key> KeyCache { get; private set; }

    internal ICache<MethodInfo, Pair<MethodInfo, TranslatedQuery>> QueryCache { get; private set; }

    internal Dictionary<AssociationInfo, ActionSet> PairSyncActions { get; private set; }

    private void NotifyOpenSession(Session session)
    {
      if (OnOpenSession!=null)
        OnOpenSession(this, new SessionEventArgs(session));
    }

    private void NotifyDisposing()
    {
      if (OnDisposing!=null)
        OnDisposing(this, EventArgs.Empty);
    }

    #endregion

    #region OpenSession methods

    /// <summary>
    /// Opens and activates the <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    /// <remarks>
    /// Session will be closed when returned <see cref="SessionConsumptionScope"/> is disposed.
    /// </remarks>
    /// <sample><code>
    /// using (domain.OpenSession()) {
    ///   // work with persistent objects here
    /// {
    /// </code></sample>
    /// <seealso cref="Session"/>
    public SessionConsumptionScope OpenSession()
    {
      return OpenSession(Configuration.Sessions.Default);
    }

    /// <summary>
    /// Opens and activates the <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    /// <remarks>
    /// Session will be closed when returned <see cref="SessionConsumptionScope"/> is disposed.
    /// </remarks>
    /// <sample><code>
    /// using (domain.OpenSession(sessionType)) {
    ///   // work with persistent objects here
    /// {
    /// </code></sample>
    public SessionConsumptionScope OpenSession(SessionType type)
    {
      switch (type) {
      case SessionType.User:
        return OpenSession(Configuration.Sessions.Default);
      case SessionType.System:
        return OpenSession(Configuration.Sessions.System);
      case SessionType.Generator:
        return OpenSession(Configuration.Sessions.Generator);
      case SessionType.Service:
        return OpenSession(Configuration.Sessions.Service);
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }

    /// <summary>
    /// Opens and activates the <see cref="Session"/> with specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <remarks>
    /// Session will be closed when returned <see cref="SessionConsumptionScope"/> is disposed.
    /// </remarks>
    /// <sample><code>
    /// using (domain.OpenSession(sessionConfiguration)) {
    ///   // work with persistent objects here
    /// {
    /// </code></sample>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    /// <seealso cref="Session"/>
    public SessionConsumptionScope OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      configuration.Lock(true);

      if (IsDebugEventLoggingEnabled)
        Log.Debug("Opening session '{0}'", configuration);

      var session = new Session(this, configuration);
      NotifyOpenSession(session);
      return new SessionConsumptionScope(session);
    }

    #endregion

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to the specified <see cref="DomainConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The configuration of domain to build.</param>
    /// <returns>Newly built <see cref="Domain"/>.</returns>
    public static Domain Build(DomainConfiguration configuration)
    {
      return UpgradingDomainBuilder.Build(configuration);
    }


    // Constructors

    internal Domain(DomainConfiguration configuration)
    {
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      RecordSetParser = new RecordSetParser(this);
      KeyGenerators = new Registry<GeneratorInfo, KeyGenerator>();
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      QueryCache = new LruCache<MethodInfo, Pair<MethodInfo, TranslatedQuery>>(
        Configuration.QueryCacheSize, k => k.First);
      PairSyncActions = new Dictionary<AssociationInfo, ActionSet>(1024);
      TemporaryData = new GlobalTemporaryData();
      ServiceContainer = new UnityContainer();
      ServiceContainer.AddExtension(new SingletonExtension());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Domain()
    {
      Dispose(false);
    }

    private void Dispose(bool isDisposing)
    {
      if (DisposingState==DisposingState.None)
        lock (this)
          if (DisposingState==DisposingState.None) {
            DisposingState = DisposingState.Disposing;
            try {
              if (IsDebugEventLoggingEnabled)
                LogTemplate<Log>.Debug("Domain disposing {0}.", isDisposing ? "explicitly" : "by calling finalizer.");
              NotifyDisposing();
              KeyGenerators.DisposeSafely();
            }
            finally {
              DisposingState = DisposingState.Disposed;
            }
          }
    }
  }
}
