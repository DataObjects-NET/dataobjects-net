// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage
{
  partial class Session 
  {
    /// <summary>
    /// Opens and activates new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (Session.Open(domain)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public static Session Open(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return Open(domain, domain.Configuration.Sessions.Default, true);
    }

    /// <summary>
    /// Opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="activate">Determines whether created session should be activated or not.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (Session.Open(domain)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public static Session Open(Domain domain, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return Open(domain, domain.Configuration.Sessions.Default, activate);
    }

    /// <summary>
    /// Opens and activates new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>    
    /// <param name="type">The type of session.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (Session.Open(domain, sessionType)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    public static Session Open(Domain domain, SessionType type)
    {
      return Open(domain, type, true);
    }

    /// <summary>
    /// Opens new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>    
    /// <param name="type">The type of session.</param>
    /// <param name="activate">Determines whether created session should be activated or not.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (Session.Open(domain, sessionType, false)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    public static Session Open(Domain domain, SessionType type, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");

      switch (type) {
      case SessionType.User:
        return Open(domain, domain.Configuration.Sessions.Default, activate);
      case SessionType.System:
        return Open(domain, domain.Configuration.Sessions.System, activate);
      case SessionType.KeyGenerator:
        return Open(domain, domain.Configuration.Sessions.KeyGenerator, activate);
      case SessionType.Service:
        return Open(domain, domain.Configuration.Sessions.Service, activate);
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }

    /// <summary>
    /// Opens and activates new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (Session.Open(domain, sessionConfiguration)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public static Session Open(Domain domain, SessionConfiguration configuration)
    {
      return Open(domain, configuration, true);
    }

    /// <summary>
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration">The session configuration.</param>
    /// <param name="activate">Determines whether created session should be activated or not.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (Session.Open(domain, sessionConfiguration, false)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    public static Session Open(Domain domain, SessionConfiguration configuration, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      return domain.OpenSession(configuration, activate);
    }
  }
}