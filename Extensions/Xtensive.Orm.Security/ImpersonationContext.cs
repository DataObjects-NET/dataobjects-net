using System;
using Xtensive.Core;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Defines an impersonation context that holds a set of all permission for current user and 
  /// provides secure queries.
  /// </summary>
  public class ImpersonationContext : SessionBound,
    IDisposable
  {
    private readonly IDisposable queryRootScope;
    private readonly ImpersonationContext outerContext;

    /// <summary>
    /// Gets or sets the current principal.
    /// </summary>
    /// <value>The principal.</value>
    public IPrincipal Principal { get; private set; }

    /// <summary>
    /// Gets or sets the permissions for the current principal.
    /// </summary>
    /// <value>The permissions.</value>
    public PermissionSet Permissions { get; private set; }

    /// <summary>
    /// Invalidates this instance. Permissions for the current principal will be recalculated.
    /// </summary>
    public void Invalidate()
    {
      Permissions = new PermissionSet(Principal.Roles);
    }

    /// <summary>
    /// Reverts the impersonation context to the outer one.
    /// </summary>
    public void Undo()
    {
      queryRootScope.Dispose();
      Session.UndoImpersonation(this, outerContext);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      Dispose(true);
    }

    /// <inheritdoc/>
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
    public ImpersonationContext(IPrincipal principal, ImpersonationContext outerContext)
      : base(principal.Session)
    {
      ArgumentNullException.ThrowIfNull(principal);

      this.outerContext = outerContext;
      Principal = principal;
      Permissions = new PermissionSet(principal.Roles);

      QueryEndpoint insecureQuery;
      var existingBuilder = Session.Query.RootBuilder as SecureQueryRootBuilder;
      if (existingBuilder != null)
        insecureQuery = existingBuilder.InsecureQuery;
      else
        insecureQuery = Session.Query;
      
      queryRootScope = Session.OverrideQueryRoot(
        new SecureQueryRootBuilder(Session, insecureQuery));
    }
  }
}
