using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// A set of permissions.
  /// </summary>
  public class PermissionSet : IEnumerable<Permission>
  {
    private readonly HashSet<Permission> permissions = new HashSet<Permission>();

    /// <summary>
    /// Gets the count.
    /// </summary>
    public int Count
    {
      get { return permissions.Count; }
    }

    /// <summary>
    /// Gets a permission specified by <typeparamref name="TPermission"/>.
    /// </summary>
    /// <typeparam name="TPermission">The type of the permission.</typeparam>
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

    /// <summary>
    /// Determines whether this instance contains a permission of the specified <typeparamref name="TPermission"/> with the specified predicate.
    /// </summary>
    /// <typeparam name="TPermission">The type of the permission.</typeparam>
    /// <param name="predicate">The predicate.</param>
    /// <returns>
    /// 	<see langword="True" /> if this instance contains a permission of the specified <typeparamref name="TPermission"/> with the specified predicate; otherwise, <see langword="False" />.
    /// </returns>
    public bool Contains<TPermission>(Func<TPermission, bool> predicate)
      where TPermission : Permission
    {
      var permission = Get<TPermission>();
      return permission != null && predicate(permission);
    }

    /// <inheritdoc/>
    public IEnumerator<Permission> GetEnumerator()
    {
      return ((IEnumerable<Permission>) permissions).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionSet"/> class.
    /// </summary>
    /// <param name="roles">The roles.</param>
    public PermissionSet(IEnumerable<IRole> roles)
    {
      foreach (var role in roles)
        permissions.UnionWith(role.Permissions);
    }
  }
}
