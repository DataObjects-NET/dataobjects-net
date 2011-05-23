using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Practices.Security
{
  public class PermissionSet : IEnumerable<Permission>
  {
    private readonly HashSet<Permission> permissions = new HashSet<Permission>();

    public TPermission Get<TPermission>()
      where TPermission : Permission
    {
      foreach (var item in this) {
        var permission = item as TPermission;
        if (permission != null)
          return permission;
      }
      return null;
    }

    public bool Contains<TPermission>(Func<TPermission, bool> predicate)
      where TPermission : Permission
    {
      var permission = Get<TPermission>();
      return permission != null && predicate(permission);
    }

    public IEnumerator<Permission> GetEnumerator()
    {
      foreach (var item in permissions)
        yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public PermissionSet(IEnumerable<Role> roles)
    {
      // TODO: Refactor to produce effective set of permissions
      foreach (var role in roles)
        permissions.UnionWith(role.Permissions);
    }
  }
}
