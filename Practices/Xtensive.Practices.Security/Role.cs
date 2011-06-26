using System.Collections.Generic;
using Xtensive.Aspects;
using Xtensive.Collections;
using Xtensive.Orm;
using Xtensive.Orm.Validation;

namespace Xtensive.Practices.Security
{
  [Index("Name", Unique = true)]
  public abstract class Role : Entity, IRole
  {
    private List<Permission> permissions;
    private ReadOnlyList<Permission> readOnlyPermissions;

    [NotNullConstraint(Mode = ConstrainMode.OnSetValue)]
    [Field(Length = 128)]
    public string Name { get; protected set; }

    [Field]
    public EntitySet<IPrincipal> Principals { get; private set; }

    [Infrastructure]
    public IList<Permission> Permissions
    {
      get { return readOnlyPermissions; }
    }

    [Infrastructure]
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

    protected override void OnInitialize()
    {
      base.OnInitialize();
      permissions = new List<Permission>();
      readOnlyPermissions = new ReadOnlyList<Permission>(permissions);
      RegisterPermissions();
    }

    [Infrastructure]
    protected abstract void RegisterPermissions();

    protected Role(Session session)
      : base(session)
    {
      Name = GetType().Name;
    }
  }
}
