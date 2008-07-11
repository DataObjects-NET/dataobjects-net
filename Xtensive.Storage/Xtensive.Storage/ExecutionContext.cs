// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage
{
  /// <summary>
  /// Storage execution context. Contains references to most frequently 
  /// used objects such as <see cref="Domain"/>, <see cref="DomainHandler"/>,
  /// <see cref="Session"/>, <see cref="SessionHandler"/> and so on.
  /// </summary>
  /// <remarks>Implements pattern 'Mediator'.</remarks>
  public sealed class ExecutionContext
  {
    private readonly Registry<HierarchyInfo, IKeyProvider> keyProviders = new Registry<HierarchyInfo, IKeyProvider>();
    private readonly KeyManager keyManager;

    /// <summary>
    /// Gets the handler provider.
    /// </summary>
    public HandlerProvider HandlerProvider { get; internal set; }

    /// <summary>
    /// Gets the domain.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the domain handler.
    /// </summary>
    public DomainHandler DomainHandler { get; internal set; }

    /// <summary>
    /// Gets the domain model.
    /// </summary>
    public DomainInfo Model { get; internal set;}

    /// <summary>
    /// Gets the configuration.
    /// </summary>
    public DomainConfiguration Configuration { get; internal set; }

    /// <summary>
    /// Gets or sets the name provider.
    /// </summary>
    public NameProvider NameProvider { get; internal set; }

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

    /// <summary>
    /// Gets the currently active session.
    /// </summary>
    public Session Session
    {
      get { return Storage.Session.Current; }
    }

    /// <summary>
    /// Gets the currently active session handler.
    /// </summary>
    public SessionHandler SessionHandler
    {
      get { return Session.Handler; }
    }


    // Constructors

    internal ExecutionContext(Domain domain)
    {
      Domain = domain;
      keyManager = new KeyManager(this);
    }
  }
}