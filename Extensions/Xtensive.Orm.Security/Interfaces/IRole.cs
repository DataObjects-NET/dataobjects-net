// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.26

using System.Collections.Generic;
using Xtensive.Orm;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Defines a role.
  /// </summary>
  public interface IRole : IEntity
  {
    /// <summary>
    /// Gets the name of the current role.
    /// </summary>
    /// <value>The name.</value>
    [Field]
    string Name { get; }

    /// <summary>
    /// Gets the principals associated with the current role.
    /// </summary>
    /// <value>The principals.</value>
    [Field]
    [Association(PairTo = "Roles", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Clear)]
    EntitySet<IPrincipal> Principals { get; }

    /// <summary>
    /// Gets the permissions associated with the current role.
    /// </summary>
    /// <value>The permissions.</value>
    IList<Permission> Permissions { get; }
  }
}