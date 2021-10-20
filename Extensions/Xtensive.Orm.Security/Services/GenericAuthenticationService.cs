// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.16

using System;
using System.Linq;
using System.Security.Principal;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Provides authentication for <see cref="GenericPrincipal"/> with username/password authentication scheme.
  /// </summary>
  [Service(typeof(IAuthenticationService), Singleton = true, Name = "default")]
  public class GenericAuthenticationService : SessionBound, IAuthenticationService
  {
    /// <inheritdoc/>
    public IPrincipal Authenticate(IIdentity identity, params object[] args)
    {
      ArgumentValidator.EnsureArgumentNotNull(identity, "identity");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(identity.Name, "identity.Name");

      return Authenticate(identity.Name, args);
    }

    /// <inheritdoc/>
    public IPrincipal Authenticate(string name, params object[] args)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      string password = string.Empty;
      if (args != null && args.Length > 0)
        password = (string) args[0];

      return Authenticate(name, password);

    }

    /// <summary>
    /// Authenticates the specified username and password pair.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns><see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null" /> if they are not.</returns>
    protected virtual IPrincipal Authenticate(string username, string password)
    {
      var config = Session.GetSecurityConfiguration();
      var service = Session.Services.Get<IHashingService>(config.HashingServiceName);

      if (service == null)
        throw new InvalidOperationException($"Hashing service by name {config.HashingServiceName} is not found. Check Xtensive.Security configuration");

      // GenericPrincipal is not in the model, let's find its descendant
      var model = Session.Domain.Model;
      var rootPrincipalType = model.Hierarchies
        .Select(h => h.Root.UnderlyingType)
        .FirstOrDefault(t => typeof (GenericPrincipal).IsAssignableFrom(t));

      if (rootPrincipalType == null)
        throw new InvalidOperationException("No descendants of GenericPrincipal type are found in domain model");

      var candidate = (Session.Query.All(rootPrincipalType) as IQueryable<GenericPrincipal>)
        .Where(u => u.Name == username)
        .SingleOrDefault();

      if (candidate != null && service.VerifyHash(password, candidate.PasswordHash))
        return candidate;

      return null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericAuthenticationService"/> class.
    /// </summary>
    /// <param name="session"><see cref="T:Xtensive.Orm.Session"/>, to which current instance
    /// is bound.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// 	<paramref name="session"/> is <see langword="null"/>.</exception>
    [ServiceConstructor]
    public GenericAuthenticationService(Session session)
      : base(session)
    {
    }
  }
}