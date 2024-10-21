// Copyright (C) 2007-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Interfaces;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Logging;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Sql;
using Xtensive.Sql.Info;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage access point.
  /// </summary>
  /// <sample>
  /// <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\DomainAndSession\DomainAndSessionSample.cs" region="Domain sample"></code>
  /// </sample>
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public sealed class Domain : IDisposable, IAsyncDisposable, IHasExtensions, ISessionSource
  {
    private readonly object disposeGuard = new object();
    private readonly object singleConnectionGuard = new object();

    private bool isDisposed;
    private Session singleConnectionOwner;

    private readonly bool isDebugEventLoggingEnabled;

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
    /// Gets the domain model.
    /// </summary>
    public DomainModel Model { get; internal set; }

    /// <summary>
    /// Gets the information about provider's capabilities.
    /// </summary>
    public ProviderInfo StorageProviderInfo => Handlers.ProviderInfo;

    /// <summary>
    /// Gets the domain-level service container.
    /// </summary>
    public IServiceContainer Services { get; internal set; }

    /// <summary>
    /// Gets storage node manager.
    /// </summary>
    public StorageNodeManager StorageNodeManager { get; private set; }

    /// <summary>
    /// Indicated whether query tagging is enabled by domain configuration
    /// by <see cref="DomainConfiguration.TagsLocation"/> proprety set to something
    /// other than <see cref="TagsLocation.Nowhere"/>.
    /// </summary>
    public bool TagsEnabled { get; }

    #region Private / internal members

    internal EntityDataReader EntityDataReader { get; private set; }

    internal Dictionary<TypeInfo, Action<SessionHandler, IEnumerable<Key>>> PrefetchActionMap { get; private set; }

    internal DomainHandler Handler { get { return Handlers.DomainHandler; } }

    internal HandlerAccessor Handlers { get; private set; }

    internal ConcurrentDictionary<TypeInfo, GenericKeyFactory> GenericKeyFactories { get; private set; }

    internal KeyGeneratorRegistry KeyGenerators { get; private set; }

    internal ConcurrentDictionary<TypeInfo, IReadOnlyList<PrefetchFieldDescriptor>> PrefetchFieldDescriptorCache { get; }

    internal ICache<object, Pair<object, ParameterizedQuery>> QueryCache { get; private set; }

    internal ICache<Key, Key> KeyCache { get; private set; }

    internal ConcurrentDictionary<Type, System.Linq.Expressions.MethodCallExpression> RootCallExpressionsCache { get; private set; }

    internal object UpgradeContextCookie { get; private set; }

    internal SqlConnection SingleConnection { get; private set; }

    internal IServiceContainer CreateSystemServices()
    {
      var registrations = new List<ServiceRegistration>{
        new ServiceRegistration(typeof (Domain), this),
        new ServiceRegistration(typeof (DomainConfiguration), Configuration),
        new ServiceRegistration(typeof (DomainHandler), Handler),
        new ServiceRegistration(typeof (HandlerAccessor), Handlers),
        new ServiceRegistration(typeof (NameBuilder), Handlers.NameBuilder),
        new ServiceRegistration(typeof (IStorageSequenceAccessor), new StorageSequenceAccessor(Handlers)),
      };

      return new ServiceContainer(registrations);
    }

    internal void ReleaseSingleConnection()
    {
      if (SingleConnection!=null)
        lock (singleConnectionGuard)
          singleConnectionOwner = null;
    }

    private void NotifySessionOpen(Session session)
    {
      var handler = SessionOpen;
      if (handler!=null)
        handler(this, new SessionEventArgs(session));
    }

    private void NotifyDisposing()
    {
      var handler = Disposing;
      if (handler!=null)
        handler(this, EventArgs.Empty);
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
    public Session OpenSession() =>
      OpenSession(Configuration.Sessions.Default);

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
    public Session OpenSession(SessionType type) =>
      OpenSession(GetSessionConfiguration(type));

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
    public Session OpenSession(SessionConfiguration configuration) =>
      OpenSessionInternal(configuration, null, configuration.Supports(SessionOptions.AutoActivation));

    internal Session OpenSessionInternal(SessionConfiguration configuration, StorageNode storageNode, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
      configuration.Lock(true);

      if (isDebugEventLoggingEnabled) {
        OrmLog.Debug(nameof(Strings.LogOpeningSessionX), configuration);
      }

      Session session;
      if (SingleConnection!=null) {
        // Ensure that we check shared connection availability
        // and acquire connection atomically.
        lock (singleConnectionGuard) {
          if (singleConnectionOwner!=null)
            throw new InvalidOperationException(string.Format(
              Strings.ExSessionXStillUsesSingleAvailableConnection, singleConnectionOwner));
          session = new Session(this, storageNode, configuration, activate);
          singleConnectionOwner = session;
        }
      }
      else
        session = new Session(this, storageNode, configuration, activate);

      NotifySessionOpen(session);
      return session;
    }

    /// <summary>
    /// Opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/> asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <sample><code>
    /// var ctSource = new CancellationTokenSource();
    /// using (var session = await Domain.OpenSessionAsync(ctSource.Token)) {
    /// // work with persistent objects here.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public Task<Session> OpenSessionAsync(CancellationToken cancellationToken = default) =>
      OpenSessionAsync(Configuration.Sessions.Default, cancellationToken);

    /// <summary>
    /// Opens new <see cref="Session"/> of specified <see cref="SessionType"/> asynchronously.
    /// </summary>
    /// <param name="type">The type of session.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <sample><code>
    /// var ctSource = new CancellationTokenSource();
    /// using (var session = await domain.OpenSessionAsync(sessionType, ctSource.Token)) {
    /// // work with persistent objects here.
    /// }
    /// </code></sample>
    public Task<Session> OpenSessionAsync(SessionType type, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return OpenSessionAsync(GetSessionConfiguration(type), cancellationToken);
    }

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/> asynchronously.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <sample><code>
    /// var ctSource = new CancellationTokenSource();
    /// using (var session = await domain.OpenSessionAsync(configuration, ctSource.Token)) {
    /// // work with persistent objects here
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public Task<Session> OpenSessionAsync(SessionConfiguration configuration, CancellationToken cancellationToken = default)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      SessionScope sessionScope = null;
      try {
        if (configuration.Supports(SessionOptions.AutoActivation)) {
          sessionScope = new SessionScope();
        }
        return OpenSessionInternalAsync(configuration, null, sessionScope, cancellationToken);
      }
      catch {
        sessionScope?.Dispose();
        throw;
      }
    }

    internal SessionConfiguration GetSessionConfiguration(SessionType type) =>
      type switch {
        SessionType.User => Configuration.Sessions.Default,
        SessionType.System => Configuration.Sessions.System,
        SessionType.KeyGenerator => Configuration.Sessions.KeyGenerator,
        SessionType.Service => Configuration.Sessions.Service,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
      };

    internal async Task<Session> OpenSessionInternalAsync(SessionConfiguration configuration, StorageNode storageNode, SessionScope sessionScope, CancellationToken cancellationToken)
    {
      configuration.Lock(true);

      if (isDebugEventLoggingEnabled) {
        OrmLog.Debug(nameof(Strings.LogOpeningSessionX), configuration);
      }

      Session session;
      if (SingleConnection!=null) {
        // Ensure that we check shared connection availability
        // and acquire connection atomically.
        lock (singleConnectionGuard) {
          if (singleConnectionOwner!=null)
            throw new InvalidOperationException(string.Format(
              Strings.ExSessionXStillUsesSingleAvailableConnection, singleConnectionOwner));
          session = new Session(this, storageNode, configuration, false);
          singleConnectionOwner = session;
        }
      }
      else {
        // DO NOT make session active right from constructor.
        // That would make session accessible for user before
        // connection become opened.
        session = new Session(this, storageNode, configuration, false);
        ExceptionDispatchInfo exceptionDispatchInfo = null;
        try {
          await ((SqlSessionHandler) session.Handler).OpenConnectionAsync(cancellationToken)
            .ContinueWith(t => {
              if (t.Status == TaskStatus.RanToCompletion) {
                if (sessionScope != null) {
                  session.AttachToScope(sessionScope);
                }
              }
              else if (t.Exception is Exception ex) {
                if (ex is System.AggregateException aggregateException && aggregateException.InnerExceptions.Count == 1) {
                  ex = aggregateException.InnerExceptions[0];
                }
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
              }
            }, TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.ExecuteSynchronously)
            .ConfigureAwait(false);
        }
        catch (OperationCanceledException) {
          await session.DisposeSafelyAsync().ConfigureAwait(false);
          throw;
        }
        finally {
          exceptionDispatchInfo?.Throw();
        }
      }
      NotifySessionOpen(session);
      return session;
    }

    #endregion

    #region IHasExtensions members

    /// <inheritdoc/>
    public IExtensionCollection Extensions { get; private set; }

    #endregion

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to the specified <see cref="DomainConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The configuration of domain to build.</param>
    /// <returns>Newly built <see cref="Domain"/>.</returns>
    public static Domain Build(DomainConfiguration configuration) => UpgradingDomainBuilder.Build(configuration);

    /// <summary>
    /// Asynchronously builds the new <see cref="Domain"/> according to the specified <see cref="DomainConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The configuration of domain to build.</param>
    /// <param name="token">The token to cancel asynchronous operation if needed.</param>
    /// <returns>Newly built <see cref="Domain"/>.</returns>
    public static Task<Domain> BuildAsync(DomainConfiguration configuration, CancellationToken token = default) =>
      UpgradingDomainBuilder.BuildAsync(configuration, token);


    // Constructors

    internal Domain(DomainConfiguration configuration, object upgradeContextCookie, SqlConnection singleConnection)
    {
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      GenericKeyFactories = new ConcurrentDictionary<TypeInfo, GenericKeyFactory>();
      EntityDataReader = new EntityDataReader(this);
      KeyGenerators = new KeyGeneratorRegistry();
      PrefetchFieldDescriptorCache = new ConcurrentDictionary<TypeInfo, IReadOnlyList<PrefetchFieldDescriptor>>();
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      QueryCache = new FastConcurrentLruCache<object, Pair<object, ParameterizedQuery>>(Configuration.QueryCacheSize, k => k.First);
      RootCallExpressionsCache = new ConcurrentDictionary<Type, System.Linq.Expressions.MethodCallExpression>();
      PrefetchActionMap = new Dictionary<TypeInfo, Action<SessionHandler, IEnumerable<Key>>>();
      Extensions = new ExtensionCollection();
      UpgradeContextCookie = upgradeContextCookie;
      SingleConnection = singleConnection;
      StorageNodeManager = new StorageNodeManager(Handlers);
      TagsEnabled = configuration.TagsLocation != TagsLocation.Nowhere;
      isDebugEventLoggingEnabled = OrmLog.IsLogged(LogLevel.Debug); // Just to cache this value
    }

    /// <inheritdoc/>
    public void Dispose() => InnerDispose(false).GetAwaiter().GetResult();

    public ValueTask DisposeAsync() => InnerDispose(true);

    public async ValueTask InnerDispose(bool isAsync)
    {
      lock (disposeGuard) {
        if (isDisposed) {
          return;
        }

        isDisposed = true;
      }

      if (isDebugEventLoggingEnabled) {
        OrmLog.Debug(nameof(Strings.LogDomainIsDisposing));
      }

      NotifyDisposing();
      Services.Dispose();

      if (SingleConnection == null) {
        return;
      }

      SqlConnection singleConnectionLocal;
      lock (singleConnectionGuard) {
        if (singleConnectionOwner != null) {
          OrmLog.Warning(
            Strings.LogUnableToCloseSingleAvailableConnectionItIsStillUsedBySessionX,
            singleConnectionOwner);
          return;
        }
        else {
          singleConnectionLocal = SingleConnection;
          SingleConnection = null;
        }
      }

      var driver = Handlers.StorageDriver;
      if (isAsync) {
        await driver.CloseConnectionAsync(null, singleConnectionLocal).ConfigureAwait(false);
        await driver.DisposeConnectionAsync(null, singleConnectionLocal).ConfigureAwait(false);
      }
      else {
        driver.CloseConnection(null, singleConnectionLocal);
        driver.DisposeConnection(null, singleConnectionLocal);
      }
    }
  }
}
