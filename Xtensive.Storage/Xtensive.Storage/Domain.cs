// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Disposable;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An access point to a single storage.
  /// </summary>
  public sealed class Domain : CriticalFinalizerObject,
    IDisposableContainer
  {
    private int sessionCounter = 1;

    /// <summary>
    /// Gets the current <see cref="Domain"/> object
    /// using <see cref="Session"/>. <see cref="Session.Current"/>.
    /// </summary>
    public static Domain Current {
      get {
        var session = Session.Current;
        return session!=null ? session.Domain : null;
      }
    }
    
    /// <summary>
    /// Gets the configuration.
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
    /// Caches <see cref="Log.IsLogged"/> method result for <see cref="LogEventTypes.Debug"/> event.
    /// </summary>
    public bool IsDebugEventLoggingEnabled { get; private set; }

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    #region Private \ internal properties

    internal HandlerAccessor Handlers { get; private set; }

    internal Registry<HierarchyInfo, KeyGenerator> KeyGenerators { get; private set; }

    internal ICache<Key, Key> KeyCache { get; private set; }

    internal Dictionary<AssociationInfo, ActionSet> PairSyncActions { get; private set; }

    #endregion

    #region OpenSession methods

    /// <summary>
    /// Opens the session with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    public SessionConsumptionScope OpenSession()
    {
      return OpenSession((SessionConfiguration)Configuration.Sessions.Default.Clone());
    }

    /// <summary>
    /// Opens the session with specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionConsumptionScope"/> object.</returns>
    public SessionConsumptionScope OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      if (EnumerableExtensions.IsNullOrEmpty(configuration.Name)) {
        configuration.Name = sessionCounter.ToString();
        Interlocked.Increment(ref sessionCounter);
      }
      configuration.Lock(true);

      if (IsDebugEventLoggingEnabled)
        Log.Debug("Opening session '{0}'", configuration);

      var session = new Session(this, configuration);
      return new SessionConsumptionScope(session);
    }

    #endregion

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to the specified 
    /// <see cref="DomainConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="DomainConfiguration"/>.</param>
    /// <returns>Newly built <see cref="Domain"/>.</returns>
    public static Domain Build(DomainConfiguration configuration)
    {
      return DomainBuilder.Build(configuration);
    }


    // Constructors

    internal Domain(DomainConfiguration configuration)
    {
      IsDebugEventLoggingEnabled = Log.IsLogged(LogEventTypes.Debug); // Just to cache this value
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      RecordSetParser = new RecordSetParser(this);
      KeyGenerators = new Registry<HierarchyInfo, KeyGenerator>();
      KeyCache = new LruCache<Key, Key>(Configuration.KeyCacheSize, k => k);
      PairSyncActions = new Dictionary<AssociationInfo, ActionSet>(1024);
      TemporaryData = new GlobalTemporaryData();
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
      if (DisposingState == DisposingState.None) lock(this) if (DisposingState == DisposingState.None) {
        DisposingState = DisposingState.Disposing;
        try {
          if (IsDebugEventLoggingEnabled)
            Log.Debug("Domain disposing {0}.", isDisposing ? "explicitly" : "by calling finalizer.");
          KeyGenerators.DisposeSafely();
        }
        finally {
          DisposingState=DisposingState.Disposed;
        }
      }
    }
  }
}
