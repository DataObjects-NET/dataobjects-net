// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System.Diagnostics;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Threading;
using Xtensive.Storage.Building;
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
  public sealed class Domain
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

    /// <summary>
    /// Gets the domain handler.
    /// </summary>
    internal DomainHandler Handler {
      [DebuggerStepThrough]
      get { return Handlers.DomainHandler; }
    }

    internal HandlerAccessor Handlers { get; private set; }

    internal ThreadSafeDictionary<RecordSetHeader, RecordSetMapping> RecordSetMappings
    {
      get { return recordSetMappings; }
    }

    #region OpenSession methods

    /// <summary>
    /// Creates the session.
    /// </summary>
    /// <returns></returns>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession()
    {
      return OpenSession((SessionConfiguration)Configuration.Session.Clone());
    }

    /// <summary>
    /// Creates the session.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      if (configuration.Name.IsNullOrEmpty()) {
        configuration.Name = sessionCounter.ToString();
        Interlocked.Increment(ref sessionCounter);
      }
      configuration.Lock(true);

      if (Log.IsLogged(LogEventTypes.Debug))
        Log.Debug("Opening session '{0}'", configuration);

      var session = new Session(this, configuration);
      return new SessionScope(session);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionScope"/> class.
    /// </summary>
    /// <param name="userName">User name to authenticate.</param>
    /// <param name="authParams">Authentication parameters (e.g. password).</param>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession(string userName, params object[] authParams)
    {
      return OpenSession(new SessionConfiguration(userName, authParams));
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
    }
  }
}
