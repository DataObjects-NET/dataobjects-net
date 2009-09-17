// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using Microsoft.Practices.Unity;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Threading;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse.Providers.Executable;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage
{
  /// <summary>
  /// An access point to a single storage.
  /// </summary>
  /// <sample>
  /// <code source="..\..\Xtensive.Storage\Xtensive.Storage.Manual\DomainAndSessionSample.cs" region="Domain sample"></code>
  /// </sample>
  public sealed class Domain : CriticalFinalizerObject,
    IDisposableContainer,
    IHasExtensions
  {
    private ExtensionCollection extensions;

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
      var session = Session.Demand();
      return session.Domain;
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

    /// <summary>
    /// Gets the collection of extension modules.
    /// </summary>
    public ModuleProvider Modules { get; private set; }

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    #region Private \ internal members

    internal HandlerAccessor Handlers { get; private set; }

    internal Registry<GeneratorInfo, KeyGenerator> KeyGenerators { get; private set; }

    internal ICache<Key, Key> KeyCache { get; private set; }

    internal ICache<object, Pair<object, TranslatedQuery>> QueryCache { get; private set; }

    internal readonly ThreadSafeIntDictionary<GenericKeyTypeInfo> genericKeyTypes = 
      ThreadSafeIntDictionary<GenericKeyTypeInfo>.Create(new object());

    internal readonly ThreadSafeDictionary<Model.FieldInfo, EntitySetTypeState> entitySetTypeStateCache =
      ThreadSafeDictionary<Model.FieldInfo, EntitySetTypeState>.Create(new object());

    private void OnSessionOpen(Session session)
    {
      if (SessionOpen!=null)
        SessionOpen(this, new SessionEventArgs(session));
    }

    private void OnDisposing()
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
        Log.Debug("Opening session '{0}'", configuration);

      var session = new Session(this, configuration);
      if (activate)
        session.Activate();
      OnSessionOpen(session);
      return session;
    }

    #endregion

    #region IHasExtensions members

    /// <inheritdoc/>
    public IExtensionCollection Extensions {
      get {
        if (extensions != null)
          return extensions;

        lock (this) {
          if (extensions == null)
            extensions = new ExtensionCollection();
        }

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

    internal Domain(DomainConfiguration configuration, ModuleProvider modules)
    {
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      RecordSetReader = new RecordSetReader(this);
      KeyGenerators = new Registry<GeneratorInfo, KeyGenerator>();
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      QueryCache = new LruCache<object, Pair<object, TranslatedQuery>>(
        Configuration.QueryCacheSize, k => k.First);
      TemporaryData = new GlobalTemporaryData();
      ServiceContainer = new UnityContainer();
      ServiceContainer.AddExtension(new SingletonExtension());
      Modules = modules;
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
                Log.Debug("Domain disposing {0}.", isDisposing ? "explicitly" : "by calling finalizer.");
              OnDisposing();
              KeyGenerators.DisposeSafely();
            }
            finally {
              DisposingState = DisposingState.Disposed;
            }
          }
    }
  }
}
