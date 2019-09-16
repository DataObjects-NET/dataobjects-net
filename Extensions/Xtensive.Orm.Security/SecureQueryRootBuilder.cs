// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.05.22

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Orm;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// A <see cref="QueryEndpoint"/> with support of secure queries.
  /// </summary>
  public class SecureQueryRootBuilder : QueryRootBuilder
  {
    private Session session;
    internal QueryEndpoint InsecureQuery { get; private set; }

    /// <inheritdoc/>
    protected override Expression  BuildRootExpression<TEntity>()
    {
      var context = session.GetImpersonationContext();
      if (context == null)
        return InsecureQuery.All<TEntity>().Expression;

      return GetSecureQuery<TEntity>(context).Expression;
    }

    private IQueryable<T> GetSecureQuery<T>(ImpersonationContext context) where T: class, IEntity
    {
      var candidates = new List<IQueryable<T>>();
      var queryType = typeof(T);

      foreach (var permission in context.Permissions) {
        var permissionType = permission.Type;

        // Query<Animal> & Permission<Animal>
        if (queryType == permissionType) {
          // Permission doesn't have restrictive query. Investigation of other permissions doesn't make sense
          if (permission.Query == null)
            return InsecureQuery.All<T>();

          candidates.Add((IQueryable<T>) permission.Query(context, InsecureQuery));
        }
        // Query<Animal> && Permission<Dog>
        else if (queryType.IsAssignableFrom(permissionType)) {
          // Permission doesn't have restrictive query. Adding Query<Dog> to candidates
          if (permission.Query == null) {
            candidates.Add((IQueryable<T>) InsecureQuery.All(permissionType));
            continue;
          }
          var p = Expression.Parameter(queryType, "p");
          var where = (Expression<Func<T, bool>>) Expression.Lambda(Expression.Not(Expression.TypeIs(p, permissionType)), p);
          candidates.Add(InsecureQuery.All<T>().Where(where).Concat(permission.Query(context, InsecureQuery).OfType<T>()));
        }
        // Query<Dog> && Permission<Animal>
        else if (permissionType.IsAssignableFrom(queryType)) {
          // Permission doesn't have restrictive query. Investigation of other permissions doesn't make sense
          if (permission.Query == null)
            return InsecureQuery.All<T>();

          candidates.Add(permission.Query(context, InsecureQuery).OfType<T>());
        }
      }
      if (candidates.Count == 0)
        return InsecureQuery.All<T>();

      if (candidates.Count == 1)
        return candidates[0];

      var result = candidates[0];
      for (int i = 1; i < candidates.Count; i++)
        result = result.Union(candidates[i]);

      return result;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SecureQueryRootBuilder"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="origin">The original <see cref="QueryEndpoint"/> instance.</param>
    public SecureQueryRootBuilder(Session session, QueryEndpoint origin)
    {
      this.session = session;
      InsecureQuery = origin;
    }
  }
}