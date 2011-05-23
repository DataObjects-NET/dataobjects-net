using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;

namespace Xtensive.Practices.Security
{
  public abstract class Role
  {
    private readonly List<Permission> permissions = new List<Permission>();
    private readonly ReadOnlyList<Permission> readOnlyPermissions;

    public string Name
    {
      get { return GetType().Name; }
    }

    public IList<Permission> Permissions
    {
      get { return readOnlyPermissions; }
    }

    protected void RegisterPermission(Permission permission)
    {
      if (permissions.Count == 0) {
        permissions.Add(permission);
        return;
      }

      // Rewriting permission if any
      for (int i = 0; i < permissions.Count; i++) {
        var item = permissions[i];
        if (item.Type != permission.Type)
          continue;

        permissions[i] = permission;
        return;
      }

      permissions.Add(permission);
    }

    protected Role()
    {
      readOnlyPermissions = new ReadOnlyList<Permission>(permissions);
    }
  }
}
