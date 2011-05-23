using System;
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
      foreach (var item in Permissions) {
        var permission = item as Permission<T>;
        if (permission != null && permission.Query != null)
          return permission.Query(this, InsecureQuery);
      }
      return InsecureQuery.All<T>();
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
      var roleProvider = Session.Services.Get<IRoleProvider>();
      Roles = new RoleSet(roleProvider.GetAllRoles(), principal.PrincipalRoles);
      Permissions = new PermissionSet(Roles);
    }
  }
}
