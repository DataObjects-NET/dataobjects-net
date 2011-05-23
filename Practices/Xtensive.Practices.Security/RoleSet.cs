using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public class RoleSet : IEnumerable<Role>
  {
    private readonly PrincipalRoleSet principalRoles;
    private List<Role> roles;
    private readonly IEnumerable<Role> allRoles;

    public int Count
    {
      get { return roles.Count; }
    }

    internal void Invalidate()
    {
      var roleNames = principalRoles.Select(r => r.Name).ToList();
      roles = allRoles.Where(r => r.Name.In(roleNames)).ToList();
    }

    public bool Contains(Role item)
    {
      return roles.Contains(item);
    }

    #region IEnumerable<Role> Members

    public IEnumerator<Role> GetEnumerator()
    {
      foreach (var role in roles)
        yield return role;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    public RoleSet(IEnumerable<Role> allRoles, PrincipalRoleSet principalRoles)
    {
      this.allRoles = allRoles;
      this.principalRoles = principalRoles;
      Invalidate();
    }
  }
}