// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.02

using System;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Provides access to low-level operations with <see cref="Persistent"/> descendants.
  /// </summary>
  [Service(typeof(DirectEntityAccessor))]
  public sealed class DirectEntityAccessor: DirectPersistentAccessor
  {
    /// <summary>
    /// Invoked to update <paramref name="targetEntity"/>'s <see cref="Entity.VersionInfo"/>.
    /// </summary>
    /// <param name="targetEntity">The changed entity.</param>
    /// <param name="changedField">The changed field.</param>
    /// <returns>
    /// <see langword="True"/>, if <see cref="VersionInfo"/> was changed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Version root can't implement
    /// <see cref="IHasVersionRoots"/>.</exception>
    public bool UpdateVersionInfo(Entity targetEntity, FieldInfo changedField)
    {
      return targetEntity.UpdateVersionInfo(targetEntity, changedField);
    }

    /// <summary>
    /// Gets the internal state of the entity.
    /// </summary>
    /// <param name="target">The target entity.</param>
    public EntityState GetEntityState(Entity target)
    {
      ArgumentNullException.ThrowIfNull(target);

      return target.State;
    }

    // Constructors

    /// <inheritdoc/>
   [ServiceConstructor]
   public DirectEntityAccessor(Session session)
      : base(session)
    {
    }
  }
}