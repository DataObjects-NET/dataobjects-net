// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.04.30

using System;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Upgrade stages enumeration.
  /// </summary>
  public enum UpgradeStage
  {
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