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
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to a single storage.
  /// </summary>
  public sealed class Domain : CriticalFinalizerObject,
    IDisposable
  {
    private readonly ThreadSafeDictionary<RecordSetHeader, RecordSetMapping> recordSetMappings = 
      ThreadSafeDictionary<RecordSetHeader, RecordSetMapping>.Create(new object());
    private int sessionCounter = 1;

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; private set; }

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
    /// Gets the key manager.
    /// </summary>
    public KeyManager KeyManager {
      [DebuggerStepThrough]
      get { return Handlers.KeyManager; }
    }

    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    internal Dictionary<TypeInfo, Tuple> Prototypes { get; private set; }

    internal HandlerAccessor Handlers { get; private set; }

    internal ThreadSafeDictionary<RecordSetHeader, RecordSetMapping> RecordSetMappings
    {
      get { return recordSetMappings; }
    }

    #region OpenSession methods

    /// <summary>
    /// Opens the session with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession()
    {
      return OpenSession((SessionConfiguration)Configuration.Session.Clone());
    }

    /// <summary>
    /// Opens the session with specified <paramref name="configuration"/>.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      if (EnumerableExtensions.IsNullOrEmpty(configuration.Name)) {
        configuration.Name = sessionCounter.ToString();
        Interlocked.Increment(ref sessionCounter);
      }
      configuration.Lock(true);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Opening session '{0}'", configuration);

      var session = new Session(this, configuration);
      return new SessionScope(session);
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
      Configuration = configuration;
      Handlers = new HandlerAccessor(this);
      Prototypes = new Dictionary<TypeInfo, Tuple>();
    }

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
      /* if (isDisposed)
        return;
      lock (_lock) {
        if (isDisposed)
          return;
        try {
          if (Log.IsLogged(LogEventTypes.Debug))
            Log.Debug("Session '{0}'. Disposing", this);
          Handler.Commit();
          Handler.DisposeSafely();
          compilationScope.DisposeSafely();
        }
        finally {
          isDisposed = true;
        }
      }*/
    }
  }
}
