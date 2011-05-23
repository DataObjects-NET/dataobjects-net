// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public class SecureQueryEndpoint : QueryEndpoint
  {
    internal QueryEndpoint InsecureQuery { get; private set; }

    public override System.Linq.IQueryable<T> All<T>()
    {
      var context = Session.GetImpersonationContext();
      if (context == null)
        return InsecureQuery.All<T>();

      return context.GetSecureQuery<T>();
    }

    public SecureQueryEndpoint(Session session, QueryEndpoint origin) : base(session)
    {
      this.InsecureQuery = origin;
    }
  }
}