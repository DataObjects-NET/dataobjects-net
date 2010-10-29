// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.11

using System;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm
{
  partial class Session 
  {
    /// <summary>
    /// Opens new <see cref="Session"/> with default <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = Domain.OpenSession()) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property when <see cref="SessionOptions.AutoActivation"/> flag is set for <see cref="SessionConfiguration.Options"/>, 
    /// // or the session is activate explicitely through <see cref="Session.Activate()"/>.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    [Obsolete("Use Domain.OpenSession() method instead")]
    public static Session Open(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");

      return domain.OpenSession();
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
    /// using (var session = domain.OpenSession(false)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property when <see cref="SessionOptions.AutoActivation"/> flag is set for <see cref="SessionConfiguration.Options"/>, 
    /// // or the session is activate explicitely through <see cref="Session.Activate()"/>.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    [Obsolete("Use Activate() method to activate the session.")]
    public static Session Open(Domain domain, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return Open(domain, domain.Configuration.Sessions.Default, activate);
    }

    /// <summary>
    /// Opens new <see cref="Session"/> of specified <see cref="SessionType"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>    
    /// <param name="type">The type of session.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.OpenSession(sessionType)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property when <see cref="SessionOptions.AutoActivation"/> flag is set for <see cref="SessionConfiguration.Options"/>, 
    /// // or the session is activate explicitely through <see cref="Session.Activate()"/>.
    /// }
    /// </code></sample>
    [Obsolete("Use Domain.OpenSession(SessionType type) method instead")]
    public static Session Open(Domain domain, SessionType type)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");

      return domain.OpenSession(type);
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
    /// using (domain.OpenSession(sessionType, true)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    [Obsolete("Use Activate() method to activate the session.")]
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
    /// Opens new <see cref="Session"/> with specified <see cref="SessionConfiguration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration">The session configuration.</param>
    /// <returns>
    /// New <see cref="Session"/> object.
    /// </returns>
    /// <sample><code>
    /// using (var session = domain.OpenSession(configuration)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property when <see cref="SessionOptions.AutoActivation"/> flag is set for <see cref="SessionConfiguration.Options"/>, 
    /// // or the session is activate explicitely through <see cref="Session.Activate()"/>.
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    [Obsolete("Use Domain.OpenSession(SessionConfiguration configuration) method instead")]
    public static Session Open(Domain domain, SessionConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      return domain.OpenSession(configuration);
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
    /// using (domain.OpenSession(sessionConfiguration, false)) {
    /// // work with persistent objects here
    /// // Session is available through static Session.Current property
    /// }
    /// </code></sample>
    /// <seealso cref="Session"/>
    [Obsolete("Use Activate() method to activate the session.")]
    public static Session Open(Domain domain, SessionConfiguration configuration, bool activate)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      return domain.OpenSession(configuration, activate);
    }
  }
}