// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage
{
  public sealed class Domain
  {
    private readonly Registry<HierarchyInfo, IKeyProvider> keyProviders = new Registry<HierarchyInfo, IKeyProvider>();
    private readonly KeyManager keyManager;

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; internal set; }

    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    public DomainInfo Model { get; internal set; }

    /// <summary>
    /// Gets or sets the handler.
    /// </summary>
    public DomainHandler Handler { get; internal set; }

    /// <summary>
    /// Gets the key providers.
    /// </summary>
    public Registry<HierarchyInfo, IKeyProvider> KeyProviders
    {
      get { return keyProviders; }
    }

    /// <summary>
    /// Gets the key manager.
    /// </summary>
    public KeyManager KeyManager
    {
      get { return keyManager; }
    }

    internal HandlerProvider HandlerProvider { get; set; }

    /// <summary>
    /// Gets or sets the name provider.
    /// </summary>
    public NameProvider NameProvider { get; internal set; }

    /// <summary>
    /// Creates the session.
    /// </summary>
    /// <returns></returns>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession()
    {
      return OpenSession(new SessionConfiguration());
    }

    /// <summary>
    /// Creates the session.
    /// </summary>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession(SessionConfiguration configuration)
    {
      Session session = new Session(this);
      session.Configure(configuration);
      SessionHandler sessionHandler = HandlerProvider.GetHandler<SessionHandler>();
      sessionHandler.Session = session;
      session.Handler = sessionHandler;
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

    /// <summary>
    /// Builds the new <see cref="Domain"/> according to specified <see cref="DomainConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The <see cref="DomainConfiguration"/>.</param>
    /// <returns>Newly built <see cref="Domain"/>.</returns>
    public static Domain Build(DomainConfiguration configuration)
    {
      return DomainBuilder.Build(configuration);
    }


    // Constructors

    internal Domain()
    {
      keyManager = new KeyManager(this);
    }
  }
}