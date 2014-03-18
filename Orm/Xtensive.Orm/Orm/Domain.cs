// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Xtensive.Caching;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sql;

namespace Xtensive.Orm
{
  /// <summary>
  /// Storage access point.
  /// </summary>
  /// <sample>
  /// <code lang="cs" source="..\Xtensive.Orm\Xtensive.Orm.Manual\DomainAndSession\DomainAndSessionSample.cs" region="Domain sample"></code>
  /// </sample>
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public sealed class Domain : IDisposable, IHasExtensions
  {
    private readonly object disposeGuard = new object();
    private readonly object singleConnectionGuard = new object();
    
    private bool isDisposed;
    private Session singleConnectionOwner;

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
    public ProviderInfo StorageProviderInfo { get { return Handlers.ProviderInfo; } }

    /// <summary>
    /// Gets the domain-level service container.
    /// </summary>
    public IServiceContainer Services { get; internal set; }

    /// <summary>
    /// Gets <see cref="NodeManager"/>.
    /// </summary>
    public NodeManager NodeManager { get; private set; }

    #region Private / internal members

    internal RecordSetReader RecordSetReader { get; private set; }

    internal Dictionary<TypeInfo, Action<SessionHandler, IEnumerable<Key>>> PrefetchActionMap { get; private set; }

    internal DomainHandler Handler { get { return Handlers.DomainHandler; } }

    internal HandlerAccessor Handlers { get; private set; }

    internal ThreadSafeDictionary<int, GenericKeyTypeInfo> GenericKeyTypes { get; private set; }

    internal KeyGeneratorRegistry KeyGenerators { get; private set; }

    internal ConcurrentDictionary<TypeInfo, ReadOnlyList<PrefetchFieldDescriptor>> PrefetchFieldDescriptorCache { get; private set; }
    
    internal ICache<object, Pair<object, TranslatedQuery>> QueryCache { get; private set; }

    internal ICache<Key, Key> KeyCache { get; private set; }

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

      return OpenSession(configuration, configuration.Supports(SessionOptions.AutoActivation));
    }

    internal Session OpenSession(SessionConfiguration configuration, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      configuration.Lock(true);

      OrmLog.Debug(Strings.LogOpeningSessionX, configuration);

      Session session;

      if (SingleConnection!=null) {
        // Ensure that we check shared connection availability
        // and acquire connection atomically.
        lock (singleConnectionGuard) {
          if (singleConnectionOwner!=null)
            throw new InvalidOperationException(string.Format(
              Strings.ExSessionXStillUsesSingleAvailableConnection, singleConnectionOwner));
          session = new Session(this, configuration, activate);
          singleConnectionOwner = session;
        }
      }
      else
        session = new Session(this, configuration, activate);

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
    public static Domain Build(DomainConfiguration configuration)
    {
      return UpgradingDomainBuilder.Build(configuration);
    }


    // Constructors

    internal Domain(DomainConfiguration configuration, object upgradeContextCookie, SqlConnection singleConnection)
    {
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      GenericKeyTypes = ThreadSafeDictionary<int, GenericKeyTypeInfo>.Create(new object());
      RecordSetReader = new RecordSetReader(this);
      KeyGenerators = new KeyGeneratorRegistry();
      PrefetchFieldDescriptorCache = new ConcurrentDictionary<TypeInfo, ReadOnlyList<PrefetchFieldDescriptor>>();
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      QueryCache = new LruCache<object, Pair<object, TranslatedQuery>>(Configuration.QueryCacheSize, k => k.First);
      PrefetchActionMap = new Dictionary<TypeInfo, Action<SessionHandler, IEnumerable<Key>>>();
      Extensions = new ExtensionCollection();
      UpgradeContextCookie = upgradeContextCookie;
      SingleConnection = singleConnection;
      NodeManager = new NodeManager(Handlers);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      lock (disposeGuard) {
        if (isDisposed)
          return;
        isDisposed = true;

        OrmLog.Debug(Strings.LogDomainIsDisposing);

        NotifyDisposing();
        Services.Dispose();

        if (SingleConnection==null)
          return;

        lock (singleConnectionGuard) {
          if (singleConnectionOwner==null)
            Handlers.StorageDriver.DisposeConnection(null, SingleConnection);
          else
            OrmLog.Warning(
              Strings.LogUnableToCloseSingleAvailableConnectionItIsStillUsedBySessionX,
              singleConnectionOwner);
        }
      }
    }
  }
}
