// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.26

using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  /// <summary>
  /// Defines the basic functionality of a principal object.
  /// </summary>
  public interface IPrincipal : IEntity, System.Security.Principal.IPrincipal
  {
    /// <summary>
    /// Gets the name of the current principal.
    /// </summary>
    /// <value>The name.</value>
    [Field]
    string Name { get; }

    /// <summary>
    /// Gets the roles of the current principal.
    /// </summary>
    /// <value>The roles.</value>
    [Field]
    EntitySet<IRole> Roles { get; }
  }
}