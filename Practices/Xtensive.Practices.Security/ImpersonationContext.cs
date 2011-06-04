using System;
using System.Collections.Generic;
using System.Linq;
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

      foreach (var item in Permissions) {
        var permission = item as Permission<T>;
        if (permission != null && permission.Query != null)
          candidates.Add(permission.Query(this, InsecureQuery));
      }
      if (candidates.Count == 0)
        return InsecureQuery.All<T>();

      var result = candidates[0];
      if (candidates.Count == 1)
        return result;

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
