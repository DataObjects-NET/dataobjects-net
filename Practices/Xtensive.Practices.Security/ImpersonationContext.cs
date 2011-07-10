using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// Defines an impersonation context that holds a set of all permission for current user and 
  /// provides secure queries.
  /// </summary>
  public class ImpersonationContext : SessionBound,
    IDisposable
  {
    private readonly QueryEndpoint insecureQuery;
    private readonly ImpersonationContext outerContext;

    /// <summary>
    /// Gets or sets the current principal.
    /// </summary>
    /// <value>The principal.</value>
    [Infrastructure]
    public IPrincipal Principal { get; private set; }

    /// <summary>
    /// Gets or sets the permissions for the current principal.
    /// </summary>
    /// <value>The permissions.</value>
    [Infrastructure]
    public PermissionSet Permissions { get; private set; }

    /// <summary>
    /// Invalidates this instance. Permissions for the current principal will be recalculated.
    /// </summary>
    [Infrastructure]
    public void Invalidate()
    {
      Permissions = new PermissionSet(Principal.Roles);
    }

    /// <summary>
    /// Gets the secure query, i.e. the query that has permission restrictions applied.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns><see cref="IQueryable{T}"/> with restrictions applied.</returns>
    [Infrastructure]
    public IQueryable<T> GetSecureQuery<T>() where T: class, IEntity
    {
      var candidates = new List<IQueryable<T>>();
      var queryType = typeof(T);

      foreach (var permission in Permissions) {
        var permissionType = permission.Type;

        // Query<Animal> & Permission<Animal>
        if (queryType == permissionType) {
          // Permission doesn't have restrictive query. Investigation of other permissions doesn't make sense
          if (permission.Query == null)
            return insecureQuery.All<T>();

          candidates.Add((IQueryable<T>) permission.Query(this, insecureQuery));
        }
        // Query<Animal> && Permission<Dog>
        else if (queryType.IsAssignableFrom(permissionType)) {
          // Permission doesn't have restrictive query. Adding Query<Dog> to candidates
          if (permission.Query == null) {
            candidates.Add((IQueryable<T>) insecureQuery.All(permissionType));
            continue;
          }
          var p = Expression.Parameter(queryType, "p");
          var where = (Expression<Func<T, bool>>) Expression.Lambda(Expression.Not(Expression.TypeIs(p, permissionType)), p);
          candidates.Add(insecureQuery.All<T>().Where(where).Concat(permission.Query(this, insecureQuery).OfType<T>()));
        }
        // Query<Dog> && Permission<Animal>
        else if (permissionType.IsAssignableFrom(queryType)) {
          // Permission doesn't have restrictive query. Investigation of other permissions doesn't make sense
          if (permission.Query == null)
            return insecureQuery.All<T>();

          candidates.Add(permission.Query(this, insecureQuery).OfType<T>());
        }
      }
      if (candidates.Count == 0)
        return insecureQuery.All<T>();

      if (candidates.Count == 1)
        return candidates[0];

      var result = candidates[0];
      for (int i = 1; i < candidates.Count; i++)
        result = result.Union(candidates[i]);

      return result;
    }

    /// <summary>
    /// Reverts the impersonation context to the outer one.
    /// </summary>
    [Infrastructure]
    public void Undo()
    {
      Session.UndoImpersonation(this, outerContext);
    }

    /// <inheritdoc/>
    [Infrastructure]
    public void Dispose()
    {
      Dispose(true);
    }

    /// <inheritdoc/>
    [Infrastructure]
    protected virtual void Dispose(bool disposing) 
    {
      if (disposing)
        Undo();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImpersonationContext"/> class.
    /// </summary>
    /// <param name="principal">The principal.</param>
    /// <param name="outerContext">The outer context.</param>
    [Infrastructure]
    public ImpersonationContext(IPrincipal principal, ImpersonationContext outerContext)
      : base(principal.Session)
    {
      ArgumentValidator.EnsureArgumentNotNull(principal, "principal");

      this.outerContext = outerContext;
      insecureQuery = Session.Query;
      var sqe = insecureQuery as SecureQueryEndpoint;
      if (sqe != null)
        insecureQuery = sqe.InsecureQuery;
      
      Principal = principal;
      Permissions = new PermissionSet(principal.Roles);
    }
  }
}
