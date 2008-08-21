// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.11

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Storage handler accessor.
  /// Provided by protected members, such as <see cref="HandlerBase.Handlers"/> 
  /// to provide access to other available handlers.
  public sealed class HandlerAccessor
  {
    /// <summary>
    /// Gets the <see cref="Xtensive.Storage.Domain"/> 
    /// this handler accessor is bound to.
    /// </summary>
    public Domain Domain { get; private set; }

    /// <summary>
    /// Gets the handler provider 
    /// creating handlers in the <see cref="Domain"/>.
    /// </summary>
    public HandlerFactory HandlerFactory { get; internal set; }

    /// <summary>
    /// Gets the name builder.
    /// </summary>
    public NameBuilder NameBuilder { get; internal set; }

    /// <summary>
    /// Gets the key manager.
    /// </summary>
    public KeyManager KeyManager { get; internal set; }

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

    /// <summary>
    /// Creates the session and returns its handler as <param name="handler"/>.
    /// </summary>
    /// <returns>New <see cref="SessionScope"/> object.</returns>
    public SessionScope OpenSession<T>(SessionType type, out T handler)
      where T : SessionHandler
    {
      var scope = Domain.OpenSession(type);
      handler = (T)scope.Session.Handler;
      return scope;
    }


    // Constructors

    internal HandlerAccessor(Domain domain)
    {
      Domain = domain;
    }
  }
}