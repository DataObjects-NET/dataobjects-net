// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IoC;
using Xtensive.Integrity.Atomicity;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Operations;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Providers;
using Xtensive.Storage.ReferentialIntegrity;
using Xtensive.Storage.Resources;
using EnumerationContext=Xtensive.Storage.Rse.Providers.EnumerationContext;

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
  /// Session can be open and activated by <see cref="Open(Xtensive.Storage.Domain)">Session.Open(domain)</see> method. 
  /// Existing session can be activated by <see cref="Activate"/> method.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code source="..\..\Xtensive.Storage\Xtensive.Storage.Manual\DomainAndSessionSample.cs" region="Session sample"></code>
  /// </example>
  /// <seealso cref="Domain"/>
  /// <seealso cref="SessionBound" />
  [DebuggerDisplay("FullName = {FullName}")]
  public sealed partial class Session : DomainBound,
    IIdentified<long>,
    IContext<SessionScope>, 
    IDisposable,
    IHasExtensions
  {
    private const string IdentifierFormat = "#{0}";
    private const string FullNameFormat   = "{0}, #{1}";

    private static Func<Session> resolver;
    private static long lastUsedIdentifier;

    private const int EntityChangeRegistrySizeLimit = 250; // TODO: -> SessionConfiguration
    private ExtensionCollection extensions;

    private readonly object _lock = new object();
    private readonly Pinner pinner = new Pinner();
    private readonly bool persistRequiresTopologicalSort;
    
    private SessionServiceLocator serviceLocator;
    private SessionScope sessionScope;

    private volatile bool isDisposed;

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
    /// Gets the identifier of the session.
    /// Identifiers are unique in <see cref="AppDomain"/> scope.
    /// </summary>
    public long Identifier { get; private set; }

    /// <inheritdoc/>
    object IIdentified.Identifier { get { return Identifier; } }

    /// <summary>
    /// Gets the full name of the <see cref="Session"/>.
    /// Full name includes both <see cref="Name"/> and <see cref="Identifier"/>.
    /// </summary>
    public string FullName {
      get {
        string name = Name;
        if (name.IsNullOrEmpty())
          return string.Format(IdentifierFormat, Identifier);
        else
          return string.Format(FullNameFormat, name, Identifier);
      }
    }

    /// <summary>
    /// Indicates whether debug event logging is enabled.
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </summary>
    public bool IsDebugEventLoggingEnabled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Persist"/> method is running.
    /// </summary>
    public bool IsPersisting { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether only a system logic is enabled.
    /// </summary>
    public bool IsSystemLogicOnly { get; internal set; }

    /// <summary>
    /// Gets or sets the <see cref="Current"/> session resolver to use
    /// when there is no active <see cref="Session"/>.
    /// </summary>
    /// <remarks>
    /// The setter of this property can be invoked just once per application lifetime; 
    /// assigned resolver can not be changed.
    /// </remarks>
    /// <exception cref="NotSupportedException">Resolver is already assigned.</exception>
    public static Func<Session> Resolver
    {
      [DebuggerStepThrough]
      get {
        return resolver;
      }
      set
      {
        resolver = value;
        if (value==null)
          Rse.Compilation.CompilationContext.Resolver = null;
        else
          Rse.Compilation.CompilationContext.Resolver = () => {
            var session = resolver.Invoke();
            return session==null ? null : session.CompilationContext;
          };
      }
    }

    /// <summary>
    /// Gets the session service provider.
    /// </summary>
    public SessionServiceLocator Services {
      get {
        if (serviceLocator==null)
          serviceLocator = new SessionServiceLocator(this);
        var container = new ServiceContainer();
        serviceLocator.SetLocatorProvider(() => new ServiceLocatorAdapter(container));
        return serviceLocator;
      }
    }

    #region Private / internal members

    internal SessionHandler Handler { get; set; }

    internal HandlerAccessor Handlers { get; private set; }

    internal CoreServiceAccessor CoreServices { get; private set; }

    internal SyncManager PairSyncManager { get; private set; }

    internal RemovalProcessor RemovalProcessor { get; private set; }

    internal bool IsDelayedQueryRunning { get; private set; }

    internal CompilationContext CompilationContext { get { return Handlers.DomainHandler.CompilationContext; } }

    internal OperationContext CurrentOperationContext { get; set; }

    private void EnsureNotDisposed()
    {
      if (isDisposed)
        throw new ObjectDisposedException(Strings.ExSessionIsAlreadyDisposed);
    }

    internal EnumerationContext CreateEnumerationContext()
    {
      Persist(PersistReason.Query);
      ExecuteAllDelayedQueries(true);
      EnsureTransactionIsStarted();
      return Handler.CreateEnumerationContext();
    }

    #endregion

    #region IContext<...> methods

    /// <summary>
    /// Gets the current active <see cref="Session"/> instance.
    /// </summary>
    public static Session Current
    {
      [DebuggerStepThrough]
      get
      {
        return 
          SessionScope.CurrentSession ?? 
          (resolver==null ? null : resolver.Invoke());
      }
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
      if (SessionScope.CurrentSession==this)
        return null;
      return new SessionScope(this);
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    /// <inheritdoc/>
    public bool IsActive
    {
      get { return Current==this; }
    }

    #endregion

    #region IHasExtensions Members

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

    /// <inheritdoc/>
    public override string ToString()
    {
      return FullName;
    }

    /// <summary>
    /// Pins the specified <see cref="IEntity"/>.
    /// Pinned entity is prevented from being persisted,
    /// when <see cref="Persist"/> is called or query is executed.
    /// If persist is to be performed due to starting a nested transaction or committing a transaction,
    /// any pinned entity will lead to failure.
    /// If <paramref name="target"/> is not present in the database,
    /// all entities that reference <paramref name="target"/> are also pinned automatically.
    /// </summary>
    /// <param name="target">The entity to pin.</param>
    /// <returns>An entity pinning scope if <paramref name="target"/> was not previously pinned,
    /// otherwise <see langword="null"/>.</returns>
    public IDisposable Pin(IEntity target)
    {
      EnsureNotDisposed();
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      var targetEntity = (Entity) target;
      targetEntity.EnsureNotRemoved();
      return pinner.RegisterRoot(targetEntity.State);
    }


    // Constructors

    internal Session(Domain domain, SessionConfiguration configuration, bool activate)
      : base(domain)
    {
      IsDebugEventLoggingEnabled = 
        Log.IsLogged(LogEventTypes.Debug); // Just to cache this value

      // Both Domain and Configuration are valid references here;
      // Configuration is already locked
      Configuration = configuration;
      Name = configuration.Name;
      Identifier = Interlocked.Increment(ref lastUsedIdentifier);

      // Handlers
      Handlers = domain.Handlers;
      Handler = Handlers.HandlerFactory.CreateHandler<SessionHandler>();
      Handler.Session = this;
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
      EntityChangeRegistry = new EntityChangeRegistry();
      // Etc...
      AtomicityContext = new AtomicityContext(this, AtomicityContextOptions.Undoable);
      CoreServices = new CoreServiceAccessor(this);
      PairSyncManager = new SyncManager(this);
      RemovalProcessor = new RemovalProcessor(this);
      EntityEventBroker = new EntityEventBroker();
      if (activate)
        sessionScope = new SessionScope(this);
      CurrentOperationContext = OperationContext.Default;
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

    private void Dispose(bool disposing)
    {
      if (isDisposed)
        return;
      lock (_lock) {
        if (isDisposed)
          return;
        try {
          if (IsDebugEventLoggingEnabled)
            Log.Debug(Strings.LogSessionXDisposing, this);
          NotifyDisposing();
          Handler.DisposeSafely();
          sessionScope.DisposeSafely();
          sessionScope = null;
        }
        finally {
          isDisposed = true;
        }
      }
    }

    #endregion
  }
}
