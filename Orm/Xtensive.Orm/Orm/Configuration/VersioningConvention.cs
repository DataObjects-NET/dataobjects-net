// Copyright (C) 2018-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2018.03.03

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// A set of rules for versioning.
  /// </summary>
  public sealed class VersioningConvention : LockableBase, ICloneable
  {
    public const EntityVersioningPolicy DefaultVersioningPolicy = EntityVersioningPolicy.Default;

    private EntityVersioningPolicy entityVersioningPolicy;
    private bool denyEntitySetOwnerVersionChange;

    /// <summary>
    /// Gets or sets versioning policy for entities.
    /// Default value is <see cref="Configuration.EntityVersioningPolicy.Default"/>
    /// </summary>
    public EntityVersioningPolicy EntityVersioningPolicy
    {
      get { return entityVersioningPolicy; }
      set {
        EnsureNotLocked();
        entityVersioningPolicy = value;
      }
    }

    /// <summary>
    /// Gets or sets value indicating that change of an <see cref="EntitySet{TItem}"/> owner version should be denied where possible.
    /// </summary>
    public bool DenyEntitySetOwnerVersionChange
    {
      get { return denyEntitySetOwnerVersionChange; }
      set {
        EnsureNotLocked();
        denyEntitySetOwnerVersionChange = value;
      }
    }

    /// <inheritdoc/>
    object ICloneable.Clone() => Clone();

    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    public VersioningConvention Clone()
    {
      var result = new VersioningConvention {
        entityVersioningPolicy = entityVersioningPolicy,
        denyEntitySetOwnerVersionChange = denyEntitySetOwnerVersionChange
      };
      return result;
    }

    /// <inheritdoc/>
    public VersioningConvention()
    {
      denyEntitySetOwnerVersionChange = false;
      entityVersioningPolicy = DefaultVersioningPolicy;
    }
  }
}

