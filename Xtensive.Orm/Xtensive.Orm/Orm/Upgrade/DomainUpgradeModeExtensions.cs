// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.05

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Extension method for <see cref="DomainUpgradeMode"/>.
  /// </summary>
  public static class DomainUpgradeModeExtensions
  {
    /// <summary>
    /// Determines whether <paramref name="upgradeMode"/> requires <see cref="UpgradeStage.Upgrading"/> stage.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    public static bool RequiresUpgradingStage(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.Perform:
        case DomainUpgradeMode.PerformSafely:
          return true;
        default:
          return false;
      }      
    }


    /// <summary>
    /// Determines whether <paramref name="upgradeMode"/> requires <see cref="UpgradeStage.Initializing"/> stage.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    public static bool RequiresInitializingStage(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.Perform:
        case DomainUpgradeMode.PerformSafely:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Determines whether the specified upgrade mode is legacy.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    /// <returns>
    /// <see langword="true"/> if the specified upgrade mode is legacy;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsLegacy(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.LegacySkip:
      case DomainUpgradeMode.LegacyValidate:
        return true;
      default:
        return false;
      }
    }

    /// <summary>
    /// Determines whether the specified upgrade mode is legacy.
    /// </summary>
    /// <param name="upgradeMode">The upgrade mode.</param>
    /// <returns>
    /// <see langword="true"/> if the specified upgrade mode is legacy;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsUpgrading(this DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
      case DomainUpgradeMode.LegacySkip:
      case DomainUpgradeMode.LegacyValidate:
      case DomainUpgradeMode.Skip:
      case DomainUpgradeMode.Validate:
        return false;
      default:
        return true;
      }
    }
  }
}