// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to a single storage.
  /// </summary>
  public sealed class Domain
  {
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


    #region OpenSession methods

    /// <summary>
    /// Creates the session.
    /// </summary>
    /// <returns></returns>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession()
    {
      return OpenSession(Configuration.Session);
    }

    /// <summary>
    /// Creates the session.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession(SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      configuration.Lock(true);
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
