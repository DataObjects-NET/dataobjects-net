// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Schema upgrade modes.
  /// </summary>
  public enum SchemaUpgradeMode
  {
    /// <summary>
    /// Validate schema to be equal to the domain model.
    /// </summary>
    ValidateExact,

    /// <summary>
    /// Validate schema to be compatible (equal or greater) with the domain model.
    /// </summary>
    ValidateCompatible,

    /// <summary>
    /// Validate schema to be compatible with the domain model.
    /// </summary>
    ValidateLegacy,

    /// <summary>
    /// Upgrade schema to domain model.
    /// </summary>
    Perform,

    /// <summary>
    /// Upgrade schema to domain model safely - 
    /// i.e. without any operations leading to data lost.
    /// </summary>
    PerformSafely,

    /// <summary>
    /// Completely recreate the schema.
    /// </summary>
    Recreate,

    /// <summary>
    /// Skip schema upgrade.
    /// </summary>
    Skip,
  }
}