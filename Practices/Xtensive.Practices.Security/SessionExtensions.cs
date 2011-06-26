// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;
using System.Security.Principal;
using Xtensive.Core;
using Xtensive.Practices.Security;
using Xtensive.Practices.Security.Configuration;
using IPrincipal = Xtensive.Practices.Security.IPrincipal;

namespace Xtensive.Orm
{
  public static class SessionExtensions
  {
    public static SecurityConfiguration GetSecurityConfiguration(this Session session)
    {
      var result = session.Domain.Extensions.Get<SecurityConfiguration>();
      if (result == null) {
        result = SecurityConfiguration.Load();
        session.Domain.Extensions.Set(result);
      }
      return result;
    }

    public static ImpersonationContext GetImpersonationContext(this Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");

      return session.Extensions.Get<ImpersonationContext>();
    }

    public static ImpersonationContext Impersonate(this Session session, IPrincipal principal)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      ArgumentValidator.EnsureArgumentNotNull(principal, "principal");

      var currentContext = session.GetImpersonationContext();

      var context = new ImpersonationContext(principal, currentContext);
      session.Extensions.Set(context);

      return context;
    }

    public static IPrincipal Authenticate(this Session session, IIdentity identity, params object[] args)
    {
      return Authenticate(session, identity.Name, args);
    }

    public static IPrincipal Authenticate(this Session session, string name, params object[] args)
    {
      var config = GetSecurityConfiguration(session);
      var service = session.Services.Get<IAuthenticationService>(config.ValidationServiceName);
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