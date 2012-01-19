// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.06

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// Enumerates possible <see cref="BuildingContext.Stage"/> values.
  /// </summary>
  public enum BuildingStage
  {
    /// <summary>
    /// Initial state.
    /// Value is <see langword="0x0"/>. 
    /// </summary>
    Created = 0,
    /// <summary>
    /// <see cref="Domain"/> is building (<see cref="Domain.Build(DomainConfiguration)"/> method is executing).
    /// Value is <see langword="0x1"/>. 
    /// </summary>
    Building = 1,
    /// <summary>
    /// <see cref="Domain"/> is upgrading the schema.
    /// <see cref="Domain.Build(DomainConfiguration)"/> method is still executing.
    /// Value is <see langword="0x4"/>. 
    /// </summary>
    Upgrading = 4,
    /// <summary>
    /// <see cref="Domain"/> is ready to use.
    /// Value is <see langword="0x10"/>. 
    /// </summary>
    Ready = 0x10,
    /// <summary>
    /// At least one error has occured during <see cref="Domain"/> building.
    /// Value is <see langword="0x100"/>. 
    /// </summary>
    Failed = 0x100,
  }
}