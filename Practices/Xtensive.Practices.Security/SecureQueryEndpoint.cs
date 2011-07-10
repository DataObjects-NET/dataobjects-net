// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// A <see cref="QueryEndpoint"/> with support of secure queries.
  /// </summary>
  public class SecureQueryEndpoint : QueryEndpoint
  {
    internal QueryEndpoint InsecureQuery { get; private set; }

    /// <inheritdoc/>
    public override System.Linq.IQueryable<T> All<T>()
    {
      var context = Session.GetImpersonationContext();
      if (context == null)
        return InsecureQuery.All<T>();

      return context.GetSecureQuery<T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureQueryEndpoint"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="origin">The original <see cref="QueryEndpoint"/> instance.</param>
    public SecureQueryEndpoint(Session session, QueryEndpoint origin) : base(session)
    {
      this.InsecureQuery = origin;
    }
  }
}