// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.10

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Logging;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Operations;
using Xtensive.Orm.PairIntegrity;
using Xtensive.Orm.Providers;
using Xtensive.Orm.ReferentialIntegrity;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Validation;
using Xtensive.Sql;
using EnumerationContext = Xtensive.Orm.Rse.Providers.EnumerationContext;

namespace Xtensive.Orm
{
  /// <summary>
  /// <c>DataContext</c> analogue maintaining database connection
  /// and entity cache (identity map in the simplest case).
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each session maintains its own connection to the database and 
  /// caches a set of materialized persistent instates.
  /// </para>
  /// <para>
  /// <c>Session</c> implements <see cref="IContext"/> interface, that means each <c>Session</c>
  /// can be either active or not active in a particular thread (see <see cref="IsActive"/> property).
  /// Each thread can contain only one active session in each point of time, such session 
  /// can be a accessed via <see cref="Current">Session.Current</see> property 
  /// or <see cref="Demand">Session.Demand()</see> method.
  /// </para>
  /// <para>
  /// Sessions are opened (and, optionally, activated) by 
  /// <see cref="Domain.OpenSession()">Domain.OpenSession()</see> method. 
  /// Existing session can be activated by <see cref="Activate()"/> method.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\DomainAndSession\DomainAndSessionSample.cs" 
  /// region="Session sample"></code>
  /// </example>
  /// <seealso cref="Domain"/>
  /// <seealso cref="SessionBound" />
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public partial class Session : DomainBound,
    IVersionSetProvider,
    IContext<SessionScope>, 
    IHasExtensions,
    IDisposable
  {
    private const string IdentifierFormat = "#{0}";
    private const string FullNameFormat   = "{0}, #{1}";

    private static Func<Session> resolver;
    private static long lastUsedIdentifier;

    private DisposableSet disposableSet;
    private ExtensionCollection extensions;
    private StorageNode storageNode;

    private readonly bool allowSwitching;
    private readonly long identifier;
    private readonly Pinner pinner;

    private int? commandTimeout;
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
    /// Indicates whether debug event logging is enabled.
    /// Caches <see cref="BaseLog.IsLogged"/> method result for <see cref="LogLevel.Debug"/> event.
    /// </summary>
    internal bool IsDebugEventLoggingEnabled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="SaveChanges"/> method is running.
    /// </summary>
    internal bool IsPersisting { get; private set; }

    /// <summary>
    /// Gets a value indicating whether session is disconnected:
    /// session supports non-transactional entity states and does not support autosaving of changes.
    /// </summary>
    public bool IsDisconnected { 
      get
      {
        return Configuration.Supports(SessionOptions.NonTransactionalEntityStates) &&
          !Configuration.Supports(SessionOptions.AutoSaveChanges);
      }
    }

    /// <summary>
    /// Indicates whether lazy generation of keys is enabled.
    /// </summary>
    internal bool LazyKeyGenerationIsEnabled { get { return Configuration.Supports(SessionOptions.LazyKeyGeneration); } }
    
    /// <summary>
    /// Gets the operations registry of this <see cref="Session"/>.
    /// </summary>
    public OperationRegistry Operations { get; private set; }

    /// <summary>
    /// Gets or sets timeout for all <see cref="IDbCommand"/>s that
    /// are executed within this session.
    /// <seealso cref="IDbCommand.CommandTimeout"/>
    /// </summary>
    public int? CommandTimeout
    {
      get { return commandTimeout; }
      set
      {
        if (Handler != null)
          Handler.SetCommandTimeout(value);
        commandTimeout = value;
      }
    }

    /// <summary>
    /// Gets or sets <see cref="ConnectionInfo"/>
    /// for this <see cref="Session"/>.
    /// </summary>
    public ConnectionInfo ConnectionInfo
    {
      get
      {
        var directSqlService = Services.Demand<IDirectSqlService>();
        return directSqlService.ConnectionInfo;
      }
      set
      {
        var directSqlService = Services.Demand<IDirectSqlService>();
        directSqlService.ConnectionInfo = value;
      }
    }

    /// <summary>
    /// Gets current storage node identifier.
    /// </summary>
    public string StorageNodeId
    {
      get { return StorageNode.Id; }
    }

    /// <summary>
    /// Gets current storage node.
    /// </summary>
    public StorageNode StorageNode
    {
      get
      {
        if (storageNode==null)
          SetStorageNode(Handlers.StorageNodeRegistry.Get(WellKnown.DefaultNodeId));
        return storageNode;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Current"/> session resolver to use
    /// when there is no active <see cref="Session"/>.
    /// </summary>
    /// <remarks>
    /// The setter of this property can be invoked just once per application lifetime; 
    /// assigned resolver can not be changed.
    /// </remarks>
    /// <exception cref="NotSupportedException">Resolver is already assigned.</exception>
    public static Func<Session> Resolver {
      [DebuggerStepThrough]
      get {
        return resolver;
      }
      set {
        resolver = value;
      }
    }

    /// <summary>
    /// Gets the session service provider.
    /// </summary>
    public IServiceContainer Services { get; private set; }

    /// <summary>
    /// Gets the unique identifier of this session.
    /// </summary>
    public Guid Guid { get; private set; }

    #region Private / internal members

    internal SessionHandler Handler { get; set; }

    internal HandlerAccessor Handlers { get; private set; }

    internal SyncManager PairSyncManager { get; private set; }

    internal RemovalProcessor RemovalProcessor { get; private set; }

    internal CompilationService CompilationService { get { return Handlers.DomainHandler.CompilationService; } }

    internal void EnsureNotDisposed()
    {
      if (isDisposed)
        throw new ObjectDisposedException(Strings.ExSessionIsAlreadyDisposed);
    }

    internal EnumerationContext CreateEnumerationContext()
    {
      Persist(PersistReason.Query);
      ProcessUserDefinedDelayedQueries(true);
      return new Providers.EnumerationContext(this, GetEnumerationContextOptions());
    }

    private EnumerationContextOptions GetEnumerationContextOptions()
    {
      var options = EnumerationContextOptions.None;
      switch (Configuration.ReaderPreloading) {
        case ReaderPreloadingPolicy.Auto:
          bool marsSupported = Handlers.ProviderInfo.Supports(ProviderFeatures.MultipleActiveResultSets);
          if (!marsSupported)
            options |= EnumerationContextOptions.GreedyEnumerator;
          break;
        case ReaderPreloadingPolicy.Always:
          options |= EnumerationContextOptions.GreedyEnumerator;
          break;
        case ReaderPreloadingPolicy.Never:
          break;
        default:
          throw new ArgumentOutOfRangeException("Configuration.ReaderPreloading");
      }
      return options;
    }

    private IServiceContainer CreateSystemServices()
    {
      var registrations = new List<ServiceRegistration>{
        new ServiceRegistration(typeof (Session), this),
        new ServiceRegistration(typeof (SessionConfiguration), Configuration),
        new ServiceRegistration(typeof (SessionHandler), Handler),
      };
      Handler.AddSystemServices(registrations);
      return new ServiceContainer(registrations, Domain.Services);
    }

    private IServiceContainer CreateServices()
    {
      var userContainerType = Configuration.ServiceContainerType ?? typeof (ServiceContainer);
      var registrations = Domain.Configuration.Types.SessionServices.SelectMany(ServiceRegistration.CreateAll);
      var systemContainer = CreateSystemServices();
      var userContainer = ServiceContainer.Create(userContainerType, systemContainer);
      return new ServiceContainer(registrations, userContainer);
    }

    internal void SetStorageNode(StorageNode node)
    {
      if (storageNode!=null)
        throw new InvalidOperationException(Strings.ExStorageNodeIsAlreadySelected);
      Handler.SetStorageNode(node);
      storageNode = node;
    }

    public ExecutableProvider Compile(CompilableProvider provider)
    {
      return CompilationService.Compile(provider, CompilationService.CreateConfiguration(this));
    }

    public ExecutableProvider Compile(CompilableProvider provider, CompilerConfiguration configuration)
    {
      return CompilationService.Compile(provider, configuration);
    }

    #endregion

    #region IContext<...> methods

    /// <summary>
    /// Gets the current active <see cref="Session"/> instance.
    /// </summary>
    public static Session Current {
      [DebuggerStepThrough]
      get {
        return
          SessionScope.CurrentSession ?? (resolver==null ? null : resolver.Invoke());
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
    public bool IsActive { get { return Current==this; } }

    /// <inheritdoc/>
    public SessionScope Activate()
    {
      var currentSession = SessionScope.CurrentSession; // Not Session.Current -
      // to avoid possible comparison with Session provided by Session.Resolver.
      return currentSession==this ? null : new SessionScope(this);
    }


    /// <summary>
    /// Activates the session.
    /// See <see cref="SessionOptions.AllowSwitching"/> for more detailed explanation
    /// of purpose of this method.
    /// </summary>
    /// <param name="checkSwitching">If set to <see langword="true"/>, 
    /// <see cref="InvalidOperationException"/> is thrown if another session is active, and
    /// either this or active session does not have <see cref="SessionOptions.AllowSwitching"/> flag.</param>
    /// <returns>A disposable object reverting the action.</returns>
    /// <exception cref="InvalidOperationException">Session switching is detected.</exception>
    public SessionScope Activate(bool checkSwitching)
    {
      if (!checkSwitching)
        return Activate();
      var currentSession = SessionScope.CurrentSession; // Not Session.Current -
      // to avoid possible comparison with Session provided by Session.Resolver.
      if (currentSession==null)
        return new SessionScope(this);
      if (currentSession==this)
        return null;
      if (currentSession.Transaction==null || (allowSwitching && currentSession.allowSwitching))
        return new SessionScope(this);
      throw new InvalidOperationException(
        string.Format(Strings.ExAttemptToAutomaticallyActivateSessionXInsideSessionYIsBlocked, this, currentSession));
    }

    /// <summary>
    /// Deactivates <see cref="Current"/> session making it equal to <see langword="null" />.
    /// See <see cref="SessionOptions.AllowSwitching"/> for more detailed explanation
    /// of purpose of this method.
    /// </summary>
    /// <returns>A disposable object reverting the action.</returns>
    public static SessionScope Deactivate()
    {
      return SessionScope.CurrentSession==null
        ? null
        : new SessionScope(null);
    }

    /// <inheritdoc/>
    IDisposable IContext.Activate()
    {
      return Activate();
    }

    #endregion

    #region IHasExtensions members

    /// <inheritdoc/>
    public IExtensionCollection Extensions {
      get {
        if (extensions==null)
          extensions = new ExtensionCollection();
        return extensions;
      }
    }

    #endregion

    #region IVersionSetProvider members

    /// <inheritdoc/>
    VersionSet IVersionSetProvider.CreateVersionSet(IEnumerable<Key> keys)
    {
      using (Activate())
      using (var tx = OpenAutoTransaction()) {
        var entities = Query.Many<Entity>(keys);
        var result = new VersionSet();
        foreach (var entity in entities)
          if (entity!=null)
            result.Add(entity, false);
        tx.Complete();
        return result;
      }
    }

    #endregion

    /// <summary>
    /// Selects storage node identifier by <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">Node identifier.</param>
    public void SelectStorageNode([NotNull] string nodeId)
    {
      ArgumentValidator.EnsureArgumentNotNull(nodeId, "nodeId");
      var node = Handlers.StorageNodeRegistry.Get(nodeId);
      SetStorageNode(node);
    }

    /// <summary>
    /// Temporary overrides <see cref="CommandTimeout"/>.
    /// </summary>
    /// <param name="newTimeout">New <see cref="CommandTimeout"/> value.</param>
    /// <returns>Command timeout overriding scope.</returns>
    public IDisposable OverrideCommandTimeout(int? newTimeout)
    {
      var oldTimeout = CommandTimeout;
      CommandTimeout = newTimeout;
      return new Disposable(_ => { CommandTimeout = oldTimeout; });
    }

    /// <summary>
    /// Removes the specified set of entities.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="entities">The entities.</param>
    /// <exception cref="ReferentialIntegrityException">
    /// Entity is associated with another entity with <see cref="OnRemoveAction.Deny"/> on-remove action.
    /// </exception>
    public void Remove<T>([InstantHandle] IEnumerable<T> entities)
      where T : IEntity
    {
      using (var tx = OpenAutoTransaction()) {
        RemovalProcessor.Remove(entities.Cast<Entity>().ToList());
        tx.Complete();
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      string name = Name;
      return name.IsNullOrEmpty()
        ? string.Format(IdentifierFormat, identifier)
        : string.Format(FullNameFormat, name, identifier);
    }

    private SessionHandler CreateSessionHandler()
    {
      SqlConnection connection;
      var connectionIsExternal = false;
      var transactionIsExternal = false;

      var upgradeContext = UpgradeContext.GetCurrent(Domain.UpgradeContextCookie);
      if (upgradeContext!=null) {
        connection = upgradeContext.Services.Connection;
        connectionIsExternal = true;
        transactionIsExternal = true;
      }
      else if (Domain.SingleConnection!=null) {
        connection = Domain.SingleConnection;
        connectionIsExternal = true;
      }
      else
        connection = Handlers.StorageDriver.CreateConnection(this);

      return new SqlSessionHandler(this, connection, connectionIsExternal, transactionIsExternal);
    }

    // Constructors

    internal Session(Domain domain, SessionConfiguration configuration, bool activate)
      : base(domain)
    {
      Guid = Guid.NewGuid();
      IsDebugEventLoggingEnabled = OrmLog.IsLogged(LogLevel.Debug); // Just to cache this value

      // Both Domain and Configuration are valid references here;
      // Configuration is already locked
      Configuration = configuration;
      Name = configuration.Name;
      identifier = Interlocked.Increment(ref lastUsedIdentifier);
      CommandTimeout = configuration.DefaultCommandTimeout;
      allowSwitching = configuration.Supports(SessionOptions.AllowSwitching);

      // Handlers
      Handlers = domain.Handlers;
      Handler = CreateSessionHandler();

      // Caches, registry
      EntityStateCache = CreateSessionCache(configuration);
      EntityChangeRegistry = new EntityChangeRegistry(this);
      EntitySetChangeRegistry = new EntitySetChangeRegistry(this);
      ReferenceFieldsChangesRegistry = new ReferenceFieldsChangesRegistry(this);
      entitySetsWithInvalidState = new HashSet<EntitySetBase>();
      NonPairedReferenceRegistry = new EntityReferenceChangesRegistry(this);

      // Events
      EntityEvents = new EntityEventBroker();
      Events = new SessionEventAccessor(this, false);
      SystemEvents = new SessionEventAccessor(this, true);

      // Etc.
      PairSyncManager = new SyncManager(this);
      RemovalProcessor = new RemovalProcessor(this);
      pinner = new Pinner(this);
      Operations = new OperationRegistry(this);

      // Validation context
      ValidationContext = Configuration.Supports(SessionOptions.ValidateEntities)
        ? (ValidationContext) new RealValidationContext()
        : new VoidValidationContext();

      // Creating Services
      Services = CreateServices();

      disposableSet = new DisposableSet();
      remapper = new KeyRemapper(this);

      disableAutoSaveChanges = !configuration.Supports(SessionOptions.AutoSaveChanges);

      // Perform activation
      if (activate)
        disposableSet.Add(new SessionScope(this));

      // Query endpoint
      SystemQuery = Query = new QueryEndpoint(new QueryProvider(this));
    }

    // IDisposable implementation

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (isDisposed)
        return;
      try {
        OrmLog.Debug(Strings.LogSessionXDisposing, this);

        SystemEvents.NotifyDisposing();
        Events.NotifyDisposing();

        Services.DisposeSafely();
        Handler.DisposeSafely();

        Domain.ReleaseSingleConnection();

        disposableSet.DisposeSafely();
        disposableSet = null;

        EntityChangeRegistry.Clear();
        EntitySetChangeRegistry.Clear();
        EntityStateCache.Clear();
        ReferenceFieldsChangesRegistry.Clear();
        NonPairedReferenceRegistry.Clear();
      }
      finally {
        isDisposed = true;
      }
    }
  }
}
