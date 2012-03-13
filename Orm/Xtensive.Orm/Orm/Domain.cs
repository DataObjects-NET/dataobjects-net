// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Threading;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage access point.
  /// </summary>
  /// <sample>
  /// <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\DomainAndSession\DomainAndSessionSample.cs" region="Domain sample"></code>
  /// </sample>
  public sealed class Domain :
    IDisposableContainer,
    IHasExtensions
  {
    private readonly object _lock = new object();

    private volatile ExtensionCollection extensions;

    private DisposingState disposingState;
    
    /// <summary>
    /// Occurs when new <see cref="Session"/> is open and activated.
    /// </summary>
    /// <seealso cref="OpenSession()"/>
    public event EventHandler<SessionEventArgs> SessionOpen;

    /// <summary>
    /// Occurs when <see cref="Domain"/> is about to be disposed.
    /// </summary>
    public event EventHandler Disposing;

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
      return Session.Demand().Domain;
    }    
    
    /// <summary>
    /// Gets the domain configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }
    
    /// <summary>
    /// Gets the dictionary providing cached type information.
    /// </summary>
    internal IntDictionary<TypeLevelCache> TypeLevelCaches { get; private set; }

    /// <summary>
    /// Gets the <see cref="RecordSetReader"/> instance.
    /// </summary>
    internal RecordSetReader RecordSetReader { get; private set; }

    /// <summary>
    /// Gets the prefetch action map.
    /// </summary>
    internal Dictionary<Model.TypeInfo, Action<SessionHandler, IEnumerable<Key>>> PrefetchActionMap { get; private set; }

    /// <summary>
    /// Gets the disposing state of the domain.
    /// </summary>
    DisposingState IDisposableContainer.DisposingState
    {
      get { return disposingState; }
    }

    /// <summary>
    /// Gets the domain model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    /// <summary>
    /// Gets the information about provider's capabilities.
    /// </summary>
    public ProviderInfo StorageProviderInfo { get { return Handlers.ProviderInfo; } }

    /// <summary>
    /// Gets the domain-level service container.
    /// </summary>
    public IServiceContainer Services { get; internal set; }

    #region Private / internal members

    /// <summary>
    /// Gets the storage model that was build from <see cref="Model"/>.
    /// </summary>
    internal StorageModel StorageModel { get; set; }

    /// <summary>
    /// Gets the domain-level temporary data.
    /// </summary>
    internal GlobalTemporaryData TemporaryData { get; private set; }

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    internal HandlerAccessor Handlers { get; private set; }

    internal ThreadSafeIntDictionary<GenericKeyTypeInfo> GenericKeyTypes { get; private set; }

    internal KeyGeneratorRegistry KeyGenerators { get; private set; }

    internal ThreadSafeDictionary<object, object> Cache { get; private set; }
    internal ICache<object, Pair<object, TranslatedQuery>> QueryCache { get; private set; }
    internal ICache<Key, Key> KeyCache { get; private set; }

    private void NotifySessionOpen(Session session)
    {
      if (SessionOpen!=null)
        SessionOpen(this, new SessionEventArgs(session));
    }

    private void NotifyDisposing()
    {
      if (Disposing!=null)
        Disposing(this, EventArgs.Empty);
    }

    #endregion

    #region OpenSession method

    /// <summary>
    /// Opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = Domain.OpenSession()) {
    /// // work with persistent objects here.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public Session OpenSession()
    {
      var configuration = Configuration.Sessions.Default;
      return OpenSession(configuration);
    }

    /// <summary>
    /// Opens new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.OpenSession(sessionType)) {
    /// // work with persistent objects here.
    /// }
    /// </code></sample>
    public Session OpenSession(SessionType type)
    {
      switch (type) {
        case SessionType.User:
          return OpenSession(Configuration.Sessions.Default);
        case SessionType.System:
          return OpenSession(Configuration.Sessions.System);
        case SessionType.KeyGenerator:
          return OpenSession(Configuration.Sessions.KeyGenerator);
        case SessionType.Service:
          return OpenSession(Configuration.Sessions.Service);
        default:
          throw new ArgumentOutOfRangeException("type");
      }
    }

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.OpenSession(configuration)) {
    /// // work with persistent objects here
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public Session OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      return OpenSession(configuration, (configuration.Options & SessionOptions.AutoActivation) == SessionOptions.AutoActivation);
    }

    internal Session OpenSession(SessionConfiguration configuration, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      configuration.Lock(true);

      Log.Debug(Strings.LogOpeningSessionX, configuration);

      var session = new Session(this, configuration, activate);
      NotifySessionOpen(session);
      return session;
    }

    #endregion

    #region IHasExtensions members

    /// <inheritdoc/>
    public IExtensionCollection Extensions {
      get {
        if (extensions==null) lock (_lock) if (extensions==null)
          extensions = new ExtensionCollection();
        return extensions;
      }
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
      Configuration = configuration;
      TypeLevelCaches = new IntDictionary<TypeLevelCache>();
      Handlers = new HandlerAccessor(this);
      GenericKeyTypes = ThreadSafeIntDictionary<GenericKeyTypeInfo>.Create(new object());
      RecordSetReader = new RecordSetReader(this);
      KeyGenerators = new KeyGeneratorRegistry();
      Cache = ThreadSafeDictionary<object, object>.Create(new object());
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      QueryCache = new LruCache<object, Pair<object, TranslatedQuery>>(Configuration.QueryCacheSize, k => k.First);
      TemporaryData = new GlobalTemporaryData();
      PrefetchActionMap = new Dictionary<TypeInfo, Action<SessionHandler, IEnumerable<Key>>>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (disposingState==DisposingState.None) lock (_lock) if (disposingState==DisposingState.None) {
        disposingState = DisposingState.Disposing;
        try {
          Log.Debug(Strings.LogDomainIsDisposing);
          NotifyDisposing();
          Services.DisposeSafely();
        }
        finally {
          disposingState = DisposingState.Disposed;
        }
      }
    }
  }
}
