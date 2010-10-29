// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.01

namespace Xtensive.Orm
{
  /// <summary>
  /// Contract for entities having <see cref="NullEntity"/> property.
  /// Any references to such entities are replaced to  <see cref="NullEntity"/> property value
  /// during reference cleanup process.
  /// </summary>
  public interface IHasNullEntity
  {
    /// <summary>
    /// Gets the special "null entity".
    /// </summary>
    IEntity NullEntity { get; }
  }
}