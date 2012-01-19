// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.30

using Xtensive.Orm.Building;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Upgrade stages enumeration.
  /// </summary>
  public enum UpgradeStage
  {
    /// <summary>
    /// The very fist upgrade stage.
    /// Only system types are visible;
    /// schema isn't upgraded, but only checked for compatibility with
    /// the model containing system types only (<see cref="SchemaUpgradeMode.ValidateCompatible"/>).
    /// </summary>
    Initializing = 0x0,
    /// <summary>
    /// The second upgrade stage.
    /// All the types are visible, including upgrade-only types;
    /// schema is upgraded; 
    /// <see cref="IUpgradeHandler.OnStage"/> events are raised at the beginning of this stage;
    /// <see cref="UpgradeHandler.OnUpgrade"/> events are raised at the end of this stage.
    /// </summary>
    Upgrading = 0x1,
    /// <summary>
    /// The final upgrade stage.
    /// Only runtime types are visible; upgrade-only types are invisible;
    /// schema is upgraded once more (upgrade-only types are removed); 
    /// </summary>
    Final = 0x2,
  }
}