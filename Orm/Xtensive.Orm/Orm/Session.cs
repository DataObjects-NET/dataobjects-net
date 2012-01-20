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
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Diagnostics;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;
using Xtensive.IoC;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Operations;
using Xtensive.Orm.PairIntegrity;
using Xtensive.Orm.Providers;
using Xtensive.Orm.ReferentialIntegrity;
using Xtensive.Orm.Resources;
using Xtensive.Orm.Rse.Compilation;
using EnumerationContext=Xtensive.Orm.Rse.Providers.EnumerationContext;
using IsolationLevel = System.Transactions.IsolationLevel;

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
  /// Existing session can be activated by <see cref="Activate"/> method.
  /// </para>
  /// </remarks>
  /// <example>
  /// <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\DomainAndSession\DomainAndSessionSample.cs" 
  /// region="Session sample"></code>
  /// </example>
  /// <seealso cref="Domain"/>
  /// <seealso cref="SessionBound" />
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
    private volatile bool isDisposed;
    private readonly bool allowSwitching;
    private long identifier;

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
    /// Caches <see cref="ILogBase.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </summary>
    internal bool IsDebugEventLoggingEnabled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="SaveChanges"/> method is running.
    /// </summary>
    internal bool IsPersisting { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether session is disconnected:
    /// a <see cref="DisconnectedState"/> is attached to it (not <see langword="null" />).
    /// </summary>
    public bool IsDisconnected { 
      get { return DisconnectedState!=null; } 
    }

    /// <summary>
    /// Gets the attached <see cref="Orm.DisconnectedState"/> object, if any.
    /// </summary>
    public DisconnectedState DisconnectedState { get; internal set; }

    /// <summary>
    /// Gets the operations registry of this <see cref="Session"/>.
    /// </summary>
    public OperationRegistry Operations { get; private set; }

    private int? commandTimeout;

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

    #region Private / internal members

    internal SessionHandler Handler { get; set; }

    internal HandlerAccessor Handlers { get; private set; }

    internal SyncManager PairSyncManager { get; private set; }

    internal RemovalProcessor RemovalProcessor { get; private set; }

    internal Pinner Pinner { get; private set; }

    internal CompilationService CompilationService { get { return Handlers.DomainHandler.CompilationService; } }

    internal bool IsDelayedQueryRunning { get; private set; }

    private void EnsureNotDisposed()
    {
      if (isDisposed)
        throw new ObjectDisposedException(Strings.ExSessionIsAlreadyDisposed);
    }

    internal EnumerationContext CreateEnumerationContext()
    {
      Persist(PersistReason.Query);
      ProcessDelayedQueries(true);
//      EnsureTransactionIsStarted();
      return Handler.CreateEnumerationContext();
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
        Strings.ExAttemptToAutomaticallyActivateSessionXInsideSessionYIsBlocked
          .FormatWith(this, currentSession));
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
    public void Remove<T>(IEnumerable<T> entities)
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


    // Constructors

    internal Session(Domain domain, SessionConfiguration configuration, bool activate)
      : base(domain)
    {
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value

      // Both Domain and Configuration are valid references here;
      // Configuration is already locked
      Configuration = configuration;
      Name = configuration.Name;
      identifier = Interlocked.Increment(ref lastUsedIdentifier);
      CommandTimeout = configuration.DefaultCommandTimeout;
      allowSwitching = (configuration.Options & SessionOptions.AllowSwitching)==SessionOptions.AllowSwitching;

      // Handlers
      Handlers = domain.Handlers;
      Handler = Handlers.HandlerFactory.CreateHandler<SessionHandler>();
      Handler.Session = this;
      Handler.Initialize();

      // Caches, registry
      EntityStateCache = CreateSessionCache(configuration);
      EntityChangeRegistry = new EntityChangeRegistry(this);

      // Events
      EntityEvents = new EntityEventBroker();
      Events = new SessionEventAccessor(this, false);
      SystemEvents = new SessionEventAccessor(this, true);

      // Etc.
      PairSyncManager = new SyncManager(this);
      RemovalProcessor = new RemovalProcessor(this);
      Pinner = new Pinner(this);
      Operations = new OperationRegistry(this);

      // Creating Services
      var serviceContainerType = Configuration.ServiceContainerType ?? typeof (ServiceContainer);
      Services = 
        ServiceContainer.Create(typeof (ServiceContainer), 
          from type in Domain.Configuration.Types.SessionServices
          from registration in ServiceRegistration.CreateAll(type)
          select registration,
          ServiceContainer.Create(serviceContainerType, Handler.CreateBaseServices()));

      disposableSet = new DisposableSet();

      // Handling Disconnected option
      if ((Configuration.Options & SessionOptions.Disconnected) == SessionOptions.Disconnected) {
        disposableSet.Add(new DisconnectedState().Attach(this));
        disposableSet.Add(DisconnectedState.Connect());
      }

      // Perform activation
      if (activate)
        disposableSet.Add(new SessionScope(this));

      // Query endpoint
      var qep = Services.Get<IQueryEndpointProvider>();
      if (qep != null)
        Query = qep.GetQueryEndpoint(this);
      else
        Query = new QueryEndpoint(this);
    }

    // IDisposable implementation

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      if (isDisposed)
        return;
      try {
        if (IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXDisposing, this);
        
        SystemEvents.NotifyDisposing();
        Events.NotifyDisposing();
        
        Services.DisposeSafely();
        Handler.DisposeSafely();
        disposableSet.DisposeSafely();
        disposableSet = null;
      }
      finally {
        isDisposed = true;
      }
    }
  }
}
