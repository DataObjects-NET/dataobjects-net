// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using Xtensive.IoC;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// Provides <see cref="SecureQueryEndpoint"/> instance.
  /// </summary>
  [Service(typeof(IQueryEndpointProvider), Singleton = true)]
  public class QueryEndpointProvider : IQueryEndpointProvider
  {
    /// <summary>
    /// Gets the query endpoint.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns><see cref="SecureQueryEndpoint"/> instance.</returns>
    public QueryEndpoint GetQueryEndpoint(Session session)
    {
      return new SecureQueryEndpoint(session, new QueryEndpoint(session));
    }
  }
}