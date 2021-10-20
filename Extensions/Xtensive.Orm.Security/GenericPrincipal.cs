// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;
using Xtensive.Orm;
using Xtensive.Orm.Security.Configuration;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Defines generic principal that can be used in username/password authentication scheme.
  /// </summary>
  public abstract class GenericPrincipal : Principal
  {
    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    /// <value>The password hash.</value>
    [Field(Length = 128)]
    public string PasswordHash { get; protected set; }

    /// <summary>
    /// Sets the password.
    /// </summary>
    /// <param name="password">The password.</param>
    public virtual void SetPassword(string password)
    {
      var config = Session.GetSecurityConfiguration();
      var service = Session.Services.Get<IHashingService>(config.HashingServiceName);

      if (service == null)
        throw new InvalidOperationException($"Hashing service by name {config.HashingServiceName} is not found. Check Xtensive.Security configuration");

      PasswordHash = service.ComputeHash(password);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericPrincipal"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected GenericPrincipal(Session session)
      : base(session)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericPrincipal"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="values">Key values.</param>
    protected GenericPrincipal(Session session, params object[] values)
      : base(session, values)
    {
    }
  }
}