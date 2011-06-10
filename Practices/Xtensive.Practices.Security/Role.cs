using System.Collections.Generic;
using Xtensive.Collections;

namespace Xtensive.Practices.Security
{
  public abstract class Role
  {
    private readonly List<Permission> permissions;
    private readonly ReadOnlyList<Permission> readOnlyPermissions;

    public string Name { get; protected set; }

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

    public bool Equals(Role other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(other.Name, Name);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof (Role)) return false;
      return Equals((Role) obj);
    }

    public override int GetHashCode()
    {
      return (Name != null ? Name.GetHashCode() : 0);
    }

    protected Role()
    {
      permissions = new List<Permission>();
      readOnlyPermissions = new ReadOnlyList<Permission>(permissions);
      Name = GetType().Name;
    }
  }
}
