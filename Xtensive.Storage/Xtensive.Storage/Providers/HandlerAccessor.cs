// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Storage handler accessor.
  /// Provided by protected members, such as <see cref="HandlerBase.HandlerAccessor"/> 
  /// to provide access to other storage handlers.
  public sealed class HandlerAccessor
  {
    /// <summary>
    /// Gets the domain.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the handler provider.
    /// </summary>
    public HandlerProvider HandlerProvider { get; internal set; }

    /// <summary>
    /// Gets the <see cref="Domain"/> handler.
    /// </summary>
    public DomainHandler DomainHandler { get; internal set; }

    /// <summary>
    /// Gets the handler of the current <see cref="Session"/>.
    /// </summary>
    public SessionHandler SessionHandler
    {
      get { return Session.Current.Handler; }
    }


    // Constructors

    internal HandlerAccessor(Domain domain)
    {
      Domain = domain;
    }
  }
}