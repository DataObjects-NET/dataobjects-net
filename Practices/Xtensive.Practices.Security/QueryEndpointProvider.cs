// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  [Service(typeof(IQueryEndpointProvider), Singleton = true)]
  public class QueryEndpointProvider : IQueryEndpointProvider
  {
    public QueryEndpoint GetQueryEndpoint(Session session)
    {
      return new SecureQueryEndpoint(session, new QueryEndpoint(session));
    }
  }
}