// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Schema upgrade mode.
  /// </summary>
  public enum SchemaUpgradeMode
  {
    /// <summary>
    /// Upgrade storage schema, if nothing will be removed.
    /// </summary>
    SafeUpgrade,

    /// <summary>
    /// Validate schema model to be compatible (not equal) with domain model.
    /// </summary>
    Validate,

    /// <summary>
    /// Upgrade schema model to domain model exactly.
    /// </summary>
    Upgrade,

    /// <summary>
    /// Recreate schema.
    /// </summary>
    Recreate
  }
}