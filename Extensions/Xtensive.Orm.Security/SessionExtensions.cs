// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System.Security.Principal;
using Xtensive.Core;
using Xtensive.Orm.Security;
using Xtensive.Orm.Security.Configuration;
using IPrincipal = Xtensive.Orm.Security.IPrincipal;

namespace Xtensive.Orm
{
  /// <summary>
  /// Session extension methods for security-related stuff.
  /// </summary>
  public static class SessionExtensions
  {
    /// <summary>
    /// Gets the security configuration.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns><see cref="SecurityConfiguration"/> instance.</returns>
    public static SecurityConfiguration GetSecurityConfiguration(this Session session)
    {
      var result = session.Domain.Extensions.Get<SecurityConfiguration>();
      if (result == null) {
        result = SecurityConfiguration.Load();
        session.Domain.Extensions.Set(result);
      }
      return result;
    }

    /// <summary>
    /// Gets the current impersonation context.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns><see cref="ImpersonationContext"/> instance.</returns>
    public static ImpersonationContext GetImpersonationContext(this Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");

      return session.Extensions.Get<ImpersonationContext>();
    }

    /// <summary>
    /// Impersonates the specified session.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="principal">The principal.</param>
    /// <returns><see cref="ImpersonationContext"/> instance.</returns>
    public static ImpersonationContext Impersonate(this Session session, IPrincipal principal)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(principal, "principal");

      var currentContext = session.GetImpersonationContext();

      var context = new ImpersonationContext(principal, currentContext);
      session.Extensions.Set(context);

      return context;
    }

    /// <summary>
    /// Authenticates a user's credentials. Returns <see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null"/> if they are not.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="identity">The identity.</param>
    /// <param name="args">The arguments to validate, e.g. password.</param>
    /// <returns>
    /// 	<see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null"/> if they are not.
    /// </returns>
    public static IPrincipal Authenticate(this Session session, IIdentity identity, params object[] args)
    {
      return Authenticate(session, identity.Name, args);
    }

    /// <summary>
    /// Authenticates a user's credentials. Returns <see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null"/> if they are not.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="name">The user name.</param>
    /// <param name="args">The arguments to validate, e.g. password.</param>
    /// <returns>
    /// 	<see cref="IPrincipal"/> instance if the credentials are valid and <see langword="null"/> if they are not.
    /// </returns>
    public static IPrincipal Authenticate(this Session session, string name, params object[] args)
    {
      var config = GetSecurityConfiguration(session);
      var service = session.Services.Get<IAuthenticationService>(config.AuthenticationServiceName);
      return service.Authenticate(name, args);
    }

    private static void ClearImpersonationContext(this Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");

      session.Extensions.Set<ImpersonationContext>(null);
    }

    private static void SetImpersonationContext(this Session session, ImpersonationContext context)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(context, "context");

      session.Extensions.Set(context);
    }

    internal static void UndoImpersonation(this Session session, ImpersonationContext innerContext, ImpersonationContext outerContext)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(innerContext, "innerContext");
      // outerContext can be null

      var currentContext = session.GetImpersonationContext();
      if (currentContext != innerContext)
        return;

      session.ClearImpersonationContext();
      if (outerContext != null)
        session.SetImpersonationContext(outerContext);
    }
  }
}