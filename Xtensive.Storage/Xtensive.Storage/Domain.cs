// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.IoC;
using Xtensive.Core.Threading;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage
{
  /// <summary>
  /// An access point to a single storage.
  /// </summary>
  /// <sample>
  /// <code lang="cs" source="..\Xtensive.Storage\Xtensive.Storage.Manual\DomainAndSession\DomainAndSessionSample.cs" region="Domain sample"></code>
  /// </sample>
  public sealed class Domain :
    IDisposableContainer,
    IHasExtensions
  {
    private readonly object _lock = new object();
    private volatile ExtensionCollection extensions;
    
    /// <summary>
    /// Occurs when new <see cref="Session"/> is open and activated.
    /// </summary>
    /// <seealso cref="Session.Open(Xtensive.Storage.Domain)"/>
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
    /// Indicates whether debug event logging is enabled.
    /// </summary>
    /// <remarks>
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </remarks>
    public bool IsDebugEventLoggingEnabled { get; private set; }

    /// <summary>
    /// Gets the information about provider's capabilities.
    /// </summary>
    public ProviderInfo StorageProviderInfo { get { return Handler.ProviderInfo; } }

    /// <summary>
    /// Gets the domain-level service container.
    /// </summary>
    public IServiceContainer Services { get; internal set; }

    #region Private / internal members

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    internal HandlerAccessor Handlers { get; private set; }

    internal ThreadSafeIntDictionary<GenericKeyTypeInfo> GenericKeyTypes { get; private set; }

    internal Registry<KeyProviderInfo, KeyGenerator> KeyGenerators { get; private set; }
    internal Registry<KeyProviderInfo, KeyGenerator> LocalKeyGenerators { get; private set; }

    internal ThreadSafeDictionary<object, object> Cache { get; private set; }
    internal ICache<Key, Key> KeyCache { get; private set; }
    internal ICache<object, Pair<object, TranslatedQuery>> QueryCache { get; private set; }
    
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

    internal Session OpenSession(SessionConfiguration configuration, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      configuration.Lock(true);

      if (IsDebugEventLoggingEnabled)
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
      IsDebugEventLoggingEnabled = 
        Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      GenericKeyTypes = ThreadSafeIntDictionary<GenericKeyTypeInfo>.Create(new object());
      RecordSetReader = new RecordSetReader(this);
      KeyGenerators = new Registry<KeyProviderInfo, KeyGenerator>();
      LocalKeyGenerators = new Registry<KeyProviderInfo, KeyGenerator>();
      Cache = ThreadSafeDictionary<object, object>.Create(new object());
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      QueryCache = new LruCache<object, Pair<object, TranslatedQuery>>(
        Configuration.QueryCacheSize, k => k.First);
      TemporaryData = new GlobalTemporaryData();
      PrefetchActionMap = new Dictionary<Model.TypeInfo, Action<SessionHandler, IEnumerable<Key>>>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (DisposingState==DisposingState.None) lock (_lock) if (DisposingState==DisposingState.None) {
        DisposingState = DisposingState.Disposing;
        try {
          if (IsDebugEventLoggingEnabled)
            Log.Debug(Strings.LogDomainIsDisposing);
          NotifyDisposing();
          Services.DisposeSafely();
          KeyGenerators.DisposeSafely();
          LocalKeyGenerators.DisposeSafely();
        }
        finally {
          DisposingState = DisposingState.Disposed;
        }
      }
    }
  }
}
