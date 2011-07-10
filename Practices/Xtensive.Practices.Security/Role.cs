using System.Collections.Generic;
using Xtensive.Aspects;
using Xtensive.Collections;
using Xtensive.Orm;
using Xtensive.Orm.Validation;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// Base implementation of <see cref="IRole"/> interface.
  /// </summary>
  [Index("Name", Unique = true)]
  public abstract class Role : Entity, IRole
  {
    private List<Permission> permissions;
    private ReadOnlyList<Permission> readOnlyPermissions;

    /// <inheritdoc/>
    [NotNullConstraint(Mode = ConstrainMode.OnSetValue)]
    [Field(Length = 128)]
    public string Name { get; protected set; }

    /// <inheritdoc/>
    [Field]
    public EntitySet<IPrincipal> Principals { get; private set; }

    /// <inheritdoc/>
    [Infrastructure]
    public IList<Permission> Permissions
    {
      get { return readOnlyPermissions; }
    }

    /// <summary>
    /// Registers the permission.
    /// </summary>
    /// <param name="permission">The permission.</param>
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

    /// <inheritdoc/>
    protected override void OnInitialize()
    {
      base.OnInitialize();
      permissions = new List<Permission>();
      readOnlyPermissions = new ReadOnlyList<Permission>(permissions);
      RegisterPermissions();
    }

    /// <summary>
    /// Registers the permissions.
    /// </summary>
    [Infrastructure]
    protected abstract void RegisterPermissions();

    /// <summary>
    /// Initializes a new instance of the <see cref="Role"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected Role(Session session)
      : base(session)
    {
      Name = GetType().Name;
    }
  }
}
