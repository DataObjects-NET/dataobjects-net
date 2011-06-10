using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public class ImpersonationContext : SessionBound,
    IDisposable
  {
    [Infrastructure]
    private QueryEndpoint InsecureQuery { get; set; }

    [Infrastructure]
    private ImpersonationContext OuterContext { get; set; }

    [Infrastructure]
    public IPrincipal Principal { get; private set; }

    [Infrastructure]
    public RoleSet Roles { get; private set; }

    [Infrastructure]
    public PermissionSet Permissions { get; private set; }

    [Infrastructure]
    public void Invalidate()
    {
      Roles.Invalidate();
    }

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
            return InsecureQuery.All<T>();

          candidates.Add((IQueryable<T>) permission.Query(this, InsecureQuery));
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
          candidates.Add(InsecureQuery.All<T>().Where(where).Concat(permission.Query(this, InsecureQuery).OfType<T>()));
        }
        // Query<Dog> && Permission<Animal>
        else if (permissionType.IsAssignableFrom(queryType)) {
          // Permission doesn't have restrictive query. Investigation of other permissions doesn't make sense
          if (permission.Query == null)
            return InsecureQuery.All<T>();

          candidates.Add(permission.Query(this, InsecureQuery).OfType<T>());
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

    [Infrastructure]
    public void Undo()
    {
      Session.UndoImpersonation(this, OuterContext);
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
      Undo();
    }

    [Infrastructure]
    public ImpersonationContext(IPrincipal principal, ImpersonationContext outerContext)
      : base(principal.Session)
    {
      ArgumentValidator.EnsureArgumentNotNull(principal, "principal");

      OuterContext = outerContext;
      InsecureQuery = Session.Query;
      var sqe = InsecureQuery as SecureQueryEndpoint;
      if (sqe != null)
        InsecureQuery = sqe.InsecureQuery;
      
      Principal = principal;

      var roleProviders = Session.Services.GetAll<IRoleProvider>();
      // Removing GenericRoleProvider if there are any explicit providers
      if (roleProviders.Count() > 1)
        roleProviders = roleProviders.Except(roleProviders.OfType<GenericRoleProvider>());

      var roles = new HashSet<Role>();
      foreach (var rp in roleProviders) 
        roles.UnionWith(rp.GetAllRoles());

      Roles = new RoleSet(roles, principal.PrincipalRoles);
      Permissions = new PermissionSet(Roles);
    }
  }
}
